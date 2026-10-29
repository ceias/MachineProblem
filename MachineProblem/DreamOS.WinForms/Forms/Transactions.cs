using DreamOS.Core;
using DreamOS.WinForms.Controls;
using DreamOS.WinForms.Services;
namespace DreamOS.WinForms.Forms;

public sealed class StoreForm : ExerciseForm
{
    private readonly ShoppingCart cart = new();
    private readonly ComboBox product = Theme.Choice(Catalog.Products.Select(p => p.Name)), payment = Theme.Choice(["Cash (5% discount)", "Charge (5% surcharge)"]);
    private readonly NumericUpDown quantity = Theme.Number(0, 999), tendered = Theme.Number(0, 1_000_000_000, 0, 2);
    private readonly ListBox lines = new() { Width = 620, Height = 125, HorizontalScrollbar = true, AccessibleName = "Cart items" };
    private readonly Label unitPrice = Theme.Label("Select an appliance.");
    private IPaymentRule Payment() => payment.SelectedIndex switch { 0 => new CashPaymentRule(), 1 => new ChargePaymentRule(), _ => throw new ArgumentException("Select cash or charge.") };
    public StoreForm(string username) : base("Appliance Store Transaction")
    {
        Info($"Demo operator: {username} • {DateTime.Now:g}");
        var menu = new MenuStrip(); var file = new ToolStripMenuItem("File"); file.DropDownItems.Add("New transaction", null, (_, _) => Reset()); file.DropDownItems.Add("Exit", null, (_, _) => Close());
        var help = new ToolStripMenuItem("Help"); help.DropDownItems.Add("About", null, (_, _) => { using var about = new AboutForm(); about.ShowDialog(this); }); menu.Items.AddRange([file, help]); Body.Controls.Add(menu); MainMenuStrip = menu;
        Field("Appliance", product); Body.Controls.Add(unitPrice); Field("Quantity", quantity); Body.Controls.Add(Theme.Button("Add to cart", () => Theme.Guard(this, () => { Require.That(product.SelectedIndex >= 0, "Select an appliance."); cart.Add(Catalog.Products[product.SelectedIndex], (int)quantity.Value); RefreshCart(); })));
        Body.Controls.Add(lines); Body.Controls.Add(Theme.Button("Remove selected row", () => Theme.Guard(this, () => { cart.RemoveAt(lines.SelectedIndex); RefreshCart(); })));
        Field("Payment", payment); Field("Amount tendered", tendered);
        product.SelectedIndexChanged += (_, _) => { unitPrice.Text = product.SelectedIndex >= 0 ? $"Unit price: {Money.Format(Catalog.Products[product.SelectedIndex].Price)}" : "Select an appliance."; };
        payment.SelectedIndexChanged += (_, _) => { tendered.Enabled = payment.SelectedIndex == 0; ShowSubtotal(); };
        tendered.ValueChanged += (_, _) => ShowSubtotal();
        Actions(("Compute", () => { var rule = Payment(); decimal total = cart.Total(rule); Output.Text = $"Subtotal: {Money.Format(cart.Subtotal)}\n{rule.Name}\nTOTAL: {Money.Format(total)}"; if (rule is CashPaymentRule) Output.Text += $"\nChange: {Money.Format(cart.Change(rule, tendered.Value))}"; }),
            ("Clear selection", () => { product.SelectedIndex = payment.SelectedIndex = -1; quantity.Value = tendered.Value = 0; ShowSubtotal(); }), ("New transaction", Reset));
        tendered.Enabled = false; Size = new(800, 770);
    }
    private void RefreshCart() { lines.Items.Clear(); foreach (var i in cart.Items) lines.Items.Add($"{i.Product.Name} × {i.Quantity} @ {Money.Format(i.Product.Price)} = {Money.Format(i.Amount)}"); ShowSubtotal(); }
    private void ShowSubtotal() => Output.Text = $"Subtotal: {Money.Format(cart.Subtotal)}\nCompute after choosing a payment method.";
    private void Reset() { cart.Clear(); product.SelectedIndex = payment.SelectedIndex = -1; quantity.Value = tendered.Value = 0; RefreshCart(); }
}

