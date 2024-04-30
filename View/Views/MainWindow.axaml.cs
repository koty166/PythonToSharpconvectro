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
            TBTarget.Text = "язык = оператор\"\\n\"...оператор\n"+
            "оператор = переменная \"=\" массив!прч   ! импорт ! условие ! конец\n\n"+

            "условие = \"if(\" логик \"):\"\n\n\n"  +

            "логик = прч [\"==\"!\">\"!\"<\"] прч \n" +

            "вызов = переменная \"(\" прч , ...прч \")\"\n" +
            "массив =  \"[\" объект ,...объект \"]\"\n\n" +

            "конец = \"end\"\n" +
            "импорт = \"import\" переменная \"as\" переменная\n\n\n" +


            "прч = блок1[\"+\"!\"-\"]...блок1\n" +
            "блок1 = блок2[\"*\"!\"/\"]...блок2\n" +
            "блок2 = блок3[\"^\"]...блок3\n" +
            "блок5 = пер!цел!\"(\"прч\")\"\n" +
            "переменная = [\"б\"!\"ц\"!\"_\"!\"-\"]...[\"б\"!\"ц\"!\"_\"!\"-\"]\n" +
            "объект = цел ! текст\n" +
            "текст = \"сим...сим\"\n" +
            "цел = ц..ц\n"+
            "сим = б!ц\n"+
            "б = A-ZА-Яa-zа-я\n"+
            "ц = 0..9\n";
        }
        private void SourceClear_Click(object sender, RoutedEventArgs e) =>
            TBSource.Text = String.Empty;

        private void TargetClear_Click(object sender, RoutedEventArgs e) =>
            TBTarget.Text = String.Empty;

        private void Translate_Click(object sender, RoutedEventArgs e)
        {   
            if(TBSource.Text.Trim() != String.Empty)
            {

                try
                {
                    if(API.Translate(TBSource.Text, out string Resault, out string Error))
                        TBResult.Text = Resault;
                    else
                        TBResult.Text = "Во время вычисления произошла ошибка.";
                }
                catch(Exception ex)
                {
                    TBResult.Text = ex.Message;
                }
            }
        }
    }
}