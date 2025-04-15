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
            var selectedTab = tabControlRightPane.SelectedTab;
            if (selectedTab.Text == "Summary")
                LoadSummaryTab();
            else
                selectedTab.Controls.Clear();
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

            box.AppendText("Summary of Output Log:\n");
            box.AppendText("----------------------------\n");
            box.AppendText($"🧭 Log Duration: {logTime.Days}d {logTime.Hours}h {logTime.Minutes}m {logTime.Seconds}s\n\n");

            box.AppendText("🖥️ System Info:\n");
            box.AppendText($"- Version: {summary["version"]}\n");
            box.AppendText($"- OS: {summary["os"]}\n");
            box.AppendText($"- CPU: {summary["cpu"]}\n");
            box.AppendText($"- RAM: {summary["ram"]}\n");

            string gpuBlock = summary["gpu"]?.ToString() ?? "";
            string gpuRenderer = "Unknown";
            string gpuVRAM = "Unknown";
            string gpuDriver = "Unknown";

            if (!string.IsNullOrWhiteSpace(gpuBlock))
            {
                foreach (string line in gpuBlock.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("Renderer:"))
                        gpuRenderer = trimmed.Substring("Renderer:".Length).Trim();
                    else if (trimmed.StartsWith("VRAM:"))
                        gpuVRAM = trimmed.Substring("VRAM:".Length).Trim();
                    else if (trimmed.StartsWith("Driver:"))
                        gpuDriver = trimmed.Substring("Driver:".Length).Trim();
                }
            }

            box.AppendText($"- GPU: {gpuRenderer}\n");
            box.AppendText($"  VRAM: {gpuVRAM}\n");
            box.AppendText($"  Driver: {gpuDriver}\n\n");


            box.AppendText($"🧩 Mods Loaded ({mods.Count}):\n");
            foreach (var mod in mods)
                box.AppendText($"- {mod}\n");

            box.AppendText($"\n⚠️ Warnings: {summary["warning_count"]}\n");
            box.AppendText($"🛑 Errors: {summary["error_count"]}\n\n");

            box.AppendText($"👥 Player Joins ({joins.Count}):\n");
            foreach (var name in joins)
                box.AppendText($"- {name}\n");

            box.AppendText($"\n🚪 Player Leaves ({leaves.Count}):\n");
            foreach (var name in leaves)
                box.AppendText($"- {name}\n");

            summaryTab.Controls.Add(box);
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
