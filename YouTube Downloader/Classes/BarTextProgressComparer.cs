using System;
using System.Collections;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YouTube_Downloader.Controls;

namespace YouTube_Downloader.Classes
{
    class BarTextProgressComparer : IComparer
    {
        private SortOrder order;

        public BarTextProgressComparer(SortOrder order)
        {
            this.order = order;
        }

        public int Compare(object x, object y)
        {
            var operationX = this.GetOperation(x);
            var operationY = this.GetOperation(y);

            if (operationX.Operation.IsWorking && operationY.Operation.IsWorking)
            {
                // Sort by progress if both is working
                return order == SortOrder.Descending ?
                    operationX.BarTextProgress.Progress.CompareTo(operationY.BarTextProgress.Progress) :
                    operationY.BarTextProgress.Progress.CompareTo(operationX.BarTextProgress.Progress);
            }
            else if (operationX.Operation.IsWorking && !operationY.Operation.IsWorking)
            {
                // Sort working first, ignoring progress
                return order == SortOrder.Descending ? 1 : 0;
            }
            else if (!operationX.Operation.IsWorking && operationY.Operation.IsWorking)
            {
                // Sort working first, ignoring progress
                return order == SortOrder.Descending ? 0 : 1;
            }
            else
            {
                // Is neither operation is working, sort by status alphabetical
                return order == SortOrder.Descending ?
                    operationY.BarTextProgress.Text.CompareTo(operationX.BarTextProgress.Text) :
                    operationX.BarTextProgress.Text.CompareTo(operationY.BarTextProgress.Text);
            }
        }

        private OperationModel GetOperation(object item)
        {
            if (!(item is ListViewItem))
                throw new ArgumentException("Expecting " + nameof(ListViewItem));

            var lvitem = item as ListViewItem;

            if (!(lvitem.ListView is ObjectListView))
                throw new ArgumentException("Expecting " + nameof(ObjectListView));

            return (OperationModel)
                (lvitem.ListView as ObjectListView).GetModelObject(lvitem.Index);
        }
    }
}
