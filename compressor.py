import io
import os
import shutil
import subprocess
from pathlib import Path

try:
    import piexif
except Exception:
    piexif = None
from PIL import Image, ImageFile

ImageFile.LOAD_TRUNCATED_IMAGES = True


def _load_optional_dependencies():
    status = {
        'imagequant': False,
        'mozjpeg': False,
        'oxipng': False,
        'dither': 'Floyd-Steinberg'
    }

    imagequant_module = None
    mozjpeg_module = None

    try:
        import imagequant
        imagequant_module = imagequant
        status['imagequant'] = True
    except Exception:
        pass

    try:
        import mozjpeg_lossless_optimization
        mozjpeg_module = mozjpeg_lossless_optimization
        status['mozjpeg'] = True
    except Exception:
        pass

    oxipng_path = _resolve_oxipng_path()
    status['oxipng'] = oxipng_path is not None

    return imagequant_module, mozjpeg_module, oxipng_path, status


def _resolve_oxipng_path():
    custom_path = os.environ.get('IMG_YASUO_OXIPNG')
    if custom_path and Path(custom_path).exists():
        return Path(custom_path)

    local_candidates = [
        Path(__file__).parent / 'bin' / 'oxipng.exe',
        Path(__file__).parent / 'bin' / 'oxipng',
    ]
    for candidate in local_candidates:
        if candidate.exists():
            return candidate

    system_path = shutil.which('oxipng')
    if not system_path:
        return None

    path = Path(system_path)
    try:
        result = subprocess.run([str(path), '--version'], capture_output=True, timeout=5)
        if result.returncode == 0:
            return path
    except Exception:
        return None

    return None


IMAGEQUANT, MOZJPEG_OPT, OXIPNG_PATH, ENGINE_STATUS = _load_optional_dependencies()


