using System.IO;

namespace Keysharp.Core.Common.Mapper
{
	/// <summary>
	/// Maps DriveTypes
	/// </summary>
	internal class DriveTypeMapper : MapperBase<DriveType>
	{
		internal override DriveType? LookUpCLRType(string keyword) => base.LookUpCLRType(keyword);

		internal override string LookUpKeysharpType(DriveType clrType)
		{
			var str = base.LookUpKeysharpType(clrType);
			return string.IsNullOrEmpty(str) ? Core.Keyword_UNKNOWN : str;
		}

		internal override void SetUpMappingTable()
		{
			clrMappingTable.Add(DriveType.CDRom, Core.Keyword_CDROM);
			clrMappingTable.Add(DriveType.Fixed, Core.Keyword_FIXED);
			clrMappingTable.Add(DriveType.Network, Core.Keyword_NETWORK);
			clrMappingTable.Add(DriveType.Ram, Core.Keyword_RAMDISK);
			clrMappingTable.Add(DriveType.Removable, Core.Keyword_REMOVABLE);
			clrMappingTable.Add(DriveType.Unknown, Core.Keyword_UNKNOWN);
			clrMappingTable.Add(DriveType.NoRootDirectory, Core.Keyword_UNKNOWN);
		}
	}
}