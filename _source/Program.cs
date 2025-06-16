using System;
using System.Threading;
using System.Windows.Forms;

namespace wib
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///  Only Once Instance Mode Mutex Variable and Remove IDE Error Message
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
