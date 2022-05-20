using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;

using TranslateLibrary.API;
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
         private void SourceClear_Click(object sender, RoutedEventArgs e)
        {
            TBSource.Clear();
            TBSource.Text = String.Empty;
        }

        private void TargetClear_Click(object sender, RoutedEventArgs e)
        {
            TBTarget.Clear();
            TBTarget.Text = String.Empty;
        }

        private void Translate_Click(object sender, RoutedEventArgs e)
        {   
            /*//c1 = ff3B2C54
            //c2 = 2B223A
            try{
            Color c1 = Color.Parse("#ff"+this.FindControl<TextBox>("Colorsvalue2").Text);
            Color c2 = Color.Parse("#ff"+this.FindControl<TextBox>("Colorsvalue1").Text);
            SetBackGround(c1,c2);
            }
            catch{}

            // var theme = "/Styles/TextBox.xaml";
            // var self = new Uri("resm:Styles?assembly=Citrus.Avalonia.Sandbox");
            // var include = new StyleInclude(self) {
            //     Source = new Uri(theme)
            // };

            // При изменении коллекции window.Styles все кисти,
            // используемые внутри окна, на которое ссылается 
            // переменная window, будут обновлены.

            //window.Styles.Add(include);
            */
            if(TBSource.Text.Trim() != String.Empty)
            {
                API.Translate(TBSource.Text,TranslateLibrary.CoreLib.PostGenerationOptimizingT.Simple, out string Resault, out string Error);
                TBTarget.Text = Resault;
            }
        }

        private void Blue_Click(object sender, PointerPressedEventArgs e)
        {

        }   

        private void White_Click(object sender, PointerPressedEventArgs e)
        {
        }

        private void Dark_Click(object sender, PointerPressedEventArgs e)
        {
        }

        private void Default_Click(object sender, PointerPressedEventArgs e)
        {
            Color Cl1 = Color.FromArgb(0xFF, 0x9D, 0xC7, 0x97);
            Color Cl2 = Color.FromArgb(0xFF, 0xE0, 0x98, 0x98);
            SetBackGround(Cl1,Cl2);
        }
        void SetBackGround(Color c1, Color c2)
        {
            try{
            GR.Background = new LinearGradientBrush() { EndPoint = new RelativePoint(0.5,1,RelativeUnit.Relative), StartPoint = new RelativePoint(0.5,0,RelativeUnit.Relative),GradientStops = new GradientStops()
            { new GradientStop(c1,0),
              new GradientStop(c2,1)
            }};
            }
            catch (Exception ex){ Console.WriteLine(ex.Message); }
        }
    }
}