using DreamOS.Core;
using DreamOS.WinForms.Controls;

namespace DreamOS.WinForms.Forms;

public sealed class PizzaForm : WorkflowForm
{
    private PizzaSize size=PizzaSize.Small; private Crust crust=Crust.Thin; private Drink drink=Drink.None;
    private readonly HashSet<Topping> toppings=[];
    private readonly List<ChoiceCard> sizes=[],crusts=[],extras=[],drinks=[];
    private readonly ReceiptPanel receipt=new();
    private readonly ComboBox dining=Theme.Choice(["Dine in","Take out"]);
    public PizzaForm() : base("Special Pizza Builder","Choose your pizza, add the little extras, then build your order.")
    {
        var grid=WorkflowUi.Grid(); grid.RowCount=3; grid.RowStyles.Clear();
        grid.ColumnStyles[0].Width=54; grid.ColumnStyles[1].Width=46;
        grid.RowStyles.Add(new(SizeType.Percent,39)); grid.RowStyles.Add(new(SizeType.Percent,61)); grid.RowStyles.Add(new(SizeType.Absolute,42));
        for(int i=0;i<3;i++) { int n=i; var card=new ChoiceCard(new[]{"Small","Medium","Large"}[i],new[]{"PHP 100","PHP 150","PHP 200"}[i],i,()=> { size=(PizzaSize)n; Changed(); }) { Size=new(93,88) }; sizes.Add(card); }
        for(int i=0;i<2;i++) { int n=i; crusts.Add(new(new[]{"Thin crust","Thick crust"}[i],i==0?"Included":"+50% size price",3+i,()=> { crust=(Crust)n; Changed(); }) { Size=new(110,88) }); }
        string[] names=["Extra cheese","Mushrooms","Black olives","Onions","Green peppers","Tomatoes"]; int[] art=[5,6,7,8,9,10];
        for(int i=0;i<6;i++) { int n=i; extras.Add(new(names[i],"+ PHP 10",art[i],()=> { if(!toppings.Add((Topping)n))toppings.Remove((Topping)n); Changed(); }) { Size=new(93,83) }); }
        string[] beverages=["No drink","Soft drink","Fruit juice","Coffee / chocolate"]; int[] sprites=[14,11,12,13];
        for(int i=0;i<4;i++) { int n=i; drinks.Add(new(beverages[i],new[]{"PHP 0","PHP 20","PHP 15","PHP 25"}[i],sprites[i],()=> { drink=(Drink)n; Changed(); }) { Size=new(110,83) }); }
        grid.Controls.Add(WorkflowUi.Section("01 · Pizza size",WorkflowUi.Cards(sizes.ToArray())),0,0);
        grid.Controls.Add(WorkflowUi.Section("02 · Crust",WorkflowUi.Cards(crusts.ToArray())),1,0);
        grid.Controls.Add(WorkflowUi.Section("03 · Toppings · select any",WorkflowUi.Cards(extras.ToArray())),0,1);
        grid.Controls.Add(WorkflowUi.Section("04 · Something to sip",WorkflowUi.Cards(drinks.ToArray())),1,1);
        var diningRow=new FlowLayoutPanel { Dock=DockStyle.Fill,WrapContents=false }; diningRow.Controls.Add(WorkflowUi.Text("Dining",9,true)); dining.Width=150; diningRow.Controls.Add(dining); dining.SelectedIndex=0; dining.SelectedIndexChanged+=(_,_)=>Changed(); grid.Controls.Add(diningRow,0,2); grid.SetColumnSpan(diningRow,2);
        Split(grid,receipt);
        Command("Build order",Compute); Command("Clear toppings",()=> { toppings.Clear(); Changed(); }); Command("Start over",Reset); PlaceActions(receipt); Changed();
    }
    private void Changed()
    {
        for(int i=0;i<sizes.Count;i++)sizes[i].SelectCard(i==(int)size);
        for(int i=0;i<crusts.Count;i++)crusts[i].SelectCard(i==(int)crust);
        for(int i=0;i<extras.Count;i++)extras[i].SelectCard(toppings.Contains((Topping)i));
        for(int i=0;i<drinks.Count;i++)drinks[i].SelectCard(i==(int)drink);
        receipt.Clear("Your pizza order will be itemized here.");
        var t=new PizzaService().Calculate(new(size,crust,toppings.ToArray(),drink,dining.SelectedIndex==1)); Status($"Current order · {Money.Format(t.Total)} · {toppings.Count} topping(s)");
    }
    private void Compute()
    {
        var t=new PizzaService().Calculate(new(size,crust,toppings.ToArray(),drink,dining.SelectedIndex==1));
        var rows=new List<ReceiptEntry> { new("Dining",dining.Text),new($"{size} · {crust} crust",Money.Format(t.Pizza)) };
        foreach(var extra in toppings)rows.Add(new(System.Text.RegularExpressions.Regex.Replace(extra.ToString(),"([a-z])([A-Z])","$1 $2"),Money.Format(10)));
        rows.Add(new(drink==Drink.None?"No drink":System.Text.RegularExpressions.Regex.Replace(drink.ToString(),"([a-z])([A-Z])","$1 $2"),Money.Format(t.Drinks)));
        receipt.ShowDocument(new("PIZZA ORDER",ReceiptData.NewReference("PIZ"),DateTime.Now,rows,"ORDER TOTAL",Money.Format(t.Total),"Made with a little extra love. Classroom order only.")); Status("Order built. Save or print your itemized slip.");
    }
    private void Reset() { size=PizzaSize.Small; crust=Crust.Thin; drink=Drink.None; toppings.Clear(); dining.SelectedIndex=0; Changed(); }
}

