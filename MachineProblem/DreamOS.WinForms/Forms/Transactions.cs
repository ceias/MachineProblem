using DreamOS.Core;
using DreamOS.WinForms.Controls;
using DreamOS.WinForms.Services;
namespace DreamOS.WinForms.Forms;

public sealed class StoreForm : WorkflowForm
{
    private readonly ShoppingCart cart=new(); private readonly ReceiptPanel receipt=new();
    private readonly ComboBox payment=Theme.Choice(["Cash · 5% discount","Charge · 5% surcharge"]);
    private readonly NumericUpDown quantity=Theme.Number(1,999,1),tendered=Theme.Number(0,1_000_000_000,0,2);
    private readonly DataGridView basket=new() { Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,RowHeadersVisible=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells,BackgroundColor=Color.White,BorderStyle=BorderStyle.FixedSingle,AccessibleName="Cashier basket" };
    private readonly Label totals=WorkflowUi.Text("Subtotal: PHP 0.00\nAdd products to start a sale.",10,true);
    private IPaymentRule Rule()=>payment.SelectedIndex switch { 0=>new CashPaymentRule(),1=>new ChargePaymentRule(),_=>throw new ArgumentException("Choose cash or charge.") };
    public StoreForm(string username) : base("Dream Appliances · Cashier","Click an appliance card to add it to the basket. Review payment and complete the demo sale.")
    {
        var main=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=2,RowCount=1 };
        main.ColumnStyles.Add(new(SizeType.Percent,100)); main.ColumnStyles.Add(new(SizeType.Absolute,270)); main.RowStyles.Add(new(SizeType.Percent,100));
        var catalogPanel=new Panel { Dock=DockStyle.Fill,Padding=new(4) };
        var catalog=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoScroll=true,Padding=new(3) };
        var header=new FlowLayoutPanel { Dock=DockStyle.Top,Height=72,WrapContents=true };
        var search=new TextBox { Width=230,PlaceholderText="Search appliances…",AccessibleName="Search appliances",Margin=new(3,3,3,6) };
        header.Controls.Add(search); header.Controls.Add(WorkflowUi.Text("Quantity per click",9)); quantity.Width=72; header.Controls.Add(quantity); header.Controls.Add(WorkflowUi.Text("Tap a product to add it · scroll to browse all items",8));
        var cards=new List<(Product Product,ChoiceCard Card)>();
        for(int i=0;i<Catalog.Products.Count;i++)
        {
            var product=Catalog.Products[i];
            var card=new ChoiceCard(product.Name,Money.Format(product.Price),15+i,()=>Run(()=> { cart.Add(product,(int)quantity.Value); RefreshBasket(); Status($"Added {quantity.Value:0} × {product.Name}."); })) { Size=new(110,119) };
            cards.Add((product,card)); catalog.Controls.Add(card);
        }
        search.TextChanged+=(_,_)=> { foreach(var pair in cards)pair.Card.Visible=pair.Product.Name.Contains(search.Text.Trim(),StringComparison.OrdinalIgnoreCase); };
        catalogPanel.Controls.Add(catalog); catalogPanel.Controls.Add(header);
        var cashier=new Panel { Dock=DockStyle.Fill,Padding=new(8),BackColor=Color.FromArgb(247,234,242) };
        var operatorLabel=WorkflowUi.Text("CASHIER\nOperator: "+username,9,true); operatorLabel.AutoSize=false; operatorLabel.Height=43; operatorLabel.Dock=DockStyle.Top;
        basket.Columns.Add("product","Item"); basket.Columns.Add("quantity","Qty"); basket.Columns.Add("amount","Amount"); basket.Columns[0].FillWeight=54; basket.Columns[1].FillWeight=14; basket.Columns[2].FillWeight=32;
        basket.DefaultCellStyle.Font=new("Tahoma",9); basket.DefaultCellStyle.WrapMode=DataGridViewTriState.True; basket.DefaultCellStyle.SelectionBackColor=Theme.Blue; basket.DefaultCellStyle.SelectionForeColor=Theme.Ink;
        var pay=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=177,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new(0,8,0,0) };
        payment.Width=235; tendered.Width=235; pay.Controls.Add(WorkflowUi.Text("Payment method",9,true)); pay.Controls.Add(payment); pay.Controls.Add(WorkflowUi.Text("Cash tendered",9,true)); pay.Controls.Add(tendered); totals.MaximumSize=new(238,0); pay.Controls.Add(totals);
        cashier.Controls.Add(basket); cashier.Controls.Add(pay); cashier.Controls.Add(operatorLabel);
        main.Controls.Add(catalogPanel,0,0); main.Controls.Add(cashier,1,0); Split(main,receipt,270);
        payment.SelectedIndexChanged+=(_,_)=> { tendered.Enabled=payment.SelectedIndex==0; UpdateTotals(); }; tendered.ValueChanged+=(_,_)=>UpdateTotals(); payment.SelectedIndex=0;
        Command("Complete sale",Complete); Command("Remove item",()=> { Require.That(basket.SelectedRows.Count>0,"Select a basket row to remove."); cart.RemoveAt(basket.SelectedRows[0].Index); RefreshBasket(); }); Command("New sale",Reset); Command("About",()=> { using var about=new AboutForm(); about.ShowDialog(this); }); Command("Close",Close);
    }
    private void RefreshBasket()
    {
        basket.Rows.Clear(); foreach(var item in cart.Items)basket.Rows.Add(item.Product.Name,item.Quantity,Money.Format(item.Amount)); UpdateTotals();
    }
    private void UpdateTotals()
    {
        receipt.Clear("Complete a sale to create the cashier receipt.");
        if(cart.Items.Count==0) { totals.Text="Subtotal: PHP 0.00\nAdd products to start a sale."; return; }
        decimal total=cart.Total(Rule()); totals.Text=$"Subtotal: {Money.Format(cart.Subtotal)}\nTOTAL: {Money.Format(total)}";
        if(payment.SelectedIndex==0)totals.Text+=tendered.Value>=total?$"\nChange: {Money.Format(tendered.Value-total)}":$"\nCash needed: {Money.Format(total-tendered.Value)}";
    }
    private void Complete()
    {
        var rule=Rule(); decimal total=cart.Total(rule); decimal change=rule is CashPaymentRule?cart.Change(rule,tendered.Value):0;
        var entries=cart.Items.Select(i=>new ReceiptEntry($"{i.Product.Name} × {i.Quantity}",Money.Format(i.Amount))).ToList();
        entries.Add(new("Subtotal",Money.Format(cart.Subtotal))); entries.Add(new("Payment",rule.Name));
        entries.Add(new(rule is CashPaymentRule?"Cash discount":"Charge surcharge",Money.Format(Math.Abs(total-cart.Subtotal))));
        if(rule is CashPaymentRule) { entries.Add(new("Cash tendered",Money.Format(tendered.Value))); entries.Add(new("Change",Money.Format(change))); }
        receipt.ShowDocument(new("APPLIANCE SALES RECEIPT",ReceiptData.NewReference("POS"),DateTime.Now,entries,"SALE TOTAL",Money.Format(total),"Classroom transaction. No real payment was processed.")); Status("Demo sale complete. Save or print the receipt, or start a new sale.");
    }
    private void Reset() { cart.Clear(); basket.Rows.Clear(); quantity.Value=1; tendered.Value=0; payment.SelectedIndex=0; UpdateTotals(); Status("New sale ready. Select products from the catalog."); }
}

