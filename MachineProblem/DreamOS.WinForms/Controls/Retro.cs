using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace DreamOS.WinForms.Controls;

public static class Theme
{
    public static readonly Color Pink = Color.FromArgb(247,195,214), Cream = Color.FromArgb(255,245,240), Blue = Color.FromArgb(156,201,238), Ink = Color.FromArgb(41,40,61), Accent = Color.FromArgb(168,55,99);
    public static Button Button(string text, Action action)
    {
        var b = new RetroButton { Text=text, AutoSize=true, MinimumSize=new(100,34), BackColor=Pink, ForeColor=Ink, Margin=new(5), Padding=new(10,4,10,4), UseVisualStyleBackColor=false };
        b.Click += (_,_) => action(); return b;
    }
    public static Label Label(string text) => new() { Text=text, AutoSize=true, MaximumSize=new(620,0), Margin=new(5,8,5,8), ForeColor=Ink, BackColor=Color.Transparent };
    public static ComboBox Choice(IEnumerable<string> items)
    {
        var c=new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Width=220, Margin=new(5), AccessibleName="Selection", BackColor=Color.White, FlatStyle=FlatStyle.Standard }; c.Items.AddRange(items.Cast<object>().ToArray()); return c;
    }
    public static ComboBox Choice<T>() where T:struct,Enum => Choice(Enum.GetNames<T>());
    public static T Selected<T>(ComboBox c) where T:struct,Enum => c.SelectedItem is string s && Enum.TryParse<T>(s,out var v) ? v : throw new ArgumentException($"Select {c.AccessibleName}.");
    public static NumericUpDown Number(decimal min,decimal max,decimal value=0,int decimals=0) => new() { Minimum=min, Maximum=max, Value=Math.Clamp(value,min,max), DecimalPlaces=decimals, Width=150, Margin=new(5), ThousandsSeparator=true };
    public static void Guard(IWin32Window owner,Action action)
    {
        try { action(); }
        catch(Exception ex) when(ex is ArgumentException or IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException or System.ComponentModel.Win32Exception)
        { MessageBox.Show(owner,ex.Message,"Dream OS • please check",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
    }
    public static void Bevel(Graphics g,Rectangle r,bool sunken=false)
    {
        if(r.Width<3||r.Height<3)return;
        ControlPaint.DrawBorder(g,r,sunken?Accent:Color.White,2,ButtonBorderStyle.Solid,sunken?Accent:Color.White,2,ButtonBorderStyle.Solid,sunken?Color.White:Accent,2,ButtonBorderStyle.Solid,sunken?Color.White:Accent,2,ButtonBorderStyle.Solid);
    }
}

public class RetroButton : Button
{
    private bool pressed,hovered;
    [System.ComponentModel.DefaultValue(false)]
    public bool DesktopShortcut { get; set; }
    public RetroButton() { FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0; DoubleBuffered=true; SetStyle(ControlStyles.SupportsTransparentBackColor,true); }
    protected override void OnMouseEnter(EventArgs e) { hovered=true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered=false; pressed=false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { pressed=true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed=false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics; var rect=ClientRectangle;
        if(DesktopShortcut)
        {
            if(hovered||Focused) { using var tint=new SolidBrush(Color.FromArgb(100,255,245,253)); g.FillRectangle(tint,rect); }
            int iconSize=LogicalToDeviceUnits(44), iconTop=LogicalToDeviceUnits(5), textTop=LogicalToDeviceUnits(53);
            if(Image is not null) { g.InterpolationMode=InterpolationMode.NearestNeighbor; g.DrawImage(Image,new Rectangle((Width-iconSize)/2,iconTop,iconSize,iconSize)); }
            var label=new Rectangle(3,textTop,Width-6,Math.Max(1,Height-textTop-2));
            TextRenderer.DrawText(g,Text,Font,new Rectangle(label.X+1,label.Y+1,label.Width,label.Height),Color.White,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak);
            TextRenderer.DrawText(g,Text,Font,label,Theme.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak);
        }
        else
        {
            using var fill=new LinearGradientBrush(rect,hovered?Color.White:ControlPaint.Light(BackColor),pressed?Theme.Pink:BackColor,LinearGradientMode.Vertical);
            g.FillRectangle(fill,rect); Theme.Bevel(g,rect,pressed);
            if(Image is not null)g.DrawImage(Image,new Rectangle(7,(Height-24)/2,24,24));
            var textArea=new Rectangle(Image is null?5:35,4,Width-(Image is null?10:40),Height-8);
            if(pressed)textArea.Offset(1,1);
            TextRenderer.DrawText(g,Text,Font,textArea,Enabled?ForeColor:SystemColors.GrayText,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        }
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,Rectangle.Inflate(rect,-4,-4));
    }
    protected override void Dispose(bool disposing) { if(disposing) { var image=Image; Image=null; image?.Dispose(); } base.Dispose(disposing); }
}

public sealed class RetroTitleBar : Panel
{
    private readonly Label caption=new() { Dock=DockStyle.Fill, TextAlign=ContentAlignment.MiddleLeft, BackColor=Color.Transparent, Padding=new(8,0,0,0), Font=new("Tahoma",10,FontStyle.Bold) };
    private readonly Button minimize,maximize;
    private readonly RetroForm owner;
    private Point dragStart,windowStart;
    public RetroTitleBar(RetroForm owner)
    {
        this.owner=owner; Height=32; Dock=DockStyle.Top; Padding=new(2); DoubleBuffered=true;
        var actions=new FlowLayoutPanel { Dock=DockStyle.Right, Width=91, FlowDirection=FlowDirection.LeftToRight, WrapContents=false, BackColor=Color.Transparent, Padding=new(0,1,0,0) };
        minimize=CaptionButton("_",owner.MinimizeWindow,"Minimize window"); maximize=CaptionButton("□",owner.ToggleMaximize,"Maximize or restore window");
        var close=CaptionButton("×",owner.Close,"Close window"); close.BackColor=Theme.Pink;
        actions.Controls.AddRange([minimize,maximize,close]); Controls.Add(caption); Controls.Add(actions);
        owner.TextChanged+=(_,_)=>caption.Text="♥  "+owner.Text; caption.Text="♥  "+owner.Text;
        caption.MouseDown+=DragDown; caption.MouseMove+=DragMove; caption.MouseUp+=(_,_)=>caption.Capture=false;
        caption.DoubleClick+=(_,_)=> { if(owner.MaximizeBox)owner.ToggleMaximize(); };
    }
    private static Button CaptionButton(string text,Action action,string name)
    {
        var b=Theme.Button(text,action); b.AutoSize=false; b.MinimumSize=Size.Empty; b.Size=new(27,25); b.Padding=Padding.Empty; b.Margin=new(1,0,1,0); b.AccessibleName=name; b.Font=new("Tahoma",11,FontStyle.Bold); b.BackColor=Theme.Blue; return b;
    }
    public void Sync() { minimize.Visible=owner.MinimizeBox; maximize.Visible=owner.MaximizeBox; }
    private void DragDown(object? sender,MouseEventArgs e)
    {
        if(e.Button!=MouseButtons.Left)return;
        if(owner.TopLevel) { Native.ReleaseCapture(); Native.SendMessage(owner.Handle,0xA1,(nint)2,nint.Zero); return; }
        if(owner.IsWorkspaceMaximized)return;
        dragStart=Cursor.Position; windowStart=owner.Location; caption.Capture=true; owner.BringToFront();
    }
    private void DragMove(object? sender,MouseEventArgs e)
    {
        if(!caption.Capture||owner.TopLevel||owner.Parent is null)return;
        var delta=new Size(Cursor.Position.X-dragStart.X,Cursor.Position.Y-dragStart.Y);
        owner.Location=new(Math.Clamp(windowStart.X+delta.Width,-owner.Width+100,owner.Parent.ClientSize.Width-100),Math.Clamp(windowStart.Y+delta.Height,0,Math.Max(0,owner.Parent.ClientSize.Height-32)));
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var fill=new LinearGradientBrush(ClientRectangle,Color.FromArgb(137,181,240),Color.FromArgb(194,222,249),LinearGradientMode.Horizontal); e.Graphics.FillRectangle(fill,ClientRectangle); Theme.Bevel(e.Graphics,ClientRectangle);
    }
    private static class Native
    {
        [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool ReleaseCapture();
        [DllImport("user32.dll")] internal static extern nint SendMessage(nint hWnd,int msg,nint wParam,nint lParam);
    }
}

public class RetroForm : Form
{
    private Rectangle normalBounds;
    private readonly RetroTitleBar chrome;
    private readonly Panel grip=new() { Size=new(15,15), Anchor=AnchorStyles.Right|AnchorStyles.Bottom, Cursor=Cursors.SizeNWSE };
    private Point resizeStart; private Size initialSize;
    private Control? workspaceParent;
    public bool IsWorkspaceMaximized { get; private set; }
    // The inherited WinForms designer instantiates base forms without arguments.
    public RetroForm() : this("Dream OS") { }
    public RetroForm(string title)
    {
        Text=title; Font=new("Tahoma",10); BackColor=Theme.Cream; ForeColor=Theme.Ink;
        AutoScaleDimensions=new(96,96); AutoScaleMode=AutoScaleMode.Dpi; Size=new(840,650); MinimumSize=new(580,430); StartPosition=FormStartPosition.CenterScreen;
        FormBorderStyle=FormBorderStyle.None; Padding=new(4); DoubleBuffered=true;
        chrome=new(this); Controls.Add(chrome); Controls.Add(grip);
        grip.Paint+=(_,e)=>ControlPaint.DrawSizeGrip(e.Graphics,Theme.Accent,grip.ClientRectangle);
        grip.MouseDown+=(_,e)=> { if(e.Button==MouseButtons.Left&&!IsWorkspaceMaximized) { resizeStart=Cursor.Position; initialSize=Size; grip.Capture=true; } };
        grip.MouseMove+=(_,_)=> { if(grip.Capture)Size=new(Math.Max(MinimumSize.Width,initialSize.Width+Cursor.Position.X-resizeStart.X),Math.Max(MinimumSize.Height,initialSize.Height+Cursor.Position.Y-resizeStart.Y)); };
        grip.MouseUp+=(_,_)=>grip.Capture=false;
        Resize+=(_,_)=> { grip.Location=new(ClientSize.Width-17,ClientSize.Height-17); grip.BringToFront(); };
        ParentChanged+=(_,_)=>
        {
            if(workspaceParent is not null)workspaceParent.SizeChanged-=WorkspaceResized;
            workspaceParent=Parent;
            if(workspaceParent is not null)workspaceParent.SizeChanged+=WorkspaceResized;
        };
    }
    private void WorkspaceResized(object? sender,EventArgs e)
    {
        if(Parent is null)return;
        if(IsWorkspaceMaximized)Bounds=Parent.ClientRectangle;
        else Location=new(Math.Min(Left,Math.Max(0,Parent.ClientSize.Width-100)),Math.Min(Top,Math.Max(0,Parent.ClientSize.Height-40)));
    }
    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        // Dock the caption before Fill content, regardless of which derived form adds content.
        if(chrome is not null)chrome.SendToBack();
        if(grip is not null)grip.BringToFront();
    }
    public void MinimizeWindow() { if(TopLevel)WindowState=FormWindowState.Minimized; else Hide(); }
    public void ToggleMaximize()
    {
        if(!MaximizeBox)return;
        if(TopLevel) { MaximizedBounds=Screen.FromControl(this).WorkingArea; WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized; return; }
        if(Parent is null)return;
        if(IsWorkspaceMaximized) { IsWorkspaceMaximized=false; Bounds=normalBounds; }
        else { normalBounds=Bounds; IsWorkspaceMaximized=true; Bounds=Parent.ClientRectangle; BringToFront(); }
        grip.Visible=!IsWorkspaceMaximized;
    }
    protected override void OnShown(EventArgs e) { chrome.Sync(); base.OnShown(e); }
    protected override bool ProcessCmdKey(ref Message msg,Keys keyData)
    {
        if(!TopLevel&&(keyData==(Keys.Control|Keys.W)||keyData==(Keys.Alt|Keys.F4))) { Close(); return true; }
        return base.ProcessCmdKey(ref msg,keyData);
    }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); Theme.Bevel(e.Graphics,ClientRectangle); }
    protected override void Dispose(bool disposing) { if(disposing&&workspaceParent is not null)workspaceParent.SizeChanged-=WorkspaceResized; base.Dispose(disposing); }
}

