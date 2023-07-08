using PortProxyGUI.Data;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PortProxyGUI
{
    static class Program
    {
        public static readonly string AppDbDirectory = Path.Combine(Environment.GetEnvironmentVariable("ALLUSERSPROFILE"), "PortProxyGUI");
        public static readonly string AppDbFile = Path.Combine(AppDbDirectory, "config.db");

        public static readonly ApplicationDbScope Database;

        static Program()
        {
            if (!Directory.Exists(AppDbDirectory)) Directory.CreateDirectory(AppDbDirectory);

            var oldAppDbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PortProxyGUI");
            var oldAppDbFile = Path.Combine(oldAppDbDirectory, "config.db");

            if (!File.Exists(AppDbFile) && File.Exists(oldAppDbFile))
            {
                var result = MessageBox.Show($"""
The configuration database storage location has changed.

Do you want to copy the old database to the new location?

Old location: {oldAppDbFile}
New location: {AppDbFile}

After the software starts, you can manually delete the old configuration directory.
""",
                "Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    File.Copy(oldAppDbFile, AppDbFile, true);
                }
            }

            Database = ApplicationDbScope.FromFile(AppDbFile);
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

#if DEBUG
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
#endif

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
