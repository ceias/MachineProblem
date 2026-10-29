using DreamOS.Core;
using System.Text.Json;

// Dependency-free executable regression suite: dotnet run --project DreamOS.Tests -c Release.
int passed = 0, failed = 0;
void Check(string name, Action test)
{
    try { test(); passed++; Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failed++; Console.Error.WriteLine($"FAIL {name}: {ex.Message}"); }
}
void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}"); }
void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } throw new Exception("Expected validation error."); }

var pizza = new PizzaService();
Check("Pizza screenshot: small, six toppings, soft drink = 180", () => Equal(180m, pizza.Calculate(new(PizzaSize.Small, Crust.Thin, Enum.GetValues<Topping>(), Drink.SoftDrink, false)).Total));
Check("Pizza thick large + fruit juice + two toppings", () => Equal(335m, pizza.Calculate(new(PizzaSize.Large, Crust.Thick, [Topping.ExtraCheese, Topping.Onions], Drink.FruitJuice, true)).Total));
Check("Pizza medium + coffee", () => Equal(175m, pizza.Calculate(new(PizzaSize.Medium, Crust.Thin, [], Drink.CoffeeChocolate, false)).Total));
Check("Pizza duplicate toppings charged once", () => Equal(110m, pizza.Calculate(new(PizzaSize.Small, Crust.Thin, [Topping.Onions, Topping.Onions], Drink.None, false)).Total));
Check("Pizza invalid size", () => Reject(() => pizza.Calculate(new((PizzaSize)99, Crust.Thin, [], Drink.None, false))));
Check("Pizza invalid crust", () => Reject(() => pizza.Calculate(new(PizzaSize.Small, (Crust)99, [], Drink.None, false))));

var lotto = new LottoService();
Check("Lotto exact ordered jackpot", () => Equal(new LottoResult(LottoOutcome.Jackpot, 1_000_000), lotto.Evaluate(100, [5,3,8], [5,3,8])));
Check("Lotto screenshot consolation", () => Equal(new LottoResult(LottoOutcome.Consolation, 100_000), lotto.Evaluate(100, [3,8,5], [5,3,8])));
Check("Lotto no match", () => Equal(LottoOutcome.NoMatch, lotto.Evaluate(10, [1,2,3], [4,5,6]).Outcome));
Check("Lotto minimum bet", () => Reject(() => lotto.ValidateTicket(9.99m, [1,2,3])));
Check("Lotto repeated digits", () => Reject(() => lotto.ValidateTicket(10, [1,1,3])));
Check("Lotto digit range", () => Reject(() => lotto.ValidateTicket(10, [-1,1,3])));
Check("Lotto ticket length", () => Reject(() => lotto.ValidateTicket(10, [1,2])));
Check("Lotto leading zero", () => Equal(LottoOutcome.Jackpot, lotto.Evaluate(10, [0,1,2], [0,1,2]).Outcome));
Check("Lotto draw invariants (500 draws)", () => { for (int i=0;i<500;i++) { var d=lotto.Draw(); Equal(3,d.Length); Equal(3,d.Distinct().Count()); if(d.Any(x=>x<0||x>9)) throw new Exception("Invalid digit"); } });

var tuition = new TuitionService();
Check("Tuition screenshot BSIT junior, 21/2", () => Equal(4600m, tuition.Calculate(Course.BSIT, YearLevel.Junior,21,2).Total));
foreach(var c in Enum.GetValues<Course>()) foreach(var y in Enum.GetValues<YearLevel>())
    Check($"Tuition miscellaneous {c}/{y}", () => Equal(new decimal[]{1000,750,500,250}[(int)y]*(c is Course.BSA or Course.BSN?2:1), tuition.Calculate(c,y,3,0).Miscellaneous));
Check("Tuition units below minimum", () => Reject(() => tuition.Calculate(Course.BSA,YearLevel.Senior,0,0)));
Check("Tuition units above maximum", () => Reject(() => tuition.Calculate(Course.BSA,YearLevel.Senior,31,0)));
Check("Tuition lab maximum", () => Reject(() => tuition.Calculate(Course.BSA,YearLevel.Senior,30,4)));
Check("Tuition invalid course", () => Reject(() => tuition.Calculate((Course)99,YearLevel.Senior,30,0)));

