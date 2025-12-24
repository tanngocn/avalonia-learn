using System;
using LibVLCSharp.Shared;
using System.Collections.Generic;
namespace camera.Factories;

public class MediaFactory(LibVLC libVlc)
{
    private readonly LibVLC _libVlc = libVlc ?? throw new ArgumentNullException(nameof(libVlc));

    // Allow true low-latency. We'll still clamp per-protocol below.
    private const int MinCachingMs = 0;
    private const int MaxCachingMs = 1500;
    
    // SRT on localhost can run with very low buffering, but on weaker machines
    // too-aggressive caching (e.g., 0–50ms) can cause freeze/stall when decode can't keep up.
    // Keep a small-but-safe default to trade a bit of latency for stability.
    private const int MaxSrtCachingMs = 200;

    // Match VLC desktop settings (per your screenshot)
    private const int DefaultFileCachingMs = 50;
    private const int DefaultLiveCachingMs = 100;    // "Live capture caching (ms)"
    private const int DefaultNetworkCachingMs = 100; // "Network caching (ms)"
    // Note: large jitter increases latency. Keep modest by default; SRT can go lower.
    private const int DefaultClockJitterMs = 300;   // "Clock jitter"
    private int cachingMs = DefaultNetworkCachingMs;
    public MediaPlayer CreatePlayer()
    {
        var mp = new MediaPlayer(_libVlc)
        {
            // Default to SW; we toggle per tile in CreateLiveMedia for BitmapRender stability.
            EnableHardwareDecoding = false
        };
        // {
        //     EnableHardwareDecoding = true
        // };

        // Minimal diagnostics to see why playback fails (Debug output in Rider).
        mp.EncounteredError += (_, _) => System.Diagnostics.Debug.WriteLine("[MediaPlayer] EncounteredError");
        mp.Playing += (_, _) => System.Diagnostics.Debug.WriteLine("[MediaPlayer] Playing");
        mp.Buffering += (_, e) => System.Diagnostics.Debug.WriteLine($"[MediaPlayer] Buffering {e.Cache:0}%");

        return mp;
    }

    // Back-compat for older naming in some callers.
    public void CreateMedia(string url, MediaPlayer mediaPlayer, bool useHardwareDecoding)
        => CreateLiveMedia(url, mediaPlayer, useHardwareDecoding);

