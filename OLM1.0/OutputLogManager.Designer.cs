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
           
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.listViewOutputLogs = new DoubleBufferedListView();
            this.checkBoxTailingToggle = new System.Windows.Forms.CheckBox();
            this.comboBoxLocations = new System.Windows.Forms.ComboBox();

           
            this.richTextBoxRawOutput = new System.Windows.Forms.RichTextBox();

          
            this.tabControlRightPane = new System.Windows.Forms.TabControl();

            this.tabControlRightPane.SelectedIndexChanged += new System.EventHandler(this.TabControlRightPane_SelectedIndexChanged);
            // menuStrip1

            this.menuStrip1.Items.AddRange(new ToolStripItem[] {
                this.fileToolStripMenuItem
            });
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1200, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";

       
            // fileToolStripMenuItem
          
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.settingsToolStripMenuItem
            });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";

            
            // settingsToolStripMenuItem
            
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);

            
            // listViewOutputLogs
            
            this.listViewOutputLogs.Location = new System.Drawing.Point(12, 60);
            this.listViewOutputLogs.Name = "listViewOutputLogs";
            this.listViewOutputLogs.Size = new System.Drawing.Size(440, 370);
            this.listViewOutputLogs.TabIndex = 1;
            this.listViewOutputLogs.View = View.Details;
            this.listViewOutputLogs.Columns.Add("Name", 140);
            this.listViewOutputLogs.Columns.Add("Date Created", 120);
            this.listViewOutputLogs.Columns.Add("Date Modified", 120);
            this.listViewOutputLogs.Columns.Add("File Size", 60);
            this.listViewOutputLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            
            // checkBoxTailingToggle
            
            this.checkBoxTailingToggle.AutoSize = true;
            this.checkBoxTailingToggle.Location = new System.Drawing.Point(530, 30);
            this.checkBoxTailingToggle.Name = "checkBoxTailingToggle";
            this.checkBoxTailingToggle.Size = new System.Drawing.Size(93, 17);
            this.checkBoxTailingToggle.TabIndex = 2;
            this.checkBoxTailingToggle.Text = "Enable Tailing";
            this.checkBoxTailingToggle.UseVisualStyleBackColor = true;
            this.checkBoxTailingToggle.CheckedChanged += new System.EventHandler(this.CheckBoxTailing_CheckedChanged);

            
            // comboBoxLocations
            
            this.comboBoxLocations.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxLocations.FormattingEnabled = true;
            this.comboBoxLocations.Location = new System.Drawing.Point(12, 27);
            this.comboBoxLocations.Name = "comboBoxLocations";
            this.comboBoxLocations.Size = new System.Drawing.Size(200, 21);
            this.comboBoxLocations.TabIndex = 3;
            this.comboBoxLocations.SelectedIndexChanged += new System.EventHandler(this.ComboBoxLocations_SelectedIndexChanged);

            
            // richTextBoxRawOutput
            
            this.richTextBoxRawOutput.Location = new System.Drawing.Point(530, 60);
            this.richTextBoxRawOutput.Name = "richTextBoxRawOutput";
            this.richTextBoxRawOutput.Size = new System.Drawing.Size(900, 370);
            this.richTextBoxRawOutput.ReadOnly = true;
            this.richTextBoxRawOutput.Font = new Font("Consolas", 9f);
            this.richTextBoxRawOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.richTextBoxRawOutput.BackColor = Color.Black;
            this.richTextBoxRawOutput.ForeColor = Color.White;

            // tabControlRightPane
            this.tabControlRightPane = new System.Windows.Forms.TabControl();
            this.tabControlRightPane.Location = new System.Drawing.Point(900, 60);
            this.tabControlRightPane.Name = "tabControlRightPane";
            this.tabControlRightPane.Size = new System.Drawing.Size(300, 570);
            this.tabControlRightPane.TabIndex = 4;
            this.tabControlRightPane.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

            // Add tabs with unique names and titles
            TabPage tab1 = new TabPage("Summary") { Name = "Summary" };
            TabPage tab2 = new TabPage("Tab 2") { Name = "Tab2" };
            TabPage tab3 = new TabPage("Tab 3") { Name = "Tab3" };
            TabPage tab4 = new TabPage("Tab 4") { Name = "Tab4" };

            this.tabControlRightPane.TabPages.Add(tab1);
            this.tabControlRightPane.TabPages.Add(tab2);
            this.tabControlRightPane.TabPages.Add(tab3);
            this.tabControlRightPane.TabPages.Add(tab4);
            this.Controls.Add(this.tabControlRightPane);

            // OutputLogManager

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 450);
            this.Controls.Add(this.listViewOutputLogs);
            this.Controls.Add(this.checkBoxTailingToggle);
            this.Controls.Add(this.comboBoxLocations);
            this.Controls.Add(this.richTextBoxRawOutput);
            this.Controls.Add(this.tabControlRightPane);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "OutputLogManager";
            this.Text = "Output Log Manager";
            this.WindowState = FormWindowState.Maximized;

            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
