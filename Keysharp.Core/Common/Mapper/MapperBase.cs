namespace Keysharp.Core.Common.Mapper
{
	internal class MapperBase<T> where T : struct, IConvertible
	{
		protected static Dictionary<T, string> clrMappingTable = [];

		internal MapperBase() => SetUpMappingTable();

		internal virtual T? LookUpCLRType(string keyword)
		{
			T? res = null;

			foreach (var kv in clrMappingTable)
			{
				if (kv.Value == keyword)
				{
					res = kv.Key;
					break;
				}
			}

			return res;
		}

		internal virtual string LookUpKeysharpType(T clrType) => clrMappingTable.TryGetValue(clrType, out var val) ? val : "";

		internal virtual void SetUpMappingTable()
		{
		}
	}
}