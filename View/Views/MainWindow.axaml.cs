using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia;

using TranslateLibrary;
using System;
using System.Diagnostics;

namespace Kyrs.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        
        }
         private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.FindControl<TextBox>("TBSource").Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.FindControl<TextBox>("TBTarget").Clear();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Default_Click(object sender, RoutedEventArgs e)
        {
            Color Cl1 = Color.FromArgb(0xFF ,0x9D, 0xC7, 0x97);
            Color Cl2 = Color.FromArgb(0xFF,0xE0,0x98,0x98);
            try{
            this.FindControl<Grid>("GR").Background = new LinearGradientBrush() { EndPoint = new RelativePoint(0.5,1,RelativeUnit.Relative), StartPoint = new RelativePoint(0.5,0,RelativeUnit.Relative),GradientStops = new GradientStops()
            { new GradientStop(Cl1,0),
              new GradientStop(Cl2,1)
            }};
            }
            catch (Exception ex){ Debug.WriteLine(ex.Message); }
        }
    }
}