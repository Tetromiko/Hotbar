using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hotbar.Controls
{
    /// <summary>
    /// Interaction logic for ItemHolder.xaml
    /// </summary>
    [Serializable]
    public class MessageShowException : Exception
    {
        public MessageShowException() { }
        public MessageShowException(string message) : base(message) {
            MessageBox.Show(message);
        }
        public MessageShowException(string message, Exception inner) : base(message, inner) { }
        protected MessageShowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public partial class ItemHolder : UserControl
    {
        public Image iconImage;
        Brush highlighted = new SolidColorBrush(Color.FromRgb(63, 63, 63));
        Brush normal = new SolidColorBrush(Color.FromRgb(7, 7, 7));
        Brush delete = new SolidColorBrush(Color.FromRgb(63, 31, 31));
        public string file;
        public ItemHolder()
        {
            InitializeComponent();
            int cellSize = (App.Current.MainWindow as MainWindow)?.CellSize ?? throw new MessageShowException(this.ToString()+" cell size");
            box.Margin = new Thickness(cellSize/8);
            box.CornerRadius = new CornerRadius(cellSize/4);
            Drop += ItemHolder_Drop;
        }

        private void ItemHolder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

                if (Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase)|| Path.GetExtension(file = GetShortcutTarget(file)).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    LoadAndDisplayIcon(file);
                }
            }
        }
        public static string GetShortcutTarget(string shortcutFilePath)
        {
            string targetPath = null;

            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);

                targetPath = shortcut.TargetPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading shortcut: " + ex.Message);
            }

            return targetPath;
        }
        public void LoadAndDisplayIcon(string filePath)
        {
            try
            {
                file = filePath;
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
                iconImage = new Image();
                iconImage.Source = bitmapSource;
                holder.Children.Add(iconImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading and displaying icon: {ex.Message}");
            }
        }
        private void Highlight(object sender, MouseEventArgs e)
        {
            if (((MainWindow)App.Current.MainWindow).isAlternative)
            {
                box.Background = delete;
            }
            else
            {
                box.Background = highlighted;
            }
            
        }

        private void OffHighlight(object sender, MouseEventArgs e)
        {
            box.Background = normal;
        }
        private void StartAction(object sender, MouseButtonEventArgs e)
        {
        }
        private void RunProgram(object sender, MouseButtonEventArgs e)
        {
            if (System.IO.File.Exists(file))
            {
                Process.Start(file);
            }
            else
            {
                MessageBox.Show("The program does not exist at the specified path.");
            }
        }
        private void box_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Run");
                Debug.WriteLine("Run");
                if (System.IO.File.Exists(file))
                {
                    Process.Start(file);
                }
                else
                {
                    Console.WriteLine("The program does not exist at the specified path.");
                }
        }
    }
}
