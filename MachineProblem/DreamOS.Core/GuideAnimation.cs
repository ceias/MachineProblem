namespace DreamOS.Core;

public sealed record GuideFrame(double X,double Y,double Rotation,double PropY,string Effect);
public static class GuideAnimation
{
    public static IReadOnlyList<string> Activities { get; }=Array.AsReadOnly(new[]{"pizza","lotto","tuition","payroll","hotel","election","password","store","about","widgets","notification","welcome"});
    public static GuideFrame Sample(string activity,double seconds,bool paused=false)
    {
        if(paused||seconds<0||seconds>=3)return new(0,0,0,0,"idle");
        double fade=Math.Sin(Math.PI*seconds/3),wave=Math.Sin(seconds*6),fast=Math.Sin(seconds*10);
        return activity switch
        {
            "pizza"=>new(3*wave*fade,-5*Math.Abs(wave)*fade,6*wave*fade,-24*Math.Abs(fast)*fade,"toss"),
            "lotto"=>new(0,-13*Math.Abs(fast)*fade,2*wave*fade,-8*wave*fade,"lucky"),
            "tuition"=>new(2*wave*fade,-2*fast*fade,3*fast*fade,0,"study"),
            "payroll"=>new(0,-2*Math.Abs(wave)*fade,-4*wave*fade,-3*fast*fade,"write"),
            "hotel"=>new(5*wave*fade,3*Math.Abs(wave)*fade,8*wave*fade,-2*wave*fade,"greet"),
            "election"=>new(3*fast*fade,0,4*wave*fade,-12*Math.Abs(wave)*fade,"wave"),
            "password"=>new(7*wave*fade,0,-3*wave*fade,0,"peek"),
            "store"=>new(6*wave*fade,-6*Math.Abs(wave)*fade,2*wave*fade,-4*Math.Abs(wave)*fade,"carry"),
            "about"=>new(2*wave*fade,-4*Math.Abs(wave)*fade,5*wave*fade,-8*wave*fade,"heart"),
            "widgets"=>new(8*wave*fade,-3*Math.Abs(wave)*fade,6*wave*fade,-6*fast*fade,"dance"),
            "notification"=>new(0,-9*Math.Abs(fast)*fade,0,-6*wave*fade,"alert"),
            _=>new(3*wave*fade,-3*Math.Abs(wave)*fade,2*wave*fade,0,"hello")
        };
    }
}
public sealed record ReceiptPage(int Count,bool Final);
public static class ReceiptPagination
{
    public static ReceiptPage Plan(int remaining,float height,float header,float row,float finalFooter,float continuationFooter)
    {
        if(remaining<0||row<=0||height<header+finalFooter+row)throw new ArgumentException("The printable area is too small. Increase the paper height or reduce the printer margins.");
        if(header+remaining*row+finalFooter<=height)return new(remaining,true);
        int count=Math.Min(remaining,(int)Math.Floor((height-header-continuationFooter)/row));
        if(count<1)throw new ArgumentException("The printable area cannot fit a receipt row.");
        return new(count,false);
    }
}
