using System.Collections.Generic;
using System;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		//internal static bool persistent;
		private HashSet<string> persistentTerms = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"settimer",
			"menu",
			"hotkey",
			"hotstring",
			"onmessage",
			"onclipboardchange",
			"gui",
			"persistent"
		};

		private void CheckPersistent(string name)
		{
			if (Persistent)
				return;

			if (persistentTerms.Contains(name))
				Persistent = true;
		}
	}
}