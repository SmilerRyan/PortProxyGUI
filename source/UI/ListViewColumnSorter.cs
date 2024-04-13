using System.Collections;
using System.Windows.Forms;
namespace PortProxyGUI.UI {
    public class ListViewColumnSorter : IComparer {
        private int ColumnToSort;
        private SortOrder OrderOfSort;
        private CaseInsensitiveComparer ObjectCompare;
        public ListViewColumnSorter() {
            ColumnToSort = 0;
            OrderOfSort = SortOrder.None;
            ObjectCompare = new CaseInsensitiveComparer();
        }
        public int Compare(object x, object y) {
            int compareResult;
            ListViewItem listviewX, listviewY;
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;
            if (listviewX.SubItems[ColumnToSort].Tag?.ToString() == "Number" && listviewY.SubItems[ColumnToSort].Tag?.ToString() == "Number") {
                int.TryParse(listviewX.SubItems[ColumnToSort].Text, out var xint);
                int.TryParse(listviewY.SubItems[ColumnToSort].Text, out var yint);
                compareResult = ObjectCompare.Compare(xint, yint);
            } else compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
            if (OrderOfSort == SortOrder.Ascending) {
                return compareResult;
            } else if (OrderOfSort == SortOrder.Descending) {
                return -compareResult;
            } else {
                return 0;
            }
        }
        public int SortColumn {
            set { ColumnToSort = value; }
            get { return ColumnToSort; }
        }
        public SortOrder Order {
            set { OrderOfSort = value; }
            get { return OrderOfSort; }
        }
    }
}
