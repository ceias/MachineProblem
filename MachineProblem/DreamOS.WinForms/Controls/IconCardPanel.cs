using System.Drawing.Drawing2D;
namespace DreamOS.WinForms.Controls;

public sealed class IconCardPanel : Panel
{
    public IconCardPanel() { DoubleBuffered=true; BackColor=Color.FromArgb(255,251,249); Margin=new(5); Padding=new(12); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); using var pen=new Pen(Color.FromArgb(222,193,211)); var r=Rectangle.Inflate(ClientRectangle,-1,-1); if(r.Width<12||r.Height<12)return;
        using var path=new GraphicsPath(); const int d=12;
        path.AddArc(r.Left,r.Top,d,d,180,90); path.AddArc(r.Right-d,r.Top,d,d,270,90); path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90); path.AddArc(r.Left,r.Bottom-d,d,d,90,90); path.CloseFigure();
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias; e.Graphics.DrawPath(pen,path);
    }
}
public static class CardIcons
{
    public static string For(string title)
    {
        string text=title.ToLowerInvariant();
        if(text.Contains("pizza")||text.Contains("crust")||text.Contains("topping")||text.Contains("sip"))return "pizza";
        if(text.Contains("lotto")||text.Contains("ticket"))return "lotto";
        if(text.Contains("tuition")||text.Contains("enroll")||text.Contains("study")||text.Contains("fee"))return "tuition";
        if(text.Contains("payroll")||text.Contains("employee")||text.Contains("pay period"))return "payroll";
        if(text.Contains("hotel")||text.Contains("guest")||text.Contains("room")||text.Contains("stay")||text.Contains("duration"))return "hotel";
        if(text.Contains("ballot")||text.Contains("tally")||text.Contains("election"))return "election";
        if(text.Contains("password")||text.Contains("sign in"))return "password";
        if(text.Contains("appliance")||text.Contains("cashier")||text.Contains("catalog"))return "store";
        if(text.Contains("music")||text.Contains("listening"))return "music";
        if(text.Contains("notepad")||text.Contains("notes"))return "payroll";
        if(text.Contains("widget")||text.Contains("remind")||text.Contains("day")||text.Contains("calendar")||text.Contains("date"))return "widgets";
        return "about";
    }
    public static PictureBox Picture(string id,int size)
    {
        var view=new PictureBox { Size=new(size,size),Image=Artwork.Icon(id,size),SizeMode=PictureBoxSizeMode.CenterImage,Margin=new(0,0,8,0) };
        view.Disposed+=(_,_)=>view.Image?.Dispose(); return view;
    }
}
