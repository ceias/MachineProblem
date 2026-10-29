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
    public static ComboBox Choice<T>() where T:struct,Enum => Choice(Enum.GetNames<T>().Select(n=>System.Text.RegularExpressions.Regex.Replace(n,"([a-z])([A-Z])","$1 $2")));
    public static T Selected<T>(ComboBox c) where T:struct,Enum => c.SelectedItem is string s && Enum.TryParse<T>(s.Replace(" ",""),out var v) ? v : throw new ArgumentException($"Select {c.AccessibleName}.");
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
    public RetroButton() { FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0; DoubleBuffered=true; SetStyle(ControlStyles.SupportsTransparentBackColor,true); }
    protected override void OnMouseEnter(EventArgs e) { hovered=true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered=false; pressed=false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { pressed=true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed=false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics; var rect=ClientRectangle;
        using var fill=new LinearGradientBrush(rect,hovered?Color.White:ControlPaint.Light(BackColor),pressed?Theme.Pink:BackColor,LinearGradientMode.Vertical);
        g.FillRectangle(fill,rect); Theme.Bevel(g,rect,pressed);
        int iconSize=LogicalToDeviceUnits(22), inset=LogicalToDeviceUnits(7);
        if(Image is not null) { g.InterpolationMode=InterpolationMode.NearestNeighbor; g.DrawImage(Image,new Rectangle(inset,(Height-iconSize)/2,iconSize,iconSize)); }
        var textArea=new Rectangle(Image is null?inset:iconSize+inset*2,4,Width-(Image is null?inset*2:iconSize+inset*3),Height-8);
        if(pressed)textArea.Offset(1,1);
        TextRenderer.DrawText(g,Text,Font,textArea,Enabled?ForeColor:SystemColors.GrayText,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPadding);
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,Rectangle.Inflate(rect,-4,-4));
    }
    protected override void Dispose(bool disposing) { if(disposing) { var image=Image; Image=null; image?.Dispose(); } base.Dispose(disposing); }
}

public sealed class DesktopShortcutButton : Button
{
    private bool hovered;
    public DesktopShortcutButton()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Opaque,true);
        FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0; UseVisualStyleBackColor=false; Cursor=Cursors.Hand;
        Font=new("Tahoma",9.5f); TabStop=true;
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnMouseEnter(EventArgs e) { hovered=true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered=false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        Artwork.PaintBackdrop(this,e.Graphics);
        float scale=DeviceDpi/96f; int icon=(int)(46*scale),top=(int)(9*scale),textTop=(int)(62*scale);
        var selection=Rectangle.Inflate(ClientRectangle,-3,-2);
        if(hovered||Focused)
        {
            using var fill=new SolidBrush(Color.FromArgb(150,255,248,253)); e.Graphics.FillRectangle(fill,selection);
            using var border=new Pen(Color.FromArgb(155,168,119,166)); e.Graphics.DrawRectangle(border,selection);
        }
        if(Image is not null) { e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor; e.Graphics.PixelOffsetMode=PixelOffsetMode.Half; e.Graphics.DrawImage(Image,new Rectangle((Width-icon)/2,top,icon,icon)); }
        var label=new Rectangle(5,textTop,Width-10,Math.Max(1,Height-textTop-5));
        using var chip=new SolidBrush(Color.FromArgb(175,255,246,250)); e.Graphics.FillRectangle(chip,label);
        TextRenderer.DrawText(e.Graphics,Text,Font,Rectangle.Inflate(label,-3,-2),Theme.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,selection);
    }
    protected override void Dispose(bool disposing) { if(disposing) { var owned=Image; Image=null; owned?.Dispose(); } base.Dispose(disposing); }
}

