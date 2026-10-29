using DreamOS.WinForms.Controls;
namespace DreamOS.WinForms.Forms;

public sealed class DesktopForm : RetroForm
{
    private sealed record AppInfo(string Id, string Title, string Icon, string Help, Func<Form> Create);
    private readonly Dictionary<string, Form> windows = [];
    private readonly Dictionary<string, Button> tasks = [];
    private readonly FlowLayoutPanel taskbar = new() { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
    private readonly GuideControl guide = new() { Dock = DockStyle.Bottom, Height = 175 };
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 1000 };
    private readonly Label clock = new() { Dock = DockStyle.Right, Width = 125, TextAlign = ContentAlignment.MiddleCenter };
    private readonly ContextMenuStrip startMenu = new();
    private readonly List<string> notifications = [];
    private readonly AppInfo[] apps;
    public bool LogoutRequested { get; private set; }
    public DesktopForm(string username) : base("Dream OS • " + username)
    {
        IsMdiContainer = true; WindowState = FormWindowState.Maximized; MinimumSize = new(1000, 700);
        apps = [
            new("pizza", "Pizza Builder", "🍕", "Build a pizza: choose size, crust, toppings and a drink, then compute your bill.", () => new PizzaForm()),
            new("lotto", "3 Digits Lotto", "★", "A fictional classroom draw. Pick three unique digits and a demo bet; no real money.", () => new LottoForm()),
            new("tuition", "Tuition Fees", "✎", "Select course and year, then compute tuition, lab and miscellaneous fees.", () => new TuitionForm()),
            new("payroll", "Employee Payroll", "▤", "Choose a demo employee to calculate basic pay, SSS, withholding tax and net pay.", () => new PayrollForm()),
            new("hotel", "Hotel Billing", "⌂", "Choose stay dates, room and payment to calculate the hotel bill.", () => new HotelForm()),
            new("election", "Elections 2010", "✓", "A classroom election: validate a demo ID, vote once, view tally and winner.", () => new ElectionForm()),
            new("password", "Password Entry", "⚿", "Practice the password flow into the appliance store. Demo password: dream10.", () => new PasswordForm(name => OpenStore(name))),
            new("store", "Appliance Store", "▣", "Add appliances to the cart. Cash gives 5% off; charge adds 5%. Compute cash change.", () => new StoreForm(username)),
            new("about", "About + Author", "♥", "Meet Dream OS, a native C# OOP classroom project.", () => new AboutForm()),
            new("widgets", "Desktop Widgets", "♫", "Your clock, calendar, saved notes, reminder notifications and local WAV player.", () => new WidgetsForm(Notify))
        ];
        var header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Theme.Pink };
        header.Controls.Add(new Label { Text = "♥ DREAM OS", Font = new("Consolas", 26, FontStyle.Bold), ForeColor = Theme.Accent, Dock = DockStyle.Left, Width = 320, TextAlign = ContentAlignment.MiddleLeft, Padding = new(15, 0, 0, 0) });
        header.Controls.Add(new Label { Text = $"Welcome, {username}  •  Small programs. Big dreams.\nC# / .NET 10 • classroom desktop", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new(10), AutoEllipsis = true });
        var shelf = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = Theme.Cream }; shelf.Controls.Add(guide);
        var shortcuts = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new(8) }; shelf.Controls.Add(shortcuts); shortcuts.BringToFront();
        foreach (var app in apps)
        {
            var shortcut = Theme.Button($"{app.Icon}  {app.Title}", () => Open(app)); shortcut.Width = 195; shortcut.Height = 45; shortcut.TextAlign = ContentAlignment.MiddleLeft;
            shortcut.AccessibleDescription = app.Help; guide.Register(shortcut, app.Help); shortcuts.Controls.Add(shortcut);
            var item = startMenu.Items.Add(app.Title, null, (_, _) => Open(app)); item.MouseEnter += (_, _) => guide.Say(app.Help);
        }
        startMenu.Items.Add(new ToolStripSeparator());
        startMenu.Items.Add("Cascade windows", null, (_, _) => LayoutMdi(MdiLayout.Cascade));
        startMenu.Items.Add("Tile windows", null, (_, _) => LayoutMdi(MdiLayout.TileVertical));
        startMenu.Items.Add("Notifications", null, (_, _) => ShowNotifications());
        startMenu.Items.Add("Log out", null, (_, _) => Logout());
        startMenu.Items.Add("Exit Dream OS", null, (_, _) => Close());
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Theme.Pink, Padding = new(3) };
        var start = Theme.Button("♥ Start", () => startMenu.Show(this, new Point(5, ClientSize.Height - bottom.Height - startMenu.PreferredSize.Height))); start.Dock = DockStyle.Left; start.Width = 105;
        bottom.Controls.Add(taskbar); bottom.Controls.Add(clock); bottom.Controls.Add(start);
        Controls.Add(shelf); Controls.Add(bottom); Controls.Add(header);
        foreach (MdiClient client in Controls.OfType<MdiClient>()) client.BackColor = Color.FromArgb(237, 213, 235);
        timer.Tick += (_, _) => clock.Text = DateTime.Now.ToString("HH:mm:ss\ndd MMM yyyy"); timer.Start();
        MdiChildActivate += (_, _) => { foreach (var (id, button) in tasks) button.BackColor = windows[id] == ActiveMdiChild ? Theme.Blue : Theme.Cream; };
        Shown += (_, _) => guide.Say("Welcome to Dream OS! Open an app from the left or Start. Native window buttons minimize, maximize and close; taskbar buttons restore apps.");
        FormClosing += (_, e) =>
        {
            // WinForms raises child FormClosing events first, propagating cancellation here.
            if (e.Cancel) LogoutRequested = false;
        };
    }
    private void Open(AppInfo app) => Theme.Guard(this, () =>
    {
        if (windows.TryGetValue(app.Id, out var running)) { Restore(running); return; }
        Form window = app.Create(); window.MdiParent = this; window.StartPosition = FormStartPosition.Manual; window.Location = new(22 + windows.Count % 5 * 20, 22 + windows.Count % 5 * 20);
        window.Size = new(Math.Min(window.Width, Math.Max(550, ClientSize.Width - 300)), Math.Min(window.Height, Math.Max(420, ClientSize.Height - 180)));
        windows.Add(app.Id, window);
        var task = Theme.Button(app.Title, () => Restore(window)); task.Height = 38; task.BackColor = Theme.Cream; tasks.Add(app.Id, task); taskbar.Controls.Add(task);
        guide.Register(task, app.Help); RegisterHelp(window, app.Help);
        window.FormClosed += (_, _) => { windows.Remove(app.Id); tasks.Remove(app.Id); taskbar.Controls.Remove(task); task.Dispose(); };
        window.Show(); guide.Say(app.Help);
    });
    private void RegisterHelp(Control parent, string help)
    {
        guide.Register(parent, help);
        foreach (Control child in parent.Controls) { string detail = string.IsNullOrWhiteSpace(child.AccessibleName) ? help : $"{child.AccessibleName}. {help}"; RegisterHelp(child, detail); }
    }
    private static void Restore(Form window) { if (window.WindowState == FormWindowState.Minimized) window.WindowState = FormWindowState.Normal; window.Show(); window.BringToFront(); window.Activate(); }
    private void OpenStore(string username)
    {
        var app = apps.First(a => a.Id == "store"); Open(app with { Create = () => new StoreForm(username) });
    }
    private void Notify(string text) { notifications.Add($"{DateTime.Now:t} • {text}"); if (notifications.Count > 100) notifications.RemoveAt(0); guide.Say(text); MessageBox.Show(this, text, "Dream OS • reminder", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    private void ShowNotifications() => MessageBox.Show(this, notifications.Count == 0 ? "No notifications yet. Schedule one in Desktop Widgets." : string.Join(Environment.NewLine, notifications), "Notifications");
    private void Logout() { LogoutRequested = true; Close(); }
    protected override void Dispose(bool disposing) { if (disposing) { timer.Dispose(); startMenu.Dispose(); } base.Dispose(disposing); }
}
