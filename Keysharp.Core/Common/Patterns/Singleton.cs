using System;

namespace Keysharp.Core.Common.Patterns
{
	/// <summary>
	/// generic for singletons
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class Singleton<T> where T : new ()
	{
		public static T Instance => SingletonCreator.instance;
		// ctor
		protected Singleton()
		{
			if (Instance != null)
			{
				throw (new Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
			}
		}
		private class SingletonCreator
		{
			internal static readonly T instance = new T();

			static SingletonCreator()
			{
			}
		}
	}

	internal class ScopeHelper
	{
		private object obj;
		public EventHandler<object> eh;

		public ScopeHelper(object o)
		{
			obj = o;
		}

		~ScopeHelper()
		{
			eh?.Invoke(this, obj);
		}
	}

	/// <summary>
	/// Gotten from https://stackoverflow.com/questions/537573/how-to-get-intptr-from-byte-in-c-sharp/537652 and
	/// https://blog.benoitblanchon.fr/safehandle/
	/// </summary>
	//internal class AutoPinner : SafeHandle
	//{
	//  GCHandle pinnedArray;
	//
	//  public override bool IsInvalid => pinnedArray.AddrOfPinnedObject().ToInt64() > 0 && pinnedArray.IsAllocated;
	//
	//  public AutoPinner(object obj)
	//      : base(IntPtr.Zero, true)
	//  {
	//      pinnedArray = GCHandle.Alloc(obj, GCHandleType.Pinned);
	//      SetHandle(pinnedArray.AddrOfPinnedObject());
	//  }
	//
	//  public static implicit operator IntPtr(AutoPinner ap) => ap.pinnedArray.AddrOfPinnedObject();
	//
	//  protected override bool ReleaseHandle()
	//  {
	//      pinnedArray.Free();
	//      return true;
	//  }
	//}
}