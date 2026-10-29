using DreamOS.WinForms.Controls;
namespace DreamOS.WinForms.Forms;

public sealed class DesktopForm : RetroForm
{
    private sealed record AppInfo(string Id,string Title,string Help,Func<Form> Create);
    private readonly Dictionary<string,Form> windows=[];
    private readonly Dictionary<string,Button> tasks=[];
    private readonly WallpaperPanel workspace=new() { Dock=DockStyle.Fill };
    private readonly FlowLayoutPanel taskbar=new() { Dock=DockStyle.Fill, WrapContents=false, AutoScroll=true, BackColor=Theme.Pink };
    private readonly GuideControl guide=new() { Anchor=AnchorStyles.Right|AnchorStyles.Bottom };
    private readonly System.Windows.Forms.Timer timer=new() { Interval=1000 };
    private readonly Label clock=new() { Dock=DockStyle.Right, Width=115, TextAlign=ContentAlignment.MiddleCenter, BorderStyle=BorderStyle.Fixed3D, BackColor=Theme.Cream };
    private readonly ContextMenuStrip startMenu=new() { BackColor=Theme.Cream, Font=new("Tahoma",10), ShowImageMargin=true };
    private readonly List<string> notifications=[];
    private readonly AppInfo[] apps;
    public bool LogoutRequested { get; private set; }
    public DesktopForm(string username) : base("Dream OS • "+username)
    {
        MinimumSize=new(1000,700); MaximizedBounds=Screen.PrimaryScreen?.WorkingArea??Rectangle.Empty; WindowState=FormWindowState.Maximized;
        apps=[
            new("pizza","Pizza Builder","Build a pizza: choose size, crust, toppings and a drink, then compute your bill.",()=>new PizzaForm()),
            new("lotto","3 Digits Lotto","A fictional classroom draw. Pick three unique digits and a demo bet; no real money.",()=>new LottoForm()),
            new("tuition","Tuition Fees","Select course and year, then compute tuition, lab and miscellaneous fees.",()=>new TuitionForm()),
            new("payroll","Employee Payroll","Choose a demo employee to calculate basic pay, SSS, withholding tax and net pay.",()=>new PayrollForm()),
            new("hotel","Hotel Billing","Choose stay dates, room and payment to calculate the hotel bill.",()=>new HotelForm()),
            new("election","Elections 2010","Validate a classroom voter ID, vote once, view the tally and winner.",()=>new ElectionForm()),
            new("password","Password Entry","Practice the password flow into the appliance store. Demo password: dream10.",()=>new PasswordForm(OpenStore)),
            new("store","Appliance Store","Add appliances. Cash gives 5% off; charge adds 5%. Compute cash change.",()=>new StoreForm(username)),
            new("about","About + Author","Meet Dream OS, a native C# OOP classroom project.",()=>new AboutForm()),
            new("widgets","Desktop Widgets","Your clock, calendar, saved notes, reminders and local WAV player.",()=>new WidgetsForm(Notify))
        ];
        var shortcuts=new FlowLayoutPanel { Location=new(14,14), Width=212, Height=540, FlowDirection=FlowDirection.TopDown, WrapContents=true, BackColor=Color.Transparent, Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left };
        foreach(var app in apps)
        {
            var shortcut=new RetroButton { DesktopShortcut=true, Text=app.Title, Size=new(96,94), Margin=new(3,4,3,4), BackColor=Color.Transparent, Image=Artwork.Icon(app.Id,44), AccessibleDescription=app.Help, AccessibleName=app.Title, Font=new("Tahoma",9) };
            shortcut.Click+=(_,_)=>Open(app); guide.Register(shortcut,app.Help); shortcuts.Controls.Add(shortcut);
            var menuImage=Artwork.Icon(app.Id,24); var item=startMenu.Items.Add(app.Title,menuImage,(_,_)=>Open(app)); item.MouseEnter+=(_,_)=>guide.Say(app.Help);
        }
        startMenu.Items.Add(new ToolStripSeparator());
        startMenu.Items.Add("Cascade windows",null,(_,_)=>ArrangeWindows(false)); startMenu.Items.Add("Tile windows",null,(_,_)=>ArrangeWindows(true));
        startMenu.Items.Add("Notifications",null,(_,_)=>ShowNotifications()); startMenu.Items.Add("Log out",null,(_,_)=>Logout()); startMenu.Items.Add("Exit Dream OS",null,(_,_)=>Close());
        var bottom=new Panel { Dock=DockStyle.Bottom, Height=49, BackColor=Theme.Pink, Padding=new(4) };
        var start=Theme.Button("♥ Start",()=>startMenu.Show(bottom,new Point(0,0),ToolStripDropDownDirection.AboveRight)); start.Dock=DockStyle.Left; start.Width=100; start.AutoSize=false; start.Font=new("Tahoma",11,FontStyle.Bold);
        bottom.Controls.Add(taskbar); bottom.Controls.Add(clock); bottom.Controls.Add(start);
        var motto=new Label { Text="Small\nPrograms\n\nBig\nDreams", AutoSize=false, Size=new(200,190), ForeColor=Color.White, BackColor=Color.Transparent, TextAlign=ContentAlignment.MiddleCenter, Font=new("Consolas",21,FontStyle.Bold), Anchor=AnchorStyles.Top|AnchorStyles.Right };
        workspace.Controls.Add(shortcuts); workspace.Controls.Add(motto); workspace.Controls.Add(guide); Controls.Add(workspace); Controls.Add(bottom);
        workspace.Resize+=(_,_)=> { guide.Location=new(Math.Max(0,workspace.Width-guide.Width-20),Math.Max(0,workspace.Height-guide.Height-15)); motto.Location=new(workspace.Width-motto.Width-35,35); shortcuts.Height=Math.Max(250,workspace.Height-28); };
        clock.Text=DateTime.Now.ToString("HH:mm\ndd MMM yyyy"); timer.Tick+=(_,_)=>clock.Text=DateTime.Now.ToString("HH:mm\ndd MMM yyyy"); timer.Start();
        Shown+=(_,_)=>Open(new("welcome","Welcome to Dream OS","Let's make something lovely. Choose an app from your desktop or Start.",()=>new WelcomeForm(()=>Open(apps.First(a=>a.Id=="widgets")))));
        FormClosing+=(_,e)=>
        {
            foreach(var window in windows.Values.ToArray())
            {
                window.Close();
                if(!window.IsDisposed) { e.Cancel=true; LogoutRequested=false; break; }
            }
        };
    }
    private void Open(AppInfo app)=>Theme.Guard(this,()=>
    {
        if(windows.TryGetValue(app.Id,out var running)) { Restore(running); return; }
        var window=app.Create(); window.TopLevel=false; window.ShowInTaskbar=false;
        workspace.Controls.Add(window); window.StartPosition=FormStartPosition.Manual;
        window.Size=new(Math.Min(window.Width,Math.Max(window.MinimumSize.Width,workspace.Width-250)),Math.Min(window.Height,Math.Max(window.MinimumSize.Height,workspace.Height-40)));
        window.Location=new(Math.Max(10,(workspace.Width-window.Width)/2)+windows.Count%4*16,Math.Max(10,(workspace.Height-window.Height)/2)+windows.Count%4*12);
        windows.Add(app.Id,window);
        var task=Theme.Button(app.Title,()=>Restore(window)); task.Height=34; task.Image=Artwork.Icon(app.Id,24); task.BackColor=Theme.Cream; tasks.Add(app.Id,task); taskbar.Controls.Add(task);
        guide.Register(task,app.Help); RegisterHelp(window,app.Help);
        window.Enter+=(_,_)=>Highlight(app.Id);
        window.VisibleChanged+=(_,_)=> { if(!window.Visible)task.BackColor=Theme.Cream; };
        window.FormClosed+=(_,_)=> { windows.Remove(app.Id); tasks.Remove(app.Id); taskbar.Controls.Remove(task); task.Dispose(); workspace.Controls.Remove(window); window.Dispose(); };
        window.Show(); window.BringToFront(); window.Focus(); Highlight(app.Id); guide.Say(app.Help);
    });
    private void Highlight(string id) { foreach(var (key,button) in tasks)button.BackColor=key==id?Theme.Blue:Theme.Cream; }
    private void RegisterHelp(Control parent,string help)
    {
        guide.Register(parent,help); foreach(Control child in parent.Controls)RegisterHelp(child,string.IsNullOrWhiteSpace(child.AccessibleName)?help:$"{child.AccessibleName}. {help}");
    }
    private static void Restore(Form window) { window.Show(); window.BringToFront(); window.Focus(); }
    private void ArrangeWindows(bool tile)
    {
        var open=windows.Values.Where(w=>w.Visible).ToArray();
        for(int i=0;i<open.Length;i++)
        {
            if(open[i] is RetroForm { IsWorkspaceMaximized:true } r)r.ToggleMaximize();
            if(tile)
            {
                int columns=Math.Max(1,(int)Math.Ceiling(Math.Sqrt(open.Length))), rows=(int)Math.Ceiling((double)open.Length/columns);
                int width=workspace.Width/columns,height=workspace.Height/rows;
                open[i].Bounds=new(i%columns*width,i/columns*height,Math.Max(open[i].MinimumSize.Width,width),Math.Max(open[i].MinimumSize.Height,height));
            }
            else open[i].Location=new(160+i%7*26,20+i%7*26);
            open[i].BringToFront();
        }
    }
    private void OpenStore(string username)=>Open(apps.First(a=>a.Id=="store") with { Create=()=>new StoreForm(username) });
    private void Notify(string text) { notifications.Add($"{DateTime.Now:t} • {text}"); if(notifications.Count>100)notifications.RemoveAt(0); guide.Say(text); MessageBox.Show(this,text,"Dream OS • reminder",MessageBoxButtons.OK,MessageBoxIcon.Information); }
    private void ShowNotifications()=>MessageBox.Show(this,notifications.Count==0?"No notifications yet. Schedule one in Desktop Widgets.":string.Join(Environment.NewLine,notifications),"Notifications");
    private void Logout() { LogoutRequested=true; Close(); }
    protected override void Dispose(bool disposing)
    {
        if(disposing) { timer.Dispose(); foreach(ToolStripItem item in startMenu.Items)item.Image?.Dispose(); startMenu.Dispose(); }
        base.Dispose(disposing);
    }
}

