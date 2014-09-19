using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Downloader_Common;

namespace Download_Manager
{
	/// <summary>
	/// Summary description for FormView.
	/// </summary>
	public class FormView : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ColumnHeader columnFiles;
		private System.Windows.Forms.ListView listViewFiles;
		private System.ComponentModel.IContainer components;
		protected string m_strDownloadDirectory;
		protected string m_strCustomerCode;
		protected string m_strSiteCode;
		protected string m_strManifestNumber;
		private System.Windows.Forms.ImageList imageListStatus;
		protected clsUtils m_Utils;
		protected enum enStatus {Downloaded,Downloading};
		public FormView()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			m_Utils = new clsUtils();
			m_strDownloadDirectory = m_Utils.ReadDownloadDirectory();
			m_strCustomerCode = m_Utils.ReadCustomerCode();
			m_strSiteCode = m_Utils.ReadSiteCode();
			m_strManifestNumber = m_Utils.ReadManifestNumber();
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormView));
			this.listViewFiles = new System.Windows.Forms.ListView();
			this.columnFiles = new System.Windows.Forms.ColumnHeader();
			this.imageListStatus = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// listViewFiles
			// 
			this.listViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnFiles});
			this.listViewFiles.Location = new System.Drawing.Point(8, 8);
			this.listViewFiles.Name = "listViewFiles";
			this.listViewFiles.Size = new System.Drawing.Size(328, 256);
			this.listViewFiles.TabIndex = 0;
			// 
			// columnFiles
			// 
			this.columnFiles.Text = "Files";
			this.columnFiles.Width = 120;
			// 
			// imageListStatus
			// 
			this.imageListStatus.ImageSize = new System.Drawing.Size(16, 16);
			this.imageListStatus.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListStatus.ImageStream")));
			this.imageListStatus.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// FormView
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(344, 273);
			this.Controls.Add(this.listViewFiles);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormView";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Download Status";
			this.Load += new System.EventHandler(this.FormView_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private int FindFileItemIndex( ListView.ListViewItemCollection ItemCol,string filename){
			
			foreach (ListViewItem itemFile in ItemCol){
				if (itemFile.Text == filename) return itemFile.Index;
			}
			return -1;
		}

		private void AddFileToListItems(string strFileSize,string strFileId,string strExt){
			string strFileName;
			int intIndex;

			strFileName = string.Format("{0}\\{1}_{2}_{3}{4}", m_strDownloadDirectory,m_strCustomerCode, m_strSiteCode, strFileId,strExt);
			intIndex = FindFileItemIndex(listViewFiles.Items,strFileName);

			if (m_Utils.FileUpToDate(strFileName,strFileSize)){
				if (intIndex != -1){
					listViewFiles.Items[intIndex].ImageIndex = (int)enStatus.Downloaded;
				}else{
					listViewFiles.Items.Add(strFileName,(int)enStatus.Downloaded);
				}
			}else{
				if (intIndex != -1){
					listViewFiles.Items[intIndex].ImageIndex = (int)enStatus.Downloading;
				}else{
					listViewFiles.Items.Add(strFileName,(int)enStatus.Downloading);
				}
			}
		}
		private void FormView_Load(object sender, System.EventArgs e) 
		{
			string strFileName;
			XmlAttributeCollection Attr;

			listViewFiles.Clear();
			listViewFiles.View = View.List;
			listViewFiles.Scrollable = true;
			listViewFiles.Sorting = SortOrder.Ascending;
			listViewFiles.SmallImageList = imageListStatus;
			//first add files from manifest file
			try{
				strFileName = string.Format("{0}\\{1}_{2}_{3}_manifest.xml", m_strDownloadDirectory,m_strCustomerCode, m_strSiteCode, m_strManifestNumber);
				XmlDocument manifest = new XmlDocument();
				XmlTextReader reader = new XmlTextReader(strFileName);
				manifest.Load(reader);
				XmlNode filesnode = manifest.SelectSingleNode("/serverDownloadInfo/files");
				foreach(System.Xml.XmlNode XmlFiles in filesnode.ChildNodes) {
					XmlNode PDFFile = XmlFiles.SelectSingleNode("pdfFile");
					Attr = PDFFile.Attributes;
					AddFileToListItems(Attr.GetNamedItem("size").Value,Attr.GetNamedItem("formId").Value,".pdf");
					XmlNode XfdfFile = XmlFiles.SelectSingleNode("xfdfFile");
					Attr = XfdfFile.Attributes;
					AddFileToListItems(Attr.GetNamedItem("size").Value,Attr.GetNamedItem("FormInstanceID").Value,".xfdf");
				}
			}catch(Exception){
				MessageBox.Show("No download results are available to show.","Download Manager");
				this.Close();
			}
		}
	}
}
