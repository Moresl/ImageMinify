from PIL import Image
import os

# 检查icon.jpg是否存在
if not os.path.exists('icon.jpg'):
    print("错误：icon.jpg文件不存在")
    exit(1)

# 打开图像
try:
    img = Image.open('icon.jpg')
    
    # 调整大小为标准图标尺寸
    sizes = [(16, 16), (32, 32), (48, 48), (64, 64), (128, 128)]
    icon_images = []
    
    for size in sizes:
        resized_img = img.resize(size, Image.LANCZOS)
        icon_images.append(resized_img)
    
    # 保存为.ico格式
    icon_images[0].save('icon.ico', format='ICO', sizes=[(img.size[0], img.size[1]) for img in icon_images])
    
    print("成功将icon.jpg转换为icon.ico")
except Exception as e:
    print(f"转换失败：{e}")
    exit(1)