var payroll = new PayrollService();
Check("Payroll A 20 days single", () => Equal(new PayrollTotal(500,10000,700,1000),payroll.Calculate(Position.A,CivilStatus.Single,20)));
Check("Payroll lower SSS bracket", () => Equal(3m,payroll.Calculate(Position.C,CivilStatus.Married,1).Sss));
Check("Payroll 1,000 SSS boundary", () => Equal(30m,payroll.Calculate(Position.A,CivilStatus.Widowed,2).Sss));
Check("Payroll below 5,000 SSS boundary", () => Equal(144m,payroll.Calculate(Position.B,CivilStatus.Married,12).Sss));
Check("Payroll 5,000 SSS boundary", () => Equal(250m,payroll.Calculate(Position.A,CivilStatus.Married,10).Sss));
Check("Payroll below 10,000 SSS boundary", () => Equal(480m,payroll.Calculate(Position.B,CivilStatus.Single,24).Sss));
Check("Payroll net and married tax", () => Equal(4500m,payroll.Calculate(Position.A,CivilStatus.Married,10).Net));
Check("Payroll zero days", () => Reject(()=>payroll.Calculate(Position.A,CivilStatus.Single,0)));
Check("Payroll invalid position", () => Reject(()=>payroll.Calculate((Position)99,CivilStatus.Single,1)));
Check("Payroll invalid status", () => Reject(()=>payroll.Calculate(Position.A,(CivilStatus)99,1)));

var hotel = new HotelService(); var date = new DateTime(2026,1,1);
Check("Hotel screenshot family deluxe, two days",()=>Equal(1500m,hotel.Calculate("Demo",date,date.AddDays(2),Capacity.Family,RoomType.DeLuxe,HotelPayment.Cash).Total));
Check("Hotel same day billed one day",()=>Equal(1,hotel.Calculate("Demo",date,date,Capacity.Single,RoomType.Regular,HotelPayment.Cash).Days));
Check("Hotel check surcharge",()=>Equal(105m,hotel.Calculate("Demo",date,date,Capacity.Single,RoomType.Regular,HotelPayment.Check).Total));
Check("Hotel credit surcharge",()=>Equal(1100m,hotel.Calculate("Demo",date,date,Capacity.Family,RoomType.Suite,HotelPayment.CreditCard).Total));
var rates = new decimal[,]{{100,300,500},{200,500,800},{500,750,1000}};
foreach(var c in Enum.GetValues<Capacity>()) foreach(var r in Enum.GetValues<RoomType>())
    Check($"Hotel matrix {c}/{r}",()=>Equal(rates[(int)c,(int)r],hotel.Calculate("Demo",date,date,c,r,HotelPayment.Cash).Rate));
Check("Hotel backwards dates",()=>Reject(()=>hotel.Calculate("Demo",date,date.AddDays(-1),Capacity.Single,RoomType.Regular,HotelPayment.Cash)));
Check("Hotel missing customer",()=>Reject(()=>hotel.Calculate(" ",date,date,Capacity.Single,RoomType.Regular,HotelPayment.Cash)));
Check("Hotel invalid room",()=>Reject(()=>hotel.Calculate("Demo",date,date,Capacity.Single,(RoomType)99,HotelPayment.Cash)));

var cash = new CashPaymentRule(); var charge = new ChargePaymentRule();
ShoppingCart ExampleCart() { var c=new ShoppingCart(); c.Add(Catalog.Products.Single(p=>p.Name=="Air Conditioner"),1); c.Add(Catalog.Products.Single(p=>p.Name=="Electric Fan"),2); return c; }
Check("Store cash screenshot total + change",()=>{var c=ExampleCart();Equal(11000m,c.Subtotal);Equal(10450m,c.Total(cash));Equal(50m,c.Change(cash,10500));});
Check("Store charge screenshot",()=>{var c=new ShoppingCart();c.Add(Catalog.Products.Single(p=>p.Name=="Electric Fan"),5);c.Add(Catalog.Products.Single(p=>p.Name=="Air Conditioner"),2);Equal(23625m,c.Total(charge));});
Check("Store quantity zero",()=>Reject(()=>new ShoppingCart().Add(Catalog.Products[0],0)));
Check("Store invalid product",()=>Reject(()=>new ShoppingCart().Add(new("Fake",1),1)));
Check("Store empty cart",()=>Reject(()=>new ShoppingCart().Total(cash)));
Check("Store insufficient cash",()=>Reject(()=>ExampleCart().Change(cash,1)));
Check("Store exact cash",()=>Equal(0m,ExampleCart().Change(cash,10450)));
Check("Store charge has no cash change",()=>Reject(()=>ExampleCart().Change(charge,20000)));
Check("Store remove row and clear",()=>{var c=ExampleCart();c.RemoveAt(0);Equal(1000m,c.Subtotal);c.Clear();Equal(0,c.Items.Count);});
Check("Store invalid row",()=>Reject(()=>ExampleCart().RemoveAt(-1)));
Check("Money midpoint rounding",()=>Equal(1.01m,Money.Round(1.005m)));

