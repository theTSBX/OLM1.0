namespace OutputLogManagerNEW.Forms
{
    partial class SettingsLocations
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Text = "Settings";
            this.ClientSize = new System.Drawing.Size(600, 250);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            for (int i = 1; i <= 5; i++)
            {
                var lbl = new System.Windows.Forms.Label
                {
                    Text = $"Location {i} Name:",
                    Location = new System.Drawing.Point(10, 10 + (i - 1) * 40),
                    Size = new System.Drawing.Size(100, 23)
                };
                var txtName = new System.Windows.Forms.TextBox
                {
                    Name = $"txtLocation{i}Name",
                    Location = new System.Drawing.Point(120, 10 + (i - 1) * 40),
                    Size = new System.Drawing.Size(150, 23)
                };
                var txtPath = new System.Windows.Forms.TextBox
                {
                    Name = $"txtLocation{i}Path",
                    Location = new System.Drawing.Point(280, 10 + (i - 1) * 40),
                    Size = new System.Drawing.Size(200, 23)
                };
                var btnBrowse = new System.Windows.Forms.Button
                {
                    Text = "...",
                    Location = new System.Drawing.Point(490, 10 + (i - 1) * 40),
                    Size = new System.Drawing.Size(30, 23),
                    Tag = i.ToString()
                };
                btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);

                this.Controls.Add(lbl);
                this.Controls.Add(txtName);
                this.Controls.Add(txtPath);
                this.Controls.Add(btnBrowse);
            }

            var btnSave = new System.Windows.Forms.Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(360, 220),
                DialogResult = System.Windows.Forms.DialogResult.OK
            };
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);

            var btnCancel = new System.Windows.Forms.Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(450, 220),
                DialogResult = System.Windows.Forms.DialogResult.Cancel
            };
            btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }
    }
}
