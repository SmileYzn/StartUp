using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace StartUp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(false, "{02B31267-B670-4665-A881-7DDADD942172}"))
            {
                if (!mutex.WaitOne(0, true))
                {
                    MessageBox.Show(Application.ProductName + Properties.Resources.Msg_IsRunning, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new FormMain());
                }
            }
        }
    }
}