public sealed class LottoForm : WorkflowForm
{
    private readonly NumericUpDown bet=Theme.Number(10,1_000_000,10,2);
    private readonly NumericUpDown[] digits=[Theme.Number(0,9),Theme.Number(0,9,1),Theme.Number(0,9,2)];
    private readonly ReceiptPanel receipt=new(); private readonly NumberBalls winners=new();
    public LottoForm() : base("3 Digits Lotto","A fictional classroom draw. Pick three different digits; no real money or prizes.")
    {
        var main=WorkflowUi.Stack(); main.Padding=new(18);
        main.Controls.Add(WorkflowUi.Text("01 · Choose your three digits",13,true));
        var ticket=new FlowLayoutPanel { AutoSize=true,WrapContents=false,Margin=new(0,8,0,18) };
        int digitIndex=0; foreach(var d in digits) { d.AccessibleName=$"Ticket digit {++digitIndex}"; d.Font=new("Consolas",25,FontStyle.Bold); d.Width=96; d.TextAlign=HorizontalAlignment.Center; d.ValueChanged+=(_,_)=> { receipt.Clear(); winners.Clear(); }; ticket.Controls.Add(d); } main.Controls.Add(ticket);
        main.Controls.Add(WorkflowUi.Text("02 · Your demo bet",11,true)); main.Controls.Add(bet);
        var chips=new FlowLayoutPanel { AutoSize=true,WrapContents=false }; foreach(int value in new[]{10,20,50,100})chips.Controls.Add(Theme.Button($"PHP {value}",()=>bet.Value=value)); main.Controls.Add(chips);
        main.Controls.Add(WorkflowUi.Text("03 · Winning numbers",12,true)); winners.Size=new(410,85); main.Controls.Add(winners);
        var rules=WorkflowUi.Text("Exact order: fictional PHP 1,000,000 jackpot.\nSame digits, different order: demo bet × 1,000.\nEvery draw uses three different digits.",9); main.Controls.Add(rules);
        bet.ValueChanged+=(_,_)=>receipt.Clear(); Split(WorkflowUi.Section("Your demo ticket",main),receipt);
        Command("Place bet + draw",Draw); Command("New ticket",()=> { if(drawing)return; for(int i=0;i<3;i++)digits[i].Value=i; bet.Value=10; winners.Clear(); receipt.Clear(); }); PlaceActions(main);
    }
    private bool drawing;
    private void Draw()
    {
        if(drawing)return;
        var service=new LottoService(); var ticket=digits.Select(d=>(int)d.Value).ToArray(); service.ValidateTicket(bet.Value,ticket); var draw=service.Draw(); var wager=bet.Value; var result=service.Evaluate(wager,ticket,draw);
        _=AnimateDraw(ticket,wager,draw,result);
    }
    private async Task AnimateDraw(int[] ticket,decimal wager,int[] draw,LottoResult result)
    {
        drawing=true; Workspace.Enabled=false; Status("Drawing your numbers… watch the balls roll!");
        try { await winners.Roll(draw); } finally { drawing=false; if(!IsDisposed)Workspace.Enabled=true; }
        if(IsDisposed)return;
        Run(()=>FinishDraw(ticket,wager,draw,result));
    }
    private void FinishDraw(int[] ticket,decimal wager,int[] draw,LottoResult result)
    {
        DreamDialog.Show(this,"Winning numbers: "+string.Join(" · ",draw)+"\nYour ticket: "+string.Join(" · ",ticket)+"\n"+result.Outcome,"Your lotto draw");
        receipt.ShowDocument(new("CLASSROOM LOTTO TICKET",ReceiptData.NewReference("LOT"),DateTime.Now,[new("Your digits",string.Join(" · ",ticket)),new("Demo bet",Money.Format(wager)),new("Winning digits",string.Join(" · ",draw)),new("Outcome",result.Outcome.ToString())],"FICTIONAL PRIZE",Money.Format(result.Prize),"Educational simulation. Not a real-money ticket."));
        Status(result.Outcome==LottoOutcome.NoMatch?"No match this time. You can try another demo draw.":$"{result.Outcome}! Your fictional prize is {Money.Format(result.Prize)}.");
    }
    private sealed class NumberBalls : Control
    {
        private int[]? numbers;
        private int frame,revealed=3;
        public async Task Roll(int[] values)
        {
            numbers=values; revealed=0;
            for(frame=0;frame<54;frame++) { if(IsDisposed)return; revealed=Math.Clamp((frame-15)/12,0,3); Invalidate(); await Task.Delay(45); }
            if(IsDisposed)return; revealed=3; frame=0; ShowNumbers(values);
        }
        public NumberBalls() { DoubleBuffered=true; BackColor=Theme.Cream; }
        public void ShowNumbers(int[] values) { numbers=values; AccessibleName="Winning digits: "+string.Join(", ",values); Invalidate(); }
        public void Clear() { numbers=null; AccessibleName="Winning digits not drawn"; Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); using var font=new Font("Consolas",26,FontStyle.Bold);
            for(int i=0;i<3;i++) { var r=new Rectangle(8+i*100,8+(revealed<=i?(int)(Math.Sin(frame*.8+i)*7):0),72,66); using var fill=new SolidBrush(new[]{Color.FromArgb(244,199,219),Color.FromArgb(203,218,248),Color.FromArgb(218,233,218)}[i]); e.Graphics.FillEllipse(fill,r); if(revealed<=i) { using var ring=new Pen(Theme.Accent,2); e.Graphics.DrawArc(ring,Rectangle.Inflate(r,-5,-5),frame*25,180); } TextRenderer.DrawText(e.Graphics,numbers is null?"?":revealed<=i?((frame*7+i*3)%10).ToString():numbers[i].ToString(),font,r,Theme.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter); }
        }
    }
}

