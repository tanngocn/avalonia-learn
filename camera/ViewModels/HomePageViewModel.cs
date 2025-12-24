using System;
using camera.Data;
using camera.Factories;
using camera.Rendering;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace camera.ViewModels;
public sealed partial class BoxVm : ObservableObject
{
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private double w;
    [ObservableProperty] private double h;
    [ObservableProperty] private string? label;
}

public sealed partial class VideoTileVm : ObservableObject
{
    public required MediaPlayer MediaPlayer { get; init; }

    [ObservableProperty]
    private WriteableBitmap? bitmap;
}
public partial class HomePageViewModel : PageViewModel
{
    private const int TileCount = 16;

    private readonly MediaFactory _mediaFactory;
    private readonly RenderingFactory _renderingFactory;

    [ObservableProperty]
    private bool aiEnabled = true;

    public bool ShowRawVideo => !AiEnabled;
    
    private readonly DispatcherTimer _frameUpdatedDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private bool _frameUpdatePending;
    
    public ObservableCollection<VideoTileVm> Tiles { get; }
    public event Action? FrameUpdated;
    private readonly DispatcherTimer _boxTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    
    public BoxVm Box { get; } = new BoxVm { X = 40, Y = 30, W = 160, H = 100 };
    public IReadOnlyList<MediaPlayer> MediaPlayers { get; }
    public IReadOnlyList<BitmapRender> Renderers { get; }
    public ObservableCollection<WriteableBitmap?> VideoBitmaps { get; }

    // MediaMTX: viewer/consumer must use streamid=read:<path>. "publish:" is for the producer side.
    private readonly string _url = "srt://127.0.0.1:8890?streamid=read:cam1&latency=50";
    public string Test => "Hello from LibVLCSharp";
    
    [RelayCommand]
    private void Play(MediaPlayer mediaPlayer)
    {
        // Keep previous behavior: tile 0 uses software decoding for BitmapRender stability.
        var useHardwareDecoding = !ReferenceEquals(mediaPlayer, MediaPlayers[0]);
        _mediaFactory.CreateLiveMedia(_url, mediaPlayer, useHardwareDecoding);
    }
    
    [RelayCommand]
    private void Stop(MediaPlayer mediaPlayer) => mediaPlayer.Stop();

    public HomePageViewModel(MediaFactory mediaFactory, RenderingFactory renderingFactory): base(ApplicationPageNames.Home)
    {
        _mediaFactory = mediaFactory;
        _renderingFactory = renderingFactory;
        
        _frameUpdatedDebounceTimer.Tick += (_, _) =>
        {
            _frameUpdatedDebounceTimer.Stop();
            if (!_frameUpdatePending) return;
            _frameUpdatePending = false;
            FrameUpdated?.Invoke();
        };

        
        var players = new MediaPlayer[TileCount];
        for (var i = 0; i < TileCount; i++)
            players[i] = _mediaFactory.CreatePlayer();
        MediaPlayers = players;
        
        VideoBitmaps = new ObservableCollection<WriteableBitmap?>();
        Tiles = new ObservableCollection<VideoTileVm>();
        for (var i = 0; i < TileCount; i++)
        {
            VideoBitmaps.Add(null);
            Tiles.Add(new VideoTileVm { MediaPlayer = MediaPlayers[i], Bitmap = null });
        }

        var renderers = new BitmapRender[TileCount];
        for (var i = 0; i < TileCount; i++)
        {
            var idx = i; // capture
            var renderer = _renderingFactory.CreateBitmapRenderer(MediaPlayers[idx]);
            renderer.BitmapChanged += bmp =>
            {
                VideoBitmaps[idx] = bmp;
                Tiles[idx].Bitmap = bmp;
            };
        
            renderer.FrameRendered += RequestFrameUpdated;
            VideoBitmaps[idx] = renderer.Bitmap;
            Tiles[idx].Bitmap = renderer.Bitmap;
            renderers[idx] = renderer;
        }
        Renderers = renderers;
        
        _boxTimer.Tick += (_, _) =>
        {
            Box.X += 20;
            if (Box.X > 400) Box.X = 40;
        };
        _boxTimer.Start();
    }
    private void RequestFrameUpdated()
    {
        _frameUpdatePending = true;
        if (!_frameUpdatedDebounceTimer.IsEnabled)
            _frameUpdatedDebounceTimer.Start();
    }

    partial void OnAiEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowRawVideo));
    }
}