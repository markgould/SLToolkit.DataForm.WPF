using System;
using System.ComponentModel;
using System.Threading;

namespace SLToolkit.DataForm.WPF.Controls
{
    internal class ValidationMetadata : INotifyPropertyChanged
    {
        private string _caption;
        private string _description;
        private bool _isRequired;
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

        private void NotifyPropertyChanged(string propertyName)
        {
            var propertyChanged = this._propertyChanged;
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsRequired
        {
            get => 
                this._isRequired;
            set
            {
                if (this._isRequired != value)
                {
                    this._isRequired = value;
                    this.NotifyPropertyChanged("IsRequired");
                }
            }
        }

        public string Description
        {
            get => 
                this._description;
            set
            {
                if (this._description != value)
                {
                    this._description = value;
                    this.NotifyPropertyChanged("Description");
                }
            }
        }

        public string Caption
        {
            get => 
                this._caption;
            set
            {
                if (this._caption != value)
                {
                    this._caption = value;
                    this.NotifyPropertyChanged("Caption");
                }
            }
        }
    }
}