var auth = new DemoAuthService();
Check("Demo auth success",()=>Equal(true,auth.Authenticate("Dreamer","dream10")));
Check("Demo auth missing name",()=>Equal(false,auth.Authenticate(" ","dream10")));
Check("Demo auth wrong password",()=>Equal(false,auth.Authenticate("Dreamer","wrong")));

Check("Receipt export preserves computed payroll and deductions",()=>
{
    var pay=payroll.Calculate(Position.A,CivilStatus.Single,20);
    var receipt=new ReceiptData("PAYSLIP","PAY-TEST",new DateTime(2026,10,29,12,30,0),[new("Gross",Money.Format(pay.Basic)),new("SSS",Money.Format(pay.Sss)),new("Tax",Money.Format(pay.Tax))],"NET PAY",Money.Format(pay.Net),"Classroom copy");
    string text=receipt.ToText();
    foreach(string required in new[]{"PAYSLIP","PAY-TEST",Money.Format(10000),Money.Format(700),Money.Format(1000),Money.Format(8300),"2026-10-29 12:30","Classroom copy"})
        if(!text.Contains(required))throw new Exception("Missing receipt data: "+required);
});
Check("Receipt export preserves all cart rows",()=>
{
    var cart=ExampleCart();
    var receipt=new ReceiptData("SALE","SALE-TEST",DateTime.Now,cart.Items.Select(i=>new ReceiptEntry(i.Product.Name+" x "+i.Quantity,Money.Format(i.Amount))).ToArray(),"TOTAL",Money.Format(cart.Total(cash)),"Demo");
    string text=receipt.ToText();
    if(!text.Contains("Air Conditioner x 1")||!text.Contains("Electric Fan x 2")||!text.Contains(Money.Format(10450)))throw new Exception("Incomplete sale receipt");
});
Check("Receipt references are unique and filename safe",()=>
{
    var references=Enumerable.Range(0,100).Select(_=>ReceiptData.NewReference("PAY")).ToArray();
    Equal(100,references.Distinct().Count());
    if(references.Any(r=>r.Any(c=>!(char.IsAsciiLetterOrDigit(c)||c=='-'))))throw new Exception("Unsafe reference");
});

Check("Guide activities have distinct motion styles",()=>
{
    var styles=GuideAnimation.Activities.Select(id=>GuideAnimation.Sample(id,.8).Effect).ToArray(); Equal(styles.Length,styles.Distinct().Count());
});
Check("Guide motion stays bounded for every activity",()=>
{
    foreach(var id in GuideAnimation.Activities)for(int i=0;i<=300;i++)
    {
        var frame=GuideAnimation.Sample(id,i/100d);
        if(!double.IsFinite(frame.X+frame.Y+frame.Rotation+frame.PropY)||Math.Abs(frame.X)>8.01||Math.Abs(frame.Y)>13.01||Math.Abs(frame.Rotation)>8.01||Math.Abs(frame.PropY)>24.01)throw new Exception("Unsafe guide bounds: "+id);
    }
});
Check("Guide paused and completed states stop all movement",()=>
{
    foreach(var id in GuideAnimation.Activities)
    {
        Equal(new GuideFrame(0,0,0,0,"idle"),GuideAnimation.Sample(id,1,true));
        Equal(new GuideFrame(0,0,0,0,"idle"),GuideAnimation.Sample(id,3));
    }
});
Check("Receipt pagination retains every visual row and terminates",()=>
{
    foreach(int total in new[]{0,1,12,40,200})
    {
        int first=0,pages=0;
        while(true)
        {
            var page=ReceiptPagination.Plan(total-first,700,218,20,206,44); first+=page.Count;
            if(++pages>201)throw new Exception("Pagination failed to terminate");
            if(page.Final)break;
            if(page.Count<=0)throw new Exception("Empty continuation page");
        }
        Equal(total,first);
    }
});
Check("Receipt rejects printer areas too small for a readable page",()=>Reject(()=>ReceiptPagination.Plan(10,200,218,20,206,44)));

