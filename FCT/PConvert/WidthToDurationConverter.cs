using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FCT.PConvert
{
    public class WidthToDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 获取容器的宽度
            if (value is double width)
            {
                // 计算动画持续时间：宽度越大，动画时间越长
                // 100 是调节因子，可以根据需求调整
                double durationInSeconds = width / 100;
                return TimeSpan.FromSeconds(durationInSeconds);
            }
            return TimeSpan.FromSeconds(1);  // 默认值
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
