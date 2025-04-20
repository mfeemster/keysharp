namespace Keyview
{
	partial class Keyview
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Keyview));
			FileName = new System.Windows.Forms.Label();
			menuStrip1 = new System.Windows.Forms.MenuStrip();
			fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			selectLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			clearSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			indentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			outdentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			uppercaseSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			lowercaseSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			wordWrapItem = new System.Windows.Forms.ToolStripMenuItem();
			indentGuidesItem = new System.Windows.Forms.ToolStripMenuItem();
			hiddenCharactersItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			zoomInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			zoomOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			zoom100ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			PanelSearch = new System.Windows.Forms.Panel();
			BtnNextSearch = new System.Windows.Forms.Button();
			BtnPrevSearch = new System.Windows.Forms.Button();
			BtnCloseSearch = new System.Windows.Forms.Button();
			TxtSearch = new System.Windows.Forms.TextBox();
			openFileDialog = new System.Windows.Forms.OpenFileDialog();
			splitContainer = new System.Windows.Forms.SplitContainer();
			txtIn = new Scintilla();
			txtOut = new Scintilla();
			toolStrip1 = new System.Windows.Forms.ToolStrip();
			tslCodeStatus = new System.Windows.Forms.ToolStripLabel();
			tssCode = new System.Windows.Forms.ToolStripSeparator();
			tslCodeCompile = new System.Windows.Forms.ToolStripLabel();
			menuStrip1.SuspendLayout();
			PanelSearch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			toolStrip1.SuspendLayout();
			SuspendLayout();
			//
			// FileName
			//
			FileName.Location = new System.Drawing.Point(0, 0);
			FileName.Name = "FileName";
			FileName.Size = new System.Drawing.Size(100, 23);
			FileName.TabIndex = 0;
			//
			// menuStrip1
			//
			menuStrip1.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
			menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, searchToolStripMenuItem, viewToolStripMenuItem });
			menuStrip1.Location = new System.Drawing.Point(0, 0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
			menuStrip1.Size = new System.Drawing.Size(1680, 24);
			menuStrip1.TabIndex = 5;
			menuStrip1.Text = "menuStrip1";
			//
			// fileToolStripMenuItem
			//
			fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
			fileToolStripMenuItem.Text = "File";
			//
			// openToolStripMenuItem
			//
			openToolStripMenuItem.Name = "openToolStripMenuItem";
			openToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
			openToolStripMenuItem.Text = "Open...";
			openToolStripMenuItem.Click += openToolStripMenuItem_Click;
			//
			// editToolStripMenuItem
			//
			editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, toolStripSeparator1, selectLineToolStripMenuItem, selectAllToolStripMenuItem, clearSelectionToolStripMenuItem, toolStripSeparator2, indentSelectionToolStripMenuItem, outdentSelectionToolStripMenuItem, toolStripSeparator3, uppercaseSelectionToolStripMenuItem, lowercaseSelectionToolStripMenuItem });
			editToolStripMenuItem.Name = "editToolStripMenuItem";
			editToolStripMenuItem.Size = new System.Drawing.Size(39, 22);
			editToolStripMenuItem.Text = "Edit";
			//
			// cutToolStripMenuItem
			//
			cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			cutToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+X";
			cutToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			cutToolStripMenuItem.Text = "Cut";
			cutToolStripMenuItem.Click += cutToolStripMenuItem_Click;
			//
			// copyToolStripMenuItem
			//
			copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			copyToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
			copyToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			copyToolStripMenuItem.Text = "Copy";
			copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
			//
			// pasteToolStripMenuItem
			//
			pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			pasteToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+V";
			pasteToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			pasteToolStripMenuItem.Text = "Paste";
			pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
			//
			// toolStripSeparator1
			//
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(172, 6);
			//
			// selectLineToolStripMenuItem
			//
			selectLineToolStripMenuItem.Name = "selectLineToolStripMenuItem";
			selectLineToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			selectLineToolStripMenuItem.Text = "Select Line";
			selectLineToolStripMenuItem.Click += selectLineToolStripMenuItem_Click;
			//
			// selectAllToolStripMenuItem
			//
			selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			selectAllToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+A";
			selectAllToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			selectAllToolStripMenuItem.Text = "Select All";
			selectAllToolStripMenuItem.Click += selectAllToolStripMenuItem_Click;
			//
			// clearSelectionToolStripMenuItem
			//
			clearSelectionToolStripMenuItem.Name = "clearSelectionToolStripMenuItem";
			clearSelectionToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			clearSelectionToolStripMenuItem.Text = "Clear Selection";
			clearSelectionToolStripMenuItem.Click += clearSelectionToolStripMenuItem_Click;
			//
			// toolStripSeparator2
			//
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(172, 6);
			//
			// indentSelectionToolStripMenuItem
			//
			indentSelectionToolStripMenuItem.Name = "indentSelectionToolStripMenuItem";
			indentSelectionToolStripMenuItem.ShortcutKeyDisplayString = "Tab";
			indentSelectionToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			indentSelectionToolStripMenuItem.Text = "Indent";
			indentSelectionToolStripMenuItem.Click += indentSelectionToolStripMenuItem_Click;
			//
			// outdentSelectionToolStripMenuItem
			//
			outdentSelectionToolStripMenuItem.Name = "outdentSelectionToolStripMenuItem";
			outdentSelectionToolStripMenuItem.ShortcutKeyDisplayString = "Shift+Tab";
			outdentSelectionToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			outdentSelectionToolStripMenuItem.Text = "Outdent";
			outdentSelectionToolStripMenuItem.Click += outdentSelectionToolStripMenuItem_Click;
			//
			// toolStripSeparator3
			//
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new System.Drawing.Size(172, 6);
			//
			// uppercaseSelectionToolStripMenuItem
			//
			uppercaseSelectionToolStripMenuItem.Name = "uppercaseSelectionToolStripMenuItem";
			uppercaseSelectionToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+U";
			uppercaseSelectionToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			uppercaseSelectionToolStripMenuItem.Text = "Uppercase";
			uppercaseSelectionToolStripMenuItem.Click += uppercaseSelectionToolStripMenuItem_Click;
			//
			// lowercaseSelectionToolStripMenuItem
			//
			lowercaseSelectionToolStripMenuItem.Name = "lowercaseSelectionToolStripMenuItem";
			lowercaseSelectionToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+L";
			lowercaseSelectionToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			lowercaseSelectionToolStripMenuItem.Text = "Lowercase";
			lowercaseSelectionToolStripMenuItem.Click += lowercaseSelectionToolStripMenuItem_Click;
			//
			// searchToolStripMenuItem
			//
			searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { findToolStripMenuItem });
			searchToolStripMenuItem.Name = "searchToolStripMenuItem";
			searchToolStripMenuItem.Size = new System.Drawing.Size(54, 22);
			searchToolStripMenuItem.Text = "Search";
			//
			// findToolStripMenuItem
			//
			findToolStripMenuItem.Name = "findToolStripMenuItem";
			findToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F";
			findToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			findToolStripMenuItem.Text = "Quick Find...";
			findToolStripMenuItem.Click += findToolStripMenuItem_Click;
			//
			// viewToolStripMenuItem
			//
			viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { wordWrapItem, indentGuidesItem, hiddenCharactersItem, toolStripSeparator4, zoomInToolStripMenuItem, zoomOutToolStripMenuItem, zoom100ToolStripMenuItem, toolStripSeparator5, collapseAllToolStripMenuItem, expandAllToolStripMenuItem });
			viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			viewToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
			viewToolStripMenuItem.Text = "View";
			//
			// wordWrapItem
			//
			wordWrapItem.Checked = true;
			wordWrapItem.CheckState = System.Windows.Forms.CheckState.Checked;
			wordWrapItem.Name = "wordWrapItem";
			wordWrapItem.Size = new System.Drawing.Size(196, 22);
			wordWrapItem.Text = "Word Wrap";
			wordWrapItem.Click += wordWrapToolStripMenuItem1_Click;
			//
			// indentGuidesItem
			//
			indentGuidesItem.Checked = true;
			indentGuidesItem.CheckState = System.Windows.Forms.CheckState.Checked;
			indentGuidesItem.Name = "indentGuidesItem";
			indentGuidesItem.Size = new System.Drawing.Size(196, 22);
			indentGuidesItem.Text = "Show Indent Guides";
			indentGuidesItem.Click += indentGuidesToolStripMenuItem_Click;
			//
			// hiddenCharactersItem
			//
			hiddenCharactersItem.Name = "hiddenCharactersItem";
			hiddenCharactersItem.Size = new System.Drawing.Size(196, 22);
			hiddenCharactersItem.Text = "Show Whitespace";
			hiddenCharactersItem.Click += hiddenCharactersToolStripMenuItem_Click;
			//
			// toolStripSeparator4
			//
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new System.Drawing.Size(193, 6);
			//
			// zoomInToolStripMenuItem
			//
			zoomInToolStripMenuItem.Name = "zoomInToolStripMenuItem";
			zoomInToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Plus";
			zoomInToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			zoomInToolStripMenuItem.Text = "Zoom In";
			zoomInToolStripMenuItem.Click += zoomInToolStripMenuItem_Click;
			//
			// zoomOutToolStripMenuItem
			//
			zoomOutToolStripMenuItem.Name = "zoomOutToolStripMenuItem";
			zoomOutToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Minus";
			zoomOutToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			zoomOutToolStripMenuItem.Text = "Zoom Out";
			zoomOutToolStripMenuItem.Click += zoomOutToolStripMenuItem_Click;
			//
			// zoom100ToolStripMenuItem
			//
			zoom100ToolStripMenuItem.Name = "zoom100ToolStripMenuItem";
			zoom100ToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+0";
			zoom100ToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			zoom100ToolStripMenuItem.Text = "Zoom 100%";
			zoom100ToolStripMenuItem.Click += zoom100ToolStripMenuItem_Click;
			//
			// toolStripSeparator5
			//
			toolStripSeparator5.Name = "toolStripSeparator5";
			toolStripSeparator5.Size = new System.Drawing.Size(193, 6);
			//
			// collapseAllToolStripMenuItem
			//
			collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
			collapseAllToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			collapseAllToolStripMenuItem.Text = "Collapse All";
			collapseAllToolStripMenuItem.Click += collapseAllToolStripMenuItem_Click;
			//
			// expandAllToolStripMenuItem
			//
			expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
			expandAllToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			expandAllToolStripMenuItem.Text = "Expand All";
			expandAllToolStripMenuItem.Click += expandAllToolStripMenuItem_Click;
			//
			// toolStripSeparator7
			//
			toolStripSeparator7.Name = "toolStripSeparator7";
			toolStripSeparator7.Size = new System.Drawing.Size(280, 6);
			//
			// PanelSearch
			//
			PanelSearch.BackColor = System.Drawing.Color.White;
			PanelSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			PanelSearch.Controls.Add(BtnNextSearch);
			PanelSearch.Controls.Add(BtnPrevSearch);
			PanelSearch.Controls.Add(BtnCloseSearch);
			PanelSearch.Controls.Add(TxtSearch);
			PanelSearch.Dock = System.Windows.Forms.DockStyle.Top;
			PanelSearch.Location = new System.Drawing.Point(0, 24);
			PanelSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			PanelSearch.Name = "PanelSearch";
			PanelSearch.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			PanelSearch.Size = new System.Drawing.Size(1680, 24);
			PanelSearch.TabIndex = 10;
			PanelSearch.Visible = false;
			//
			// BtnNextSearch
			//
			BtnNextSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			BtnNextSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			BtnNextSearch.ForeColor = System.Drawing.Color.White;
			BtnNextSearch.Image = (System.Drawing.Image)resources.GetObject("BtnNextSearch.Image");
			BtnNextSearch.Location = new System.Drawing.Point(1645, 2);
			BtnNextSearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			BtnNextSearch.Name = "BtnNextSearch";
			BtnNextSearch.Size = new System.Drawing.Size(15, 15);
			BtnNextSearch.TabIndex = 9;
			BtnNextSearch.Tag = "Find next (Enter)";
			BtnNextSearch.UseVisualStyleBackColor = true;
			BtnNextSearch.Click += BtnNextSearch_Click;
			//
			// BtnPrevSearch
			//
			BtnPrevSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			BtnPrevSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			BtnPrevSearch.ForeColor = System.Drawing.Color.White;
			BtnPrevSearch.Image = (System.Drawing.Image)resources.GetObject("BtnPrevSearch.Image");
			BtnPrevSearch.Location = new System.Drawing.Point(1629, 2);
			BtnPrevSearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			BtnPrevSearch.Name = "BtnPrevSearch";
			BtnPrevSearch.Size = new System.Drawing.Size(15, 15);
			BtnPrevSearch.TabIndex = 8;
			BtnPrevSearch.Tag = "Find previous (Shift+Enter)";
			BtnPrevSearch.UseVisualStyleBackColor = true;
			BtnPrevSearch.Click += BtnPrevSearch_Click;
			//
			// BtnCloseSearch
			//
			BtnCloseSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			BtnCloseSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			BtnCloseSearch.ForeColor = System.Drawing.Color.White;
			BtnCloseSearch.Image = (System.Drawing.Image)resources.GetObject("BtnCloseSearch.Image");
			BtnCloseSearch.Location = new System.Drawing.Point(1661, 2);
			BtnCloseSearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			BtnCloseSearch.Name = "BtnCloseSearch";
			BtnCloseSearch.Size = new System.Drawing.Size(15, 15);
			BtnCloseSearch.TabIndex = 7;
			BtnCloseSearch.Tag = "Close (Esc)";
			BtnCloseSearch.UseVisualStyleBackColor = true;
			BtnCloseSearch.Click += BtnClearSearch_Click;
			//
			// TxtSearch
			//
			TxtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
			TxtSearch.Dock = System.Windows.Forms.DockStyle.Top;
			TxtSearch.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			TxtSearch.Location = new System.Drawing.Point(3, 2);
			TxtSearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			TxtSearch.Name = "TxtSearch";
			TxtSearch.Size = new System.Drawing.Size(1672, 16);
			TxtSearch.TabIndex = 6;
			TxtSearch.TextChanged += TxtSearch_TextChanged;
			TxtSearch.KeyDown += TxtSearch_KeyDown;
			//
			// openFileDialog
			//
			openFileDialog.DefaultExt = "txt";
			openFileDialog.FileName = "New File";
			openFileDialog.Filter = "All files|*.*";
			//
			// splitContainer
			//
			splitContainer.Cursor = System.Windows.Forms.Cursors.VSplit;
			splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer.Location = new System.Drawing.Point(0, 48);
			splitContainer.Margin = new System.Windows.Forms.Padding(0);
			splitContainer.Name = "splitContainer";
			//
			// splitContainer.Panel1
			//
			splitContainer.Panel1.Controls.Add(txtIn);
			splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			//
			// splitContainer.Panel2
			//
			splitContainer.Panel2.Controls.Add(txtOut);
			splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			splitContainer.Size = new System.Drawing.Size(1680, 412);
			splitContainer.SplitterDistance = 860;
			splitContainer.SplitterWidth = 2;
			splitContainer.TabIndex = 0;
			splitContainer.DoubleClick += splitContainer_DoubleClick;
			//
			// txtIn
			//
			txtIn.AutoCMaxHeight = 9;
			txtIn.BiDirectionality = BiDirectionalDisplayType.Disabled;
			txtIn.CaretLineBackColor = System.Drawing.Color.AliceBlue;
			txtIn.Dock = System.Windows.Forms.DockStyle.Fill;
			txtIn.Font = new System.Drawing.Font("Consolas", 10F);
			txtIn.LexerName = null;
			txtIn.Location = new System.Drawing.Point(1, 0);
			txtIn.Margin = new System.Windows.Forms.Padding(0);
			txtIn.Name = "txtIn";
			txtIn.ScrollWidth = 1;
			txtIn.Size = new System.Drawing.Size(858, 412);
			txtIn.TabIndents = true;
			txtIn.TabIndex = 1;
			txtIn.UseRightToLeftReadingLayout = false;
			txtIn.WrapMode = WrapMode.Word;
			txtIn.TextChanged += txtIn_TextChanged;
			txtIn.KeyDown += txtIn_KeyDown;
			//
			// txtOut
			//
			txtOut.AutoCMaxHeight = 9;
			txtOut.BiDirectionality = BiDirectionalDisplayType.Disabled;
			txtOut.CaretLineBackColor = System.Drawing.Color.AliceBlue;
			txtOut.Dock = System.Windows.Forms.DockStyle.Fill;
			txtOut.Font = new System.Drawing.Font("Consolas", 10F);
			txtOut.LexerName = null;
			txtOut.Location = new System.Drawing.Point(1, 0);
			txtOut.Margin = new System.Windows.Forms.Padding(0);
			txtOut.Name = "txtOut";
			txtOut.ReadOnly = true;
			txtOut.ScrollWidth = 1;
			txtOut.Size = new System.Drawing.Size(816, 412);
			txtOut.TabIndents = true;
			txtOut.TabIndex = 2;
			txtOut.WrapMode = WrapMode.Word;
			txtOut.UseRightToLeftReadingLayout = false;
			txtOut.KeyDown += txtOut_KeyDown;
			//
			// toolStrip1
			//
			toolStrip1.AutoSize = false;
			toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
			toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStrip1.Margin = new System.Windows.Forms.Padding(15);
			toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tslCodeStatus, tssCode, tslCodeCompile });
			toolStrip1.Location = new System.Drawing.Point(0, 460);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new System.Drawing.Size(1680, 30);
			toolStrip1.TabIndex = 10;
			//
			// tslCodeStatus
			//
			tslCodeStatus.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			tslCodeStatus.AutoSize = false;
			tslCodeStatus.Name = "tslCodeStatus";
			tslCodeStatus.Size = new System.Drawing.Size(300, 39);
			tslCodeStatus.Text = "------------------------";
			//
			// tssCode
			//
			tssCode.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			tssCode.Name = "tssCode";
			tssCode.Size = new System.Drawing.Size(6, 22);
			//
			// tslCodeCompile
			//
			tslCodeCompile.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			tslCodeCompile.Name = "tslCodeCompile";
			tslCodeCompile.Size = new System.Drawing.Size(84, 19);
			tslCodeCompile.Text = "Code compile:";
			//
			// Keyview
			//
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.White;
			ClientSize = new System.Drawing.Size(1680, 482);
			Controls.Add(splitContainer);
			Controls.Add(PanelSearch);
			Controls.Add(toolStrip1);
			Controls.Add(menuStrip1);
			Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			MainMenuStrip = menuStrip1;
			Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			Name = "Keyview";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Keyview";
			WindowState = System.Windows.Forms.FormWindowState.Maximized;
			FormClosing += Keyview_FormClosing;
			Load += Keyview_Load;
			ResizeEnd += Keyview_ResizeEnd;
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			PanelSearch.ResumeLayout(false);
			PanelSearch.PerformLayout();
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Label FileName;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem wordWrapItem;
		private System.Windows.Forms.ToolStripMenuItem hiddenCharactersItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem zoomInToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoomOutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom100ToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem indentSelectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem outdentSelectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem uppercaseSelectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem lowercaseSelectionToolStripMenuItem;
		private System.Windows.Forms.Panel PanelSearch;
		private System.Windows.Forms.Button BtnNextSearch;
		private System.Windows.Forms.Button BtnPrevSearch;
		private System.Windows.Forms.Button BtnCloseSearch;
		private System.Windows.Forms.TextBox TxtSearch;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ToolStripMenuItem selectLineToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearSelectionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
		//private System.Windows.Forms.ToolStripMenuItem goToLineToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripMenuItem indentGuidesItem;
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel tslCodeCompile;
		private System.Windows.Forms.ToolStripSeparator tssCode;
		private System.Windows.Forms.ToolStripLabel tslCodeStatus;
		private ScintillaNET.Scintilla txtIn;
		private ScintillaNET.Scintilla txtOut;
	}
}