public sealed class PasswordForm : WorkflowForm
{
    private readonly TextBox username=new() { Width=240,MaxLength=60,PlaceholderText="Your demo name" },password=new() { Width=240,UseSystemPasswordChar=true,MaxLength=100 };
    public PasswordForm(Action<string> openStore) : base("Appliance Store · Sign in","A local classroom password exercise. Demo password: dream10.")
    {
        MinimumSize=new(560,350); Size=new(650,400);
        var main=WorkflowUi.Stack(); main.Padding=new(28); main.Controls.Add(WorkflowUi.Text("Welcome to the cashier",16,true)); WorkflowUi.Field(main,"Username",username); WorkflowUi.Field(main,"Password",password);
        Workspace.Controls.Add(main);
        Command("Sign in",()=> { Require.Text(username.Text,"username"); if(!new DemoAuthService().Authenticate(username.Text,password.Text)) { if(MessageBox.Show(this,"Incorrect demo password. Try again?","Password entry",MessageBoxButtons.RetryCancel,MessageBoxIcon.Warning)==DialogResult.Cancel)Close(); else { password.Clear(); password.Focus(); } return; } openStore(username.Text.Trim()); Close(); }); Command("Cancel",Close);
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
        ballot.Controls.Add(WorkflowUi.Text("Demo IDs: 12345, 10001, 10002, 10003.\nThis is not a real public election.",9));
        tally.Columns.Add("candidate","Candidate"); tally.Columns.Add("votes","Votes"); main.Controls.Add(WorkflowUi.Section("Your classroom ballot",ballot),0,0); main.Controls.Add(WorkflowUi.Section("Live tally",tally),1,0); Split(main,receipt);
        voter.TextChanged+=(_,_)=> { validated=null; candidate.Enabled=false; candidate.SelectedIndex=-1; receipt.Clear(); };
        Command("Cast vote",()=> { Require.That(validated is not null,"Validate your voter ID first."); Require.That(candidate.SelectedIndex>=0,"Choose a candidate."); if(MessageBox.Show(this,$"Confirm your classroom vote for {candidate.Text}?","Confirm ballot",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return; string id=validated!,choice=candidate.Text; election.Vote(id,choice); voter.Clear(); RefreshTally(); receipt.ShowDocument(new("CLASSROOM VOTE CONFIRMATION",ReceiptData.NewReference("VOTE"),DateTime.Now,[new("Voter ID",id),new("Candidate",choice)],"STATUS","RECORDED","Classroom demonstration. No real public-election vote.")); Status("Vote recorded. This voter ID cannot vote again."); });
        Command("Show result",()=>Status(election.Result())); Command("Reset election",()=> { if(MessageBox.Show(this,"Erase all locally saved classroom votes?","Reset",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes) { election.Reset(); voter.Clear(); RefreshTally(); receipt.Clear(); } }); Command("Close",Close); RefreshTally();
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
        grid.Controls.Add(portrait,0,0); grid.Controls.Add(copy,1,0); Workspace.Controls.Add(grid);
        Command("Author",()=>MessageBox.Show(this,"Dream OS project owner: ceias\nC# OOP coursework\nOriginal exercise/reference authors retain credit for their materials.","Author")); Command("Close",Close);
    }
}
