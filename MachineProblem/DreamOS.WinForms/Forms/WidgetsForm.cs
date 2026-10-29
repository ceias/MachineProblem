using DreamOS.WinForms.Controls;
using DreamOS.WinForms.Services;
namespace DreamOS.WinForms.Forms;

public sealed class WidgetsForm : WorkflowForm
{
    private readonly TextBox notes=new() { Multiline=true,Dock=DockStyle.Fill,ScrollBars=ScrollBars.Vertical,AcceptsReturn=true,MaxLength=100_000,AccessibleName="Saved notes",Font=new("Consolas",11),BackColor=Color.FromArgb(255,253,242) };
    private readonly Label clock=WorkflowUi.Text("",21,true),track=WorkflowUi.Text("Choose a local audio file.",11),reminderStatus=WorkflowUi.Text("No reminder scheduled.",10);
    private readonly TabControl tabs=new() { Dock=DockStyle.Fill,Padding=new(18,8) };
    private readonly System.Windows.Forms.Timer timer=new() { Interval=1000 }; private readonly WindowsAudioPlayer player=new();
    private readonly Label playback=WorkflowUi.Text("No audio loaded.",9);
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
        var notesTab=new TabPage("Notes") { BackColor=Theme.Cream }; var paper=new Panel { Dock=DockStyle.Fill,Padding=new(14) }; paper.Controls.Add(notes); notesTab.Controls.Add(WorkflowUi.Section("Your notepad",paper));
        Theme.Guard(this,()=>notes.Text=LocalData.Store("notes",()=>new NoteState()).Load().Text);
        notes.TextChanged+=(_,_)=> { dirty=true; Status("Notes changed. Save before closing to keep them on your desktop."); };
        var musicTab=new TabPage("Music") { BackColor=Theme.Cream }; var media=WorkflowUi.Stack();
        media.Controls.Add(WorkflowUi.Text("Your little listening corner",17,true)); media.Controls.Add(WorkflowUi.Text("Play, pause, loop, or stop local audio you have permission to use.",10)); media.Controls.Add(track); media.Controls.Add(playback);
        var transport=new FlowLayoutPanel { AutoSize=true,WrapContents=false };
        var play=Theme.Button("Play",()=>Run(()=> { player.Play(); Status("Playing "+track.Text); }));
        var pause=Theme.Button("Pause",()=>Run(()=> { player.Pause(); Status("Playback paused."); }));
        var repeat=Theme.Button("Loop",()=>Run(()=> { player.Play(!player.IsLooping); Status(player.IsLooping?"Looping "+track.Text:"Loop disabled."); }));
        var stop=Theme.Button("Stop",()=>Run(()=> { player.Stop(); Status("Playback stopped."); }));
        Button[] controls=[play,pause,repeat,stop]; foreach(var control in controls) { control.Enabled=false; transport.Controls.Add(control); control.Click+=(_,_)=>UpdateTransport(); }
        void UpdateTransport()
        {
            play.Enabled=player.HasTrack&&player.State!="Playing"; pause.Enabled=player.HasTrack&&player.State=="Playing";
            repeat.Enabled=player.HasTrack; stop.Enabled=player.HasTrack&&player.State!="Stopped";
            repeat.Text=player.IsLooping?"Loop on":"Loop"; repeat.BackColor=player.IsLooping?Theme.Blue:Theme.Pink;
        }
        media.Controls.Add(Theme.Button("Choose audio…",()=>Run(()=>
        {
            using var dialog=new OpenFileDialog { Filter="Audio files|*.mp3;*.wav;*.m4a;*.aac;*.wma;*.aif;*.aiff|All files (*.*)|*.*",CheckFileExists=true };
            if(dialog.ShowDialog(this)!=DialogResult.OK)return;
            try { player.Load(dialog.FileName); }
            catch(InvalidOperationException) { Status("The selected file was not loaded. Choose another recording or a converted copy.",true); throw; }
            track.Text=player.FileName??"Audio loaded"; UpdateTransport();
            Status("Audio loaded. Press Play to listen.");
        })));
        media.Controls.Add(transport);
        media.Controls.Add(WorkflowUi.Text("MP3, WAV and other supported Windows audio formats.\nChanging a filename extension does not convert the recording.",9)); musicTab.Controls.Add(WorkflowUi.Section("Your listening corner",media));
        tabs.TabPages.AddRange([calendarTab,notesTab,musicTab]); Workspace.Controls.Add(tabs); SelectPage(initialPage);
        var saveNotes=Command("Save notes",()=> { if(SaveNotes())Status("Notes saved. The desktop preview is updated."); }); Command("Close",Close);
        saveNotes.Visible=tabs.SelectedTab?.Text=="Notes"; tabs.SelectedIndexChanged+=(_,_)=>saveNotes.Visible=tabs.SelectedTab?.Text=="Notes";
        clock.Text=DateTime.Now.ToString("HH:mm:ss\ndd MMM yyyy"); timer.Tick+=(_,_)=> { clock.Text=DateTime.Now.ToString("HH:mm:ss\ndd MMM yyyy"); UpdateTransport(); playback.Text="Audio: "+player.State+(player.IsLooping?" · looping":""); if(player.TakePlaybackError() is string error) { Status("Playback failed. Choose another recording or a converted copy.",true); DreamDialog.Show(this,error,"Audio playback",MessageBoxButtons.OK,MessageBoxIcon.Warning); } if(remindAt is DateTime due&&DateTime.Now>=due) { remindAt=null; reminderStatus.Text="Reminder delivered."; notify(message.Text); } }; timer.Start();
        FormClosing+=(_,e)=> { if(!dirty)return; var answer=DreamDialog.Show(this,"Save your notes before closing?","Notes",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question); if(answer==DialogResult.Cancel)e.Cancel=true; if(answer==DialogResult.Yes&&!SaveNotes())e.Cancel=true; };
    }
    public void SelectPage(string name) { foreach(TabPage page in tabs.TabPages)if(page.Text==name) { tabs.SelectedTab=page; return; } }
    private bool SaveNotes()
    {
        bool saved=false; Theme.Guard(this,()=> { LocalData.Store("notes",()=>new NoteState()).Save(new() { Text=notes.Text }); dirty=false; saved=true; NotesSaved?.Invoke(this,EventArgs.Empty); }); return saved;
    }
    protected override void Dispose(bool disposing) { if(disposing) { timer.Dispose(); player.Dispose(); } base.Dispose(disposing); }
}
