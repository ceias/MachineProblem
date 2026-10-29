using DreamOS.Core;
namespace DreamOS.WinForms.Services;
public static class LocalData
{
    public static string DirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DreamOS");
    public static JsonStorage<T> Store<T>(string name, Func<T> create) where T : class => new(Path.Combine(DirectoryPath, name + ".json"), create);
}
public sealed class NoteState { public string Text { get; set; } = ""; }
