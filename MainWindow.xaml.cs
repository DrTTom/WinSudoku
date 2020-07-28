using System;
using System.IO;
using System.Globalization;
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
        internal enum FillMode
        {
            None,
            Direct,
            Complete
        }

        private static readonly CultureInfo CULTURE = new CultureInfo("de-DE");
        private static readonly int SIZE = 9;
        private static readonly Regex NUMBER = new Regex(".*([1-9]\\z)");

        private static readonly SolidColorBrush INPUT = Brushes.Black;
        private static readonly SolidColorBrush DIRECT_CONCLUSION = Brushes.Gray;
        private static readonly SolidColorBrush COMPUTED = Brushes.Blue;
        
        private TextBox[][] cells = new TextBox[SIZE][];

        private FillMode fillMode = FillMode.None;
        private int[][] currentInput;
        private Sudoku currentSolver;

        public MainWindow()
        {
            InitializeComponent();
            for (int row = 0; row < SIZE; row++)
            {
                cells[row] = new TextBox[SIZE];
                for (int col = 0; col < SIZE; col++)
                {
                    cells[row][col] = CreateTextBox();
                    Grid.SetRow(cells[row][col], row);
                    Grid.SetColumn(cells[row][col], col);
                    cells[row][col].Background = (row / 3 + col / 3) % 2 == 0 ? Brushes.Bisque : Brushes.Beige;

                    _ = PLQ.Children.Add(cells[row][col]);
                }
            }
        }

        private void AdaptFont(object sender, SizeChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            box.FontSize = 0.7 * box.ActualHeight;
        }

        /// <summary>
        /// Creates a TextBox with necessary properties.
        /// </summary>
        private TextBox CreateTextBox()
        {
            TextBox result = new TextBox()
            {
                FontWeight = FontWeights.Bold,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            var margin = result.Margin;
            margin.Left = 1;
            margin.Right = 1;
            margin.Top = 1;
            margin.Bottom = 1;
            result.Margin = margin;

            result.TextChanged += AssertNumber;
            result.SizeChanged += AdaptFont;
            return result;
        }

        private void AssertNumber(object sender, RoutedEventArgs args)
        {
            if (args is TextChangedEventArgs && sender != null)
            {
                TextBox box = (TextBox)sender;
                box.Text = NUMBER.Match(box.Text).Groups[1].Value;
                box.CaretIndex = box.Text.Length;
                box.Foreground = INPUT;
                if (fillMode != FillMode.None)
                {
                    ComputeAndShowConclusions(box);
                }
            }
        }

        private void ComputeAndShowConclusions(TextBox box)
        {
            forIdentifiedCell(box, (row, col) =>
            {
                int num = box.Text.Length > 0 ? Int32.Parse(box.Text, CULTURE) : 0;
                if (num != currentInput[row][col])
                {
                    try
                    {
                        if (currentInput[row][col] > 0)
                        {
                            clear(bbox => bbox.Foreground != INPUT);
                        }
                        else
                        {
                            currentInput[row][col] = num;
                            currentSolver.SetEntry(row, col, num - 1);
                        }
                        currentSolver.AddFindings();
                        stateLabel.Content = "";
                        ShowResult(currentSolver, DIRECT_CONCLUSION);
                    }
                    catch (IllegalEntryException)
                    {
                        stateLabel.Content = "Unlösbar";
                    }
                }
            });
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
                if (box.Text.Length == 1 && box.Foreground == INPUT)
                {
                    result[row][col] = Int32.Parse(box.Text, CULTURE);
                }
            });
            return result;
        }

        private void Button_Solve(object sender, RoutedEventArgs e)
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
                stateLabel.Dispatcher.Invoke(() => ShowResult(solver, DIRECT_CONCLUSION));
                BruteForceSolver brute = new BruteForceSolver();
                solver = (Sudoku) brute.Complete(solver);
                duration = Environment.TickCount - duration;
                stateLabel.Dispatcher.Invoke(() => ShowResult(solver, COMPUTED));

                Sudoku other = createSolver(input);
                brute.Reverse = true;
                other = (Sudoku) brute.Complete(other);
                status = solver.Equals(other) ? "Schwierigkeit " + Convert.ToString(brute.Effort, CULTURE) : "Mehrdeutig";
                status = status + " (" + Convert.ToString(duration, CULTURE) + " ms)";
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

        private static Sudoku createSolver(int[][] input)
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
            clear(box => true);
        }

        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {

            handleDialog(new System.Windows.Forms.OpenFileDialog(),
                path =>
                {
                    var read = JsonNet.Deserialize<int[][]>(File.ReadAllText(path));
                    forEachCell((row, col, box) =>
                    {
                        box.Text = (read[row][col] > 0 ? Convert.ToString(read[row][col], CULTURE) : "");
                        box.Foreground = Brushes.Black;
                    }
                    );
                    if (fillMode==FillMode.Direct)
                    {
                        currentInput = getUserInput();
                        currentSolver = createSolver(currentInput);
                        ShowResult(currentSolver, DIRECT_CONCLUSION);
                    }
                });
        }

        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            handleDialog(new System.Windows.Forms.SaveFileDialog(),
                path => File.WriteAllText(path, JsonNet.Serialize(getUserInput())));
        }

        private void handleDialog(System.Windows.Forms.FileDialog dialog, Action<String> pathAction)
        {
            dialog.Filter = "Sudoku (*.sudoku)|*.sudoku";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pathAction.Invoke(dialog.FileName);
            }
            dialog.Dispose();
        }

        private void ShowResult(Sudoku solver, SolidColorBrush color)
        {
            FillMode oldMode = fillMode;
            fillMode = FillMode.None;
            forEachCell((row, col, box) =>
            {
                if (box.Text.Length == 0)
                {
                    int num = solver.GetEntry(row, col);
                    box.Text = num == NumberSet.UNKNOWN ? "" : Convert.ToString(num + 1, CULTURE);
                    box.Foreground = color;
                }
            });
            fillMode = oldMode;
        }

        private void clear(Predicate<TextBox> predicate)
        {
            FillMode oldMode = fillMode;
            fillMode = FillMode.None;
            forEachCell((row, col, box) =>
            {
                if (predicate.Invoke(box))
                {
                    box.Text = "";                    
                }
            });
            currentInput = getUserInput();
            currentSolver = createSolver(currentInput);
            stateLabel.Content = "";
            fillMode = oldMode;
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

        private void forIdentifiedCell(TextBox box, Action<int, int> action)
        {
            forEachCell((row, col, b) => { if (box == b) { action.Invoke(row, col); } });
        }

        internal void ModeBoxClicked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.IsChecked == true)
            {
                fillMode = FillMode.Direct;
                currentInput = getUserInput();
                currentSolver = createSolver(currentInput);
                ShowResult(currentSolver, DIRECT_CONCLUSION);
            }
            else
            {
                fillMode = FillMode.None;
            }
        }
    }

}