using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Meticulus_Downloader_Service
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
		private System.ServiceProcess.ServiceInstaller serviceInstaller1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();

			ServiceProcessInstaller process = new ServiceProcessInstaller();

			process.Account = ServiceAccount.LocalSystem;

			ServiceInstaller serviceAdmin = new ServiceInstaller();
			
			serviceAdmin.StartType = ServiceStartMode.Automatic ;
			serviceAdmin.ServiceName = "Meticulus Downloader Service";

			Installers.Add( process );
			Installers.Add( serviceAdmin );
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
			this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();

		}
		#endregion

		private void serviceInstaller1_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e) {
		
		}
	}
}
