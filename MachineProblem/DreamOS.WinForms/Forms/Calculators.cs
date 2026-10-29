using DreamOS.Core;
using DreamOS.WinForms.Controls;

namespace DreamOS.WinForms.Forms;

public sealed class PizzaForm : ExerciseForm
{
    private readonly ComboBox size = Theme.Choice<PizzaSize>(), crust = Theme.Choice<Crust>(), drink = Theme.Choice<Drink>(), dining = Theme.Choice(["Dine in", "Take out"]);
    private readonly CheckedListBox toppings = new() { CheckOnClick = true, Height = 125, Width = 245 };
    private global::DreamOS.Core.PizzaOrder Order() => new(
        Theme.Selected<PizzaSize>(size),
        Theme.Selected<Crust>(crust),
        toppings.CheckedItems.Cast<Topping>().ToArray(),
        Theme.Selected<Drink>(drink),
        dining.SelectedIndex == 1);
    public PizzaForm() : base("Special Pizza Builder")
    {
        toppings.Items.AddRange(Enum.GetValues<Topping>().Cast<object>().ToArray());
        Field("Size", size); Field("Crust", crust); Field("Toppings • PHP 10 each", toppings); Field("Drink", drink); Field("Dining", dining);
        Info("Small 100 • Medium 150 • Large 200. Thick crust adds 50% of the size price.");
        Actions(("Compute", Compute), ("Build pizza", () => { Compute(); MessageBox.Show(this, $"{dining.Text}\n{size.Text} pizza • {crust.Text} crust\n{string.Join(", ", toppings.CheckedItems.Cast<Topping>())}\n{drink.Text}\n\n{Output.Text}", "Your pizza"); }),
            ("Clear toppings", () => { for (int i = 0; i < toppings.Items.Count; i++) toppings.SetItemChecked(i, false); ResetOutput(); }), ("Clear all", Reset));
        foreach (var c in new[] { size, crust, drink, dining }) c.SelectedIndexChanged += (_, _) => ResetOutput();
        toppings.ItemCheck += (_, _) => ResetOutput(); Reset();
    }
    private void Compute()
    {
        Require.That(dining.SelectedIndex >= 0, "Select dine in or take out.");
        var t = new PizzaService().Calculate(Order()); Output.Text = $"Pizza: {Money.Format(t.Pizza)}\nToppings: {Money.Format(t.Toppings)}\nDrink: {Money.Format(t.Drinks)}\nTOTAL: {Money.Format(t.Total)}";
    }
    private void Reset() { size.SelectedIndex = crust.SelectedIndex = drink.SelectedIndex = dining.SelectedIndex = 0; for (int i = 0; i < toppings.Items.Count; i++) toppings.SetItemChecked(i, false); ResetOutput(); }
}

public sealed class LottoForm : ExerciseForm
{
    private readonly NumericUpDown bet = Theme.Number(0, 1_000_000, 10, 2);
    private readonly NumericUpDown[] digits = [Theme.Number(0, 9), Theme.Number(0, 9, 1), Theme.Number(0, 9, 2)];
    public LottoForm() : base("3 Digits Lotto • classroom simulation")
    {
        Info("Fictional classroom exercise. No real money or prizes.\nChoose three different digits. Ordered match: PHP 1 million. Any-order match: bet × 1,000.");
        Field("Demo bet", bet); for (int i = 0; i < 3; i++) Field($"Digit {i+1}", digits[i]);
        Actions(("Draw", () =>
        {
            var service = new LottoService(); var ticket = digits.Select(d => (int)d.Value).ToArray(); service.ValidateTicket(bet.Value, ticket);
            var draw = service.Draw(); var result = service.Evaluate(bet.Value, ticket, draw);
            Output.Text = $"Winning numbers: {string.Join(" • ", draw)}\n{result.Outcome}\nFictional prize: {Money.Format(result.Prize)}";
        }), ("Clear", () => { bet.Value = 10; for (int i = 0; i < 3; i++) digits[i].Value = i; ResetOutput(); }));
    }
}

public sealed class TuitionForm : ExerciseForm
{
    private readonly ComboBox course = Theme.Choice<Course>(), year = Theme.Choice<YearLevel>();
    private readonly NumericUpDown units = Theme.Number(0, 30), labs = Theme.Number(0, 3);
    public TuitionForm() : base("Tuition Fee Computation")
    {
        Field("Course", course); Field("Year level", year); Field("Total units (3–30)", units); Field("Lab units (0–3)", labs);
        Info("Tuition PHP 100/unit • Lab PHP 1,000/unit.\nMiscellaneous BSIT/BSCS: 1,000 / 750 / 500 / 250.\nBSA/BSN: 2,000 / 1,500 / 1,000 / 500 (Freshman → Senior).");
        Actions(("Calculate", () => { var t = new TuitionService().Calculate(Theme.Selected<Course>(course), Theme.Selected<YearLevel>(year), (int)units.Value, (int)labs.Value); Output.Text = $"Tuition: {Money.Format(t.Tuition)}\nLab: {Money.Format(t.Lab)}\nMiscellaneous: {Money.Format(t.Miscellaneous)}\nTOTAL: {Money.Format(t.Total)}"; }),
            ("Clear", () => { course.SelectedIndex = year.SelectedIndex = -1; units.Value = labs.Value = 0; ResetOutput(); }));
        course.SelectedIndexChanged += (_, _) => ResetOutput(); year.SelectedIndexChanged += (_, _) => ResetOutput(); units.ValueChanged += (_, _) => ResetOutput(); labs.ValueChanged += (_, _) => ResetOutput();
    }
}

