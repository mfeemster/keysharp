namespace Keysharp.Core.Common.Images
{
	internal sealed class GdiHandleHolder : KeysharpObject
	{
		private readonly bool disposeHandle = true;
		private readonly nint handle;

		public new (Type, object) super => (typeof(KeysharpObject), this);

		internal GdiHandleHolder(nint h, bool d)
		{
			handle = h;
			disposeHandle = d;
		}

		~GdiHandleHolder()
		{
#if WINDOWS

			if (disposeHandle && handle != 0)
				_ = WindowsAPI.DeleteObject(handle);//Windows specific, figure out how to do this, or if it's even needed on other platforms.//TODO

#endif
		}

		public static implicit operator long(GdiHandleHolder holder) => holder.handle.ToInt64();

		public override string ToString() => handle.ToInt64().ToString();
	}
}
