using System.Collections.Generic;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private List<object> Dissect(List<object> parts, int start, int end)
		{
			var extracted = new List<object>(end - start);

			for (var i = start; i < end; i++)
			{
				extracted.Add(parts[start]);
				parts.RemoveAt(start);
			}

			return extracted;
		}
	}
}