public sealed class PayrollForm : ExerciseForm
{
    private readonly ComboBox employee = Theme.Choice(["EMP001 — Alex Santos", "EMP002 — Sam Reyes", "EMP003 — Jamie Cruz"]), position = Theme.Choice<Position>(), status = Theme.Choice<CivilStatus>();
    private readonly TextBox name = new() { ReadOnly = true, Width = 245 };
    private readonly NumericUpDown days = Theme.Number(0, 31);
    public PayrollForm() : base("Employee Payroll Computation")
    {
        Info("Sample employee records for classroom use.");
        Field("Employee number", employee); Field("Employee name", name); Field("Position code", position); Field("Civil status", status); Field("Days worked", days);
        employee.SelectedIndexChanged += (_, _) => { name.Text = employee.SelectedItem is string s ? s.Split(" — ")[1] : ""; ResetOutput(); };
        position.SelectedIndexChanged += (_, _) => ResetOutput(); status.SelectedIndexChanged += (_, _) => ResetOutput(); days.ValueChanged += (_, _) => ResetOutput();
        Info("A: PHP 500/day • B: PHP 400/day • C: PHP 300/day.\nTax: Single 10%, Married/Widowed 5%. SSS: 1%, 3%, 5%, 7% at 0 / 1,000 / 5,000 / 10,000.");
        Actions(("Compute", () => { Require.Text(name.Text, "employee"); var t = new PayrollService().Calculate(Theme.Selected<Position>(position), Theme.Selected<CivilStatus>(status), (int)days.Value); Output.Text = $"Rate/day: {Money.Format(t.Rate)}\nBasic pay: {Money.Format(t.Basic)}\nSSS: {Money.Format(t.Sss)}\nWithholding tax: {Money.Format(t.Tax)}\nNET PAY: {Money.Format(t.Net)}"; }),
            ("Clear", () => { employee.SelectedIndex = position.SelectedIndex = status.SelectedIndex = -1; name.Clear(); days.Value = 0; ResetOutput(); }));
    }
}

public sealed class HotelForm : ExerciseForm
{
    private readonly TextBox customer = new() { Width = 245, MaxLength = 120 };
    private readonly DateTimePicker checkIn = new() { Format = DateTimePickerFormat.Short, Width = 245 }, checkOut = new() { Format = DateTimePickerFormat.Short, Width = 245 };
    private readonly ComboBox capacity = Theme.Choice<Capacity>(), room = Theme.Choice<RoomType>(), payment = Theme.Choice<HotelPayment>();
    public HotelForm() : base("Hotel Billing System")
    {
        Field("Customer", customer); Field("Check-in", checkIn); Field("Check-out", checkOut); Field("Room capacity", capacity); Field("Room type", room); Field("Payment", payment);
        Info("Regular / DeLuxe / Suite rates per day:\nSingle 100 / 300 / 500 • Double 200 / 500 / 800 • Family 500 / 750 / 1,000.\nCash 0% • Check +5% • Credit card +10%. Same-day stays cost one day.");
        Actions(("Compute", () => { var t = new HotelService().Calculate(customer.Text, checkIn.Value, checkOut.Value, Theme.Selected<Capacity>(capacity), Theme.Selected<RoomType>(room), Theme.Selected<HotelPayment>(payment)); Output.Text = $"{customer.Text.Trim()} • {t.Days} day(s)\nRate/day: {Money.Format(t.Rate)}\nRoom charge: {Money.Format(t.BaseAmount)}\nSurcharge: {Money.Format(t.Surcharge)}\nTOTAL: {Money.Format(t.Total)}"; }),
            ("Clear", () => { customer.Clear(); checkIn.Value = checkOut.Value = DateTime.Today; capacity.SelectedIndex = room.SelectedIndex = payment.SelectedIndex = -1; ResetOutput(); }));
        customer.TextChanged += (_, _) => ResetOutput(); checkIn.ValueChanged += (_, _) => ResetOutput(); checkOut.ValueChanged += (_, _) => ResetOutput(); foreach (var c in new[] { capacity, room, payment }) c.SelectedIndexChanged += (_, _) => ResetOutput();
    }
}
