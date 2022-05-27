using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CompactTaskManager
{
    /// <summary>
    /// This class manages all images of the processes, it saves system ressources.
    /// </summary>
    public static class ImageManager
    {
        private static List<Tuple<ImageSource, string>> CachedImages = new List<Tuple<ImageSource, string>>() { };
        private static List<string> IgnoredPaths = new List<string>();


        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject([In] IntPtr hObject);

        private static ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            IntPtr hbitmap = bmp.GetHbitmap();
            try
            {
                ImageSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(hbitmap);
                return sourceFromHbitmap;
            }
            catch (Exception)
            {
                DeleteObject(hbitmap);
                return null;

            }
        }


        public static ImageSource GetIcon(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;
            if (IgnoredPaths.Contains(fileName))
                return null;
            
            var foundIcon = CachedImages.FirstOrDefault(ico => ico.Item2 == fileName);
            if (foundIcon != null) 
                return foundIcon.Item1 as ImageSource;
            
            

            Icon associatedIcon = Icon.ExtractAssociatedIcon(fileName);
            if(associatedIcon == null)
            {
                IgnoredPaths.Add(fileName);
                return null;
            }
            
            ImageSource icon = ImageSourceForBitmap(associatedIcon.ToBitmap());
            CachedImages.Add(new Tuple<ImageSource, string>(icon, fileName));
            return icon;
        }
    }
}
