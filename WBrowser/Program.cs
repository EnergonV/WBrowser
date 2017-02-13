using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using CommonMark.Syntax;
using System.Security.Permissions;


namespace WBrowser
{
	[PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
   [HostProtectionAttribute(SecurityAction.LinkDemand, SharedState = true, 
	Synchronization = true, ExternalProcessMgmt = true, SelfAffectingProcessMgmt = true)]
   [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]

    static class Program
    {
	
         /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
          Application.EnableVisualStyles();
          Application.SetCompatibleTextRenderingDefault(true);
		  Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
          Application.Run(new WBrowser(Environment.GetCommandLineArgs()));
        }
    }
}


/*			
		    SplashScreen.ShowSplashScreen(); 
			InitializeComponent();
			SplashScreen.SetStatus("Загружается модуль 1");
			System.Threading.Thread.Sleep(500);
			SplashScreen.SetStatus("Загружается модуль 2");
			System.Threading.Thread.Sleep(300);
			SplashScreen.SetStatus("Загружается модуль 3");
			System.Threading.Thread.Sleep(900);
			SplashScreen.SetStatus("Загружается модуль 4");
			System.Threading.Thread.Sleep(100);
			SplashScreen.SetStatus("Загружается модуль 5");
			System.Threading.Thread.Sleep(400);
			SplashScreen.SetStatus("Загружается модуль 6");
			System.Threading.Thread.Sleep(50);
			SplashScreen.SetStatus("Загружается модуль 7");
			System.Threading.Thread.Sleep(240);
			SplashScreen.SetStatus("Загружается модуль 8");
			System.Threading.Thread.Sleep(900);
			SplashScreen.SetStatus("Загружается модуль 9");
			System.Threading.Thread.Sleep(240);
			SplashScreen.SetStatus("Загружается модуль 10");
			System.Threading.Thread.Sleep(90);
			SplashScreen.SetStatus("Загружается модуль 11");
			System.Threading.Thread.Sleep(1000);
			SplashScreen.SetStatus("Загружается модуль 12");
			System.Threading.Thread.Sleep(100);
			SplashScreen.SetStatus("Загружается модуль 13");
            System.Threading.Thread.Sleep(500);
			SplashScreen.SetStatus("Загружается модуль 14", false);
			System.Threading.Thread.Sleep(1000);
			SplashScreen.SetStatus("Загружается модуль 14a", false);
			System.Threading.Thread.Sleep(1000);
			SplashScreen.SetStatus("Загружается модуль 14b", false);
			System.Threading.Thread.Sleep(1000);
			SplashScreen.SetStatus("Загружается модуль 14c", false);
			System.Threading.Thread.Sleep(1000);
			SplashScreen.SetStatus("Загружается модуль 15");
			System.Threading.Thread.Sleep(20);
			SplashScreen.SetStatus("Загружается модуль 16");
			System.Threading.Thread.Sleep(450);
			SplashScreen.SetStatus("Загружается модуль 17");
			System.Threading.Thread.Sleep(240);
			SplashScreen.SetStatus("Загружается модуль 18");
			System.Threading.Thread.Sleep(90);
			SplashScreen.CloseForm();
 */