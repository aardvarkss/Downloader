using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Timers;
using System.Xml;
using System.IO;
using System.Threading;
using Downloader_Common;
using Downloader_Common.PdfRenditionDownloaderWebService;
using Downloader_Common.PrefillDataDownloaderWebService;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

///////////////////////////////////////////////////////////////////////////////
///
///Following error is caused by incorrect Url for webservice e.g. in Registry.
///Additional information: Server did not recognize the value of HTTP Header 
///SOAPAction: http://DigitalPenAndPaper.net/webservices/GetManifest.
///
///////////////////////////////////////////////////////////////////////////////

namespace Downloader_Common
{
	/// <summary>
	/// Summary description for clsDownload.
	/// </summary>
	public class clsDownload
	{
		private const string		EVENT_LOG_SOURCE = "Downloader Service";

		private clsUtils m_Utils;
		public string m_strDownloadDirectory;
		public string m_strDownloadURL;
		public string m_strPrintDirectory;
		public string m_strPrintManifestDirectory;
		public string m_strPDFDirectory;
		private bool m_pdfFolderSpecified;
		private bool m_printFolderSpecified;
        private bool m_printManifestFolderSpecified;

		private Hashtable downloadedFileTableToMove = null;
		private Hashtable downloadedFileTableToCopy = null;
        private Dictionary<string, XmlNode> downloadedManifestFileTable = null;

		private const int RETRIES = 3;
		public enum enStatusType{Failure,GotLatest,Downloaded};

		private void ReadRegistry()
		{
			m_strDownloadURL = m_Utils.ReadDownloadURL();

			try
			{
				m_strDownloadDirectory = m_Utils.ReadDownloadDirectory();
			}
			catch
			{
				m_strDownloadDirectory = "";
			}

			try
			{
				m_strPrintDirectory = m_Utils.ReadPrintDirectory();
                m_strPrintManifestDirectory = m_strPrintDirectory + "\\Print Manifest Documents";
			}
			catch
			{
				m_strPrintDirectory = "";
                m_strPrintManifestDirectory = "";
			}

			try
			{
				m_strPDFDirectory = m_Utils.ReadPDFDirectory();
			}
			catch
			{
				m_strPDFDirectory = "";
			}
			
			ValidateFolders();
		}

