using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using OutputLogManagerNEW.Forms;
using OutputLogManagerNEW.Components;
using OutputLogManagerNEW.Interfaces;
using OutputLogManagerNEW.Models;
using OutputLogManagerNEW.Components.Sorting;

namespace OutputLogManagerNEW
{
    public partial class OutputLogManager : Form
    {
        private FileTailer fileTailer;
        private LogParser logParser;
        private string currentFilePath = string.Empty;
        private ListViewColumnSorter listViewSorter;
        private ContextMenuStrip contextMenu;
        private int exceptionDisplayIndex = 0;
        public OutputLogManager()
        {
            InitializeComponent();
            fileTailer = new FileTailer();
            logParser = new LogParser();
            LoadLocations();
            listViewSorter = new ListViewColumnSorter();
            listViewOutputLogs.ListViewItemSorter = listViewSorter;
            listViewOutputLogs.ColumnClick += ListViewOutputLogs_ColumnClick;
            listViewOutputLogs.ItemActivate += listViewOutputLogs_ItemActivate;

            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Select Log", null, (_, __) => SelectLogFromContext());
            contextMenu.Items.Add("Open Log", null, (_, __) => OpenLogExternally());
            contextMenu.Items.Add("Open File Location", null, (_, __) => OpenFileLocation());
            contextMenu.Items.Add("Copy to Clipboard", null, (_, __) => CopyLogToClipboard());

            listViewOutputLogs.ContextMenuStrip = contextMenu;
            listViewOutputLogs.MouseClick += ListViewOutputLogs_MouseClick;
        }



        private void ListViewOutputLogs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == listViewSorter.SortColumn)
                listViewSorter.Order = listViewSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            else
            {
                listViewSorter.SortColumn = e.Column;
                listViewSorter.Order = SortOrder.Ascending;
            }

