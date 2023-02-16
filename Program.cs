using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UpdateApp
{
    static class Program
    {       
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var temp = Update.HasNewVersion();
            if (Convert.ToBoolean(temp.result))
            {
                if (Update.IsSystemPath() && ! Update.IsAdministrator())
                {
                    Update.ReStartAppWithAdministrator();                  
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1(temp));
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}
