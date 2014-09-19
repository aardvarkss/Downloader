using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using Downloader_Common;
using System.Threading;
using System.Timers;
using System.Xml;
using System.IO;

namespace Meticulus_Downloader_Service {
	public class Download_Service : System.ServiceProcess.ServiceBase {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private const string		EVENT_LOG_SOURCE = "Downloader Service";

		private System.ComponentModel.Container components = null;
		protected System.Timers.Timer myTimer;
		protected clsUtils m_Utils;
		protected clsDownload m_Dwnld;
		protected EventLog m_AppEventLog;		
		protected int m_intPollInterval;

		public Download_Service() {
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();
			// TODO: Add any initialization after the InitComponent call			
            
			m_Utils = new clsUtils();
			this.ServiceName = m_Utils.GetServiceName();

			m_Dwnld = new clsDownload();
			m_AppEventLog = null;
			m_Utils.CreateEventLog(EVENT_LOG_SOURCE);
		}

		// The main entry point for the process
		static void Main() {
			System.ServiceProcess.ServiceBase[] ServicesToRun;
	
			// More than one user Service may run within the same process. To add
			// another service to this process, change the following line to
			// create a second service object. For example,
			//
			//   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
			//
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Download_Service() };

			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
			//Download_Service DebugService = new Download_Service();
			//DebugService.OnStart(null);
		
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			// 
			// Download_Service
			// 
			this.ServiceName = "Download_Service";

		}
		
		

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if (components != null) {
					components.Dispose();
					if (myTimer != null)
					{
						myTimer.Dispose();
					}
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args) {
			try{
				int iPollMinutes=0,iPollHours=0;

				m_Utils.ReadPollDelay (ref iPollMinutes,ref iPollHours);
				m_intPollInterval = iPollHours*60+iPollMinutes;
				if (m_intPollInterval == 0) return;
				myTimer = new System.Timers.Timer();
				myTimer.Elapsed+=new ElapsedEventHandler(OnTimedEvent);
				myTimer.Interval = m_intPollInterval*60000;
				myTimer.AutoReset = false;
				myTimer.Enabled = true;
				if(m_Utils.DownloadLock()){
					m_Utils.DownloadUnLock();
				}
				m_Utils.WriteEventLogEntry(this.ServiceName + " started.", EventLogEntryType.Information, EVENT_LOG_SOURCE);
			}catch(Exception e){
				m_Utils.WriteEventLogEntry("OnStart: " + e.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
			}
			
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop() {
			if(m_Utils.DownloadLock()){
				m_Utils.DownloadUnLock();
			}
			m_Utils.WriteEventLogEntry(this.ServiceName + " stopped.", EventLogEntryType.Information, EVENT_LOG_SOURCE);

		}
		// Specify what you want to happen when the event is raised.
		private void OnTimedEvent(object source, ElapsedEventArgs e) {
			if(m_Utils.DownloadLock()){
				m_Dwnld.DownloadPrefilledData();
				m_Utils.DownloadUnLock();
			}
			myTimer.Start(); 
		}
	}
}