    public void CreateLiveMedia(string url, MediaPlayer mediaPlayer, bool useHardwareDecoding)
    {
        if (mediaPlayer is null) throw new ArgumentNullException(nameof(mediaPlayer));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required", nameof(url));

        url = url.Trim();
        var networkCaching = Math.Clamp(cachingMs, MinCachingMs, MaxCachingMs);
        var fileCaching = Math.Clamp(DefaultFileCachingMs, MinCachingMs, MaxCachingMs);
        var liveCaching = Math.Clamp(DefaultLiveCachingMs, MinCachingMs, MaxCachingMs);
        var scheme = GetScheme(url);
        var isSrt = string.Equals(scheme, "srt", StringComparison.OrdinalIgnoreCase);

        // Prefer ultra-low buffering for SRT (when the producer is configured for low latency).
        if (isSrt)
        {
            // Keep modest buffering to avoid "plays for a few seconds then freezes" on weak CPUs.
            networkCaching = Math.Min(networkCaching, MaxSrtCachingMs);
            fileCaching = Math.Min(fileCaching, MaxSrtCachingMs);
            liveCaching = Math.Min(liveCaching, MaxSrtCachingMs);
        }

        // For SRT, VLC Desktop accepts parameters in the URL query (e.g., latency/streamid).
        // Some LibVLC builds also accept them as input options; to maximize compatibility,
        // keep the original MRL intact AND also add explicit options below.
        var query = GetQueryString(url);
        var srtParams = isSrt
            ? ParseQuery(query)
            : null;

        // IMPORTANT: many VLC MRLs are not valid RFC URIs (e.g. udp://@239.x.x.x:1234).
        // Creating Media from the raw location string keeps compatibility with VLC/libVLC.
        var media = LooksLikeWindowsPath(url)
            ? new Media(_libVlc, url, FromType.FromPath)
            : new Media(_libVlc, url, FromType.FromLocation);
        // Allow per-tile HW decode; fallback to SW by forcing hw=none.
        mediaPlayer.EnableHardwareDecoding = useHardwareDecoding;
        if (!useHardwareDecoding)
            media.AddOption(":avcodec-hw=none");

        // Protocol-specific tuning.
        if (string.Equals(scheme, "rtsp", StringComparison.OrdinalIgnoreCase))
        {
            // RTSP over UDP is frequently blocked/unreliable on Windows; TCP is safer.
                media.AddOption(":rtsp-tcp");

            media.AddOption($":file-caching={fileCaching}");
            media.AddOption($":network-caching={networkCaching}");
            media.AddOption($":live-caching={liveCaching}");
        }
        else if (string.Equals(scheme, "rtmp", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(scheme, "rtmps", StringComparison.OrdinalIgnoreCase))
        {
            // Treat RTMP as live.
            media.AddOption(":rtmp-live=1");
            media.AddOption($":file-caching={fileCaching}");
            media.AddOption($":rtmp-caching={networkCaching}");
            media.AddOption($":network-caching={networkCaching}");
            media.AddOption($":live-caching={liveCaching}");
        }
        else
        {
            // srt/http/https/udp/...
            media.AddOption($":file-caching={fileCaching}");
            media.AddOption($":network-caching={networkCaching}");
            media.AddOption($":live-caching={liveCaching}");
        }

        // SRT-specific options (prefer explicit options over relying on the MRL query string)
        if (srtParams is not null)
        {
            if (srtParams.TryGetValue("streamid", out var streamId) && !string.IsNullOrWhiteSpace(streamId))
            {
                // MediaMTX uses streamid=read:<path> / publish:<path>
                // Support both plain and URL-encoded variants.
                var decodedStreamId = Uri.UnescapeDataString(streamId);

                // Different LibVLC/VLC builds accept different option names; adding both is safe.
                media.AddOption($":streamid={decodedStreamId}");
                media.AddOption($":srt-streamid={decodedStreamId}");
            }
        
            var srtLatencyMs = 50;
            if (srtParams.TryGetValue("latency", out var latency) && int.TryParse(latency, out var parsedLatency) && parsedLatency >= 0)
                srtLatencyMs = parsedLatency;

            // Different VLC builds use different option names; adding both is harmless (unknown options are ignored).
            media.AddOption($":srt-latency={srtLatencyMs}");
            media.AddOption($":latency={srtLatencyMs}");
        }

        // Keep latency bounded when the machine can't keep up.
        // Prefer dropping late frames over accumulating seconds of lag.
        media.AddOption(":drop-late-frames");

        // Closer to realtime (trade-off sync):
        // Note: disabling clock sync can prevent playback from starting on some live streams.
        // Keep VLC defaults unless explicitly requested.
        // Match VLC's clock jitter knob (Clock synchronisation stays Default unless you opt out below).
        // For SRT we bias toward realtime (lower jitter + disable synchro) to avoid multi-second buildup.
        media.AddOption($":clock-jitter={(isSrt ? 0 : DefaultClockJitterMs)}");
        if (isSrt)
            media.AddOption(":clock-synchro=0");
        // These can cause "freeze then jump" if timestamps are odd. Keep them off for diagnostics.
        // media.AddOption(":drop-late-frames");
        // NOTE: skip-frames can result in displaying mostly keyframes only.
        // If your encoder GOP/keyint is ~8s, it looks like "image updates every ~8s".
        // Keep it off for live view to avoid that symptom.
        
        media.AddOption(":no-audio");

        mediaPlayer.Play(media);
    }

    private static bool LooksLikeWindowsPath(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (s.StartsWith(@"\\", StringComparison.Ordinal)) return true; // UNC
        return s.Length >= 3
               && char.IsLetter(s[0])
               && s[1] == ':'
               && (s[2] == '\\' || s[2] == '/');
    }

    private static string GetScheme(string mrl)
    {
        // Quick-and-tolerant scheme detection for VLC MRLs.
        var idx = mrl.IndexOf("://", StringComparison.Ordinal);
        if (idx <= 0) return string.Empty;
        return mrl[..idx];
    }

    private static string GetQueryString(string mrl)
    {
        var q = mrl.IndexOf('?', StringComparison.Ordinal);
        if (q < 0) return string.Empty;
        var hash = mrl.IndexOf('#', q + 1);
        return hash >= 0 ? mrl[q..hash] : mrl[q..];
    }
    
    private static Dictionary<string, string> ParseQuery(string query)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query)) return dict;
        var q = query;
        if (q.StartsWith("?", StringComparison.Ordinal)) q = q[1..];
        if (q.Length == 0) return dict;

        foreach (var part in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = part.IndexOf('=');
            if (idx <= 0) continue;
            var key = part[..idx];
            var val = idx < part.Length - 1 ? part[(idx + 1)..] : string.Empty;
            dict[key] = val;
        }

        return dict;
    }
    
}