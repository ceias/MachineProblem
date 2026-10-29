using DreamOS.Core;
using DreamOS.WinForms.Controls;

namespace DreamOS.WinForms.Forms;
public sealed class LoginForm : RetroForm
{
    private readonly TextBox username = new() { Width = 250, MaxLength = 60, AccessibleName = "Demo username" };
    private readonly TextBox password = new() { Width = 250, UseSystemPasswordChar = true, MaxLength = 100, AccessibleName = "Demo password" };
    public string Username => username.Text.Trim();
    public event EventHandler? Authenticated;
    public LoginForm() : base("Dream OS • sign in")
    {
        Size = new(580, 640); MinimumSize = new(500, 560); MaximizeBox = false; BackColor = Theme.Pink;
        var body = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new(65, 24, 20, 20) };
        body.Controls.Add(new PixelPortrait { Size = new(165, 165) });
        body.Controls.Add(new Label { Text = "♥ DREAM OS ♥", Font = new("Consolas", 25, FontStyle.Bold), AutoSize = true, ForeColor = Theme.Accent });
        body.Controls.Add(Theme.Label("Small programs. Big dreams.\nYour own little pastel desktop."));
        body.Controls.Add(Theme.Label("Username (any demo name)")); body.Controls.Add(username);
        body.Controls.Add(Theme.Label("Password")); body.Controls.Add(password);
        var submit = Theme.Button("Enter Dream OS", () =>
        {
            if (!new DemoAuthService().Authenticate(username.Text, password.Text)) { MessageBox.Show(this, "Enter a username and the demo password: dream10", "Demo sign in", MessageBoxButtons.OK, MessageBoxIcon.Information); password.SelectAll(); password.Focus(); return; }
            Authenticated?.Invoke(this, EventArgs.Empty);
        });
        body.Controls.Add(submit); body.Controls.Add(Theme.Label("Local classroom demo • password: dream10\nThis is not secure account authentication."));
        Controls.Add(body); AcceptButton = submit;
    }
}
