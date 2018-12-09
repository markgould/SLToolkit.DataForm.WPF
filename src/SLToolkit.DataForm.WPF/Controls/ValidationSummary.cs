using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SLToolkit.DataForm.WPF.Automation;
using SLToolkit.DataForm.WPF.Controls.Common;

namespace SLToolkit.DataForm.WPF.Controls
{
    [TemplateVisualState(Name="Normal", GroupName="CommonStates"), TemplatePart(Name="SummaryListBox", Type=typeof(ListBox)), TemplateVisualState(Name="Disabled", GroupName="CommonStates"), TemplateVisualState(Name="HasErrors", GroupName="ValidationStates"), StyleTypedProperty(Property="SummaryListBoxStyle", StyleTargetType=typeof(ListBox)), TemplateVisualState(Name="Empty", GroupName="ValidationStates"), StyleTypedProperty(Property="ErrorStyle", StyleTargetType=typeof(ListBoxItem))]
    public class ValidationSummary : Control
    {
        private const string PART_SummaryListBox = "SummaryListBox";
        private const string PART_HeaderContentControl = "HeaderContentControl";
        private ValidationSummaryItemSource _currentValidationSummaryItemSource;
        private ValidationItemCollection _displayedErrors;
        private ValidationItemCollection _errors;
        private ListBox _errorsListBox;
        private ContentControl _headerContentControl;
        private bool _initialized;
        private FrameworkElement _registeredParent;
        private Dictionary<string, ValidationSummaryItem> _validationSummaryItemDictionary;
        public static readonly DependencyProperty ShowErrorsInSummaryProperty = DependencyProperty.RegisterAttached("ShowErrorsInSummary", typeof(bool), typeof(ValidationSummary), new PropertyMetadata(true, new PropertyChangedCallback(ValidationSummary.OnShowErrorsInSummaryPropertyChanged)));
        public static readonly DependencyProperty ErrorStyleProperty = DependencyProperty.Register("ErrorStyle", typeof(Style), typeof(ValidationSummary), null);
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(ValidationSummaryFilters), typeof(ValidationSummary), new PropertyMetadata(ValidationSummaryFilters.All, new PropertyChangedCallback(ValidationSummary.OnFilterPropertyChanged)));
        public static readonly DependencyProperty FocusControlsOnClickProperty = DependencyProperty.Register("FocusControlsOnClick", typeof(bool), typeof(ValidationSummary), new PropertyMetadata(true, null));
        public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.Register("HasErrors", typeof(bool), typeof(ValidationSummary), new PropertyMetadata(false, new PropertyChangedCallback(ValidationSummary.OnHasErrorsPropertyChanged)));
        public static readonly DependencyProperty HasDisplayedErrorsProperty = DependencyProperty.Register("HasDisplayedErrors", typeof(bool), typeof(ValidationSummary), new PropertyMetadata(false, new PropertyChangedCallback(ValidationSummary.OnHasDisplayedErrorsPropertyChanged)));
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ValidationSummary), new PropertyMetadata(new PropertyChangedCallback(ValidationSummary.OnHasHeaderPropertyChanged)));
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(ValidationSummary), null);
        public static readonly DependencyProperty SummaryListBoxStyleProperty = DependencyProperty.Register("SummaryListBoxStyle", typeof(Style), typeof(ValidationSummary), null);
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(UIElement), typeof(ValidationSummary), new PropertyMetadata(new PropertyChangedCallback(ValidationSummary.OnTargetPropertyChanged)));

        public event EventHandler<FocusingInvalidControlEventArgs> FocusingInvalidControl;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;    

        public ValidationSummary()
        {
            base.DefaultStyleKey = typeof(ValidationSummary);
            this._errors = new ValidationItemCollection();
            this._validationSummaryItemDictionary = new Dictionary<string, ValidationSummaryItem>();
            this._displayedErrors = new ValidationItemCollection();
            this._errors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(this.Errors_CollectionChanged);
            base.Loaded += new RoutedEventHandler(this.ValidationSummary_Loaded);
            base.Unloaded += new RoutedEventHandler(this.ValidationSummary_Unloaded);
            base.IsEnabledChanged += new DependencyPropertyChangedEventHandler(this.ValidationSummary_IsEnabledChanged);
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.Errors.Add(new ValidationSummaryItem(Properties.Resources.ValidationSummarySampleError, typeof(ValidationSummaryItem).Name, ValidationSummaryItemType.ObjectError, null, null));
                this.Errors.Add(new ValidationSummaryItem(Properties.Resources.ValidationSummarySampleError, typeof(ValidationSummaryItem).Name, ValidationSummaryItemType.ObjectError, null, null));
                this.Errors.Add(new ValidationSummaryItem(Properties.Resources.ValidationSummarySampleError, typeof(ValidationSummaryItem).Name, ValidationSummaryItemType.ObjectError, null, null));
            }
        }

        internal static int CompareValidationSummaryItems(ValidationSummaryItem x, ValidationSummaryItem y)
        {
            int num;
            if (ReferencesAreValid(x, y, out num))
            {
                if (TryCompareReferences(x.ItemType, y.ItemType, out num))
                {
                    return num;
                }
                var objA = (x.Sources.Count > 0) ? x.Sources[0].Control : null;
                var objB = (y.Sources.Count > 0) ? y.Sources[0].Control : null;
                if (!ReferenceEquals(objA, objB))
                {
                    if (!ReferencesAreValid(objA, objB, out num))
                    {
                        return num;
                    }
                    if (objA.TabIndex != objB.TabIndex)
                    {
                        return objA.TabIndex.CompareTo(objB.TabIndex);
                    }
                    num = SortByVisualTreeOrdering(objA, objB);
                    if (num != 0)
                    {
                        return num;
                    }
                    if (TryCompareReferences(objA.Name, objB.Name, out num))
                    {
                        return num;
                    }
                }
                if (!TryCompareReferences(x.MessageHeader, y.MessageHeader, out num))
                {
                    TryCompareReferences(x.Message, y.Message, out num);
                }
            }
            return num;
        }

        private void Errors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ValidationSummaryItem item in e.OldItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged -= new PropertyChangedEventHandler(this.ValidationSummaryItem_PropertyChanged);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (ValidationSummaryItem item2 in e.NewItems)
                {
                    if (item2 != null)
                    {
                        item2.PropertyChanged += new PropertyChangedEventHandler(this.ValidationSummaryItem_PropertyChanged);
                    }
                }
            }
            this.HasErrors = this._errors.Count > 0;
            this.UpdateDisplayedErrors();
        }

        private void ErrorsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ExecuteClick(sender);
            }
        }

        private void ErrorsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ExecuteClick(sender);
        }

        private void ErrorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectionChanged = this.SelectionChanged;
            selectionChanged?.Invoke(this, e);
        }

        private void ExecuteClick(object sender)
        {
            var box = sender as ListBox;
            if (box != null)
            {
                var selectedItem = box.SelectedItem as ValidationSummaryItem;
                if ((selectedItem != null) && this.FocusControlsOnClick)
                {
                    if (selectedItem.Sources.Count == 0)
                    {
                        this._currentValidationSummaryItemSource = null;
                    }
                    else if (FindMatchingErrorSource(selectedItem.Sources, this._currentValidationSummaryItemSource) < 0)
                    {
                        this._currentValidationSummaryItemSource = selectedItem.Sources[0];
                    }
                    var e = new FocusingInvalidControlEventArgs(selectedItem, this._currentValidationSummaryItemSource);
                    this.OnFocusingInvalidControl(e);
                    var peer = FrameworkElementAutomationPeer.FromElement(this) as ValidationSummaryAutomationPeer;
                    if ((peer != null) && AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
                    {
                        peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
                    }
                    if ((!e.Handled && (e.Target != null)) && (e.Target.Control != null))
                    {
                        e.Target.Control.Focus();
                    }
                    if (selectedItem.Sources.Count > 0)
                    {
                        var num = FindMatchingErrorSource(selectedItem.Sources, e.Target);
                        num = (num < 0) ? 0 : (++num % selectedItem.Sources.Count);
                        this._currentValidationSummaryItemSource = selectedItem.Sources[num];
                    }
                }
            }
        }

        internal void ExecuteClickInternal()
        {
            this.ExecuteClick(this.ErrorsListBoxInternal);
        }

        private static int FindMatchingErrorSource(IList<ValidationSummaryItemSource> sources, ValidationSummaryItemSource sourceToFind)
        {
            if (sources != null)
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (sources[i].Equals(sourceToFind))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal string GetHeaderString()
        {
            string validationSummaryHeaderError;
            if (this._displayedErrors.Count == 1)
            {
                validationSummaryHeaderError = Properties.Resources.ValidationSummaryHeaderError;
            }
            else
            {
                var args = new object[] { this._displayedErrors.Count };
                validationSummaryHeaderError = string.Format(CultureInfo.CurrentCulture, Properties.Resources.ValidationSummaryHeaderErrors, args);
            }
            return validationSummaryHeaderError;
        }

        public static bool GetShowErrorsInSummary(DependencyObject inputControl)
        {
            if (inputControl == null)
            {
                throw new ArgumentNullException("inputControl");
            }
            return (bool) inputControl.GetValue(ShowErrorsInSummaryProperty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var handler = new MouseButtonEventHandler(this.ErrorsListBox_MouseLeftButtonUp);
            var handler2 = new KeyEventHandler(this.ErrorsListBox_KeyDown);
            var handler3 = new SelectionChangedEventHandler(this.ErrorsListBox_SelectionChanged);
            if (this._errorsListBox != null)
            {
                this._errorsListBox.MouseLeftButtonUp -= handler;
                this._errorsListBox.KeyDown -= handler2;
                this._errorsListBox.SelectionChanged -= handler3;
            }
            this._errorsListBox = base.GetTemplateChild("SummaryListBox") as ListBox;
            if (this._errorsListBox != null)
            {
                this._errorsListBox.MouseLeftButtonUp += handler;
                this._errorsListBox.KeyDown += handler2;
                this._errorsListBox.ItemsSource = this.DisplayedErrors;
                this._errorsListBox.SelectionChanged += handler3;
            }
            this._headerContentControl = base.GetTemplateChild("HeaderContentControl") as ContentControl;
            this.UpdateDisplayedErrors();
            this.UpdateCommonState(false);
            this.UpdateValidationState(false);
        }

        protected override AutomationPeer OnCreateAutomationPeer() => 
            new ValidationSummaryAutomationPeer(this);

        private static void OnFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ValidationSummary summary)
            {
                summary.UpdateDisplayedErrors();
            }
        }

        protected virtual void OnFocusingInvalidControl(FocusingInvalidControlEventArgs e)
        {
            var focusingInvalidControl = this.FocusingInvalidControl;
            focusingInvalidControl?.Invoke(this, e);
        }

        private static void OnHasDisplayedErrorsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ValidationSummary summary) && !summary.AreHandlersSuspended())
            {
                summary.SetValueNoCallback(HasDisplayedErrorsProperty, e.OldValue);
                var args = new object[] { "HasDisplayedErrors" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnHasErrorsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var summary = d as ValidationSummary;
            if ((summary != null) && !summary.AreHandlersSuspended())
            {
                summary.SetValueNoCallback(HasErrorsProperty, e.OldValue);
                var args = new object[] { "HasErrors" };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.UnderlyingPropertyIsReadOnly, args));
            }
        }

        private static void OnHasHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var summary = d as ValidationSummary;
            if (summary != null)
            {
                summary.UpdateHeaderText();
            }
        }

        private static void OnShowErrorsInSummaryPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var parent = (Application.Current != null) ? (Window.GetWindow(o) as FrameworkElement) : null;
            if (parent != null)
            {
                UpdateDisplayedErrorsOnAllValidationSummaries(parent);
            }
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = e.OldValue as FrameworkElement;
            var summary = d as ValidationSummary;
            var handler = new EventHandler<ValidationErrorEventArgs>(summary.Target_BindingValidationError);
            if (summary._registeredParent != null)
            {
                //summary._registeredParent.BindingValidationError -= handler;
                summary._registeredParent.RemoveHandler(Validation.ErrorEvent, handler);
                summary._registeredParent = null;
            }
            if (oldValue != null)
            {
                //oldValue.BindingValidationError -= handler;
                oldValue.RemoveHandler(Validation.ErrorEvent, handler);
            }
            var newValue = e.NewValue as FrameworkElement;
            if (newValue != null)
            {
                //newValue.BindingValidationError += handler;
                newValue.AddHandler(Validation.ErrorEvent, handler);
            }
            summary._errors.ClearErrors(ValidationSummaryItemType.PropertyError);
            summary.UpdateDisplayedErrors();
        }

        private static bool ReferencesAreValid(object x, object y, out int val)
        {
            if (x == null)
            {
                val = (y == null) ? 0 : -1;
                return false;
            }
            if (y == null)
            {
                val = 1;
                return false;
            }
            val = 0;
            return true;
        }

        public static void SetShowErrorsInSummary(DependencyObject inputControl, bool value)
        {
            if (inputControl == null)
            {
                throw new ArgumentNullException("inputControl");
            }
            inputControl.SetValue(ShowErrorsInSummaryProperty, value);
        }

        internal static int SortByVisualTreeOrdering(DependencyObject controlX, DependencyObject controlY)
        {
            if (((controlX != null) && (controlY != null)) && !ReferenceEquals(controlX, controlY))
            {
                var list = new List<DependencyObject>();
                var item = controlX;
                list.Add(item);
                while ((item = VisualTreeHelper.GetParent(item)) != null)
                {
                    list.Add(item);
                }
                item = controlY;
                var objB = item;
                while (true)
                {
                    item = VisualTreeHelper.GetParent(item);
                    if (item == null)
                    {
                        return 0;
                    }
                    var index = list.IndexOf(item);
                    if (index == 0)
                    {
                        return -1;
                    }
                    if (index > 0)
                    {
                        var obj4 = list[index - 1];
                        if (obj4 == null)
                        {
                            break;
                        }
                        if (objB == null)
                        {
                            break;
                        }
                        var childrenCount = VisualTreeHelper.GetChildrenCount(item);
                        for (var i = 0; i < childrenCount; i++)
                        {
                            var child = VisualTreeHelper.GetChild(item, i);
                            if (ReferenceEquals(child, objB))
                            {
                                return 1;
                            }
                            if (ReferenceEquals(child, obj4))
                            {
                                return -1;
                            }
                        }
                    }
                    objB = item;
                }
            }
            return 0;
        }

        private void Target_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            if (((e != null) && ((e.Error != null) && (e.Error.ErrorContent != null))) && (originalSource != null))
            {
                string name;
                var message = e.Error.ErrorContent.ToString();
                if (!string.IsNullOrEmpty(originalSource.Name))
                {
                    name = originalSource.Name;
                }
                else
                {
                    name = originalSource.GetHashCode().ToString(CultureInfo.InvariantCulture);
                }
                var key = name + message;
                if (this._validationSummaryItemDictionary.ContainsKey(key))
                {
                    var item = this._validationSummaryItemDictionary[key];
                    this._errors.Remove(item);
                    this._validationSummaryItemDictionary.Remove(key);
                }
                if ((e.Action == ValidationErrorEventAction.Added) && GetShowErrorsInSummary(originalSource))
                {
                    object obj2;
                    BindingExpression expression;
                    string messageHeader = null;
                    var metadata = ValidationHelper.ParseMetadata(originalSource, false, out obj2, out expression);
                    if (metadata != null)
                    {
                        messageHeader = metadata.Caption;
                    }
                    var item = new ValidationSummaryItem(message, messageHeader, ValidationSummaryItemType.PropertyError, new ValidationSummaryItemSource(messageHeader, originalSource as Control), null);
                    this._errors.Add(item);
                    this._validationSummaryItemDictionary[key] = item;
                }
            }
        }

        private static bool TryCompareReferences(object x, object y, out int returnVal)
        {
            if (((x == null) && (y == null)) || ((x != null) && x.Equals(y)))
            {
                returnVal = 0;
                return false;
            }
            if (ReferencesAreValid(x, y, out returnVal))
            {
                var comparable = x as IComparable;
                var comparable2 = y as IComparable;
                if ((comparable == null) || (comparable2 == null))
                {
                    returnVal = 0;
                    return false;
                }
                returnVal = comparable.CompareTo(comparable2);
            }
            return true;
        }

        private void UpdateCommonState(bool useTransitions)
        {
            if (base.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }
        }

        private void UpdateDisplayedErrors()
        {
            var list = new List<ValidationSummaryItem>();
            foreach (var item in this.Errors)
            {
                if (item == null)
                {
                    continue;
                }
                if (((item.ItemType == ValidationSummaryItemType.ObjectError) && ((this.Filter & ValidationSummaryFilters.ObjectErrors) != ValidationSummaryFilters.None)) || ((item.ItemType == ValidationSummaryItemType.PropertyError) && ((this.Filter & ValidationSummaryFilters.PropertyErrors) != ValidationSummaryFilters.None)))
                {
                    list.Add(item);
                }
            }
            list.Sort(new Comparison<ValidationSummaryItem>(ValidationSummary.CompareValidationSummaryItems));
            this._displayedErrors.Clear();
            foreach (var item2 in list)
            {
                this._displayedErrors.Add(item2);
            }
            this.UpdateValidationState(true);
            this.UpdateHeaderText();
        }

        private static void UpdateDisplayedErrorsOnAllValidationSummaries(DependencyObject parent)
        {
            if (parent != null)
            {
                var summary = parent as ValidationSummary;
                if (summary != null)
                {
                    summary.UpdateDisplayedErrors();
                }
                else
                {
                    for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                    {
                        var child = VisualTreeHelper.GetChild(parent, i);
                        UpdateDisplayedErrorsOnAllValidationSummaries(child);
                    }
                }
            }
        }

        private void UpdateHeaderText()
        {
            if (this._headerContentControl != null)
            {
                if (this.Header != null)
                {
                    this._headerContentControl.Content = this.Header;
                }
                else
                {
                    this._headerContentControl.Content = this.GetHeaderString();
                }
            }
        }

        private void UpdateValidationState(bool useTransitions)
        {
            this.HasDisplayedErrors = this._displayedErrors.Count > 0;
            VisualStateManager.GoToState(this, this.HasDisplayedErrors ? "HasErrors" : "Empty", useTransitions);
        }

        private void ValidationSummary_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UpdateCommonState(true);
        }

        private void ValidationSummary_Loaded(object sender, RoutedEventArgs e)
        {
            if ((this.Target == null) && (this._registeredParent == null))
            {
                this._registeredParent = VisualTreeHelper.GetParent(this) as FrameworkElement;
                var handler = new EventHandler<ValidationErrorEventArgs>(this.Target_BindingValidationError);
                if (this._registeredParent != null)
                {
                    //this._registeredParent.BindingValidationError += new EventHandler<ValidationErrorEventArgs>(this.Target_BindingValidationError);                    
                    this._registeredParent.AddHandler(Validation.ErrorEvent, handler);
                }
            }
            base.Loaded -= new RoutedEventHandler(this.ValidationSummary_Loaded);
            this._initialized = true;
        }

        private void ValidationSummary_Unloaded(object sender, RoutedEventArgs e)
        {
            var handler = new EventHandler<ValidationErrorEventArgs>(this.Target_BindingValidationError);
            if (this._registeredParent != null)
            {               
                //this._registeredParent.BindingValidationError -= new EventHandler<ValidationErrorEventArgs>(this.Target_BindingValidationError);
                this._registeredParent.RemoveHandler(Validation.ErrorEvent, handler);
            }
            base.Unloaded -= new RoutedEventHandler(this.ValidationSummary_Unloaded);
            this._initialized = false;
        }

        private void ValidationSummaryItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemType")
            {
                this.UpdateDisplayedErrors();
            }
        }

        public Style ErrorStyle
        {
            get => 
                (base.GetValue(ErrorStyleProperty) as Style);
            set => 
                base.SetValue(ErrorStyleProperty, value);
        }

        public ValidationSummaryFilters Filter
        {
            get => 
                ((ValidationSummaryFilters) base.GetValue(FilterProperty));
            set => 
                base.SetValue(FilterProperty, value);
        }

        public bool FocusControlsOnClick
        {
            get => 
                ((bool) base.GetValue(FocusControlsOnClickProperty));
            set => 
                base.SetValue(FocusControlsOnClickProperty, value);
        }

        public bool HasErrors
        {
            get => 
                ((bool) base.GetValue(HasErrorsProperty));
            internal set => 
                this.SetValueNoCallback(HasErrorsProperty, value);
        }

        public bool HasDisplayedErrors
        {
            get => 
                ((bool) base.GetValue(HasDisplayedErrorsProperty));
            internal set => 
                this.SetValueNoCallback(HasDisplayedErrorsProperty, value);
        }

        public object Header
        {
            get => 
                base.GetValue(HeaderProperty);
            set => 
                base.SetValue(HeaderProperty, value);
        }

        public DataTemplate HeaderTemplate
        {
            get => 
                (base.GetValue(HeaderTemplateProperty) as DataTemplate);
            set => 
                base.SetValue(HeaderTemplateProperty, value);
        }

        public Style SummaryListBoxStyle
        {
            get => 
                (base.GetValue(SummaryListBoxStyleProperty) as Style);
            set => 
                base.SetValue(SummaryListBoxStyleProperty, value);
        }

        public UIElement Target
        {
            get => 
                (base.GetValue(TargetProperty) as UIElement);
            set => 
                base.SetValue(TargetProperty, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<ValidationSummaryItem> Errors =>
            this._errors;

        public System.Collections.ObjectModel.ReadOnlyObservableCollection<ValidationSummaryItem> DisplayedErrors =>
            new System.Collections.ObjectModel.ReadOnlyObservableCollection<ValidationSummaryItem>(this._displayedErrors);

        internal new bool Initialized =>
            this._initialized;

        internal ListBox ErrorsListBoxInternal =>
            this._errorsListBox;

        internal ContentControl HeaderContentControlInternal =>
            this._headerContentControl;
    }
}

