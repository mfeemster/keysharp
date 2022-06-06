using System;
using System.Text;
using System.Windows.Forms;

namespace Keysharp.Core
{
	internal class HotkeyBox : TextBox
	{
		private Keys key, mod;

		public Limits Limit { get; set; }

		public HotkeyBox()
		{
			key = mod = Keys.None;
			Limit = Limits.None;
			Multiline = false;
			ContextMenuStrip = new ContextMenuStrip();
			Text = Enum.GetName(typeof(Keys), key);
			KeyPress += (sender, e) =>
			{
				e.Handled = true;
			};
			KeyUp += (sender, e) =>
			{
				if (e.KeyCode == Keys.None && e.Modifiers == Keys.None)
					key = Keys.None;
			};
			KeyDown += (sender, e) =>
			{
				if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
				{
					key = mod = Keys.None;
				}
				else
				{
					key = e.KeyCode;
					mod = e.Modifiers;
					Validate();
				}

				SetText();
			};
			PreviewKeyDown += (sender, e) =>
			{
				if (e.KeyCode == Keys.Tab)
					e.IsInputKey = true;
			};
		}

		public string GetText()
		{
			var str = "";

			if ((mod & Keys.Control) == Keys.Control)
				str += Core.Keyword_ModifierCtrl;

			if ((mod & Keys.Shift) == Keys.Shift)
				str += Core.Keyword_ModifierShift;

			if ((mod & Keys.Alt) == Keys.Alt)
				str += Core.Keyword_ModifierAlt;

			return str + key.ToString();
		}

		internal void SetText(string text)
		{
			Keys keys = Keys.None, mods = Keys.None;

			foreach (var ch in text)
			{
				switch (ch)
				{
					case Core.Keyword_ModifierAlt: mods |= Keys.Alt; break;

					case Core.Keyword_ModifierCtrl: mods |= Keys.Control; break;

					case Core.Keyword_ModifierShift: mods |= Keys.Shift; break;

					default:
					{
						if (Enum.TryParse(ch.ToString(), true, out Keys k))
							keys = k;

						break;
					}
				}
			}

			key = keys;
			mod = mods;
			Validate();
			SetText();
		}

		private void SetText()
		{
			var buf = new StringBuilder(45);
			const string sep = " + ";

			if ((mod & Keys.Control) == Keys.Control)
			{
				_ = buf.Append(Enum.GetName(typeof(Keys), Keys.Control));
				_ = buf.Append(sep);
			}

			if ((mod & Keys.Shift) == Keys.Shift)
			{
				_ = buf.Append(Enum.GetName(typeof(Keys), Keys.Shift));
				_ = buf.Append(sep);
			}

			if ((mod & Keys.Alt) == Keys.Alt)
			{
				_ = buf.Append(Enum.GetName(typeof(Keys), Keys.Alt));
				_ = buf.Append(sep);
			}

			_ = buf.Append(key.ToString());
			Text = buf.ToString();
		}

		private void Validate()
		{
			Keys[,] sym = { { Keys.Control, Keys.ControlKey }, { Keys.Shift, Keys.ShiftKey }, { Keys.Alt, Keys.Menu } };

			for (var i = 0; i < 3; i++)
			{
				if (key == sym[i, 1] && (mod & sym[i, 0]) == sym[i, 0])
					mod &= ~sym[i, 0];
			}

			if ((Limit & Limits.PreventUnmodified) == Limits.PreventUnmodified)
			{
				if (mod == Keys.None)
					key = Keys.None;
			}

			if ((Limit & Limits.PreventShiftOnly) == Limits.PreventShiftOnly)
			{
				if (mod == Keys.Shift)
					key = mod = Keys.None;
			}

			if ((Limit & Limits.PreventControlOnly) == Limits.PreventControlOnly)
			{
				if (mod == Keys.Control)
					key = mod = Keys.None;
			}

			if ((Limit & Limits.PreventAltOnly) == Limits.PreventAltOnly)
			{
				if (mod == Keys.Alt)
					key = mod = Keys.None;
			}

			if ((Limit & Limits.PreventShiftControl) == Limits.PreventShiftControl)
			{
				if ((mod & Keys.Shift) == Keys.Shift && (mod & Keys.Control) == Keys.Control && (mod & Keys.Alt) != Keys.Alt)
					key = mod = Keys.None;
			}

			if ((Limit & Limits.PreventShiftAlt) == Limits.PreventShiftAlt)
			{
				if ((mod & Keys.Shift) == Keys.Shift && (mod & Keys.Control) != Keys.Control && (mod & Keys.Control) == Keys.Alt)
					key = mod = Keys.None;
			}

			if ((Limit & Limits.PreventShiftControlAlt) == Limits.PreventShiftControlAlt)
			{
				if ((mod & Keys.Shift) == Keys.Shift && (mod & Keys.Control) == Keys.Control && (mod & Keys.Control) == Keys.Alt)
					key = mod = Keys.None;
			}
		}

		[Flags]
		public enum Limits
		{
			None = 0,
			PreventUnmodified = 1,
			PreventShiftOnly = 2,
			PreventControlOnly = 4,
			PreventAltOnly = 8,
			PreventShiftControl = 16,
			PreventShiftAlt = 32,
			PreventShiftControlAlt = 128,
		}
	}
}