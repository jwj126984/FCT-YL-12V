using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FCT.Ctrl.PlayCtrl
{
    public class DecimalTextBox : TextBox
    {
        public int MaxDecimalPlaces { get; set; } = 16; // 默认最多允许16位小数

        public DecimalTextBox()
        {
            this.PreviewTextInput += DecimalTextBox_PreviewTextInput;
            this.PreviewKeyDown += DecimalTextBox_PreviewKeyDown;
            this.LostFocus += DecimalTextBox_LostFocus;
            InputMethod.SetIsInputMethodEnabled(this, false);
        }

        private void DecimalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string inputText = this.Text + e.Text;
            ;
            if (double.TryParse(inputText, out double d))
            {
                //this.Text = d.ToString();
            }
            else
            {
                e.Handled = true;
            }
            //if (inputText.Contains("."))
            //{
            //    int decimalIndex = inputText.IndexOf(".");
            //    if (inputText.Substring(decimalIndex).Length > MaxDecimalPlaces + 1) // 包含小数点
            //    {
            //        e.Handled = true;
            //    }
            //}

            //string regexPattern = @"^\d*\.?\d*$"; // 正小数的正则表达式
            //if (!Regex.IsMatch(e.Text, regexPattern))
            //{
            //    e.Handled = true;
            //}
        }

        private void DecimalTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Back || e.Key == Key.Decimal)
            //    return;

            //if (e.Key < Key.D0 || e.Key > Key.D9)
            //{
            //    e.Handled = true;
            //}
        }

        private void DecimalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                this.Text = "0";
            }
        }
    }

}
