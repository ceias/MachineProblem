using System.Media;
using DreamOS.WinForms.Controls;
using DreamOS.WinForms.Services;
namespace DreamOS.WinForms.Forms;

public sealed class WidgetsForm : WorkflowForm
{
    private readonly TextBox notes=new() { Multiline=true,Dock=DockStyle.Fill,ScrollBars=ScrollBars.Vertical,AcceptsReturn=true,MaxLength=100_000,AccessibleName="Saved notes",Font=new("Consolas",11),BackColor=Color.FromArgb(255,253,242) };
    private readonly Label clock=WorkflowUi.Text("",21,true),track=WorkflowUi.Text("Choose your own WAV file.",11),reminderStatus=WorkflowUi.Text("No reminder scheduled.",10);
    private readonly TabControl tabs=new() { Dock=DockStyle.Fill,Padding=new(18,8) };
    private readonly System.Windows.Forms.Timer timer=new() { Interval=1000 }; private readonly SoundPlayer player=new();
    private DateTime? remindAt; private bool dirty;
    public event EventHandler? NotesSaved;
    public WidgetsForm(Action<string> notify,string initialPage="Clock + calendar") : base("Your Desktop Widgets","Calendar, notes, reminders, and your own music—all in one little corner.")
    {
        var calendarTab=new TabPage("Clock + calendar") { BackColor=Theme.Cream };
        var calendarGrid=WorkflowUi.Grid(); var calendarBody=WorkflowUi.Stack(); calendarBody.Controls.Add(clock); calendarBody.Controls.Add(new MonthCalendar { MaxSelectionCount=1,AccessibleName="Calendar" });
        var reminder=WorkflowUi.Stack(); var message=new TextBox { Width=245,Text="Take a little dream break!",MaxLength=240 }; var delay=Theme.Number(1,1440,5);
        WorkflowUi.Field(reminder,"Reminder message",message); WorkflowUi.Field(reminder,"Minutes from now",delay);
        reminder.Controls.Add(Theme.Button("Set reminder",()=> { remindAt=DateTime.Now.AddMinutes((double)delay.Value); reminderStatus.Text=$"Scheduled for {remindAt:t}.\nKeep this window open."; }));
        reminder.Controls.Add(Theme.Button("Cancel reminder",()=> { remindAt=null; reminderStatus.Text="Reminder cancelled."; })); reminder.Controls.Add(reminderStatus);
        calendarGrid.Controls.Add(WorkflowUi.Section("Your day",calendarBody),0,0); calendarGrid.Controls.Add(WorkflowUi.Section("A little nudge",reminder),1,0); calendarTab.Controls.Add(calendarGrid);
        var notesTab=new TabPage("Notes") { BackColor=Theme.Cream }; var paper=new Panel { Dock=DockStyle.Fill,Padding=new(14) }; paper.Controls.Add(notes); notesTab.Controls.Add(paper);
        Theme.Guard(this,()=>notes.Text=LocalData.Store("notes",()=>new NoteState()).Load().Text);
        notes.TextChanged+=(_,_)=> { dirty=true; Status("Notes changed. Save before closing to keep them on your desktop."); };
        var musicTab=new TabPage("Music") { BackColor=Theme.Cream }; var media=WorkflowUi.Stack();
        media.Controls.Add(WorkflowUi.Text("Your little listening corner",17,true)); media.Controls.Add(WorkflowUi.Text("Play, loop, or stop a local WAV file you have permission to use.",10)); media.Controls.Add(track);
        media.Controls.Add(Theme.Button("Choose WAV…",()=>Run(()=> { using var dialog=new OpenFileDialog { Filter="WAV audio (*.wav)|*.wav",CheckFileExists=true }; if(dialog.ShowDialog(this)!=DialogResult.OK)return; player.Stop(); player.SoundLocation=dialog.FileName; player.Load(); track.Text=Path.GetFileName(dialog.FileName); })));
        var transport=new FlowLayoutPanel { AutoSize=true,WrapContents=false };
        transport.Controls.Add(Theme.Button("Play",()=>Run(()=> { RequireTrack(); player.Play(); Status("Playing "+track.Text); })));
        transport.Controls.Add(Theme.Button("Loop",()=>Run(()=> { RequireTrack(); player.PlayLooping(); Status("Looping "+track.Text); })));
        transport.Controls.Add(Theme.Button("Stop",()=> { player.Stop(); Status("Playback stopped."); })); media.Controls.Add(transport); musicTab.Controls.Add(media);
        tabs.TabPages.AddRange([calendarTab,notesTab,musicTab]); Workspace.Controls.Add(tabs); SelectPage(initialPage);
        Command("Save notes",()=> { if(SaveNotes())Status("Notes saved. The desktop preview is updated."); }); Command("Close",Close);
        clock.Text=DateTime.Now.ToString("HH:mm:ss\ndd MMM yyyy"); timer.Tick+=(_,_)=> { clock.Text=DateTime.Now.ToString("HH:mm:ss\ndd MMM yyyy"); if(remindAt is DateTime due&&DateTime.Now>=due) { remindAt=null; reminderStatus.Text="Reminder delivered."; notify(message.Text); } }; timer.Start();
        FormClosing+=(_,e)=> { if(!dirty)return; var answer=MessageBox.Show(this,"Save your notes before closing?","Notes",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question); if(answer==DialogResult.Cancel)e.Cancel=true; if(answer==DialogResult.Yes&&!SaveNotes())e.Cancel=true; };
    }
    public void SelectPage(string name) { foreach(TabPage page in tabs.TabPages)if(page.Text==name) { tabs.SelectedTab=page; return; } }
    private void RequireTrack() { if(string.IsNullOrEmpty(player.SoundLocation))throw new ArgumentException("Choose a WAV file first."); }
    private bool SaveNotes()
    {
        bool saved=false; Theme.Guard(this,()=> { LocalData.Store("notes",()=>new NoteState()).Save(new() { Text=notes.Text }); dirty=false; saved=true; NotesSaved?.Invoke(this,EventArgs.Empty); }); return saved;
    }
    protected override void Dispose(bool disposing) { if(disposing) { timer.Dispose(); player.Stop(); player.Dispose(); } base.Dispose(disposing); }
}
