using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using DreamOS.Core;

namespace DreamOS.WinForms.Controls;

public static class WorkflowUi
{
    public static Label Text(string value,float size=10,bool bold=false)=>new() { Text=value,AutoSize=true,Font=new("Tahoma",size,bold?FontStyle.Bold:FontStyle.Regular),ForeColor=Theme.Ink,Margin=new(0,0,0,6),MaximumSize=new(600,0) };
    public static TableLayoutPanel Grid(int columns=2)
    {
        var grid=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=columns,Padding=new(0),Margin=new(0),AutoScroll=true };
        for(int i=0;i<columns;i++)grid.ColumnStyles.Add(new(SizeType.Percent,100f/columns));
        grid.RowCount=1; grid.RowStyles.Add(new(SizeType.Percent,100)); return grid;
    }
    public static FlowLayoutPanel Stack()=>new() { Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new(12),Margin=new(0),BackColor=Theme.Cream };
    public static Panel Section(string title,Control content)
    {
        var panel=new Panel { Dock=DockStyle.Fill,Padding=new(10),Margin=new(4),BackColor=Color.FromArgb(255,248,245) };
        var label=Text(title,10,true); label.Dock=DockStyle.Top; label.Height=26; label.AutoSize=false;
        content.Dock=DockStyle.Fill; panel.Controls.Add(content); panel.Controls.Add(label); return panel;
    }
    public static void Field(FlowLayoutPanel target,string label,Control input)
    {
        target.Controls.Add(Text(label,9,true)); input.AccessibleName=label; input.Margin=new(0,0,0,12); target.Controls.Add(input);
    }
    public static FlowLayoutPanel Cards(params Control[] controls)
    {
        var flow=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,Margin=new(0),Padding=new(0) }; flow.Controls.AddRange(controls); return flow;
    }
}

public class WorkflowForm : RetroForm
{
    protected Panel Workspace { get; }=new() { Dock=DockStyle.Fill,Padding=new(10) };
    private readonly FlowLayoutPanel commands=new() { Dock=DockStyle.Bottom,Height=54,Padding=new(8,4,8,4),BackColor=Theme.Pink,WrapContents=false,AutoScroll=true };
    private readonly Label hint=new() { Dock=DockStyle.Bottom,Height=23,AutoEllipsis=true,ForeColor=Color.FromArgb(114,98,128),Font=new("Tahoma",9) };
    public WorkflowForm() : this("Dream OS","Choose your options to get started.") { }
    protected WorkflowForm(string title,string subtitle) : base(title)
    {
        MinimumSize=new(820,510); Size=new(1060,620);
        var header=new Panel { Dock=DockStyle.Top,Height=62,Padding=new(15,8,15,2),BackColor=Theme.Cream };
        var heading=WorkflowUi.Text(title,16,true); heading.Dock=DockStyle.Top; heading.AutoSize=false; heading.Height=27; heading.ForeColor=Theme.Accent;
        hint.Text=subtitle; header.Controls.Add(hint); header.Controls.Add(heading);
        Controls.Add(Workspace); Controls.Add(commands); Controls.Add(header);
    }
    protected void Split(Control main,Control side,int sideWidth=280)
    {
        var grid=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new(0) };
        grid.ColumnStyles.Add(new(SizeType.Percent,100)); grid.ColumnStyles.Add(new(SizeType.Absolute,sideWidth)); grid.RowStyles.Add(new(SizeType.Percent,100));
        main.Dock=side.Dock=DockStyle.Fill; grid.Controls.Add(main,0,0); grid.Controls.Add(side,1,0); Workspace.Controls.Add(grid);
    }
    protected Button Command(string title,Action action)
    {
        var button=Theme.Button(title,()=>Run(action)); button.Margin=new(3,3,3,3); commands.Controls.Add(button); return button;
    }
    protected void Status(string text,bool error=false) { hint.Text=text; hint.ForeColor=error?Color.Firebrick:Theme.Accent; }
    protected void Run(Action action)=>Theme.Guard(this,()=> { try { action(); } catch(ArgumentException e) { Status(e.Message,true); throw; } });
}

