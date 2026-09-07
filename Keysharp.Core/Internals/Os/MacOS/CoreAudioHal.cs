#if OSX
namespace Keysharp.Internals.Os.MacOS
{
	/// <summary>
	/// The Core Audio HAL property accessors, shared by everything that reads or writes a device property. The
	/// HAL exposes one generic get/set pair over an opaque address, so every caller needs the same handful of
	/// typed wrappers around it; keeping one copy is what stops the Sound functions and the Audio class drifting
	/// apart on details like who owns a returned CFString.
	/// <para>
	/// Every framework entry point is reached by absolute path, and every wrapper returns the raw OSStatus so a
	/// caller can classify a failure rather than have it thrown at them.
	/// </para>
	/// </summary>
	internal static class CoreAudioHal
	{
		internal const string CoreAudio = "/System/Library/Frameworks/CoreAudio.framework/CoreAudio";
		internal const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

		internal const uint kAudioObjectSystemObject = 1u;
		internal const uint kAudioObjectPropertyScopeGlobal = 0x676C6F62u;   // 'glob'
		internal const uint kAudioObjectPropertyScopeOutput = 0x6F757470u;   // 'outp'
		internal const uint kAudioObjectPropertyScopeInput = 0x696E7074u;    // 'inpt'
		internal const uint kAudioObjectPropertyElementMain = 0u;
		internal const uint kAudioObjectPropertyName = 0x6C6E616Du;          // 'lnam'
		internal const uint kAudioDevicePropertyStreamConfiguration = 0x736C6179u;   // 'slay'

		[StructLayout(LayoutKind.Sequential)]
		internal struct AudioObjectPropertyAddress
		{
			public uint mSelector;
			public uint mScope;
			public uint mElement;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct CFRange
		{
			public long location;
			public long length;
		}

		[DllImport(CoreAudio)]
		internal static extern int AudioObjectGetPropertyData(uint objectId, ref AudioObjectPropertyAddress addr, uint qualifierSize, nint qualifierData, ref uint dataSize, nint outData);

		[DllImport(CoreAudio)]
		internal static extern int AudioObjectSetPropertyData(uint objectId, ref AudioObjectPropertyAddress addr, uint qualifierSize, nint qualifierData, uint dataSize, nint inData);

		[DllImport(CoreAudio)]
		internal static extern int AudioObjectGetPropertyDataSize(uint objectId, ref AudioObjectPropertyAddress addr, uint qualifierSize, nint qualifierData, out uint outDataSize);

		[DllImport(CoreFoundation)]
		internal static extern long CFStringGetLength(nint cfStr);

		[DllImport(CoreFoundation)]
		internal static extern void CFStringGetCharacters(nint cfStr, CFRange range, nint buffer);

		[DllImport(CoreFoundation)]
		internal static extern nint CFStringCreateWithCharacters(nint allocator, nint chars, long numChars);

		[DllImport(CoreFoundation)]
		internal static extern void CFRelease(nint cfTypeRef);

		internal static AudioObjectPropertyAddress Address(uint selector, uint scope, uint element = kAudioObjectPropertyElementMain) => new ()
		{
			mSelector = selector,
			mScope = scope,
			mElement = element,
		};

