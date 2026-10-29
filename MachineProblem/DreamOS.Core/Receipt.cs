using System.Text;
namespace DreamOS.Core;

public sealed record ReceiptEntry(string Label,string Value);
public sealed record ReceiptData(string Title,string Reference,DateTime Issued,IReadOnlyList<ReceiptEntry> Entries,string TotalLabel,string Total,string Footer)
{
    public static string NewReference(string prefix)=>prefix+"-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
    public string ToText()
    {
        var text=new StringBuilder(); text.AppendLine("DREAM OS").AppendLine(Title).AppendLine(Reference).AppendLine(Issued.ToString("yyyy-MM-dd HH:mm")).AppendLine(new string('-',42));
        foreach(var row in Entries)text.AppendLine($"{row.Label}: {row.Value}");
        return text.AppendLine(new string('-',42)).AppendLine($"{TotalLabel}: {Total}").AppendLine().AppendLine(Footer).ToString();
    }
}
