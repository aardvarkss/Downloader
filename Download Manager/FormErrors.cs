using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using Downloader_Common;

namespace Download_Manager
{
	/// <summary>
	/// Summary description for FormErrors.
	/// </summary>
	public class FormErrors : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TreeView treeViewErrors;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FormErrors(string source)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//Make sure event log exists.
			clsUtils utils = new clsUtils();
			utils.CreateEventLog(source);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormErrors));
			this.treeViewErrors = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// treeViewErrors
			// 
			this.treeViewErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.treeViewErrors.ImageIndex = -1;
			this.treeViewErrors.Location = new System.Drawing.Point(8, 8);
			this.treeViewErrors.Name = "treeViewErrors";
			this.treeViewErrors.SelectedImageIndex = -1;
			this.treeViewErrors.Size = new System.Drawing.Size(328, 256);
			this.treeViewErrors.TabIndex = 0;
			// 
			// FormErrors
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(344, 270);
			this.Controls.Add(this.treeViewErrors);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormErrors";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Download Errors";
			this.Load += new System.EventHandler(this.FormErrors_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormErrors_Load(object sender, System.EventArgs e) {
			EventLog elmeticulusLog = null;
			string strDelimiters = "\n";
			string [] strSplit = null;
			char [] caDelimiters = strDelimiters.ToCharArray();

			try
			{
				this.treeViewErrors.Nodes.Clear();
				elmeticulusLog = new EventLog();
				elmeticulusLog.Source = "Downloader Service";
				foreach( EventLogEntry eleEntry in elmeticulusLog.Entries )
				{
					if ((eleEntry.EntryType == EventLogEntryType.Error) && eleEntry.Source == "Downloader Service" && ((string.Compare(eleEntry.Category, "(2)") == 0) || (string.Compare(eleEntry.Category, "(1)") ==0)))
					{
						strSplit = eleEntry.Message.Split(caDelimiters, 3);
						treeViewErrors.Nodes.Insert(0, new TreeNode(strSplit[0]));
						treeViewErrors.Nodes[0].Nodes.Add("Date: " + Convert.ToString(eleEntry.TimeGenerated));
						for(int iCounter=1; iCounter < strSplit.Length; iCounter++)
						{
							if (string.Compare(eleEntry.Category, "(2)") == 0)
							{
								treeViewErrors.Nodes[0].Nodes.Add(strSplit[iCounter].Substring(1));
							}
							else if (string.Compare(eleEntry.Category, "(1)") == 0)
							{
								treeViewErrors.Nodes[0].Nodes.Add(strSplit[iCounter]);
							}
						}
					}
				}
			}
			finally
			{
				if (elmeticulusLog != null)
				{
					elmeticulusLog.Close();
				}
			}
		}
	}
}
