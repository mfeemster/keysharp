namespace System
{
	public static class SystemExtensions
	{
		public static bool IsNullOrEmpty(this object obj) => obj == null ? true : obj is string s ? s?.Length == 0 : false;
	}
}