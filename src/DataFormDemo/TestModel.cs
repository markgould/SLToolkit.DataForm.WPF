using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFormDemo
{
    public class TestModel : IDataErrorInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrEmpty(Name))
                            result = "Enter name";
                        break;
                    case nameof(Address):
                        if (string.IsNullOrEmpty(Name))
                            result = "Enter address";
                        break;
                }

                return result;
            }
        }

        public string Error { get; }
    }
}
