using System.Text;

namespace TranslateLibrary;
public class CodeGenerator
{
    public static string GenCode(Node[] NodeTree)
    {
        //StringBuilder Resb = new StringBuilder(10000);
        String Resb=String.Empty;

        foreach (var item in NodeTree)
        {
            Resb+=item.ToString()+ "\n";
            //Корректно получать доступ к корневому элементу. Можно даже не хранить все, а только корневой.
        }
        return Resb.ToString();
    }
}