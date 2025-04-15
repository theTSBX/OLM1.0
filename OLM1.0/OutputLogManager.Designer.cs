using System.Drawing;
using System.Windows.Forms;
using OutputLogManagerNEW.Components;

namespace OutputLogManagerNEW
{
    partial class OutputLogManager
    {
        private System.ComponentModel.IContainer components = null;

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private DoubleBufferedListView listViewOutputLogs;
        private CheckBox checkBoxTailingToggle;
        private ComboBox comboBoxLocations;
        private RichTextBox richTextBoxRawOutput;
        private TabControl tabControlRightPane; // New TabControl for the third pane

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        /// Initializes the form controls.

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            listViewOutputLogs = new DoubleBufferedListView();
            checkBoxTailingToggle = new CheckBox();
            comboBoxLocations = new ComboBox();
            richTextBoxRawOutput = new RichTextBox();
            tabControlRightPane = new TabControl();

            // MenuStrip
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1440, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";

            // File menu
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { settingsToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";

            // Settings menu item
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(116, 22);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += SettingsToolStripMenuItem_Click;

            // ComboBox Locations
            comboBoxLocations.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxLocations.FormattingEnabled = true;
            comboBoxLocations.Location = new Point(14, 31);
            comboBoxLocations.Name = "comboBoxLocations";
            comboBoxLocations.Size = new Size(233, 23);
            comboBoxLocations.TabIndex = 1;
            comboBoxLocations.SelectedIndexChanged += ComboBoxLocations_SelectedIndexChanged;

            // CheckBox Tailing Toggle
            checkBoxTailingToggle.AutoSize = true;
            checkBoxTailingToggle.Location = new Point(530, 34);
            checkBoxTailingToggle.Name = "checkBoxTailingToggle";
            checkBoxTailingToggle.Size = new Size(99, 19);
            checkBoxTailingToggle.TabIndex = 2;
            checkBoxTailingToggle.Text = "Enable Tailing";
            checkBoxTailingToggle.UseVisualStyleBackColor = true;
            checkBoxTailingToggle.CheckedChanged += CheckBoxTailing_CheckedChanged;

            // ListView Output Logs
            listViewOutputLogs.Location = new Point(14, 60);
            listViewOutputLogs.Name = "listViewOutputLogs";
            listViewOutputLogs.Size = new Size(440, 700);
            listViewOutputLogs.TabIndex = 3;
            listViewOutputLogs.View = View.Details;
            listViewOutputLogs.FullRowSelect = true;
            listViewOutputLogs.Columns.Add("Name", 140);
            listViewOutputLogs.Columns.Add("Date Created", 120);
            listViewOutputLogs.Columns.Add("Date Modified", 120);
            listViewOutputLogs.Columns.Add("File Size", 60);
            listViewOutputLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // RichTextBox Raw Output
            richTextBoxRawOutput.Location = new Point(460, 60); 
            richTextBoxRawOutput.Name = "richTextBoxRawOutput";
            richTextBoxRawOutput.Size = new Size(600, 700); 
            richTextBoxRawOutput.ReadOnly = true;
            richTextBoxRawOutput.Font = new Font("Consolas", 9F);
            richTextBoxRawOutput.BackColor = Color.Black;
            richTextBoxRawOutput.ForeColor = Color.White;
            richTextBoxRawOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // TabControl Right Pane
            tabControlRightPane.Name = "tabControlRightPane";
            tabControlRightPane.Location = new Point(1065, 60);
            tabControlRightPane.Size = new Size(340, 700); 
            tabControlRightPane.TabIndex = 4;
            tabControlRightPane.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right; tabControlRightPane.SelectedIndexChanged += TabControlRightPane_SelectedIndexChanged;

            var tab1 = new TabPage("Summary");
            tab1.Name = "Summary";
            var tab2 = new TabPage("Tab 2");
            var tab3 = new TabPage("Tab 3");
            var tab4 = new TabPage("Tab 4");
            tabControlRightPane.TabPages.Add(tab1);
            tabControlRightPane.TabPages.Add(tab2);
            tabControlRightPane.TabPages.Add(tab3);
            tabControlRightPane.TabPages.Add(tab4);

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1440, 801);
            Controls.Add(menuStrip1);
            Controls.Add(comboBoxLocations);
            Controls.Add(checkBoxTailingToggle);
            Controls.Add(listViewOutputLogs);
            Controls.Add(richTextBoxRawOutput);
            Controls.Add(tabControlRightPane);
            MainMenuStrip = menuStrip1;
            Name = "OutputLogManager";
            Text = "Output Log Manager";
            WindowState = FormWindowState.Maximized;

            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

    }
}
