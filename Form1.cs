using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static GeneticAlgorithm.Population;
using static UtilityNamespace.Utility;

namespace GeneticAlgorithm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Start.Select();
        }

        private async void Start_ClickAsync(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                InputProcessing.tests = true;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Set up a Timer to update the elapsed time display
                var elapsedTimeTimer = new System.Windows.Forms.Timer { Interval = 500 }; // 500 ms interval

                elapsedTimeTimer.Tick += (timerSender, args) =>
                {
                    // Update the TextBox with the current elapsed time
                    TimeSpan elapsed = stopwatch.Elapsed;
                    elapsedTimeTextBox.Text = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                };
                elapsedTimeTimer.Start();

                // Run the main task
                await RunTestsAndDisplayResultsAsync();

                // Stop the stopwatch and timer when the task is complete
                stopwatch.Stop();
                elapsedTimeTimer.Stop();

                InputProcessing.tests = false;
                myRandom = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
                return;
            }
            dataGridView1.Rows.Clear();
            dataGridView1.Visible = true;
            List<object> columns = InputProcessing.Process(
                Double.Parse(textBox1.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                Double.Parse(textBox2.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                Double.Parse(comboBox1.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                int.Parse(textBox3.Text),
                Double.Parse(textBox4.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                Double.Parse(textBox5.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                int.Parse(textBox6.Text),
                checkBox1.Checked
            );

            chart1.Series.Clear();
            Series series1 = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Legend = "Legend1",
                Name = "fₘᵢₙ"
            };
            chart1.Series.Add(series1);
            for (int i = 0; i < InputProcessing.T; ++i)
            {
                series1.Points.AddXY(i + 1, InputProcessing.plotDataMin[i]);
            }
            Series series2 = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Legend = "Legend1",
                Name = "fₐᵥₑ"
            };
            chart1.Series.Add(series2);
            for (int i = 0; i < InputProcessing.T; ++i)
            {
                series2.Points.AddXY(i + 1, InputProcessing.plotDataAvg[i]);
            }
            Series series3 = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Legend = "Legend1",
                Name = "fₘₐₓ"
            };
            chart1.Series.Add(series3);
            for (int i = 0; i < InputProcessing.T; ++i)
            {
                series3.Points.AddXY(i + 1, InputProcessing.plotDataMax[i]);
            }
            chart1.Invalidate();


            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            Dictionary<double, int> valsGroups = new Dictionary<double, int>();
            int index = 0;
            for (int i = 0; i < InputProcessing.N; ++i)
            {
                double val = (((double[])columns[((int)PopulationStagesNames.ResultVals)])[i]);
                if (!valsGroups.ContainsKey(val))
                {
                    valsGroups.Add(val, 1);
                    ++index;
                    DataGridViewRow row = new DataGridViewRow();
                    row.DefaultCellStyle = dataGridView1.DefaultCellStyle;
                    row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    row.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 10.2F,
                        FontStyle.Regular, GraphicsUnit.Point, ((byte)(204)));

                    row.DefaultCellStyle.Padding = new Padding(15, 0, 5, 0);
                    row.CreateCells(dataGridView1);
                    row.Cells[2].Value =
                        (((BitArray[])columns[((int)PopulationStagesNames.OffspringVals)])[i]).BinToString();

                    row.Cells[0].Value =
                        index.ToString();

                    row.Cells[1].Value = val.ToString("F" + InputProcessing.prec.ToString());

                    row.Cells[3].Value =
                        (((double[])columns[((int)PopulationStagesNames.FinalGoalVals)])[i])
                        .ToString("F" + InputProcessing.prec.ToString());

                    rows.Add(row);
                } else
                {
                    ++valsGroups[val];
                }
            }

            foreach(DataGridViewRow row in rows)
            {
                row.Cells[4].Value = (Math.Round((double)
                    (valsGroups[Double.Parse((String)(row.Cells[1].Value))] 
                    / (double)InputProcessing.N * 100), 3)).ToString();
            }

            dataGridView1.Rows.AddRange(rows.ToArray());
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private async Task RunTestsAndDisplayResultsAsync()
        {
            const int testsCount = 100;
            int[] N_tests = { 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80 };
            double[] pk_tests = { 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9 };
            double[] pm_tests = { 0.0001, 0.0005, 0.001, 0.005, 0.01 };
            int[] T_tests = { 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160 };
            // Calculate total combinations and initialize progress
            int totalCombinations = N_tests.Length * pk_tests.Length * pm_tests.Length * T_tests.Length;
            int completedCombinations = 0;

            // Initialize progress bar properties
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            // Use ConcurrentDictionary to store results
            ConcurrentDictionary<string, double> testsResults = new ConcurrentDictionary<string, double>();

            // Set up a Timer to batch update the progress bar periodically
            var progressUpdateTimer = new System.Windows.Forms.Timer { Interval = 100 }; // 100 ms interval

            // Timer event to update progress bar
            progressUpdateTimer.Tick += (timerSender, args) =>
            {
                int progressPercentage = (int)((double)completedCombinations / totalCombinations * 100);
                progressBar1.Value = progressPercentage;
                progressBar1.Refresh();
            };
            progressUpdateTimer.Start();

            await Task.Run(() =>
            {
                Parallel.ForEach(N_tests, testN =>
                {
                    foreach (double testpk in pk_tests)
                    {
                        foreach (double testpm in pm_tests)
                        {
                            foreach (int testT in T_tests)
                            {
                                double bestSum = 0;
                                for (int i = 0; i < testsCount; ++i)
                                {
                                    bestSum += ((Double[])((InputProcessing.Process(
                                        -4,
                                        12,
                                        0.001,
                                        testN,
                                        testpk,
                                        testpm,
                                        testT,
                                        checkBox1.Checked
                                    ))[0])).Max();
                                }
                                testsResults.TryAdd($"{testN}, {testpk}, {testpm}, {testT}",
                                    bestSum / (double)testsCount);

                                // Increment completed combinations in a thread-safe manner
                                Interlocked.Increment(ref completedCombinations);
                            }
                        }
                    }
                });
            });

            // Stop the timer and complete the progress bar on completion
            progressUpdateTimer.Stop();
            progressBar1.Value = 100;
            progressBar1.Refresh();

            // Sort and display results in the DataGridView
            DisplayResults(testsResults);
        }

        private void DisplayResults(ConcurrentDictionary<string, double> testsResults)
        {
            var myList = testsResults.ToList();
            myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            dataGridView2.Rows.Clear();
            dataGridView2.Visible = true;
            List<DataGridViewRow> testRows = new List<DataGridViewRow>();
            for (int i = 0; i < 10; ++i)
            {
                string key = myList[i].Key;
                double val = myList[i].Value;
                DataGridViewRow testRow = new DataGridViewRow();
                testRow.DefaultCellStyle = dataGridView2.DefaultCellStyle;
                testRow.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                testRow.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 10.2F,
                    FontStyle.Regular, GraphicsUnit.Point, ((byte)(204)));

                testRow.DefaultCellStyle.Padding = new Padding(15, 0, 5, 0);
                testRow.CreateCells(dataGridView2);
                testRow.Cells[0].Value = (i + 1).ToString();
                testRow.Cells[1].Value = key;
                testRow.Cells[2].Value = val.ToString("F3");
                testRows.Add(testRow);
            }
            dataGridView2.Rows.AddRange(testRows.ToArray());
            dataGridView2.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            InputProcessing.tests = false;
        }

        void textBox1_TextChanged(object sender, EventArgs e)
        {
            SizeF mySize = new SizeF();

            // Use the textbox font
            Font myFont = textBox1.Font;

            using (Graphics g = CreateGraphics())
            {
                // Get the size given the string and the font
                mySize = g.MeasureString(textBox1.Text, myFont);
            }

            // Resize the textbox 
            textBox1.Width = (int)Math.Round(mySize.Width, 0) + 3;
        }

        void textBox2_TextChanged(object sender, EventArgs e)
        {
            SizeF mySize = new SizeF();

            // Use the textbox font
            Font myFont = textBox2.Font;

            using (Graphics g = CreateGraphics())
            {
                // Get the size given the string and the font
                mySize = g.MeasureString(textBox2.Text, myFont);
            }

            // Resize the textbox 
            textBox2.Width = (int)Math.Round(mySize.Width, 0) + 3;
        }

        void textBox3_TextChanged(object sender, EventArgs e)
        {
            SizeF mySize = new SizeF();

            // Use the textbox font
            Font myFont = textBox3.Font;

            using (Graphics g = CreateGraphics())
            {
                // Get the size given the string and the font
                mySize = g.MeasureString(textBox3.Text, myFont);
            }

            // Resize the textbox 
            textBox3.Width = (int)Math.Round(mySize.Width, 0) + 3;
        }
        void textBox4_TextChanged(object sender, EventArgs e)
        {
            SizeF mySize = new SizeF();

            // Use the textbox font
            Font myFont = textBox4.Font;

            using (Graphics g = CreateGraphics())
            {
                // Get the size given the string and the font
                mySize = g.MeasureString(textBox4.Text, myFont);
            }

            // Resize the textbox 
            textBox4.Width = (int)Math.Round(mySize.Width, 0) + 3;
        }
        void textBox5_TextChanged(object sender, EventArgs e)
        {
            SizeF mySize = new SizeF();

            // Use the textbox font
            Font myFont = textBox5.Font;

            using (Graphics g = CreateGraphics())
            {
                // Get the size given the string and the font
                mySize = g.MeasureString(textBox5.Text, myFont);
            }

            // Resize the textbox 
            textBox5.Width = (int)Math.Round(mySize.Width, 0) + 3;
        }

        void textBox6_TextChanged(object sender, EventArgs e)
        {
            SizeF mySize = new SizeF();

            // Use the textbox font
            Font myFont = textBox6.Font;

            using (Graphics g = CreateGraphics())
            {
                // Get the size given the string and the font
                mySize = g.MeasureString(textBox6.Text, myFont);
            }

            // Resize the textbox 
            textBox6.Width = (int)Math.Round(mySize.Width, 0) + 3;
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            string text = (sender as TextBox).Text;
            int pos = (sender as TextBox).SelectionStart;
            if (e.KeyChar == '-' && pos == 0 && (text.IndexOf('-') == -1))
            {
                e.Handled = false;
                return;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.') && (e.KeyChar != ','))
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (text.IndexOf('.') > -1
                || text.IndexOf(',') > -1))
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (pos == 0
                || !char.IsDigit(text[pos - 1])))
            {
                e.Handled = true;
                return;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            string text = (sender as TextBox).Text;
            int pos = (sender as TextBox).SelectionStart;
            if (e.KeyChar == '-' && pos == 0 && (text.IndexOf('-') == -1))
            {
                e.Handled = false;
                return;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.') && (e.KeyChar != ','))
            {
                e.Handled = true;
                return;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (text.IndexOf('.') > -1
                || text.IndexOf(',') > -1))
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (pos == 0
                || !char.IsDigit(text[pos - 1])))
            {
                e.Handled = true;
                return;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            string text = (sender as TextBox).Text;
            int pos = (sender as TextBox).SelectionStart;

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.') && (e.KeyChar != ','))
            {
                e.Handled = true;
                return;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (text.IndexOf('.') > -1
                || text.IndexOf(',') > -1))
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (pos == 0
                || !char.IsDigit(text[pos - 1])))
            {
                e.Handled = true;
                return;
            }
        }
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            string text = (sender as TextBox).Text;
            int pos = (sender as TextBox).SelectionStart;

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.') && (e.KeyChar != ','))
            {
                e.Handled = true;
                return;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (text.IndexOf('.') > -1
                || text.IndexOf(',') > -1))
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar == '.' || e.KeyChar == ',')
                && (pos == 0
                || !char.IsDigit(text[pos - 1])))
            {
                e.Handled = true;
                return;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }








    }
}