public sealed class ChoiceCard : Button
{
    private bool selected;
    private readonly Image? sprite;
    private readonly string description;
    public ChoiceCard(string title,string description,int spriteIndex,Action action)
    {
        Text=title; this.description=description; sprite=WorkflowArt.Sprite(spriteIndex);
        Size=new(102,94); Margin=new(3); Font=new("Tahoma",9); FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0; UseVisualStyleBackColor=false;
        AccessibleName=title+" "+description; Click+=(_,_)=>action();
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
    }
    public void SelectCard(bool value) { selected=value; AccessibleDescription=value?"Selected":"Not selected"; Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(selected?Color.FromArgb(252,222,234):Color.FromArgb(255,251,248));
        ControlPaint.DrawBorder(e.Graphics,ClientRectangle,selected?Theme.Accent:Color.FromArgb(220,205,218),selected?2:1,ButtonBorderStyle.Solid,selected?Theme.Accent:Color.FromArgb(220,205,218),1,ButtonBorderStyle.Solid,selected?Theme.Accent:Color.FromArgb(220,205,218),1,ButtonBorderStyle.Solid,selected?Theme.Accent:Color.FromArgb(220,205,218),1,ButtonBorderStyle.Solid);
        int S(int value)=>LogicalToDeviceUnits(value);
        int imageHeight=(int)(Height*.5f),textTop=imageHeight+S(6),footer=S(18);
        if(sprite is not null) { e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor; e.Graphics.DrawImage(sprite,new Rectangle((Width-imageHeight)/2,S(4),imageHeight,imageHeight)); }
        TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(S(3),textTop,Width-S(6),Math.Max(1,Height-textTop-footer-S(3))),Theme.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);
        using var small=new Font("Tahoma",8); TextRenderer.DrawText(e.Graphics,description,small,new Rectangle(S(3),Height-footer,Width-S(6),footer-S(2)),Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPadding|TextFormatFlags.EndEllipsis);
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-4,-4));
    }
    protected override void Dispose(bool disposing) { if(disposing)sprite?.Dispose(); base.Dispose(disposing); }
}
public static class WorkflowArt
{
    private static readonly Lazy<Bitmap?> Atlas=new(()=>
    {
        string path=Path.Combine(AppContext.BaseDirectory,"Assets","WorkflowSprites.png");
        if(!File.Exists(path))return null; using var image=Image.FromFile(path); return new Bitmap(image);
    });
    static WorkflowArt()=>Application.ApplicationExit+=(_,_)=> { if(Atlas.IsValueCreated)Atlas.Value?.Dispose(); };
    public static Bitmap? Sprite(int index)
    {
        if(index<0||index>=30)return null;
        var atlas=Atlas.Value; if(atlas is null)return null; int col=index%6,row=index/6;
        int x=col*atlas.Width/6,y=row*atlas.Height/5,w=(col+1)*atlas.Width/6-x,h=(row+1)*atlas.Height/5-y;
        var output=new Bitmap(w,h); using var g=Graphics.FromImage(output); g.DrawImage(atlas,new Rectangle(0,0,w,h),new Rectangle(x,y,w,h),GraphicsUnit.Pixel); return output;
    }
}