public class ExerciseForm : RetroForm
{
    protected FlowLayoutPanel Body { get; } = new() { Dock=DockStyle.Fill, FlowDirection=FlowDirection.TopDown, WrapContents=false, AutoScroll=true, Padding=new(14), BackColor=Theme.Cream };
    protected Label Output { get; } = new() { AutoSize=true, MaximumSize=new(200,0), BackColor=Color.Transparent, Padding=new(8), Margin=new(5), Text="Ready when you are.", Font=new("Tahoma",11,FontStyle.Bold) };
    private readonly FlowLayoutPanel actions=new() { Dock=DockStyle.Bottom, Height=62, Padding=new(8), BackColor=Theme.Pink, AutoScroll=true };
    public ExerciseForm() : this("Dream OS exercise") { }
    protected ExerciseForm(string title) : base(title)
    {
        var content=new Panel { Dock=DockStyle.Fill };
        var summary=new FlowLayoutPanel { Dock=DockStyle.Right, Width=230, BackColor=Color.FromArgb(255,231,239), FlowDirection=FlowDirection.TopDown, WrapContents=false, Padding=new(10), AutoScroll=true };
        string id=title.StartsWith("Special")?"pizza":title.StartsWith("3 Digits")?"lotto":title.StartsWith("Tuition")?"tuition":title.StartsWith("Employee")?"payroll":title.StartsWith("Hotel")?"hotel":title.StartsWith("Presidential")?"election":title.StartsWith("Password")?"password":title.StartsWith("Appliance")?"store":"about";
        var illustration=new PictureBox { Size=new(100,110), Image=Artwork.Icon(id,100), SizeMode=PictureBoxSizeMode.Zoom, Margin=new(45,10,0,16) };
        illustration.Disposed+=(_,_)=>illustration.Image?.Dispose(); summary.Controls.Add(illustration);
        summary.Controls.Add(new Label { Text="♥  YOUR DREAM DESK", AutoSize=true, Font=new("Tahoma",10,FontStyle.Bold), ForeColor=Theme.Accent, Margin=new(6,4,4,15) });
        summary.Controls.Add(Output); content.Controls.Add(Body); content.Controls.Add(summary); content.Controls.Add(actions); Controls.Add(content);
        Body.Controls.Add(new Label { Text=title, Font=new("Tahoma",15,FontStyle.Bold), ForeColor=Theme.Accent, AutoSize=true, MaximumSize=new(500,0), Margin=new(5,5,5,18) });
    }
    protected void Field(string label,Control control)
    {
        control.AccessibleName=label;
        var group=new FlowLayoutPanel { AutoSize=true, FlowDirection=FlowDirection.TopDown, WrapContents=false, Margin=new(0,0,0,8) };
        group.Controls.Add(new Label { Text=label, AutoSize=true, ForeColor=Theme.Ink, Margin=new(5,0,5,2) }); group.Controls.Add(control); Body.Controls.Add(group);
    }
    protected void Info(string text) { var label=Theme.Label(text); label.MaximumSize=new(470,0); Body.Controls.Add(label); }
    protected void Actions(params (string Text,Action Action)[] buttons)
    {
        foreach(var b in buttons)actions.Controls.Add(Theme.Button(b.Text,()=>Theme.Guard(this,b.Action)));
        actions.Controls.Add(Theme.Button("Close",Close));
    }
    protected void ResetOutput()=>Output.Text="Ready when you are.";
}

