namespace SLToolkit.DataForm.WPF.Controls
{
    public class ValidationSummaryItemSource
    {
        public ValidationSummaryItemSource(string propertyName) : this(propertyName, null)
        {
        }

        public ValidationSummaryItemSource(string propertyName, System.Windows.Controls.Control control)
        {
            this.PropertyName = propertyName;
            this.Control = control;
        }

        public override bool Equals(object obj)
        {
            ValidationSummaryItemSource source = obj as ValidationSummaryItemSource;
            return ((source != null) ? ((this.PropertyName == source.PropertyName) && ReferenceEquals(this.Control, source.Control)) : false);
        }

        public override int GetHashCode() => 
            (this.PropertyName + "." + this.Control.Name).GetHashCode();

        public string PropertyName { get; private set; }

        public System.Windows.Controls.Control Control { get; private set; }
    }
}

