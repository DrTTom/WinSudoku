using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Json.Net;


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

        private int[][] getUserInput()
        {
            int[][] result = new int[cells.Length][];
            for (int row = 0; row < cells.Length; row++)
            {
                result[row] = new int[cells[0].Length];
            }
            forEachCell((row, col, box) =>
            {
                if (box.Text.Length == 1 && box.Foreground == Brushes.Black)
                {
                    result[row][col] = Int32.Parse(box.Text);
                }
            });
            return result;
        }

        private async void Button_Solve(object sender, RoutedEventArgs e)
        {
            var input = getUserInput();
            SolveButton.IsEnabled = false;
            Task.Run(() => solveAndAnalyze(input));
            stateLabel.Content = "Rechne...";
        }

        private void solveAndAnalyze(int[][] input)
        {
            String status = "";
            try
            {
                int duration = Environment.TickCount;
                Sudoku solver = createSolver(input);
                solver.AddFindings();
                // TODO: should better copy the results before continuing computation
                stateLabel.Dispatcher.Invoke(() => ShowResult(solver, Brushes.Blue));
                int diff = solver.complete(false);
                duration = Environment.TickCount - duration;
                stateLabel.Dispatcher.Invoke(() => ShowResult(solver, Brushes.Gray));

                Sudoku other = createSolver(input);
                other.AddFindings();
                diff += other.complete(true);
                status = IsDifferent(solver, other) ? "Mehrdeutig" : "Schwierigkeit " + Convert.ToString(diff);                
                status = status + " (" + Convert.ToString(duration) + " ms)";
            }
            catch (IllegalEntryException)
            {
                status = "Unlösbar";
            }
            stateLabel.Dispatcher.Invoke(() =>
            {
                stateLabel.Content = status;
                SolveButton.IsEnabled = true;
            });
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

        private Sudoku createSolver(int[][] input)
        {
            Sudoku solver = Sudoku.create(3, 3);
            for (int row = 0; row < input.Length; row++)
            {
                for (int col = 0; col < input[row].Length; col++)
                {
                    int val = input[row][col];
                    if (val > 0)
                    {
                        solver.SetEntry(row, col, val - 1);
                    }
                }
            }
            return solver;
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            forEachCell((row, col, box) => box.Text = "");
        }

        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Sudoku (*.sudoku)|*.sudoku",
                InitialDirectory = path
            };
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var read = JsonNet.Deserialize<int[][]>(File.ReadAllText(fileDialog.FileName));
                forEachCell((row, col, box) =>
                {
                    box.Text = (read[row][col] > 0 ? Convert.ToString(read[row][col]) : "");
                    box.Foreground = Brushes.Black;
                }
                );
            }         
        }

        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "Sudoku (*.sudoku)|*.sudoku",
                InitialDirectory = path
            };
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String json = Json.Net.JsonNet.Serialize(getUserInput());
                File.WriteAllText(fileDialog.FileName, json);
            }
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