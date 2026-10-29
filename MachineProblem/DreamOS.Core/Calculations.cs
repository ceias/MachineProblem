using System.Globalization;
using System.Security.Cryptography;

namespace DreamOS.Core;

public static class Money
{
    public static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    public static string Format(decimal value) => value.ToString("C2", CultureInfo.GetCultureInfo("en-PH"));
}

public static class Require
{
    public static string Text(string value, string label) => !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new ArgumentException($"Enter {label}.");
    public static void That(bool condition, string message) { if (!condition) throw new ArgumentException(message); }
}

public enum PizzaSize { Small, Medium, Large }
public enum Crust { Thin, Thick }
public enum Drink { None, SoftDrink, FruitJuice, CoffeeChocolate }
public enum Topping { ExtraCheese, Mushrooms, BlackOlives, Onions, GreenPeppers, Tomatoes }
public sealed record PizzaOrder(PizzaSize Size, Crust Crust, IReadOnlyCollection<Topping> Toppings, Drink Drink, bool Takeout);
public sealed record PizzaTotal(decimal Pizza, decimal Toppings, decimal Drinks) { public decimal Total => Pizza + Toppings + Drinks; }
public sealed class PizzaService
{
    public PizzaTotal Calculate(PizzaOrder order)
    {
        decimal size = order.Size switch { PizzaSize.Small => 100, PizzaSize.Medium => 150, PizzaSize.Large => 200, _ => throw new ArgumentException("Select a pizza size.") };
        Require.That(Enum.IsDefined(order.Crust) && Enum.IsDefined(order.Drink) && order.Toppings.All(Enum.IsDefined), "Invalid pizza option.");
        return new(size * (order.Crust == Crust.Thick ? 1.5m : 1m), order.Toppings.Distinct().Count() * 10m,
            order.Drink switch { Drink.SoftDrink => 20, Drink.FruitJuice => 15, Drink.CoffeeChocolate => 25, _ => 0 });
    }
}

public enum LottoOutcome { NoMatch, Consolation, Jackpot }
public sealed record LottoResult(LottoOutcome Outcome, decimal Prize);
public sealed class LottoService
{
    private static void Validate(IReadOnlyList<int> digits) => Require.That(digits.Count == 3 && digits.All(d => d is >= 0 and <= 9) && digits.Distinct().Count() == 3, "Enter three different digits from 0 to 9.");
    public void ValidateTicket(decimal bet, IReadOnlyList<int> ticket) { Require.That(bet >= 10, "Enter a minimum demo bet of PHP 10."); Validate(ticket); }
    public int[] Draw()
    {
        var digits = Enumerable.Range(0, 10).ToArray();
        for (int i = 0; i < 3; i++) { int j = RandomNumberGenerator.GetInt32(i, 10); (digits[i], digits[j]) = (digits[j], digits[i]); }
        return digits[..3];
    }
    public LottoResult Evaluate(decimal bet, IReadOnlyList<int> ticket, IReadOnlyList<int> draw)
    {
        ValidateTicket(bet, ticket); Validate(draw);
        if (ticket.SequenceEqual(draw)) return new(LottoOutcome.Jackpot, 1_000_000);
        if (ticket.Order().SequenceEqual(draw.Order())) return new(LottoOutcome.Consolation, Money.Round(bet * 1000));
        return new(LottoOutcome.NoMatch, 0);
    }
}

public enum Course { BSIT, BSCS, BSA, BSN }
public enum YearLevel { Freshman, Sophomore, Junior, Senior }
public sealed record TuitionTotal(decimal Tuition, decimal Lab, decimal Miscellaneous) { public decimal Total => Tuition + Lab + Miscellaneous; }
public sealed class TuitionService
{
    public TuitionTotal Calculate(Course course, YearLevel year, int units, int labUnits)
    {
        Require.That(Enum.IsDefined(course) && Enum.IsDefined(year), "Select a course and year level.");
        Require.That(units is >= 3 and <= 30 && labUnits is >= 0 and <= 3, "Total units must be 3–30; lab units must be 0–3.");
        decimal misc = new decimal[] { 1000, 750, 500, 250 }[(int)year] * (course is Course.BSA or Course.BSN ? 2 : 1);
        return new(units * 100m, labUnits * 1000m, misc);
    }
}

