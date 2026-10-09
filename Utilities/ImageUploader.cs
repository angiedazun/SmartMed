using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SmartMed.Utilities
{
    public static class ImageUploader
    {
        private static readonly string ImageFolder = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images");

        public static string UploadMedicineImage(string destinationFolder = null)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Medicine Image";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() != DialogResult.OK) return null;

                string folder = destinationFolder ?? ImageFolder;
                Directory.CreateDirectory(folder);

                string ext = Path.GetExtension(ofd.FileName);
                string fileName = $"med_{Guid.NewGuid():N}{ext}";
                string destPath = Path.Combine(folder, fileName);
                File.Copy(ofd.FileName, destPath, true);
                return destPath;
            }
        }

        public static string UploadProfileImage()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Profile Image";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() != DialogResult.OK) return null;

                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Profiles");
                Directory.CreateDirectory(folder);
                string ext = Path.GetExtension(ofd.FileName);
                string fileName = $"profile_{Guid.NewGuid():N}{ext}";
                string destPath = Path.Combine(folder, fileName);
                File.Copy(ofd.FileName, destPath, true);
                return destPath;
            }
        }

        public static Image LoadImage(string path, int width = 80, int height = 80)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;
            try
            {
                return Image.FromFile(path).GetThumbnailImage(width, height, null, IntPtr.Zero);
            }
            catch
            {
                return null;
            }
        }
    }
}
