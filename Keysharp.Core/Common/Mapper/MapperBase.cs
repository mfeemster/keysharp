using System;
using System.Collections.Generic;

namespace Keysharp.Core.Common.Mapper
{
	internal class MapperBase<T> where T : struct, IConvertible
	{
		static protected Dictionary<T, string> clrMappingTable = new Dictionary<T, string>();

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

		internal virtual string LookUpIAType(T clrType)
		{
			if (clrMappingTable.ContainsKey(clrType))
				return clrMappingTable[clrType];
			else
				return "";
		}

		internal virtual void SetUpMappingTable()
		{
			//
		}
	}
}