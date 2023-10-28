using System;
using System.Collections.Generic;
using System.Text;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Threading;

namespace Keysharp.Core.Common.Input
{
	public class InputObject : KeysharpObject
	{
		internal InputType input;
		private IFuncObj onChar, onEnd, onKeyDown, onKeyUp;

		public object BackspaceIsUndo
		{
			get => input.BackspaceIsUndo;
			set => input.BackspaceIsUndo = value.Ab();
		}

		public object CaseSensitive
		{
			get => input.CaseSensitive;
			set => input.CaseSensitive = value.Ab();
		}

		public string EndKey
		{
			get
			{
				if (input.Status == InputStatusType.TerminatedByEndKey)
				{
					var str = "";
					_ = input.GetEndReason(ref str);
					return str;
				}

				return "";
			}
		}

		public string EndMods
		{
			get
			{
				var sb = new StringBuilder(8);

				for (var i = 0; i < 8; ++i)
					if ((input.EndingMods & (1 << i)) != 0)
					{
						_ = sb.Append(KeyboardMouseSender.ModLRString[i * 2]);
						_ = sb.Append(KeyboardMouseSender.ModLRString[(i * 2) + 1]);
					}

				return sb.ToString();
			}
		}

		public string EndReason
		{
			get
			{
				string str = null;
				return input.GetEndReason(ref str);
			}
		}

		public object FindAnywhere
		{
			get => input.FindAnywhere;
			set => input.FindAnywhere = value.Ab();
		}

		public bool InProgress => input.InProgress();

		public string Input => input.buffer;

		public string Match
		{
			get
			{
				return input.Status == InputStatusType.TerminatedByMatch && input.EndingMatchIndex < input.match.Count
					   ? input.match[input.EndingMatchIndex]
					   : "";
			}
		}

		public object MinSendLevel
		{
			get => (long)input.MinSendLevel;

			set
			{
				var val = value.Al();

				if (val < 0 || val > 101)
					throw new ValueError($"Cannot set InputObject.MinSendLevel to a value outside of the range 0 - 101 ({value}).");

				input.MinSendLevel = (uint)val;
			}
		}

		public object NotifyNonText
		{
			get => input.NotifyNonText;
			set => input.NotifyNonText = value.Ab();
		}

		public object OnChar
		{
			get => onChar;
			set => onChar = Keysharp.Core.Function.GetFuncObj(value, null, true);
		}

		public object OnEnd
		{
			get => onEnd;
			set => onEnd = Keysharp.Core.Function.GetFuncObj(value, null, true);
		}

		public object OnKeyDown
		{
			get => onKeyDown;
			set => onKeyDown = Keysharp.Core.Function.GetFuncObj(value, null, true);
		}

		public object OnKeyUp
		{
			get => onKeyUp;
			set => onKeyUp = Keysharp.Core.Function.GetFuncObj(value, null, true);
		}

		public object Timeout
		{
			get => input.Timeout / 1000.0;

			set
			{
				input.Timeout = (int)(value.ParseDouble() * 1000);

				if (input.InProgress() && input.Timeout > 0)
					input.SetTimeoutTimer();
			}
		}

		public object VisibleNonText
		{
			get => input.VisibleNonText;
			set => input.VisibleNonText = value.Ab();
		}

		public object VisibleText
		{
			get => input.VisibleText;
			set => input.VisibleText = value.Ab();
		}

		public InputObject(string options, string endKeys, string matchList) => input = new InputType(this, options, endKeys, matchList);

		public void KeyOpt(object obj0, object obj1)
		{
			var keys = obj0.As();
			var options = obj1.As();
			var adding = true;
			uint flag, addFlags = 0u, removeFlags = 0u;

			for (var i = 0; i < options.Length; ++i)
			{
				switch (char.ToUpper(options[i]))
				{
					case '+': adding = true; continue;

					case '-': adding = false; continue;

					case ' ': case '\t': continue;

					case 'E': flag = HookThread.END_KEY_ENABLED; break;

					case 'I': flag = HookThread.INPUT_KEY_IGNORE_TEXT; break;

					case 'N': flag = HookThread.INPUT_KEY_NOTIFY; break;

					case 'S':
						flag = HookThread.INPUT_KEY_SUPPRESS;

						if (adding)
							removeFlags |= HookThread.INPUT_KEY_VISIBLE;

						break;

					case 'V':
						flag = HookThread.INPUT_KEY_VISIBLE;

						if (adding)
							removeFlags |= HookThread.INPUT_KEY_SUPPRESS;

						break;

					case 'Z': // Zero (reset)
						addFlags = 0;
						removeFlags = HookThread.INPUT_KEY_OPTION_MASK;
						continue;

					default:
						throw new ValueError("Invalid option.", options);
				}

				if (adding)
					addFlags |= flag; // Add takes precedence over remove, so remove_flags isn't changed.
				else
				{
					removeFlags |= flag;
					addFlags &= ~flag; // Override any previous add.
				}
			}

			if (string.Compare(keys, "{All}", true) == 0)
			{
				// Could optimize by using memset() when remove_flags == 0xFF, but that doesn't seem
				// worthwhile since this mode is already faster than SetKeyFlags() with a single key.
				for (var i = 0; i < input.KeyVK.Length; ++i)
					input.KeyVK[i] = (input.KeyVK[i] & ~removeFlags) | addFlags;

				for (var i = 0; i < input.KeySC.Length; ++i)
					input.KeySC[i] = (input.KeySC[i] & ~removeFlags) | addFlags;
			}

			input.SetKeyFlags(keys, false, removeFlags, addFlags);
		}

		public void Start()
		{
			if (!input.InProgress())
			{
				input.buffer = "";
				input.InputStart();
			}
		}

		public void Stop()
		{
			if (input.InProgress())
				input.Stop();
		}

		public void Wait(object obj)
		{
			var ms = obj.Ad(double.MaxValue) * 1000.0;
			var tickStart = DateTime.Now;

			while (input.InProgress() && (DateTime.Now - tickStart).TotalMilliseconds < ms)
				Keysharp.Core.Flow.Sleep(20);
		}
	}
}