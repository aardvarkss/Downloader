using System;
using System.Runtime.InteropServices;

using Downloader_Common;

namespace MLDownloader
{
	[ClassInterface(ClassInterfaceType.None),ComVisible(true),Guid("127EB1BE-0EC2-4209-8000-40D0D895FF52")]
	public class Downloader : IDownload
	{
		private string url;
		private string customerCode;
		private string siteCode;

		public Downloader()
		{
		}

		private void Initialise()
		{
			//TODO: replace with Registry class from Utils (or Shared) project
			clsUtils commonUtils = new clsUtils();
			customerCode = commonUtils.ReadCustomerCode();
			siteCode = commonUtils.ReadSiteCode();
			url = commonUtils.ReadDownloadURL();
		}

		#region IDownload Members

		public int Download(int manifestNumber)
		{
			Initialise();

			clsDownload.enStatusType enStatus = clsDownload.enStatusType.Failure;
			try
			{
				clsDownload commonDownloader = new clsDownload();
				enStatus = commonDownloader.DownloadSpecificPrefilledData(url, customerCode, siteCode, manifestNumber);
			}
			catch (Exception ex)
			{
				//TODO: Log using Logger class.
				throw ex;
			}
			return (int)enStatus;
		}

		#endregion
	}
}
