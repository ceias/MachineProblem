using DreamOS.Core;
using DreamOS.WinForms.Controls;

namespace DreamOS.WinForms.Forms;
public sealed class LoginForm : RetroForm
{
    private readonly TextBox username=new() { BorderStyle=BorderStyle.None, MaxLength=60, AccessibleName="Username", PlaceholderText="username", Font=new("Tahoma",12) };
    private readonly TextBox password=new() { BorderStyle=BorderStyle.None, UseSystemPasswordChar=true, MaxLength=100, AccessibleName="Password", PlaceholderText="password", Font=new("Tahoma",12) };
    public string Username=>username.Text.Trim();
    public event EventHandler? Authenticated;
    public LoginForm() : base("Dream OS Login")
    {
        MinimumSize=new(480,600); ClientSize=new(550,650); MaximizeBox=false; StartPosition=FormStartPosition.CenterScreen;
        var scene=new LoginScene { Dock=DockStyle.Fill };
        var card=new Panel { Size=new(350,460), BackColor=Color.Transparent };
        var badge=new HeartBadge { Bounds=new(125,0,100,100) };
        var heading=new Label { Text="DREAM OS", Font=new("Consolas",28,FontStyle.Bold), ForeColor=Color.FromArgb(203,65,116), BackColor=Color.Transparent, TextAlign=ContentAlignment.MiddleCenter, Bounds=new(0,110,350,48) };
        var subtitle=new Label { Text="Please login to continue…", Font=new("Tahoma",11), ForeColor=Theme.Ink, BackColor=Color.Transparent, TextAlign=ContentAlignment.MiddleCenter, Bounds=new(0,164,350,25) };
        var nameField=new LoginField(username,false) { Bounds=new(15,213,320,45), TabIndex=0 };
        var passField=new LoginField(password,true) { Bounds=new(15,276,320,45), TabIndex=1 };
        var error=new Label { Bounds=new(15,392,320,40), TextAlign=ContentAlignment.MiddleCenter, ForeColor=Theme.Accent, BackColor=Color.Transparent };
        var submit=Theme.Button("Login",()=>
        {
            if(!new DemoAuthService().Authenticate(username.Text,password.Text)) { error.Text="Enter a username and password dream10."; password.SelectAll(); password.Focus(); return; }
            error.Text=""; Authenticated?.Invoke(this,EventArgs.Empty);
        });
        submit.AutoSize=false; submit.Bounds=new(78,343,194,42); submit.BackColor=Color.FromArgb(239,111,163); submit.ForeColor=Color.White; submit.Font=new("Tahoma",12,FontStyle.Bold); submit.TabIndex=2;
        card.Controls.AddRange([badge,heading,subtitle,nameField,passField,submit,error]);
        var guide=new PixelPortrait { Size=new(125,205), Anchor=AnchorStyles.Left|AnchorStyles.Bottom };
        var footer=new Label { Text="♥  Insert your dreams here…  ♥", AutoSize=false, Height=33, BackColor=Theme.Cream, ForeColor=Theme.Accent, Dock=DockStyle.Bottom, TextAlign=ContentAlignment.MiddleCenter, Font=new("Tahoma",10) };
        var hint=new Label { Text="Demo login: any name / dream10", Height=25, BackColor=Color.Transparent, ForeColor=Theme.Accent, TextAlign=ContentAlignment.MiddleCenter };
        scene.Controls.Add(card); scene.Controls.Add(guide); scene.Controls.Add(hint); scene.Controls.Add(footer); Controls.Add(scene); AcceptButton=submit;
        scene.Layout+=(_,_)=>
        {
            float scale=DeviceDpi/96f;
            card.Location=new((scene.ClientSize.Width-card.Width)/2,Math.Max(15,(scene.ClientSize.Height-footer.Height-card.Height)/2-15));
            guide.Location=new(7,scene.ClientSize.Height-footer.Height-guide.Height+12);
            hint.SetBounds((scene.ClientSize.Width-(int)(330*scale))/2,scene.ClientSize.Height-footer.Height-(int)(30*scale),(int)(330*scale),(int)(25*scale));
            guide.BringToFront();
        };
        Shown+=(_,_)=>username.Focus();
    }
    private sealed class LoginScene : Panel
    {
        public LoginScene() { DoubleBuffered=true; ResizeRedraw=true; }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Artwork.Wallpaper(e.Graphics,ClientRectangle,true);
            using var glow=new SolidBrush(Color.FromArgb(205,255,233,241));
            e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(glow,new Rectangle(Width/2-220,25,440,Math.Max(480,Height-50)));
        }
    }
    private sealed class LoginField : Panel
    {
        private readonly bool locked;
        public LoginField(TextBox input,bool locked)
        {
            this.locked=locked; BackColor=Color.White; Padding=new(42,12,8,6); Controls.Add(input); input.Dock=DockStyle.Fill; TabStop=false; DoubleBuffered=true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); using var pen=new Pen(Color.FromArgb(220,106,150),2); e.Graphics.DrawRectangle(pen,1,1,Width-3,Height-3);
            int x=16,y=Height/2;
            if(locked) { e.Graphics.DrawArc(pen,x-1,y-12,11,15,180,180); e.Graphics.DrawRectangle(pen,x-4,y-3,17,15); e.Graphics.DrawLine(pen,x+4,y+1,x+4,y+7); }
            else { e.Graphics.DrawEllipse(pen,x-1,y-13,11,11); e.Graphics.DrawArc(pen,x-6,y+1,22,15,180,180); e.Graphics.DrawLine(pen,x-6,y+8,x+16,y+8); }
        }
    }
}
