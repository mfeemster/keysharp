namespace Keysharp.Scripting
{
	public partial class Parser
	{
		//internal static bool persistent;

		private void CheckPersistent(string name)
		{
			if (Persistent)
				return;

			switch (name.ToLowerInvariant())
			{
				case "settimer":
				case "menu":
				case "hotkey":
				case "hotstring":
				case "onmessage":
				case "onclipboardchange":
				case "gui":
				case "persistent":
					//case "inputhook":
					Persistent = true;
					break;
			}
		}
	}
}