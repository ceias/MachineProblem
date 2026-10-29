namespace DreamOS.WinForms.Controls;

public interface IDialogGuideHost { void PresentDialogGuide(string message); }

public sealed class DreamDialog : RetroForm
{
    private readonly DialogResult dismiss;
    public DreamDialog() : this("Dream OS","A little notification.",MessageBoxButtons.OK,MessageBoxIcon.Information) { }
    private DreamDialog(string title,string text,MessageBoxButtons buttons,MessageBoxIcon icon) : base(title)
    {
        ShowInTaskbar=false; MinimizeBox=MaximizeBox=false; MinimumSize=new(440,270); Size=new(520,340); StartPosition=FormStartPosition.CenterParent;
        dismiss=buttons switch { MessageBoxButtons.OK=>DialogResult.OK,MessageBoxButtons.YesNo=>DialogResult.No,MessageBoxButtons.AbortRetryIgnore=>DialogResult.Abort,_=>DialogResult.Cancel };
        var content=new Panel { Dock=DockStyle.Fill,Padding=new(17),BackColor=Theme.Cream };
        var card=new IconCardPanel { Dock=DockStyle.Fill,Padding=new(16) };
        var heading=WorkflowUi.Text(title,15,true); heading.AutoSize=false; heading.Dock=DockStyle.Top; heading.Height=38; heading.ForeColor=Theme.Accent;
        var message=new TextBox { Text=text,Multiline=true,ReadOnly=true,BorderStyle=BorderStyle.None,Dock=DockStyle.Fill,BackColor=Color.FromArgb(255,251,249),ForeColor=Theme.Ink,ScrollBars=ScrollBars.Vertical,Font=new("Tahoma",10),AccessibleName=title+" message",TabStop=true };
        var symbol=new DialogSymbol(icon) { Dock=DockStyle.Left,Width=68 };
        var body=new Panel { Dock=DockStyle.Fill,BackColor=Color.FromArgb(255,251,249) }; body.Controls.Add(message); body.Controls.Add(symbol);
        card.Controls.Add(body); card.Controls.Add(heading); content.Controls.Add(card);
        var actions=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=54,FlowDirection=FlowDirection.RightToLeft,Padding=new(10,3,10,3),BackColor=Theme.Pink,WrapContents=false };
        var options=buttons switch
        {
            MessageBoxButtons.YesNo=>new[]{("No",DialogResult.No),("Yes",DialogResult.Yes)},
            MessageBoxButtons.YesNoCancel=>new[]{("Cancel",DialogResult.Cancel),("No",DialogResult.No),("Save",DialogResult.Yes)},
            MessageBoxButtons.RetryCancel=>new[]{("Cancel",DialogResult.Cancel),("Try again",DialogResult.Retry)},
            MessageBoxButtons.OKCancel=>new[]{("Cancel",DialogResult.Cancel),("OK",DialogResult.OK)},
            MessageBoxButtons.AbortRetryIgnore=>new[]{("Cancel",DialogResult.Abort),("Retry",DialogResult.Retry),("Ignore",DialogResult.Ignore)},
            _=>new[]{("Got it",DialogResult.OK)}
        };
        foreach(var (label,result) in options)
        {
            var answer=result; var button=Theme.Button(label,()=> { DialogResult=answer; Close(); }); actions.Controls.Add(button);
            if(answer==dismiss)CancelButton=button;
            if(buttons==MessageBoxButtons.YesNoCancel) { if(answer==DialogResult.Yes)AcceptButton=button; }
            else if(answer==DialogResult.OK||answer==DialogResult.No||answer==DialogResult.Retry)AcceptButton=button;
        }
        Controls.Add(content); Controls.Add(actions);
        FormClosing+=(_,_)=> { if(DialogResult==DialogResult.None)DialogResult=dismiss; };
    }
    public static DialogResult Show(IWin32Window? owner,string text,string title="Dream OS",MessageBoxButtons buttons=MessageBoxButtons.OK,MessageBoxIcon icon=MessageBoxIcon.Information)
    {
        using var dialog=new DreamDialog(title,text,buttons,icon);
        if(owner is Control control)
        {
            owner=control.TopLevelControl??control.FindForm()??control;
            if(owner is IDialogGuideHost host)host.PresentDialogGuide(text);
        }
        return owner is null?dialog.ShowDialog():dialog.ShowDialog(owner);
    }
    private sealed class DialogSymbol(MessageBoxIcon icon) : Control
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); var r=new Rectangle(3,6,47,47); using var fill=new SolidBrush(Color.FromArgb(250,221,234)); using var pen=new Pen(Theme.Accent,2); e.Graphics.FillEllipse(fill,r); e.Graphics.DrawEllipse(pen,r);
            using var font=new Font("Tahoma",24,FontStyle.Bold); string glyph=icon is MessageBoxIcon.Warning or MessageBoxIcon.Error?"!":icon==MessageBoxIcon.Question?"?":"♥";
            TextRenderer.DrawText(e.Graphics,glyph,font,r,Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
        }
    }
}
