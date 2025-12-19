using System;
using camera.Data;
using camera.Factories;
using camera.Rendering;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Avalonia.Threading;
namespace camera.ViewModels;
public sealed partial class BoxVm : ObservableObject
{
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private double w;
    [ObservableProperty] private double h;
    [ObservableProperty] private string? label;
}
public partial class HomePageViewModel : PageViewModel
{
    private readonly MediaFactory _mediaFactory;
    private readonly RenderingFactory _renderingFactory;
    public event Action? FrameUpdated;
    private readonly DispatcherTimer _boxTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    
    public BoxVm Box { get; } = new BoxVm { X = 40, Y = 30, W = 160, H = 100 };

    public MediaPlayer MediaPlayer { get; }
    public MediaPlayer MediaPlayer1 { get; }
    public MediaPlayer MediaPlayer2 { get; }
    public MediaPlayer MediaPlayer3 { get; }
    public MediaPlayer MediaPlayer4 { get; }
    public MediaPlayer MediaPlayer5 { get; }
    public MediaPlayer MediaPlayer6 { get; }
    public MediaPlayer MediaPlayer7 { get; }
    public MediaPlayer MediaPlayer8 { get; }
    public MediaPlayer MediaPlayer9 { get; }
    public MediaPlayer MediaPlayer10 { get; }
    public MediaPlayer MediaPlayer11 { get; }
    public MediaPlayer MediaPlayer12 { get; }
    public MediaPlayer MediaPlayer13 { get; }
    public MediaPlayer MediaPlayer14 { get; }
    public MediaPlayer MediaPlayer15 { get; }
    
    public BitmapRender Renderer { get; }
    public BitmapRender Renderer1 { get; }
    public BitmapRender Renderer2 { get; }
    public BitmapRender Renderer3 { get; }

    [ObservableProperty]
    private WriteableBitmap? videoBitmap;
    [ObservableProperty]
    private WriteableBitmap? videoBitmap1;
    [ObservableProperty]
    private WriteableBitmap? videoBitmap2;
    [ObservableProperty]
    private WriteableBitmap? videoBitmap3;


    private readonly string _url = "rtmp://127.0.0.1:1935/substream";
    public string Test => "Hello from LibVLCSharp";

    [RelayCommand]
    private void Play1() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer, false);

    [RelayCommand]
    private void Play2() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer1, true);

    [RelayCommand]
    private void Play3() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer2, true);  

    [RelayCommand]
    private void Play4() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer3, true);
    [RelayCommand]
    private void Play5() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer4, true);

    [RelayCommand]
    private void Play6() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer5, true);

    [RelayCommand]
    private void Play7() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer6, true);

    [RelayCommand]
    private void Play8() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer7, true);
    [RelayCommand]
    private void Play9() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer8, true);

    [RelayCommand]
    private void Play10() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer9, true);

    [RelayCommand]
    private void Play11() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer10, true);

    [RelayCommand]
    private void Play12() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer11, true);
    [RelayCommand]
    private void Play13() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer12, true);
    [RelayCommand]
    private void Play14() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer13, true);
    [RelayCommand]
    private void Play15() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer14, true);
    [RelayCommand]
    private void Play16() => _mediaFactory.CreateLiveMedia(_url, MediaPlayer15, true);
    [RelayCommand]
    private void Stop() => MediaPlayer.Stop();

    public HomePageViewModel(MediaFactory mediaFactory, RenderingFactory renderingFactory)
    {
        _mediaFactory = mediaFactory;
        _renderingFactory = renderingFactory;
        MediaPlayer = _mediaFactory.CreatePlayer();
        MediaPlayer1 = _mediaFactory.CreatePlayer();
        MediaPlayer2 = _mediaFactory.CreatePlayer();
        MediaPlayer3 = _mediaFactory.CreatePlayer();
        MediaPlayer4 = _mediaFactory.CreatePlayer();
        MediaPlayer5 = _mediaFactory.CreatePlayer();
        MediaPlayer6 = _mediaFactory.CreatePlayer();
        MediaPlayer7 = _mediaFactory.CreatePlayer();
        MediaPlayer8= _mediaFactory.CreatePlayer();
        MediaPlayer9 = _mediaFactory.CreatePlayer();
        MediaPlayer10 = _mediaFactory.CreatePlayer();
        MediaPlayer11 = _mediaFactory.CreatePlayer();
        MediaPlayer12 = _mediaFactory.CreatePlayer();
        MediaPlayer13 = _mediaFactory.CreatePlayer();
        MediaPlayer14 = _mediaFactory.CreatePlayer();
        MediaPlayer15 = _mediaFactory.CreatePlayer();
        
        _boxTimer.Tick += (_, _) =>
        {
            Box.X += 20;
            if (Box.X > 400) Box.X = 40;
        };
        _boxTimer.Start();
        
        Renderer = _renderingFactory.CreateBitmapRenderer(MediaPlayer); // Attach callbacks ở đây
        Renderer.BitmapChanged += bmp => VideoBitmap = bmp;
        Renderer.FrameRendered += () => FrameUpdated?.Invoke();
        VideoBitmap = Renderer.Bitmap;
        
        Renderer1 = _renderingFactory.CreateBitmapRenderer(MediaPlayer1); // Attach callbacks ở đây
        Renderer1.BitmapChanged += bmp => VideoBitmap = bmp;
        Renderer1.FrameRendered += () => FrameUpdated?.Invoke();
        VideoBitmap1 = Renderer1.Bitmap;
        
        Renderer2 = _renderingFactory.CreateBitmapRenderer(MediaPlayer2); // Attach callbacks ở đây
        Renderer2.BitmapChanged += bmp => VideoBitmap = bmp;
        Renderer2.FrameRendered += () => FrameUpdated?.Invoke();
        VideoBitmap2 = Renderer2.Bitmap;
        
        Renderer3 = _renderingFactory.CreateBitmapRenderer(MediaPlayer3); // Attach callbacks ở đây
        Renderer3.BitmapChanged += bmp => VideoBitmap = bmp;
        Renderer3.FrameRendered += () => FrameUpdated?.Invoke();
        VideoBitmap3 = Renderer3.Bitmap;
        
        PageName = ApplicationPageNames.Home;
    }
}