internal enum CaptionAction { Minimize, Maximize, Close }
internal sealed class WindowCaptionButton : Button
{
    private readonly CaptionAction glyph;
    private bool hovered;
    public WindowCaptionButton(CaptionAction glyph,Action action)
    {
        this.glyph=glyph; Size=new(28,24); AutoSize=false; Margin=new(2,0,0,0); MinimumSize=Size.Empty;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
        FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0; TabStop=true;
        AccessibleName=glyph switch { CaptionAction.Minimize=>"Minimize window",CaptionAction.Maximize=>"Maximize or restore window",_=>"Close window" };
        Click+=(_,_)=>action();
    }
    protected override void OnMouseEnter(EventArgs e) { hovered=true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered=false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(glyph==CaptionAction.Close?(hovered?Color.FromArgb(244,158,185):Color.FromArgb(251,203,218)):(hovered?Color.White:Color.FromArgb(224,235,253)));
        ControlPaint.DrawBorder(e.Graphics,ClientRectangle,Color.FromArgb(108,131,170),ButtonBorderStyle.Solid);
        float s=DeviceDpi/96f,cx=Width/2f,cy=Height/2f,r=4*s;
        using var pen=new Pen(Color.FromArgb(54,53,83),Math.Max(1.5f,s*1.5f));
        if(glyph==CaptionAction.Minimize)e.Graphics.DrawLine(pen,cx-r,cy+r/2,cx+r,cy+r/2);
        else if(glyph==CaptionAction.Maximize) { e.Graphics.DrawRectangle(pen,cx-r,cy-r,r*2,r*2); e.Graphics.DrawLine(pen,cx-r,cy-r+2*s,cx+r,cy-r+2*s); }
        else { e.Graphics.DrawLine(pen,cx-r,cy-r,cx+r,cy+r); e.Graphics.DrawLine(pen,cx+r,cy-r,cx-r,cy+r); }
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-3,-3));
    }
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
        var actions=new FlowLayoutPanel { Dock=DockStyle.Right, Width=98, FlowDirection=FlowDirection.LeftToRight, WrapContents=false, BackColor=Color.Transparent, Padding=new(0,1,0,0) };
        minimize=new WindowCaptionButton(CaptionAction.Minimize,owner.MinimizeWindow);
        maximize=new WindowCaptionButton(CaptionAction.Maximize,owner.ToggleMaximize);
        var close=new WindowCaptionButton(CaptionAction.Close,owner.Close);
        actions.Controls.AddRange([minimize,maximize,close]); Controls.Add(caption); Controls.Add(actions);
        owner.TextChanged+=(_,_)=>caption.Text=owner.Text; caption.Text=owner.Text;
        caption.MouseDown+=DragDown; caption.MouseMove+=DragMove; caption.MouseUp+=(_,_)=>caption.Capture=false;
        caption.DoubleClick+=(_,_)=> { if(owner.MaximizeBox)owner.ToggleMaximize(); };
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
    private readonly Image? portrait=Artwork.Guide();
    private string speech="Hover an app to learn more! Keyboard focus works too.";
    public GuideControl()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Opaque,true);
        Height=315; Width=230; TabStop=false; AccessibleRole=AccessibleRole.StaticText;
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
        Artwork.PaintBackdrop(this,e.Graphics);
        var g=e.Graphics; float scale=DeviceDpi/96f;
        var bubble=new Rectangle(2,2,Width-5,(int)(120*scale));
        using var fill=new SolidBrush(Color.FromArgb(255,249,247)); using var edge=new Pen(Color.FromArgb(189,146,178));
        g.FillRectangle(fill,bubble); g.DrawRectangle(edge,bubble);
        var tail=new[]{new Point(Width/2-8,bubble.Bottom),new Point(Width/2,bubble.Bottom+9),new Point(Width/2+8,bubble.Bottom)};
        g.FillPolygon(fill,tail); g.DrawLines(edge,tail);
        using var tagFont=new Font("Tahoma",8,FontStyle.Bold); using var copyFont=new Font("Tahoma",9.5f);
        TextRenderer.DrawText(g,"YOUR DREAM GUIDE",tagFont,new Rectangle(12,12,Width-24,(int)(18*scale)),Theme.Accent,TextFormatFlags.NoPadding|TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g,speech,copyFont,new Rectangle(14,(int)(36*scale),Width-28,bubble.Height-(int)(43*scale)),Theme.Ink,TextFormatFlags.NoPadding|TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak|TextFormatFlags.EndEllipsis);
        if(portrait is not null)
        {
            var source=new Rectangle(158,80,302,481); int height=Math.Max(1,Height-bubble.Bottom-(int)(20*scale)),width=(int)(height*302f/481f);
            using var shadow=new SolidBrush(Color.FromArgb(30,92,65,110)); g.FillEllipse(shadow,Width/2-width/2,Height-12,width,9);
            g.InterpolationMode=InterpolationMode.NearestNeighbor; g.PixelOffsetMode=PixelOffsetMode.Half;
            g.DrawImage(portrait,new Rectangle((Width-width)/2,bubble.Bottom+(int)(14*scale),width,height),source,GraphicsUnit.Pixel);
        }
    }
    public void Say(string text) { speech=text; AccessibleName="Guide: "+text; Invalidate(); if(IsHandleCreated)AccessibilityNotifyClients(AccessibleEvents.NameChange,-1); }
    public void Register(Control control,string text) { control.MouseEnter+=(_,_)=>Say(text); control.Enter+=(_,_)=>Say(text); }
    protected override void Dispose(bool disposing) { if(disposing)portrait?.Dispose(); base.Dispose(disposing); }
}
