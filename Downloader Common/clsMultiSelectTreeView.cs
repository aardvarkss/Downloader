using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MultiSelectTreeView {
	/// <summary>
	/// Summary description for MultiSelectTreeView.
	/// The MultiSelectTreeView inherits from System.Windows.Forms.TreeView to 
	/// allow user to select multiple nodes.
	/// The underlying comctl32 TreeView doesn't support multiple selection.
	/// Hence this MultiSelectTreeView listens for the BeforeSelect && AfterSelect
	/// events to dynamically change the BackColor of the individual treenodes to
	/// denote selection. 
	/// It then adds the TreeNode to the internal arraylist of currently
	/// selectedNodes after validation checks.
	/// 
	/// The MultiSelectTreeView supports
	///		1) Select + Control will add the current node to list of SelectedNodes
	///		2) Select + Shift  will add the current node and all the nodes between the two 
	///			(if the start node and end node is at the same level)
	///		3) Control + A when the MultiSelectTreeView has focus will select all Nodes.
	///		
	/// 
	/// </summary>
	public class MultiSelectTreeView : System.Windows.Forms.TreeView {
		/// <summary>
		///  This is private member stores the list of SelectedNodes
		/// </summary>
		private	ArrayList m_alSelectedNodes;

		/// <summary>
		///  This is private member which caches the first treenode user clicked
		/// </summary>
		private TreeNode m_tnFirstNode;

		/// <summary>
		/// The constructor which initialises the MultiSelectTreeView.
		/// </summary>
		public MultiSelectTreeView(){
			m_alSelectedNodes = new ArrayList();
		}

		/// <summary>
		/// The constructor which initialises the MultiSelectTreeView.
		/// </summary>
		[
		Category("Selection"),
		Description("Gets or sets the selected nodes as ArrayList")
		]
		public ArrayList SelectedNodes {
			get{
				return m_alSelectedNodes;
			}
			set{
				DeselectNodes();
				m_alSelectedNodes.Clear();
				m_alSelectedNodes = value;
				SelectNodes();
			}
		}

		#region overrides
		/// <summary>
		///		If the user has pressed "Control+A" keys then select all nodes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyDown (e);
			bool Pressed = (e.Control && ((e.KeyData & Keys.A) == Keys.A));
			if (Pressed){
				m_alSelectedNodes.Clear();
				SelectAllNodes();
			}
		}

		/// <summary>
		///		This Function starts the multiple selection.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnBeforeSelect(TreeViewCancelEventArgs e) {
			TreeNode tnRootNode;
			bool bIsShiftPressed;

			base.OnBeforeSelect(e);

			//Check for the current keys press..	
			bIsShiftPressed = (ModifierKeys==Keys.Shift);

			tnRootNode = e.Node;
			while(tnRootNode.Parent != null){
				tnRootNode = tnRootNode.Parent;
			}
			//If Shift not pressed...
			//Remember this Node to be the Start Node .. in case user presses Shift to select multiple nodes.
			if (!bIsShiftPressed) m_tnFirstNode = tnRootNode;
		}

		/// <summary>
		///		This function ends the multi selection. Also adds and removes the node to
		///		the selectedNodes depending upon the keys prssed.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnAfterSelect(TreeViewEventArgs e) {
			TreeNode tnRootNode;
			bool bIsControlPressed;
			bool bIsShiftPressed;

			base.OnAfterSelect(e);

			//Check for the current keys press..
			bIsControlPressed = (ModifierKeys==Keys.Control);
			bIsShiftPressed = (ModifierKeys==Keys.Shift);

			tnRootNode = e.Node;
			while(tnRootNode.Parent != null){
				tnRootNode = tnRootNode.Parent;
			}
			
			if (bIsControlPressed){
				e.Node.TreeView.SelectedNode = null;
				ControlSelect(tnRootNode, e.Node);
			}else if (bIsShiftPressed){
				ShiftSelect(tnRootNode);
				e.Node.TreeView.SelectedNode = null;
			}else{
				SingleSelect(tnRootNode);
				e.Node.TreeView.SelectedNode = null;
			}
		}

		/// <summary>
		///		Overriden OnLostFocus to mimic TreeView's behavior of de-selecting nodes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLostFocus(EventArgs e) {
			base.OnLostFocus (e);
			DeselectNodes();
		}

		/// <summary>
		///		Overriden OnGotFocus to mimic TreeView's behavior of selecting nodes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus (e);
			SelectNodes();
		}

		#endregion overrides

		/// <summary>
		///		This function provides the user feedback that the node is selected
		///		Basically the BackColor and the ForeColor is changed for all
		///		the nodes in the selectedNodes collection.
		/// </summary>
		private void SelectNodes() {
			foreach ( TreeNode tnCountNode in m_alSelectedNodes ){
				tnCountNode.BackColor = SystemColors.Highlight;
				tnCountNode.ForeColor = SystemColors.HighlightText;
			}
		}

		/// <summary>
		///		This function provides the user feedback that the node is de-selected
		///		Basically the BackColor and the ForeColor is changed for all
		///		the nodes in the selectedNodes collection.
		/// </summary>
		private void DeselectNodes() {
			if (m_alSelectedNodes.Count==0) return;

			TreeNode tnNode = (TreeNode) m_alSelectedNodes[0];
			Color cBackColor = tnNode.TreeView.BackColor;
			Color cForeColor= tnNode.TreeView.ForeColor;

			foreach ( TreeNode tnCountNode in m_alSelectedNodes ){
				tnCountNode.BackColor = cBackColor;
				tnCountNode.ForeColor = cForeColor;
			}
		}

		/// <summary>
		///		This function selects all the Nodes in the MultiSelectTreeView..
		/// </summary>
		private void SelectAllNodes() {
			foreach (TreeNode tnCountNode in this.Nodes){
				if(tnCountNode.Parent == null){
					m_alSelectedNodes.Add(tnCountNode);
				}
			}
			SelectNodes();
		}

		private void ShiftSelect(TreeNode tnRootNode){
			TreeNode tnUppernode = m_tnFirstNode;
			TreeNode tnBottomnode = tnRootNode;
			TreeNode tnTemp = tnUppernode;
			int nIndexUpper = tnUppernode.Index;
			int nIndexBottom = tnBottomnode.Index;

			//Need to SWAP if the order is reversed...
			if (nIndexBottom < nIndexUpper){
				tnTemp = tnUppernode;
				tnUppernode = tnBottomnode;
				tnBottomnode = tnTemp;
				nIndexUpper = tnUppernode.Index;
				nIndexBottom = tnBottomnode.Index;
			}
			tnTemp = tnUppernode;
			DeselectNodes();
			m_alSelectedNodes.Clear();
			while (nIndexUpper <= nIndexBottom){
				//Add all the nodes if nodes not present in the current SelectedNodes list...
				if (!m_alSelectedNodes.Contains( tnTemp )){
					m_alSelectedNodes.Add(tnTemp);
				}
				tnTemp = tnTemp.NextNode;
				nIndexUpper++;
			}
			//Add the Last Node.
			SelectNodes();
		}
	
		private void ControlSelect(TreeNode tnRootNode, TreeNode tnOriginal){
			if (!m_alSelectedNodes.Contains(tnRootNode)){
				//This is a new Node, so add it to the list.
				m_alSelectedNodes.Add(tnRootNode);
			}else{
				//If control is pressed and the selectedNodes contains current Node Deselect that node...
				//Remove from the selectedNodes Collection...
				if(tnOriginal == tnRootNode){
					DeselectNodes();
					m_alSelectedNodes.Remove(tnRootNode);
				}
			}	
			SelectNodes();
		}

		private void SingleSelect(TreeNode tnRootNode){
			// If Normal selection then add this to SelectedNodes Collection.
			if (m_alSelectedNodes!=null && m_alSelectedNodes.Count>0){
				DeselectNodes();
				m_alSelectedNodes.Clear();
			}

			m_alSelectedNodes.Add(tnRootNode);
			//e.Node.TreeView.SelectedNode = tnRootNode;
			SelectNodes();
		}
	}
}