using PortProxyGUI.Data;
using System;
using System.IO;
using System.Windows.Forms;
namespace PortProxyGUI {
    static class Program {
        public static readonly ApplicationDbScope Database = ApplicationDbScope.FromFile(
            Path.Combine(
                Environment.CurrentDirectory, "config.db"
            ));
        [STAThread]
        static void Main() {
#if NET6_0_OR_GREATER
            ApplicationConfiguration.Initialize();
#elif NETCOREAPP3_1_OR_GREATER    
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#else
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Application.Run(new PortProxyGUI());
        }
    }
}
