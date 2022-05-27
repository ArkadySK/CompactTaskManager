using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CompactTaskManager
{
    public class ProcessModel
    {
        public ImageSource ExeIcon { get; } = null; //icon of the process, taken from exe file
        public int Id { get; } //process id, e.g. 32
        public string FileName { get; } // "c:\windows\explorer.exe"
        public string Arguments { get; } //TO DO: add command line arguments, e.g. "/s /l"
        public string ShortName { get; } //"shortname"
        public string ProcessName { get; set; } //"shortname.exe"
        public string Title { get; } //program title
        public string Status { get; } //Running / Not Responding
        private IntPtr hWnd { get; } //window handle needed for program management - minimize / maximize / focus / ...


        public ProcessModel(Process process, bool isAdmin)
        {
            if (process == null)
            {
                return;
            }

            try
            {
                ProcessName = process.ProcessName;
                ShortName = ProcessName + ".exe";
                Id = process.Id;
                hWnd = process.MainWindowHandle;
                Title = process.MainWindowTitle;
                Status = !process.Responding ? "Not Responding" : "Running";
                if (isAdmin)
                {
                    foreach (object module in process.Modules)
                    {
                        if (module is ProcessModule processModule4 && processModule4.FileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            FileName = processModule4.FileName;
                        }
                    }
                }
                else
                {
                    FileName = process.MainModule.FileName;
                }
            }
            catch
            {
            }
            finally
            {
                process.Dispose();
            }

            if (isAdmin)
                if (string.IsNullOrEmpty(FileName))
                {
                    bool found = false;
                    string[] folders = new string[3]
                    {
                      "C:\\Windows\\",
                      "C:\\Windows\\System32\\",
                      "C:\\Windows\\SysWOW64\\"
                    };
                    foreach (string f in folders)
                    {
                        if (!found)
                        {
                            FileInfo fileInfo = new FileInfo(f + ShortName);
                            if (fileInfo.Exists)
                            {
                                found = true;
                                FileName = fileInfo.FullName;
                            }
                        }
                    }
                }
            ExeIcon = GetIcon();
            if (ExeIcon == null)
            {
                return;
            }

            ExeIcon.Freeze();
        }

        private ImageSource GetIcon()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return null;
            }

            Icon associatedIcon = Icon.ExtractAssociatedIcon(FileName);
            return associatedIcon == null ? null : ImageSourceForBitmap(associatedIcon.ToBitmap());
        }

        public void Minimize() => WindowsAPICaller.ShowWindow(hWnd, 6);

        public void Maximize() => WindowsAPICaller.ShowWindow(hWnd, 3);

        public void Restore() => WindowsAPICaller.ShowWindow(hWnd, 1);

        public void Focus() => WindowsAPICaller.ShowWindow(hWnd, 5);

        public void Kill()
        {
            Process processById = Process.GetProcessById(Id);

            if (!processById.Responding)
            {
                var message = MessageBox.Show($"The process {this.ProcessName} is not responding.\n\nDo you want to kill the process?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (message == MessageBoxResult.No)
                {
                    return;
                }
            }
            try
            {
                processById.Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        public void KillAllWithThisName()
        {
            Process processById = Process.GetProcessById(Id);
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "C:\\Windows\\System32\\taskkill.exe",
                    Arguments = "/F /IM " + ShortName,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject([In] IntPtr hObject);

        private ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            IntPtr hbitmap = bmp.GetHbitmap();
            try
            {
                ImageSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                ProcessModel.DeleteObject(hbitmap);
                return sourceFromHbitmap;
            }
            catch (Exception)
            {
                ProcessModel.DeleteObject(hbitmap);
                return null;
            }
        }
    }
}
