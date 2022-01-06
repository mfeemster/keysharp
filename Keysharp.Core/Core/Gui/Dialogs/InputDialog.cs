using System;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Scripting;

namespace Keysharp.Core
{
	internal class InputDialog : Form
	{
		private const int CP_NOCLOSE_BUTTON = 0x200;
		private Button btnCancel;
		private Button btnOK;
		private Label prompt;
		private System.Windows.Forms.Timer timer;
		private TextBox txtMessage;
		public string Default { get; set; }

		public string Message
		{
			get => txtMessage.Text;
			set => txtMessage.Text = value;
			//set => txtMessage.Invoke(() => txtMessage.Text = value);
		}

		public string PasswordChar
		{
			get => txtMessage.PasswordChar.ToString();

			set
			{
				if (string.IsNullOrEmpty(value))
					txtMessage.UseSystemPasswordChar = true;
				else
					txtMessage.PasswordChar = value[0];
			}
		}

		public string Prompt
		{
			get => prompt.Text;
			set => prompt.Text = value;
			//set => prompt.Invoke(() => prompt.Text = value);
		}

		public string Result { get; private set; } = "";

		public int Timeout { get; set; }

		public string Title
		{
			get => Text;
			set => Text = value;
			//set => this.Invoke(() => Text = value);
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var myCp = base.CreateParams;
				myCp.ClassStyle |= CP_NOCLOSE_BUTTON;
				return myCp;
			}
		}

		public InputDialog()
		{
			InitializeComponent();
			Load += InputDialog_Load;
			Shown += InputDialog_Shown;
			txtMessage.KeyDown += TxtMessage_KeyDown;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Result = "Cancel";
			Hide();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			Result = "OK";
			Hide();
		}

		private void InitializeComponent()
		{
			prompt = new Label();
			btnOK = new Button();
			btnCancel = new Button();
			txtMessage = new TextBox();
			SuspendLayout();
			//
			// label1
			//
			prompt.AutoSize = true;
			prompt.Location = new Point(9, 5);
			prompt.Name = "Prompt";
			prompt.Size = new Size(240, 48);
			prompt.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			//
			// btnOK
			//
			btnOK.DialogResult = DialogResult.OK;
			btnOK.Location = new Point(10, 120);
			btnOK.Name = "OK";
			btnOK.Size = new Size(130, 44);
			btnOK.TabIndex = 1;
			btnOK.Text = "OK";
			btnOK.Click += btnOK_Click;
			btnOK.UseVisualStyleBackColor = true;
			btnOK.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			//
			// btnCancel
			//
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.Location = new Point(160, 120);
			btnCancel.Name = "Cancel";
			btnCancel.Size = new Size(130, 44);
			btnCancel.TabIndex = 2;
			btnCancel.Text = "Cancel";
			btnCancel.Click += btnCancel_Click;
			btnCancel.UseVisualStyleBackColor = true;
			btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
			//
			// txtMessage
			//
			txtMessage.Location = new Point(10, 72);
			txtMessage.Name = "Message";
			txtMessage.Size = new Size(280, 24);
			txtMessage.TabIndex = 0;
			txtMessage.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			//
			// InputDialog
			//
			//AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
			AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new Size(300, 300);
			MinimumSize = new Size(310, 200);
			//Can't set controlbox to false, else icon will not show.
			Controls.Add(btnCancel);
			Controls.Add(btnOK);
			Controls.Add(prompt);
			Controls.Add(txtMessage);
			ShowIcon = true;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "KeysharpInputBox";
			Text = "Keysharp Input Box";
			ResumeLayout(false);
			PerformLayout();
		}

		private void InputDialog_Load(object sender, EventArgs e)
		{
			txtMessage.Text = Default;
			txtMessage.Top = prompt.Top + prompt.ClientSize.Height + txtMessage.Margin.Top;
			btnOK.Top = txtMessage.Top + (int)(txtMessage.Height * Accessors.A_ScaledScreenDPI);
			btnCancel.Top = btnOK.Top;
			Height = btnOK.Top + (int)(((txtMessage.Margin.Top * 8) + btnOK.Height) * Accessors.A_ScaledScreenDPI);
		}

		private void InputDialog_Shown(object sender, EventArgs e)
		{
			if (Core.Tray?.Icon != null)
				Icon = Core.Tray.Icon;

			if (Timeout > 0)
			{
				timer = new Timer
				{
					Interval = Timeout * 1000
				};
				timer.Tick += Timer_Tick;
				timer.Enabled = true;
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			timer.Enabled = false;
			btnCancel.PerformClick();
		}

		private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				btnOK.PerformClick();
			else if (e.KeyCode == Keys.Escape)
				btnCancel.PerformClick();
		}
	}
}