namespace DreamOS.Core;

public sealed record Offer(string Serial,string Name,decimal Percent,decimal Credit);
public sealed record CheckoutTotal(decimal BaseTotal,decimal Savings,decimal Delivery,decimal Total);
public static class Checkout
{
    public static IReadOnlyList<Offer> DemoOffers { get; }=Array.AsReadOnly(new Offer[] {
        new("DREAM-VOUCHER-10","10% voucher",.10m,0),new("DREAM-PROMO-05","5% promo",.05m,0),new("DREAM-GIFT-500","PHP 500 gift check",0,500) });
    public static CheckoutTotal Calculate(decimal baseTotal,bool delivery,string serial)
    {
        Require.That(baseTotal>0,"Add an appliance first.");
        var code=serial.Trim(); var offer=DemoOffers.FirstOrDefault(o=>o.Serial.Equals(code,StringComparison.OrdinalIgnoreCase));
        Require.That(code.Length==0||offer is not null,"Unknown serial number. Use a listed classroom demo code or leave blank.");
        decimal savings=offer is null?0:Math.Min(baseTotal,Money.Round(baseTotal*offer.Percent+offer.Credit));
        decimal fee=delivery?150:0;
        return new(baseTotal,savings,fee,Money.Round(baseTotal-savings+fee));
    }
}
public sealed record ReminderItem(Guid Id,DateTime Due,string Message);
public sealed class ReminderState { public List<ReminderItem> Items { get; set; }=[]; }
public sealed class ReminderService(IStorage<ReminderState> storage)
{
    public IReadOnlyList<ReminderItem> Pending=>storage.Load().Items.AsReadOnly();
    public void Schedule(DateTime due,string message)
    {
        Require.Text(message,"reminder message");
        var state=storage.Load(); state.Items.Add(new(Guid.NewGuid(),due,message.Trim())); storage.Save(state);
    }
    public void CancelAll()=>storage.Save(new());
    public IReadOnlyList<ReminderItem> TakeDue(DateTime now)
    {
        var state=storage.Load(); var due=state.Items.Where(i=>i.Due<=now).ToArray();
        if(due.Length>0) { state.Items.RemoveAll(i=>i.Due<=now); storage.Save(state); }
        return due;
    }
}
