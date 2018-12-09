using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SLToolkit.DataForm.WPF.Automation;
using SLToolkit.DataForm.WPF.Controls.Common;

namespace SLToolkit.DataForm.WPF.Controls
{
    [TemplateVisualState(Name="InvalidFocused", GroupName="ValidationStates"), TemplateVisualState(Name="InvalidUnfocused", GroupName="ValidationStates"), TemplateVisualState(Name="Disabled", GroupName="CommonStates"), TemplateVisualState(Name="ValidUnfocused", GroupName="ValidationStates"), TemplateVisualState(Name="NoDescription", GroupName="DescriptionStates"), TemplateVisualState(Name="HasDescription", GroupName="DescriptionStates"), TemplateVisualState(Name="ValidFocused", GroupName="ValidationStates"), TemplateVisualState(Name="Normal", GroupName="CommonStates"), StyleTypedProperty(Property="ToolTipStyle", StyleTargetType=typeof(ToolTip))]
    public class DescriptionViewer : Control
    {
        private bool _descriptionOverridden;
        private bool _initialized;
        private new static readonly DependencyProperty DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), typeof(DescriptionViewer), new PropertyMetadata(new PropertyChangedCallback(DescriptionViewer.OnDataContextPropertyChanged)));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(DescriptionViewer), new PropertyMetadata(new PropertyChangedCallback(DescriptionViewer.OnDescriptionPropertyChanged)));
        public static readonly DependencyProperty GlyphTemplateProperty = DependencyProperty.Register("GlyphTemplate", typeof(ControlTemplate), typeof(DescriptionViewer), null);
        public static readonly DependencyProperty ToolTipStyleProperty = DependencyProperty.Register("ToolTipStyle", typeof(Style), typeof(DescriptionViewer), null);
        public new static readonly DependencyProperty IsFocusedProperty = DependencyProperty.Register("IsFocused", typeof(bool), typeof(DescriptionViewer), new PropertyMetadata(false, new PropertyChangedCallback(DescriptionViewer.OnIsFocusedPropertyChanged)));
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register("IsValid", typeof(bool), typeof(DescriptionViewer), new PropertyMetadata(true, new PropertyChangedCallback(DescriptionViewer.OnIsValidPropertyChanged)));
        public static readonly DependencyProperty PropertyPathProperty = DependencyProperty.Register("PropertyPath", typeof(string), typeof(DescriptionViewer), new PropertyMetadata(new PropertyChangedCallback(DescriptionViewer.OnPropertyPathPropertyChanged)));
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(DescriptionViewer), new PropertyMetadata(new PropertyChangedCallback(DescriptionViewer.OnTargetPropertyChanged)));

        public DescriptionViewer()
        {
            base.DefaultStyleKey = typeof(DescriptionViewer);
            base.SetBinding(DataContextProperty, new Binding());
            base.Loaded += new RoutedEventHandler(this.DescriptionViewer_Loaded);
            base.IsEnabledChanged += new DependencyPropertyChangedEventHandler(this.DescriptionViewer_IsEnabledChanged);
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.Description = typeof(DescriptionViewer).Name;
            }
        }

        private void DescriptionViewer_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UpdateCommonState();
        }

        private void DescriptionViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._initialized)
            {
                this.LoadMetadata(false);
                this._initialized = true;
                base.Loaded -= new RoutedEventHandler(this.DescriptionViewer_Loaded);
            }
        }

        private void LoadMetadata(bool forceUpdate)
        {
            ValidationMetadata objB = null;
            object entity = null;
            BindingExpression bindingExpression = null;
            if (!string.IsNullOrEmpty(this.PropertyPath))
            {
                entity = base.DataContext;
                objB = ValidationHelper.ParseMetadata(this.PropertyPath, entity);
            }
            else if (this.Target != null)
            {
                objB = ValidationHelper.ParseMetadata(this.Target, forceUpdate, out entity, out bindingExpression);
            }
            if (!ReferenceEquals(this.ValidationMetadata, objB))
            {
                this.ValidationMetadata = objB;
                if (!this._descriptionOverridden)
                {
                    string description = null;
                    if (this.ValidationMetadata != null)
                    {
                        description = this.ValidationMetadata.Description;
                    }
                    base.SetValue(DescriptionProperty, description);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.UpdateValidationState();
            this.UpdateDescriptionState();
        }

        protected override AutomationPeer OnCreateAutomationPeer() => 
            new DescriptionViewerAutomationPeer(this);

        private static void OnDataContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DescriptionViewer viewer = d as DescriptionViewer;
            if ((viewer != null) && (((e.OldValue == null) || (e.NewValue == null)) || !ReferenceEquals(e.OldValue.GetType(), e.NewValue.GetType())))
            {
                viewer.LoadMetadata(false);
            }
        }

        private static void OnDescriptionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            DescriptionViewer viewer = depObj as DescriptionViewer;
            if (viewer != null)
            {
                viewer.UpdateDescriptionState();
            }
        }

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Label label = d as Label;
            if ((label != null) && !label.AreHandlersSuspended())
            {
                label.SetValueNoCallback(IsFocusedProperty, e.OldValue);
                object[] args = new object[] { "IsFocused" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnIsValidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DescriptionViewer viewer = d as DescriptionViewer;
            if ((viewer != null) && !viewer.AreHandlersSuspended())
            {
                viewer.SetValueNoCallback(IsValidProperty, e.OldValue);
                object[] args = new object[] { "IsValid" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnPropertyPathPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            DescriptionViewer viewer = depObj as DescriptionViewer;
            if ((viewer != null) && viewer.Initialized)
            {
                viewer.LoadMetadata(false);
                viewer.ParseTargetValidState();
            }
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DescriptionViewer dependencyObject = d as DescriptionViewer;
            if (dependencyObject != null)
            {
                bool flag = false;
                //TODO: review this logic
                //flag = e.NewValue == FocusManager.GetFocusedElement(Window.GetWindow(dependencyObject));
                flag = e.NewValue == FocusManager.GetFocusedElement(dependencyObject);
                if (dependencyObject.IsFocused != flag)
                {
                    dependencyObject.IsFocused = flag;
                }
                dependencyObject.LoadMetadata(false);
                FrameworkElement oldValue = e.OldValue as FrameworkElement;
                FrameworkElement newValue = e.NewValue as FrameworkElement;
                EventHandler<ValidationErrorEventArgs> handler = new EventHandler<ValidationErrorEventArgs>(dependencyObject.Target_BindingValidationError);
                RoutedEventHandler handler2 = new RoutedEventHandler(dependencyObject.Target_GotFocus);
                RoutedEventHandler handler3 = new RoutedEventHandler(dependencyObject.Target_LostFocus);
                if (oldValue != null)
                {
                    //oldValue.BindingValidationError -= handler;
                    oldValue.RemoveHandler(Validation.ErrorEvent, handler);
                    oldValue.GotFocus -= handler2;
                    oldValue.LostFocus -= handler3;
                }
                if (newValue != null)
                {
                    //newValue.BindingValidationError += handler;
                    newValue.AddHandler(Validation.ErrorEvent, handler);
                    newValue.GotFocus += handler2;
                    newValue.LostFocus += handler3;
                }
                dependencyObject.ParseTargetValidState();
            }
        }

        private void ParseTargetValidState()
        {
            this.IsValid = !string.IsNullOrEmpty(this.PropertyPath) || ((this.Target == null) || !Validation.GetHasError(this.Target));
            this.UpdateValidationState();
        }

        public virtual void Refresh()
        {
            this._descriptionOverridden = false;
            this.LoadMetadata(true);
            this.ParseTargetValidState();
        }

        private void Target_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            this.ParseTargetValidState();
        }

        private void Target_GotFocus(object sender, RoutedEventArgs e)
        {
            this.IsFocused = true;
            this.UpdateValidationState();
        }

        private void Target_LostFocus(object sender, RoutedEventArgs e)
        {
            this.IsFocused = false;
            this.UpdateValidationState();
        }

        private void UpdateCommonState()
        {
            VisualStateManager.GoToState(this, base.IsEnabled ? "Normal" : "Disabled", true);
        }

        private void UpdateDescriptionState()
        {
            VisualStateManager.GoToState(this, string.IsNullOrEmpty(this.Description) ? "NoDescription" : "HasDescription", true);
        }

        private void UpdateValidationState()
        {
            if (!this.IsValid)
            {
                VisualStateManager.GoToState(this, this.IsFocused ? "InvalidFocused" : "InvalidUnfocused", true);
            }
            else if (!this.IsFocused || string.IsNullOrEmpty(this.Description))
            {
                VisualStateManager.GoToState(this, "ValidUnfocused", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "ValidFocused", true);
            }
        }

        public string Description
        {
            get => 
                (base.GetValue(DescriptionProperty) as string);
            set
            {
                this._descriptionOverridden = true;
                base.SetValue(DescriptionProperty, value);
            }
        }

        public ControlTemplate GlyphTemplate
        {
            get => 
                (base.GetValue(GlyphTemplateProperty) as ControlTemplate);
            set => 
                base.SetValue(GlyphTemplateProperty, value);
        }

        public Style ToolTipStyle
        {
            get => 
                (base.GetValue(ToolTipStyleProperty) as Style);
            set => 
                base.SetValue(ToolTipStyleProperty, value);
        }

        protected new bool IsFocused
        {
            get => 
                ((bool) base.GetValue(IsFocusedProperty));
            private set => 
                this.SetValueNoCallback(IsFocusedProperty, value);
        }

        public bool IsValid
        {
            get => 
                ((bool) base.GetValue(IsValidProperty));
            private set => 
                this.SetValueNoCallback(IsValidProperty, value);
        }

        public string PropertyPath
        {
            get => 
                (base.GetValue(PropertyPathProperty) as string);
            set => 
                base.SetValue(PropertyPathProperty, value);
        }

        public FrameworkElement Target
        {
            get => 
                (base.GetValue(TargetProperty) as FrameworkElement);
            set => 
                base.SetValue(TargetProperty, value);
        }

        internal ValidationMetadata ValidationMetadata { get; set; }

        internal new bool Initialized =>
            this._initialized;
    }
}

