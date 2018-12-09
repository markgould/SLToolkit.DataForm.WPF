using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using SLToolkit.DataForm.WPF.Properties;

namespace SLToolkit.DataForm.WPF.Controls
{
    public class ValidationSummaryItem : INotifyPropertyChanged
    {
        internal const string PROPERTYNAME_ITEMTYPE = "ItemType";
        private object _context;
        private ValidationSummaryItemType _itemType;
        private string _message;
        private string _messageHeader;
        private PropertyChangedEventHandler _propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEventHandler propertyChanged = this._propertyChanged;
                while (true)
                {
                    PropertyChangedEventHandler a = propertyChanged;
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler) Delegate.Combine(a, value);
                    propertyChanged = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChanged, handler3, a);
                    if (ReferenceEquals(propertyChanged, a))
                    {
                        return;
                    }
                }
            }
            remove
            {
                PropertyChangedEventHandler propertyChanged = this._propertyChanged;
                while (true)
                {
                    PropertyChangedEventHandler source = propertyChanged;
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler) Delegate.Remove(source, value);
                    propertyChanged = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChanged, handler3, source);
                    if (ReferenceEquals(propertyChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public ValidationSummaryItem() : this(null)
        {
        }

        public ValidationSummaryItem(string message) : this(message, null, ValidationSummaryItemType.ObjectError, null, null)
        {
        }

        public ValidationSummaryItem(string message, string messageHeader, ValidationSummaryItemType itemType, ValidationSummaryItemSource source, object context)
        {
            this.MessageHeader = messageHeader;
            this.Message = message;
            this.ItemType = itemType;
            this.Context = context;
            this.Sources = new System.Collections.ObjectModel.ObservableCollection<ValidationSummaryItemSource>();
            if (source != null)
            {
                this.Sources.Add(source);
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            var propertyChanged = this._propertyChanged;
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            object[] args = new object[] { this.MessageHeader, this.Message };
            return string.Format(CultureInfo.InvariantCulture, Resources.ValidationSummaryItem, args);
        }

        public object Context
        {
            get => 
                this._context;
            set
            {
                if (this._context != value)
                {
                    this._context = value;
                    this.NotifyPropertyChanged("Context");
                }
            }
        }

        public ValidationSummaryItemType ItemType
        {
            get => 
                this._itemType;
            set
            {
                if (this._itemType != value)
                {
                    this._itemType = value;
                    this.NotifyPropertyChanged("ItemType");
                }
            }
        }

        public string Message
        {
            get => 
                this._message;
            set
            {
                if (this._message != value)
                {
                    this._message = value;
                    this.NotifyPropertyChanged("Message");
                }
            }
        }

        public string MessageHeader
        {
            get => 
                this._messageHeader;
            set
            {
                if (this._messageHeader != value)
                {
                    this._messageHeader = value;
                    this.NotifyPropertyChanged("MessageHeader");
                }
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<ValidationSummaryItemSource> Sources { get; private set; }
    }
}

