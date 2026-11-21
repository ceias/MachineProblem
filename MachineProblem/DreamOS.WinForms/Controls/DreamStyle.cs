using System.Drawing.Drawing2D;
namespace DreamOS.WinForms.Controls;

/// <summary>Shared GDI+ surfaces from the supplied Dream OS style reference.</summary>
public static class DreamStyle
{
    public static readonly Color Border=ColorTranslator.FromHtml("#C28DBB"),Muted=ColorTranslator.FromHtml("#7A6B82"),Lavender=ColorTranslator.FromHtml("#CDB7F3");
    public static GraphicsPath Rounded(RectangleF r,float radius=9)
    {
        var path=new GraphicsPath(); float d=Math.Min(radius*2,Math.Min(r.Width,r.Height));
        if(d<=0)return path;
        path.AddArc(r.X,r.Y,d,d,180,90); path.AddArc(r.Right-d,r.Y,d,d,270,90);
        path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90); path.AddArc(r.X,r.Bottom-d,d,d,90,90); path.CloseFigure(); return path;
    }
    public static void Surface(Graphics g,Rectangle bounds,Color top,Color bottom,bool selected=false,int radius=9)
    {
        if(bounds.Width<4||bounds.Height<4)return;
        var state=g.Save(); g.SmoothingMode=SmoothingMode.AntiAlias;
        var r=new RectangleF(bounds.X+1,bounds.Y+1,bounds.Width-3,bounds.Height-3);
        using var path=Rounded(r,radius); using var fill=new LinearGradientBrush(r,top,bottom,LinearGradientMode.Vertical);
        g.FillPath(fill,path); using var edge=new Pen(selected?Theme.Accent:Border,selected?2:1); g.DrawPath(edge,path);
        using var inset=Rounded(RectangleF.Inflate(r,-2,-2),Math.Max(2,radius-2)); using var light=new Pen(Color.FromArgb(200,255,255,255)); g.DrawPath(light,inset);
        g.Restore(state);
    }
}
