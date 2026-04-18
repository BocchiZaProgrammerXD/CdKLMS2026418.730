using System;
using System.Windows.Forms;
using ColegioLibrarySystem.Views; 

namespace ColegioLibrarySystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm()); 
        }
    }
}