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
        Command("Build order",Compute); Command("Clear toppings",()=> { toppings.Clear(); Changed(); }); Command("Start over",Reset); Command("Close",Close); Changed();
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
        bet.ValueChanged+=(_,_)=>receipt.Clear(); Split(main,receipt);
        Command("Draw numbers",Draw); Command("New ticket",()=> { for(int i=0;i<3;i++)digits[i].Value=i; bet.Value=10; winners.Clear(); receipt.Clear(); }); Command("Close",Close);
    }
    private void Draw()
    {
        var service=new LottoService(); var ticket=digits.Select(d=>(int)d.Value).ToArray(); service.ValidateTicket(bet.Value,ticket); var draw=service.Draw(); var result=service.Evaluate(bet.Value,ticket,draw); winners.ShowNumbers(draw);
        receipt.ShowDocument(new("CLASSROOM LOTTO TICKET",ReceiptData.NewReference("LOT"),DateTime.Now,[new("Your digits",string.Join(" · ",ticket)),new("Demo bet",Money.Format(bet.Value)),new("Winning digits",string.Join(" · ",draw)),new("Outcome",result.Outcome.ToString())],"FICTIONAL PRIZE",Money.Format(result.Prize),"Educational simulation. Not a real-money ticket."));
        Status(result.Outcome==LottoOutcome.NoMatch?"No match this time. You can try another demo draw.":$"{result.Outcome}! Your fictional prize is {Money.Format(result.Prize)}.");
    }
    private sealed class NumberBalls : Control
    {
        private int[]? numbers;
        public NumberBalls() { DoubleBuffered=true; BackColor=Theme.Cream; }
        public void ShowNumbers(int[] values) { numbers=values; AccessibleName="Winning digits: "+string.Join(", ",values); Invalidate(); }
        public void Clear() { numbers=null; AccessibleName="Winning digits not drawn"; Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); using var font=new Font("Consolas",26,FontStyle.Bold);
            for(int i=0;i<3;i++) { var r=new Rectangle(8+i*100,8,72,66); using var fill=new SolidBrush(new[]{Color.FromArgb(244,199,219),Color.FromArgb(203,218,248),Color.FromArgb(218,233,218)}[i]); e.Graphics.FillEllipse(fill,r); TextRenderer.DrawText(e.Graphics,numbers is null?"?":numbers[i].ToString(),font,r,Theme.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter); }
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
        academic.Controls.Add(WorkflowUi.Text("PHP 100 per academic unit\nPHP 1,000 per laboratory unit",10));
        grid.Controls.Add(WorkflowUi.Section("Enrollment",enrollment),0,0); grid.Controls.Add(WorkflowUi.Section("Study load",academic),1,0);
        var fees=new DataGridView { Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White,BorderStyle=BorderStyle.FixedSingle };
        fees.Columns.Add("year","Year level"); fees.Columns.Add("it","BSIT / BSCS"); fees.Columns.Add("other","BSA / BSN");
        foreach(var y in Enum.GetValues<YearLevel>())fees.Rows.Add(y,Money.Format(new TuitionService().Calculate(Course.BSIT,y,3,0).Miscellaneous),Money.Format(new TuitionService().Calculate(Course.BSA,y,3,0).Miscellaneous));
        grid.Controls.Add(fees,0,1); grid.SetColumnSpan(fees,2); Split(grid,receipt);
        course.SelectedIndexChanged+=(_,_)=>receipt.Clear(); year.SelectedIndexChanged+=(_,_)=>receipt.Clear(); units.ValueChanged+=(_,_)=>receipt.Clear(); labs.ValueChanged+=(_,_)=>receipt.Clear(); student.TextChanged+=(_,_)=>receipt.Clear();
        Command("Assess fees",Compute); Command("Clear",()=> { student.Clear(); course.SelectedIndex=year.SelectedIndex=-1; units.Value=3; labs.Value=0; receipt.Clear(); }); Command("Close",Close);
    }
    private void Compute()
    {
        var t=new TuitionService().Calculate(Theme.Selected<Course>(course),Theme.Selected<YearLevel>(year),(int)units.Value,(int)labs.Value);
        receipt.ShowDocument(new("TUITION ASSESSMENT",ReceiptData.NewReference("EDU"),DateTime.Now,[new("Student",string.IsNullOrWhiteSpace(student.Text)?"Classroom student":student.Text.Trim()),new("Course / year",$"{course.Text} / {year.Text}"),new($"Tuition · {units.Value:0} units",Money.Format(t.Tuition)),new($"Laboratory · {labs.Value:0} units",Money.Format(t.Lab)),new("Miscellaneous",Money.Format(t.Miscellaneous))],"TOTAL ASSESSED",Money.Format(t.Total),"Assessment only. This slip does not acknowledge payment.")); Status("Assessment ready. The rates follow the supplied exercise table.");
    }
}

