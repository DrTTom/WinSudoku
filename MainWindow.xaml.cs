using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace WinSudoku
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TextBox[][] cells = new TextBox[9][];

        public MainWindow()
        {
            InitializeComponent();
            for (int row = 0; row < 9; row++)
            {
                cells[row] = new TextBox[9];
                for (int col = 0; col < 9; col++)
                {
                    cells[row][col] = CreateTextBox();
                    Grid.SetRow(cells[row][col], row);
                    Grid.SetColumn(cells[row][col], col);
                    cells[row][col].Background = (row / 3 + col / 3) % 2 == 0 ? Brushes.Bisque : Brushes.Beige;

                    _ = PLQ.Children.Add(cells[row][col]);
                }
            }
        }

        /// <summary>
        /// Creates a TextBox with necessary properties.
        /// </summary>
        /// <returns></returns>
        private TextBox CreateTextBox()
        {
            TextBox result = new TextBox();
            result.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            var margin = result.Margin;
            margin.Left = 1;
            margin.Right = 1;
            margin.Top = 1;
            margin.Bottom = 1;
            result.Margin = margin;
            result.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Center);
            result.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            result.FontSize = 30.0;
            result.FontWeight = FontWeights.Bold;
            result.AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(AssertNumber));
            return result;
        }


        private Regex number = new Regex(".*([1-9]\\z)");

        private void AssertNumber(object sender, RoutedEventArgs args)
        {
            if (args is TextChangedEventArgs && sender != null)
            {
                TextBox box = (TextBox)sender;
                box.Text = number.Match(box.Text).Groups[1].Value;
                box.CaretIndex = box.Text.Length;
                box.Foreground = Brushes.Black;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String status = "";
            try
            {
                int start = Environment.TickCount;
                Sudoku solver = createSolver();
                solver.AddFindings();
                ShowResult(solver, Brushes.Blue);
                int diff = solver.complete(false);
                ShowResult(solver, Brushes.Gray);

                Sudoku other = createSolver();
                other.AddFindings();
                diff += other.complete(true);
                status = IsDifferent(solver, other) ? "Mehrdeutig" : "Schwierigkeit " + Convert.ToString(diff);
                start = Environment.TickCount - start;
                status = status +  "(" + Convert.ToString(start) + ")";                
            }
            catch (IllegalEntryException)
            {
                status = "Unlösbar";
            }
            stateLabel.Content = status;
        }

        private bool IsDifferent(Sudoku solver, Sudoku other)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (solver.GetEntry(row, col) != other.GetEntry(row, col))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Sudoku createSolver()
        {
            Sudoku solver = Sudoku.create(3, 3);
            forEachCell((row, col, box) =>
            {
                if (box.Text.Length == 1 && box.Foreground == Brushes.Black)
                {
                    try
                    {
                        solver.SetEntry(row, col, Int32.Parse(box.Text) - 1);
                    }
                    catch (IllegalEntryException e)
                    {
                        cells[row][col].Foreground = Brushes.Red;
                        throw e;
                    }
                }
            });
            return solver;
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            forEachCell((row, col, box) => box.Text = "");
        }

        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            // TODO: find out which deserializazion works and how to include it to the project and then read file:            
            int[][] mostDifficult = new int[][] {
                new int[] { 8, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 3, 6, 0, 0, 0, 0, 0 },
                new int[] { 0, 7, 0, 0, 9, 0, 2, 0, 0 },
                new int[] { 0, 5, 0, 0, 0, 7, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 4, 5, 7, 0, 0 },
                new int[] { 0, 0, 0, 1, 0, 0, 0, 3, 0 },
                new int[] { 0, 0, 1, 0, 0, 0, 0, 6, 8 },
                new int[] { 0, 0, 8, 5, 0, 0, 0, 1, 0 },
                new int[] { 0, 9, 0, 0, 0, 0, 4, 0, 0 }
            };

            int[][] weserkurierSchwer= new int[][] {
                new int[] { 0, 0, 0, 5, 0, 9, 0, 0, 0 },
                new int[] { 0, 0, 3, 0, 0, 0, 4, 0, 0 },
                new int[] { 0, 6, 0, 0, 7, 0, 0, 9, 0 },
                new int[] { 4, 0, 0, 0, 9, 0, 0, 0, 3 },
                new int[] { 0, 0, 6, 8, 0, 4, 2, 0, 0 },
                new int[] { 8, 0, 0, 0, 3, 0, 0, 0, 5 },
                new int[] { 0, 1, 0, 0, 5, 0, 0, 6, 0 },
                new int[] { 0, 0, 8, 0, 0, 0, 7, 0, 0 },
                new int[] { 0, 0, 0, 1, 0, 3, 0, 0, 0 }
            }; 
            int[][] example = mostDifficult;

            forEachCell((row, col, box) => box.Text = (example[row][col] > 0 ? Convert.ToString(example[row][col]): ""));
        }


        private void ShowResult(Sudoku solver, SolidColorBrush color)
        {
            forEachCell((row, col, box) =>
            {
                if (box.Text.Length == 0)
                {
                    int num = solver.GetEntry(row, col);
                    box.Text = num == NumberSet.UNKNOWN ? "" : Convert.ToString(num + 1);
                    box.Foreground = color;
                }
            });
        }

        private void forEachCell(Action<int, int, TextBox> action)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    action.Invoke(row, col, cells[row][col]);
                }
            }
        }
    }

}