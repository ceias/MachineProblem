using System.Media;
using DreamOS.WinForms.Controls;
using DreamOS.WinForms.Services;

namespace DreamOS.WinForms.Forms;
public sealed class WidgetsForm : RetroForm
{
    private readonly TextBox notes = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true, MaxLength = 100_000, AccessibleName = "Saved notes" };
    private readonly Label clock = Theme.Label(""), track = Theme.Label("Choose a local WAV file. No media is supplied."), reminderStatus = Theme.Label("No reminder scheduled.");
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 1000 };
    private readonly SoundPlayer player = new();
    private DateTime? remindAt;
    private bool dirty;
    public WidgetsForm(Action<string> notify) : base("Desktop widgets • your little corner")
    {
        Size = new(800, 690);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var calendarTab = new TabPage("Clock + calendar") { BackColor = Theme.Cream };
        var calendarBody = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new(20) };
        clock.Font = new("Consolas", 20, FontStyle.Bold); calendarBody.Controls.Add(clock); calendarBody.Controls.Add(new MonthCalendar { MaxSelectionCount = 1, AccessibleName = "Calendar" });
        calendarBody.Controls.Add(Theme.Label("Reminder message")); var message = new TextBox { Width = 400, Text = "Take a little dream break!", MaxLength = 240, AccessibleName = "Reminder message" }; calendarBody.Controls.Add(message);
        calendarBody.Controls.Add(Theme.Label("Remind me in minutes (while Dream OS is open)")); var delay = Theme.Number(1, 1440, 5); delay.AccessibleName = "Reminder delay in minutes"; calendarBody.Controls.Add(delay);
        calendarBody.Controls.Add(Theme.Button("Set reminder", () => { remindAt = DateTime.Now.AddMinutes((double)delay.Value); reminderStatus.Text = $"Reminder scheduled for {remindAt:t}."; }));
        calendarBody.Controls.Add(Theme.Button("Cancel reminder", () => { remindAt = null; reminderStatus.Text = "Reminder cancelled."; })); calendarBody.Controls.Add(reminderStatus); calendarTab.Controls.Add(calendarBody);
        var notesTab = new TabPage("Notes") { BackColor = Theme.Cream }; var noteButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55 };
        noteButtons.Controls.Add(Theme.Button("Save notes", SaveNotes)); notesTab.Controls.Add(notes); notesTab.Controls.Add(noteButtons);
        Theme.Guard(this, () => notes.Text = LocalData.Store("notes", () => new NoteState()).Load().Text);
        notes.TextChanged += (_, _) => dirty = true;
        var mediaTab = new TabPage("Music") { BackColor = Theme.Cream }; var media = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new(20), WrapContents = false, AutoScroll = true };
        media.Controls.Add(Theme.Label("LOCAL MUSIC PLAYER\nPlay your own permitted WAV audio. No bundled music.")); media.Controls.Add(track);
        media.Controls.Add(Theme.Button("Choose WAV…", () => Theme.Guard(this, () =>
        {
            using var dialog = new OpenFileDialog { Filter = "WAV audio (*.wav)|*.wav", CheckFileExists = true };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            player.Stop(); player.SoundLocation = dialog.FileName; player.Load(); track.Text = Path.GetFileName(dialog.FileName);
        })));
        media.Controls.Add(Theme.Button("Play", () => Theme.Guard(this, () => { if (string.IsNullOrEmpty(player.SoundLocation)) throw new ArgumentException("Choose a WAV file first."); player.Play(); })));
        media.Controls.Add(Theme.Button("Loop", () => Theme.Guard(this, () => { if (string.IsNullOrEmpty(player.SoundLocation)) throw new ArgumentException("Choose a WAV file first."); player.PlayLooping(); })));
        media.Controls.Add(Theme.Button("Stop", player.Stop)); mediaTab.Controls.Add(media);
        tabs.TabPages.AddRange([calendarTab, notesTab, mediaTab]); Controls.Add(tabs);
        timer.Tick += (_, _) =>
        {
            clock.Text = DateTime.Now.ToString("HH:mm:ss\ndddd, dd MMM yyyy");
            if (remindAt is DateTime due && DateTime.Now >= due) { remindAt = null; reminderStatus.Text = "Reminder delivered."; notify(message.Text); }
        }; timer.Start();
        FormClosing += (_, e) =>
        {
            if (!dirty) return;
            var answer = MessageBox.Show(this, "Save your notes before closing?", "Notes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel) e.Cancel = true;
            if (answer == DialogResult.Yes && !SaveNotesCore()) e.Cancel = true;
        };
    }
    private bool SaveNotesCore()
    {
        bool saved = false; Theme.Guard(this, () => { LocalData.Store("notes", () => new NoteState()).Save(new() { Text = notes.Text }); dirty = false; saved = true; }); return saved;
    }
    private void SaveNotes() { if (SaveNotesCore()) MessageBox.Show(this, "Notes saved on this computer.", "Dream OS"); }
    protected override void Dispose(bool disposing) { if (disposing) { timer.Dispose(); player.Stop(); player.Dispose(); } base.Dispose(disposing); }
}
