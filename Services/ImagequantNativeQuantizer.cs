using System.Runtime.InteropServices;
using ImageMinify.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace ImageMinify.Services;

public sealed class ImagequantNativeQuantizer
{
    private readonly object _syncRoot = new();
    private NativeApi? _api;

    public bool IsAvailable => EnsureApi() is not null;

    public bool TryCreateQuantizer(Image<Rgba32> image, int quality, out IQuantizer? quantizer)
    {
        quantizer = null;
        var api = EnsureApi();
        if (api is null)
        {
            return false;
        }

        var pixelData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixelData);

        var pinnedPixels = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
        IntPtr attr = IntPtr.Zero;
        IntPtr liqImage = IntPtr.Zero;
        IntPtr result = IntPtr.Zero;

        try
        {
            attr = api.AttrCreate();
            if (attr == IntPtr.Zero)
            {
                return false;
            }

            api.SetMaxColors(attr, CalculateMaxColors(quality));
            api.SetQuality(attr, CalculateMinQuality(quality), 100);
            api.SetSpeed(attr, 3);

            liqImage = api.ImageCreateRgba(attr, pinnedPixels.AddrOfPinnedObject(), image.Width, image.Height, 0D);
            if (liqImage == IntPtr.Zero)
            {
                return false;
            }

            result = api.QuantizeImage(attr, liqImage);
            if (result == IntPtr.Zero)
            {
                return false;
            }

            api.SetDitheringLevel(result, 1F);
            var palette = api.ReadPalette(result);
            if (palette.Length == 0)
            {
                return false;
            }

            quantizer = new PaletteQuantizer(
                palette,
                new QuantizerOptions
                {
                    MaxColors = palette.Length,
                    Dither = KnownDitherings.FloydSteinberg,
                    DitherScale = 1F,
                });
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (result != IntPtr.Zero)
            {
                api.ResultDestroy(result);
            }

            if (liqImage != IntPtr.Zero)
            {
                api.ImageDestroy(liqImage);
            }

            if (attr != IntPtr.Zero)
            {
                api.AttrDestroy(attr);
            }

            if (pinnedPixels.IsAllocated)
            {
                pinnedPixels.Free();
            }
        }
    }

    private NativeApi? EnsureApi()
    {
        if (_api is not null)
        {
            return _api;
        }

        lock (_syncRoot)
        {
            if (_api is not null)
            {
                return _api;
            }

            if (!NativeLibraryLoader.TryLoadOptional(out var handle, "libimagequant.dll", "imagequant.dll", "libimagequant.so"))
            {
                return null;
            }

            _api = new NativeApi(handle);
            return _api;
        }
    }

    private static int CalculateMaxColors(int quality)
    {
        return Math.Clamp(32 + (int)(224 * quality / 100D), 32, 256);
    }

    private static int CalculateMinQuality(int quality)
    {
        return Math.Clamp(quality - 20, 10, 100);
    }

    private sealed class NativeApi
    {
        private readonly IntPtr _handle;
        private readonly liq_attr_create _attrCreate;
        private readonly liq_attr_destroy _attrDestroy;
        private readonly liq_set_max_colors _setMaxColors;
        private readonly liq_set_quality _setQuality;
        private readonly liq_set_speed _setSpeed;
        private readonly liq_image_create_rgba _imageCreateRgba;
        private readonly liq_image_destroy _imageDestroy;
        private readonly liq_quantize_image _quantizeImage;
        private readonly liq_result_destroy _resultDestroy;
        private readonly liq_set_dithering_level _setDitheringLevel;
        private readonly liq_get_palette _getPalette;

        public NativeApi(IntPtr handle)
        {
            _handle = handle;
            _attrCreate = GetExport<liq_attr_create>("liq_attr_create");
            _attrDestroy = GetExport<liq_attr_destroy>("liq_attr_destroy");
            _setMaxColors = GetExport<liq_set_max_colors>("liq_set_max_colors");
            _setQuality = GetExport<liq_set_quality>("liq_set_quality");
            _setSpeed = GetExport<liq_set_speed>("liq_set_speed");
            _imageCreateRgba = GetExport<liq_image_create_rgba>("liq_image_create_rgba");
            _imageDestroy = GetExport<liq_image_destroy>("liq_image_destroy");
            _quantizeImage = GetExport<liq_quantize_image>("liq_quantize_image");
            _resultDestroy = GetExport<liq_result_destroy>("liq_result_destroy");
            _setDitheringLevel = GetExport<liq_set_dithering_level>("liq_set_dithering_level");
            _getPalette = GetExport<liq_get_palette>("liq_get_palette");
        }

        public IntPtr AttrCreate() => _attrCreate();

        public void AttrDestroy(IntPtr attr) => _attrDestroy(attr);

        public int SetMaxColors(IntPtr attr, int maxColors) => _setMaxColors(attr, maxColors);

        public int SetQuality(IntPtr attr, int minimum, int maximum) => _setQuality(attr, minimum, maximum);

        public int SetSpeed(IntPtr attr, int speed) => _setSpeed(attr, speed);

        public IntPtr ImageCreateRgba(IntPtr attr, IntPtr pixels, int width, int height, double gamma) =>
            _imageCreateRgba(attr, pixels, width, height, gamma);

        public void ImageDestroy(IntPtr image) => _imageDestroy(image);

        public IntPtr QuantizeImage(IntPtr attr, IntPtr image) => _quantizeImage(attr, image);

        public void ResultDestroy(IntPtr result) => _resultDestroy(result);

        public int SetDitheringLevel(IntPtr result, float level) => _setDitheringLevel(result, level);

        public Color[] ReadPalette(IntPtr result)
        {
            var palettePointer = _getPalette(result);
            if (palettePointer == IntPtr.Zero)
            {
                return [];
            }

            var count = Marshal.ReadInt32(palettePointer);
            if (count <= 0)
            {
                return [];
            }

            var colors = new Color[count];
            var offset = sizeof(int);

            for (var index = 0; index < count; index++)
            {
                var red = Marshal.ReadByte(palettePointer, offset + (index * 4));
                var green = Marshal.ReadByte(palettePointer, offset + (index * 4) + 1);
                var blue = Marshal.ReadByte(palettePointer, offset + (index * 4) + 2);
                var alpha = Marshal.ReadByte(palettePointer, offset + (index * 4) + 3);
                colors[index] = Color.FromRgba(red, green, blue, alpha);
            }

            return colors;
        }

        private TDelegate GetExport<TDelegate>(string exportName)
            where TDelegate : Delegate
        {
            var export = NativeLibrary.GetExport(_handle, exportName);
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(export);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr liq_attr_create();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void liq_attr_destroy(IntPtr attr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int liq_set_max_colors(IntPtr attr, int colors);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int liq_set_quality(IntPtr attr, int minimum, int maximum);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int liq_set_speed(IntPtr attr, int speed);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr liq_image_create_rgba(IntPtr attr, IntPtr pixels, int width, int height, double gamma);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void liq_image_destroy(IntPtr image);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr liq_quantize_image(IntPtr attr, IntPtr image);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void liq_result_destroy(IntPtr result);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int liq_set_dithering_level(IntPtr result, float ditherLevel);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr liq_get_palette(IntPtr result);
    }
}