public sealed class PixelPortrait : Control
{
    private readonly Image? portrait=Artwork.Guide();
    public PixelPortrait() { DoubleBuffered=true; SetStyle(ControlStyles.SupportsTransparentBackColor,true); BackColor=Color.Transparent; Size=new(145,170); AccessibleName="Dream guide, girl in red with her teddy bear"; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); if(portrait is null)return;
        e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor; e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;
        // Crop unused outer margins while preserving the girl's proportions and teddy.
        var source=new Rectangle(158,80,302,481);
        float scale=Math.Min((float)Width/source.Width,(float)Height/source.Height);
        var target=new Rectangle((Width-(int)(source.Width*scale))/2,(Height-(int)(source.Height*scale))/2,(int)(source.Width*scale),(int)(source.Height*scale));
        e.Graphics.DrawImage(portrait,target,source,GraphicsUnit.Pixel);
    }
    protected override void Dispose(bool disposing) { if(disposing)portrait?.Dispose(); base.Dispose(disposing); }
}
public sealed class GuideControl : UserControl
{
    private readonly Label speech=new() { Dock=DockStyle.Top, Height=100, TextAlign=ContentAlignment.MiddleCenter, Padding=new(12), BackColor=Theme.Cream, BorderStyle=BorderStyle.FixedSingle };
    public GuideControl()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor,true); BackColor=Color.Transparent; Height=290; Width=230;
        Controls.Add(new PixelPortrait { Dock=DockStyle.Fill }); Controls.Add(speech); Say("Hover an app to learn more! Keyboard focus works too. ♥");
    }
    public void Say(string text) { speech.Text=text; speech.AccessibleName="Guide: "+text; }
    public void Register(Control control,string text) { control.MouseEnter+=(_,_)=>Say(text); control.Enter+=(_,_)=>Say(text); }
}
