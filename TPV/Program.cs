using System;
using System.Windows.Forms;

namespace TPV
{
    internal static class Program
    {
        /// <summary>
        /// Aplikazioaren sarrera puntua
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Login());
        }
    }
}
