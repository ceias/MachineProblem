using System;
using System.Windows.Forms;

namespace PizzaOrder
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MPFRM03BrisiasLabuson());
        }
    }
}
