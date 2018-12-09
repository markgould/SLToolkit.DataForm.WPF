using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace SLToolkit.DataForm.WPF.Controls
{
    internal class ValidationHelper
    {
        internal static readonly DependencyProperty ValidationMetadataProperty = DependencyProperty.RegisterAttached("ValidationMetadata", typeof(ValidationMetadata), typeof(ValidationHelper), null);

        private static Type GetCustomOrCLRType(object instance)
        {
            System.Reflection.ICustomTypeProvider provider = instance as System.Reflection.ICustomTypeProvider;
            if (provider == null)
            {
                return instance.GetType();
            }
            Type customType = provider.GetCustomType();
            return (customType ?? instance.GetType());
        }

        private static PropertyInfo GetProperty(Type entityType, string propertyPath)
        {
            Type propertyType = entityType;
            string[] strArray = propertyPath.Split(new char[] { '.' });
            if (strArray != null)
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    PropertyInfo property = propertyType.GetProperty(strArray[i]);
                    if ((property == null) || !property.CanRead)
                    {
                        return null;
                    }
                    if (i == (strArray.Length - 1))
                    {
                        return property;
                    }
                    propertyType = property.PropertyType;
                }
            }
            return null;
        }

        internal static ValidationMetadata GetValidationMetadata(DependencyObject inputControl)
        {
            if (inputControl == null)
            {
                throw new ArgumentNullException("inputControl");
            }
            return (inputControl.GetValue(ValidationMetadataProperty) as ValidationMetadata);
        }

        internal static ValidationMetadata ParseMetadata(string bindingPath, object entity)
        {
            if ((entity != null) && !string.IsNullOrEmpty(bindingPath))
            {
                PropertyInfo property = GetProperty(GetCustomOrCLRType(entity), bindingPath);
                if (property != null)
                {
                    ValidationMetadata metadata = new ValidationMetadata();
                    foreach (object obj2 in property.GetCustomAttributes(false))
                    {
                        RequiredAttribute attribute = obj2 as RequiredAttribute;
                        if (attribute != null)
                        {
                            metadata.IsRequired = true;
                        }
                        else
                        {
                            DisplayAttribute attribute2 = obj2 as DisplayAttribute;
                            if (attribute2 != null)
                            {
                                metadata.Description = attribute2.GetDescription();
                                metadata.Caption = attribute2.GetName();
                            }
                        }
                    }
                    if (metadata.Caption == null)
                    {
                        metadata.Caption = property.Name;
                    }
                    return metadata;
                }
            }
            return null;
        }

        internal static ValidationMetadata ParseMetadata(FrameworkElement element, bool forceUpdate, out object entity, out BindingExpression bindingExpression)
        {
            entity = null;
            bindingExpression = null;
            if (element == null)
            {
                return null;
            }
            if (!forceUpdate)
            {
                ValidationMetadata metadata = element.GetValue(ValidationMetadataProperty) as ValidationMetadata;
                if (metadata != null)
                {
                    return metadata;
                }
            }
            BindingExpression expression = null;
            foreach (FieldInfo info in element.GetType().GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static))
            {
                if (ReferenceEquals(info.FieldType, typeof(DependencyProperty)))
                {
                    expression = element.GetBindingExpression((DependencyProperty) info.GetValue(null));
                    if (((expression != null) && (expression.ParentBinding != null)) && (expression.ParentBinding.Path != null))
                    {
                        entity = (expression.DataItem != null) ? expression.DataItem : element.DataContext;
                        if (entity != null)
                        {
                            if (expression.ParentBinding.Mode == BindingMode.TwoWay)
                            {
                                bindingExpression = expression;
                                break;
                            }
                            if ((bindingExpression == null) || (string.Compare(expression.ParentBinding.Path.Path, bindingExpression.ParentBinding.Path.Path, StringComparison.Ordinal) < 0))
                            {
                                bindingExpression = expression;
                            }
                        }
                    }
                }
            }
            if (bindingExpression == null)
            {
                return null;
            }
            ValidationMetadata metadata2 = ParseMetadata(bindingExpression.ParentBinding.Path.Path, entity);
            element.SetValue(ValidationMetadataProperty, metadata2);
            return metadata2;
        }

        internal static void SetValidationMetadata(DependencyObject inputControl, ValidationMetadata value)
        {
            if (inputControl == null)
            {
                throw new ArgumentNullException("inputControl");
            }
            inputControl.SetValue(ValidationMetadataProperty, value);
        }
    }
}

