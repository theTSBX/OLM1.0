using System;
using System.Collections;
using System.Windows.Forms;

namespace OutputLogManagerNEW.Components.Sorting
{
    public class ListViewColumnSorter : IComparer
    {
        public int SortColumn { get; set; }
        public SortOrder Order { get; set; }

        public int Compare(object x, object y)
        {
            if (x is ListViewItem itemX && y is ListViewItem itemY)
            {
                string textX = itemX.SubItems[SortColumn].Text;
                string textY = itemY.SubItems[SortColumn].Text;
                int compareResult;

                if (SortColumn == 1 || SortColumn == 2)
                {
                    if (DateTime.TryParse(textX, out var dx) && DateTime.TryParse(textY, out var dy))
                        compareResult = dx.CompareTo(dy);
                    else
                        compareResult = string.Compare(textX, textY, StringComparison.Ordinal);
                }
                else if (SortColumn == 3)
                {
                    textX = textX.Replace("KB", "").Trim();
                    textY = textY.Replace("KB", "").Trim();
                    if (double.TryParse(textX, out var sizeX) && double.TryParse(textY, out var sizeY))
                        compareResult = sizeX.CompareTo(sizeY);
                    else
                        compareResult = string.Compare(textX, textY, StringComparison.Ordinal);
                }
                else
                {
                    compareResult = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);
                }

                return Order == SortOrder.Descending ? -compareResult : compareResult;
            }
            return 0;
        }
    }
}
