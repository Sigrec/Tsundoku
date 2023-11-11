using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Tsundoku.Helpers
{
    public class ComboBoxListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string>)
            {
                List<ComboBoxItem> itemList = new List<ComboBoxItem>();
                foreach (string themeName in value as List<string>)
                {
                    itemList.Add(new ComboBoxItem { Content = themeName });
                }
                return itemList;
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
