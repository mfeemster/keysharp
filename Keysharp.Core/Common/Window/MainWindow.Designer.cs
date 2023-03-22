namespace Keysharp.Scripting
{
	partial class MainWindow
	{
		/// <summary>
		/// Required designer variable.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtStack = new System.Windows.Forms.TextBox();
			this.mainMenu = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.reloadScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.windowSpyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.suspendHotkeysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.variablesAndTheirContentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.hotkeysAndTheirMethodsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.keyHistoryAndScriptInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.websiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tcMain = new System.Windows.Forms.TabControl();
			this.tpStack = new System.Windows.Forms.TabPage();
			this.tpDebug = new System.Windows.Forms.TabPage();
			this.txtDebug = new System.Windows.Forms.TextBox();
			this.tpVars = new System.Windows.Forms.TabPage();
			this.txtVars = new System.Windows.Forms.TextBox();
			this.tpHotkeys = new System.Windows.Forms.TabPage();
			this.txtHotkeys = new System.Windows.Forms.TextBox();
			this.tpHistory = new System.Windows.Forms.TabPage();
			this.txtHistory = new System.Windows.Forms.TextBox();
			this.mainMenu.SuspendLayout();
			this.tcMain.SuspendLayout();
			this.tpStack.SuspendLayout();
			this.tpDebug.SuspendLayout();
			this.tpVars.SuspendLayout();
			this.tpHotkeys.SuspendLayout();
			this.tpHistory.SuspendLayout();
			this.SuspendLayout();
			//
			// txtStack
			//
			this.txtStack.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtStack.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtStack.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtStack.Location = new System.Drawing.Point(2, 2);
			this.txtStack.Margin = new System.Windows.Forms.Padding(2);
			this.txtStack.Multiline = true;
			this.txtStack.Name = "txtStack";
			this.txtStack.ReadOnly = true;
			this.txtStack.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtStack.Size = new System.Drawing.Size(831, 479);
			this.txtStack.TabIndex = 0;
			//
			// mainMenu
			//
			this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.fileToolStripMenuItem,
				this.viewToolStripMenuItem,
				this.helpToolStripMenuItem
			});
			this.mainMenu.Location = new System.Drawing.Point(0, 0);
			this.mainMenu.Name = "mainMenu";
			this.mainMenu.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
			this.mainMenu.Size = new System.Drawing.Size(843, 24);
			this.mainMenu.TabIndex = 2;
			this.mainMenu.Text = "MainMenu";
			//
			// fileToolStripMenuItem
			//
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.reloadScriptToolStripMenuItem,
				this.editScriptToolStripMenuItem,
				this.windowSpyToolStripMenuItem,
				this.toolStripSeparator1,
				this.suspendHotkeysToolStripMenuItem,
				this.exitToolStripMenuItem
			});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
			this.fileToolStripMenuItem.Text = "&File";
			//
			// reloadScriptToolStripMenuItem
			//
			this.reloadScriptToolStripMenuItem.Name = "reloadScriptToolStripMenuItem";
			this.reloadScriptToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.reloadScriptToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.reloadScriptToolStripMenuItem.Text = "&Reload Script";
			this.reloadScriptToolStripMenuItem.Click += new System.EventHandler(this.reloadScriptToolStripMenuItem_Click);
			//
			// editScriptToolStripMenuItem
			//
			this.editScriptToolStripMenuItem.Name = "editScriptToolStripMenuItem";
			this.editScriptToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
			this.editScriptToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.editScriptToolStripMenuItem.Text = "&Edit Script";
			this.editScriptToolStripMenuItem.Click += new System.EventHandler(this.editScriptToolStripMenuItem_Click);
			//
			// windowSpyToolStripMenuItem
			//
			this.windowSpyToolStripMenuItem.Name = "windowSpyToolStripMenuItem";
			this.windowSpyToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.windowSpyToolStripMenuItem.Text = "&Window Spy";
			this.windowSpyToolStripMenuItem.Click += new System.EventHandler(this.windowSpyToolStripMenuItem_Click);
			//
			// toolStripSeparator1
			//
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
			//
			// suspendHotkeysToolStripMenuItem
			//
			this.suspendHotkeysToolStripMenuItem.Name = "suspendHotkeysToolStripMenuItem";
			this.suspendHotkeysToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.suspendHotkeysToolStripMenuItem.Text = "&Suspend Hotkeys";
			this.suspendHotkeysToolStripMenuItem.Click += new System.EventHandler(this.suspendHotkeysToolStripMenuItem_Click);
			//
			// exitToolStripMenuItem
			//
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			//
			// viewToolStripMenuItem
			//
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.variablesAndTheirContentsToolStripMenuItem,
				this.hotkeysAndTheirMethodsToolStripMenuItem,
				this.keyHistoryAndScriptInfoToolStripMenuItem,
				this.toolStripSeparator2,
				this.refreshToolStripMenuItem
			});
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
			this.viewToolStripMenuItem.Text = "&View";
			//
			// variablesAndTheirContentsToolStripMenuItem
			//
			this.variablesAndTheirContentsToolStripMenuItem.Name = "variablesAndTheirContentsToolStripMenuItem";
			this.variablesAndTheirContentsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.variablesAndTheirContentsToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
			this.variablesAndTheirContentsToolStripMenuItem.Text = "&Variables and their contents";
			this.variablesAndTheirContentsToolStripMenuItem.Click += new System.EventHandler(this.variablesAndTheirContentsToolStripMenuItem_Click);
			//
			// hotkeysAndTheirMethodsToolStripMenuItem
			//
			this.hotkeysAndTheirMethodsToolStripMenuItem.Name = "hotkeysAndTheirMethodsToolStripMenuItem";
			this.hotkeysAndTheirMethodsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this.hotkeysAndTheirMethodsToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
			this.hotkeysAndTheirMethodsToolStripMenuItem.Text = "&Hotkeys and their methods";
			this.hotkeysAndTheirMethodsToolStripMenuItem.Click += new System.EventHandler(this.hotkeysAndTheirMethodsToolStripMenuItem_Click);
			//
			// keyHistoryAndScriptInfoToolStripMenuItem
			//
			this.keyHistoryAndScriptInfoToolStripMenuItem.Name = "keyHistoryAndScriptInfoToolStripMenuItem";
			this.keyHistoryAndScriptInfoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
			this.keyHistoryAndScriptInfoToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
			this.keyHistoryAndScriptInfoToolStripMenuItem.Text = "&Key history and script info";
			this.keyHistoryAndScriptInfoToolStripMenuItem.Click += new System.EventHandler(this.keyHistoryAndScriptInfoToolStripMenuItem_Click);
			//
			// toolStripSeparator2
			//
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(257, 6);
			//
			// refreshToolStripMenuItem
			//
			this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
			this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.refreshToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
			this.refreshToolStripMenuItem.Text = "&Refresh";
			this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
			//
			// helpToolStripMenuItem
			//
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.userManualToolStripMenuItem,
				this.websiteToolStripMenuItem
			});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
			this.helpToolStripMenuItem.Text = "&Help";
			//
			// userManualToolStripMenuItem
			//
			this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
			this.userManualToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.userManualToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.userManualToolStripMenuItem.Text = "&User Manual";
			this.userManualToolStripMenuItem.Click += new System.EventHandler(this.userManualToolStripMenuItem_Click);
			//
			// websiteToolStripMenuItem
			//
			this.websiteToolStripMenuItem.Name = "websiteToolStripMenuItem";
			this.websiteToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.websiteToolStripMenuItem.Text = "&Website";
			this.websiteToolStripMenuItem.Click += new System.EventHandler(this.websiteToolStripMenuItem_Click);
			//
			// tcMain
			//
			this.tcMain.Controls.Add(this.tpStack);
			this.tcMain.Controls.Add(this.tpDebug);
			this.tcMain.Controls.Add(this.tpVars);
			this.tcMain.Controls.Add(this.tpHotkeys);
			this.tcMain.Controls.Add(this.tpHistory);
			this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tcMain.Location = new System.Drawing.Point(0, 24);
			this.tcMain.Margin = new System.Windows.Forms.Padding(2);
			this.tcMain.Name = "tcMain";
			this.tcMain.SelectedIndex = 0;
			this.tcMain.Size = new System.Drawing.Size(843, 511);
			this.tcMain.TabIndex = 3;
			//
			// tpStack
			//
			this.tpStack.Controls.Add(this.txtStack);
			this.tpStack.Location = new System.Drawing.Point(4, 24);
			this.tpStack.Margin = new System.Windows.Forms.Padding(2);
			this.tpStack.Name = "tpStack";
			this.tpStack.Padding = new System.Windows.Forms.Padding(2);
			this.tpStack.Size = new System.Drawing.Size(835, 483);
			this.tpStack.TabIndex = 0;
			this.tpStack.Text = "Stack";
			this.tpStack.UseVisualStyleBackColor = true;
			//
			// tpDebug
			//
			this.tpDebug.Controls.Add(this.txtDebug);
			this.tpDebug.Location = new System.Drawing.Point(4, 24);
			this.tpDebug.Margin = new System.Windows.Forms.Padding(2);
			this.tpDebug.Name = "tpDebug";
			this.tpDebug.Padding = new System.Windows.Forms.Padding(2);
			this.tpDebug.Size = new System.Drawing.Size(835, 483);
			this.tpDebug.TabIndex = 1;
			this.tpDebug.Text = "Debug";
			this.tpDebug.UseVisualStyleBackColor = true;
			//
			// txtDebug
			//
			this.txtDebug.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtDebug.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtDebug.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtDebug.Location = new System.Drawing.Point(2, 2);
			this.txtDebug.Margin = new System.Windows.Forms.Padding(2);
			this.txtDebug.Multiline = true;
			this.txtDebug.Name = "txtDebug";
			this.txtDebug.ReadOnly = true;
			this.txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtDebug.Size = new System.Drawing.Size(831, 479);
			this.txtDebug.TabIndex = 1;
			//
			// tpVars
			//
			this.tpVars.Controls.Add(this.txtVars);
			this.tpVars.Location = new System.Drawing.Point(4, 24);
			this.tpVars.Margin = new System.Windows.Forms.Padding(2);
			this.tpVars.Name = "tpVars";
			this.tpVars.Padding = new System.Windows.Forms.Padding(2);
			this.tpVars.Size = new System.Drawing.Size(835, 483);
			this.tpVars.TabIndex = 2;
			this.tpVars.Text = "Vars";
			this.tpVars.UseVisualStyleBackColor = true;
			//
			// txtVars
			//
			this.txtVars.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtVars.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtVars.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtVars.Location = new System.Drawing.Point(2, 2);
			this.txtVars.Margin = new System.Windows.Forms.Padding(2);
			this.txtVars.Multiline = true;
			this.txtVars.Name = "txtVars";
			this.txtVars.ReadOnly = true;
			this.txtVars.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtVars.Size = new System.Drawing.Size(831, 479);
			this.txtVars.TabIndex = 2;
			//
			// tpHotkeys
			//
			this.tpHotkeys.Controls.Add(this.txtHotkeys);
			this.tpHotkeys.Location = new System.Drawing.Point(4, 24);
			this.tpHotkeys.Name = "tpHotkeys";
			this.tpHotkeys.Padding = new System.Windows.Forms.Padding(3);
			this.tpHotkeys.Size = new System.Drawing.Size(835, 483);
			this.tpHotkeys.TabIndex = 3;
			this.tpHotkeys.Text = "Hotkeys";
			this.tpHotkeys.UseVisualStyleBackColor = true;
			//
			// txtHotkeys
			//
			this.txtHotkeys.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtHotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtHotkeys.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtHotkeys.Location = new System.Drawing.Point(3, 3);
			this.txtHotkeys.Margin = new System.Windows.Forms.Padding(2);
			this.txtHotkeys.Multiline = true;
			this.txtHotkeys.Name = "txtHotkeys";
			this.txtHotkeys.ReadOnly = true;
			this.txtHotkeys.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtHotkeys.Size = new System.Drawing.Size(829, 477);
			this.txtHotkeys.TabIndex = 3;
			//
			// tpHistory
			//
			this.tpHistory.Controls.Add(this.txtHistory);
			this.tpHistory.Location = new System.Drawing.Point(4, 24);
			this.tpHistory.Margin = new System.Windows.Forms.Padding(2);
			this.tpHistory.Name = "tpHistory";
			this.tpHistory.Padding = new System.Windows.Forms.Padding(2);
			this.tpHistory.Size = new System.Drawing.Size(835, 483);
			this.tpHistory.TabIndex = 4;
			this.tpHistory.Text = "History";
			this.tpHistory.UseVisualStyleBackColor = true;
			//
			// txtHistory
			//
			this.txtHistory.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtHistory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtHistory.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtHistory.Location = new System.Drawing.Point(2, 2);
			this.txtHistory.Margin = new System.Windows.Forms.Padding(2);
			this.txtHistory.Multiline = true;
			this.txtHistory.Name = "txtHistory";
			this.txtHistory.ReadOnly = true;
			this.txtHistory.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtHistory.Size = new System.Drawing.Size(831, 479);
			this.txtHistory.TabIndex = 3;
			//
			// MainWindow
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(843, 535);
			this.Controls.Add(this.tcMain);
			this.Controls.Add(this.mainMenu);
			this.MainMenuStrip = this.mainMenu;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "MainWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Keysharp";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
			this.Load += new System.EventHandler(this.MainWindow_Load);
			this.Shown += new System.EventHandler(this.MainWindow_Shown);
			this.SizeChanged += new System.EventHandler(this.MainWindow_SizeChanged);
			this.mainMenu.ResumeLayout(false);
			this.mainMenu.PerformLayout();
			this.tcMain.ResumeLayout(false);
			this.tpStack.ResumeLayout(false);
			this.tpStack.PerformLayout();
			this.tpDebug.ResumeLayout(false);
			this.tpDebug.PerformLayout();
			this.tpVars.ResumeLayout(false);
			this.tpVars.PerformLayout();
			this.tpHotkeys.ResumeLayout(false);
			this.tpHotkeys.PerformLayout();
			this.tpHistory.ResumeLayout(false);
			this.tpHistory.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.TextBox txtStack;
		private System.Windows.Forms.MenuStrip mainMenu;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem reloadScriptToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editScriptToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem windowSpyToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem suspendHotkeysToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem variablesAndTheirContentsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem hotkeysAndTheirMethodsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem keyHistoryAndScriptInfoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem userManualToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem websiteToolStripMenuItem;
		private System.Windows.Forms.TabControl tcMain;
		private System.Windows.Forms.TabPage tpStack;
		private System.Windows.Forms.TabPage tpDebug;
		private System.Windows.Forms.TabPage tpVars;
		private System.Windows.Forms.TextBox txtDebug;
		private System.Windows.Forms.TextBox txtVars;
		private System.Windows.Forms.TabPage tpHotkeys;
		private System.Windows.Forms.TextBox txtHotkeys;
		private System.Windows.Forms.TabPage tpHistory;
		private System.Windows.Forms.TextBox txtHistory;
	}
}