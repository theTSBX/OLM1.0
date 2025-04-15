using System;
using System.IO;
using System.Windows.Forms;
using OutputLogManagerNEW.Utils;

namespace OutputLogManagerNEW.Forms
{
    public partial class SettingsLocations : Form
    {
        private readonly string iniPath;

        public SettingsLocations()
        {
            InitializeComponent();
            iniPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OutputLogManager",
                "locationsConfig.ini");

            Directory.CreateDirectory(Path.GetDirectoryName(iniPath));
            LoadSettings();
        }

        private void LoadSettings()
        {
            var ini = new IniFile(iniPath);
            for (int i = 1; i <= 5; i++)
            {
                var nameBox = FindControlRecursive(this, $"txtLocation{i}Name") as TextBox;
                var pathBox = FindControlRecursive(this, $"txtLocation{i}Path") as TextBox;

                if (nameBox != null) nameBox.Text = ini.Read($"Location{i}", "LocationName");
                if (pathBox != null) pathBox.Text = ini.Read($"Location{i}", "LocationPath");
            }

        }

        private void SaveSettings()
        {
            var ini = new IniFile(iniPath);
            for (int i = 1; i <= 5; i++)
            {
                var nameBox = FindControlRecursive(this, $"txtLocation{i}Name") as TextBox;
                var pathBox = FindControlRecursive(this, $"txtLocation{i}Path") as TextBox;

                if (nameBox != null)
                    ini.Write($"Location{i}", "LocationName", nameBox.Text);

                if (pathBox != null)
                    ini.Write($"Location{i}", "LocationPath", pathBox.Text);
            }

        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string target)
            {
                using var folder = new FolderBrowserDialog();
                if (folder.ShowDialog() == DialogResult.OK)
                {
                    Controls[$"txtLocation{target}Path"].Text = folder.SelectedPath;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private Control? FindControlRecursive(Control parent, string name)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl.Name == name)
                    return ctrl;

                var child = FindControlRecursive(ctrl, name);
                if (child != null)
                    return child;
            }
            return null;
        }


    }
}
