using Avalonia.Controls;
using Avalonia.Interactivity;

using TranslateLibrary.API;
using System;

namespace Kyrs.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();      
        }
        private void SourceClear_Click(object sender, RoutedEventArgs e) =>
            TBSource.Text = String.Empty;

        private void TargetClear_Click(object sender, RoutedEventArgs e) =>
            TBTarget.Text = String.Empty;

        private void Translate_Click(object sender, RoutedEventArgs e)
        {   
            if(TBSource.Text.Trim() != String.Empty)
            {
                if(API.Translate(TBSource.Text, out string Resault, out string Error))
                    TBTarget.Text = Resault;
                else
                    TBTarget.Text = "Во время трансляции произошла ошибка.";
            }
        }
    }
}