using DreamOS.WinForms.Forms;

namespace DreamOS.WinForms;
internal static class Program
{
    [STAThread]
    private static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new DreamSession()); }
}
internal sealed class DreamSession : ApplicationContext
{
    public DreamSession() => ShowLogin();
    private void ShowLogin()
    {
        var login = new LoginForm(); MainForm = login;
        login.Authenticated += (_, _) =>
        {
            var desktop = new DesktopForm(login.Username); MainForm = desktop;
            desktop.FormClosed += (_, _) => { if (desktop.LogoutRequested) ShowLogin(); };
            desktop.Show(); login.Close(); login.Dispose();
        };
        login.Show();
    }
    protected override void OnMainFormClosed(object? sender, EventArgs e)
    {
        if (sender is DesktopForm { LogoutRequested: true }) return;
        base.OnMainFormClosed(sender, e);
    }
}