            listViewOutputLogs.Sort();
        }

        private void ListViewOutputLogs_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var item = listViewOutputLogs.HitTest(e.Location).Item;
                if (item != null)
                    item.Selected = true;
            }
        }

        private void SelectLogFromContext()
        {
            if (listViewOutputLogs.SelectedItems.Count == 0) return;
            var selectedItem = listViewOutputLogs.SelectedItems[0];
            if (selectedItem.Tag is string filePath && File.Exists(filePath))
            {
                currentFilePath = filePath;
                DisplayFileContent(filePath);
                foreach (ListViewItem item in listViewOutputLogs.Items)
                    item.Selected = item == selectedItem;
                selectedItem.EnsureVisible();
            }
        }

        private void OpenLogExternally()
        {
            if (listViewOutputLogs.SelectedItems.Count == 0) return;
            var filePath = listViewOutputLogs.SelectedItems[0].Tag?.ToString();
            if (filePath == null || !File.Exists(filePath)) return;
            Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
        }

        private void CopyLogToClipboard()
        {
            if (listViewOutputLogs.SelectedItems.Count == 0) return;
            var filePath = listViewOutputLogs.SelectedItems[0].Tag?.ToString();
            if (filePath == null || !File.Exists(filePath)) return;
            var collection = new System.Collections.Specialized.StringCollection();
            collection.Add(filePath);
            Clipboard.SetFileDropList(collection);
        }

        private void OpenFileLocation()
        {
            if (listViewOutputLogs.SelectedItems.Count == 0) return;
            var filePath = listViewOutputLogs.SelectedItems[0].Tag?.ToString();
            if (filePath == null || !File.Exists(filePath)) return;
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }

        private void listViewOutputLogs_ItemActivate(object sender, EventArgs e)
        {
            if (listViewOutputLogs.SelectedItems.Count == 0) return;
            var selectedItem = listViewOutputLogs.SelectedItems[0];
            if (selectedItem.Tag is string filePath && File.Exists(filePath))
            {
                currentFilePath = filePath;
                DisplayFileContent(filePath);
            }
        }

        private void CheckBoxTailing_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTailingToggle.Checked)
                fileTailer.StartTailing(currentFilePath, AppendLineToRichTextBox);
            else
                fileTailer.StopTailing();
        }

        private async void DisplayFileContent(string filePath)
        {
            if (!File.Exists(filePath)) return;
            if (!IsHandleCreated)
            {
                this.Shown += (_, _) => DisplayFileContent(filePath);
                return;
            }

            try
            {
                string fullText = await Task.Run(() =>
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(fileStream);
                    return reader.ReadToEnd();
                });

                Invoke(() =>
                {
                    Text = $"Output Log Manager - {Path.GetFileName(filePath)}";
                    richTextBoxRawOutput.Clear();
                    richTextBoxRawOutput.Text = fullText;
                    richTextBoxRawOutput.SelectionStart = richTextBoxRawOutput.TextLength;
                    richTextBoxRawOutput.ScrollToCaret();
                    LoadSummaryTab(); // <<<<<<<<<<<< ADDED THIS LINE TO TRIGGER SUMMARY UPDATE
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
                richTextBoxRawOutput.Invoke(new Action<string>(AppendLineToRichTextBox), text);
            else
            {
                richTextBoxRawOutput.AppendText(text);
                richTextBoxRawOutput.SelectionStart = richTextBoxRawOutput.TextLength;
                richTextBoxRawOutput.ScrollToCaret();
            }
        }

        private void TabControlRightPane_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSummaryTab();
        }


        private void LoadSummaryTab()
        {
            if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath)) return;
            string logContent = File.ReadAllText(currentFilePath);
            var summary = logParser.Parse(logContent);
            DisplaySummaryResults(summary);
        }

        private void DisplaySummaryResults(Dictionary<string, object> summary)
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

            var logTime = (TimeSpan)summary["log_duration"];
            var mods = (List<string>)summary["mods"];
            var joins = (List<string>)summary["player_joins"];
            var leaves = (List<string>)summary["player_leaves"];
            var allExceptions = summary["all_exceptions"] as List<string>;

            box.AppendText("Summary of Output Log:\n");
            box.AppendText("----------------------------\n");
            box.AppendText("🕱 Issues");

            box.AppendText($"\n-Exceptions: {allExceptions?.Count ?? 0}\n");
            box.AppendText($"-Errors: {summary["error_count"]}\n");
            box.AppendText($"-Warnings: {summary["warning_count"]}\n");
            box.AppendText("----------------------------\n");
            box.AppendText($"⏲ Log Duration: {logTime.Days}d {logTime.Hours}h {logTime.Minutes}m {logTime.Seconds}s\n");
            box.AppendText($"🌐 UTC Offset: {summary["utc_offset"]}\n");
            box.AppendText("----------------------------\n");
            box.AppendText("🖥️ System Info:\n");
            box.AppendText($"- Version: {summary["version"]}\n");
            box.AppendText($"- OS: {summary["os"]}\n");
            box.AppendText($"- CPU: {summary["cpu"]}\n");
            box.AppendText($"- RAM: {summary["ram"]}\n");
           
            string gpuRenderer = summary.ContainsKey("gpu") ? summary["gpu"].ToString() : "Unknown";
            string gpuVRAM = summary.ContainsKey("vram") ? summary["vram"].ToString() : "Unknown";
            string gpuDriver = summary.ContainsKey("driver") ? summary["driver"].ToString() : "Unknown";
            box.AppendText($"- GPU: {gpuRenderer}\n");
            box.AppendText($"  VRAM: {gpuVRAM}\n");
            box.AppendText($"  Driver: {gpuDriver}\n");
            box.AppendText("----------------------------\n");
            box.AppendText($"🧩 Mods Loaded ({mods.Count}):\n");
            foreach (var mod in mods)
                box.AppendText($"- {mod}\n");

            box.AppendText("----------------------------\n");
            box.AppendText($"♛ Players (Max Concurrent: {summary["max_players"]})\n\n");
            box.AppendText($"Player Joins ({joins.Count}):\n");
            foreach (var name in joins)
                box.AppendText($"• {name}\n");
            box.AppendText($"\nPlayer Leaves ({leaves.Count}):\n");
            foreach (var name in leaves)
                box.AppendText($"• {name}\n");
            box.AppendText("----------------------------\n");

            summaryTab.Controls.Add(box);

            string exc = summary["exc_block"]?.ToString();
            if (!string.IsNullOrWhiteSpace(exc))
            {
                box.AppendText($"\n🕱 First Exception:\n\n");
                box.AppendText(exc + "\n");
            }
            // -------------------------
            // Exceptions Tab
            // -------------------------
            if (tabControlRightPane.TabPages.ContainsKey("Exceptions"))
            {
                var exceptionsTab = tabControlRightPane.TabPages["Exceptions"];
                exceptionsTab.Controls.Clear();

                RichTextBox exceptionBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10),
                    BackColor = Color.Black,
                    ForeColor = Color.White
                };

                exceptionBox.AppendText($"Total Exceptions: {allExceptions?.Count ?? 0}\n");
                if (allExceptions != null)
                {
                    foreach (var exLine in allExceptions.Take(25))
                    {
                        exceptionBox.AppendText("------------------------------\n");
                        exceptionBox.AppendText(exLine + "\n\n");
                    }
                }

                exceptionsTab.Controls.Add(exceptionBox);
            }

            // -------------------------
            // Errors Tab
            // -------------------------
            if (tabControlRightPane.TabPages.ContainsKey("Errors"))
            {
                var errorsTab = tabControlRightPane.TabPages["Errors"];
                errorsTab.Controls.Clear();

                RichTextBox errorBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10),
                    BackColor = Color.Black,
                    ForeColor = Color.White
                };

                errorBox.AppendText($"Total Errors: {summary["error_count"]}\n");
                errorBox.AppendText("------------------------------\n");

                var allErrors = summary["all_errors"] as List<string>;
                if (allErrors != null)
                {
                    foreach (var err in allErrors.Take(25))
                        errorBox.AppendText(err + "\n\n");
                }

                errorsTab.Controls.Add(errorBox);
            }

            // -------------------------
            // Warnings Tab
            // -------------------------
            if (tabControlRightPane.TabPages.ContainsKey("Warnings"))
            {
                var warningsTab = tabControlRightPane.TabPages["Warnings"];
                warningsTab.Controls.Clear();

                RichTextBox warnBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10),
                    BackColor = Color.Black,
                    ForeColor = Color.White
                };

                warnBox.AppendText($"Total Warnings: {summary["warning_count"]}\n");
                var allWarnings = summary["all_warnings"] as List<string>;
                if (allWarnings != null)
                {
                    foreach (var wrn in allWarnings.Take(25))
                        warnBox.AppendText(wrn + "\n");
                }

                warningsTab.Controls.Add(warnBox);
            }


        }


        private void AppendExceptionsToBox(RichTextBox box, List<string> exceptions)
        {
            int count = 0;
            while (exceptionDisplayIndex < exceptions.Count && count < 10)
            {
                box.AppendText("------------------------------\n");
                box.AppendText(exceptions[exceptionDisplayIndex] + "\n\n");
                exceptionDisplayIndex++;
                count++;
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var settingsForm = new SettingsLocations();
            if (settingsForm.ShowDialog() == DialogResult.OK)
                LoadLocations();
        }

        private void LoadLocations()
        {
            comboBoxLocations.Items.Clear();

            string iniPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OutputLogManager",
                "locationsConfig.ini");

            if (!File.Exists(iniPath)) return;

            var ini = new Utils.IniFile(iniPath);
            for (int i = 1; i <= 5; i++)
            {
                string? name = ini.Read($"Location{i}", "LocationName");
                string? path = ini.Read($"Location{i}", "LocationPath");
                string? disabled = ini.Read($"Location{i}", "Disabled");

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path)) continue;
                if (!Directory.Exists(path)) continue;
                if (disabled?.Trim().ToLower() == "true") continue;

                comboBoxLocations.Items.Add(new LocationEntry { Name = name, Path = path });
            }

            if (comboBoxLocations.Items.Count > 0)
                comboBoxLocations.SelectedIndex = 0;
        }

        private void ComboBoxLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLocations.SelectedItem is not LocationEntry entry) return;

            currentFilePath = string.Empty;
            listViewOutputLogs.Items.Clear();

            if (!Directory.Exists(entry.Path)) return;

            string[] files = Directory.GetFiles(entry.Path, "*.txt", SearchOption.TopDirectoryOnly);
            Array.Sort(files);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var item = new ListViewItem(fileInfo.Name) { Tag = file };
                item.SubItems.Add(fileInfo.CreationTime.ToString("g"));
                item.SubItems.Add(fileInfo.LastWriteTime.ToString("g"));
                item.SubItems.Add($"{fileInfo.Length / 1024} KB");
                listViewOutputLogs.Items.Add(item);
            }

            if (listViewOutputLogs.Items.Count > 0)
            {
                currentFilePath = listViewOutputLogs.Items[0].Tag?.ToString() ?? "";
                DisplayFileContent(currentFilePath);
            }
        }
    }

    public class LocationEntry
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public override string ToString() => Name;
    }
}