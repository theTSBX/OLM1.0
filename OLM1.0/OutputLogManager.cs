using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using OutputLogManagerNEW.Components;
using OutputLogManagerNEW.Interfaces;

namespace OutputLogManagerNEW
{
    public partial class OutputLogManager : Form
    {
        private FileTailer fileTailer;
        private LogParser logParser;
        private string currentFilePath = string.Empty;

        public OutputLogManager()
        {
            InitializeComponent();
            fileTailer = new FileTailer();
            logParser = new LogParser();
        }

        private void CheckBoxTailing_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTailingToggle.Checked)
            {
                fileTailer.StartTailing(currentFilePath, AppendLineToRichTextBox);
            }
            else
            {
                fileTailer.StopTailing();
            }
        }

        private async void DisplayFileContent(string filePath)
        {
            if (!File.Exists(filePath)) return;

            richTextBoxRawOutput.Clear();

            try
            {
                await Task.Run(() =>
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            AppendLineToRichTextBox(line + Environment.NewLine);
                        }
                    }
                });
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        private void AppendLineToRichTextBox(string text)
        {
            if (richTextBoxRawOutput.InvokeRequired)
            {
                richTextBoxRawOutput.Invoke(new Action<string>(AppendLineToRichTextBox), text);
                return;
            }

            richTextBoxRawOutput.AppendText(text);
            richTextBoxRawOutput.SelectionStart = richTextBoxRawOutput.TextLength;
            richTextBoxRawOutput.ScrollToCaret();
        }

        private void TabControlRightPane_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = tabControlRightPane.SelectedTab;

            if (selectedTab.Text == "Summary")
            {
                LoadSummaryTab();
            }
            else
            {
                selectedTab.Controls.Clear();
            }
        }

        private void LoadSummaryTab()
        {
            if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath))
            {
                MessageBox.Show("Please select a file to summarize.",
                                "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string logContent = File.ReadAllText(currentFilePath);
            var summary = logParser.Parse(logContent);
            DisplaySummaryResults(summary);
        }

        private void DisplaySummaryResults(Dictionary<string, int> summaryResults)
        {
            var summaryTab = tabControlRightPane.TabPages["Summary"];
            summaryTab.Controls.Clear();

            RichTextBox box = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            box.AppendText("Summary of Output Log:\n");
            box.AppendText("----------------------------\n");
            foreach (var kvp in summaryResults)
            {
                box.AppendText($"{kvp.Key}: {kvp.Value}\n");
            }

            summaryTab.Controls.Add(box);
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Settings dialog not implemented yet.");
        }

        private void ComboBoxLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLocations.SelectedItem == null) return;

            var selectedLocation = comboBoxLocations.SelectedItem.ToString();
            var folder = selectedLocation;
            if (Directory.Exists(folder))
            {
                var firstLog = Directory.GetFiles(folder, "*.txt");
                if (firstLog.Length > 0)
                {
                    currentFilePath = firstLog[0];
                    DisplayFileContent(currentFilePath);
                }
            }
        }
    }
}
