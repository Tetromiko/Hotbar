using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hotbar.Controls
{
    /// <summary>
    /// Interaction logic for TableGrid.xaml
    /// </summary>
    public partial class TableGrid : UserControl
    {
        public ProgramHolder[,] Items;
        public int ColumnsCount;
        public int RowsCount;
        Random random;
        public TableGrid()
        {
            random = new Random();
            //Brush brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256)));
            InitializeComponent();
            ColumnsCount = (App.Current.MainWindow as MainWindow)?.ColumnsCount ?? throw new MessageShowException(this.ToString() + " columns count");
            RowsCount = (App.Current.MainWindow as MainWindow)?.RowsCount ?? throw new MessageShowException(this.ToString() + " rows count");

            for (int i = 0; i < ColumnsCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < RowsCount; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            Items = new ProgramHolder[ColumnsCount, RowsCount];

            for (int i = 0; i < ColumnsCount; i++)
            {
                for (int j = 0; j < RowsCount; j++)
                {
                    Items[i, j] = new ProgramHolder();
                    //Items[i, j].box.Background = brush;
                    grid.Children.Add(Items[i, j]);
                    Grid.SetColumn(Items[i, j], i);
                    Grid.SetRow(Items[i, j], j);
                }
            }
        }

        public void ChangeLeftColumnsCount(int columns)
        {
            //Brush brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256)));
            int add = Math.Abs(columns);
            int newColumns = ColumnsCount + columns;
            ProgramHolder[,] newItems = new ProgramHolder[newColumns, RowsCount];

            if (columns > 0)
            {
                for (int x = 0; x < newColumns; x++)
                {
                    if (x < add)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        for (int y = 0; y < RowsCount; y++)
                        {
                            newItems[x, y] = new ProgramHolder();
                            //newItems[x, y].box.Background = brush;
                            Grid.SetRow(newItems[x, y], y);
                            Grid.SetColumn(newItems[x, y], x);
                            grid.Children.Add(newItems[x, y]);
                        }
                        
                    }
                    else
                    {
                        for (int y = 0; y < RowsCount; y++)
                        {
                            newItems[x, y] = Items[x-add, y];
                            Grid.SetRow(newItems[x, y], y);
                            Grid.SetColumn(newItems[x, y], x);
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < add; x++)
                {
                    grid.ColumnDefinitions.RemoveAt(0);
                    for (int y = 0; y < RowsCount; y++)
                    {
                        grid.Children.Remove(Items[x, y]);
                    }
                }
                for (int x = 0; x < newColumns; x++)
                {
                    for (int y = 0; y < RowsCount; y++)
                    {
                        newItems[x, y] = Items[x + add, y];
                        Grid.SetRow(newItems[x, y], y);
                        Grid.SetColumn(newItems[x, y], x);
                    }
                }
            }
            ColumnsCount = newColumns;
            Items = newItems;
        }
        public void ChangeRightColumnsCount(int columns)
        {
            //Brush brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256)));
            int add = Math.Abs(columns);
            int newColumns = ColumnsCount + columns;
            ProgramHolder[,] newItems = new ProgramHolder[newColumns, RowsCount];

            if (columns > 0)
            {
                for (int x = 0; x < newColumns; x++)
                {
                    if (x < ColumnsCount)
                    {
                        for (int y = 0; y < RowsCount; y++)
                        {
                            newItems[x, y] = Items[x, y];
                            Grid.SetRow(newItems[x, y], y);
                            Grid.SetColumn(newItems[x, y], x);
                        }
                    }
                    else
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        for (int y = 0; y < RowsCount; y++)
                        {
                            newItems[x, y] = new ProgramHolder();
                            //newItems[x, y].box.Background = brush;
                            Grid.SetRow(newItems[x, y], y);
                            Grid.SetColumn(newItems[x, y], x);
                            grid.Children.Add(newItems[x, y]);
                        }
                    }
                }
            }
            else
            {
                for (int x = ColumnsCount - add; x < ColumnsCount; x++)
                {
                    grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.Count - 1);
                    for (int y = 0; y < RowsCount; y++)
                    {
                        grid.Children.Remove(Items[x, y]);
                    }
                }

                for (int x = 0; x < newColumns; x++)
                {
                    for (int y = 0; y < RowsCount; y++)
                    {
                        newItems[x, y] = Items[x, y];
                        Grid.SetRow(newItems[x, y], y);
                        Grid.SetColumn(newItems[x, y], x);
                    }
                }
            }

            ColumnsCount = newColumns;
            Items = newItems;
        }

        public void ChangeTopRowsCount(int rows)
        {
            //Brush brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256)));
            int add = Math.Abs(rows);
            int newRows = RowsCount + rows;
            ProgramHolder[,] newItems = new ProgramHolder[ColumnsCount, newRows];

            if (rows > 0)
            {
                for (int y = 0; y < newRows; y++)
                {
                    if (y < add)
                    {
                        grid.RowDefinitions.Insert(0, new RowDefinition());
                        for (int x = 0; x < ColumnsCount; x++)
                        {
                            newItems[x, y] = new ProgramHolder();
                            //newItems[x, y].box.Background = brush;
                            Grid.SetRow(newItems[x, y], y);
                            Grid.SetColumn(newItems[x, y], x);
                            grid.Children.Add(newItems[x, y]);
                        }
                    }
                    else
                    {
                        for (int x = 0; x < ColumnsCount; x++)
                        {
                            newItems[x, y] = Items[x, y - add];
                            Grid.SetRow(newItems[x, y], y);
                            Grid.SetColumn(newItems[x, y], x);
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < add; y++)
                {
                    grid.RowDefinitions.RemoveAt(0);
                    for (int x = 0; x < ColumnsCount; x++)
                    {
                        grid.Children.Remove(Items[x, y]);
                    }
                }
                for (int y = 0; y < newRows; y++)
                {
                    for (int x = 0; x < ColumnsCount; x++)
                    {
                        newItems[x, y] = Items[x, y + add];
                        Grid.SetRow(newItems[x, y], y);
                        Grid.SetColumn(newItems[x, y], x);
                    }
                }
            }

            RowsCount = newRows;
            Items = newItems;
        }

        public void ChangeBottomRowsCount(int rows)
        {
            //Brush brush = new SolidColorBrush(Color.FromRgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256)));
            int add = Math.Abs(rows);
            int newRows = RowsCount + rows;
            ProgramHolder[,] newItems = new ProgramHolder[ColumnsCount, newRows];

            if (rows > 0)
            {
                for (int y = 0; y < RowsCount; y++)
                {
                    for (int x = 0; x < ColumnsCount; x++)
                    {
                        newItems[x, y] = Items[x, y];
                        Grid.SetRow(newItems[x, y], y);
                        Grid.SetColumn(newItems[x, y], x);
                    }
                }

                for (int y = RowsCount; y < newRows; y++)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    for (int x = 0; x < ColumnsCount; x++)
                    {
                        newItems[x, y] = new ProgramHolder();
                        //newItems[x, y].box.Background = brush;
                        Grid.SetRow(newItems[x, y], y);
                        Grid.SetColumn(newItems[x, y], x);
                        grid.Children.Add(newItems[x, y]);
                    }
                }
            }
            else
            {
                for (int y = RowsCount - add; y < RowsCount; y++)
                {
                    grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count - 1);
                    for (int x = 0; x < ColumnsCount; x++)
                    {
                        grid.Children.Remove(Items[x, y]);
                    }
                }

                for (int x = 0; x < ColumnsCount; x++)
                {
                    for (int y = 0; y < newRows; y++)
                    {
                        newItems[x, y] = Items[x, y];
                        Grid.SetRow(newItems[x, y], y);
                        Grid.SetColumn(newItems[x, y], x);
                    }
                }
            }

            RowsCount = newRows;
            Items = newItems;
        }
    }
}
