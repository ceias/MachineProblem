using System.Drawing.Drawing2D;

namespace DreamOS.WinForms.Controls;

public static class Theme
{
    public static readonly Color Pink = Color.FromArgb(247, 195, 214), Cream = Color.FromArgb(255, 245, 240), Blue = Color.FromArgb(156, 201, 238), Ink = Color.FromArgb(41, 40, 61), Accent = Color.FromArgb(168, 55, 99);
    public static Button Button(string text, Action action)
    {
        var b = new Button { Text = text, AutoSize = true, MinimumSize = new(100, 34), BackColor = Pink, ForeColor = Ink, FlatStyle = FlatStyle.Flat, Margin = new(5), Padding = new(8, 3, 8, 3), UseVisualStyleBackColor = false };
        b.FlatAppearance.BorderColor = Accent; b.Click += (_, _) => action(); return b;
    }
    public static Label Label(string text) => new() { Text = text, AutoSize = true, MaximumSize = new(680, 0), Margin = new(5, 8, 5, 8), ForeColor = Ink };
    public static ComboBox Choice(IEnumerable<string> items)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 245, Margin = new(5), AccessibleName = "Selection" }; c.Items.AddRange(items.Cast<object>().ToArray()); return c;
    }
    public static ComboBox Choice<T>() where T : struct, Enum => Choice(Enum.GetNames<T>());
    public static T Selected<T>(ComboBox c) where T : struct, Enum => c.SelectedItem is string s && Enum.TryParse<T>(s, out var v) ? v : throw new ArgumentException($"Select {c.AccessibleName}.");
    public static NumericUpDown Number(decimal min, decimal max, decimal value = 0, int decimals = 0) => new() { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), DecimalPlaces = decimals, Width = 160, Margin = new(5), ThousandsSeparator = true };
    public static void Guard(IWin32Window owner, Action action)
    {
        try { action(); }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException or System.ComponentModel.Win32Exception)
        { MessageBox.Show(owner, ex.Message, "Dream OS • please check", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}

public class RetroForm : Form
{
    public RetroForm(string title)
    {
        Text = title; Font = new("Segoe UI", 10); BackColor = Theme.Cream; ForeColor = Theme.Ink;
        AutoScaleMode = AutoScaleMode.Dpi; Size = new(760, 650); MinimumSize = new(560, 420); StartPosition = FormStartPosition.CenterParent;
    }
}
public class ExerciseForm : RetroForm
{
    protected FlowLayoutPanel Body { get; } = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new(16) };
    protected Label Output { get; } = new() { AutoSize = true, MaximumSize = new(650, 0), BackColor = Color.FromArgb(255, 236, 246), Padding = new(12), Margin = new(5, 14, 5, 10), Text = "Ready when you are." };
    protected ExerciseForm(string title) : base(title) { Controls.Add(Body); Body.Controls.Add(Theme.Label(title.ToUpperInvariant())); }
    protected void Field(string label, Control control)
    {
        control.AccessibleName = label;
        var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new(0), Padding = new(0) };
        row.Controls.Add(new Label { Text = label, Width = 180, Height = 35, TextAlign = ContentAlignment.MiddleLeft }); row.Controls.Add(control); Body.Controls.Add(row);
    }
    protected void Info(string text) => Body.Controls.Add(Theme.Label(text));
    protected void Actions(params (string Text, Action Action)[] actions)
    {
        var row = new FlowLayoutPanel { AutoSize = true, MaximumSize = new(670, 0) };
        foreach (var a in actions) row.Controls.Add(Theme.Button(a.Text, () => Theme.Guard(this, a.Action)));
        row.Controls.Add(Theme.Button("Close", Close)); Body.Controls.Add(row); Body.Controls.Add(Output);
    }
    protected void ResetOutput() => Output.Text = "Ready when you are.";
}

public sealed class PixelPortrait : Control
{
    private readonly Image? portrait;
    public PixelPortrait()
    {
        DoubleBuffered = true;
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Guide.jpg");
        if (File.Exists(path)) { using var source = Image.FromFile(path); portrait = new Bitmap(source); }
        Size = new(145, 150); AccessibleName = "Dream guide, girl in red with her teddy bear";
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); if (portrait is null) { TextRenderer.DrawText(e.Graphics, "♥", Font, ClientRectangle, Theme.Accent); return; }
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor; e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        float scale = Math.Min((float)Width / portrait.Width, (float)Height / portrait.Height);
        var size = new Size((int)(portrait.Width * scale), (int)(portrait.Height * scale));
        e.Graphics.DrawImage(portrait, new Rectangle((Width-size.Width)/2, (Height-size.Height)/2, size.Width, size.Height));
    }
    protected override void Dispose(bool disposing) { if (disposing) portrait?.Dispose(); base.Dispose(disposing); }
}
public sealed class GuideControl : UserControl
{
    private readonly Label speech = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new(12), AutoEllipsis = true };
    public GuideControl()
    {
        Height = 150; Width = 340; BackColor = Color.FromArgb(255, 230, 240); BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(speech); Controls.Add(new PixelPortrait { Dock = DockStyle.Left, Width = 125 }); Say("Welcome, dreamer! Hover or focus an app to learn what it does.");
    }
    public void Say(string text) { speech.Text = text; speech.AccessibleName = "Guide: " + text; }
    public void Register(Control control, string text) { control.MouseEnter += (_, _) => Say(text); control.Enter += (_, _) => Say(text); }
}
