using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoshanVirusKiller {
    /// <summary>
    /// CheckButton.xaml 的交互逻辑
    /// </summary>
    public partial class CheckButton : UserControl {
        public CheckButton() {
            InitializeComponent();
        }
    }
    /// <summary>
    /// 数值转换器
    /// </summary>
    public class MathConvert : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return value;
            try {
                double v = (double)value;

                String parm = (String)parameter;
                String op = null;
                double num = double.NaN;
                if (parm != null) {
                    String[] parms = parm.Split(",".ToCharArray(), 2);
                    op = parms[0];
                    if (parms.Length > 1)
                        num = double.Parse(parms[1]);
                }
                if (double.IsNaN(num)) {
                    return value;
                }
                switch (op) {
                    case "+": return v + num;
                    case "-": return v - num;
                    case "*": return v * num;
                    case "/": return v / num;
                    case "^": return Math.Pow(v, num);
                    case "_": return Math.Log(v, num);
                }
            } catch (Exception ex) {
                Console.WriteLine("转换数值出错：原始值：" + value + ",转换参数：" + parameter + ".消息:" + ex.Message);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
