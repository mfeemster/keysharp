namespace Keysharp.Core.Common.Mapper
{
	internal sealed class MappingService
	{
		internal readonly DriveTypeMapper DriveType;

		internal static MappingService Instance { get; } = new MappingService();

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static MappingService() { }

		private MappingService() =>
		// add here all mapping providers here
		DriveType = new DriveTypeMapper();
	}
}