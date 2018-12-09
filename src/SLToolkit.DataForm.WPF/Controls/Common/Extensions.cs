using System.Windows;

namespace SLToolkit.DataForm.WPF.Controls.Common
{
    internal static class Extensions
    {
        public static bool AreHandlersSuspended(this DependencyObject obj) => 
            ExtensionProperties.GetAreHandlersSuspended(obj);

        public static void SetValueNoCallback(this DependencyObject obj, DependencyProperty property, object value)
        {
            ExtensionProperties.SetAreHandlersSuspended(obj, true);
            try
            {
                obj.SetValue(property, value);
            }
            finally
            {
                ExtensionProperties.SetAreHandlersSuspended(obj, false);
            }
        }
    }
}

