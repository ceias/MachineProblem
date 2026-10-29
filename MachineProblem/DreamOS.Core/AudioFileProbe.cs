namespace DreamOS.Core;

public enum AudioContainer { Wave, Mp3, Mp4, Aac, Aiff, Wma, Ogg, WebM, Flac, Unknown }
public static class AudioFileProbe
{
    public static AudioContainer Identify(ReadOnlySpan<byte> header)
    {
        if(header.Length>=12&&(header[..4].SequenceEqual("RIFF"u8)||header[..4].SequenceEqual("RF64"u8))&&header.Slice(8,4).SequenceEqual("WAVE"u8))return AudioContainer.Wave;
        if(header.Length>=3&&header[..3].SequenceEqual("ID3"u8))return AudioContainer.Mp3;
        if(header.Length>=2&&header[0]==0xff&&(header[1]&0xe0)==0xe0&&(header[1]&6)!=0)return AudioContainer.Mp3;
        if(header.Length>=8&&header.Slice(4,4).SequenceEqual("ftyp"u8))return AudioContainer.Mp4;
        if(header.Length>=2&&header[0]==0xff&&(header[1]&0xf6)==0xf0)return AudioContainer.Aac;
        if(header.Length>=12&&header[..4].SequenceEqual("FORM"u8)&&(header.Slice(8,4).SequenceEqual("AIFF"u8)||header.Slice(8,4).SequenceEqual("AIFC"u8)))return AudioContainer.Aiff;
        if(header.Length>=8&&header[..8].SequenceEqual(new byte[]{0x30,0x26,0xb2,0x75,0x8e,0x66,0xcf,0x11}))return AudioContainer.Wma;
        if(header.Length>=4&&header[..4].SequenceEqual("OggS"u8))return AudioContainer.Ogg;
        if(header.Length>=4&&header[..4].SequenceEqual(new byte[]{0x1a,0x45,0xdf,0xa3}))return AudioContainer.WebM;
        if(header.Length>=4&&header[..4].SequenceEqual("fLaC"u8))return AudioContainer.Flac;
        return AudioContainer.Unknown;
    }
}
