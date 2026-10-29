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
        if (Load(part.File) is null) return null;
        var output = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(output); g.Clear(Color.Transparent);
        float scale = Math.Min((float)size / part.Region.Width, (float)size / part.Region.Height);
        var target = new Rectangle((size-(int)(part.Region.Width*scale))/2, (size-(int)(part.Region.Height*scale))/2, (int)(part.Region.Width*scale), (int)(part.Region.Height*scale));
        Draw(g, part.File, part.Region, target);
        return output;
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
        using var gradient = new LinearGradientBrush(bounds, login ? Color.FromArgb(255,188,215) : Color.FromArgb(146,198,236), Color.FromArgb(255,180,214), LinearGradientMode.Vertical);
        g.FillRectangle(gradient,bounds);
        // Clean sky/cloud regions: exclude the reference's shortcuts, window, text and taskbar.
        int bandHeight = Math.Max(100,bounds.Height / 3);
        for (int x = bounds.Left; x < bounds.Right; x += 410)
            Draw(g,"Desktop.png",new(82,244,205,49),new(x,bounds.Bottom-bandHeight,410,bandHeight));
        for (int x = bounds.Left-80; x < bounds.Right; x += 540)
            Draw(g,"Desktop.png",new(82,20,270,43),new(x,bounds.Top,540,86));
        // Login's blush tint preserves the art while matching the pink reference.
        if (login) { using var blush = new SolidBrush(Color.FromArgb(135,255,188,215)); g.FillRectangle(blush,bounds); }
        using var star = new SolidBrush(Color.FromArgb(235,255,247,236));
        using var pink = new SolidBrush(Color.FromArgb(185,255,110,167));
        for (int i = 0; i < 26; i++)
        {
            int x = bounds.Left + (i*137+29) % Math.Max(1,bounds.Width-18), y = bounds.Top + (i*83+23) % Math.Max(1,bounds.Height-18), r = i%3+3;
            g.FillRectangle(i%4==0 ? pink : star,x-r,y,r*2+1,2); g.FillRectangle(i%4==0 ? pink : star,x,y-r,2,r*2+1);
        }
    }
}

public sealed class WallpaperPanel : Panel
{
    [System.ComponentModel.DefaultValue(false)]
    public bool LoginStyle { get; set; }
    public WallpaperPanel() { DoubleBuffered = true; ResizeRedraw = true; }
    protected override void OnPaintBackground(PaintEventArgs e) => Artwork.Wallpaper(e.Graphics, ClientRectangle, LoginStyle);
}

public sealed class HeartBadge : Control
{
    public HeartBadge() { DoubleBuffered = true; SetStyle(ControlStyles.SupportsTransparentBackColor,true); BackColor = Color.Transparent; }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); Artwork.Draw(e.Graphics,"Login.png",new(120,60,68,69),ClientRectangle); }
}
