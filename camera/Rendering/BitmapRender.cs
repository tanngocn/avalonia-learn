using System;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace camera.Rendering;

/// <summary>
/// Renders LibVLC video frames into an Avalonia <see cref="WriteableBitmap"/> using LibVLC video callbacks.
/// </summary>
public sealed class BitmapRender : IDisposable
{
    private readonly MediaPlayer _mediaPlayer;
    private WriteableBitmap? _bitmap;

    private uint _width;
    private uint _height;
    private uint _pitch;

    private IntPtr _buffer = IntPtr.Zero;
    private int _renderScheduled;
    private bool _disposed;

    private byte[]? _rowCopyBuffer;

    public event Action<WriteableBitmap?>? BitmapChanged;

    /// <summary>
    /// Fires after a frame has been copied into the current bitmap (i.e. per-frame repaint trigger).
    /// </summary>
    public event Action? FrameRendered;

    public WriteableBitmap? Bitmap => _bitmap;

    public BitmapRender(MediaPlayer mediaPlayer)
    {
        _mediaPlayer = mediaPlayer ?? throw new ArgumentNullException(nameof(mediaPlayer));
    }

    /// <summary>
    /// Hooks video callbacks. Must be called before playback starts.
    /// </summary>
    public void Attach()
    {
        ThrowIfDisposed();

        _mediaPlayer.SetVideoFormatCallbacks(VideoFormat, CleanupVideo);
        _mediaPlayer.SetVideoCallbacks(LockVideo, UnlockVideo, DisplayVideo);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _mediaPlayer.SetVideoCallbacks(null, null, null);
            _mediaPlayer.SetVideoFormatCallbacks(null, null);
        }
        catch
        {
            // ignore
        }

        FreeBuffer();
        _rowCopyBuffer = null;

        var bmp = _bitmap;
        _bitmap = null;
        if (bmp is not null)
        {
            Dispatcher.UIThread.Post(() => bmp.Dispose(), DispatcherPriority.Background);
        }
    }

    private uint VideoFormat(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
    {
        Marshal.Copy(new byte[] { (byte)'R', (byte)'V', (byte)'3', (byte)'2' }, 0, chroma, 4);

        _width = width;
        _height = height;
        _pitch = width * 4;

        pitches = _pitch;
        lines = _height;

        AllocateBufferIfNeeded();
        EnsureBitmapOnUIThread();

        return 1;
    }

    private void CleanupVideo(ref IntPtr opaque)
    {
        FreeBuffer();
    }

    private IntPtr LockVideo(IntPtr opaque, IntPtr planes)
    {
        if (_buffer == IntPtr.Zero)
            AllocateBufferIfNeeded();

        Marshal.WriteIntPtr(planes, _buffer);
        return IntPtr.Zero;
    }

    private void UnlockVideo(IntPtr opaque, IntPtr picture, IntPtr planes)
    {
        // no-op
    }

    private void DisplayVideo(IntPtr opaque, IntPtr picture)
    {
        if (_bitmap is null || _buffer == IntPtr.Zero) return;
        if (Interlocked.Exchange(ref _renderScheduled, 1) == 1) return;

        Dispatcher.UIThread.Post(RenderLatestFrameToBitmap, DispatcherPriority.Render);
    }

    private void EnsureBitmapOnUIThread()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_disposed) return;
            if (_width == 0 || _height == 0) return;

            if (_bitmap is null || _bitmap.PixelSize.Width != (int)_width || _bitmap.PixelSize.Height != (int)_height)
            {
                _bitmap?.Dispose();
                _bitmap = new WriteableBitmap(
                    new PixelSize((int)_width, (int)_height),
                    new Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Unpremul);

                BitmapChanged?.Invoke(_bitmap);
            }
        }, DispatcherPriority.Background);
    }

    private void RenderLatestFrameToBitmap()
    {
        try
        {
            if (_disposed) return;
            if (_bitmap is null || _buffer == IntPtr.Zero) return;

            using ILockedFramebuffer fb = _bitmap.Lock();

            var bytesPerRow = (int)Math.Min(_pitch, (uint)fb.RowBytes);
            for (var y = 0; y < fb.Size.Height; y++)
            {
                var srcRow = IntPtr.Add(_buffer, y * (int)_pitch);
                var dstRow = IntPtr.Add(fb.Address, y * fb.RowBytes);
                CopyRow(dstRow, srcRow, bytesPerRow);
            }

            // IMPORTANT: trigger View repaint per frame
            FrameRendered?.Invoke();
        }
        finally
        {
            Volatile.Write(ref _renderScheduled, 0);
        }
    }

    private void CopyRow(IntPtr dst, IntPtr src, int bytes)
    {
        if (bytes <= 0) return;

        if (OperatingSystem.IsWindows())
        {
            CopyMemory(dst, src, (UIntPtr)bytes);
            return;
        }

        _rowCopyBuffer ??= new byte[Math.Max(bytes, 4096)];
        if (_rowCopyBuffer.Length < bytes)
            _rowCopyBuffer = new byte[bytes];

        Marshal.Copy(src, _rowCopyBuffer, 0, bytes);
        Marshal.Copy(_rowCopyBuffer, 0, dst, bytes);
    }

    [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
    private static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr count);

    private void AllocateBufferIfNeeded()
    {
        if (_width == 0 || _height == 0 || _pitch == 0) return;

        var needed = checked((int)(_pitch * _height));
        if (_buffer != IntPtr.Zero) return;

        _buffer = Marshal.AllocHGlobal(needed);
    }

    private void FreeBuffer()
    {
        if (_buffer == IntPtr.Zero) return;
        Marshal.FreeHGlobal(_buffer);
        _buffer = IntPtr.Zero;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BitmapRender));
    }
}