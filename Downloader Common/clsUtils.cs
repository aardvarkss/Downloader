using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace Downloader_Common
{
	/// <summary>
	/// Summary description for Utils.
	/// </summary>
	public class clsUtils
	{
		public clsUtils(){}
		
		private const string SERVICE_NAME = "Meticulus Downloader Service";
		
		public string GetServiceName(){
			return SERVICE_NAME;
		}

		public void ReadPollDelay(ref int iMinuteDelay,ref int iHourDelay) {
			iMinuteDelay = -1;
			iHourDelay  = -1;

			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader");
				if (rkBase != null)
				{
					string strTimeDelay = (String)rkBase.GetValue("PollDelayMinutes");
					iMinuteDelay = Convert.ToInt32(strTimeDelay);

					strTimeDelay = (String)rkBase.GetValue("PollDelayHours");
					iHourDelay = Convert.ToInt32(strTimeDelay);
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public void WritePollDelay(int iMinuteDelay,int iHourDelay) {
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
				if (rkBase != null)
				{
					string strTimeDelay = iMinuteDelay.ToString();
					rkBase.SetValue("PollDelayMinutes", strTimeDelay);
					strTimeDelay = iHourDelay.ToString();
					rkBase.SetValue("PollDelayHours", strTimeDelay);
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public string ReadDownloadDirectory()
		{
			return ReadRegistryStringValue("Software\\Meticulus\\Downloader", "Download Directory");
		}

		public void WriteDownloadDirectory(string strDirectory){
			RegistryKey rkBase = null;

			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
				if(rkBase != null)
				{
					rkBase.SetValue("Download Directory", strDirectory);
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public string ReadDownloadURL()
		{
			return ReadRegistryStringValue("Software\\Meticulus\\Downloader", "Download URL");
		}

		public void WriteDownloadURL(string strURL){
			RegistryKey rkBase = null;

			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
				if(rkBase != null)
				{
					rkBase.SetValue("Download URL", strURL);
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public string ReadCustomerCode()
		{
			return ReadRegistryStringValue("Software\\Meticulus\\Downloader", "Customer Code");
		}

		public string ReadSiteCode()
		{
			return ReadRegistryStringValue("Software\\Meticulus\\Downloader", "Site Code");
		}

		public void WriteCustomerCode(string strCustomerCode)
		{
			RegistryKey rkBase = null;

			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
				if(rkBase != null)
				{
					rkBase.SetValue("Customer Code", strCustomerCode);
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public void WriteSiteCode(string strSiteCode)
		{
			RegistryKey rkBase = null;

			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
				if(rkBase != null)
				{
					rkBase.SetValue("Site Code", strSiteCode);
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public string ReadManifestNumber()
		{
			return ReadRegistryStringValue("Software\\Meticulus\\Downloader", "Manifest Number");
		}

		public void WriteManifestNumber(string strManifestNumber)
		{
			RegistryKey rkBase = null;

			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
				if(rkBase != null)
				{
					rkBase.SetValue("Manifest Number", strManifestNumber);
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public void DownloadUnLock()
		{
			RegistryKey rkBase = null;

			try
			{
				lock(this)
				{
					rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
					if(rkBase != null)
					{
						rkBase.SetValue("DownloadLock", "None");
					}
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		//FIXED BY CRQ-100369: Errors on uninstall, if Manager is running.
		//TODO: Fix locking strategy to use Mutex?
		public bool DownloadLock(){
			string strVal = "None";
			string strProcess;

			RegistryKey rkBase = null;
			lock(this)
			{
				Process currentProcess = Process.GetCurrentProcess();
				strProcess = currentProcess.ProcessName;
				try
				{
					rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader");
					if(rkBase != null)
					{
						strVal = (String)rkBase.GetValue("DownloadLock");
					}
					else
					{
						strVal = "None";
					}
				}
				finally
				{
					if (rkBase != null)
					{
						rkBase.Close();
					}
				}

				try
				{
					if (strVal.CompareTo("None")==0)
					{
						rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
						if(rkBase != null)
						{
							rkBase.SetValue("DownloadLock",currentProcess.ProcessName);
						}
						return true;
					}
					else if (strVal.CompareTo(strProcess)==0)
					{
						//lock is from same product so ignore and return true.
						return true;
					}
					else
					{
						Process [] localByName = Process.GetProcessesByName(strVal);
						//if the lock is held by the manager and the service wants to lock it, make sure the service is still running
						//if not give the lock to the manager.
					
						if (localByName.Length == 0)
						{
							//Process is not running but has the lock, so lets take it
							rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
							if(rkBase != null)
							{
								rkBase.SetValue("DownloadLock",strProcess);
							}
							return true;
						}
						else
						{
							return false;
						}
					}
				}
				finally
				{
					if (rkBase != null)
					{
						rkBase.Close();
					}
				}
			}
		}

		public string ReadPrintDirectory()
		{
			return ReadRegistryStringValue("Software\\Meticulus\\Remote Printing", "Print Directory");
		}

		public string ReadPDFDirectory()
		{
			return ReadRegistryStringValue("Software\\Meticulus\\Remote Printing", "PDF Directory");
		}

		public void ReadLastPollDate(ref DateTime dtPollDate){
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader");
				if (rkBase != null)
				{
					string strPollDate = (String)rkBase.GetValue("LastPollDate");
					if ((strPollDate == null) || (strPollDate.Length == 0))
					{
						dtPollDate = DateTime.MinValue;
					}
					else
					{
						long lTemp = Convert.ToInt64(strPollDate);
						dtPollDate = new DateTime(lTemp);
					}
				}
				else
				{
					dtPollDate = DateTime.MinValue;
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		public void ReadLastPollDate(ref string strPollDate){
			DateTime dtTemp;

			dtTemp = new DateTime();
			ReadLastPollDate(ref dtTemp);
			strPollDate = Convert.ToString(dtTemp);
		}

		public void WriteLastPollDate(string strPollDate){
			WriteLastPollDate(Convert.ToDateTime(strPollDate));
		}

		public void WriteLastPollDate(DateTime dtPollDate){
			WriteLastPollDate(dtPollDate.Ticks);
		}

		public void WriteLastPollDate(long lTicks){
			RegistryKey rkBase = null;

			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey("Software\\Meticulus\\Downloader", true);
				if(rkBase != null)
				{
					rkBase.SetValue("LastPollDate", Convert.ToString(lTicks));
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}
		}

		private string ReadRegistryStringValue(string keyName, string valueName)
		{
			object data = null;
			RegistryKey rkBase = null;
			try
			{
				rkBase = Registry.LocalMachine.OpenSubKey(keyName);
				if (rkBase == null)
				{
					throw new Exception(keyName + " registry key not found.");
				}

				data = rkBase.GetValue(valueName);
				if (data == null)
				{
					throw new Exception(keyName + "Value: " + valueName + " registry name not found.");
				}
			}
			finally
			{
				if (rkBase != null)
				{
					rkBase.Close();
				}
			}

			return (string)data;
		}

		public void CreateEventLog(string source)
		{
			if (!EventLog.SourceExists(source))
			{
				EventLog.CreateEventSource(source, "Meticulus Log");
			}
		}
		
		public void WriteEventLogEntry(string message, EventLogEntryType entryType, string source)
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

		//TODO: Need check sum or hash code etc.
		public bool FileUpToDate(string strFilePath , string strFileSize )
		{
			bool fileUpToDate = false;
			if (File.Exists(strFilePath))
			{
				FileInfo FileInf = new FileInfo(strFilePath);
				try
				{
					if (FileInf.Length == Convert.ToInt64(strFileSize))
					{
						fileUpToDate = true;
					}
				}
				catch
				{
				}
			}
			return fileUpToDate;
		}
	}
}
