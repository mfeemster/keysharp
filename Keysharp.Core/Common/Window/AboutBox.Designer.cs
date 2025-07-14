namespace Keysharp.Core.Common.Window
{
	partial class AboutBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
			tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			logoPictureBox = new System.Windows.Forms.PictureBox();
			labelProductName = new System.Windows.Forms.Label();
			textBoxDescription = new System.Windows.Forms.TextBox();
			okButton = new System.Windows.Forms.Button();
			linkLabel = new System.Windows.Forms.LinkLabel();
			tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
			SuspendLayout();
			//
			// tableLayoutPanel
			//
			tableLayoutPanel.ColumnCount = 2;
			tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.61391F));
			tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.38609F));
			tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
			tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
			tableLayoutPanel.Controls.Add(textBoxDescription, 1, 2);
			tableLayoutPanel.Controls.Add(okButton, 1, 3);
			tableLayoutPanel.Controls.Add(linkLabel, 1, 1);
			tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel.Location = new System.Drawing.Point(10, 10);
			tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			tableLayoutPanel.Name = "tableLayoutPanel";
			tableLayoutPanel.RowCount = 4;
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.51735F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.832808F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 69.71609F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanel.Size = new System.Drawing.Size(834, 283);
			tableLayoutPanel.TabIndex = 0;
			//
			// logoPictureBox
			//
			logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			logoPictureBox.Image = (System.Drawing.Image)resources.GetObject("logoPictureBox.Image");
			logoPictureBox.Location = new System.Drawing.Point(4, 3);
			logoPictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			logoPictureBox.Name = "logoPictureBox";
			tableLayoutPanel.SetRowSpan(logoPictureBox, 4);
			logoPictureBox.Size = new System.Drawing.Size(264, 277);
			logoPictureBox.TabIndex = 12;
			logoPictureBox.TabStop = false;
			//
			// labelProductName
			//
			labelProductName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			labelProductName.Location = new System.Drawing.Point(272, 2);
			labelProductName.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
			labelProductName.MaximumSize = new System.Drawing.Size(0, 20);
			labelProductName.Name = "labelProductName";
			labelProductName.Size = new System.Drawing.Size(548, 20);
			labelProductName.TabIndex = 19;
			labelProductName.Text = "Keysharp";
			labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// textBoxDescription
			//
			textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			textBoxDescription.Location = new System.Drawing.Point(272, 52);
			textBoxDescription.Margin = new System.Windows.Forms.Padding(0, 3, 4, 3);
			textBoxDescription.Multiline = true;
			textBoxDescription.Name = "textBoxDescription";
			textBoxDescription.ReadOnly = true;
			textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			textBoxDescription.Size = new System.Drawing.Size(558, 192);
			textBoxDescription.TabIndex = 23;
			textBoxDescription.TabStop = false;
			textBoxDescription.Text = "Description";
			//
			// okButton
			//
			okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			okButton.Location = new System.Drawing.Point(742, 253);
			okButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			okButton.Name = "okButton";
			okButton.Size = new System.Drawing.Size(88, 27);
			okButton.TabIndex = 24;
			okButton.Text = "&OK";
			okButton.Click += okButton_Click;
			//
			// linkLabel
			//
			linkLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			linkLabel.AutoSize = true;
			linkLabel.Location = new System.Drawing.Point(272, 29);
			linkLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			linkLabel.Name = "linkLabel";
			linkLabel.Size = new System.Drawing.Size(298, 15);
			linkLabel.TabIndex = 25;
			linkLabel.TabStop = true;
			linkLabel.Text = "https://github.com/mfeemster/keysharp/tree/master";
			linkLabel.LinkClicked += linkLabel_LinkClicked;
			//
			// AboutBox
			//
			AcceptButton = okButton;
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(854, 303);
			Controls.Add(tableLayoutPanel);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "AboutBox";
			Padding = new System.Windows.Forms.Padding(10);
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "About Keysharp";
			tableLayoutPanel.ResumeLayout(false);
			tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private System.Windows.Forms.PictureBox logoPictureBox;
		private System.Windows.Forms.Label labelProductName;
		private System.Windows.Forms.TextBox textBoxDescription;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.LinkLabel linkLabel;
	}
}