class ImageCompressor:
    def __init__(self):
        self.supported_formats = ['.jpg', '.jpeg', '.png', '.bmp', '.webp']

    def is_supported_format(self, file_path):
        ext = os.path.splitext(file_path)[1].lower()
        return ext in self.supported_formats

    def get_file_size(self, file_path):
        return os.path.getsize(file_path)

    def get_formatted_size(self, size_in_bytes):
        for unit in ['B', 'KB', 'MB', 'GB']:
            if size_in_bytes < 1024.0:
                return f"{size_in_bytes:.2f} {unit}"
            size_in_bytes /= 1024.0
        return f"{size_in_bytes:.2f} TB"

    def get_capabilities(self):
        return {
            'png_quantization': 'imagequant' if ENGINE_STATUS['imagequant'] else 'Pillow fallback',
            'png_dithering': ENGINE_STATUS['dither'],
            'png_lossless': 'oxipng' if ENGINE_STATUS['oxipng'] else 'disabled',
            'jpeg_optimizer': 'MozJPEG' if ENGINE_STATUS['mozjpeg'] else 'Pillow fallback',
            'has_imagequant': ENGINE_STATUS['imagequant'],
            'has_oxipng': ENGINE_STATUS['oxipng'],
            'has_mozjpeg': ENGINE_STATUS['mozjpeg'],
        }

    def _resolve_output_ext(self, input_ext, output_format):
        if output_format == 'original':
            return input_ext
        if output_format == 'jpeg':
            return '.jpg'
        if output_format == 'png':
            return '.png'
        if output_format == 'webp':
            return '.webp'
        return input_ext

    def _to_rgb_for_jpeg(self, image):
        if image.mode in ('RGBA', 'LA', 'P'):
            if image.mode == 'P':
                image = image.convert('RGBA')

            background = Image.new('RGB', image.size, (255, 255, 255))
            if image.mode in ('RGBA', 'LA'):
                background.paste(image, mask=image.split()[-1])
            else:
                background.paste(image)
            return background

        if image.mode != 'RGB':
            return image.convert('RGB')
        return image

    def _reduced_exif_bytes(self, input_path):
        if piexif is None:
            return None
        try:
            exif_dict = piexif.load(input_path)
            keep_tags = {271, 272, 274, 305, 306}
            for ifd in ('0th', 'Exif', 'GPS', '1st'):
                if ifd in exif_dict:
                    for key in list(exif_dict[ifd].keys()):
                        if key not in keep_tags:
                            del exif_dict[ifd][key]
            return piexif.dump(exif_dict)
        except Exception:
            return None

    def _compress_jpeg(self, image, input_path, output_path, quality):
        image = self._to_rgb_for_jpeg(image)
        buffer = io.BytesIO()

        save_args = {
            'format': 'JPEG',
            'quality': quality,
            'optimize': True,
            'progressive': True,
            'subsampling': '4:2:0',
        }

        input_ext = os.path.splitext(input_path)[1].lower()
        if input_ext in ['.jpg', '.jpeg']:
            exif_bytes = self._reduced_exif_bytes(input_path)
            if exif_bytes:
                save_args['exif'] = exif_bytes

        image.save(buffer, **save_args)
        jpeg_bytes = buffer.getvalue()

        if ENGINE_STATUS['mozjpeg'] and MOZJPEG_OPT is not None:
            try:
                jpeg_bytes = MOZJPEG_OPT.optimize(jpeg_bytes)
            except Exception:
                pass

        with open(output_path, 'wb') as file_obj:
            file_obj.write(jpeg_bytes)

    def _compress_png_with_imagequant(self, image, output_path, quality):
        if not ENGINE_STATUS['imagequant'] or IMAGEQUANT is None:
            return False

        if image.mode != 'RGBA':
            image = image.convert('RGBA')

        width, height = image.size
        rgba_data = image.tobytes()

        max_colors = max(32, min(256, 32 + int(224 * quality / 100)))
        min_quality = max(10, min(100, quality - 20))

        indexed_pixels, palette = IMAGEQUANT.quantize_raw_rgba_bytes(
            rgba_data,
            width,
            height,
            dithering_level=1.0,
            max_colors=max_colors,
            min_quality=min_quality,
            max_quality=100,
        )

        palette_image = Image.frombytes('P', (width, height), indexed_pixels)

        rgb_palette = []
        alpha_palette = []
        color_count = len(palette) // 4
        for index in range(color_count):
            rgb_palette.extend([
                palette[index * 4],
                palette[index * 4 + 1],
                palette[index * 4 + 2],
            ])
            alpha_palette.append(palette[index * 4 + 3])

        padded_palette = rgb_palette + [0] * (768 - len(rgb_palette))
        palette_image.putpalette(padded_palette)

        if any(alpha != 255 for alpha in alpha_palette):
            palette_image.info['transparency'] = bytes(alpha_palette)

        palette_image.save(output_path, 'PNG', optimize=True, compress_level=9)
        return True

    def _compress_png_with_pillow(self, image, output_path, quality):
        max_colors = max(32, min(256, 32 + int(224 * quality / 100)))
        has_alpha = image.mode in ('RGBA', 'LA') or (image.mode == 'P' and 'transparency' in image.info)

        if has_alpha:
            if image.mode != 'RGBA':
                image = image.convert('RGBA')
            quantized = image.quantize(
                colors=max_colors,
                method=Image.Quantize.FASTOCTREE,
                dither=Image.Dither.FLOYDSTEINBERG,
            )
            quantized.convert('RGBA').save(output_path, 'PNG', optimize=True, compress_level=9)
            return

        if image.mode != 'RGB':
            image = image.convert('RGB')

        quantized = image.convert(
            'P',
            palette=Image.Palette.ADAPTIVE,
            colors=max_colors,
            dither=Image.Dither.FLOYDSTEINBERG,
        )
        quantized.save(output_path, 'PNG', optimize=True, compress_level=9)

    def _optimize_png_lossless(self, file_path):
        if not ENGINE_STATUS['oxipng'] or OXIPNG_PATH is None:
            return

        try:
            subprocess.run(
                [str(OXIPNG_PATH), '-o', '4', '--strip', 'all', str(file_path)],
                check=True,
                capture_output=True,
                timeout=60,
            )
        except Exception:
            pass

    def compress_png_advanced(self, input_path, output_path, quality=85):
        with Image.open(input_path) as image:
            success = False
            try:
                success = self._compress_png_with_imagequant(image, output_path, quality)
            except Exception:
                success = False

            if not success:
                self._compress_png_with_pillow(image, output_path, quality)

        self._optimize_png_lossless(output_path)
        return True

    def compress_image(self, input_path, output_path=None, output_format='original', quality=85):
        if not self.is_supported_format(input_path):
            return None

        input_filename, input_ext = os.path.splitext(input_path)
        output_ext = self._resolve_output_ext(input_ext, output_format)

        if output_path is None:
            output_path = f"{input_filename}_compressed{output_ext}"

        try:
            original_size = self.get_file_size(input_path)

            with Image.open(input_path) as image:
                if output_ext in ['.jpg', '.jpeg']:
                    self._compress_jpeg(image, input_path, output_path, quality)
                elif output_ext == '.png':
                    compression_ok = False
                    try:
                        compression_ok = self._compress_png_with_imagequant(image, output_path, quality)
                    except Exception:
                        compression_ok = False

                    if not compression_ok:
                        self._compress_png_with_pillow(image, output_path, quality)

                    self._optimize_png_lossless(output_path)
                elif output_ext == '.webp':
                    webp_image = image
                    if webp_image.mode not in ['RGB', 'RGBA']:
                        webp_image = webp_image.convert('RGBA')
                    webp_image.save(output_path, format='WEBP', quality=quality, method=6)
                else:
                    image.save(output_path, optimize=True)

            compressed_size = self.get_file_size(output_path)
            ratio = (1 - compressed_size / original_size) * 100 if original_size > 0 else 0

            return {
                'original_path': input_path,
                'compressed_path': output_path,
                'original_size': original_size,
                'compressed_size': compressed_size,
                'original_size_formatted': self.get_formatted_size(original_size),
                'compressed_size_formatted': self.get_formatted_size(compressed_size),
                'compression_ratio': ratio,
                'success': True
            }
        except Exception as error:
            print(f"Error compressing {input_path}: {error}")
            return {
                'original_path': input_path,
                'success': False,
                'error': str(error)
            }

    def compress_directory(self, directory_path, output_format='original', quality=85, callback=None):
        results = []

        files = [
            os.path.join(directory_path, filename)
            for filename in os.listdir(directory_path)
            if os.path.isfile(os.path.join(directory_path, filename))
        ]
        image_files = [file_path for file_path in files if self.is_supported_format(file_path)]

        total_files = len(image_files)
        processed_files = 0
        total_original_size = 0
        total_compressed_size = 0

        for image_path in image_files:
            result = self.compress_image(image_path, output_format=output_format, quality=quality)

            if result and result['success']:
                results.append(result)
                total_original_size += result['original_size']
                total_compressed_size += result['compressed_size']

            processed_files += 1
            if callback:
                callback(processed_files, total_files, result)

        overall_ratio = (1 - total_compressed_size / total_original_size) * 100 if total_original_size > 0 else 0
        summary = {
            'total_files': total_files,
            'processed_files': processed_files,
            'total_original_size': total_original_size,
            'total_compressed_size': total_compressed_size,
            'total_original_size_formatted': self.get_formatted_size(total_original_size),
            'total_compressed_size_formatted': self.get_formatted_size(total_compressed_size),
            'overall_compression_ratio': overall_ratio
        }

        return results, summary
