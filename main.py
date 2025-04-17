import sys
from PyQt5.QtWidgets import QApplication
from PyQt5.QtGui import QIcon
from ui import ImageCompressorUI

def main():
    app = QApplication(sys.argv)
    # 设置应用程序图标
    app.setWindowIcon(QIcon("icon.ico"))
    window = ImageCompressorUI()
    window.show()
    sys.exit(app.exec_())

if __name__ == "__main__":
    main()
