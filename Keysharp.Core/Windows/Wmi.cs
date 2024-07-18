#if WINDOWS
namespace Keysharp.Core.Windows
{
	internal class Wmi
	{
		/// <summary>
		/// Gotten from https://stackoverflow.com/questions/4084402/get-hard-disk-serial-number
		/// </summary>
		/// <param name="wmiClass">The Windows Management class to retrieve a property from</param>
		/// <param name="wmiProperty">The property to retrieve from the Windows Management class</param>
		/// <param name="search">Optional WQL query to limit the search to specific criteria. Default: empty.</param>
		/// <returns>The value of the property</returns>
		internal static string Identifier(string wmiClass, string wmiProperty, string search = "")
		{
			var result = "";
			ManagementObjectCollection moc;

			if (string.IsNullOrEmpty(search))
			{
				var mc = new ManagementClass(wmiClass);
				moc = mc.GetInstances();
			}
			else
			{
				var mos = new ManagementObjectSearcher(search);
				moc = mos.Get();
			}

			foreach (var mo in moc)
			{
				if (result?.Length == 0)
				{
					try
					{
						result = mo[wmiProperty].ToString();
						break;//Only get the first one
					}
					catch
					{
					}
				}
			}

			return result;
		}
	}
}
#endif