public sealed class TuitionForm : WorkflowForm
{
    private readonly ComboBox course=Theme.Choice<Course>(),year=Theme.Choice<YearLevel>();
    private readonly TextBox student=new() { Width=230,PlaceholderText="Student name (optional)",MaxLength=100 };
    private readonly NumericUpDown units=Theme.Number(3,30,21),labs=Theme.Number(0,3,0);
    private readonly ReceiptPanel receipt=new();
    public TuitionForm() : base("Tuition Assessment","Enrollment details on the left. A clear, itemized assessment on the right.")
    {
        var grid=WorkflowUi.Grid(); grid.RowCount=2; grid.RowStyles.Clear(); grid.RowStyles.Add(new(SizeType.Percent,61)); grid.RowStyles.Add(new(SizeType.Percent,39));
        var enrollment=WorkflowUi.Stack(); WorkflowUi.Field(enrollment,"Student",student); WorkflowUi.Field(enrollment,"Course",course); WorkflowUi.Field(enrollment,"Year level",year);
        var academic=WorkflowUi.Stack(); WorkflowUi.Field(academic,"Total academic units · 3–30",units); WorkflowUi.Field(academic,"Laboratory units · 0–3",labs);
        academic.Controls.Add(new InformationCard("YOUR ASSESSMENT RATES","PHP 100 per academic unit\nPHP 1,000 per laboratory unit"));
        grid.Controls.Add(WorkflowUi.Section("Enrollment",enrollment),0,0); grid.Controls.Add(WorkflowUi.Section("Study load",academic),1,0);
        var fees=new DataGridView { Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White,BorderStyle=BorderStyle.FixedSingle };
        fees.Columns.Add("year","Year level"); fees.Columns.Add("it","BSIT / BSCS"); fees.Columns.Add("other","BSA / BSN");
        foreach(var y in Enum.GetValues<YearLevel>())fees.Rows.Add(y,Money.Format(new TuitionService().Calculate(Course.BSIT,y,3,0).Miscellaneous),Money.Format(new TuitionService().Calculate(Course.BSA,y,3,0).Miscellaneous));
        grid.Controls.Add(WorkflowUi.Section("Fee schedule",fees),0,1); grid.SetColumnSpan(fees,2); Split(grid,receipt);
        course.SelectedIndexChanged+=(_,_)=>receipt.Clear(); year.SelectedIndexChanged+=(_,_)=>receipt.Clear(); units.ValueChanged+=(_,_)=>receipt.Clear(); labs.ValueChanged+=(_,_)=>receipt.Clear(); student.TextChanged+=(_,_)=>receipt.Clear();
        Command("Assess fees",Compute); Command("Clear",()=> { student.Clear(); course.SelectedIndex=year.SelectedIndex=-1; units.Value=3; labs.Value=0; receipt.Clear(); }); PlaceActions(academic);
    }
    private void Compute()
    {
        var t=new TuitionService().Calculate(Theme.Selected<Course>(course),Theme.Selected<YearLevel>(year),(int)units.Value,(int)labs.Value);
        receipt.ShowDocument(new("TUITION ASSESSMENT",ReceiptData.NewReference("EDU"),DateTime.Now,[new("Student",string.IsNullOrWhiteSpace(student.Text)?"Classroom student":student.Text.Trim()),new("Course / year",$"{course.Text} / {year.Text}"),new($"Tuition · {units.Value:0} units",Money.Format(t.Tuition)),new($"Laboratory · {labs.Value:0} units",Money.Format(t.Lab)),new("Miscellaneous",Money.Format(t.Miscellaneous))],"TOTAL ASSESSED",Money.Format(t.Total),"Assessment only. This slip does not acknowledge payment.")); Status("Assessment ready. The rates follow the supplied exercise table.");
    }
}