public enum Position { A, B, C }
public enum CivilStatus { Single, Married, Widowed }
public sealed record PayrollTotal(decimal Rate, decimal Basic, decimal Sss, decimal Tax) { public decimal Net => Basic - Sss - Tax; }
public sealed class PayrollService
{
    public PayrollTotal Calculate(Position position, CivilStatus status, int days)
    {
        Require.That(days is >= 1 and <= 31, "Days worked must be 1–31.");
        Require.That(Enum.IsDefined(status), "Select civil status.");
        decimal rate = position switch { Position.A => 500, Position.B => 400, Position.C => 300, _ => throw new ArgumentException("Select a position.") };
        decimal basic = rate * days;
        // Continuous half-open brackets avoid the reference's implicit gap above 9,999.
        decimal sss = basic >= 10000 ? .07m : basic >= 5000 ? .05m : basic >= 1000 ? .03m : .01m;
        return new(rate, basic, Money.Round(basic * sss), Money.Round(basic * (status == CivilStatus.Single ? .10m : .05m)));
    }
}

public enum Capacity { Single, Double, Family }
public enum RoomType { Regular, DeLuxe, Suite }
public enum HotelPayment { Cash, Check, CreditCard }
public sealed record HotelTotal(int Days, decimal Rate, decimal BaseAmount, decimal Surcharge) { public decimal Total => BaseAmount + Surcharge; }
public sealed class HotelService
{
    private static readonly decimal[,] Rates = { { 100, 300, 500 }, { 200, 500, 800 }, { 500, 750, 1000 } };
    public HotelTotal Calculate(string customer, DateTime checkIn, DateTime checkOut, Capacity capacity, RoomType room, HotelPayment payment)
    {
        Require.Text(customer, "customer name");
        Require.That(Enum.IsDefined(capacity) && Enum.IsDefined(room) && Enum.IsDefined(payment), "Select room capacity, room type and payment.");
        Require.That(checkOut.Date >= checkIn.Date, "Check-out cannot precede check-in.");
        int days = Math.Max(1, (checkOut.Date - checkIn.Date).Days);
        decimal rate = Rates[(int)capacity, (int)room], basic = days * rate;
        return new(days, rate, basic, Money.Round(basic * (payment switch { HotelPayment.Check => .05m, HotelPayment.CreditCard => .10m, _ => 0 })));
    }
}

public sealed record Product(string Name, decimal Price) { public override string ToString() => $"{Name} — {Money.Format(Price)}"; }
public static class Catalog
{
    public static IReadOnlyList<Product> Products { get; } = Array.AsReadOnly(new Product[] {
        new("Electric Fan", 500), new("Refrigerator", 9000), new("Washing Machine", 4000), new("Oven Toaster", 550),
        new("Flat Iron", 350), new("VCD Player", 1200), new("Mini Component", 3500), new("Television", 5000),
        new("Air Conditioner", 10000), new("Rice Cooker", 600), new("Electric Stove", 1000), new("Electric Pot", 800),
        new("Karaoke", 2500), new("Microwave Oven", 3000) });
}
public sealed record CartItem(Product Product, int Quantity) { public decimal Amount => Product.Price * Quantity; }
public interface IPaymentRule { string Name { get; } decimal Calculate(decimal subtotal); }
public sealed class CashPaymentRule : IPaymentRule { public string Name => "Cash (5% discount)"; public decimal Calculate(decimal subtotal) => Money.Round(subtotal * .95m); }
public sealed class ChargePaymentRule : IPaymentRule { public string Name => "Charge (5% surcharge)"; public decimal Calculate(decimal subtotal) => Money.Round(subtotal * 1.05m); }
public sealed class ShoppingCart
{
    private readonly List<CartItem> items = [];
    public IReadOnlyList<CartItem> Items => items.AsReadOnly();
    public decimal Subtotal => items.Sum(i => i.Amount);
    public void Add(Product product, int quantity)
    {
        Require.That(Catalog.Products.Contains(product) && quantity is >= 1 and <= 999, "Select a catalog item and quantity from 1–999.");
        items.Add(new(product, quantity));
    }
    public void RemoveAt(int index) { Require.That(index >= 0 && index < items.Count, "Select a cart row to remove."); items.RemoveAt(index); }
    public void Clear() => items.Clear();
    public decimal Total(IPaymentRule payment) { Require.That(items.Count > 0, "Add an appliance first."); return payment.Calculate(Subtotal); }
    public decimal Change(IPaymentRule payment, decimal tendered)
    {
        Require.That(payment is CashPaymentRule, "Change applies to cash payments only.");
        decimal total = Total(payment); Require.That(tendered >= total, "Amount tendered must cover the total."); return Money.Round(tendered - total);
    }
}

public sealed class DemoAuthService
{
    public const string Password = "dream10";
    public bool Authenticate(string username, string password) => !string.IsNullOrWhiteSpace(username) && password == Password;
}
