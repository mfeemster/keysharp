#if WINDOWS
namespace Keysharp.Core.Common.Window
{
	internal class KeysharpActiveX : UserControl
	{
		private static bool loadedDll = false;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string AxText { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal ComObject Iid { get; private set; }

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style |= WindowsAPI.WS_CLIPSIBLINGS;
				return cp;
			}
		}

		public KeysharpActiveX(string text)
		{
			AxText = text;
			InitializeComponent();
			//this.Load += KeysharpActiveX_Load;

			if (!loadedDll)
			{
				int result = AtlAxWinInit();

				if (result == 0)
				{
					Error err;
					_ = Errors.ErrorOccurred(err = new Error($"Initializing ActiveX with AtlAxWinInit() failed.")) ? throw err : "";
				}
				else
					loadedDll = true;
			}
		}

		[DllImport("atl.dll", CharSet = CharSet.Unicode)]
		public static extern int AtlAxCreateControl(
			string lpszName,
			IntPtr hWnd,
			IntPtr pStream,
			[MarshalAs(UnmanagedType.IDispatch)] out object ppUnkContainer);

		[DllImport("atl.dll", CharSet = CharSet.Unicode)]
		public static extern int AtlAxWinInit();

		internal void Init()
		{
			if (loadedDll)
			{
				var hInstance = Marshal.GetHINSTANCE(GetType().Module);

				if (AtlAxCreateControl(AxText, Handle, IntPtr.Zero, out var ob) >= 0)
					//if (axHandle != IntPtr.Zero)
				{
					Console.WriteLine("AtlAxCreateControl() succeeded.");

					if (ob is IDispatch iid)
						Iid = new ComObject(Com.vt_variant, iid);
					else
						Iid = new ComObject(Com.vt_unknown, ob);
				}
			}
		}

		/// <summary>
		/// Clean up any resources being used.
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			SuspendLayout();
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Dpi;
			Name = "KeysharpActiveX";
			Size = new Size(500, 500);
			ResumeLayout(false);
		}

		#endregion Component Designer generated code
	}
}

#endif