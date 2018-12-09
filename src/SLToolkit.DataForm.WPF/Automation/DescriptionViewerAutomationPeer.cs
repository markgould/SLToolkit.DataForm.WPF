using System.Windows;
using System.Windows.Automation.Peers;
using SLToolkit.DataForm.WPF.Controls;

namespace SLToolkit.DataForm.WPF.Automation
{
    public class DescriptionViewerAutomationPeer : FrameworkElementAutomationPeer
    {
        public DescriptionViewerAutomationPeer(DescriptionViewer owner) : base((FrameworkElement) owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore() => 
            AutomationControlType.Text;

        protected override string GetClassNameCore() => 
            typeof(DescriptionViewer).Name;

        protected override string GetNameCore()
        {
            DescriptionViewer owner = base.Owner as DescriptionViewer;
            return ((owner == null) ? base.GetNameCore() : owner.Description);
        }
    }
}