internal sealed class WelcomeForm : RetroForm
{
    public WelcomeForm(Action widgets) : base("Welcome to Dream OS")
    {
        MinimumSize=new(430,340); Size=new(470,370); MaximizeBox=false;
        var content=new TableLayoutPanel { Dock=DockStyle.Fill, Padding=new(24), ColumnCount=2, RowCount=3, BackColor=Theme.Cream };
        content.ColumnStyles.Add(new(SizeType.Absolute,115)); content.ColumnStyles.Add(new(SizeType.Percent,100));
        var computer=new PictureBox { Dock=DockStyle.Fill, Image=Artwork.Icon("computer",90), SizeMode=PictureBoxSizeMode.CenterImage }; computer.Disposed+=(_,_)=>computer.Image?.Dispose();
        content.Controls.Add(computer,0,0); content.SetRowSpan(computer,2);
        content.Controls.Add(new Label { Text="Welcome to\nDream OS", AutoSize=true, Font=new("Consolas",24,FontStyle.Bold), ForeColor=Theme.Ink, Margin=new(10,10,0,15) },1,0);
        content.Controls.Add(new Label { Text="A cute retro environment\nfor your C# adventures!", AutoSize=true, Margin=new(10,5,0,20) },1,1);
        var buttons=new FlowLayoutPanel { Dock=DockStyle.Fill, AutoSize=true }; buttons.Controls.Add(Theme.Button("♥ Let's start!",Close)); buttons.Controls.Add(Theme.Button("My widgets",()=> { widgets(); Close(); })); content.Controls.Add(buttons,0,2); content.SetColumnSpan(buttons,2); Controls.Add(content);
    }
}
