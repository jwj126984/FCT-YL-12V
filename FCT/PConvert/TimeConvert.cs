using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FCT.PConvert
{
    public class TimeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TestResult model)
            {
                if (model.EndTime.HasValue == false || model.StartTime.HasValue == false)
                {
                    return "";
                }
                var temp = (model.EndTime - model.StartTime).Value.TotalSeconds;
                if (temp < 0.010)
                {
                    return "0.010";
                }
                return temp.ToString("0.000");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
