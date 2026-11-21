using DreamOS.Core;
using DreamOS.WinForms.Controls;
using DreamOS.WinForms.Services;
namespace DreamOS.WinForms.Forms;

public sealed class StoreForm : WorkflowForm
{
    private readonly ShoppingCart cart=new(); private readonly ReceiptPanel receipt=new();
    private readonly SelectionCards<int> payment=new([0,1],["Cash","Charge"],["5% discount","5% surcharge"],33);
    private readonly CheckBox delivery=new() { Text="Deliver my order · PHP 150",AutoSize=true };
    private readonly TextBox address=new() { Width=240,MaxLength=240,PlaceholderText="Delivery address",Enabled=false };
    private readonly TextBox serial=new() { Width=240,MaxLength=60,PlaceholderText="Voucher / promo / gift serial" };
    private readonly NumericUpDown quantity=Theme.Number(1,999,1),tendered=Theme.Number(0,1_000_000_000,0,2);
    private readonly DataGridView basket=new() { Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,RowHeadersVisible=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells,BackgroundColor=Color.White,BorderStyle=BorderStyle.FixedSingle,AccessibleName="Cashier basket" };
    private readonly Label review=WorkflowUi.Text("Add appliances to start your sale.",12,true);
    private readonly Label totals=WorkflowUi.Text("Subtotal: PHP 0.00\nAdd products to start a sale.",10,true);
    private IPaymentRule Rule()=>payment.SelectedIndex switch { 0=>new CashPaymentRule(),1=>new ChargePaymentRule(),_=>throw new ArgumentException("Choose cash or charge.") };
    public StoreForm(string username) : base("Dream Appliances · Cashier","Set quantity on each card, add to basket, then follow Payment, Delivery, and Complete sale.")
    {
        var main=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=2,RowCount=1 };
        main.ColumnStyles.Add(new(SizeType.Percent,100)); main.ColumnStyles.Add(new(SizeType.Absolute,310)); main.RowStyles.Add(new(SizeType.Percent,100));
        var catalogPanel=new Panel { Dock=DockStyle.Fill,Padding=new(4) };
        var catalog=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoScroll=true,Padding=new(3) };
        var header=new FlowLayoutPanel { Dock=DockStyle.Top,Height=72,WrapContents=true };
        var search=new TextBox { Width=230,PlaceholderText="Search appliances…",AccessibleName="Search appliances",Margin=new(3,3,3,6) };
        header.Controls.Add(search);  header.Controls.Add(WorkflowUi.Text("Tap a product to add it · scroll to browse all items",8));
        var cards=new List<(Product Product,Control Card)>();
        for(int i=0;i<Catalog.Products.Count;i++)
        {
            var product=Catalog.Products[i];
            var itemQuantity=Theme.Number(1,999,1); itemQuantity.Width=100; itemQuantity.AccessibleName=product.Name+" quantity";
            var tile=new IconCardPanel { Size=new(125,169),Padding=new(3) };
            var card=new ChoiceCard(product.Name,Money.Format(product.Price),15+i,()=>Run(()=> { cart.Add(product,(int)itemQuantity.Value); RefreshBasket(); Status($"Added {itemQuantity.Value:0} × {product.Name}."); })) { Dock=DockStyle.Top,Height=125 };
            itemQuantity.Dock=DockStyle.Bottom; tile.Controls.Add(card); tile.Controls.Add(itemQuantity);
            cards.Add((product,tile)); catalog.Controls.Add(tile);
        }
        search.TextChanged+=(_,_)=> { foreach(var pair in cards)pair.Card.Visible=pair.Product.Name.Contains(search.Text.Trim(),StringComparison.OrdinalIgnoreCase); };
        catalogPanel.Controls.Add(catalog); catalogPanel.Controls.Add(header);
        var cashier=new Panel { Dock=DockStyle.Fill,Padding=new(8),BackColor=Color.FromArgb(247,234,242) };
        var operatorLabel=WorkflowUi.Text("CASHIER\nOperator: "+username,9,true); operatorLabel.AutoSize=false; operatorLabel.Height=43; operatorLabel.Dock=DockStyle.Top;
        basket.Columns.Add("product","Item"); basket.Columns.Add("quantity","Qty"); basket.Columns.Add("amount","Amount"); basket.Columns[0].FillWeight=54; basket.Columns[1].FillWeight=14; basket.Columns[2].FillWeight=32;
        basket.DefaultCellStyle.Font=new("Segoe UI",9); basket.DefaultCellStyle.WrapMode=DataGridViewTriState.True; basket.DefaultCellStyle.SelectionBackColor=Theme.Blue; basket.DefaultCellStyle.SelectionForeColor=Theme.Ink;
        var pay=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new(0,8,0,0) };
        payment.MaximumSize=new(250,0); payment.Width=240; tendered.Width=235; pay.Controls.Add(WorkflowUi.Text("Payment method",9,true)); pay.Controls.Add(payment); pay.Controls.Add(WorkflowUi.Text("Cash tendered",9,true)); pay.Controls.Add(tendered); using(var truck=WorkflowArt.Sprite(50))if(truck is not null)delivery.Image=new Bitmap(truck,new Size(32,32));
        delivery.TextImageRelation=TextImageRelation.ImageBeforeText; delivery.Disposed+=(_,_)=>delivery.Image?.Dispose();
        var dispatch=WorkflowUi.Stack(); dispatch.Controls.Add(new InformationCard("PICKUP OR DELIVERY","Store pickup is free. Delivery costs PHP 150 and requires an address.")); dispatch.Controls.Add(delivery); dispatch.Controls.Add(address); var offerHeading=new FlowLayoutPanel { AutoSize=true,WrapContents=false,Margin=new(0,5,0,0) };
        var giftArt=new PictureBox { Size=new(32,32),SizeMode=PictureBoxSizeMode.Zoom,Image=WorkflowArt.Sprite(51) }; giftArt.Disposed+=(_,_)=>giftArt.Image?.Dispose();
        offerHeading.Controls.Add(giftArt); offerHeading.Controls.Add(WorkflowUi.Text("Voucher / promo / gift check",9,true)); pay.Controls.Add(offerHeading); pay.Controls.Add(serial);
        pay.Controls.Add(Theme.Button("Demo offer serials",()=>DreamDialog.Show(this,string.Join("\n",Checkout.DemoOffers.Select(o=>o.Name+" · "+o.Serial))+"\nOne offer per sale, applied after the cash/charge rule. Delivery costs PHP 150. Codes are reusable classroom examples.","Classroom offers")));
        totals.MaximumSize=Size.Empty; totals.AutoSize=false; totals.Height=88; totals.Dock=DockStyle.Bottom; totals.Padding=new(4); totals.BackColor=Theme.Cream;
        var basketArea=new Panel { Dock=DockStyle.Fill }; var basketActions=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=48,WrapContents=true };
        basketActions.Controls.Add(Theme.Button("Remove selected",()=>Run(()=> { Require.That(basket.SelectedRows.Count>0,"Select a basket row to remove."); cart.RemoveAt(basket.SelectedRows[0].Index); RefreshBasket(); })));
        basketArea.Controls.Add(basket); basketArea.Controls.Add(basketActions);
        cashier.Controls.Add(basketArea); cashier.Controls.Add(totals); cashier.Controls.Add(operatorLabel);
        main.Controls.Add(WorkflowUi.Section("Appliance catalog",catalogPanel),0,0); main.Controls.Add(WorkflowUi.Section("Basket overview",cashier),1,0);
        var steps=new TabControl { Dock=DockStyle.Fill,Multiline=true };
        var shopPage=new TabPage("01 · Appliances + basket"); shopPage.Controls.Add(main);
        var payPage=new TabPage("02 · Payment"); payPage.Controls.Add(WorkflowUi.Section("Payment + optional offer",pay));
        var deliveryPage=new TabPage("03 · Delivery"); deliveryPage.Controls.Add(WorkflowUi.Section("Delivery preferences",dispatch));
        var finalPage=new TabPage("04 · Complete sale"); var final=WorkflowUi.Stack(); final.Controls.Add(review); final.Controls.Add(new InformationCard("REVIEW YOUR SALE","Check your payment and delivery details above, then complete the classroom sale.")); finalPage.Controls.Add(WorkflowUi.Section("Final review",final));
        steps.TabPages.AddRange([shopPage,payPage,deliveryPage,finalPage]); Split(steps,receipt,270);
        payment.SelectedIndexChanged+=(_,_)=> { tendered.Enabled=payment.SelectedIndex==0; UpdateTotals(); }; tendered.ValueChanged+=(_,_)=>UpdateTotals(); payment.SelectedIndex=0;
        delivery.CheckedChanged+=(_,_)=> { address.Enabled=delivery.Checked; UpdateTotals(); }; address.TextChanged+=(_,_)=>UpdateTotals(); serial.TextChanged+=(_,_)=>UpdateTotals();
        Command("Complete sale",Complete); Command("New sale",()=> { Reset(); steps.SelectedTab=shopPage; }); Command("About",()=> { using var about=new AboutForm(); about.ShowDialog(this); }); PlaceActions(final);
    }
    private void RefreshBasket()
    {
        basket.Rows.Clear(); foreach(var item in cart.Items)basket.Rows.Add(item.Product.Name,item.Quantity,Money.Format(item.Amount)); UpdateTotals();
    }
    private void UpdateTotals()
    {
        receipt.Clear("Complete a sale to create the cashier receipt.");
        if(cart.Items.Count==0) { totals.Text="Subtotal: PHP 0.00\nAdd products to start a sale."; review.Text=totals.Text; return; }
        CheckoutTotal quote;
        try { quote=Checkout.Calculate(cart.Total(Rule()),delivery.Checked,serial.Text); }
        catch(ArgumentException ex) { totals.Text=ex.Message; review.Text=ex.Message; return; }
        decimal total=quote.Total; totals.Text=$"Subtotal: {Money.Format(cart.Subtotal)}\nOffer: −{Money.Format(quote.Savings)} · Delivery: {Money.Format(quote.Delivery)}\nTOTAL: {Money.Format(total)}";
        if(payment.SelectedIndex==0)totals.Text+=tendered.Value>=total?$"\nChange: {Money.Format(tendered.Value-total)}":$"\nCash needed: {Money.Format(total-tendered.Value)}";
        review.Text=totals.Text+"\n\nPayment: "+payment.Text+"\n"+(delivery.Checked?"Delivery to: "+address.Text:"Store pickup")+"\nOffer: "+(string.IsNullOrWhiteSpace(serial.Text)?"None":serial.Text.Trim());
    }
    private void Complete()
    {
        var rule=Rule(); decimal ruleTotal=cart.Total(rule); var quote=Checkout.Calculate(ruleTotal,delivery.Checked,serial.Text); decimal total=quote.Total;
        if(delivery.Checked)Require.Text(address.Text,"delivery address");
        if(rule is CashPaymentRule)Require.That(tendered.Value>=total,"Cash tendered must cover the final total.");
        decimal change=rule is CashPaymentRule?tendered.Value-total:0;
        var entries=cart.Items.Select(i=>new ReceiptEntry($"{i.Product.Name} × {i.Quantity}",Money.Format(i.Amount))).ToList();
        entries.Add(new("Subtotal",Money.Format(cart.Subtotal))); entries.Add(new("Payment",rule.Name));
        entries.Add(new(rule is CashPaymentRule?"Cash discount":"Charge surcharge",Money.Format(Math.Abs(ruleTotal-cart.Subtotal))));
        entries.Add(new("Offer serial",string.IsNullOrWhiteSpace(serial.Text)?"None":serial.Text.Trim())); entries.Add(new("Offer savings","− "+Money.Format(quote.Savings)));
        entries.Add(new(delivery.Checked?"Delivery fee":"Store pickup",Money.Format(quote.Delivery))); if(delivery.Checked)entries.Add(new("Deliver to",address.Text.Trim()));
        if(rule is CashPaymentRule) { entries.Add(new("Cash tendered",Money.Format(tendered.Value))); entries.Add(new("Change",Money.Format(change))); }
        receipt.ShowDocument(new("APPLIANCE SALES RECEIPT",ReceiptData.NewReference("POS"),DateTime.Now,entries,"SALE TOTAL",Money.Format(total),"Classroom transaction. No real payment was processed.")); Status("Demo sale complete. Save or print the receipt, or start a new sale.");
    }
    private void Reset() { cart.Clear(); basket.Rows.Clear(); delivery.Checked=false; address.Clear(); serial.Clear(); quantity.Value=1; tendered.Value=0; payment.SelectedIndex=0; UpdateTotals(); Status("New sale ready. Select products from the catalog."); }
}

