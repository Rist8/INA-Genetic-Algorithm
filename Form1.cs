using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using static GeneticAlgorithm.Population;
using static UtilityNamespace.Utility;

namespace GeneticAlgorithm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            boldFont = new Font(dataGridView1.DefaultCellStyle.Font, FontStyle.Bold);
            normalBrush = new SolidBrush(dataGridView1.DefaultCellStyle.ForeColor);
            Start.Select();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Visible = true;

            ArrayList columns = InputProcessing.Process(
                Double.Parse(textBox1.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                Double.Parse(textBox2.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                Double.Parse(comboBox1.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                int.Parse(textBox3.Text),
                Double.Parse(textBox4.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                Double.Parse(textBox5.Text.Replace(',', '.'), CultureInfo.InvariantCulture)
            );
            

            
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            for (int i = 0; i < InputProcessing.N; ++i)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.DefaultCellStyle = dataGridView1.DefaultCellStyle;
                row.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                row.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                row.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(15, 0, 5, 0);
                row.CreateCells(dataGridView1);
                row.Cells[((int)PopulationStagesNames.SelectedValsBin)].Value =
                    ((BitArray)((ArrayList)columns[((int)PopulationStagesNames.SelectedValsBin)])[i]).BinToString();

                row.Cells[(int)PopulationStagesNames.InitialVals].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.InitialVals)])[i])
                    .ToString("F" + InputProcessing.prec.ToString());
                
                row.Cells[((int)PopulationStagesNames.SelectedVals)].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.SelectedVals)])[i])
                    .ToString("F" + InputProcessing.prec.ToString());

                row.Cells[((int)PopulationStagesNames.GoalVals)].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.GoalVals)])[i])
                    .ToString("F" + InputProcessing.prec.ToString());

                row.Cells[((int)PopulationStagesNames.DistributionVals)].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.DistributionVals)])[i])
                    .ToString();

                row.Cells[((int)PopulationStagesNames.RatingVals)].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.RatingVals)])[i])
                    .ToString("F" + InputProcessing.prec.ToString());

                row.Cells[((int)PopulationStagesNames.SelectProbs)].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.SelectProbs)])[i])
                    .ToString();

                row.Cells[((int)PopulationStagesNames.SelectRandomVals)].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.SelectRandomVals)])[i])
                    .ToString();

                row.Cells[((int)PopulationStagesNames.Indexes)].Value =
                    ((ArrayList)columns[((int)PopulationStagesNames.Indexes)])[i].ToString();

                if (((ArrayList)columns[((int)PopulationStagesNames.ParentsVals)])[i] != null)
                {
                    row.Cells[((int)PopulationStagesNames.ParentsVals)].Value =
                        ((BitArray)((ArrayList)columns[((int)PopulationStagesNames.ParentsVals)])[i]).BinToString();
                    string val = (string)row.Cells[((int)PopulationStagesNames.ParentsVals)].Value;
                    val = val.Insert((int)(((ArrayList)columns[((int)PopulationStagesNames.PairsCuts)])[i]), "|");
                    row.Cells[((int)PopulationStagesNames.ParentsVals)].Value = val;
                }
                else
                {
                    row.Cells[((int)PopulationStagesNames.ParentsVals)].Value =
                        "------";
                }

                if (((ArrayList)columns[((int)PopulationStagesNames.PairsCuts)])[i] != null)
                {
                    row.Cells[((int)PopulationStagesNames.PairsCuts)].Value =
                        ((ArrayList)columns[((int)PopulationStagesNames.PairsCuts)])[i].ToString();
                }
                else
                {
                    row.Cells[((int)PopulationStagesNames.PairsCuts)].Value =
                        "------";
                }
                
                if (((ArrayList)columns[((int)PopulationStagesNames.ChildrenVals)])[i] != null)
                {
                    row.Cells[((int)PopulationStagesNames.ChildrenVals)].Value =
                        ((BitArray)((ArrayList)columns[((int)PopulationStagesNames.ChildrenVals)])[i]).BinToString();
                    string val = (string)row.Cells[((int)PopulationStagesNames.ChildrenVals)].Value;
                    val = val.Insert((int)(((ArrayList)columns[((int)PopulationStagesNames.PairsCuts)])[i]), "|");
                    row.Cells[((int)PopulationStagesNames.ChildrenVals)].Value = val;
                }
                else
                {
                    row.Cells[((int)PopulationStagesNames.ChildrenVals)].Value =
                        "------";
                }

                row.Cells[((int)PopulationStagesNames.OffspringVals)].Value =
                    ((BitArray)((ArrayList)columns[((int)PopulationStagesNames.OffspringVals)])[i]).BinToString();

                string tmp = ((BitArray)((ArrayList)columns[((int)PopulationStagesNames.MutateVals)])[2 * i]).BinToString();
                int added = 0;
                for (int j = 0; j < tmp.Length; ++j)
                {
                    if (((BitArray)((ArrayList)columns[((int)PopulationStagesNames.MutateVals)])[2 * i + 1])[j - added] == true) {
                        if (j > 0 && j != tmp.Length - 1 && tmp[j - 1] == '|')
                        {
                            tmp = tmp.Remove(j-- - 1, 1);
                            tmp = tmp.Insert(j++ + 1, "|");
                        }
                        else if(j == tmp.Length - 1)
                        {
                            if (j > 0 && tmp[j - 1] == '|')
                            {
                                tmp = tmp.Remove(j-- - 1, 1);
                                continue;
                            }
                            tmp = tmp.Insert(j++, "|");
                        }
                        else
                        {
                            tmp = tmp.Insert(j++, "|");
                            tmp = tmp.Insert(j++ + 1, "|");
                            added += 2;
                        }
                    }
                }

                row.Cells[((int)PopulationStagesNames.MutateVals)].Value = tmp;

                row.Cells[(int)PopulationStagesNames.ResultVals].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.ResultVals)])[i])
                    .ToString("F" + InputProcessing.prec.ToString());

                row.Cells[(int)PopulationStagesNames.FinalGoalVals].Value =
                    ((double)((ArrayList)columns[((int)PopulationStagesNames.FinalGoalVals)])[i])
                    .ToString("F" + InputProcessing.prec.ToString());

                rows.Add(row);
            }

            dataGridView1.Rows.AddRange(rows.ToArray());
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        void textBox1_TextChanged(object sender, EventArgs e)
        {
            System.Drawing.SizeF mySize = new System.Drawing.SizeF();

            // Use the textbox font
            System.Drawing.Font myFont = textBox1.Font;

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
            System.Drawing.SizeF mySize = new System.Drawing.SizeF();

            // Use the textbox font
            System.Drawing.Font myFont = textBox2.Font;

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
            System.Drawing.SizeF mySize = new System.Drawing.SizeF();

            // Use the textbox font
            System.Drawing.Font myFont = textBox3.Font;

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
            System.Drawing.SizeF mySize = new System.Drawing.SizeF();

            // Use the textbox font
            System.Drawing.Font myFont = textBox4.Font;

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
            System.Drawing.SizeF mySize = new System.Drawing.SizeF();

            // Use the textbox font
            System.Drawing.Font myFont = textBox5.Font;

            using (Graphics g = CreateGraphics())
            {
                // Get the size given the string and the font
                mySize = g.MeasureString(textBox5.Text, myFont);
            }

            // Resize the textbox 
            textBox5.Width = (int)Math.Round(mySize.Width, 0) + 3;
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
        private Font boldFont;
        private Brush normalBrush;
        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Check if we are painting a data cell (ignore headers)
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Get the text from the cell
                string cellText = e.FormattedValue?.ToString() ?? string.Empty;

                // Define the delimiter to detect the text you want to highlight
                string delimiter = "|";

                // Only process cells containing the delimiter to minimize unnecessary operations
                if (!cellText.Contains(delimiter))
                {
                    return; // Let the default cell painting happen if no delimiter is present
                }

                // Begin custom painting
                e.Handled = true; // Prevent default painting
                e.PaintBackground(e.CellBounds, true); // Paint the background

                // Use DataGridView's font for consistency
                Font gridFont = e.CellStyle.Font;

                // Define the default and highlight text colors
                Color normalColor = Color.Black;
                Color highlightColor = Color.Red;

                // Margin settings
                int marginLeft = 5;
                int marginRight = 5;
                int currentX = e.CellBounds.X + marginLeft; // Default to left-aligned

                // Measure the entire cell width and adjust for alignment using TextRenderer
                Size totalSize = TextRenderer.MeasureText(string.Join("", cellText.Split('|')), gridFont);

                if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter)
                {
                    currentX = e.CellBounds.X + (e.CellBounds.Width - totalSize.Width) / 2;
                }
                else if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleRight)
                {
                    currentX = e.CellBounds.Right - totalSize.Width - marginRight;
                }

                // Calculate the Y position for vertically aligned text
                int textHeight = totalSize.Height;
                int currentY = e.CellBounds.Y + (e.CellBounds.Height - textHeight) / 2;

                // Draw each part of the string by highlighting delimiters
                int delimiterStartIndex = 0;
                bool isDelimiterPart = false;
                Color colorToUse = normalColor;

                for (int i = 0; i < cellText.Length; i++)
                {
                    
                    // Check if we encounter a delimiter
                    if (cellText[i] == delimiter[0])
                    {
                        // Draw the part before the delimiter
                        if (i != 0)
                        {
                            string partBeforeDelimiter = cellText.Substring(delimiterStartIndex, i - delimiterStartIndex);
                            Size partSize = TextRenderer.MeasureText(partBeforeDelimiter, gridFont);
                            TextRenderer.DrawText(e.Graphics, partBeforeDelimiter, gridFont, new Point(currentX, currentY), colorToUse);

                            // Move X forward

                            currentX += partSize.Width - 8;
                        }
                        // Now mark the next section as highlighted
                        delimiterStartIndex = i + 1;
                        isDelimiterPart = !isDelimiterPart;
                        colorToUse = isDelimiterPart ? highlightColor : normalColor;
                    }
                    else if (i == cellText.Length - 1)  // If it's the last part of the string
                    {
                        string lastPart = cellText.Substring(delimiterStartIndex, i - delimiterStartIndex + 1);
                        TextRenderer.DrawText(e.Graphics, lastPart, gridFont, new Point(currentX, currentY), colorToUse);
                    }
                }

                // Paint the focus rectangle if the cell is selected
                e.Paint(e.CellBounds, DataGridViewPaintParts.Focus);
            }
        }










    }
}