public sealed class PayrollForm : WorkflowForm
{
    private readonly SelectionCards<string> employee=new(["EMP001 — Alex Santos","EMP002 — Sam Reyes","EMP003 — Jamie Cruz"],["Alex Santos","Sam Reyes","Jamie Cruz"],["EMP001","EMP002","EMP003"],34);
    private readonly SelectionCards<Position> position=new(Enum.GetValues<Position>(),["Position A","Position B","Position C"],["PHP 500 / day","PHP 400 / day","PHP 300 / day"],32);
    private readonly SelectionCards<CivilStatus> status=new(Enum.GetValues<CivilStatus>(),["Single","Married","Widowed"],["10% tax","5% tax","5% tax"],34);
    private readonly NumericUpDown days=Theme.Number(1,31,20);
    private readonly DateTimePicker period=new() { Format=DateTimePickerFormat.Custom,CustomFormat="MMMM yyyy",ShowUpDown=true,Width=220 };
    private readonly ReceiptPanel receipt=new();
    public PayrollForm() : base("Employee Payroll","Create a readable payslip with gross earnings, deductions, and net pay.")
    {
        var grid=WorkflowUi.Grid(); var profile=WorkflowUi.Stack(); var work=WorkflowUi.Stack();
        WorkflowUi.Field(profile,"Employee · sample records",employee); WorkflowUi.Field(profile,"Position code",position); WorkflowUi.Field(profile,"Civil status",status);
        WorkflowUi.Field(work,"Payroll month",period); WorkflowUi.Field(work,"Days worked · 1–31",days);
        work.Controls.Add(new InformationCard("DAILY RATES","A · PHP 500  /  B · PHP 400  /  C · PHP 300\nChoose a position card to apply its rate."));
        work.Controls.Add(WorkflowUi.Text("Deductions follow the reference SSS brackets.\nSingle: 10% tax\nMarried / Widowed: 5% tax",9));
        grid.Controls.Add(WorkflowUi.Section("Employee profile",profile),0,0); grid.Controls.Add(WorkflowUi.Section("Pay period + attendance",work),1,0); Split(grid,receipt);
        employee.SelectedIndexChanged+=(_,_)=>receipt.Clear(); position.SelectedIndexChanged+=(_,_)=>receipt.Clear(); status.SelectedIndexChanged+=(_,_)=>receipt.Clear(); days.ValueChanged+=(_,_)=>receipt.Clear(); period.ValueChanged+=(_,_)=>receipt.Clear();
        Command("Generate payslip",Compute); Command("Clear",()=> { employee.SelectedIndex=position.SelectedIndex=status.SelectedIndex=-1; days.Value=20; period.Value=DateTime.Today; receipt.Clear(); }); PlaceActions(work);
    }
    private void Compute()
    {
        Require.That(employee.SelectedIndex>=0,"Select an employee."); var p=new PayrollService().Calculate(position.SelectedValue,status.SelectedValue,(int)days.Value);
        receipt.ShowDocument(new("EMPLOYEE PAYSLIP",ReceiptData.NewReference("PAY"),DateTime.Now,[new("Employee",employee.Text),new("Period",period.Value.ToString("MMMM yyyy")),new("Position / status",position.Text+" / "+status.Text),new("Days worked",days.Value.ToString("0")),new("Daily rate",Money.Format(p.Rate)),new("Gross earnings",Money.Format(p.Basic)),new("SSS deduction","− "+Money.Format(p.Sss)),new("Withholding tax","− "+Money.Format(p.Tax))],"NET TAKE-HOME PAY",Money.Format(p.Net),"Classroom payroll statement. Keep this copy for your records.")); Status($"Payslip ready · net pay {Money.Format(p.Net)}.");
    }
}

