namespace SLToolkit.DataForm.WPF.Controls
{
    internal class ValidationItemCollection : System.Collections.ObjectModel.ObservableCollection<ValidationSummaryItem>
    {
        internal void ClearErrors(ValidationSummaryItemType errorType)
        {
            ValidationItemCollection items = new ValidationItemCollection();
            foreach (ValidationSummaryItem item in this)
            {
                if (item == null)
                {
                    continue;
                }
                if (item.ItemType == errorType)
                {
                    items.Add(item);
                }
            }
            foreach (ValidationSummaryItem item2 in items)
            {
                base.Remove(item2);
            }
        }

        protected override void ClearItems()
        {
            while (base.Count > 0)
            {
                base.RemoveAt(0);
            }
        }
    }
}

