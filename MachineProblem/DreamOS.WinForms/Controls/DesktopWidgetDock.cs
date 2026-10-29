using DreamOS.WinForms.Services;
namespace DreamOS.WinForms.Controls;

public sealed class DesktopWidgetDock : UserControl
{
    private readonly Label preview=new() { Dock=DockStyle.Fill,Text="A little space for your notes.",Font=new("Tahoma",9),ForeColor=Theme.Ink,Padding=new(6),AutoEllipsis=true };
    public event Action<string>? PageRequested;
    public DesktopWidgetDock()
    {
        Size=new(230,110); BackColor=Color.FromArgb(255,247,249); Padding=new(5); BorderStyle=BorderStyle.FixedSingle;
        var actions=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=34,WrapContents=false };
        foreach(var (name,page) in new[]{("Notes","Notes"),("Music","Music"),("Remind","Clock + calendar")})
        {
            string selected=page; var button=Theme.Button(name,()=>PageRequested?.Invoke(selected)); button.AutoSize=false; button.MinimumSize=Size.Empty; button.Size=new(68,28); button.Margin=new(1,2,1,0); button.Padding=Padding.Empty; button.Font=new("Tahoma",8.5f); actions.Controls.Add(button);
        }
        Controls.Add(preview); Controls.Add(actions); ReloadNotes();
    }
    public void ReloadNotes()
    {
        try { string text=LocalData.Store("notes",()=>new NoteState()).Load().Text; preview.Text=string.IsNullOrWhiteSpace(text)?"YOUR LITTLE NOTEPAD\nTap Notes to write something lovely.":"SAVED NOTE\n"+(text.Length>200?text[..200]+"…":text); }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException) { preview.Text="Notes unavailable. Open Notes to review the saved file."; }
    }
}
