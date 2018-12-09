using System.Windows;

namespace SLToolkit.DataForm.WPF.Controls.Common
{
    internal class ExtensionProperties : DependencyObject
    {
        public static readonly DependencyProperty AreHandlersSuspended = DependencyProperty.RegisterAttached("AreHandlersSuspended", typeof(bool), typeof(ExtensionProperties), new PropertyMetadata(false));

        public static bool GetAreHandlersSuspended(DependencyObject obj) => 
            ((bool) obj.GetValue(AreHandlersSuspended));

        public static void SetAreHandlersSuspended(DependencyObject obj, bool value)
        {
            obj.SetValue(AreHandlersSuspended, value);
        }
    }
}

