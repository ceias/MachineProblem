using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using DreamOS.Core;

namespace DreamOS.WinForms.Controls;

internal sealed record ReceiptVisualRow(string Label,string Value);
internal static class ReceiptDesign
{
    public const float Width=300,Header=218,Row=20,Footer=206,ContinuedFooter=44;
    private static readonly Color Rose=Color.FromArgb(203,99,144),Muted=Color.FromArgb(167,123,145);
    private static Font Font(float px,bool bold=false)=>new("Tahoma",px,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel);
    public static List<ReceiptVisualRow> Layout(Graphics g,ReceiptData document)
    {
        using var font=Font(11.5f); var rows=new List<ReceiptVisualRow>();
        foreach(var entry in document.Entries)
        {
            var left=Wrap(g,entry.Label,font,156); var right=Wrap(g,entry.Value,font,88);
            for(int i=0;i<Math.Max(left.Count,right.Count);i++)rows.Add(new(i<left.Count?left[i]:"",i<right.Count?right[i]:""));
        }
        return rows;
    }
    private static List<string> Wrap(Graphics g,string text,Font font,float width)
    {
        var result=new List<string>();
        foreach(string paragraph in text.Replace("\r","").Split('\n'))
        {
            string line="";
            foreach(char c in paragraph)
            {
                if(line.Length>0&&g.MeasureString(line+c,font,int.MaxValue,StringFormat.GenericTypographic).Width>width) { result.Add(line); line=""; }
                line+=c;
            }
            result.Add(line);
        }
        return result;
    }
    public static float Height(int rows)=>Header+rows*Row+Footer;
    public static void Draw(Graphics g,ReceiptData document,IReadOnlyList<ReceiptVisualRow> rows,int first,int count,bool final,int page=1)
    {
        float height=Header+count*Row+(final?Footer:ContinuedFooter);
        using var white=new SolidBrush(Color.FromArgb(255,254,252)); using var ink=new SolidBrush(Rose); using var muted=new SolidBrush(Muted);
        using var line=new Pen(Color.FromArgb(218,174,195),1); using var dash=new Pen(Rose,1.5f) { DashStyle=DashStyle.Dash };
        g.FillRectangle(white,0,0,Width,height);
        using var logo=Font(31,true); using var tiny=Font(8.5f); using var regular=Font(11.5f); using var bold=Font(11.5f,true); using var amount=Font(21,true);
        var center=new StringFormat { Alignment=StringAlignment.Center };
        var right=new StringFormat { Alignment=StringAlignment.Far };
        g.DrawString("dream os.",logo,ink,new RectangleF(20,18,260,40),center);
        g.DrawString("SMALL PROGRAMS. BIG DREAMS.",tiny,muted,new RectangleF(20,59,260,15),center);
        g.DrawLine(line,22,79,278,79);
        using(var pizza=WorkflowArt.Sprite(0))using(var appliance=WorkflowArt.Sprite(15))using(var slip=WorkflowArt.Sprite(29))using(var heart=Artwork.Icon("about",40))
        {
            Image?[] icons=[pizza,appliance,slip,heart];
            for(int i=0;i<icons.Length;i++)if(icons[i] is Image icon) { g.InterpolationMode=InterpolationMode.NearestNeighbor; g.DrawImage(icon,new Rectangle(47+i*54,88,42,42)); }
        }
        g.DrawLine(line,22,141,278,141);
        g.DrawString(document.Title,bold,ink,new RectangleF(22,150,256,17),center);
        g.DrawString(document.Reference,tiny,muted,new RectangleF(22,171,256,14),center);
        g.DrawString(document.Issued.ToString("dd MMM yyyy  HH:mm")+$"  ·  {page}",tiny,muted,new RectangleF(22,186,256,14),center);
        g.DrawString("ITEM / DETAILS",bold,ink,22,205); g.DrawString("VALUE",bold,ink,new RectangleF(186,205,92,17),right);
        float y=Header+4;
        for(int i=first;i<first+count;i++)
        {
            g.DrawString(rows[i].Label,regular,ink,new RectangleF(22,y,156,Row));
            g.DrawString(rows[i].Value,regular,ink,new RectangleF(186,y,92,Row),right); y+=Row;
        }
        y+=6; g.DrawLine(dash,22,y,278,y); y+=13;
        if(!final)
        {
            g.DrawString("CONTINUED ON NEXT PAGE",tiny,muted,new RectangleF(22,y,256,18),center); return;
        }
        g.DrawString(document.TotalLabel,bold,ink,new RectangleF(22,y,256,18)); y+=22;
        g.DrawString(document.Total,amount,ink,new RectangleF(22,y,256,30),right); y+=34;
        g.DrawLine(line,22,y,278,y); y+=12;
        byte[] pattern=SHA256.HashData(Encoding.UTF8.GetBytes(document.Reference)); float x=33;
        for(int i=0;i<64;i++) { float width=1+(pattern[i%pattern.Length]>>(i%5)&1); g.FillRectangle(ink,x,y,width,36+(i%3==0?3:0)); x+=width+2; }
        y+=43;
        g.DrawString("D R E A M   O S",tiny,muted,new RectangleF(22,y,256,14),center); y+=18;
        g.DrawString(document.Footer,tiny,muted,new RectangleF(22,y,256,38),center);
        using var edge=new Pen(Color.FromArgb(225,204,214),.6f);
        for(int i=0;i<300;i+=12) { g.DrawLine(edge,i,height-4,i+6,height); g.DrawLine(edge,i+6,height,i+12,height-4); }
    }
    public static Bitmap Image(ReceiptData document,int width=1200)
    {
        using var probe=new Bitmap(1,1); using var measure=Graphics.FromImage(probe); var rows=Layout(measure,document);
        float scale=width/Width; var output=new Bitmap(width,(int)Math.Ceiling(Height(rows.Count)*scale),PixelFormat.Format32bppArgb);
        output.SetResolution(width/3.15f,width/3.15f);
        using var g=Graphics.FromImage(output); g.Clear(Color.White); g.ScaleTransform(scale,scale); Draw(g,document,rows,0,rows.Count,true); return output;
    }
}

internal sealed class BrandedReceiptView : Control
{
    private ReceiptData? document; private string message=""; private List<ReceiptVisualRow> rows=[];
    public BrandedReceiptView() { DoubleBuffered=true; BackColor=Color.FromArgb(255,254,252); }
    public void SetDocument(ReceiptData? value,string empty) { document=value; message=empty; Reflow(); }
    public void Reflow()
    {
        if(document is not null) { using var sample=new Bitmap(1,1); using var g=Graphics.FromImage(sample); rows=ReceiptDesign.Layout(g,document); }
        Height=document is null?LogicalToDeviceUnits(260):(int)Math.Ceiling(ReceiptDesign.Height(rows.Count)*Math.Max(1,Width)/ReceiptDesign.Width); Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if(document is null)
        {
            using var icon=WorkflowArt.Sprite(29); if(icon is not null)e.Graphics.DrawImage(icon,new Rectangle(Width/2-40,25,80,80));
            using var font=new Font("Tahoma",9); TextRenderer.DrawText(e.Graphics,message,font,new Rectangle(15,133,Width-30,110),Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak); return;
        }
        var state=e.Graphics.Save(); e.Graphics.PageUnit=GraphicsUnit.Pixel; float scale=Width/ReceiptDesign.Width; e.Graphics.ScaleTransform(scale,scale);
        ReceiptDesign.Draw(e.Graphics,document,rows,0,rows.Count,true); e.Graphics.Restore(state);
    }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Reflow(); }
}
