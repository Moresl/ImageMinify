from setuptools import setup, find_packages

with open("README.md", "r", encoding="utf-8") as fh:
    long_description = fh.read()

setup(
    name="ImgYasuo",
    version="1.0.0",
    author="Your Name",
    author_email="your.email@example.com",
    description="一款简洁高效的图片压缩工具",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/yourusername/ImgYasuo",
    packages=find_packages(),
    classifiers=[
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
        "Topic :: Multimedia :: Graphics",
        "Topic :: Utilities",
    ],
    python_requires=">=3.8",
    install_requires=[
        "PyQt5>=5.15.0",
        "Pillow>=9.0.0",
        "piexif>=1.1.3",
    ],
    entry_points={
        "console_scripts": [
            "imgyasuo=main:main",
        ],
    },
    include_package_data=True,
    package_data={
        "": ["icon.ico"],
    },
)