public sealed class HotelForm : WorkflowForm
{
    private readonly TextBox customer=new() { Width=230,MaxLength=100,PlaceholderText="Guest full name" },contact=new() { Width=230,MaxLength=80,PlaceholderText="Phone or email (optional)" };
    private readonly MonthCalendar checkIn=new() { MaxSelectionCount=1 },checkOut=new() { MaxSelectionCount=1 };
    private readonly SelectionCards<Capacity> capacity=new(Enum.GetValues<Capacity>(),["Single","Double","Family"],["1 guest · 1 bed","2 guests · 2 beds","Family room"],30);
    private readonly SelectionCards<RoomType> room=new(Enum.GetValues<RoomType>(),["Regular","DeLuxe","Suite"],["Essential stay","Extra comfort","Premium stay"],31);
    private readonly SelectionCards<HotelPayment> payment=new(Enum.GetValues<HotelPayment>(),["Cash","Check","Credit card"],["No surcharge","+5% surcharge","+10% surcharge"],33);
    private readonly Label stay=WorkflowUi.Text("Choose room details to see your stay estimate.",10,true);
    private readonly ReceiptPanel receipt=new();
    public HotelForm() : base("Dream Hotel · Front Desk","Guest details → stay dates → room and payment → itemized guest folio.")
    {
        var tabs=new TabControl { Dock=DockStyle.Fill };
        var guestPage=new TabPage("01 · Guest + room"); var datesPage=new TabPage("02 · Stay calendars"); var payPage=new TabPage("03 · Payment + estimate");
        var guestGrid=WorkflowUi.Grid(); var guest=WorkflowUi.Stack(); WorkflowUi.Field(guest,"Guest name",customer); WorkflowUi.Field(guest,"Contact",contact);
        guest.Controls.Add(new InformationCard("YOUR LITTLE HOTEL STAY","Complete the guest details, choose a room, then use the Stay calendars and Payment tabs."));
        var rooms=WorkflowUi.Stack(); WorkflowUi.Field(rooms,"Room capacity",capacity); WorkflowUi.Field(rooms,"Room category",room);
        guestGrid.Controls.Add(WorkflowUi.Section("Guest profile",guest),0,0); guestGrid.Controls.Add(WorkflowUi.Section("Room selection",rooms),1,0); guestPage.Controls.Add(guestGrid);
        var datesGrid=WorkflowUi.Grid(); var arrival=WorkflowUi.Stack(); var departure=WorkflowUi.Stack();
        arrival.Controls.Add(checkIn); arrival.Controls.Add(new InformationCard("CHECK-IN","Click your arrival date on the calendar."));
        departure.Controls.Add(checkOut); departure.Controls.Add(new InformationCard("CHECK-OUT","Click your departure date. Same-day stays count as one billable day."));
        datesGrid.Controls.Add(WorkflowUi.Section("Arrival calendar",arrival),0,0); datesGrid.Controls.Add(WorkflowUi.Section("Departure calendar",departure),1,0); datesPage.Controls.Add(datesGrid);
        var payGrid=WorkflowUi.Grid(); var methods=WorkflowUi.Stack(); WorkflowUi.Field(methods,"Payment method",payment);
        var overview=WorkflowUi.Stack(); overview.Controls.Add(stay); overview.Controls.Add(new InformationCard("BEFORE YOU CHECK IN","An estimate for your classroom guest folio. No room reservation or payment is confirmed."));
        payGrid.Controls.Add(WorkflowUi.Section("Payment options",methods),0,0); payGrid.Controls.Add(WorkflowUi.Section("Stay overview",overview),1,0); payPage.Controls.Add(payGrid);
        tabs.TabPages.AddRange([guestPage,datesPage,payPage]); Split(tabs,receipt);
        checkOut.SetDate(DateTime.Today.AddDays(1)); checkOut.MinDate=checkIn.SelectionStart;
        capacity.SelectedIndexChanged+=(_,_)=>Changed(); room.SelectedIndexChanged+=(_,_)=>Changed(); payment.SelectedIndexChanged+=(_,_)=>Changed(); checkIn.DateChanged+=(_,_)=> { checkOut.MinDate=checkIn.SelectionStart.Date; Changed(); }; checkOut.DateChanged+=(_,_)=>Changed(); customer.TextChanged+=(_,_)=>receipt.Clear(); contact.TextChanged+=(_,_)=>receipt.Clear();
        Command("Create guest folio",Compute); Command("New guest",()=> { customer.Clear(); contact.Clear(); capacity.SelectedIndex=room.SelectedIndex=payment.SelectedIndex=-1; checkIn.SetDate(DateTime.Today); checkOut.SetDate(DateTime.Today.AddDays(1)); Changed(); }); PlaceActions(overview);
    }
    private void Changed()
    {
        receipt.Clear(); stay.Text="Choose capacity, category and payment.";
        if(capacity.SelectedIndex<0||room.SelectedIndex<0||payment.SelectedIndex<0)return;
        try { var t=new HotelService().Calculate("Preview",checkIn.SelectionStart,checkOut.SelectionStart,capacity.SelectedValue,room.SelectedValue,payment.SelectedValue); stay.Text=$"{t.Days} billable day(s)\n{Money.Format(t.Rate)} / day\nEstimated total: {Money.Format(t.Total)}"; } catch(ArgumentException e) { stay.Text=e.Message; }
    }
    private void Compute()
    {
        var t=new HotelService().Calculate(customer.Text,checkIn.SelectionStart,checkOut.SelectionStart,capacity.SelectedValue,room.SelectedValue,payment.SelectedValue);
        receipt.ShowDocument(new("GUEST FOLIO · ESTIMATE",ReceiptData.NewReference("HTL"),DateTime.Now,[new("Guest",customer.Text.Trim()),new("Contact",string.IsNullOrWhiteSpace(contact.Text)?"Not provided":contact.Text.Trim()),new("Check-in",checkIn.SelectionStart.ToString("dd MMM yyyy")),new("Check-out",checkOut.SelectionStart.ToString("dd MMM yyyy")),new("Room",capacity.Text+" / "+room.Text),new("Billable days",t.Days.ToString()),new("Daily rate",Money.Format(t.Rate)),new("Accommodation",Money.Format(t.BaseAmount)),new("Payment",payment.Text),new("Payment surcharge",Money.Format(t.Surcharge))],"TOTAL DUE",Money.Format(t.Total),"Billing estimate only. No reservation or payment is confirmed.")); Status("Guest folio ready to save or print.");
    }
}
