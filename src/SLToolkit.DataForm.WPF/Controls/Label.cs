using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using SLToolkit.DataForm.WPF.Controls.Common;
using LabelAutomationPeer = SLToolkit.DataForm.WPF.Automation.LabelAutomationPeer;

namespace SLToolkit.DataForm.WPF.Controls
{
    [TemplateVisualState(Name="Invalid", GroupName="ValidationStates"), TemplateVisualState(Name="Disabled", GroupName="CommonStates"), TemplateVisualState(Name="Normal", GroupName="CommonStates"), TemplateVisualState(Name="Valid", GroupName="ValidationStates"), TemplateVisualState(Name="NotRequired", GroupName="RequiredStates"), TemplateVisualState(Name="Required", GroupName="RequiredStates")]
    public class Label : ContentControl
    {
        private bool _initialized;
        private bool _isRequiredOverridden;
        private bool _canContentUseMetaData;
        private bool _isContentBeingSetInternally;
        private List<ValidationError> _errors;
        private new static readonly DependencyProperty DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), typeof(Label), new PropertyMetadata(new PropertyChangedCallback(Label.OnDataContextPropertyChanged)));
        public static readonly DependencyProperty IsRequiredProperty = DependencyProperty.Register("IsRequired", typeof(bool), typeof(Label), new PropertyMetadata(new PropertyChangedCallback(Label.OnIsRequiredPropertyChanged)));
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register("IsValid", typeof(bool), typeof(Label), new PropertyMetadata(true, new PropertyChangedCallback(Label.OnIsValidPropertyChanged)));
        public static readonly DependencyProperty PropertyPathProperty = DependencyProperty.Register("PropertyPath", typeof(string), typeof(Label), new PropertyMetadata(new PropertyChangedCallback(Label.OnPropertyPathPropertyChanged)));
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(Label), new PropertyMetadata(new PropertyChangedCallback(Label.OnTargetPropertyChanged)));

        public Label()
        {
            base.DefaultStyleKey = typeof(Label);
            this._errors = new List<ValidationError>();
            base.SetBinding(DataContextProperty, new Binding());
            base.Loaded += new RoutedEventHandler(this.Label_Loaded);
            base.IsEnabledChanged += new DependencyPropertyChangedEventHandler(this.Label_IsEnabledChanged);
            this._canContentUseMetaData = base.Content == null;
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.SetContentInternally(typeof(Label).Name);
            }
        }

        private void Label_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UpdateCommonState();
        }

        private void Label_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._initialized)
            {
                this.LoadMetadata(false);
                this._initialized = true;
                base.Loaded -= new RoutedEventHandler(this.Label_Loaded);
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
                if (this.ValidationMetadata == null)
                {
                    if (this._canContentUseMetaData)
                    {
                        this.SetContentInternally(null);
                    }
                }
                else
                {
                    string caption = this.ValidationMetadata.Caption;
                    if ((caption != null) && this._canContentUseMetaData)
                    {
                        this.SetContentInternally(caption);
                    }
                }
                if (!this._isRequiredOverridden)
                {
                    bool flag = (this.ValidationMetadata != null) && this.ValidationMetadata.IsRequired;
                    base.SetValue(IsRequiredProperty, flag);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.UpdateValidationState();
            this.UpdateRequiredState();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (DesignerProperties.GetIsInDesignMode(this) && (newContent == null))
            {
                this.SetContentInternally(typeof(Label).Name);
            }
            this._canContentUseMetaData = this._isContentBeingSetInternally || (newContent == null);
        }

        protected override AutomationPeer OnCreateAutomationPeer() => 
            new LabelAutomationPeer(this);

        private static void OnDataContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Label label = d as Label;
            if ((label != null) && (((e.OldValue == null) || (e.NewValue == null)) || !ReferenceEquals(e.OldValue.GetType(), e.NewValue.GetType())))
            {
                label.LoadMetadata(false);
            }
        }

        private static void OnIsRequiredPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            Label label = depObj as Label;
            if (label != null)
            {
                label.UpdateRequiredState();
            }
        }

        private static void OnIsValidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Label label = d as Label;
            if ((label != null) && !label.AreHandlersSuspended())
            {
                label.SetValueNoCallback(IsValidProperty, e.OldValue);
                object[] args = new object[] { "IsValid" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnPropertyPathPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            Label label = depObj as Label;
            if ((label != null) && label.Initialized)
            {
                label.LoadMetadata(false);
                label.ParseTargetValidState();
            }
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Label label))
                return;

            label.LoadMetadata(false);
            label._errors.Clear();
            var oldValue = e.OldValue as FrameworkElement;
            var newValue = e.NewValue as FrameworkElement;
            var handler = new EventHandler<ValidationErrorEventArgs>(label.Target_BindingValidationError);
            //oldValue.BindingValidationError -= handler;
            oldValue?.RemoveHandler(Validation.ErrorEvent, handler);
            if (newValue != null)
            {
                //newValue.BindingValidationError += handler;
                newValue.AddHandler(Validation.ErrorEvent, handler);
                System.Collections.ObjectModel.ReadOnlyObservableCollection<ValidationError> errors = Validation.GetErrors(newValue);
                if (errors.Count > 0)
                {
                    label._errors.Add(errors[0]);
                }
            }
            label.ParseTargetValidState();
        }

        private void ParseTargetValidState()
        {
            this.IsValid = this._errors.Count == 0;
            this.UpdateValidationState();
        }

        public virtual void Refresh()
        {
            this._isRequiredOverridden = false;
            this.LoadMetadata(true);
            this.ParseTargetValidState();
        }

        private void SetContentInternally(object value)
        {
            try
            {
                this._isContentBeingSetInternally = true;
                base.Content = value;
            }
            finally
            {
                this._isContentBeingSetInternally = false;
            }
        }

        private void Target_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                if (!this._errors.Contains(e.Error))
                {
                    this._errors.Add(e.Error);
                }
            }
            else if ((e.Action == ValidationErrorEventAction.Removed) && this._errors.Contains(e.Error))
            {
                this._errors.Remove(e.Error);
            }
            this.ParseTargetValidState();
        }

        private void UpdateCommonState()
        {
            VisualStateManager.GoToState(this, base.IsEnabled ? "Normal" : "Disabled", true);
        }

        private void UpdateRequiredState()
        {
            VisualStateManager.GoToState(this, this.IsRequired ? "Required" : "NotRequired", true);
        }

        private void UpdateValidationState()
        {
            VisualStateManager.GoToState(this, this.IsValid ? "Valid" : "Invalid", true);
        }

        public bool IsRequired
        {
            get => 
                ((bool) base.GetValue(IsRequiredProperty));
            set
            {
                this._isRequiredOverridden = true;
                base.SetValue(IsRequiredProperty, value);
            }
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

