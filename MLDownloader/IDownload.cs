using System;
using System.Runtime.InteropServices;

namespace MLDownloader
{
	[ComVisible(true),Guid("0E4B2351-DCEE-4e86-8ADF-61C6E48AE04E")]
	public interface IDownload
	{
		int Download([In]int manifestNumber);
	}
}
