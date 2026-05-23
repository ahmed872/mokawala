// ============================================================================
// DELIVERABLE 6: XAML Converters for Data Binding
// Fleet Management System - C# WPF Implementation
// ============================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FleetManagementSystem.WPF.Converters
{
    // ========================================================================
    // BOOL TO VISIBILITY CONVERTER
    // ========================================================================

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    // ========================================================================
    // INVERSE BOOL TO VISIBILITY CONVERTER
    // ========================================================================

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return true;
        }
    }

    // ========================================================================
    // STRING TO VISIBILITY CONVERTER
    // ========================================================================

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // INVERSE BOOLEAN CONVERTER
    // ========================================================================

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }
    }

    // ========================================================================
    // ENUM TO STRING CONVERTER
    // ========================================================================

    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var enumType = value.GetType();
            if (!enumType.IsEnum)
                return value.ToString() ?? string.Empty;

            var enumValue = (Enum)value;
            var fieldInfo = enumType.GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    var descriptionAttribute = (System.ComponentModel.DescriptionAttribute)attributes[0];
                    return descriptionAttribute.Description;
                }
            }

            return enumValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // DATE TO STRING CONVERTER
    // ========================================================================

    public class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                string format = parameter as string ?? "dd/MM/yyyy";
                return dateTime.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && DateTime.TryParse(str, culture, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    // ========================================================================
    // BYTE ARRAY TO IMAGE CONVERTER
    // ========================================================================

    public class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] imageData && imageData.Length > 0)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new System.IO.MemoryStream(imageData);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // NULLABLE DATETIME TO STRING CONVERTER
    // ========================================================================

    public class NullableDateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime nullableDateTime)
            {
                string format = parameter as string ?? "dd/MM/yyyy";
                return nullableDateTime.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && DateTime.TryParse(str, culture, System.Globalization.DateTimeStyles.None, out var result))
            {
                return (DateTime?)result;
            }
            return DependencyProperty.UnsetValue;
        }
    }

    // ========================================================================
    // DECIMAL TO STRING CONVERTER
    // ========================================================================

    public class DecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                string format = parameter as string ?? "N2";
                return decimalValue.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && decimal.TryParse(str, System.Globalization.NumberStyles.Any, culture, out var result))
            {
                return result;
            }
            return 0m;
        }
    }

    // ========================================================================
    // INT TO STRING CONVERTER
    // ========================================================================

    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString(culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && int.TryParse(str, System.Globalization.NumberStyles.Any, culture, out var result))
            {
                return result;
            }
            return 0;
        }
    }

    public class VisibleRowNumberConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not ItemCollection items || values[1] is null)
            {
                return string.Empty;
            }

            var index = items.IndexOf(values[1]);
            return index >= 0 ? (index + 1).ToString(culture) : string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // MULTI-VALUE CONVERTER FOR FULL NAME
    // ========================================================================

    public class FullNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is string firstName && values[1] is string lastName)
            {
                return $"{firstName} {lastName}".Trim();
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // MULTI-VALUE CONVERTER FOR DATE RANGE
    // ========================================================================

    public class DateRangeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is DateTime startDate && values[1] is DateTime endDate)
            {
                string format = parameter as string ?? "dd/MM/yyyy";
                return $"{startDate.ToString(format, culture)} - {endDate.ToString(format, culture)}";
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // OBJECT TO BOOLEAN CONVERTER
    // ========================================================================

    public class ObjectToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // ZERO TO VISIBILITY CONVERTER
    // ========================================================================

    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            if (value is decimal decimalValue)
            {
                return decimalValue == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // STATUS COLOR CONVERTER
    // ========================================================================

    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "active" or "نشط" => "#388E3C",
                    "inactive" or "غير نشط" => "#757575",
                    "pending" or "قيد الانتظار" => "#F57C00",
                    "completed" or "مكتملة" => "#388E3C",
                    "cancelled" or "ملغى" => "#C62828",
                    "expired" or "منتهي" => "#C62828",
                    _ => "#757575"
                };
            }
            return "#757575";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

// ============================================================================
// END OF DELIVERABLE 6
// ============================================================================
