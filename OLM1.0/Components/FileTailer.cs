using System;
using System.IO;
using System.Windows.Forms;
using OutputLogManagerNEW.Interfaces;

namespace OutputLogManagerNEW.Components
{
    public class FileTailer : IFileTailer
    {
private System.Windows.Forms.Timer timer;        private string filePath;
        private long lastPosition;
        private Action<string> onNewLine;

        public FileTailer()
        {
            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += Timer_Tick;
        }

        public void StartTailing(string filePath, Action<string> onNewLine)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            this.filePath = filePath;
            this.onNewLine = onNewLine;
            this.lastPosition = new FileInfo(filePath).Length;
            timer.Start();
        }

        public void StopTailing()
        {
            timer.Stop();
            filePath = null;
            lastPosition = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Seek(lastPosition, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            onNewLine?.Invoke(line + Environment.NewLine);
                        }
                        lastPosition = stream.Position;
                    }
                }
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tailer error: {ex.Message}");
            }
        }
    }
}