public sealed class PasswordForm : WorkflowForm
{
    private readonly TextBox username=new() { Width=240,MaxLength=60,PlaceholderText="Your demo name" },password=new() { Width=240,UseSystemPasswordChar=true,MaxLength=100 };
    public PasswordForm(Action<string> openStore) : base("Appliance Store · Sign in","A local classroom password exercise. Demo password: dream10.")
    {
        MinimumSize=new(560,350); Size=new(650,400);
        var main=WorkflowUi.Stack(); main.Padding=new(28); main.Controls.Add(WorkflowUi.Text("Welcome to the cashier",16,true)); WorkflowUi.Field(main,"Username",username); WorkflowUi.Field(main,"Password",password);
        Workspace.Controls.Add(WorkflowUi.Section("Sign in",main));
        Command("Sign in",()=> { Require.Text(username.Text,"username"); if(!new DemoAuthService().Authenticate(username.Text,password.Text)) { if(DreamDialog.Show(this,"Incorrect demo password. Try again?","Password entry",MessageBoxButtons.RetryCancel,MessageBoxIcon.Warning)==DialogResult.Cancel)Close(); else { password.Clear(); password.Focus(); } return; } openStore(username.Text.Trim()); Close(); }); PlaceActions(main);
    }
}

public sealed class ElectionForm : WorkflowForm
{
    private readonly TextBox voter=new() { Width=220,MaxLength=20,PlaceholderText="12345 or 10001–10003" };
    private readonly ComboBox candidate=Theme.Choice(ElectionService.Candidates); private readonly ReceiptPanel receipt=new();
    private readonly DataGridView tally=new() { Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White };
    private readonly ElectionService election; private string? validated;
    public ElectionForm() : base("Classroom Elections 2010","Validate an ID, choose a candidate, and confirm one classroom vote per voter.")
    {
        election=new(LocalData.Store("classroom-votes",()=>new ElectionState()));
        var main=WorkflowUi.Grid(); var ballot=WorkflowUi.Stack(); WorkflowUi.Field(ballot,"Demo voter ID",voter);
        ballot.Controls.Add(Theme.Button("Validate ID",()=>Run(()=> { var name=election.ValidateVoter(voter.Text.Trim()); validated=voter.Text.Trim(); candidate.Enabled=true; Status($"ID accepted · {name}. Choose a candidate."); })));
        WorkflowUi.Field(ballot,"Candidate",candidate); candidate.Enabled=false;
        var directory=WorkflowUi.Stack(); directory.Controls.Add(new InformationCard("DEMO VOTER DIRECTORY","Choose your classroom ID below, then validate it. Each ID can vote once."));
        foreach(var entry in ElectionService.DemoVoters) { var id=entry.Key; directory.Controls.Add(Theme.Button(entry.Value+" · "+id,()=> { voter.Text=id; Status("ID selected. Click Validate ID to check eligibility."); })); }
        tally.Columns.Add("candidate","Candidate"); tally.Columns.Add("votes","Votes"); main.Controls.Add(WorkflowUi.Section("01 · Voter directory",directory),0,0); main.Controls.Add(WorkflowUi.Section("02 · Validate + vote",ballot),1,0);
        var electionTabs=new TabControl { Dock=DockStyle.Fill }; var votePage=new TabPage("Voter + ballot"); votePage.Controls.Add(main);
        var resultsPage=new TabPage("Results + management"); var resultsBody=new Panel { Dock=DockStyle.Fill }; var resultsActions=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=100,WrapContents=true,Padding=new(8) };
        resultsBody.Controls.Add(tally); resultsBody.Controls.Add(resultsActions); resultsPage.Controls.Add(WorkflowUi.Section("Election results",resultsBody)); electionTabs.TabPages.AddRange([votePage,resultsPage]); Split(electionTabs,receipt);
        voter.TextChanged+=(_,_)=> { validated=null; candidate.Enabled=false; candidate.SelectedIndex=-1; receipt.Clear(); };
        Command("Cast vote",()=> { Require.That(validated is not null,"Validate your voter ID first."); Require.That(candidate.SelectedIndex>=0,"Choose a candidate."); if(DreamDialog.Show(this,$"Confirm your classroom vote for {candidate.Text}?","Confirm ballot",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return; string id=validated!,choice=candidate.Text; election.Vote(id,choice); voter.Clear(); RefreshTally(); receipt.ShowDocument(new("CLASSROOM VOTE CONFIRMATION",ReceiptData.NewReference("VOTE"),DateTime.Now,[new("Voter ID",id),new("Candidate",choice)],"STATUS","RECORDED","Classroom demonstration. No real public-election vote.")); Status("Vote recorded. This voter ID cannot vote again."); });
        resultsActions.Controls.Add(Theme.Button("Show result",()=>Run(()=>DreamDialog.Show(this,election.Result(),"Classroom results"))));
        resultsActions.Controls.Add(Theme.Button("Reset election",()=>Run(()=> { if(DreamDialog.Show(this,"Erase all locally saved classroom votes?","Reset",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes) { election.Reset(); voter.Clear(); RefreshTally(); receipt.Clear(); } })));
        PlaceActions(ballot); RefreshTally();
    }
    private void RefreshTally() { tally.Rows.Clear(); foreach(var row in election.Tally())tally.Rows.Add(row.Key,row.Value); }
}

public sealed class AboutForm : WorkflowForm
{
    public AboutForm() : base("About Dream OS","Small programs. Big dreams.")
    {
        MinimumSize=new(560,400); Size=new(690,470);
        var grid=WorkflowUi.Grid(); grid.ColumnStyles[0].Width=30; grid.ColumnStyles[1].Width=70;
        var portrait=new PixelPortrait { Dock=DockStyle.Fill };
        var copy=WorkflowUi.Stack(); copy.Controls.Add(WorkflowUi.Text("A little world of your own",17,true)); copy.Controls.Add(WorkflowUi.Text("Dream OS brings nine classroom exercises into a pastel, pixel-inspired C# desktop.\n\nNative Windows Forms · .NET 10\nProject owner: ceias\n\nSupplied artwork and exercise references retain their original credits. Authentication, voting and lotto are educational simulations.",10));
        grid.Controls.Add(portrait,0,0); grid.Controls.Add(WorkflowUi.Section("About this little world",copy),1,0); Workspace.Controls.Add(grid);
        Command("Author",()=>DreamDialog.Show(this,"Dream OS project owner: ceias\nC# OOP coursework\nOriginal exercise/reference authors retain credit for their materials.","Author")); PlaceActions(copy);
    }
}
