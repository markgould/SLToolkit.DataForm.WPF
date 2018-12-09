using System;

namespace SLToolkit.DataForm.WPF.Controls
{
    public class FocusingInvalidControlEventArgs : EventArgs
    {
        public FocusingInvalidControlEventArgs(ValidationSummaryItem item, ValidationSummaryItemSource target)
        {
            this.Handled = false;
            this.Item = item;
            this.Target = target;
        }

        public bool Handled { get; set; }

        public ValidationSummaryItem Item { get; private set; }

        public ValidationSummaryItemSource Target { get; set; }
    }
}

