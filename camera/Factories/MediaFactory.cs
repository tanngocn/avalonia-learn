using System;
using LibVLCSharp.Shared;

namespace camera.Factories;

public class MediaFactory(LibVLC libVlc)
{
    private readonly LibVLC _libVlc = libVlc ?? throw new AggregateException(nameof(libVlc));

    public MediaPlayer CreatePlayer()
    {
        var mp = new MediaPlayer(_libVlc)
        {
            EnableHardwareDecoding = false
        };
        // {
        //     EnableHardwareDecoding = true
        // };
        return mp;
    }

    public void CreateLiveMedia(string url, MediaPlayer mediaPlayer, bool useHardwareDecoding)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required", nameof(url));
        var uri = new Uri(url);
        var media = new Media(_libVlc,uri);
        if (!useHardwareDecoding)
            media.AddOption(":avcodec-hw=none"); // tránh D3D11 map fail khi dùng BitmapRender
        // Stable RTSP:
        if (string.Equals(uri.Scheme, "rtsp", StringComparison.OrdinalIgnoreCase))
        {
            // Stable RTSP (helps with NAT/Wi-Fi):
            media.AddOption(":rtsp-tcp");
        }
        else if (string.Equals(uri.Scheme, "rtmp", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(uri.Scheme, "rtmps", StringComparison.OrdinalIgnoreCase))
        {
            // Treat as LIVE RTMP (avoid seek/buffer behavior where possible)
            media.AddOption(":rtmp-live=1");
        }

        // Low-latency (tune 50..300 depending on network stability):
        
        media.AddOption(":network-caching=50");
        media.AddOption(":live-caching=50");

        // Closer to realtime (trade-off sync):
        media.AddOption(":clock-jitter=1.5");
        media.AddOption(":clock-synchro=1.5");
        media.AddOption(":drop-late-frames");
        media.AddOption(":skip-frames");
        
        media.AddOption(":no-audio");

        mediaPlayer.Play(media);
    }
}