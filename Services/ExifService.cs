using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace ImageMinify.Services;

public sealed class ExifService
{
    public void NormalizeForJpegOutput(Image image, string inputPath)
    {
        image.Mutate(context => context.AutoOrient());
        image.Metadata.ExifProfile = BuildReducedProfile(inputPath);
        image.Metadata.XmpProfile = null;
    }

    public void NormalizeForNonJpegOutput(Image image)
    {
        image.Mutate(context => context.AutoOrient());
        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.XmpProfile = null;
    }

    private static ExifProfile? BuildReducedProfile(string inputPath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(inputPath);
            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0 is null)
            {
                return null;
            }

            var profile = new ExifProfile
            {
                Parts = ExifParts.IfdTags,
            };

            var make = ifd0.GetString(ExifDirectoryBase.TagMake);
            if (!string.IsNullOrWhiteSpace(make))
            {
                profile.SetValue(ExifTag.Make, make);
            }

            var model = ifd0.GetString(ExifDirectoryBase.TagModel);
            if (!string.IsNullOrWhiteSpace(model))
            {
                profile.SetValue(ExifTag.Model, model);
            }

            var software = ifd0.GetString(ExifDirectoryBase.TagSoftware);
            if (!string.IsNullOrWhiteSpace(software))
            {
                profile.SetValue(ExifTag.Software, software);
            }

            var dateTime = ifd0.GetString(ExifDirectoryBase.TagDateTime);
            if (!string.IsNullOrWhiteSpace(dateTime))
            {
                profile.SetValue(ExifTag.DateTime, dateTime);
            }

            profile.SetValue(ExifTag.Orientation, (ushort)1);
            return profile;
        }
        catch
        {
            return null;
        }
    }
}
