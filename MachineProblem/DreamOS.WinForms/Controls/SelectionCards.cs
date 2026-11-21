namespace DreamOS.WinForms.Controls;

/// <summary>Keyboard-accessible single selection using illustrated native buttons.</summary>
public sealed class SelectionCards<T> : FlowLayoutPanel
{
    private readonly T[] values;
    private readonly List<ChoiceCard> cards=[];
    private int selected=-1;
    public event EventHandler? SelectedIndexChanged;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex { get=>selected; set { selected=value; for(int i=0;i<cards.Count;i++)cards[i].SelectCard(i==value); SelectedIndexChanged?.Invoke(this,EventArgs.Empty); } }
    public T SelectedValue => selected>=0 ? values[selected] : throw new ArgumentException("Choose an option for "+AccessibleName+".");
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text { get=>selected>=0&&cards is not null&&selected<cards.Count?cards[selected].Text:""; set=>base.Text=value; }
    public SelectionCards(T[] values,string[] labels,string[] details,int icon=30)
    {
        this.values=values; Width=330; AutoSize=true; AutoSizeMode=AutoSizeMode.GrowAndShrink; MaximumSize=new(340,0); WrapContents=true; Margin=new(0,0,0,10);
        for(int i=0;i<values.Length;i++) { int index=i; int art=icon switch {
            30=>40+i, 31=>43+i, 32=>46, 34=>49,
            33=>labels[i]=="Cash"?46:labels[i]=="Check"?47:48,
            _=>icon }; var card=new ChoiceCard(labels[i],details[i],art,()=>SelectedIndex=index) { Size=new(100,100),TabIndex=i }; cards.Add(card); Controls.Add(card); }
    }
}

public sealed class InformationCard : IconCardPanel
{
    public InformationCard(string title,string body)
    {
        Width=300; Height=104; Padding=new(12); Margin=new(0,8,0,12);
        var heading=WorkflowUi.Text(title,10,true); heading.ForeColor=Theme.Accent; heading.Dock=DockStyle.Top; heading.AutoSize=false; heading.Height=25;
        var copy=WorkflowUi.Text(body,9); copy.Dock=DockStyle.Fill; copy.AutoSize=false;
        Controls.Add(copy); Controls.Add(heading);
    }
}
