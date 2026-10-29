using System.Text.Json;

namespace DreamOS.Core;

public interface IStorage<T> where T : class { T Load(); void Save(T value); }
public sealed class JsonStorage<T>(string path, Func<T> createDefault) : IStorage<T> where T : class
{
    public T Load()
    {
        if (!File.Exists(path)) return createDefault();
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? throw new InvalidDataException("Saved data is empty. Restore a backup or rename the saved file.");
    }
    public void Save(T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllText(temporary, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })); File.Move(temporary, path, true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
public sealed class ElectionState { public Dictionary<string, string> Votes { get; set; } = []; }
public sealed class ElectionService
{
    public static IReadOnlyList<string> Candidates { get; } = Array.AsReadOnly(new[] { "Noli", "Val", "Lester", "Gwapo" });
    public static IReadOnlyDictionary<string, string> DemoVoters { get; } = new Dictionary<string, string> { ["12345"] = "Demo Voter", ["10001"] = "Alex", ["10002"] = "Sam", ["10003"] = "Jamie" }.AsReadOnly();
    private readonly IStorage<ElectionState> storage;
    private ElectionState state;
    public ElectionService(IStorage<ElectionState> storage)
    {
        this.storage = storage; state = storage.Load();
        if (state.Votes is null || state.Votes.Any(v => !DemoVoters.ContainsKey(v.Key) || !Candidates.Contains(v.Value))) throw new InvalidDataException("Saved election data contains invalid entries. Restore a valid backup.");
    }
    public string ValidateVoter(string id)
    {
        Require.That(DemoVoters.ContainsKey(id), "Unknown demo voter ID. Use 12345 or 10001–10003.");
        Require.That(!state.Votes.ContainsKey(id), "This voter has already voted.");
        return DemoVoters[id];
    }
    public void Vote(string id, string candidate)
    {
        ValidateVoter(id); Require.That(Candidates.Contains(candidate), "Select a candidate.");
        var next = new ElectionState { Votes = new(state.Votes) { [id] = candidate } };
        storage.Save(next); state = next;
    }
    public IReadOnlyDictionary<string, int> Tally() => Candidates.ToDictionary(c => c, c => state.Votes.Count(v => v.Value == c));
    public string Result()
    {
        var tally = Tally(); int max = tally.Values.Max();
        if (max == 0) return "No votes have been cast.";
        var winners = tally.Where(v => v.Value == max).Select(v => v.Key).ToArray();
        return (winners.Length > 1 ? "Tie: " : "Winner: ") + string.Join(", ", winners) + $" ({max} votes)";
    }
    public void Reset() { var empty = new ElectionState(); storage.Save(empty); state = empty; }
}
