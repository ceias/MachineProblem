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
        var panel=new IconCardPanel { Dock=DockStyle.Fill,Padding=new(8),Margin=new(4) };
        var header=new Panel { Dock=DockStyle.Top,Height=26,BackColor=panel.BackColor };
        var label=Text(title,10,true); label.Dock=DockStyle.Fill; label.AutoSize=false; label.TextAlign=ContentAlignment.MiddleLeft;
        var icon=CardIcons.Picture(CardIcons.For(title),24); icon.Dock=DockStyle.Left; icon.Width=32;
        header.Controls.Add(label); header.Controls.Add(icon);
        content.Dock=DockStyle.Fill; content.BackColor=panel.BackColor; panel.Controls.Add(content); panel.Controls.Add(header); return panel;
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
        var header=new IconCardPanel { Dock=DockStyle.Top,Height=66,Padding=new(12,5,12,4),Margin=new(0) };
        var text=new Panel { Dock=DockStyle.Fill,BackColor=header.BackColor };
        var heading=WorkflowUi.Text(title,16,true); heading.Dock=DockStyle.Top; heading.AutoSize=false; heading.Height=27; heading.ForeColor=Theme.Accent;
        hint.Text=subtitle; text.Controls.Add(hint); text.Controls.Add(heading);
        var icon=CardIcons.Picture(CardIcons.For(title),42); icon.Dock=DockStyle.Left; icon.Width=56;
        header.Controls.Add(text); header.Controls.Add(icon);
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
    private readonly BrandedReceiptView paper=new() { Location=new(5,5),Width=250 };
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
        using var dialog=new SaveFileDialog { Filter="Dream OS receipt image (*.png)|*.png|Text receipt (*.txt)|*.txt",FileName=document.Reference,DefaultExt="png",AddExtension=true,OverwritePrompt=true };
        if(dialog.ShowDialog(this)==DialogResult.OK)
        {
            if(dialog.FilterIndex==2)File.WriteAllText(dialog.FileName,document.ToText());
            else { using var image=ReceiptDesign.Image(document); image.Save(dialog.FileName,System.Drawing.Imaging.ImageFormat.Png); }
        }
    }
    private void Print()
    {
        if(document is null)return;
        if(PrinterSettings.InstalledPrinters.Count==0)throw new InvalidOperationException("No printer is installed. Use Save slip, or configure a printer/PDF printer in Windows.");
        var current=document; int first=0,page=1;
        using var job=new PrintDocument { DocumentName=current.Title };
        job.DefaultPageSettings.Margins=new Margins(35,35,35,35);
        job.BeginPrint+=(_,_)=> { first=0; page=1; };
        job.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            var g=e.Graphics; var state=g.Save();
            try
            {
                g.PageUnit=GraphicsUnit.Pixel;
                float px=g.DpiX/100f,py=g.DpiY/100f;
                float width=Math.Min(e.MarginBounds.Width*px,g.DpiX*3.15f),scale=width/ReceiptDesign.Width;
                if(scale<=0)throw new InvalidOperationException("Printer margins leave no room for the receipt.");
                float height=e.MarginBounds.Height*py/scale;
                g.TranslateTransform(e.MarginBounds.Left*px+(e.MarginBounds.Width*px-width)/2,e.MarginBounds.Top*py);
                g.ScaleTransform(scale,scale);
                var rows=ReceiptDesign.Layout(g,current);
                var plan=ReceiptPagination.Plan(rows.Count-first,height,ReceiptDesign.Header,ReceiptDesign.Row,ReceiptDesign.Footer,ReceiptDesign.ContinuedFooter);
                ReceiptDesign.Draw(g,current,rows,first,plan.Count,plan.Final,page++);
                first+=plan.Count; e.HasMorePages=!plan.Final;
            }
            finally { g.Restore(state); }
        };
        using var dialog=new PrintDialog { Document=job,UseEXDialog=true }; if(dialog.ShowDialog(this)==DialogResult.OK)
        {
            try { job.Print(); }
            catch(InvalidPrinterException e) { throw new InvalidOperationException("The selected printer is unavailable. Use Save slip or select another printer.",e); }
        }
    }
}