Check("Audio probe recognizes WAVE contents independently of the extension",()=>Equal(AudioContainer.Wave,AudioFileProbe.Identify("RIFF0000WAVEfmt "u8)));
Check("Audio probe recognizes tagged MP3",()=>Equal(AudioContainer.Mp3,AudioFileProbe.Identify("ID3"u8)));
Check("Audio probe recognizes raw MP3 frames",()=>Equal(AudioContainer.Mp3,AudioFileProbe.Identify(new byte[]{0xff,0xfb,0x90,0x64})));
Check("Audio probe recognizes MP4 and M4A containers",()=>Equal(AudioContainer.Mp4,AudioFileProbe.Identify(new byte[]{0,0,0,24,(byte)'f',(byte)'t',(byte)'y',(byte)'p'})));
Check("Audio probe recognizes AAC frames",()=>Equal(AudioContainer.Aac,AudioFileProbe.Identify(new byte[]{0xff,0xf1,0x50,0x80})));
Check("Audio probe recognizes AIFF",()=>Equal(AudioContainer.Aiff,AudioFileProbe.Identify("FORM0000AIFF"u8)));
Check("Renamed HTML does not become a WAV file",()=>Equal(AudioContainer.Unknown,AudioFileProbe.Identify("<!DOCTYPE html><html>"u8)));
Check("Audio probe identifies unsupported WebM and Ogg downloads",()=>{Equal(AudioContainer.WebM,AudioFileProbe.Identify(new byte[]{0x1a,0x45,0xdf,0xa3}));Equal(AudioContainer.Ogg,AudioFileProbe.Identify("OggS"u8));});
Check("Audio probe handles empty and truncated files",()=>{Equal(AudioContainer.Unknown,AudioFileProbe.Identify([]));Equal(AudioContainer.Unknown,AudioFileProbe.Identify("RIFF"u8));});

string temp = Path.Combine(Path.GetTempPath(),"dreamos-tests-"+Guid.NewGuid().ToString("N"));
try
{
    var store = new JsonStorage<ElectionState>(Path.Combine(temp,"votes.json"),()=>new());
    var election = new ElectionService(store);
    Check("Election initial result",()=>Equal("No votes have been cast.",election.Result()));
    Check("Election valid demo ID",()=>Equal("Demo Voter",election.ValidateVoter("12345")));
    Check("Election unknown ID",()=>Reject(()=>election.ValidateVoter("99999")));
    Check("Election invalid candidate",()=>Reject(()=>election.Vote("12345","Fake")));
    Check("Election vote and persistent reload",()=>{election.Vote("12345","Noli");Equal(1,new ElectionService(store).Tally()["Noli"]);});
    Check("Election duplicate rejected after reload",()=>Reject(()=>new ElectionService(store).Vote("12345","Val")));
    Check("Election winner",()=>Equal("Winner: Noli (1 votes)",election.Result()));
    Check("Election tie",()=>{election.Vote("10001","Val");Equal("Tie: Noli, Val (1 votes)",election.Result());});
    Check("Election reset persists",()=>{election.Reset();Equal(0,new ElectionService(store).Tally().Values.Sum());});
    Check("Election failed save does not mutate state",()=>{var e=new ElectionService(new FailingStore());try{e.Vote("12345","Noli");throw new Exception("Expected I/O failure");}catch(IOException){} Equal(0,e.Tally()["Noli"]);});
    Check("JSON notes roundtrip",()=>{var s=new JsonStorage<Dictionary<string,string>>(Path.Combine(temp,"notes.json"),()=>[]);s.Save(new(){["note"]="♥ Dream\nnext line"});Equal("♥ Dream\nnext line",s.Load()["note"]);});
    Check("JSON corruption is reported",()=>{File.WriteAllText(Path.Combine(temp,"bad.json"),"not JSON");try{new JsonStorage<ElectionState>(Path.Combine(temp,"bad.json"),()=>new()).Load();throw new Exception("Expected invalid JSON error");}catch(JsonException){} });
    Check("JSON invalid election records reported",()=>{store.Save(new(){Votes=new(){["unknown"]="Noli"}});try{new ElectionService(store);throw new Exception("Expected invalid data error");}catch(InvalidDataException){} });
}
finally { if(Directory.Exists(temp))Directory.Delete(temp,true); }
Console.WriteLine($"\n{passed} passed, {failed} failed; no skipped tests.");
return failed == 0 ? 0 : 1;

sealed class FailingStore : IStorage<ElectionState>
{
    public ElectionState Load()=>new();
    public void Save(ElectionState value)=>throw new IOException("Simulated disk failure");
}
