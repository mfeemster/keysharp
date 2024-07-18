#if LINUX
namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of StatusBar for the linux platfrom.
	/// </summary>
	internal class StatusBar : Keysharp.Core.Common.Window.StatusBarBase
	{
		internal StatusBar(IntPtr hWnd)
			: base(hWnd)
		{
		}

		//May need to add wait functionality here the way AHK does in StatusBarUtil().
		protected override string GetCaption(uint index)
		{
			throw new NotImplementedException();
		}

		protected override int GetOwningPid()
		{
			throw new NotImplementedException();
		}

		protected override int GetPanelCount()
		{
			throw new NotImplementedException();
		}
	}
}

#endif