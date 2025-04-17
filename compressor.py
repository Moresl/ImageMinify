import os
from PIL import Image
import time
import io
import piexif
import tempfile
import subprocess
import shutil
import sys

class ImageCompressor:
    def __init__(self):
        self.supported_formats = ['.jpg', '.jpeg', '.png', '.bmp', '.webp']

    def is_supported_format(self, file_path):
        """Check if the file is a supported image format."""
        ext = os.path.splitext(file_path)[1].lower()
        return ext in self.supported_formats

    def get_file_size(self, file_path):
        """Get file size in bytes."""
        return os.path.getsize(file_path)

    def get_formatted_size(self, size_in_bytes):
        """Convert bytes to human-readable format."""
        for unit in ['B', 'KB', 'MB', 'GB']:
            if size_in_bytes < 1024.0:
                return f"{size_in_bytes:.2f} {unit}"
            size_in_bytes /= 1024.0
        return f"{size_in_bytes:.2f} TB"

    def compress_png_advanced(self, input_path, output_path):
        """Advanced PNG compression using multiple techniques."""
        # First, use PIL's built-in optimization
        with Image.open(input_path) as img:
            # Create a temporary file for intermediate processing
            with tempfile.NamedTemporaryFile(delete=False, suffix='.png') as temp_file:
                temp_path = temp_file.name

            # Save with PIL's optimization first
            if img.mode in ['RGBA', 'LA'] or (img.mode == 'P' and 'transparency' in img.info):
                # Image has transparency, preserve it
                img.save(temp_path, format='PNG', optimize=True, compress_level=9)
            else:
                # No transparency, we can quantize to reduce size
                if img.mode != 'P':
                    img = img.convert('RGB')
                    img = img.quantize(colors=256)  # Reduce to 256 colors
                img.save(temp_path, format='PNG', optimize=True, compress_level=9)

        # Try to use external tools if available
        try:
            # Try using pngquant for further optimization
            pngquant_cmd = ['pngquant', '--force', '--output', output_path, '--quality=80-100', '--speed=1', temp_path]

            # On Windows, we need to handle the command differently
            if sys.platform == 'win32':
                # Check if pngquant is installed and in PATH
                try:
                    subprocess.run(['where', 'pngquant'], check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
                    subprocess.run(pngquant_cmd, check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
                    os.remove(temp_path)  # Clean up temp file
                    return True
                except (subprocess.SubprocessError, FileNotFoundError):
                    # pngquant not available, fall back to temp file
                    shutil.copy(temp_path, output_path)
                    os.remove(temp_path)  # Clean up temp file
                    return True
            else:
                # For non-Windows platforms
                try:
                    subprocess.run(pngquant_cmd, check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
                    os.remove(temp_path)  # Clean up temp file
                    return True
                except (subprocess.SubprocessError, FileNotFoundError):
                    # pngquant not available, fall back to temp file
                    shutil.copy(temp_path, output_path)
                    os.remove(temp_path)  # Clean up temp file
                    return True
        except Exception as e:
            print(f"Error in advanced PNG compression: {e}")
            # If anything goes wrong, make sure we clean up and provide a valid output
            if os.path.exists(temp_path):
                shutil.copy(temp_path, output_path)
                os.remove(temp_path)
            else:
                # If temp file doesn't exist, just copy the original
                shutil.copy(input_path, output_path)
            return False

    def compress_image(self, input_path, output_path=None, output_format='original', quality=85):
        """Compress a single image with optional format conversion."""
        if not self.is_supported_format(input_path):
            return None

        # Determine output extension based on selected format
        input_filename, input_ext = os.path.splitext(input_path)

        if output_format == 'original':
            output_ext = input_ext
        elif output_format == 'jpeg':
            output_ext = '.jpg'
        elif output_format == 'png':
            output_ext = '.png'
        elif output_format == 'webp':
            output_ext = '.webp'
        else:
            output_ext = input_ext  # Default to original if invalid format

        if output_path is None:
            # Create output path in the same directory with '_compressed' suffix
            output_path = f"{input_filename}_compressed{output_ext}"

        try:
            # Get original file size
            original_size = self.get_file_size(input_path)

            # Open the image
            with Image.open(input_path) as img:
                # Convert image mode if needed
                if output_format == 'jpeg' and img.mode in ('RGBA', 'LA'):
                    # JPEG doesn't support alpha channel, convert to RGB
                    background = Image.new('RGB', img.size, (255, 255, 255))
                    if img.mode == 'RGBA':
                        background.paste(img, mask=img.split()[3])  # Use alpha channel as mask
                    else:  # LA mode
                        background.paste(img, mask=img.split()[1])  # Use alpha channel as mask
                    img = background

                # Apply optimized compression based on output format
                if output_ext in ['.jpg', '.jpeg']:
                    # For JPEG, we'll strip metadata but keep quality high
                    try:
                        # Try to remove EXIF data to reduce file size
                        if input_ext.lower() in ['.jpg', '.jpeg']:
                            exif_dict = piexif.load(input_path)
                            # Keep only essential EXIF data
                            for ifd in ("0th", "Exif", "GPS", "1st"):
                                if ifd in exif_dict:
                                    for key in list(exif_dict[ifd].keys()):
                                        # Keep only essential tags
                                        if key not in [271, 272, 274, 305, 306]:
                                            del exif_dict[ifd][key]
                            exif_bytes = piexif.dump(exif_dict)
                            img.save(output_path, format='JPEG', quality=quality, optimize=True, exif=exif_bytes)
                        else:
                            # No EXIF data for non-JPEG inputs
                            img.save(output_path, format='JPEG', quality=quality, optimize=True)
                    except Exception as e:
                        # If EXIF processing fails, just optimize
                        print(f"EXIF processing error: {e}")
                        img.save(output_path, format='JPEG', quality=quality, optimize=True)

                elif output_ext == '.png':
                    # Close the image before advanced PNG compression
                    img.close()
                    # Use our advanced PNG compression method
                    self.compress_png_advanced(input_path, output_path)
                    # Reopen the image for any further processing
                    img = Image.open(output_path)

                elif output_ext == '.webp':
                    # For WebP, use lossless mode for best quality
                    img.save(output_path, format='WEBP', lossless=True, quality=100)

                else:
                    # For other formats, just use optimize
                    img.save(output_path, optimize=True)

            # Get compressed file size
            compressed_size = self.get_file_size(output_path)

            # Calculate compression ratio
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
        except Exception as e:
            print(f"Error compressing {input_path}: {e}")
            return {
                'original_path': input_path,
                'success': False,
                'error': str(e)
            }

    def compress_directory(self, directory_path, output_format='original', quality=85, callback=None):
        """Compress all supported images in a directory."""
        results = []

        # Get all files in the directory
        files = [os.path.join(directory_path, f) for f in os.listdir(directory_path)
                if os.path.isfile(os.path.join(directory_path, f))]

        # Filter for supported image formats
        image_files = [f for f in files if self.is_supported_format(f)]

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

            # Call the callback function with progress information
            if callback:
                callback(processed_files, total_files, result)

        # Calculate overall statistics
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
