using System;

namespace Keysharp.Core
{
	public partial class Core
	{
		public const bool Debug =
#if DEBUG
			true
#else
			false
#endif
			;
	}
}