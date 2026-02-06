import unittest
import sys
import os

# Add parent directory to path to import ui module
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

# Import UI class (now requires qfluentwidgets)
try:
    from PyQt5.QtWidgets import QApplication
    from ui import ImageCompressorUI

    UI_AVAILABLE = True
except ImportError:
    UI_AVAILABLE = False

@unittest.skipIf(not UI_AVAILABLE, "PyQt5 or qfluentwidgets is not available")
class TestImageCompressorUI(unittest.TestCase):
    """Tests for the ImageCompressorUI class"""
    
    @classmethod
    def setUpClass(cls):
        """Create the application and the main window"""
        cls.app = QApplication(sys.argv)
        cls.form = ImageCompressorUI()
    
    @classmethod
    def tearDownClass(cls):
        """Close the application"""
        cls.form.close()
    
    def test_window_title(self):
        """Test if the window title is correct"""
        self.assertEqual(self.form.windowTitle(), '图片压缩工具')
    
    def test_initial_state(self):
        """Test the initial state of the UI"""
        # Compress button should be disabled initially
        self.assertFalse(self.form.compress_button.isEnabled())
        
        # Progress bar should be hidden initially
        self.assertFalse(self.form.progress_bar.isVisible())
        
        # Results table should be empty initially
        self.assertEqual(self.form.results_table.rowCount(), 0)
    
    def test_format_combo(self):
        """Test the format combo box"""
        # Check if the format combo has the correct number of items
        self.assertEqual(self.form.format_combo.count(), 4)
        
        # Check if the default format is 'original'
        self.assertEqual(self.form.format_combo.currentData(), 'original')
        
        # Change the format to JPEG and check if quality settings are enabled
        index = self.form.format_combo.findData('jpeg')
        self.form.format_combo.setCurrentIndex(index)
        self.assertEqual(self.form.format_combo.currentData(), 'jpeg')
        self.assertTrue(self.form.quality_slider.isEnabled())
        self.assertTrue(self.form.quality_value.isEnabled())
        
        # Change the format to PNG and check if quality settings are disabled
        index = self.form.format_combo.findData('png')
        self.form.format_combo.setCurrentIndex(index)
        self.assertEqual(self.form.format_combo.currentData(), 'png')
        self.assertFalse(self.form.quality_slider.isEnabled())
        self.assertFalse(self.form.quality_value.isEnabled())
    
    def test_rename_checkbox(self):
        """Test the rename checkbox"""
        # Rename settings should be disabled initially
        self.assertFalse(self.form.prefix_input.isEnabled())
        self.assertFalse(self.form.separator_combo.isEnabled())
        self.assertFalse(self.form.number_spinbox.isEnabled())
        
        # Enable rename checkbox and check if settings are enabled
        self.form.rename_checkbox.setChecked(True)
        self.assertTrue(self.form.prefix_input.isEnabled())
        self.assertTrue(self.form.separator_combo.isEnabled())
        self.assertTrue(self.form.number_spinbox.isEnabled())
        
        # Disable rename checkbox and check if settings are disabled
        self.form.rename_checkbox.setChecked(False)
        self.assertFalse(self.form.prefix_input.isEnabled())
        self.assertFalse(self.form.separator_combo.isEnabled())
        self.assertFalse(self.form.number_spinbox.isEnabled())

if __name__ == '__main__':
    unittest.main()