public sealed class PayrollForm : WorkflowForm
{
    private readonly ComboBox employee=Theme.Choice(["EMP001 — Alex Santos","EMP002 — Sam Reyes","EMP003 — Jamie Cruz"]),position=Theme.Choice<Position>(),status=Theme.Choice<CivilStatus>();
    private readonly NumericUpDown days=Theme.Number(1,31,20);
    private readonly DateTimePicker period=new() { Format=DateTimePickerFormat.Custom,CustomFormat="MMMM yyyy",ShowUpDown=true,Width=220 };
    private readonly ReceiptPanel receipt=new();
    public PayrollForm() : base("Employee Payroll","Create a readable payslip with gross earnings, deductions, and net pay.")
    {
        var grid=WorkflowUi.Grid(); var profile=WorkflowUi.Stack(); var work=WorkflowUi.Stack();
        WorkflowUi.Field(profile,"Employee · sample records",employee); WorkflowUi.Field(profile,"Position code",position); WorkflowUi.Field(profile,"Civil status",status);
        WorkflowUi.Field(work,"Payroll month",period); WorkflowUi.Field(work,"Days worked · 1–31",days);
        work.Controls.Add(WorkflowUi.Text("DAILY RATES",10,true)); work.Controls.Add(WorkflowUi.Text("Position A   PHP 500\nPosition B   PHP 400\nPosition C   PHP 300",10));
        work.Controls.Add(WorkflowUi.Text("Deductions follow the reference SSS brackets.\nSingle: 10% tax\nMarried / Widowed: 5% tax",9));
        grid.Controls.Add(WorkflowUi.Section("Employee profile",profile),0,0); grid.Controls.Add(WorkflowUi.Section("Pay period + attendance",work),1,0); Split(grid,receipt);
        foreach(var c in new[]{employee,position,status})c.SelectedIndexChanged+=(_,_)=>receipt.Clear(); days.ValueChanged+=(_,_)=>receipt.Clear(); period.ValueChanged+=(_,_)=>receipt.Clear();
        Command("Generate payslip",Compute); Command("Clear",()=> { employee.SelectedIndex=position.SelectedIndex=status.SelectedIndex=-1; days.Value=20; period.Value=DateTime.Today; receipt.Clear(); }); Command("Close",Close);
    }
    private void Compute()
    {
        Require.That(employee.SelectedIndex>=0,"Select an employee."); var p=new PayrollService().Calculate(Theme.Selected<Position>(position),Theme.Selected<CivilStatus>(status),(int)days.Value);
        receipt.ShowDocument(new("EMPLOYEE PAYSLIP",ReceiptData.NewReference("PAY"),DateTime.Now,[new("Employee",employee.Text),new("Period",period.Value.ToString("MMMM yyyy")),new("Position / status",position.Text+" / "+status.Text),new("Days worked",days.Value.ToString("0")),new("Daily rate",Money.Format(p.Rate)),new("Gross earnings",Money.Format(p.Basic)),new("SSS deduction","− "+Money.Format(p.Sss)),new("Withholding tax","− "+Money.Format(p.Tax))],"NET TAKE-HOME PAY",Money.Format(p.Net),"Classroom payroll statement. Keep this copy for your records.")); Status($"Payslip ready · net pay {Money.Format(p.Net)}.");
    }
}