		internal static int GetPropertyFloat(uint objectId, AudioObjectPropertyAddress addr, out float value)
		{
			var ptr = Marshal.AllocHGlobal(sizeof(float));

			try
			{
				uint size = sizeof(float);
				var result = AudioObjectGetPropertyData(objectId, ref addr, 0, nint.Zero, ref size, ptr);
				value = result == 0 ? BitConverter.Int32BitsToSingle(Marshal.ReadInt32(ptr)) : 0f;
				return result;
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		internal static int SetPropertyFloat(uint objectId, AudioObjectPropertyAddress addr, float value)
		{
			var ptr = Marshal.AllocHGlobal(sizeof(float));

			try
			{
				Marshal.WriteInt32(ptr, BitConverter.SingleToInt32Bits(value));
				return AudioObjectSetPropertyData(objectId, ref addr, 0, nint.Zero, sizeof(float), ptr);
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		internal static int GetPropertyUInt(uint objectId, AudioObjectPropertyAddress addr, out uint value)
		{
			var ptr = Marshal.AllocHGlobal(sizeof(uint));

			try
			{
				uint size = sizeof(uint);
				var result = AudioObjectGetPropertyData(objectId, ref addr, 0, nint.Zero, ref size, ptr);
				value = result == 0 ? (uint)Marshal.ReadInt32(ptr) : 0u;
				return result;
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		internal static int SetPropertyUInt(uint objectId, AudioObjectPropertyAddress addr, uint value)
		{
			var ptr = Marshal.AllocHGlobal(sizeof(uint));

			try
			{
				Marshal.WriteInt32(ptr, (int)value);
				return AudioObjectSetPropertyData(objectId, ref addr, 0, nint.Zero, sizeof(uint), ptr);
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		internal static uint[] GetPropertyUInts(uint objectId, AudioObjectPropertyAddress addr)
		{
			if (AudioObjectGetPropertyDataSize(objectId, ref addr, 0, nint.Zero, out var dataSize) != 0 || dataSize == 0)
				return [];

			var ptr = Marshal.AllocHGlobal((int)dataSize);

			try
			{
				if (AudioObjectGetPropertyData(objectId, ref addr, 0, nint.Zero, ref dataSize, ptr) != 0)
					return [];

				var count = (int)(dataSize / sizeof(uint));
				var result = new uint[count];

				for (var i = 0; i < count; i++)
					result[i] = (uint)Marshal.ReadInt32(ptr, i * sizeof(uint));

				return result;
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		internal static string GetPropertyString(uint objectId, AudioObjectPropertyAddress addr)
		{
			var ptrSize = nint.Size;
			var cfStrHolder = Marshal.AllocHGlobal(ptrSize);

			try
			{
				var size = (uint)ptrSize;

				if (AudioObjectGetPropertyData(objectId, ref addr, 0, nint.Zero, ref size, cfStrHolder) != 0)
					return "";

				var cfStr = Marshal.ReadIntPtr(cfStrHolder);

				if (cfStr == nint.Zero)
					return "";

				try
				{
					var len = CFStringGetLength(cfStr);

					if (len <= 0)
						return "";

					var charBuf = Marshal.AllocHGlobal((int)(len * 2));

					try
					{
						CFStringGetCharacters(cfStr, new CFRange { location = 0, length = len }, charBuf);
						return Marshal.PtrToStringUni(charBuf, (int)len) ?? "";
					}
					finally
					{
						Marshal.FreeHGlobal(charBuf);
					}
				}
				finally
				{
					// A CFString from a Get property arrives with a reference this caller owns.
					CFRelease(cfStr);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(cfStrHolder);
			}
		}

		/// <summary>
		/// Total channels the device carries in one direction, which is also how its kind is decided. The
		/// property is an AudioBufferList: a UInt32 buffer count, then 16-byte AudioBuffer entries starting at
		/// offset 8, each opening with its own channel count.
		/// </summary>
		internal static int GetChannelCount(uint deviceId, uint scope)
		{
			var addr = Address(kAudioDevicePropertyStreamConfiguration, scope);

			if (AudioObjectGetPropertyDataSize(deviceId, ref addr, 0, nint.Zero, out var dataSize) != 0 || dataSize < 4)
				return 0;

			var ptr = Marshal.AllocHGlobal((int)dataSize);

			try
			{
				if (AudioObjectGetPropertyData(deviceId, ref addr, 0, nint.Zero, ref dataSize, ptr) != 0)
					return 0;

				var buffers = (uint)Marshal.ReadInt32(ptr);
				var total = 0;

				for (var i = 0; i < buffers; i++)
				{
					var entry = 8 + (i * 16);

					if (entry + 4 > dataSize)
						break;

					total += Marshal.ReadInt32(ptr, entry);
				}

				return total;
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}
	}
}
#endif
