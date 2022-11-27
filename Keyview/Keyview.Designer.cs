
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
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.txtIn = new System.Windows.Forms.RichTextBox();
			this.lines1 = new Rtf.LineNumbers_For_RichTextBox();
			this.txtOut = new System.Windows.Forms.RichTextBox();
			this.lines2 = new Rtf.LineNumbers_For_RichTextBox();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tslCodeStatus = new System.Windows.Forms.ToolStripLabel();
			this.tssCode = new System.Windows.Forms.ToolStripSeparator();
			this.tslCodeCompile = new System.Windows.Forms.ToolStripLabel();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			//
			// splitContainer
			//
			this.splitContainer.Cursor = System.Windows.Forms.Cursors.VSplit;
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer.Name = "splitContainer";
			//
			// splitContainer.Panel1
			//
			this.splitContainer.Panel1.Controls.Add(this.txtIn);
			this.splitContainer.Panel1.Controls.Add(this.lines1);
			this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(1);
			//
			// splitContainer.Panel2
			//
			this.splitContainer.Panel2.Controls.Add(this.txtOut);
			this.splitContainer.Panel2.Controls.Add(this.lines2);
			this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(1);
			this.splitContainer.Size = new System.Drawing.Size(976, 405);
			this.splitContainer.SplitterDistance = 500;
			this.splitContainer.TabIndex = 0;
			this.splitContainer.DoubleClick += new System.EventHandler(this.splitContainer_DoubleClick);
			//
			// txtIn
			//
			this.txtIn.AcceptsTab = true;
			this.txtIn.AutoWordSelection = true;
			this.txtIn.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtIn.Font = new System.Drawing.Font("Consolas", 9.857143F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtIn.Location = new System.Drawing.Point(43, 1);
			this.txtIn.Margin = new System.Windows.Forms.Padding(0);
			this.txtIn.Name = "txtIn";
			this.txtIn.Size = new System.Drawing.Size(456, 403);
			this.txtIn.TabIndex = 0;
			this.txtIn.Text = "";
			this.txtIn.TextChanged += new System.EventHandler(this.txtIn_TextChanged);
			this.txtIn.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtIn_KeyDown);
			//
			// lines1
			//
			this.lines1._SeeThroughMode_ = false;
			this.lines1.AutoSizing = true;
			this.lines1.BackColor = System.Drawing.SystemColors.Control;
			this.lines1.BackgroundGradient_AlphaColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.lines1.BackgroundGradient_BetaColor = System.Drawing.Color.LightSteelBlue;
			this.lines1.BackgroundGradient_Direction = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
			this.lines1.BorderLines_Color = System.Drawing.Color.SlateGray;
			this.lines1.BorderLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
			this.lines1.BorderLines_Thickness = 1F;
			this.lines1.Dock = System.Windows.Forms.DockStyle.Left;
			this.lines1.DockSide = Rtf.LineNumbers_For_RichTextBox.LineNumberDockSide.Left;
			this.lines1.GridLines_Color = System.Drawing.Color.SlateGray;
			this.lines1.GridLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
			this.lines1.GridLines_Thickness = 1F;
			this.lines1.LineNrs_Alignment = System.Drawing.ContentAlignment.MiddleRight;
			this.lines1.LineNrs_AntiAlias = true;
			this.lines1.LineNrs_AsHexadecimal = false;
			this.lines1.LineNrs_ClippedByItemRectangle = true;
			this.lines1.LineNrs_LeadingZeroes = false;
			this.lines1.LineNrs_Offset = new System.Drawing.Size(0, 0);
			this.lines1.Location = new System.Drawing.Point(1, 1);
			this.lines1.Margin = new System.Windows.Forms.Padding(0);
			this.lines1.MarginLines_Color = System.Drawing.Color.SlateGray;
			this.lines1.MarginLines_Side = Rtf.LineNumbers_For_RichTextBox.LineNumberDockSide.Right;
			this.lines1.MarginLines_Style = System.Drawing.Drawing2D.DashStyle.Solid;
			this.lines1.MarginLines_Thickness = 1F;
			this.lines1.Name = "lines1";
			this.lines1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.lines1.ParentRichTextBox = this.txtIn;
			this.lines1.Show_BackgroundGradient = false;
			this.lines1.Show_BorderLines = false;
			this.lines1.Show_GridLines = false;
			this.lines1.Show_LineNrs = true;
			this.lines1.Show_MarginLines = false;
			this.lines1.Size = new System.Drawing.Size(42, 403);
			this.lines1.TabIndex = 0;
			//
			// txtOut
			//
			this.txtOut.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOut.Font = new System.Drawing.Font("Consolas", 9.857143F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.txtOut.Location = new System.Drawing.Point(43, 1);
			this.txtOut.Margin = new System.Windows.Forms.Padding(0);
			this.txtOut.Name = "txtOut";
			this.txtOut.ReadOnly = true;
			this.txtOut.Size = new System.Drawing.Size(428, 403);
			this.txtOut.TabIndex = 1;
			this.txtOut.Text = "";
			//
			// lines2
			//
			this.lines2._SeeThroughMode_ = false;
			this.lines2.AutoSizing = true;
			this.lines2.BackColor = System.Drawing.SystemColors.Control;
			this.lines2.BackgroundGradient_AlphaColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.lines2.BackgroundGradient_BetaColor = System.Drawing.Color.LightSteelBlue;
			this.lines2.BackgroundGradient_Direction = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
			this.lines2.BorderLines_Color = System.Drawing.Color.SlateGray;
			this.lines2.BorderLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
			this.lines2.BorderLines_Thickness = 1F;
			this.lines2.Dock = System.Windows.Forms.DockStyle.Left;
			this.lines2.DockSide = Rtf.LineNumbers_For_RichTextBox.LineNumberDockSide.Left;
			this.lines2.GridLines_Color = System.Drawing.Color.SlateGray;
			this.lines2.GridLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
			this.lines2.GridLines_Thickness = 1F;
			this.lines2.LineNrs_Alignment = System.Drawing.ContentAlignment.MiddleRight;
			this.lines2.LineNrs_AntiAlias = true;
			this.lines2.LineNrs_AsHexadecimal = false;
			this.lines2.LineNrs_ClippedByItemRectangle = false;
			this.lines2.LineNrs_LeadingZeroes = false;
			this.lines2.LineNrs_Offset = new System.Drawing.Size(0, 0);
			this.lines2.Location = new System.Drawing.Point(1, 1);
			this.lines2.Margin = new System.Windows.Forms.Padding(0);
			this.lines2.MarginLines_Color = System.Drawing.Color.SlateGray;
			this.lines2.MarginLines_Side = Rtf.LineNumbers_For_RichTextBox.LineNumberDockSide.Right;
			this.lines2.MarginLines_Style = System.Drawing.Drawing2D.DashStyle.Solid;
			this.lines2.MarginLines_Thickness = 1F;
			this.lines2.Name = "lines2";
			this.lines2.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.lines2.ParentRichTextBox = this.txtOut;
			this.lines2.Show_BackgroundGradient = false;
			this.lines2.Show_BorderLines = false;
			this.lines2.Show_GridLines = false;
			this.lines2.Show_LineNrs = true;
			this.lines2.Show_MarginLines = false;
			this.lines2.Size = new System.Drawing.Size(42, 403);
			this.lines2.TabIndex = 0;
			//
			// toolStrip1
			//
			this.toolStrip1.AutoSize = false;
			this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			//this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.tslCodeStatus,
				this.tssCode,
				this.tslCodeCompile
			});
			this.toolStrip1.Location = new System.Drawing.Point(0, 405);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(976, 45);
			this.toolStrip1.TabIndex = 1;
			//
			// tslCodeStatus
			//
			this.tslCodeStatus.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tslCodeStatus.AutoSize = false;
			this.tslCodeStatus.Name = "tslCodeStatus";
			this.tslCodeStatus.Size = new System.Drawing.Size(300, 39);
			this.tslCodeStatus.Text = "------------------------";
			//
			// tssCode
			//
			this.tssCode.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tssCode.Name = "tssCode";
			this.tssCode.Size = new System.Drawing.Size(6, 45);
			//
			// tslCodeCompile
			//
			this.tslCodeCompile.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tslCodeCompile.Name = "tslCodeCompile";
			this.tslCodeCompile.Size = new System.Drawing.Size(145, 39);
			this.tslCodeCompile.Text = "Code compile:";
			//
			// Keyview
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(976, 450);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.toolStrip1);
			this.Name = "Keyview";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Keyview";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Keyview_FormClosing);
			this.Load += new System.EventHandler(this.Keyview_Load);
			this.ResizeEnd += new System.EventHandler(this.Keyview_ResizeEnd);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.RichTextBox txtIn;
		private System.Windows.Forms.RichTextBox txtOut;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel tslCodeCompile;
		private System.Windows.Forms.ToolStripSeparator tssCode;
		private System.Windows.Forms.ToolStripLabel tslCodeStatus;
		private Rtf.LineNumbers_For_RichTextBox lines1;
		private Rtf.LineNumbers_For_RichTextBox lines2;
	}
}

