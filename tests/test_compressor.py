import unittest
import os
import tempfile
import shutil
from PIL import Image
import sys

# Add parent directory to path to import compressor module
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from compressor import ImageCompressor

class TestImageCompressor(unittest.TestCase):
    def setUp(self):
        # Create a temporary directory for test images
        self.test_dir = tempfile.mkdtemp()
        
        # Create a test image
        self.test_image_path = os.path.join(self.test_dir, "test.jpg")
        img = Image.new('RGB', (100, 100), color='red')
        img.save(self.test_image_path)
        
        # Initialize compressor
        self.compressor = ImageCompressor()
    
    def tearDown(self):
        # Remove temporary directory
        shutil.rmtree(self.test_dir)
    
    def test_is_supported_format(self):
        """Test if the compressor correctly identifies supported formats"""
        self.assertTrue(self.compressor.is_supported_format("test.jpg"))
        self.assertTrue(self.compressor.is_supported_format("test.jpeg"))
        self.assertTrue(self.compressor.is_supported_format("test.png"))
        self.assertTrue(self.compressor.is_supported_format("test.webp"))
        self.assertTrue(self.compressor.is_supported_format("test.bmp"))
        self.assertFalse(self.compressor.is_supported_format("test.txt"))
        self.assertFalse(self.compressor.is_supported_format("test.pdf"))
    
    def test_get_file_size(self):
        """Test if the compressor correctly gets file size"""
        size = self.compressor.get_file_size(self.test_image_path)
        self.assertGreater(size, 0)
    
    def test_get_formatted_size(self):
        """Test if the compressor correctly formats file size"""
        self.assertEqual(self.compressor.get_formatted_size(1024), "1.00 KB")
        self.assertEqual(self.compressor.get_formatted_size(1048576), "1.00 MB")
        self.assertEqual(self.compressor.get_formatted_size(500), "500.00 B")
    
    def test_compress_image(self):
        """Test if the compressor correctly compresses an image"""
        output_path = os.path.join(self.test_dir, "test_compressed.jpg")
        result = self.compressor.compress_image(self.test_image_path, output_path, quality=85)
        
        # Check if compression was successful
        self.assertTrue(result['success'])
        
        # Check if output file exists
        self.assertTrue(os.path.exists(output_path))
        
        # Check if compression statistics are correct
        self.assertEqual(result['original_path'], self.test_image_path)
        self.assertEqual(result['compressed_path'], output_path)
        self.assertGreater(result['original_size'], 0)
        self.assertGreater(result['compressed_size'], 0)
        
        # In most cases, compressed size should be less than or equal to original size
        # But for very small test images, this might not always be true
        # So we don't assert this condition

if __name__ == '__main__':
    unittest.main()
