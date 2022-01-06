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
			this.txtMain = new System.Windows.Forms.TextBox();
			this.mainMenu = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.reloadScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.windowSpyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.pauseScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
			this.tpMain = new System.Windows.Forms.TabPage();
			this.tpDebug = new System.Windows.Forms.TabPage();
			this.txtDebug = new System.Windows.Forms.TextBox();
			this.tpVars = new System.Windows.Forms.TabPage();
			this.txtVars = new System.Windows.Forms.TextBox();
			this.mainMenu.SuspendLayout();
			this.tcMain.SuspendLayout();
			this.tpMain.SuspendLayout();
			this.tpDebug.SuspendLayout();
			this.tpVars.SuspendLayout();
			this.SuspendLayout();
			//
			// txtMain
			//
			this.txtMain.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtMain.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtMain.Location = new System.Drawing.Point(3, 3);
			this.txtMain.Multiline = true;
			this.txtMain.Name = "txtMain";
			this.txtMain.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMain.Size = new System.Drawing.Size(1251, 728);
			this.txtMain.TabIndex = 0;
			//
			// mainMenu
			//
			this.mainMenu.ImageScalingSize = new System.Drawing.Size(28, 28);
			this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.fileToolStripMenuItem,
				this.viewToolStripMenuItem,
				this.helpToolStripMenuItem
			});
			this.mainMenu.Location = new System.Drawing.Point(0, 0);
			this.mainMenu.Name = "mainMenu";
			this.mainMenu.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
			this.mainMenu.Size = new System.Drawing.Size(1265, 33);
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
				this.pauseScriptToolStripMenuItem,
				this.suspendHotkeysToolStripMenuItem,
				this.exitToolStripMenuItem
			});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
			this.fileToolStripMenuItem.Text = "&File";
			//
			// reloadScriptToolStripMenuItem
			//
			this.reloadScriptToolStripMenuItem.Name = "reloadScriptToolStripMenuItem";
			this.reloadScriptToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.reloadScriptToolStripMenuItem.Size = new System.Drawing.Size(280, 34);
			this.reloadScriptToolStripMenuItem.Text = "&Reload Script";
			this.reloadScriptToolStripMenuItem.Click += new System.EventHandler(this.reloadScriptToolStripMenuItem_Click);
			//
			// editScriptToolStripMenuItem
			//
			this.editScriptToolStripMenuItem.Name = "editScriptToolStripMenuItem";
			this.editScriptToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
			this.editScriptToolStripMenuItem.Size = new System.Drawing.Size(280, 34);
			this.editScriptToolStripMenuItem.Text = "&Edit Script";
			this.editScriptToolStripMenuItem.Click += new System.EventHandler(this.editScriptToolStripMenuItem_Click);
			//
			// windowSpyToolStripMenuItem
			//
			this.windowSpyToolStripMenuItem.Name = "windowSpyToolStripMenuItem";
			this.windowSpyToolStripMenuItem.Size = new System.Drawing.Size(280, 34);
			this.windowSpyToolStripMenuItem.Text = "&Window Spy";
			this.windowSpyToolStripMenuItem.Click += new System.EventHandler(this.windowSpyToolStripMenuItem_Click);
			//
			// toolStripSeparator1
			//
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(277, 6);
			//
			// pauseScriptToolStripMenuItem
			//
			this.pauseScriptToolStripMenuItem.Name = "pauseScriptToolStripMenuItem";
			this.pauseScriptToolStripMenuItem.Size = new System.Drawing.Size(280, 34);
			this.pauseScriptToolStripMenuItem.Text = "&Pause Script";
			this.pauseScriptToolStripMenuItem.Click += new System.EventHandler(this.pauseScriptToolStripMenuItem_Click);
			//
			// suspendHotkeysToolStripMenuItem
			//
			this.suspendHotkeysToolStripMenuItem.Name = "suspendHotkeysToolStripMenuItem";
			this.suspendHotkeysToolStripMenuItem.Size = new System.Drawing.Size(280, 34);
			this.suspendHotkeysToolStripMenuItem.Text = "&Suspend Hotkeys";
			this.suspendHotkeysToolStripMenuItem.Click += new System.EventHandler(this.suspendHotkeysToolStripMenuItem_Click);
			//
			// exitToolStripMenuItem
			//
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(280, 34);
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
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
			this.viewToolStripMenuItem.Text = "&View";
			//
			// variablesAndTheirContentsToolStripMenuItem
			//
			this.variablesAndTheirContentsToolStripMenuItem.Name = "variablesAndTheirContentsToolStripMenuItem";
			this.variablesAndTheirContentsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.variablesAndTheirContentsToolStripMenuItem.Size = new System.Drawing.Size(394, 34);
			this.variablesAndTheirContentsToolStripMenuItem.Text = "&Variables and their contents";
			this.variablesAndTheirContentsToolStripMenuItem.Click += new System.EventHandler(this.variablesAndTheirContentsToolStripMenuItem_Click);
			//
			// hotkeysAndTheirMethodsToolStripMenuItem
			//
			this.hotkeysAndTheirMethodsToolStripMenuItem.Name = "hotkeysAndTheirMethodsToolStripMenuItem";
			this.hotkeysAndTheirMethodsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this.hotkeysAndTheirMethodsToolStripMenuItem.Size = new System.Drawing.Size(394, 34);
			this.hotkeysAndTheirMethodsToolStripMenuItem.Text = "&Hotkeys and their methods";
			this.hotkeysAndTheirMethodsToolStripMenuItem.Click += new System.EventHandler(this.hotkeysAndTheirMethodsToolStripMenuItem_Click);
			//
			// keyHistoryAndScriptInfoToolStripMenuItem
			//
			this.keyHistoryAndScriptInfoToolStripMenuItem.Name = "keyHistoryAndScriptInfoToolStripMenuItem";
			this.keyHistoryAndScriptInfoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
			this.keyHistoryAndScriptInfoToolStripMenuItem.Size = new System.Drawing.Size(394, 34);
			this.keyHistoryAndScriptInfoToolStripMenuItem.Text = "&Key history and script info";
			this.keyHistoryAndScriptInfoToolStripMenuItem.Click += new System.EventHandler(this.keyHistoryAndScriptInfoToolStripMenuItem_Click);
			//
			// toolStripSeparator2
			//
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(391, 6);
			//
			// refreshToolStripMenuItem
			//
			this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
			this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.refreshToolStripMenuItem.Size = new System.Drawing.Size(394, 34);
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
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
			this.helpToolStripMenuItem.Text = "&Help";
			//
			// userManualToolStripMenuItem
			//
			this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
			this.userManualToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.userManualToolStripMenuItem.Size = new System.Drawing.Size(243, 34);
			this.userManualToolStripMenuItem.Text = "&User Manual";
			this.userManualToolStripMenuItem.Click += new System.EventHandler(this.userManualToolStripMenuItem_Click);
			//
			// websiteToolStripMenuItem
			//
			this.websiteToolStripMenuItem.Name = "websiteToolStripMenuItem";
			this.websiteToolStripMenuItem.Size = new System.Drawing.Size(243, 34);
			this.websiteToolStripMenuItem.Text = "&Website";
			this.websiteToolStripMenuItem.Click += new System.EventHandler(this.websiteToolStripMenuItem_Click);
			//
			// tcMain
			//
			this.tcMain.Controls.Add(this.tpMain);
			this.tcMain.Controls.Add(this.tpDebug);
			this.tcMain.Controls.Add(this.tpVars);
			this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tcMain.Location = new System.Drawing.Point(0, 33);
			this.tcMain.Name = "tcMain";
			this.tcMain.SelectedIndex = 0;
			this.tcMain.Size = new System.Drawing.Size(1265, 769);
			this.tcMain.TabIndex = 3;
			//
			// tpMain
			//
			this.tpMain.Controls.Add(this.txtMain);
			this.tpMain.Location = new System.Drawing.Point(4, 31);
			this.tpMain.Name = "tpMain";
			this.tpMain.Padding = new System.Windows.Forms.Padding(3);
			this.tpMain.Size = new System.Drawing.Size(1257, 734);
			this.tpMain.TabIndex = 0;
			this.tpMain.Text = "Main";
			this.tpMain.UseVisualStyleBackColor = true;
			//
			// tpDebug
			//
			this.tpDebug.Controls.Add(this.txtDebug);
			this.tpDebug.Location = new System.Drawing.Point(4, 31);
			this.tpDebug.Name = "tpDebug";
			this.tpDebug.Padding = new System.Windows.Forms.Padding(3);
			this.tpDebug.Size = new System.Drawing.Size(1257, 734);
			this.tpDebug.TabIndex = 1;
			this.tpDebug.Text = "Debug";
			this.tpDebug.UseVisualStyleBackColor = true;
			//
			// txtDebug
			//
			this.txtDebug.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtDebug.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtDebug.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtDebug.Location = new System.Drawing.Point(3, 3);
			this.txtDebug.Multiline = true;
			this.txtDebug.Name = "txtDebug";
			this.txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtDebug.Size = new System.Drawing.Size(1251, 728);
			this.txtDebug.TabIndex = 1;
			//
			// tpVars
			//
			this.tpVars.Controls.Add(this.txtVars);
			this.tpVars.Location = new System.Drawing.Point(4, 31);
			this.tpVars.Name = "tpVars";
			this.tpVars.Padding = new System.Windows.Forms.Padding(3);
			this.tpVars.Size = new System.Drawing.Size(1257, 734);
			this.tpVars.TabIndex = 2;
			this.tpVars.Text = "Vars";
			this.tpVars.UseVisualStyleBackColor = true;
			//
			// txtVars
			//
			this.txtVars.BackColor = System.Drawing.SystemColors.MenuBar;
			this.txtVars.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtVars.Font = new System.Drawing.Font("Cascadia Code", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtVars.Location = new System.Drawing.Point(3, 3);
			this.txtVars.Multiline = true;
			this.txtVars.Name = "txtVars";
			this.txtVars.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtVars.Size = new System.Drawing.Size(1251, 728);
			this.txtVars.TabIndex = 2;
			//
			// MainWindow
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(1265, 802);
			this.Controls.Add(this.tcMain);
			this.Controls.Add(this.mainMenu);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.MainMenuStrip = this.mainMenu;
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
			this.tpMain.ResumeLayout(false);
			this.tpMain.PerformLayout();
			this.tpDebug.ResumeLayout(false);
			this.tpDebug.PerformLayout();
			this.tpVars.ResumeLayout(false);
			this.tpVars.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.TextBox txtMain;
		private System.Windows.Forms.MenuStrip mainMenu;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem reloadScriptToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editScriptToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem windowSpyToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem pauseScriptToolStripMenuItem;
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
		private System.Windows.Forms.TabPage tpMain;
		private System.Windows.Forms.TabPage tpDebug;
		private System.Windows.Forms.TabPage tpVars;
		private System.Windows.Forms.TextBox txtDebug;
		private System.Windows.Forms.TextBox txtVars;
	}
}