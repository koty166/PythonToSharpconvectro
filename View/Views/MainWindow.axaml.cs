using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;


using Avalonia.Automation.Peers;
using Avalonia.Collections;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media.TextFormatting;
using Avalonia.Metadata;
using Avalonia.Utilities;

using TranslateLibrary.API;
using System;

namespace Kyrs.Views;

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
            "блок2 = блок3[\"**\"]...блок3\n" +
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
        private void Refresh(object sender, GotFocusEventArgs e) 
        {
            TBLSource.Inlines.Clear();
            TBResult.Text= "";
        }

        private void Translate_Click(object sender, RoutedEventArgs e)
        {   
            TBLSource.Inlines.Clear();
            if(TBSource.Text.Trim() != String.Empty)
            {
                string res1,error,res2;
                try
                {
                    API.Translate(TBSource.Text, out string Resault, out string Error);
                    string res = Resault;
                    TBResult.Text = res;
                    return;
                }
                catch(TranslateLibrary.CoreLib.ParsingException ex)
                {
                    TBResult.Text = ex.Message;
                    string[] reses = TBSource.Text.Split(ex.ErrorLine);
                    if(reses.Length != 2)
                        return;
                    res1 = reses[0]; res2 = reses[1];
                    error = ex.ErrorLine;
                    if(ex.ErrorText != null)
                    {
                        reses = ex.ErrorLine.Split(ex.ErrorText);

                        res1+=reses[0];
                        res2=reses[1]+res2;
                         error = ex.ErrorText;
                    }
                   
                    
                }
                catch(TranslateLibrary.CoreLib.AnalyzeException ex)
                {
                    TBResult.Text = ex.Message;
                    string[] reses = TBSource.Text.Split(ex.ErrorLine);
                    if(reses.Length != 2)
                        return;
                    res1 = reses[0]; res2 = reses[1];
                    error = ex.ErrorLine;
                    if(ex.ErrorText != null)
                    {
                        reses = ex.ErrorLine.Split(ex.ErrorText);
                        
                        res1+=reses[0];
                        res2=reses[1]+res2;
                         error = ex.ErrorText;
                    }
                   
                }
                //catch(Exception ex)
                //{
               //     return;
               // }


                TBLSource.Inlines.Add(new Run(res1) { Foreground = Brushes.Black });
                TBLSource.Inlines.Add(new Run(error) { Foreground = Brushes.Red });
                TBLSource.Inlines.Add(new Run(res2) { Foreground = Brushes.Black });
            }
        }
}