public sealed class ReceiptPanel : UserControl
{
    private readonly ReceiptPaper paper=new() { Location=new(5,5),Width=250 };
    private readonly Panel scroll=new() { Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.FromArgb(238,221,232) };
    private readonly Button save,print;
    private readonly Label summary=new() { Dock=DockStyle.Bottom,Height=62,BackColor=Color.FromArgb(255,248,245),ForeColor=Theme.Accent,TextAlign=ContentAlignment.MiddleCenter,Font=new("Consolas",14,FontStyle.Bold),AutoEllipsis=true,Padding=new(4) };
    private ReceiptData? document;
    public ReceiptPanel()
    {
        Padding=new(4); BackColor=Color.FromArgb(238,221,232);
        var actions=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=48,WrapContents=false,Padding=new(3) };
        save=Theme.Button("Save slip",()=>Theme.Guard(this,Save)); print=Theme.Button("Print",()=>Theme.Guard(this,Print)); save.MinimumSize=new(90,32); print.MinimumSize=new(75,32);
        actions.Controls.Add(save); actions.Controls.Add(print); scroll.Controls.Add(paper); Controls.Add(scroll); Controls.Add(summary); Controls.Add(actions);
        scroll.Resize+=(_,_)=> { paper.Width=Math.Max(150,scroll.ClientSize.Width-SystemInformation.VerticalScrollBarWidth-10); paper.Reflow(); };
        Clear();
    }
    public void Clear(string message="Your itemized slip will appear here.") { document=null; paper.SetDocument(null,message); summary.Text="AWAITING\nCALCULATION"; save.Enabled=print.Enabled=false; }
    public void ShowDocument(ReceiptData value) { document=value; paper.SetDocument(value,""); summary.Text=value.TotalLabel+"\n"+value.Total; save.Enabled=print.Enabled=true; }
    private void Save()
    {
        if(document is null)return;
        using var dialog=new SaveFileDialog { Filter="Text receipt (*.txt)|*.txt",FileName=document.Reference+".txt",OverwritePrompt=true };
        if(dialog.ShowDialog(this)==DialogResult.OK)File.WriteAllText(dialog.FileName,document.ToText());
    }
    private void Print()
    {
        if(document is null)return;
        if(PrinterSettings.InstalledPrinters.Count==0)throw new InvalidOperationException("No printer is installed. Use Save slip, or configure a printer/PDF printer in Windows.");
        string remaining=document.ToText(); using var job=new PrintDocument { DocumentName=document.Title };
        using var font=new Font("Consolas",10);
        job.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            e.Graphics.MeasureString(remaining,font,e.MarginBounds.Size,StringFormat.GenericTypographic,out int chars,out int lines);
            if(chars==0)throw new InvalidOperationException("Printer margins leave no room for the receipt.");
            e.Graphics.DrawString(remaining[..chars],font,Brushes.Black,e.MarginBounds,StringFormat.GenericTypographic); remaining=remaining[chars..]; e.HasMorePages=remaining.Length>0;
        };
        using var dialog=new PrintDialog { Document=job,UseEXDialog=true }; if(dialog.ShowDialog(this)==DialogResult.OK)
        {
            try { job.Print(); }
            catch(InvalidPrinterException e) { throw new InvalidOperationException("The selected printer is unavailable. Use Save slip or select another printer.",e); }
        }
    }
    private sealed class ReceiptPaper : Control
    {
        private ReceiptData? document; private string empty="";
        private readonly Image? icon=WorkflowArt.Sprite(29);
        public ReceiptPaper() { DoubleBuffered=true; BackColor=Color.FromArgb(255,253,247); Font=new("Tahoma",9); }
        public void SetDocument(ReceiptData? value,string message) { document=value; empty=message; Reflow(); Invalidate(); }
        private int RowHeight(ReceiptEntry row)
        {
            int inside=Math.Max(80,Width-LogicalToDeviceUnits(32)),w=inside*58/100;
            int label=TextRenderer.MeasureText(row.Label,Font,new Size(w,int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height;
            int value=TextRenderer.MeasureText(row.Value,Font,new Size(inside-w,int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height;
            return Math.Max(LogicalToDeviceUnits(26),Math.Max(label,value)+LogicalToDeviceUnits(8));
        }
        public void Reflow() { Height=document is null?LogicalToDeviceUnits(280):LogicalToDeviceUnits(326)+document.Entries.Sum(RowHeight); Invalidate(); }
        protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Reflow(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); var g=e.Graphics; int S(int value)=>LogicalToDeviceUnits(value);
            using var ink=new SolidBrush(Theme.Ink); using var muted=new SolidBrush(Color.FromArgb(128,104,121));
            using var head=new Font("Consolas",16,FontStyle.Bold); using var bold=new Font("Tahoma",10,FontStyle.Bold); using var small=new Font("Tahoma",8);
            if(document is null)
            {
                if(icon is not null) { g.InterpolationMode=InterpolationMode.NearestNeighbor; g.DrawImage(icon,new Rectangle(Width/2-S(45),S(24),S(90),S(90))); }
                TextRenderer.DrawText(g,"YOUR RECEIPT",bold,new Rectangle(S(12),S(134),Width-S(24),S(24)),Theme.Accent,TextFormatFlags.HorizontalCenter);
                TextRenderer.DrawText(g,empty,Font,new Rectangle(S(18),S(173),Width-S(36),S(80)),Theme.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak); return;
            }
            int margin=S(16),inside=Width-margin*2;
            g.DrawString("DREAM OS",head,ink,margin,S(17)); g.DrawString(document.Title,bold,ink,new RectangleF(margin,S(47),inside,S(40)));
            g.DrawString(document.Reference,small,muted,new RectangleF(margin,S(89),inside,S(32))); g.DrawString(document.Issued.ToString("dd MMM yyyy · HH:mm"),small,muted,margin,S(120));
            using var dash=new Pen(Color.FromArgb(194,171,183)) { DashStyle=DashStyle.Dash }; int y=S(147); g.DrawLine(dash,S(14),y,Width-S(14),y); y+=S(12);
            foreach(var row in document.Entries)
            {
                int height=RowHeight(row),leftWidth=inside*58/100;
                TextRenderer.DrawText(g,row.Label,Font,new Rectangle(margin,y,leftWidth,height),Theme.Ink,TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g,row.Value,Font,new Rectangle(margin+leftWidth,y,inside-leftWidth,height),Theme.Ink,TextFormatFlags.Right|TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);
                y+=height;
            }
            g.DrawLine(dash,S(14),y,Width-S(14),y); y+=S(12);
            g.DrawString(document.TotalLabel,bold,muted,margin,y); y+=S(25);
            using var total=new Font("Consolas",19,FontStyle.Bold); g.DrawString(document.Total,total,ink,new RectangleF(margin,y,inside,S(40))); y+=S(47);
            TextRenderer.DrawText(g,document.Footer,small,new Rectangle(margin,y,inside,S(55)),Theme.Accent,TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);
            for(int x=0;x<Width;x+=S(12))g.FillPolygon(Brushes.White,[new Point(x,Height),new Point(x+S(6),Height-S(6)),new Point(x+S(12),Height)]);
        }
        protected override void Dispose(bool disposing) { if(disposing)icon?.Dispose(); base.Dispose(disposing); }
    }
}
