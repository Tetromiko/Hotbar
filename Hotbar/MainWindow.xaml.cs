using HelpBar.Hotkeys;
using Hotbar.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Hotbar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public enum WindowPosition
    {
        Top,
        Bottom,
        Right,
        Left,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public partial class MainWindow : Window
    {
        private ProgramConfiguration config;
        public int ColumnsCount = 1, RowsCount = 1;
        private TableGrid tableGrid;
        public int CellSize = 100;
        private Point mouseOffset;
        private bool isDragging;
        private bool canTransform;
        public int windowWidth;
        public int windowHeight;
        public int PosX = 0;
        public int PosY = 0;
        private bool isResizing;
        private Border? resizingBorder;
        private Point startPoint;
        public bool isVisible;
        public bool isAlternative;
        public ProgramHolder draggedItem;

        public MainWindow()
        {
            InitializeComponent();
            HotkeysManager.AddHotkey(ModifierKeys.Alt, Key.LWin, ShowProgram);
            HotkeysManager.SetupSystemHook();
            LoadConfiguration();
            Loaded += WindowLoaded;
            Closing += SaveConfiguration;
        }


        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists("config.xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ProgramConfiguration));
                    using (FileStream fs = new FileStream("config.xml", FileMode.Open))
                    {
                        config = (ProgramConfiguration)serializer.Deserialize(fs);
                    }
                    ColumnsCount = config.ColumnsCount;
                    RowsCount = config.RowsCount;
                    PosX = config.PosX;
                    PosY = config.PosY;
                }
                else
                {
                    config = new ProgramConfiguration();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження конфігурації: " + ex.Message);
            }
        }
        private void SaveConfiguration(object? sender, CancelEventArgs e)
        {
                config.ColumnsCount = ColumnsCount;
                config.RowsCount = RowsCount;
                config.PosX = (int)Canvas.GetLeft(helpbar);
                config.PosY = (int)Canvas.GetTop(helpbar);
                config.Apps.Clear();
                for (int x = 0; x < ColumnsCount; x++)
                {
                    for (int y = 0; y < RowsCount; y++)
                    {
                        if (tableGrid.Items[x, y].file != null)
                            config.Apps.Add(new ItemData(x, y, tableGrid.Items[x, y].file));
                    }
                }
                XmlSerializer serializer = new XmlSerializer(typeof(ProgramConfiguration));
                using (FileStream fs = new FileStream("config.xml", FileMode.Create))
                {
                    serializer.Serialize(fs, config);
                }
        }
        private void ShowProgram()
        {
            if (isVisible) ShowWindow();
            else HideWindow();
        }
        private void ShowWindow()
        {
            isVisible = false;
            Visibility = Visibility.Visible;
            Focus();
        }
        private void HideWindow()
        {
            isVisible = true;
            Visibility = Visibility.Hidden;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = 0;
            Width = screenWidth;
            Height = screenHeight;
            windowWidth = (int)screenWidth;
            windowHeight = (int)screenHeight;
            CellSize = FindGCD(windowHeight, windowWidth);
            ui.Visibility = Visibility.Hidden;
            SetSize(CellSize * ColumnsCount + CellSize / 4, CellSize * RowsCount + CellSize / 4);
            SetPosition(PosX,PosY);
            CreateTableGrid();
            UpdateTable();
            CreateUI();
            UpdateUI();
            foreach (var app in config.Apps)
            {
                tableGrid.Items[app.X, app.Y].LoadAndDisplayIcon(app.Path);
            }
        }

        private void CreateUI()
        {
            tableGridPreview.Width = windowWidth;
            tableGridPreview.Height = windowHeight;
            cells.Viewport = new Rect(0,0,1.0/ (windowWidth/CellSize), 1.0 / (windowHeight / CellSize));
            cell.Width = CellSize/4; 
            cell.Height = CellSize/4;
            cell.CornerRadius = new CornerRadius( CellSize/16);
            tableGridPreview.Margin = new Thickness(CellSize / 8);
            ConfigureExpander(leftExpander, CellSize / 4, CellSize / 4, new CornerRadius(0, CellSize / 16, CellSize / 16, 0));
            ConfigureExpander(topExpander, CellSize / 4, CellSize / 4, new CornerRadius(0, 0, CellSize / 16, CellSize / 16));
            ConfigureExpander(rightExpander, CellSize / 4, CellSize / 4, new CornerRadius(CellSize / 16, 0, 0, CellSize / 16));
            ConfigureExpander(bottomExpander, CellSize / 4, CellSize / 4, new CornerRadius(CellSize / 16, CellSize / 16, 0, 0));
        }

        private void ConfigureExpander(Border expander, double width, double height, CornerRadius cornerRadius)
        {
            expander.Width = width;
            expander.Height = height;
            expander.CornerRadius = cornerRadius;
        }

        private void CreateTableGrid()
        {
            tableGrid = new TableGrid();
            box.Child = tableGrid;
            box.Padding = new Thickness(CellSize / 8);
        }
        public void UpdateTable()
        {
            tableGrid.Width = CellSize * ColumnsCount;
            tableGrid.Height = CellSize * RowsCount;
        }
        public void UpdateUI()
        {
            CornerRadius cornerRadius = new CornerRadius(CellSize / 2);
            if (Canvas.GetLeft(helpbar) == 0)
            {
                cornerRadius.TopLeft = 0;
                cornerRadius.BottomLeft = 0;
            }
            if (Canvas.GetTop(helpbar) == 0)
            {
                cornerRadius.TopLeft = 0;
                cornerRadius.TopRight = 0;
            }
            if (AreApproximatelyEqual(Canvas.GetLeft(helpbar), windowWidth - helpbar.Width, 0.5))
            {
                cornerRadius.BottomRight = 0;
                cornerRadius.TopRight = 0;
            }
            if (AreApproximatelyEqual(Canvas.GetTop(helpbar), windowHeight - helpbar.Height, 0.5))
            {
                cornerRadius.BottomLeft = 0;
                cornerRadius.BottomRight = 0;
            }
            box.CornerRadius = cornerRadius;
            ConfigureExpander(leftExpander, CellSize / 4 , CellSize / 4 * Math.Clamp(RowsCount, 1, 5), new CornerRadius(0, CellSize / 16, CellSize / 16, 0));
            ConfigureExpander(topExpander, CellSize / 4 * Math.Clamp(ColumnsCount, 1, 5), CellSize / 4 , new CornerRadius(0, 0, CellSize / 16, CellSize / 16));
            ConfigureExpander(rightExpander, CellSize / 4 , CellSize / 4 * Math.Clamp(RowsCount, 1, 5), new CornerRadius(CellSize / 16, 0, 0, CellSize / 16));
            ConfigureExpander(bottomExpander, CellSize / 4 * Math.Clamp(ColumnsCount, 1, 5), CellSize / 4, new CornerRadius(CellSize / 16, CellSize / 16, 0, 0));
        }
        #region Window positioning
        public void SetPosition(WindowPosition position)
        {
            double x = 0, y = 0;
            switch (position)
            {
                case WindowPosition.Top:
                    x = (windowWidth - helpbar.Width) / 2;
                    break;
                case WindowPosition.Bottom:
                    x = (windowWidth - helpbar.Width) / 2;
                    y = windowHeight - helpbar.Height;
                    break;
                case WindowPosition.Right:
                    x = windowWidth - helpbar.Width;
                    y = (windowHeight - helpbar.Height) / 2;
                    break;
                case WindowPosition.Left:
                    y = (windowHeight - helpbar.Height) / 2;
                    break;
                case WindowPosition.TopLeft:
                    break;
                case WindowPosition.TopRight:
                    x = windowWidth - helpbar.Width;
                    break;
                case WindowPosition.BottomLeft:
                    y = windowHeight - helpbar.Height;
                    break;
                case WindowPosition.BottomRight:
                    x = windowWidth - helpbar.Width;
                    y = windowHeight - helpbar.Height;
                    break;
                default:
                    break;
            }
            SetPosition(x, y);
        }
        public void SetPosition(double X, double Y)
        {
            Canvas.SetLeft(helpbar, X);
            Canvas.SetTop(helpbar, Y);
        }
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Capture(canvas) && canTransform)
            {
                isDragging = true;
                mouseOffset = Mouse.GetPosition(helpbar);
            }
        }
        public void DragItem(ProgramHolder target)
        {
            draggedItem = target;
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newPoint = Mouse.GetPosition(canvas);
                SetPosition(Math.Clamp(newPoint.X - mouseOffset.X, 0, windowWidth - helpbar.Width), Math.Clamp(newPoint.Y - mouseOffset.Y, 0, windowHeight - helpbar.Height));
                UpdateUI();
                InvalidateVisual();
            }
        }
        private void AllowTransform(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift && canTransform == false)
            {
                ui.Visibility = Visibility.Visible;
                tableGrid.IsEnabled = false;
                canTransform = true;
            }
            if(e.Key == Key.LeftCtrl && isAlternative == false)
            {
                isAlternative = true;
            }
        }
        private void DisallowTransform(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ui.Visibility = Visibility.Hidden;
                tableGrid.IsEnabled = true;
                canTransform = false;
                isResizing = false;
                isDragging = false;
                Mouse.Capture(null);
            }
            if (e.Key == Key.LeftCtrl)
            {
                isAlternative = false;
            }
        }
        #endregion
        #region Window resizing
        private void Extender_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Capture(canvas) && canTransform)
            {
                isResizing = true;
                resizingBorder = sender as Border;
                startPoint = e.GetPosition(canvas);
                if (resizingBorder == leftExpander)
                {
                    tableGrid.HorizontalAlignment = HorizontalAlignment.Right;
                    tableGridPreview.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else if (resizingBorder == topExpander)
                {
                    tableGrid.VerticalAlignment = VerticalAlignment.Bottom;
                    tableGridPreview.VerticalAlignment = VerticalAlignment.Bottom;
                }
                else if (resizingBorder == rightExpander)
                {
                    tableGrid.HorizontalAlignment = HorizontalAlignment.Left;
                    tableGridPreview.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else if (resizingBorder == bottomExpander)
                {
                    tableGrid.VerticalAlignment = VerticalAlignment.Top;
                    tableGridPreview.VerticalAlignment = VerticalAlignment.Top;
                }
            }
        }

        private void Extender_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                Point endPoint = e.GetPosition(canvas);

                double offsetX = endPoint.X - startPoint.X;
                double offsetY = endPoint.Y - startPoint.Y;

                double width = helpbar.Width;
                double height = helpbar.Height;
                double left = Canvas.GetLeft(helpbar);
                double top = Canvas.GetTop(helpbar);

                if (resizingBorder == leftExpander)
                {
                    double delta = left + offsetX;
                    left = Math.Clamp(delta, 0, windowWidth);
                    delta -= left;
                    double delta2 = width - (offsetX - delta);
                    width = Math.Max(delta2, CellSize + CellSize / 4);
                    left  += delta2 - width;
                }
                else if (resizingBorder == topExpander)
                {
                    double delta = top + offsetY;
                    top = Math.Clamp(delta, 0, windowHeight);
                    delta -= top;
                    double delta2 = height - (offsetY - delta);
                    height = Math.Max(delta2, CellSize + CellSize / 4);
                    top += delta2 - height;
                }
                else if (resizingBorder == rightExpander)
                {
                    width = Math.Clamp(width + offsetX, CellSize + CellSize / 4, windowWidth - left);
                }
                else if (resizingBorder == bottomExpander)
                {
                    height = Math.Clamp(height + offsetY, CellSize + CellSize / 4, windowHeight - top);
                }
                SetSize(width, height);
                SetPosition(left, top);
                UpdateUI();
                startPoint = endPoint;
            }
        }

        private void SetSize(double width, double height)
        {
            helpbar.Width = width;
            helpbar.Height = height;
        }

        private void StopAll(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;
            isDragging = false;
            Mouse.Capture(null);
            int AddColumns = ColumnsCount;
            int AddRows = RowsCount;
            NormalizeSize();
            AddColumns = ColumnsCount - AddColumns;
            AddRows = RowsCount - AddRows;
            if(AddColumns != 0) 
            { 
                if (resizingBorder == leftExpander)
                {
                    tableGrid.ChangeLeftColumnsCount(AddColumns);
                }
                else if (resizingBorder == rightExpander)
                {
                    tableGrid.ChangeRightColumnsCount(AddColumns);
                }
            }
            if (AddRows != 0)
            {
                if (resizingBorder == topExpander)
                {
                    tableGrid.ChangeTopRowsCount(AddRows);
                }
                else if (resizingBorder == bottomExpander)
                {
                    tableGrid.ChangeBottomRowsCount(AddRows);
                }
            }
        }
        private void NormalizeSize()
        {
            ColumnsCount = (int)(helpbar.Width + CellSize/2 - CellSize / 4) / CellSize;
            RowsCount = (int)(helpbar.Height + CellSize / 2 - CellSize / 4) / CellSize;
            UpdateTable();
            double width = helpbar.Width;
            double height = helpbar.Height;
            double fixedWidth = ColumnsCount * CellSize + CellSize / 4;
            double fixedHeight = RowsCount * CellSize + CellSize / 4;
            double left = Canvas.GetLeft(helpbar);
            double top = Canvas.GetTop(helpbar);
            if (fixedWidth + left > windowWidth)
            {
                fixedWidth -= CellSize;
                ColumnsCount -= 1;
            }
            if (fixedHeight + top > windowHeight)
            {
                fixedHeight -= CellSize;
                RowsCount -= 1;
            }
            if (resizingBorder == leftExpander)
            {
                left += width - fixedWidth; 
                if (left < 0)
                {
                    left += CellSize;
                    fixedWidth -= CellSize;
                    ColumnsCount -= 1;
                }
            }
            else if (resizingBorder == topExpander)
            {
                top += height - fixedHeight; 
                if (top < 0)
                {
                    top += CellSize;
                    fixedHeight -= CellSize;
                    RowsCount -= 1;
                }
            }
            SetPosition(Math.Round(left), Math.Round(top));
            SetSize(Math.Round(fixedWidth), Math.Round(fixedHeight));
            UpdateTable();
            UpdateUI();
        }
        #endregion
        #region Math
        public static bool AreApproximatelyEqual(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if(draggedItem != null)
            {
                GeneralTransform transform = ((UIElement)draggedItem.Parent).TransformToAncestor(Application.Current.MainWindow);
                Point parentPosition = transform.Transform(new Point(0, 0));
                Point mousePosition = e.GetPosition(this);
                Point elementPosition = new Point(mousePosition.X-parentPosition.X, mousePosition.Y-parentPosition.Y);
                Debug.WriteLine(elementPosition);
                Canvas.SetLeft(draggedItem.iconImage, elementPosition.X);
                Canvas.SetTop(draggedItem.iconImage, elementPosition.Y);
            }
        }

        public static int FindGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        #endregion
    }
    [Serializable]
    public class ProgramConfiguration
    {
        public int ColumnsCount { get; set; } = 2;
        public int RowsCount { get; set; } = 2;
        public int PosX { get; set; } = 0;
        public int PosY { get; set; } = 0;
        public List<ItemData> Apps { get; set; } = new List<ItemData>();
    }
    [Serializable]
    public class ItemData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Path { get; set; } = string.Empty;
        public ItemData(int x, int y, string path)
        {
            X = x;
            Y = y;
            Path = path;
        }
        public ItemData()
        {
        }
    }
}