public sealed class HotelForm : WorkflowForm
{
    private readonly TextBox customer=new() { Width=230,MaxLength=100,PlaceholderText="Guest full name" },contact=new() { Width=230,MaxLength=80,PlaceholderText="Phone or email (optional)" };
    private readonly DateTimePicker checkIn=new() { Format=DateTimePickerFormat.Short,Width=180 },checkOut=new() { Format=DateTimePickerFormat.Short,Width=180 };
    private readonly ComboBox capacity=Theme.Choice<Capacity>(),room=Theme.Choice<RoomType>(),payment=Theme.Choice<HotelPayment>();
    private readonly Label stay=WorkflowUi.Text("Choose room details to see your stay estimate.",10,true);
    private readonly ReceiptPanel receipt=new();
    public HotelForm() : base("Dream Hotel · Front Desk","Guest details → stay dates → room and payment → itemized guest folio.")
    {
        var grid=WorkflowUi.Grid(); grid.RowCount=2; grid.RowStyles.Clear(); grid.RowStyles.Add(new(SizeType.Percent,55)); grid.RowStyles.Add(new(SizeType.Percent,45));
        var guest=WorkflowUi.Stack(); WorkflowUi.Field(guest,"Guest name",customer); WorkflowUi.Field(guest,"Contact",contact); WorkflowUi.Field(guest,"Payment method",payment);
        var dates=WorkflowUi.Stack(); WorkflowUi.Field(dates,"Check-in",checkIn); WorkflowUi.Field(dates,"Check-out",checkOut); dates.Controls.Add(WorkflowUi.Text("Same-day stays are billed as one day.\nCash 0% · Check +5% · Credit card +10%",9));
        var rooms=WorkflowUi.Stack(); WorkflowUi.Field(rooms,"Room capacity",capacity); WorkflowUi.Field(rooms,"Room category",room);
        var overview=WorkflowUi.Stack(); overview.Controls.Add(WorkflowUi.Text("STAY OVERVIEW",10,true)); overview.Controls.Add(stay); overview.Controls.Add(WorkflowUi.Text("This is a billing estimate.\nRoom inventory and reservations are not tracked.",9));
        grid.Controls.Add(WorkflowUi.Section("Guest profile",guest),0,0); grid.Controls.Add(WorkflowUi.Section("Dates + duration",dates),1,0); grid.Controls.Add(WorkflowUi.Section("Room selection",rooms),0,1); grid.Controls.Add(overview,1,1); Split(grid,receipt);
        checkOut.Value=DateTime.Today.AddDays(1);
        foreach(var c in new[]{capacity,room,payment})c.SelectedIndexChanged+=(_,_)=>Changed(); checkIn.ValueChanged+=(_,_)=> { checkOut.MinDate=checkIn.Value.Date; Changed(); }; checkOut.ValueChanged+=(_,_)=>Changed(); customer.TextChanged+=(_,_)=>receipt.Clear(); contact.TextChanged+=(_,_)=>receipt.Clear();
        Command("Create guest folio",Compute); Command("New guest",()=> { customer.Clear(); contact.Clear(); capacity.SelectedIndex=room.SelectedIndex=payment.SelectedIndex=-1; checkIn.Value=DateTime.Today; checkOut.Value=DateTime.Today.AddDays(1); Changed(); }); Command("Close",Close);
    }
    private void Changed()
    {
        receipt.Clear(); stay.Text="Choose capacity, category and payment.";
        if(capacity.SelectedIndex<0||room.SelectedIndex<0||payment.SelectedIndex<0)return;
        try { var t=new HotelService().Calculate("Preview",checkIn.Value,checkOut.Value,Theme.Selected<Capacity>(capacity),Theme.Selected<RoomType>(room),Theme.Selected<HotelPayment>(payment)); stay.Text=$"{t.Days} billable day(s)\n{Money.Format(t.Rate)} / day\nEstimated total: {Money.Format(t.Total)}"; } catch(ArgumentException e) { stay.Text=e.Message; }
    }
    private void Compute()
    {
        var t=new HotelService().Calculate(customer.Text,checkIn.Value,checkOut.Value,Theme.Selected<Capacity>(capacity),Theme.Selected<RoomType>(room),Theme.Selected<HotelPayment>(payment));
        receipt.ShowDocument(new("GUEST FOLIO · ESTIMATE",ReceiptData.NewReference("HTL"),DateTime.Now,[new("Guest",customer.Text.Trim()),new("Contact",string.IsNullOrWhiteSpace(contact.Text)?"Not provided":contact.Text.Trim()),new("Check-in",checkIn.Value.ToString("dd MMM yyyy")),new("Check-out",checkOut.Value.ToString("dd MMM yyyy")),new("Room",capacity.Text+" / "+room.Text),new("Billable days",t.Days.ToString()),new("Daily rate",Money.Format(t.Rate)),new("Accommodation",Money.Format(t.BaseAmount)),new("Payment",payment.Text),new("Payment surcharge",Money.Format(t.Surcharge))],"TOTAL DUE",Money.Format(t.Total),"Billing estimate only. No reservation or payment is confirmed.")); Status("Guest folio ready to save or print.");
    }
}
