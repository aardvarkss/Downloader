using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.Timers;
using System.Threading;
using System.ServiceProcess;
using Downloader_Common;

namespace Download_Manager
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class FormConfigure : System.Windows.Forms.Form
	{
		#region Form variables
		private const string		EVENT_LOG_SOURCE = "Downloader Service";

		private System.Windows.Forms.ContextMenu cmenuNotify;
		private System.Windows.Forms.MenuItem menuConfigure;
		private System.Windows.Forms.MenuItem menuViewDownloads;
		private System.Windows.Forms.MenuItem menuExit;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Form FormVw;
		private System.Windows.Forms.Form FormErr;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnCancel;
		private clsNotifyBalloon niconNotifyBalloon = null;
		protected Icon m_icAllFine;
		protected string m_strDownloadDirectory;
		protected string m_strDownloadURL;
		protected string m_strPollMinutes;
		protected string m_strPollHours;
		protected string m_strCustomerCode;
		protected string m_strSiteCode;
		protected string m_strManifestNumber;
		protected DateTime m_dtLastTimeRun;
		protected bool m_bSwapToError;
		protected bool m_bShowingError;
		private System.Windows.Forms.GroupBox groupBoxDownload;
		private System.Windows.Forms.TextBox txtFolder;
		private System.Windows.Forms.Button btnFolderBrowse;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtURL;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBoxCertificate;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtCustomerCode;
		private System.Windows.Forms.TextBox txtSiteCode;
		protected clsUtils m_Utils;
		private System.Windows.Forms.MenuItem menuViewErrors;
		protected System.Timers.Timer myTimer,myIconTimer;
		protected Icon m_icRed;
		protected Icon m_icYellow;
		protected Icon m_icGreen;
		protected Icon m_icDefault;
		private System.Windows.Forms.MenuItem menuDownload;
		protected Thread m_trdDownloader;
		private System.Windows.Forms.GroupBox groupBoxPollingInterval;
		private System.Windows.Forms.NumericUpDown numUDPollHours;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numUDPollMinutes;
		private System.ServiceProcess.ServiceController serviceControllerDwnld;
	
		protected enum enIconType{Default,Red,Yellow,Green};
		protected enIconType enIcon; 
		//static AutoResetEvent signal;

		#endregion
		public FormConfigure()
		{
			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			int iHours=0,iMinutes=0;

			m_icAllFine = new Icon(GetType(), "downloader.ico");
			if(niconNotifyBalloon == null)
			{
				niconNotifyBalloon = new clsNotifyBalloon();
				niconNotifyBalloon.Text = "Meticulus Download Manager";
				niconNotifyBalloon.Icon = m_icAllFine;
				niconNotifyBalloon.Visible = true;
				niconNotifyBalloon.ContextMenu = cmenuNotify;

				niconNotifyBalloon.BalloonClick += new EventHandler(DownloadIcon_ClickBalloon);
				niconNotifyBalloon.DoubleClick += new EventHandler(DownloadIcon_DoubleClick);
			}

			m_Utils = new clsUtils();
			m_Utils.ReadLastPollDate(ref m_dtLastTimeRun); 
			m_strDownloadDirectory = m_Utils.ReadDownloadDirectory();
			m_strDownloadURL = m_Utils.ReadDownloadURL();
			m_Utils.ReadPollDelay(ref iMinutes,ref iHours);
			m_strPollMinutes = iMinutes.ToString();
			m_strPollHours = iHours.ToString();
			m_strCustomerCode = m_Utils.ReadCustomerCode();
			m_strSiteCode = m_Utils.ReadSiteCode();

			this.WindowState = FormWindowState.Minimized;
			this.menuConfigure.Click +=new EventHandler(menuConfigure_Click);
			this.menuViewDownloads.Click +=new EventHandler(menuViewDownloads_Click);
			this.menuExit.Click += new EventHandler(menuExit_Click);
			m_icRed = new Icon(GetType(), "red.ico");
			m_icYellow = new Icon(GetType(), "yellow.ico");
			m_icGreen = new Icon(GetType(), "green.ico");
			m_icDefault = new Icon(GetType(), "downloader.ico");
			if(m_Utils.DownloadLock()){
				m_Utils.DownloadUnLock();
			}

			//setup up manager app to catch event log errors
			EventLog myNewLog = new EventLog();
			myNewLog.Log = "Meticulus Log";                      
        
			myNewLog.EntryWritten += new EntryWrittenEventHandler(OnEventWritten);
			myNewLog.EnableRaisingEvents = true;
        
			m_bSwapToError = false;
			m_bShowingError = false;
			CheckPastErrorEvents(); 
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}

				if(m_Utils.DownloadLock()){
					m_Utils.DownloadUnLock();
				}

				if (serviceControllerDwnld != null)
				{
					serviceControllerDwnld.Close();
					serviceControllerDwnld.Dispose();
					serviceControllerDwnld = null;
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormConfigure));
			this.components = new System.ComponentModel.Container();
			this.cmenuNotify = new System.Windows.Forms.ContextMenu();
			this.menuViewErrors = new System.Windows.Forms.MenuItem();
			this.menuViewDownloads = new System.Windows.Forms.MenuItem();
			this.menuDownload = new System.Windows.Forms.MenuItem();
			this.menuConfigure = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.groupBoxCertificate = new System.Windows.Forms.GroupBox();
			this.txtSiteCode = new System.Windows.Forms.TextBox();
			this.txtCustomerCode = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBoxDownload = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtFolder = new System.Windows.Forms.TextBox();
			this.btnFolderBrowse = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtURL = new System.Windows.Forms.TextBox();
			this.groupBoxPollingInterval = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.numUDPollHours = new System.Windows.Forms.NumericUpDown();
			this.numUDPollMinutes = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.serviceControllerDwnld = new System.ServiceProcess.ServiceController();
			this.groupBoxCertificate.SuspendLayout();
			this.groupBoxDownload.SuspendLayout();
			this.groupBoxPollingInterval.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numUDPollHours)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numUDPollMinutes)).BeginInit();
			this.SuspendLayout();

			// 
			// cmenuNotify
			// 
			this.cmenuNotify.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuViewErrors,
																						this.menuViewDownloads,
																						this.menuDownload,
																						this.menuConfigure,
																						this.menuExit});
			this.cmenuNotify.Popup += new System.EventHandler(this.Manager_Popup);
			// 
			// menuViewErrors
			// 
			this.menuViewErrors.Index = 0;
			this.menuViewErrors.Text = "View Errors";
			this.menuViewErrors.Click += new System.EventHandler(this.menuViewErrors_Click);
			// 
			// menuViewDownloads
			// 
			this.menuViewDownloads.Index = 1;
			this.menuViewDownloads.Text = "View Last Download";
			// 
			// menuDownload
			// 
			this.menuDownload.Index = 2;
			this.menuDownload.Text = "Download";
			this.menuDownload.Click += new System.EventHandler(this.menuDownload_Click);
			// 
			// menuConfigure
			// 
			this.menuConfigure.DefaultItem = true;
			this.menuConfigure.Index = 3;
			this.menuConfigure.Text = "Configure";
			// 
			// menuExit
			// 
			this.menuExit.Index = 4;
			this.menuExit.Text = "Exit";
			this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// btnOK
			// 
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(208, 240);
			this.btnOK.Name = "btnOK";
			this.btnOK.TabIndex = 9;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnApply
			// 
			this.btnApply.Enabled = false;
			this.btnApply.Location = new System.Drawing.Point(288, 240);
			this.btnApply.Name = "btnApply";
			this.btnApply.TabIndex = 10;
			this.btnApply.Text = "Apply";
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(368, 240);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 11;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// groupBoxCertificate
			// 
			this.groupBoxCertificate.Controls.Add(this.txtSiteCode);
			this.groupBoxCertificate.Controls.Add(this.txtCustomerCode);
			this.groupBoxCertificate.Controls.Add(this.label6);
			this.groupBoxCertificate.Controls.Add(this.label5);
			this.groupBoxCertificate.Location = new System.Drawing.Point(200, 152);
			this.groupBoxCertificate.Name = "groupBoxCertificate";
			this.groupBoxCertificate.Size = new System.Drawing.Size(248, 80);
			this.groupBoxCertificate.TabIndex = 13;
			this.groupBoxCertificate.TabStop = false;
			this.groupBoxCertificate.Text = "Client Information";
			// 
			// txtSiteCode
			// 
			this.txtSiteCode.Location = new System.Drawing.Point(120, 46);
			this.txtSiteCode.Name = "txtSiteCode";
			this.txtSiteCode.Size = new System.Drawing.Size(120, 20);
			this.txtSiteCode.TabIndex = 3;
			this.txtSiteCode.Text = "site1";
			this.txtSiteCode.TextChanged += new System.EventHandler(this.txtSiteCode_TextChanged);
			// 
			// txtCustomerCode
			// 
			this.txtCustomerCode.Location = new System.Drawing.Point(120, 22);
			this.txtCustomerCode.Name = "txtCustomerCode";
			this.txtCustomerCode.Size = new System.Drawing.Size(120, 20);
			this.txtCustomerCode.TabIndex = 2;
			this.txtCustomerCode.Text = "meticulus";
			this.txtCustomerCode.TextChanged += new System.EventHandler(this.txtCustomerCode_TextChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 48);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 16);
			this.label6.TabIndex = 1;
			this.label6.Text = "Site Code:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 16);
			this.label5.TabIndex = 0;
			this.label5.Text = "Customer Code:";
			// 
			// groupBoxDownload
			// 
			this.groupBoxDownload.Controls.Add(this.label2);
			this.groupBoxDownload.Controls.Add(this.txtFolder);
			this.groupBoxDownload.Controls.Add(this.btnFolderBrowse);
			this.groupBoxDownload.Controls.Add(this.label1);
			this.groupBoxDownload.Controls.Add(this.txtURL);
			this.groupBoxDownload.Location = new System.Drawing.Point(8, 8);
			this.groupBoxDownload.Name = "groupBoxDownload";
			this.groupBoxDownload.Size = new System.Drawing.Size(440, 136);
			this.groupBoxDownload.TabIndex = 14;
			this.groupBoxDownload.TabStop = false;
			this.groupBoxDownload.Text = "Download Information";
			this.groupBoxDownload.Enter += new System.EventHandler(this.groupBoxDownload_Enter);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(152, 16);
			this.label2.TabIndex = 20;
			this.label2.Text = "Download Folder Location:";
			// 
			// txtFolder
			// 
			this.txtFolder.Location = new System.Drawing.Point(8, 104);
			this.txtFolder.Name = "txtFolder";
			this.txtFolder.ReadOnly = true;
			this.txtFolder.Size = new System.Drawing.Size(336, 20);
			this.txtFolder.TabIndex = 16;
			this.txtFolder.Text = "";
			// 
			// btnFolderBrowse
			// 
			this.btnFolderBrowse.Location = new System.Drawing.Point(360, 104);
			this.btnFolderBrowse.Name = "btnFolderBrowse";
			this.btnFolderBrowse.TabIndex = 15;
			this.btnFolderBrowse.Text = "Browse";
			this.btnFolderBrowse.Click += new System.EventHandler(this.btnFolderBrowse_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(200, 23);
			this.label1.TabIndex = 14;
			this.label1.Text = "Download Server Web Service URL:";
			// 
			// txtURL
			// 
			this.txtURL.Location = new System.Drawing.Point(8, 48);
			this.txtURL.Name = "txtURL";
			this.txtURL.Size = new System.Drawing.Size(416, 20);
			this.txtURL.TabIndex = 13;
			this.txtURL.Text = "";
			this.txtURL.TextChanged += new System.EventHandler(this.txtURL_TextChanged);
			// 
			// groupBoxPollingInterval
			// 
			this.groupBoxPollingInterval.Controls.Add(this.label3);
			this.groupBoxPollingInterval.Controls.Add(this.numUDPollHours);
			this.groupBoxPollingInterval.Controls.Add(this.numUDPollMinutes);
			this.groupBoxPollingInterval.Controls.Add(this.label4);
			this.groupBoxPollingInterval.Location = new System.Drawing.Point(8, 152);
			this.groupBoxPollingInterval.Name = "groupBoxPollingInterval";
			this.groupBoxPollingInterval.Size = new System.Drawing.Size(192, 80);
			this.groupBoxPollingInterval.TabIndex = 17;
			this.groupBoxPollingInterval.TabStop = false;
			this.groupBoxPollingInterval.Text = "Polling Interval";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(72, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 16);
			this.label3.TabIndex = 23;
			this.label3.Text = "Hours";
			// 
			// numUDPollHours
			// 
			this.numUDPollHours.Location = new System.Drawing.Point(8, 46);
			this.numUDPollHours.Maximum = new System.Decimal(new int[] {
																		   10000,
																		   0,
																		   0,
																		   0});
			this.numUDPollHours.Name = "numUDPollHours";
			this.numUDPollHours.Size = new System.Drawing.Size(56, 20);
			this.numUDPollHours.TabIndex = 22;
			this.numUDPollHours.ValueChanged += new System.EventHandler(this.numUDPollHours_ValueChanged);
			// 
			// numUDPollMinutes
			// 
			this.numUDPollMinutes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.numUDPollMinutes.Location = new System.Drawing.Point(8, 22);
			this.numUDPollMinutes.Maximum = new System.Decimal(new int[] {
																			 10000,
																			 0,
																			 0,
																			 0});
			this.numUDPollMinutes.Name = "numUDPollMinutes";
			this.numUDPollMinutes.Size = new System.Drawing.Size(56, 20);
			this.numUDPollMinutes.TabIndex = 21;
			this.numUDPollMinutes.Value = new System.Decimal(new int[] {
																		   10,
																		   0,
																		   0,
																		   0});
			this.numUDPollMinutes.ValueChanged += new System.EventHandler(this.numUDPollMinutes_ValueChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(72, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 16);
			this.label4.TabIndex = 20;
			this.label4.Text = "Minutes";
			// 
			// FormConfigure
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(450, 271);
			this.ControlBox = false;
			this.Controls.Add(this.groupBoxDownload);
			this.Controls.Add(this.groupBoxCertificate);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.groupBoxPollingInterval);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormConfigure";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Configure Downloader Manager";
			this.Load += new System.EventHandler(this.FormConfigure_Load);
			this.groupBoxCertificate.ResumeLayout(false);
			this.groupBoxDownload.ResumeLayout(false);
			this.groupBoxPollingInterval.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numUDPollHours)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numUDPollMinutes)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			FormConfigure form = null;
			try
			{
				form = new FormConfigure();
				Application.Run(form);
			}
			catch (Exception ex)
			{
				clsUtils utils = new clsUtils();
				utils.CreateEventLog(EVENT_LOG_SOURCE);

				// Write an entry to the event log.
				if (utils != null)
				{
					string strMessage = string.Format("Unexpected error.\n\t{0}\n\t{1}", ex.Message, ex.StackTrace);
					if (utils != null)
					{
						utils.WriteEventLogEntry(strMessage, EventLogEntryType.Error, EVENT_LOG_SOURCE);
					}
				}

				MessageBox.Show("A severe error has occurred causing the Meticulus Download Manager to close.\n\n" + ex.Message, "Meticulus Download Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void Manager_Popup(object sender, System.EventArgs e) {
		}
		private void menuConfigure_Click(object Sender, EventArgs e) {
			this.WindowState = FormWindowState.Normal;
			this.Show();
			m_bSwapToError = false;

		}
		
		private void menuViewDownloads_Click(object Sender, EventArgs e) {
			//this.WindowState = FormWindowState.Minimized;
			if (FormVw != null){
				FormVw.Close();
			}
			FormVw = new FormView();
			FormVw.Show();
			m_bSwapToError = false;
		}
		private void menuExit_Click(object Sender, EventArgs e) {
			this.Close();
		}
		private void btnOK_Click(object sender, System.EventArgs e) {
			if ((numUDPollMinutes.Value + numUDPollHours.Value) >= 0){
				UpdateOptions();
				//this.WindowState = FormWindowState.Minimized;
				this.Hide();
			}else{
				MessageBox.Show("Please set a poll interval",this.Text);
			}
		}

		private void DownloadIcon_ClickBalloon(object sender, EventArgs e) {
			m_bSwapToError = true;
			m_bShowingError = false;
			ShowDlg();
		}
		private void DownloadIcon_DoubleClick(object sender, EventArgs e) {
			ShowDlg();
		}		
		private void ShowDlg(){
			if(m_bSwapToError == true){
				DisplayErrors();
			}else{
				this.WindowState = FormWindowState.Normal;
				Activate();
				this.Show();
			}
			m_bSwapToError = false;
		}
		private void InitVals(){
			int iPollMinutes =  Convert.ToInt32(numUDPollMinutes.Minimum);
			int iPollHours = Convert.ToInt32(numUDPollHours.Minimum);
			m_strDownloadURL = m_Utils.ReadDownloadURL();
			txtURL.Text=m_strDownloadURL;
			m_strDownloadDirectory = m_Utils.ReadDownloadDirectory();
			txtFolder.Text=m_strDownloadDirectory;
			m_Utils.ReadPollDelay (ref iPollMinutes,ref iPollHours);
			if (iPollMinutes < Convert.ToInt32(numUDPollMinutes.Minimum))
				numUDPollMinutes.Value = numUDPollMinutes.Minimum;
			else
				numUDPollMinutes.Value = iPollMinutes;
			numUDPollHours.Value = iPollHours;
			
			m_strCustomerCode = m_Utils.ReadCustomerCode();
			txtCustomerCode.Text = m_strCustomerCode;
			m_strSiteCode = m_Utils.ReadSiteCode();
			txtSiteCode.Text = m_strSiteCode;
			this.btnOK.Enabled = false;
			this.btnApply.Enabled = false;
		}
		private void UpdateOptions(){
			try{
				m_Utils.WriteDownloadDirectory(m_strDownloadDirectory);
				m_Utils.WriteDownloadURL(m_strDownloadURL);
				m_Utils.WritePollDelay(Convert.ToInt32(m_strPollMinutes),Convert.ToInt32(m_strPollHours));
				m_Utils.WriteCustomerCode(m_strCustomerCode);
				m_Utils.WriteSiteCode(m_strSiteCode);
			}
			catch(Exception e)
			{
				niconNotifyBalloon.ShowBalloon("Meticulus FAS",e.Message + "\n\nClick here to view the errors.", clsNotifyBalloon.NotifyInfoFlags.Error);
			}

			try
			{
				serviceControllerDwnld.ServiceName = m_Utils.GetServiceName();
				serviceControllerDwnld.Refresh();
				if(!serviceControllerDwnld.Status.Equals(ServiceControllerStatus.Stopped))
				{ 
					serviceControllerDwnld.Stop();
					serviceControllerDwnld.WaitForStatus(ServiceControllerStatus.Stopped);
				}
				serviceControllerDwnld.Start();
			}
			catch (Exception e)
			{
				//Just log exceptions here as warnings. The service may not be installed.
				string message = e.Message + "\n" + "Please ignore this warning if the service is not installed.";
				m_Utils.WriteEventLogEntry(message, EventLogEntryType.Warning, EVENT_LOG_SOURCE);
			}
		}

		private void FormConfigure_Load(object sender, System.EventArgs e) {
			InitVals();
		}

		private void txtCustomerCode_TextChanged(object sender, System.EventArgs e) {
			m_strCustomerCode = txtCustomerCode.Text;
			if (txtCustomerCode.Text  != ""){
				this.btnOK.Enabled = true;
				this.btnApply.Enabled = true;
			}
			
		}

		private void txtSiteCode_TextChanged(object sender, System.EventArgs e) {
			m_strSiteCode = txtSiteCode.Text;
			if (txtSiteCode.Text  != ""){
				this.btnOK.Enabled = true;
				this.btnApply.Enabled = true;
			}
		}

		private void btnCancel_Click(object sender, System.EventArgs e) {
			InitVals();
			this.Hide();
		}
		

		private void btnFolderBrowse_Click(object sender, System.EventArgs e) {
			FolderBrowserDialog dlgBrowse;

			dlgBrowse = new FolderBrowserDialog();

			dlgBrowse.Description = "Browse for Download Folder Location";
			dlgBrowse.SelectedPath = m_strDownloadDirectory;
			dlgBrowse.ShowNewFolderButton = false;

			if (dlgBrowse.ShowDialog() == DialogResult.OK){
				m_strDownloadDirectory = dlgBrowse.SelectedPath;
				this.txtFolder.Text = m_strDownloadDirectory;
				this.btnOK.Enabled = true;
				this.btnApply.Enabled = true;
			}
		}

		private void txtURL_TextChanged(object sender, System.EventArgs e) {
			m_strDownloadURL = txtURL.Text;
			if (txtURL.Text != ""){
				this.btnOK.Enabled = true;
				this.btnApply.Enabled = true;
			}
		
		}
		
		private void OnTimedSwapIcon(object source, ElapsedEventArgs e) {
			try{
				switch(enIcon){
					case enIconType.Green:
						enIcon = enIconType.Yellow;
						niconNotifyBalloon.Icon = m_icYellow;
						break;
					case enIconType.Yellow: 
						enIcon = enIconType.Red;
						niconNotifyBalloon.Icon = m_icRed;
						break;
					case enIconType.Red:
						enIcon = enIconType.Green;
						niconNotifyBalloon.Icon = m_icGreen;
						break;
					default:
						enIcon = enIconType.Green;
						niconNotifyBalloon.Icon = m_icGreen;
						break;
				}

				//DownloadIcon.Text = "Meticulus Download Error";
				myIconTimer.Start(); 
			}catch{
				//ensure icon swapping continues
				myIconTimer.Start(); 
			}
		}
		private void OnEventWritten(object source, EntryWrittenEventArgs e)
		{
			EventLogEntry eleEntry;
			eleEntry = e.Entry;			
			if(eleEntry.Source == EVENT_LOG_SOURCE && eleEntry.EntryType == EventLogEntryType.Error && !m_bShowingError){
				m_bShowingError = true;
				niconNotifyBalloon.ShowBalloon("Meticulus FAS", "Download error has occurred.\n\nClick here to view the errors.",clsNotifyBalloon.NotifyInfoFlags.Error);
				SwapIcons();
			}
		}

		private void CheckPastErrorEvents() 
		{
			bool bErrorFound = false;
			int iCounter = 0;
			EventLog elmeticulusLog = new EventLog();
			EventLogEntry eleEntry;

			elmeticulusLog.Log = "Meticulus Log";
			for(iCounter=elmeticulusLog.Entries.Count-1; iCounter>=0 && bErrorFound == false; iCounter--){
				eleEntry = elmeticulusLog.Entries[iCounter];
				if(DateTime.Compare(eleEntry.TimeWritten, m_dtLastTimeRun) > 0){
					if(eleEntry.Source == EVENT_LOG_SOURCE && eleEntry.EntryType == EventLogEntryType.Error){
						bErrorFound = true;
						niconNotifyBalloon.ShowBalloon("Meticulus FAS", "Download error has occurred.\n\nClick here to view the errors.",clsNotifyBalloon.NotifyInfoFlags.Error);
					}
				}
			}
			if(bErrorFound == true){
				SwapIcons();
			}

			if (elmeticulusLog != null)
			{
				elmeticulusLog.Close();
				elmeticulusLog.Dispose();
			}
		}
	
		private void SwapIcons(){
			m_bSwapToError = true;
			m_dtLastTimeRun = DateTime.Now;
			m_Utils.WriteLastPollDate(m_dtLastTimeRun);
			myIconTimer = new System.Timers.Timer();
			myIconTimer.Elapsed+=new ElapsedEventHandler(OnTimedSwapIcon);
			myIconTimer.Interval = 500;
			myIconTimer.AutoReset = false;
			myIconTimer.Enabled = true;
		}
		private void DisplayErrors(){
			if (FormErr != null){
				FormErr.Close();
			}
			if (myIconTimer != null) myIconTimer.Stop();
			niconNotifyBalloon.Icon = m_icDefault;
			FormErr = new FormErrors(EVENT_LOG_SOURCE);
			FormErr.Show();
			FormErr.Activate();
		}

		private void menuViewErrors_Click(object sender, System.EventArgs e) {
			DisplayErrors();
			m_bSwapToError = false;
		}

		private void groupBoxDownload_Enter(object sender, System.EventArgs e) {
		
		}
		private void DownloaderThread(){
			try{
				clsDownload m_Dwnld;
				clsDownload.enStatusType enStatus;
				
				m_Dwnld = new clsDownload();
				enStatus = m_Dwnld.DownloadPrefilledData();
				if (enStatus == clsDownload.enStatusType.Downloaded){
					niconNotifyBalloon.ShowBalloon("Meticulus FAS", "Download completed successfully.", clsNotifyBalloon.NotifyInfoFlags.None);
				}else if(enStatus == clsDownload.enStatusType.GotLatest){
					niconNotifyBalloon.ShowBalloon("Meticulus FAS", "Downloads are already up to date.", clsNotifyBalloon.NotifyInfoFlags.Info);
				}else{
					niconNotifyBalloon.ShowBalloon("Meticulus FAS", "Download error has occurred.\n\nClick here to view the errors.", clsNotifyBalloon.NotifyInfoFlags.Error);
					SwapIcons();
				}
				m_Utils.DownloadUnLock();
			}catch(Exception e){
				m_Utils.DownloadUnLock();
				niconNotifyBalloon.ShowBalloon("Meticulus FAS",e.Message + "\n\nClick here to view the errors.", clsNotifyBalloon.NotifyInfoFlags.Error);
			}
			
		}
		private void menuDownload_Click(object sender, System.EventArgs e) {
			if (m_Utils.DownloadLock()){
				ThreadStart tsDownloader = new ThreadStart( this.DownloaderThread );
				m_trdDownloader = new Thread( tsDownloader );
				m_trdDownloader.Start();
			}else{
				niconNotifyBalloon.ShowBalloon("Meticulus FAS", "Download already running", clsNotifyBalloon.NotifyInfoFlags.Info);
			}
		}

		private void numUDPollMinutes_ValueChanged(object sender, System.EventArgs e) {
			m_strPollMinutes =  numUDPollMinutes.Value.ToString();
			this.btnOK.Enabled = true;
			this.btnApply.Enabled = true;
		}

		private void numUDPollHours_ValueChanged(object sender, System.EventArgs e) {
			m_strPollHours = numUDPollHours.Value.ToString();
			this.btnOK.Enabled = true;
			this.btnApply.Enabled = true;
		}

		private void btnApply_Click(object sender, System.EventArgs e) {
			if ((numUDPollMinutes.Value + numUDPollHours.Value) >= 0){
				UpdateOptions();
			}else{
				MessageBox.Show("Please set a poll interval",this.Text);
			}
		}

		
	}
}
