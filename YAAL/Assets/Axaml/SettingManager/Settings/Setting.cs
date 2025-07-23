using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public abstract class Setting : UserControl
    {
        public SettingManager manager;
        protected Border background;

        protected object? settingName;
        public string SettingNameText
        {
            get
            {
                if (settingName is TextBox tb)
                    return tb.Text;
                if (settingName is TextBlock tbl)
                    return tbl.Text;
                return string.Empty; // Or throw, or log
            }
            set
            {
                if (settingName is TextBox tb)
                    tb.Text = value;
                else if (settingName is TextBlock tbl)
                    tbl.Text = value;
                // else ignore or throw/log
            }
        }

        protected object? settingValue;
        public string SettingValueText
        {
            get
            {
                if (settingValue is TextBox tb)
                    return tb.Text;
                if (settingValue is TextBlock tbl)
                    return tbl.Text;
                return string.Empty; // Or throw, or log
            }
            set
            {
                if (settingValue is TextBox tb)
                    tb.Text = value;
                else if (settingValue is TextBlock tbl)
                    tbl.Text = value;
                // else ignore or throw/log
            }
        }

        public void SetBackground()
        {
            var theme = Application.Current.ActualThemeVariant;
            if (theme == ThemeVariant.Dark)
            {
                background.Background = new SolidColorBrush(Color.Parse("#454545"));
            }
            else
            {
                background.Background = new SolidColorBrush(Color.Parse("#AAA"));

            }
        }

        public void RemoveSetting(object? sender, RoutedEventArgs e)
        {
            manager.RemoveSetting(this);
            var parent = this.Parent as StackPanel;
            parent?.Children.Remove(this);
        }

        public virtual void SetSetting(string name, string value)
        {
            SettingNameText = name;
            SettingValueText = value;
        }
    }
}
