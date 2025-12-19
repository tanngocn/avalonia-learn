using System;
using camera.Rendering;
using LibVLCSharp.Shared;

namespace camera.Factories;

public class RenderingFactory
{
    public BitmapRender CreateBitmapRenderer(MediaPlayer mediaPlayer)
    {
        if (mediaPlayer is null) throw new ArgumentNullException(nameof(mediaPlayer));

        var renderer = new BitmapRender(mediaPlayer);
        renderer.Attach();
        return renderer;
    }
}