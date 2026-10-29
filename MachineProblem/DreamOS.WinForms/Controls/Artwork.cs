using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DreamOS.WinForms.Controls;

/// <summary>Decorative regions from the supplied mockups; never draws baked-in controls as inputs.</summary>
public static class Artwork
{
    private static readonly Dictionary<string, Bitmap> Images = [];
    private static Bitmap? Load(string name)
    {
        if (Images.TryGetValue(name, out var cached)) return cached;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", name);
        if (!File.Exists(path)) return null;
        using var source = Image.FromFile(path);
        var bitmap = new Bitmap(source); Images[name] = bitmap; return bitmap;
    }
    static Artwork() => Application.ApplicationExit += (_, _) => { foreach (var bitmap in Images.Values) bitmap.Dispose(); Images.Clear(); };
    public static void Draw(Graphics g, string name, Rectangle source, Rectangle destination)
    {
        var image = Load(name); if (image is null) return;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(image, destination, source, GraphicsUnit.Pixel);
    }
    public static Bitmap? Icon(string id, int size = 40)
    {
        (string File, Rectangle Region) part = id switch
        {
            "pizza" => ("Pizza.png", new(20, 46, 57, 59)),
            "lotto" => ("Lotto.png", new(37, 87, 78, 116)),
            "tuition" => ("Tuition.png", new(39, 36, 41, 34)),
            "hotel" => ("Hotel.png", new(30, 36, 48, 62)),
            "password" => ("Widgets.png", new(198, 86, 34, 36)),
            "payroll" => ("Widgets.png", new(154, 24, 31, 35)),
            "election" => ("Widgets.png", new(195, 87, 39, 35)),
            "store" => ("Widgets.png", new(92, 25, 35, 31)),
            "widgets" => ("Widgets.png", new(330, 85, 38, 38)),
            "music" => ("Widgets.png", new(346, 24, 31, 33)),
            "about" => ("Login.png", new(120, 60, 68, 69)),
            _ => ("Widgets.png", new(40, 86, 37, 36))
        };
        var sourceImage=Load(part.File); if(sourceImage is null)return null;
        using var sprite=new Bitmap(part.Region.Width,part.Region.Height,PixelFormat.Format32bppArgb);
        using(var crop=Graphics.FromImage(sprite))crop.DrawImage(sourceImage,new Rectangle(Point.Empty,sprite.Size),part.Region,GraphicsUnit.Pixel);
        MaskIconBackdrop(sprite);
        var output = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(output); g.Clear(Color.Transparent);
        float scale = Math.Min((float)size / part.Region.Width, (float)size / part.Region.Height);
        var target = new Rectangle((size-(int)(part.Region.Width*scale))/2, (size-(int)(part.Region.Height*scale))/2, (int)(part.Region.Width*scale), (int)(part.Region.Height*scale));
        g.InterpolationMode=InterpolationMode.NearestNeighbor; g.PixelOffsetMode=PixelOffsetMode.Half;
        g.DrawImage(sprite,target,new Rectangle(Point.Empty,sprite.Size),GraphicsUnit.Pixel);
        return output;
    }
    private static void MaskIconBackdrop(Bitmap sprite)
    {
        Color[] corners=[sprite.GetPixel(0,0),sprite.GetPixel(sprite.Width-1,0),sprite.GetPixel(0,sprite.Height-1),sprite.GetPixel(sprite.Width-1,sprite.Height-1)];
        var backgrounds=corners.Where(c=>c.R>190&&c.G>165&&c.B>170).ToArray();
        if(backgrounds.Length==0)return;
        var visited=new bool[sprite.Width*sprite.Height]; var queue=new Queue<Point>();
        for(int x=0;x<sprite.Width;x++) { queue.Enqueue(new(x,0)); queue.Enqueue(new(x,sprite.Height-1)); }
        for(int y=0;y<sprite.Height;y++) { queue.Enqueue(new(0,y)); queue.Enqueue(new(sprite.Width-1,y)); }
        while(queue.TryDequeue(out var p))
        {
            if(p.X<0||p.Y<0||p.X>=sprite.Width||p.Y>=sprite.Height)continue;
            int i=p.Y*sprite.Width+p.X; if(visited[i])continue; visited[i]=true;
            var c=sprite.GetPixel(p.X,p.Y);
            if(!backgrounds.Any(b=>Math.Abs(b.R-c.R)<34&&Math.Abs(b.G-c.G)<34&&Math.Abs(b.B-c.B)<34))continue;
            sprite.SetPixel(p.X,p.Y,Color.Transparent);
            queue.Enqueue(new(p.X+1,p.Y)); queue.Enqueue(new(p.X-1,p.Y)); queue.Enqueue(new(p.X,p.Y+1)); queue.Enqueue(new(p.X,p.Y-1));
        }
    }
    public static Bitmap? Guide()
    {
        if(Images.TryGetValue("Guide.cutout",out var cached))return new Bitmap(cached);
        var source = Load("Guide.jpg"); if (source is null) return null;
        // Only border-connected pale backdrop is transparent; enclosed white socks/eyes survive.
        var image = new Bitmap(source); var pending = new Queue<Point>(); var visited = new bool[image.Width * image.Height];
        for (int x = 0; x < image.Width; x++) { pending.Enqueue(new(x,0)); pending.Enqueue(new(x,image.Height-1)); }
        for (int y = 0; y < image.Height; y++) { pending.Enqueue(new(0,y)); pending.Enqueue(new(image.Width-1,y)); }
        while (pending.TryDequeue(out var p))
        {
            if (p.X < 0 || p.Y < 0 || p.X >= image.Width || p.Y >= image.Height) continue;
            int index = p.Y * image.Width + p.X; if (visited[index]) continue; visited[index] = true;
            var c = image.GetPixel(p.X,p.Y);
            if (c.R < 228 || c.G < 228 || c.B < 228 || Math.Max(c.R,Math.Max(c.G,c.B))-Math.Min(c.R,Math.Min(c.G,c.B)) > 18) continue;
            image.SetPixel(p.X,p.Y,Color.Transparent);
            pending.Enqueue(new(p.X+1,p.Y)); pending.Enqueue(new(p.X-1,p.Y)); pending.Enqueue(new(p.X,p.Y+1)); pending.Enqueue(new(p.X,p.Y-1));
        }
        Images["Guide.cutout"]=image;
        return new Bitmap(image);
    }
    public static void Wallpaper(Graphics g, Rectangle bounds, bool login)
    {
        if (bounds.Width < 1 || bounds.Height < 1) return;
        var state = g.Save();
        using var gradient = new LinearGradientBrush(bounds, Color.White, Color.White, LinearGradientMode.Vertical);
        gradient.InterpolationColors = new ColorBlend
        {
            Colors = login
                ? [Color.FromArgb(248,208,229), Color.FromArgb(244,206,230), Color.FromArgb(238,179,212)]
                : [Color.FromArgb(170,203,237), Color.FromArgb(216,199,231), Color.FromArgb(244,191,217)],
            Positions = [0f, .58f, 1f]
        };
        g.FillRectangle(gradient,bounds);
        // Draw continuous cloud banks. Never stretch strips from a flattened UI screenshot.
        g.SmoothingMode = SmoothingMode.None;
        void Cloud(float cx, float cy, float scale, Color color)
        {
            using var brush = new SolidBrush(color);
            Point[] pixels = [new(-52,12),new(-52,0),new(-42,0),new(-42,-12),new(-30,-12),new(-30,-24),new(-14,-24),new(-14,-34),new(3,-34),new(3,-27),new(17,-27),new(17,-15),new(34,-15),new(34,-4),new(47,-4),new(47,6),new(59,6),new(59,19),new(-52,19)];
            var shape = pixels.Select(p=>new PointF(bounds.Left+cx+p.X*scale,bounds.Top+cy+p.Y*scale)).ToArray();
            g.FillPolygon(brush,shape);
        }
        float w=bounds.Width,h=bounds.Height;
        for(int layer=0;layer<3;layer++)
        {
            Color color = layer switch { 0=>Color.FromArgb(245,219,236),1=>Color.FromArgb(252,228,239),_=>Color.FromArgb(255,240,243) };
            float scale=layer==0?2.1f:layer==1?1.75f:1.45f;
            for(int i=-1;i<10;i++)
            {
                float x=i*180+layer*63, y=h+18-layer*19+(i%3)*13;
                Cloud(x,y,scale,color);
            }
        }
        Cloud(w*.42f,h*.22f,.85f,Color.FromArgb(228,227,243));
        Cloud(w*.76f,h*.14f,.65f,Color.FromArgb(239,232,244));
        Cloud(w*.12f,h*.36f,.95f,Color.FromArgb(233,221,241));
        using var light=new SolidBrush(Color.FromArgb(235,255,247,245));
        using var rose=new SolidBrush(Color.FromArgb(170,224,131,177));
        for(int i=0;i<32;i++)
        {
            int x=bounds.Left+(i*179+41)%Math.Max(1,bounds.Width-25), y=bounds.Top+(i*97+31)%Math.Max(1,(int)(bounds.Height*.8f));
            int r=i%5==0?5:2; var brush=i%4==0?rose:light;
            g.FillRectangle(brush,x-r,y,r*2+1,1); g.FillRectangle(brush,x,y-r,1,r*2+1);
            if(r==5)g.FillRectangle(brush,x-1,y-1,3,3);
        }
        g.Restore(state);
    }

