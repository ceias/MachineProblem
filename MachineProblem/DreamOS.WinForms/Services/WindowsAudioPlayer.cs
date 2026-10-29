using DreamOS.Core;
using NAudio;
using NAudio.Wave;
using System.Runtime.InteropServices;
namespace DreamOS.WinForms.Services;

/// <summary>Windows audio decoding with transactional loading: a failed file never replaces a working track.</summary>
public sealed class WindowsAudioPlayer : IDisposable
{
    private WaveStream? reader;
    private LoopStream? loop;
    private WaveOutEvent? output;
    private string? playbackError;
    public string? TakePlaybackError()=>Interlocked.Exchange(ref playbackError,null);
    public bool HasTrack=>reader is not null&&output is not null;
    public string? FileName { get; private set; }
    public bool IsLooping=>loop?.Repeating==true;
    public string State=>output?.PlaybackState.ToString()??"No track";
    public void Load(string path)
    {
        if(!File.Exists(path))throw new ArgumentException("The selected audio file no longer exists. Choose it again.");
        AudioContainer container;
        try
        {
            using var file=File.OpenRead(path); Span<byte> bytes=stackalloc byte[32]; int count=file.Read(bytes); container=AudioFileProbe.Identify(bytes[..count]);
        }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException) { throw new InvalidOperationException("The selected audio file cannot be read. Check its permissions or choose a local copy.",e); }
        if(container is AudioContainer.Unknown or AudioContainer.WebM or AudioContainer.Ogg or AudioContainer.Flac)
            throw new InvalidOperationException("This file is not in a supported audio container, or its contents do not match its filename. Export a real MP3 or PCM WAV (16-bit, 44.1 kHz). Renaming a file to .wav does not convert it.");
        WaveStream next;
        try { next=Decode(path,container); }
        catch(Exception e) when(IsDecodeError(e))
        {
            throw new InvalidOperationException("Windows could not decode this recording. It may be damaged or use an unsupported audio encoding. Re-export it as MP3 or PCM WAV (16-bit, 44.1 kHz), then choose the converted file.",e);
        }
        var nextLoop=new LoopStream(next); var nextOutput=new WaveOutEvent { DesiredLatency=150 };
        try
        {
            if(next.Length==0)throw new InvalidOperationException("The recording has no playable audio.");
            nextOutput.Init(nextLoop);
        }
        catch(Exception e) when(e is MmException or InvalidOperationException or ArgumentException or COMException)
        {
            nextOutput.Dispose(); nextLoop.Dispose(); next.Dispose();
            throw new InvalidOperationException("The recording could not be opened for playback. Check your Windows output device, or try another audio file.",e);
        }
        nextOutput.PlaybackStopped+=(sender,e)=>
        {
            if(ReferenceEquals(sender,output)&&e.Exception is not null)
                Interlocked.Exchange(ref playbackError,"Playback stopped because Windows could not read or play this recording. Check your audio device, or convert the file to MP3 / PCM WAV and choose it again.");
        };
        Release(); reader=next; loop=nextLoop; output=nextOutput; FileName=Path.GetFileName(path); Interlocked.Exchange(ref playbackError,null);
    }
    private static WaveStream Decode(string path,AudioContainer container)
    {
        try
        {
            // Media Foundation detects container contents, including mislabeled MP4/AAC downloads.
            if(container is AudioContainer.Mp4 or AudioContainer.Aac or AudioContainer.Wma)return new MediaFoundationReader(path);
            return new AudioFileReader(path);
        }
        catch(Exception first) when(IsDecodeError(first))
        {
            return new MediaFoundationReader(path);
        }
    }
    private static bool IsDecodeError(Exception e)=>e is IOException or ArgumentException or InvalidOperationException or NotSupportedException or FormatException or COMException or MmException or TypeInitializationException or DllNotFoundException or EntryPointNotFoundException;
    private void Ready() { if(!HasTrack)throw new ArgumentException("Choose a playable audio file first."); }
    public void Play(bool? repeat=null)
    {
        Ready(); if(repeat.HasValue)loop!.Repeating=repeat.Value;
        if(reader!.Position>=reader.Length)reader.Position=0;
        try { output!.Play(); }
        catch(MmException e) { throw new InvalidOperationException("Windows could not start playback. Check your default audio device.",e); }
    }
    public void Pause()
    {
        Ready(); try { output!.Pause(); } catch(MmException e) { throw new InvalidOperationException("The audio device is unavailable.",e); }
    }
    public void Stop()
    {
        if(!HasTrack)return;
        loop!.Repeating=false;
        try { output!.Stop(); reader!.Position=0; } catch(MmException e) { throw new InvalidOperationException("The audio device is unavailable.",e); }
    }
    private void Release()
    {
        if(loop is not null)loop.Repeating=false;
        try { output?.Stop(); } catch(MmException) { }
        try { output?.Dispose(); } catch(MmException) { }
        loop?.Dispose(); reader?.Dispose(); reader=null; output=null; loop=null; FileName=null;
    }
    public void Dispose()=>Release();
    private sealed class LoopStream(WaveStream source) : WaveStream
    {
        public volatile bool Repeating;
        public override WaveFormat WaveFormat=>source.WaveFormat;
        public override long Length=>source.Length;
        public override long Position { get=>source.Position; set=>source.Position=value; }
        public override int Read(byte[] buffer,int offset,int count)
        {
            int read=0;
            while(read<count)
            {
                int n=source.Read(buffer,offset+read,count-read); read+=n;
                if(n>0)continue;
                if(!Repeating||source.Position==0)break;
                source.Position=0;
            }
            return read;
        }
    }
}