		private void ValidateFolders()
		{
			if (m_strDownloadDirectory.Length == 0)
			{
				m_Utils.WriteEventLogEntry("Downloaded files folder not configured.", EventLogEntryType.Error, EVENT_LOG_SOURCE);
			}

			if (!Directory.Exists(m_strDownloadDirectory))
			{
                string error = string.Format("Cannot access downloaded files folder ({0}).", m_strDownloadDirectory);
				m_Utils.WriteEventLogEntry(error, EventLogEntryType.Error, EVENT_LOG_SOURCE);
			}

			if (m_strPDFDirectory.Length > 0)
			{
				m_pdfFolderSpecified = true;
				if (!Directory.Exists(m_strPDFDirectory))
				{
					string error = string.Format("Cannot access pdf print folder ({0}).", m_strPDFDirectory);
					m_Utils.WriteEventLogEntry(error, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
			}

			if (m_strPrintDirectory.Length == 0)
			{
				m_Utils.WriteEventLogEntry("Print folder not configured.", EventLogEntryType.Warning, EVENT_LOG_SOURCE);
			}
			else
			{
				m_printFolderSpecified = true;
				if (!Directory.Exists(m_strPrintDirectory))
				{
					string error = string.Format("Cannot access print folder ({0}).", m_strPrintDirectory);
					m_Utils.WriteEventLogEntry(error, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
			}

            if (m_strPrintManifestDirectory.Length == 0)
			{
				m_Utils.WriteEventLogEntry("Print manifest folder not configured.", EventLogEntryType.Warning, EVENT_LOG_SOURCE);
			}
			else
			{
				m_printManifestFolderSpecified = true;
                if (!Directory.Exists(m_strPrintManifestDirectory))
				{
                    string error = string.Format("Cannot access print manifest folder ({0}).", m_strPrintManifestDirectory);
					m_Utils.WriteEventLogEntry(error, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
			}
		}

		public clsDownload(){
			m_Utils = new clsUtils();
			m_Utils.CreateEventLog(EVENT_LOG_SOURCE);
			downloadedFileTableToCopy = new Hashtable();
			downloadedFileTableToMove = new Hashtable();
            downloadedManifestFileTable = new Dictionary<string, XmlNode>();
		}

		/// <summary>
		/// Downloads latest manifest for configured customer and site.
		/// </summary>
		/// <returns></returns>
		public enStatusType DownloadPrefilledData()
		{
            ClearDownloadedFileTables();
			ReadRegistry();

			string customerCode = m_Utils.ReadCustomerCode();
			string siteCode = m_Utils.ReadSiteCode();
			int manifestNumber = 0;//Downloads all open (not yet confirmed downloaded) files.

			return DoDownload(customerCode, siteCode, manifestNumber);
		}

        private void ClearDownloadedFileTables()
        {
            downloadedFileTableToMove.Clear();
            downloadedFileTableToCopy.Clear();
            downloadedManifestFileTable.Clear();
        }
		/// <summary>
		/// Downloads specified manifest for specified customer and site.
		/// </summary>
		/// <returns></returns>
		public enStatusType DownloadSpecificPrefilledData(string downloaderServiceUrl, string customerCode, string siteCode, int manifestNumber)
		{
            ClearDownloadedFileTables();
            ReadRegistry();

			//Override registry value, since specified by client.
			m_strDownloadURL = downloaderServiceUrl;

			return DoDownload(customerCode, siteCode, manifestNumber);
		}
		/// <summary>
		/// Does the download.
		/// </summary>
		/// <returns></returns>
		private enStatusType DoDownload(string customerCode, string siteCode, int manifestNumber)
		{
			enStatusType enStatus = enStatusType.Failure;
			try
			{
				bool downloadingPrefilledFiles = true;
				enStatus = DownloadFiles(customerCode, siteCode, manifestNumber, downloadingPrefilledFiles);
			}
			catch(Exception e)
			{
				string error = string.Format("Download failed for customer code {0}, site code {1}.\n",
					customerCode, siteCode);
				m_Utils.WriteEventLogEntry(error + e.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
			}

			return enStatus;
		}		
		/// <summary>
		/// Downloads specified manifest of pdf renditions.
		/// </summary>
		/// <returns></returns>
		public enStatusType DownloadPdfRenditionData(string downloaderServiceUrl, string customerCode, string siteCode, int manifestNumber)
		{
            ClearDownloadedFileTables();
            ReadRegistry();

			//Override registry value, since specified by client.
			m_strDownloadURL = downloaderServiceUrl;

			enStatusType enStatus = enStatusType.Failure;
			try
			{
				bool downloadingPrefilledFiles = false;
				enStatus = DownloadFiles(customerCode, siteCode, manifestNumber, downloadingPrefilledFiles);
			}
			catch(Exception e)
			{
				string error = string.Format("Download failed for customer code {0}, site code {1}.\n",
					customerCode, siteCode);
				m_Utils.WriteEventLogEntry(error + e.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
			}

			return enStatus;
		}

		private enStatusType DownloadFiles(string customerCode, string siteCode, int manifestNumber, bool downloadingPrefilledFiles)
		{
			XmlDocument manifest = DownloadManifest(customerCode, siteCode, manifestNumber, downloadingPrefilledFiles);
			if (manifest == null)
			{
				//Nothing new to download.
				return enStatusType.GotLatest;
			}

			//Download files in manifest.
			if (downloadingPrefilledFiles)
			{
				DownloadPrefilledFiles(customerCode, siteCode, manifest);
			}
			else
			{
				DownloadPdfRenditionFiles(customerCode, siteCode, manifest);
			}

			//Finally, tell the server that we are done with all the files in this manifest,
			//and then copy them to the print folder watcher area. The server won't given 
			//them to us again, unless we ask specifically for them by manifest number.
			//Note: It's only after this call that we can be sure everything has worked.
			bool success = SetManifestDownloaded(customerCode, siteCode, manifest, downloadingPrefilledFiles);
			if (success)
			{
                //Dump a copy of the manifest node for each file to the PFW folder
                //so that the PFW can work out what printer and DuplexMode to use.
                // Do this first so that the PFW can read it when the pdf/fdf is ready to print.
                DumpDownloadedManifestFiles();

				//Copy downloaded form pdf source files into pdf folder watcher (if set).
				//Must do this before moving Xfdf files, since Xfdfs reference the pdf.
				//Note: This can throw an exception if a new version of the PDF has been downloaded
				//		and the old version is still open in Adobe. This is by design.
				CopyDownloadedFiles();

				//Move downloaded Xfdf and pdf rendition files into print folder watcher (if set).
				MoveDownloadedFiles();
				
				//Save manifest to disk, purely for audit purposes.
				SaveManifest(manifest, customerCode, siteCode);

				return enStatusType.Downloaded;
			}
			else
			{
				//We don't need to purge downloaded files from download area.
				//The next call to get manifest will only download then again if 
				//the checksum (file size) has changed. SO they will get copied next time.
				return enStatusType.Failure;
			}
		}

		private void CopyDownloadedFiles()
		{
			IDictionaryEnumerator enumerator = downloadedFileTableToCopy.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string strSrcPath = (string)enumerator.Key;
				string strDestPath = (string)enumerator.Value;
				CopyFile(strSrcPath, strDestPath);
			}
		}

		private void MoveDownloadedFiles()
		{
			IDictionaryEnumerator enumerator = downloadedFileTableToMove.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string strSrcPath = (string)enumerator.Key;
				string strDestPath = (string)enumerator.Value;
				MoveFile(strSrcPath, strDestPath);
			}
		}

		private void DumpDownloadedManifestFiles()
		{
            IDictionaryEnumerator enumerator = downloadedManifestFileTable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string strSrcPath = (string)enumerator.Key;
                XmlNode fileNode = (XmlNode)enumerator.Value;

                SaveBytesToFile(Encoding.UTF8.GetBytes(fileNode.OuterXml), strSrcPath);
            }
		}
		private XmlDocument DownloadManifest(string customerCode, string siteCode, int manifestNumber, bool downloadingPrefilledFiles)
		{
			Byte[] data = null;
			if (downloadingPrefilledFiles)
			{
				data = GetPrefilledManifest(customerCode, siteCode, manifestNumber);
			}
			else
			{
				data = GetPdfRenditionManifest(customerCode, siteCode, manifestNumber);
			}

			MemoryStream memoryStream = new MemoryStream(data);
			XmlDocument manifest = new XmlDocument();
			manifest.Load(memoryStream);
			memoryStream.Close();

			int downloadedManifestNumber = GetManifestNumber(manifest);
			if (downloadedManifestNumber == 0)
			{
				//Nothing new to download.
				manifest = null;
			}

			if (manifestNumber != 0 && manifestNumber != downloadedManifestNumber)
			{
				string error = string.Format("Failed to download requested manifest number (Requested = {0}, Downloaded = {1}).",
					manifestNumber, downloadedManifestNumber);
				throw new Exception(error);
			}

			return manifest;
		}

		private int GetManifestNumber(XmlDocument manifest)
		{
			XmlNode node = manifest.SelectSingleNode("/serverDownloadInfo/manifestNumber");
			string downloadedManifestNumber = node.InnerText;
			return Convert.ToInt32(downloadedManifestNumber);
		}

		/// <summary>
		/// Save manifest only for diagnostic purposes.
		/// </summary>
		/// <param name="manifest"></param>
		/// <param name="customerCode"></param>
		/// <param name="siteCode"></param>
		/// <param name="manifestNumber"></param>
		private void SaveManifest(XmlDocument manifest, string customerCode, string siteCode)
		{
			int manifestNumber = GetManifestNumber(manifest);
			string fileName = string.Format("{0}\\{1}_{2}_{3}_manifest.xml", m_strDownloadDirectory, customerCode, siteCode, manifestNumber);
			try
			{
				manifest.Save(fileName);
				m_Utils.WriteManifestNumber(manifestNumber.ToString());
			}
			catch (Exception ex)
			{
				//Just log any error here.
				string error = string.Format("Unable to save mainfest file {0} to disk.\n", fileName);
				m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Warning, EVENT_LOG_SOURCE);
			}
		}

		private bool SetManifestDownloaded(string customerCode, string siteCode, XmlDocument manifest, bool downloadingPrefilledFiles)
		{
			int manifestNumber = GetManifestNumber(manifest);
			if (downloadingPrefilledFiles)
			{
				return SetPrefilledManifestDownloaded(customerCode, siteCode, manifestNumber);
			}
			else
			{
				return SetPdfRenditionManifestDownloaded(customerCode, siteCode, manifestNumber);
			}
		}

		private bool SetPrefilledManifestDownloaded(string customerCode, string siteCode, int manifestNumber)
		{
			bool bSuccess = false;
			for (int iFailureCnt = 0; !bSuccess && iFailureCnt < RETRIES; iFailureCnt++)
			{
				PrefillDataDownloader prefillDownloader = null;
				try
				{
					prefillDownloader = new PrefillDataDownloader();
					prefillDownloader.Url = m_strDownloadURL;
					prefillDownloader.SetManifestDownloaded(customerCode, siteCode, manifestNumber);
					bSuccess = true;
				}
				catch (Exception ex)
				{
					string error = string.Format("Set prefilled manifest downloaded failed for customer code {0}, site code {1}, manifest number {2}. Retrying...\n",
						customerCode, siteCode, manifestNumber);
					m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
				finally
				{
					if (prefillDownloader != null)
					{
						prefillDownloader.Dispose();
					}
				}
			}

			if (!bSuccess)
			{
				//Note: Don't throw here, because we need to purge the downloaded files.
				string error = string.Format("Set prefilled manifest downloaded failed after {0} retries. Download must be run again.", RETRIES);
				m_Utils.WriteEventLogEntry(error, EventLogEntryType.Error, EVENT_LOG_SOURCE);
			}

			return bSuccess;
		}

		private bool SetPdfRenditionManifestDownloaded(string customerCode, string siteCode, int manifestNumber)
		{
			bool bSuccess = false;
			for (int iFailureCnt = 0; !bSuccess && iFailureCnt < RETRIES; iFailureCnt++)
			{
				PdfRenditionDownloader pdfRenditionDownloader = null;
				try
				{
					pdfRenditionDownloader = new PdfRenditionDownloader();
					pdfRenditionDownloader.Url = m_strDownloadURL;
					pdfRenditionDownloader.SetManifestDownloaded(customerCode, siteCode, manifestNumber);
					bSuccess = true;
				}
				catch (Exception ex)
				{
					string error = string.Format("Set pdf rendition manifest downloaded failed for customer code {0}, site code {1}, manifest number {2}. Retrying...\n",
						customerCode, siteCode, manifestNumber);
					m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
				finally
				{
					if (pdfRenditionDownloader != null)
					{
						pdfRenditionDownloader.Dispose();
					}
				}
			}

			if (!bSuccess)
			{
				//Note: Don't throw here, because we need to purge the downloaded files.
				string error = string.Format("Set pdf rendition manifest downloaded failed after {0} retries. Download must be run again.", RETRIES);
				m_Utils.WriteEventLogEntry(error, EventLogEntryType.Error, EVENT_LOG_SOURCE);
			}

			return bSuccess;
		}

		private Byte[] GetPrefilledManifest(string customerCode, string siteCode, int manifestNumber)
		{
			Byte[] data = null;
			bool bSuccess = false;
			for (int iFailureCnt = 0; !bSuccess && iFailureCnt < RETRIES; iFailureCnt++)
			{
				PrefillDataDownloader downloader = null;
				try
				{
					downloader = new PrefillDataDownloader();
					downloader.Url = m_strDownloadURL;
					data = downloader.GetManifest(customerCode, siteCode, manifestNumber);
					bSuccess = true;
				}
				catch (Exception ex)
				{
					string error = string.Format("Get prefilled manifest failed for customer code {0}, site code {1}, manifest number {2}. Retrying...\n",
						customerCode, siteCode, manifestNumber);
					m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
				finally
				{
					if (downloader != null)
					{
						downloader.Dispose();
					}
				}
			}

			if (!bSuccess)
			{
				string error = string.Format("Attempt to downloaded prefilled manifest failed after {0} retries.", RETRIES);
				throw new Exception(error);
			}

			return data;
		}

		private Byte[] GetPdfRenditionManifest(string customerCode, string siteCode, int manifestNumber)
		{
			Byte[] data = null;
			bool bSuccess = false;
			for (int iFailureCnt = 0; !bSuccess && iFailureCnt < RETRIES; iFailureCnt++)
			{
				PdfRenditionDownloader downloader = null;
				try
				{
					downloader = new PdfRenditionDownloader();
					downloader.Url = m_strDownloadURL;
					data = downloader.GetPdfRenditionManifest(customerCode, siteCode, manifestNumber);
					bSuccess = true;
				}
				catch (Exception ex)
				{
					string error = string.Format("Get pdf rendition manifest failed for customer code {0}, site code {1}, manifest number {2}. Retrying...\n",
						customerCode, siteCode, manifestNumber);
					m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
				finally
				{
					if (downloader != null)
					{
						downloader.Dispose();
					}
				}
			}

			if (!bSuccess)
			{
				string error = string.Format("Attempt to download pdf rendition manifest failed after {0} retries.", RETRIES);
				throw new Exception(error);
			}

			return data;
		}

		private Byte[] GetPdf(string customerCode, string siteCode, string formId)
		{
			Byte[] data = null;
			bool bSuccess = false;
			for (int iFailureCnt = 0; !bSuccess && iFailureCnt < RETRIES; iFailureCnt++)
			{
				PrefillDataDownloader downloader = null;
				try
				{
					downloader = new PrefillDataDownloader();
					downloader.Url = m_strDownloadURL;
					data = downloader.GetPdf(customerCode, siteCode, formId);
					bSuccess = true;
				}
				catch (Exception ex)
				{
					string error = string.Format("Get pdf failed for customer code {0}, site code {1}, formId {2}. Retrying...\n",
						customerCode, siteCode, formId);
					m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
				finally
				{
					if (downloader != null)
					{
						downloader.Dispose();
					}
				}
			}

			if (!bSuccess)
			{
				string error = string.Format("Get pdf failed after {0} retries.", RETRIES);
				throw new Exception(error);
			}

			return data;
		}

		private Byte[] GetPdfRendition(string customerCode, string siteCode, int formHistoryTransactionUid)
		{
			Byte[] data = null;
			bool bSuccess = false;
			for (int iFailureCnt = 0; !bSuccess && iFailureCnt < RETRIES; iFailureCnt++)
			{
				PdfRenditionDownloader downloader = null;
				try
				{
					downloader = new PdfRenditionDownloader();
					downloader.Url = m_strDownloadURL;
					data = downloader.GetPdfRendition(customerCode, siteCode, formHistoryTransactionUid);
					bSuccess = true;
				}
				catch (Exception ex)
				{
					string error = string.Format("Get pdf failed for customer code {0}, site code {1}, formHistoryTransactionUid {2}. Retrying...\n",
						customerCode, siteCode, formHistoryTransactionUid);
					m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
				finally
				{
					if (downloader != null)
					{
						downloader.Dispose();
					}
				}
			}

			if (!bSuccess)
			{
				string error = string.Format("Get pdf rendition failed after {0} retries.", RETRIES);
				throw new Exception(error);
			}

			return data;
		}

		private Byte[] GetXfdf(string customerCode, string siteCode, string formInstanceId)
		{
			Byte[] data = null;
			bool bSuccess = false;
			for (int iFailureCnt = 0; !bSuccess && iFailureCnt < RETRIES; iFailureCnt++)
			{
				PrefillDataDownloader downloader = null;
				try
				{
					downloader = new PrefillDataDownloader();
					downloader.Url = m_strDownloadURL;
					data = downloader.GetXfdf(customerCode, siteCode, formInstanceId);
					bSuccess = true;
				}
				catch (Exception ex)
				{
					string error = string.Format("Get xfdf failed for customer code {0}, site code {1}, formInstanceId {2}. Retrying...\n",
						customerCode, siteCode, formInstanceId);
					m_Utils.WriteEventLogEntry(error + ex.Message, EventLogEntryType.Error, EVENT_LOG_SOURCE);
				}
				finally
				{
					if (downloader != null)
					{
						downloader.Dispose();
					}
				}
			}

			if (!bSuccess)
			{
				string error = string.Format("Get xfdf failed after {0} retries.", RETRIES);
				throw new Exception(error);
			}

			return data;
		}

		private void DownloadXfdf(string customerCode, string siteCode, string formInstanceId, string xfdfFullName)
		{
			Byte[] xfdfBytes = GetXfdf(customerCode, siteCode, formInstanceId);

			MemoryStream memoryStream = null;
			try
			{
				memoryStream = new MemoryStream(xfdfBytes);
				XmlDocument xfdf = new XmlDocument();
				xfdf.Load(memoryStream);
				memoryStream.Close();
				memoryStream = null;
				xfdf.Save(xfdfFullName);
			}
			finally
			{
				if (memoryStream != null)
				{
					memoryStream.Close();
				}
			}
		}

		private string GetDownloadPdfRenditonFileName(string traceabilityCode)
		{
			return string.Format("{0}\\{1}.pdf", m_strDownloadDirectory, traceabilityCode);
		}

		private string GetDownloadFileName(string customerCode, string siteCode, string id, string type)
		{
			string fileName = string.Format("{0}\\{1}_{2}_{3}.{4}", m_strDownloadDirectory, customerCode, siteCode, id, type);
			return fileName;
		}

		private void DownloadPdf(string customerCode, string siteCode, string formId, string pdfFullName)
		{
			Byte[] pdfBytes = GetPdf(customerCode, siteCode, formId);

			SaveBytesToFile(pdfBytes, pdfFullName);
		}

		private void DownloadPdfRendition(string customerCode, string siteCode, int formHistoryTransactionUid, string pdfFullName)
		{
			Byte[] pdfBytes = GetPdfRendition(customerCode, siteCode, formHistoryTransactionUid);

			SaveBytesToFile(pdfBytes, pdfFullName);
		}

		private void SaveBytesToFile(Byte[] bytes, string fullName)
		{
			FileStream fs = null;
			BinaryWriter writer = null;
			try
			{
				fs = new FileStream(fullName, FileMode.Create);
				writer = new BinaryWriter(fs);
				writer.Write(bytes);
			}
			finally
			{
				if (writer != null)
				{
					writer.Close();
				}
			}
		}

		/// <summary>
		/// Pdfs are copied to avoid overhead of repeated downloads.
		/// Note: If pdf in the PFW area is up to date, don't copy.
		///		  This can break if Adobe reader has a handle on the PDF.
		/// </summary>
        private void AddPdfToDownloadedFileTable(string pdfFullName, string destfileName, string fileSize)
		{
			if (!m_pdfFolderSpecified)
			{
				//No print folder specified for pdf file. So just download it and leave it.
				return;
			}

			if (!downloadedFileTableToCopy.ContainsKey(pdfFullName))
			{
                string destfilePath = string.Format("{0}\\{1}.pdf", m_strPDFDirectory, destfileName);

				if (!m_Utils.FileUpToDate(destfilePath, fileSize))
				{
					//Don't copy file until we know everything has worked.
					downloadedFileTableToCopy.Add(pdfFullName, destfilePath);
				}
			}
		}

		/// <summary>
		/// Renditions (and xfdf) files are moved. They should only get downloaded once anyway.
		/// </summary>
        private void AddPdfRenditionToDownloadedFileTable(string pdfFullName, string destfileName)
		{
			if (!m_printFolderSpecified)
			{
				//No print folder specified for xfdf or pdr rendition files. So just download it and leave it.
				return;
			}

            string destfilePath = string.Format("{0}\\{1}.pdf", m_strPrintDirectory, destfileName);
		
			//Don't copy file until we know everything has worked.
			downloadedFileTableToMove.Add(pdfFullName, destfilePath);
		}

		/// Xfdf files (and renditions) files are moved. They should only get downloaded once anyway.
        private void AddXfdfToDownloadedFileTable(string xfdfFullName, string destfileName)
		{
			if (!m_printFolderSpecified)
			{
				//No print folder specified for xfdf or pdr rendition files. So just download it and leave it.
				return;
			}

            string destfilePath = string.Format("{0}\\{1}.xfdf", m_strPrintDirectory, destfileName);
            downloadedFileTableToMove.Add(xfdfFullName, destfilePath);
		}

		/// Print manifest files (and renditions) files are moved. They should only get downloaded once anyway.
        private void AddManifestToDownloadedFileTable(XmlNode fileNode, string destfileName)
		{
			if (!m_printManifestFolderSpecified)
			{
				//No print folder specified for xfdf or pdr rendition files. So just download it and leave it.
				return;
			}

            string destfilePath = string.Format("{0}\\{1}.xml", m_strPrintManifestDirectory, destfileName);
            if (!downloadedManifestFileTable.ContainsKey(destfilePath))
            {
                downloadedManifestFileTable.Add(destfilePath, fileNode);
            }
		}

		private void CopyFile(string strSrcPath , string strDestPath )
		{
			FileInfo FileInf = new FileInfo(strSrcPath);
			FileInf.CopyTo(strDestPath, true);
		}

		private void MoveFile(string strSrcPath , string strDestPath )
		{
			if (File.Exists(strDestPath))
			{
				File.Delete(strDestPath);
			}

			FileInfo FileInf = new FileInfo(strSrcPath);
			FileInf.MoveTo(strDestPath);
		}

		private void DownloadPdfRenditionFiles(string customerCode, string siteCode, XmlDocument manifest)
		{
			//Go through each of the files in the manifest. Should only be one.
			XmlNode filesnode = manifest.SelectSingleNode("/serverDownloadInfo/files");
            foreach (System.Xml.XmlNode node in filesnode.ChildNodes) 
			{
                XmlNode pdfRenditionFileNode = node.SelectSingleNode("pdfRenditionFile");
				ProcessPdfRenditionFile(node, pdfRenditionFileNode, customerCode, siteCode);
			}
		}

		private void DownloadPrefilledFiles(string customerCode, string siteCode, XmlDocument manifest)
		{
			//Go through each of the files in the manifest
			XmlNode filesnode = manifest.SelectSingleNode("/serverDownloadInfo/files");
			foreach(System.Xml.XmlNode node in filesnode.ChildNodes) 
			{
				//Must download and copy pdf file before corresponding xfdf.
                XmlNode pdfFileNode = node.SelectSingleNode("pdfFile");
				ProcessPdfFile(node, pdfFileNode, customerCode, siteCode);

				//Now download Xfdf files.
                XmlNode xfdfFileNode = node.SelectSingleNode("xfdfFile");
				ProcessXfdfFile(node, xfdfFileNode, customerCode, siteCode);
			}
		}

        private void ProcessPdfFile(XmlNode fileNode, XmlNode pdfFileNode, string customerCode, string siteCode)
		{
			XmlAttributeCollection Attr = pdfFileNode.Attributes;
			string fileSize = Attr.GetNamedItem("size").Value;
			string formId = Attr.GetNamedItem("formId").Value; 

			string pdfFullName = GetDownloadFileName(customerCode, siteCode, formId, "pdf");
			if (!m_Utils.FileUpToDate(pdfFullName, fileSize))
			{
				DownloadPdf(customerCode, siteCode, formId, pdfFullName);
			}

            string destfileName = formId;
		
			//Copy pdf to print folder watcher pdf folder (later).
            AddPdfToDownloadedFileTable(pdfFullName, destfileName, fileSize);

            //Also dump a copy of this file node to print folder watcher folder manifest folder (later)
            //so that it can be aware of what printer or duplex to use.
            AddManifestToDownloadedFileTable(fileNode, destfileName);

		}

		private void ProcessPdfRenditionFile(XmlNode fileNode, XmlNode pdfFileRenditionNode, string customerCode, string siteCode)
		{
			XmlAttributeCollection Attr = pdfFileRenditionNode.Attributes;
			int formHistoryTransactionUid = Convert.ToInt32(Attr.GetNamedItem("formHistoryTransactionUid").Value); 
			string traceabilityCode = Attr.GetNamedItem("traceabilityCode").Value; 
			string fileSize = Attr.GetNamedItem("size").Value;

			string pdfRenditionFullName = GetDownloadPdfRenditonFileName(traceabilityCode);
			if (!m_Utils.FileUpToDate(pdfRenditionFullName, fileSize))
			{
				DownloadPdfRendition(customerCode, siteCode, formHistoryTransactionUid, pdfRenditionFullName);
			}


            string destfileName = traceabilityCode;

            //Copy pdf rendition to print folder watcher folder (later).
            AddPdfRenditionToDownloadedFileTable(pdfRenditionFullName, destfileName);

			//Also dump a copy of this file node to print folder watcher folder manifest folder (later)
            //so that it can be aware of what printer or duplex to use.
            AddManifestToDownloadedFileTable(fileNode, destfileName);
		}

        private void ProcessXfdfFile(XmlNode fileNode, XmlNode xfdfFileNode, string customerCode, string siteCode)
		{
			XmlAttributeCollection Attr = xfdfFileNode.Attributes;
			string fileSize = Attr.GetNamedItem("size").Value;
			string formInstanceIdValue = Attr.GetNamedItem("FormInstanceID").Value;
			int formInstanceId = Convert.ToInt32(formInstanceIdValue);
            string formInstanceIdZeroPadded = string.Format("{0:D6}", formInstanceId);//Left zero padding ensure filenames sort correctly in PFW.

			string xfdfFullName = GetDownloadFileName(customerCode, siteCode, formInstanceIdZeroPadded, "xfdf");
			if (!m_Utils.FileUpToDate(xfdfFullName, fileSize))
			{
				DownloadXfdf(customerCode, siteCode, formInstanceIdValue, xfdfFullName);
			}

			//Copy xfdf to print folder watcher print folder (later).
            string destfileName = string.Format("{0}_{1}_{2}", customerCode, siteCode, formInstanceIdZeroPadded);
            AddXfdfToDownloadedFileTable(xfdfFullName, destfileName);

            //Also dump a copy of this file node to print folder watcher folder manifest folder (later)
            //so that it can be aware of what printer or duplex to use.
            AddManifestToDownloadedFileTable(fileNode, destfileName);
		}
	}
}