    public static void PaintBackdrop(Control control,Graphics g)
    {
        Point offset=Point.Empty; Control? current=control;
        while(current is not null && current is not WallpaperPanel)
        {
            offset.Offset(current.Left,current.Top); current=current.Parent;
        }
        if(current is not WallpaperPanel wallpaper) { g.Clear(Theme.Cream); return; }
        var state=g.Save(); g.SetClip(control.ClientRectangle); g.TranslateTransform(-offset.X,-offset.Y);
        wallpaper.PaintWallpaper(g); g.Restore(state);
    }
}

public sealed class WallpaperPanel : Panel
{
    private Bitmap? wallpaper;
    [System.ComponentModel.DefaultValue(false)]
    public bool LoginStyle { get; set; }
    [System.ComponentModel.DefaultValue(false)]
    public bool ShowDesktopBrand { get; set; }
    public WallpaperPanel() { DoubleBuffered=true; ResizeRedraw=true; }
    internal void PaintWallpaper(Graphics g)
    {
        if(ClientSize.Width<1||ClientSize.Height<1)return;
        if(wallpaper is null||wallpaper.Size!=ClientSize)
        {
            wallpaper?.Dispose(); wallpaper=new Bitmap(Width,Height,PixelFormat.Format32bppPArgb);
            using var canvas=Graphics.FromImage(wallpaper); Artwork.Wallpaper(canvas,ClientRectangle,LoginStyle);
            if(ShowDesktopBrand)
            {
                using var eyebrow=new Font("Tahoma",8,FontStyle.Bold); using var logo=new Font("Consolas",26,FontStyle.Bold); using var detail=new Font("Tahoma",10);
                using var ink=new SolidBrush(Theme.Ink); using var muted=new SolidBrush(Color.FromArgb(113,99,137));
                canvas.DrawString("YOUR PERSONAL LITTLE UNIVERSE",eyebrow,muted,256,24);
                canvas.DrawString("DREAM OS",logo,ink,251,42);
                canvas.DrawString("Small programs. Big dreams.",detail,muted,256,89);
            }
        }
        g.DrawImageUnscaled(wallpaper,0,0);
    }
    protected override void OnPaintBackground(PaintEventArgs e)=>PaintWallpaper(e.Graphics);
    protected override void OnResize(EventArgs e) { wallpaper?.Dispose(); wallpaper=null; base.OnResize(e); Invalidate(true); }
    protected override void Dispose(bool disposing) { if(disposing)wallpaper?.Dispose(); base.Dispose(disposing); }
}