public sealed class PasswordForm : ExerciseForm
{
    private readonly TextBox username = new() { Width = 245, MaxLength = 60 }, password = new() { Width = 245, UseSystemPasswordChar = true, MaxLength = 100 };
    public PasswordForm(Action<string> openStore) : base("Password Entry • appliance store")
    {
        Info("Classroom password exercise. Enter any username and demo password dream10.\nSuccessful entry opens the appliance transaction. No real account security.");
        Field("Username", username); Field("Password", password);
        Actions(("Accept", () =>
        {
            Require.Text(username.Text, "username");
            if (!new DemoAuthService().Authenticate(username.Text, password.Text))
            {
                if (MessageBox.Show(this, "Incorrect demo password. Retry?", "Password entry", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Cancel) Close();
                else { password.Clear(); password.Focus(); } return;
            }
            openStore(username.Text.Trim()); Close();
        }), ("Cancel", Close));
    }
}

public sealed class ElectionForm : ExerciseForm
{
    private readonly TextBox voter = new() { Width = 245, MaxLength = 20 };
    private readonly ComboBox candidate = Theme.Choice(ElectionService.Candidates);
    private readonly Label tally = Theme.Label("");
    private readonly ElectionService election;
    private string? validatedId;
    public ElectionForm() : base("Presidential Elections 2010 • classroom simulation")
    {
        election = new(LocalData.Store("classroom-votes", () => new ElectionState()));
        Info("Fictional classroom election • no real election results.\nDemo voter IDs: 12345, 10001, 10002, 10003. One vote per ID; votes saved locally.");
        Field("Voter ID", voter); voter.TextChanged += (_, _) => { validatedId = null; candidate.Enabled = false; ResetOutput(); };
        Body.Controls.Add(Theme.Button("Validate ID", () => Theme.Guard(this, () => { string name = election.ValidateVoter(voter.Text.Trim()); validatedId = voter.Text.Trim(); candidate.Enabled = true; Output.Text = $"ID accepted: {name}. Choose a candidate."; candidate.Focus(); })));
        candidate.Enabled = false; Field("Candidate", candidate); Body.Controls.Add(tally);
        Actions(("Vote", () =>
        {
            Require.That(validatedId is not null, "Validate your voter ID first."); Require.That(candidate.SelectedIndex >= 0, "Select a candidate.");
            if (MessageBox.Show(this, $"Cast your classroom vote for {candidate.Text}?", "Confirm vote", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            election.Vote(validatedId!, candidate.Text); voter.Clear(); candidate.SelectedIndex = -1; RefreshTally(); Output.Text = "Vote recorded. Thank you! Enter another demo voter ID.";
        }), ("Result", () => Output.Text = election.Result()), ("Reset demo election", () =>
        {
            if (MessageBox.Show(this, "Erase all locally saved classroom votes?", "Reset demo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            election.Reset(); voter.Clear(); candidate.SelectedIndex = -1; RefreshTally(); ResetOutput();
        })); RefreshTally();
    }
    private void RefreshTally() => tally.Text = "ACTUAL CLASSROOM TALLY\n" + string.Join("   •   ", election.Tally().Select(v => $"{v.Key}: {v.Value}"));
}

public sealed class AboutForm : ExerciseForm
{
    public AboutForm() : base("About Dream OS")
    {
        Body.Controls.Add(new PixelPortrait());
        Info("♥ DREAM OS ♥\nSmall programs. Big dreams.\n\nA native C# WinForms / .NET 10 classroom project.\nPastel desktop, nine exercises, and a guide for curious dreamers.\n\nOriginal supplied artwork and exercise references are included in the repository.\nDemo login, voting and lotto are educational simulations.");
        Actions(("Author", () => MessageBox.Show(this, "Dream OS project owner: ceias\nC# OOP coursework project\nAuthor name can be personalized in AboutForm.cs.\nReference exercise authors retain credit for their materials.", "Author", MessageBoxButtons.OK, MessageBoxIcon.Information)), ("OK", Close));
    }
}
