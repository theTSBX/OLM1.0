using System.Windows.Forms;

namespace OutputLogManagerNEW.Components
{
    public class DoubleBufferedListView : ListView
    {
        public DoubleBufferedListView()
        {
            this.DoubleBuffered = true;
        }
    }
}