/// <summary>Paints the workspace at the correct offset without WinForms transparent-parent recursion.</summary>
public sealed class DesktopIconShelf : FlowLayoutPanel
{
    public DesktopIconShelf() { SetStyle(ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint,true); }
    protected override void OnPaintBackground(PaintEventArgs e)=>Artwork.PaintBackdrop(this,e.Graphics);
}

public sealed class HeartBadge : Control
{
    public HeartBadge() { DoubleBuffered = true; SetStyle(ControlStyles.SupportsTransparentBackColor,true); BackColor = Color.Transparent; }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); Artwork.Draw(e.Graphics,"Login.png",new(120,60,68,69),ClientRectangle); }
}

public sealed class DesktopCalendarCard : Button
{
    public DesktopCalendarCard()
    {
        Size=new(230,134); AccessibleName="Open calendar and notes"; Cursor=Cursors.Hand;
        FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Opaque,true);
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics; g.Clear(Color.FromArgb(255,247,249));
        ControlPaint.DrawBorder(g,ClientRectangle,Color.FromArgb(195,166,196),ButtonBorderStyle.Solid);
        float s=DeviceDpi/96f; int S(int x)=>(int)(x*s);
        using var tiny=new Font("Tahoma",7.5f,FontStyle.Bold); using var day=new Font("Consolas",32,FontStyle.Bold); using var time=new Font("Consolas",20,FontStyle.Bold); using var text=new Font("Tahoma",9);
        TextRenderer.DrawText(g,"TODAY IN YOUR LITTLE WORLD",tiny,new Rectangle(S(13),S(12),Width-S(26),S(16)),Theme.Accent,TextFormatFlags.NoPadding);
        TextRenderer.DrawText(g,DateTime.Now.ToString("dd"),day,new Rectangle(S(11),S(35),S(68),S(52)),Theme.Accent,TextFormatFlags.NoPadding|TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g,DateTime.Now.ToString("MMM").ToUpperInvariant(),tiny,new Rectangle(S(11),S(87),S(68),S(15)),Theme.Accent,TextFormatFlags.NoPadding|TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g,DateTime.Now.ToString("dddd"),text,new Rectangle(S(91),S(37),Width-S(100),S(18)),Theme.Ink,TextFormatFlags.NoPadding|TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g,DateTime.Now.ToString("HH:mm"),time,new Rectangle(S(87),S(57),Width-S(95),S(33)),Theme.Ink,TextFormatFlags.NoPadding);
        TextRenderer.DrawText(g,"Calendar + notes",text,new Rectangle(S(91),S(100),Width-S(100),S(18)),Color.FromArgb(125,109,143),TextFormatFlags.NoPadding);
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,Rectangle.Inflate(ClientRectangle,-4,-4));
    }
}
