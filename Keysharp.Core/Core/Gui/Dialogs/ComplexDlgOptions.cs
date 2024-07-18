using static Keysharp.Scripting.Keywords;

namespace Keysharp.Core
{
	public class ComplexDlgOptions
	{
		public bool AlwaysOnTop = true;
		public Color? BackgroundColor = null;
		public bool Borderless = false;
		public bool CenterMainText = true;
		public bool CenterSubText = true;
		public int GUIID = 1;
		public bool Hide = false;
		public string MainText = "";
		public Font MainTextFont = new Font(FontFamily.GenericSansSerif, 10);
		public Rectangle ObjectGeometry = new Rectangle(0, 0, 0, 0);
		public string Param1; //Progress percent or Image path or off
		public bool ShowInTaskBar = false;
		public string SubText = "";
		public Font SubTextFont = new Font(FontFamily.GenericSansSerif, 8);
		public Color TextColor = Color.Black;
		public Rectangle WindowGeometry = new Rectangle(0, 0, 0, 0);
		public string WinTitle = "";

		public ComplexDlgOptions()
		{
		}

		public void AppendShowHideTo(IComplexDialog complexDlg)
		{
			if (Param1.ToLowerInvariant() == Keyword_Off)
			{
				complexDlg.Close();
				complexDlg.Dispose();

				if (complexDlg is SplashDialog)
				{
					_ = Dialogs.SplashDialogs.Remove(GUIID);
				}
				else if (complexDlg is ProgressDialog)
				{
					_ = Dialogs.ProgressDialogs.Remove(GUIID);
				}

				return;
			}

			if (Param1.ToLowerInvariant() == Keyword_Show)
			{
				if (!complexDlg.Visible)
					complexDlg.Show();
			}
		}

		/// <summary>
		/// Append the given Options here to a IComplexDialoge.
		/// </summary>
		/// <param name="complexDlg"></param>
		public void AppendTo(IComplexDialog complexDlg)
		{
			if (complexDlg.InvokeRequired)
			{
				_ = complexDlg.Invoke(new Dialogs.AsyncComplexDialoge(AppendTo), complexDlg);
			}

			complexDlg.TopMost = AlwaysOnTop;

			if (WinTitle != "")
				complexDlg.Title = WinTitle;

			if (SubText != "")
				complexDlg.SubText = SubText;

			if (MainText != "")
				complexDlg.MainText = MainText;

			if (!Hide)
			{
				complexDlg.Show();
			}
		}

		public void ParseComplexOptions(string options)
		{
			var optsItems = new Dictionary<string, Regex>();
			optsItems.Add(Keyword_NotAlwaysOnTop, new Regex("(" + Keyword_NotAlwaysOnTop + ")"));
			optsItems.Add(Keyword_Borderless, new Regex("(" + Keyword_Borderless + ")"));
			optsItems.Add(Keyword_ProgressStartPos, new Regex(Keyword_ProgressStartPos + @"(\d*)"));
			optsItems.Add(Keyword_ProgressRange, new Regex(Keyword_ProgressRange + @"(\d*-\d*)"));
			optsItems.Add(Keyword_ShowInTaskbar, new Regex("(" + Keyword_ShowInTaskbar + ")"));
			optsItems.Add(Keyword_X, new Regex(Keyword_X + @"(\d*)"));
			optsItems.Add(Keyword_Y, new Regex(Keyword_Y + @"(\d*)"));
			optsItems.Add(Keyword_W, new Regex(Keyword_W + @"(\d*)"));
			optsItems.Add(Keyword_H, new Regex(Keyword_H + @"(\d*)"));
			optsItems.Add(Keyword_Hide, new Regex("(" + Keyword_Hide + ")"));
			optsItems.Add(Keyword_Centered, new Regex(Keyword_Centered + @"([0,1]{2})"));
			optsItems.Add(Keyword_ZH, new Regex(Keyword_ZH + @"([-,\d]*)"));
			optsItems.Add(Keyword_ZW, new Regex(Keyword_ZW + @"([-,\d]*)"));
			optsItems.Add(Keyword_ZX, new Regex(Keyword_ZX + @"(\d*)"));
			optsItems.Add(Keyword_ZY, new Regex(Keyword_ZY + @"(\d*)"));
			var dktOptions = Options.ParseOptionsRegex(ref options, optsItems, true);
			AlwaysOnTop = (dktOptions[Keyword_NotAlwaysOnTop]?.Length == 0);
			Borderless = (dktOptions[Keyword_Borderless] != "");
			ShowInTaskBar = dktOptions[Keyword_ShowInTaskbar] != "";

			try
			{
				if (dktOptions[Keyword_X] != "")
					WindowGeometry.X = int.Parse(dktOptions[Keyword_X]);

				if (dktOptions[Keyword_Y] != "")
					WindowGeometry.Y = int.Parse(dktOptions[Keyword_Y]);

				if (dktOptions[Keyword_W] != "")
					WindowGeometry.Width = int.Parse(dktOptions[Keyword_W]);

				if (dktOptions[Keyword_H] != "")
					WindowGeometry.Height = int.Parse(dktOptions[Keyword_H]);
			}
			catch (FormatException)
			{
				WindowGeometry = new Rectangle(0, 0, 0, 0);
			}

			Hide = dktOptions[Keyword_Hide] != "";

			if (dktOptions[Keyword_Centered] != "")
			{
				CenterMainText = MemoryExtensions.Equals(dktOptions[Keyword_Centered].AsSpan(0, 1), "1", StringComparison.Ordinal);
				CenterSubText  = MemoryExtensions.Equals(dktOptions[Keyword_Centered].AsSpan(1, 1), "1", StringComparison.Ordinal);
			}

			try
			{
				if (dktOptions[Keyword_ZX] != "")
					ObjectGeometry.X = int.Parse(dktOptions[Keyword_ZX]);

				if (dktOptions[Keyword_ZY] != "")
					ObjectGeometry.Y = int.Parse(dktOptions[Keyword_ZY]);

				if (dktOptions[Keyword_ZW] != "")
					ObjectGeometry.Width = int.Parse(dktOptions[Keyword_ZW]);

				if (dktOptions[Keyword_ZH] != "")
					ObjectGeometry.Height = int.Parse(dktOptions[Keyword_ZH]);
			}
			catch (FormatException)
			{
				ObjectGeometry = new Rectangle(0, 0, 0, 0);
			}
		}

		public void ParseGuiID(string Param1)
		{
			var regExIsGUIID = new Regex("([0-9]{1,}):");
			var regExGUIID = new Regex("([0-9]*):(.*)");

			if (regExIsGUIID.IsMatch(Param1))
			{
				GUIID = int.Parse(regExGUIID.Match(Param1).Groups[1].Captures[0].ToString());
				this.Param1 = regExGUIID.Match(Param1).Groups[2].Captures[0].ToString();
			}
			else
			{
				GUIID = 1;
				this.Param1 = Param1;
			}
		}
	}
}