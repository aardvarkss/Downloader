using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace Downloader_Common
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private EventLogInstaller eventLogInstaller;
		private static readonly string INSTALLED_APP_NAME = "Meticulus Download Application";
		private static readonly string EVENT_LOG_SOURCE = "Downloader Service";//TODO: Refactor other uses.
		private static readonly string EVENT_LOG = "Meticulus Log";
		
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();


			AfterInstall += new InstallEventHandler(AfterInstallEventHandler);

			eventLogInstaller = new EventLogInstaller();
			eventLogInstaller.Source = EVENT_LOG_SOURCE;
			eventLogInstaller.Log = EVENT_LOG;
			eventLogInstaller.UninstallAction = UninstallAction.NoAction;

			Installers.Add(eventLogInstaller);
		}

		private void AfterInstallEventHandler(object sender, InstallEventArgs e)
		{
			if (eventLogInstaller != null)
			{
				string message = string.Format("{0} event log source created.", INSTALLED_APP_NAME);
				WriteEventLogEntry(message, EventLogEntryType.Information, EVENT_LOG_SOURCE);
			}
		}

		private void WriteEventLogEntry(string message, EventLogEntryType entryType, string source)
		{
			EventLog eventLog = null;
			
			try
			{
				eventLog = new EventLog();
				eventLog.Source = source;
				eventLog.WriteEntry(message, entryType, 0, 0x01);
			}
			finally
			{
				if (eventLog != null)
				{
					eventLog.Close();
				}
			}
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
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
