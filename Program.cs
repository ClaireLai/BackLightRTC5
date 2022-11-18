using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackLight
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new BKMain());
            Forms frm = new Forms();
            frm.init(); 
            Application.Run(Forms.frmMain);
        }        
    }
    class Forms
    {
        public static frmBKMain frmMain;
        public void init()
        {
            frmMain = new frmBKMain();
        }
    }
}
