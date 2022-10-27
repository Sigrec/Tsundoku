using Avalonia.Data;
using Avalonia.Data.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Src
{
    public class VolumeCountConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            Debug.WriteLine("Decrement Button Pressed = " + (string)values[0]);
            return values[0];
        }
    }
}
