using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// This code was taken from a project named NAudio which is located at https://github.com/naudio/NAudio
/// It consists of only the parts needed to make certain Keysharp sound functions work
/// The rest is omitted to keep the size of Keysharp as small as possible.
///
/// LICENSE
/// -------
/// Copyright(C) 2007 Ray Molenkamp
///
/// This source code is provided 'as-is', without any express or implied
/// warranty.In no event will the authors be held liable for any damages
/// arising from the use of this source code or the software it produces.
///
/// Permission is granted to anyone to use this source code for any purpose,
/// including commercial applications, and to alter it and redistribute it
/// freely, subject to the following restrictions:
///
/// 1. The origin of this source code must not be misrepresented; you must not
/// claim that you wrote the original source code.If you use this source code
///
///  in a product, an acknowledgment in the product documentation would be
///  appreciated but is not required.
/// 2. Altered source versions must be plainly marked as such, and must not be
///
///  misrepresented as being the original source code.
/// 3. This notice may not be removed or altered from any source distribution.
/// </summary>
namespace Keysharp.Core.Windows
{
	/// <summary>
	/// IMMNotificationClient
	/// </summary>
	[Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMNotificationClient
	{
		/// <summary>
		/// Device State Changed
		/// </summary>
		void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.I4)] DeviceState newState);

		/// <summary>
		/// Device Added
		/// </summary>
		void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);

		/// <summary>
		/// Device Removed
		/// </summary>
		void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

		/// <summary>
		/// Default Device Changed
		/// </summary>
		void OnDefaultDeviceChanged(DataFlow flow, Role role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

		/// <summary>
		/// Property Value Changed
		/// </summary>
		/// <param name="pwstrDeviceId"></param>
		/// <param name="key"></param>
		void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, PropertyKey key);
	}

	[Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioEndpointVolume
	{
		int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
		int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
		int GetChannelCount(out int pnChannelCount);
		int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
		int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
		int GetMasterVolumeLevel(out float pfLevelDB);
		int GetMasterVolumeLevelScalar(out float pfLevel);
		int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
		int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
		int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
		int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
		int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
		int GetMute(out bool pbMute);
		int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
		int VolumeStepUp(ref Guid pguidEventContext);
		int VolumeStepDown(ref Guid pguidEventContext);
		int QueryHardwareSupport(out uint pdwHardwareSupportMask);
		int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
	}

	[Guid("657804FA-D6AD-4496-8A60-352752AF4F89"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioEndpointVolumeCallback
	{
		void OnNotify(IntPtr notifyData);
	};

	[Guid("D666063F-1587-4E43-81F1-B948E807363F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMDevice
	{
		// activationParams is a propvariant
		int Activate(ref Guid id, ClsCtx clsCtx, IntPtr activationParams,
					 [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

		int OpenPropertyStore(StorageAccessMode stgmAccess, out IPropertyStore properties);

		int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

		int GetState(out DeviceState state);
	}

	[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMDeviceCollection
	{
		int GetCount(out int numDevices);
		int Item(int deviceNumber, out IMMDevice device);
	}

	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMDeviceEnumerator
	{
		int EnumAudioEndpoints(DataFlow dataFlow, DeviceState stateMask,
							   out IMMDeviceCollection devices);

		[PreserveSig]
		int GetDefaultAudioEndpoint(DataFlow dataFlow, Role role, out IMMDevice endpoint);

		int GetDevice(string id, out IMMDevice deviceName);

		int RegisterEndpointNotificationCallback(IMMNotificationClient client);

		int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
	}

	[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IPropertyStore
	{
		int GetCount(out int propCount);
		int GetAt(int property, out PropertyKey key);
		int GetValue(ref PropertyKey key, out PropVariant value);
		int SetValue(ref PropertyKey key, ref PropVariant value);
		int Commit();
	}

	internal struct AudioVolumeNotificationDataStruct
	{
		internal Guid guidEventContext;
		internal bool bMuted;
		internal float fMasterVolume;
		internal uint nChannels;
		internal float ChannelVolume;
	}

	/// <summary>
	/// Property Keys
	/// </summary>
	internal static class PropertyKeys
	{
		/// <summary>
		/// PKEY_AudioEndpoint_Association
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_Association = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 2);

		/// <summary>
		/// PKEY_AudioEndpoint_ControlPanelPageProvider
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_ControlPanelPageProvider = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 1);

		/// <summary>
		/// PKEY_AudioEndpoint_Disable_SysFx
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_Disable_SysFx = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 5);

		/// <summary>
		/// PKEY_AudioEndpoint_FormFactor
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_FormFactor = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 0);

		/// <summary>
		/// PKEY_AudioEndpoint_FullRangeSpeakers
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_FullRangeSpeakers = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 6);

		/// <summary>
		/// PKEY_AudioEndpoint_GUID
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_GUID = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 4);

		/// <summary>
		/// PKEY_AudioEndpoint_JackSubType
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_JackSubType = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 8);

		/// <summary>
		/// PKEY_AudioEndpoint_PhysicalSpeakers
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_PhysicalSpeakers = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 3);

		/// <summary>
		/// PKEY_AudioEndpoint_Supports_EventDriven_Mode
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEndpoint_Supports_EventDriven_Mode = new PropertyKey(new Guid(0x1da5d803, unchecked((short)0xd492), 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 7);

		/// <summary>
		/// PKEY_AudioEngine_DeviceFormat
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEngine_DeviceFormat = new PropertyKey(new Guid(unchecked((int)0xf19f064d), 0x82c, 0x4e27, 0xbc, 0x73, 0x68, 0x82, 0xa1, 0xbb, 0x8e, 0x4c), 0);

		/// <summary>
		/// PKEY_AudioEngine_OEMFormat
		/// </summary>
		internal static readonly PropertyKey PKEY_AudioEngine_OEMFormat = new PropertyKey(new Guid(unchecked((int)0xe4870e26), 0x3cc5, 0x4cd2, 0xba, 0x46, 0xca, 0xa, 0x9a, 0x70, 0xed, 0x4), 3);

		/// <summary>
		/// Id of controller device for endpoint device property.
		/// </summary>
		internal static readonly PropertyKey PKEY_Device_ControllerDeviceId = new PropertyKey(new Guid(unchecked((int)0xb3f8fa53), unchecked((short)0x0004), 0x438e, 0x90, 0x03, 0x51, 0xa4, 0x6e, 0x13, 0x9b, 0xfc), 2);

		/// <summary>
		/// Device description property.
		/// </summary>
		internal static readonly PropertyKey PKEY_Device_DeviceDesc = new PropertyKey(new Guid(unchecked((int)0xa45c254e), unchecked((short)0xdf1c), 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 2);

		/// <summary>
		/// PKEY _Devie_FriendlyName
		/// </summary>
		internal static readonly PropertyKey PKEY_Device_FriendlyName = new PropertyKey(new Guid(unchecked((int)0xa45c254e), unchecked((short)0xdf1c), 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 14);

		/// <summary>
		/// PKEY _Device_IconPath
		/// </summary>
		internal static readonly PropertyKey PKEY_Device_IconPath = new PropertyKey(new Guid(unchecked((int)0x259abffc), unchecked((short)0x50a7), 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 12);

		/// <summary>
		/// System-supplied device instance identification string, assigned by PnP manager, persistent across system restarts.
		/// </summary>
		internal static readonly PropertyKey PKEY_Device_InstanceId = new PropertyKey(new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57), 256);

		/// <summary>
		/// Device interface key property.
		/// </summary>
		internal static readonly PropertyKey PKEY_Device_InterfaceKey = new PropertyKey(new Guid(unchecked((int)0x233164c8), unchecked((short)0x1b2c), 0x4c7d, 0xbc, 0x68, 0xb6, 0x71, 0x68, 0x7a, 0x25, 0x67), 1);

		/// <summary>
		/// PKEY_DeviceInterface_FriendlyName
		/// </summary>
		internal static readonly PropertyKey PKEY_DeviceInterface_FriendlyName = new PropertyKey(new Guid(0x026e516e, unchecked((short)0xb814), 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22), 2);
	}

	/// <summary>
	/// Audio Endpoint Volume
	/// </summary>
	internal class AudioEndpointVolume : IDisposable
	{
		private readonly IAudioEndpointVolume audioEndPointVolume;
		private AudioEndpointVolumeCallback callBack;

		private Guid notificationGuid = Guid.Empty;

		/// <summary>
		/// Channels
		/// </summary>
		internal AudioEndpointVolumeChannels Channels { get; }

		/// <summary>
		/// Hardware Support
		/// </summary>
		internal EEndpointHardwareSupport HardwareSupport { get; }

		/// <summary>
		/// Master Volume Level
		/// </summary>
		internal float MasterVolumeLevel
		{
			get
			{
				Marshal.ThrowExceptionForHR(audioEndPointVolume.GetMasterVolumeLevel(out var result));
				return result;
			}

			set => Marshal.ThrowExceptionForHR(audioEndPointVolume.SetMasterVolumeLevel(value, ref notificationGuid));
		}

		/// <summary>
		/// Master Volume Level Scalar
		/// </summary>
		internal float MasterVolumeLevelScalar
		{
			get
			{
				Marshal.ThrowExceptionForHR(audioEndPointVolume.GetMasterVolumeLevelScalar(out var result));
				return result;
			}

			set => Marshal.ThrowExceptionForHR(audioEndPointVolume.SetMasterVolumeLevelScalar(value, ref notificationGuid));
		}

		/// <summary>
		/// Mute
		/// </summary>
		internal bool Mute
		{
			get
			{
				Marshal.ThrowExceptionForHR(audioEndPointVolume.GetMute(out var result));
				return result;
			}

			set => Marshal.ThrowExceptionForHR(audioEndPointVolume.SetMute(value, ref notificationGuid));
		}

		/// <summary>
		/// GUID to pass to AudioEndpointVolumeCallback
		/// </summary>
		internal Guid NotificationGuid
		{
			get => notificationGuid;
			set => notificationGuid = value;
		}

		/// <summary>
		/// Step Information
		/// </summary>
		internal AudioEndpointVolumeStepInformation StepInformation { get; }

		/// <summary>
		/// Volume Range
		/// </summary>
		internal AudioEndpointVolumeVolumeRange VolumeRange { get; }

		/// <summary>
		/// Creates a new Audio endpoint volume
		/// </summary>
		/// <param name="realEndpointVolume">IAudioEndpointVolume COM interface</param>
		internal AudioEndpointVolume(IAudioEndpointVolume realEndpointVolume)
		{
			audioEndPointVolume = realEndpointVolume;
			Channels = new AudioEndpointVolumeChannels(audioEndPointVolume);
			StepInformation = new AudioEndpointVolumeStepInformation(audioEndPointVolume);
			Marshal.ThrowExceptionForHR(audioEndPointVolume.QueryHardwareSupport(out var hardwareSupp));
			HardwareSupport = (EEndpointHardwareSupport)hardwareSupp;
			VolumeRange = new AudioEndpointVolumeVolumeRange(audioEndPointVolume);
			callBack = new AudioEndpointVolumeCallback(this);
			Marshal.ThrowExceptionForHR(audioEndPointVolume.RegisterControlChangeNotify(callBack));
		}

		/// <summary>
		/// Finalizer
		/// </summary>
		~AudioEndpointVolume()
		{
			Dispose();
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (callBack != null)
			{
				Marshal.ThrowExceptionForHR(audioEndPointVolume.UnregisterControlChangeNotify(callBack));
				callBack = null;
			}

			_ = Marshal.ReleaseComObject(audioEndPointVolume);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Volume Step Down
		/// </summary>
		internal void VolumeStepDown() => Marshal.ThrowExceptionForHR(audioEndPointVolume.VolumeStepDown(ref notificationGuid));

		/// <summary>
		/// Volume Step Up
		/// </summary>
		internal void VolumeStepUp() => Marshal.ThrowExceptionForHR(audioEndPointVolume.VolumeStepUp(ref notificationGuid));

		internal void FireNotification(AudioVolumeNotificationData notificationData) => OnVolumeNotification?.Invoke(notificationData);

		/// <summary>
		/// On Volume Notification
		/// </summary>
		internal event AudioEndpointVolumeNotificationDelegate OnVolumeNotification;
	}

	/// <summary>
	/// Audio Endpoint Volume Channel
	/// </summary>
	internal class AudioEndpointVolumeChannel
	{
		private readonly IAudioEndpointVolume audioEndpointVolume;
		private readonly uint channel;
		private Guid notificationGuid = Guid.Empty;

		/// <summary>
		/// GUID to pass to AudioEndpointVolumeCallback
		/// </summary>
		internal Guid NotificationGuid
		{
			get => notificationGuid;
			set => notificationGuid = value;
		}

		/// <summary>
		/// Volume Level
		/// </summary>
		internal float VolumeLevel
		{
			get
			{
				Marshal.ThrowExceptionForHR(audioEndpointVolume.GetChannelVolumeLevel(channel, out var result));
				return result;
			}

			set => Marshal.ThrowExceptionForHR(audioEndpointVolume.SetChannelVolumeLevel(channel, value, ref notificationGuid));
		}

		/// <summary>
		/// Volume Level Scalar
		/// </summary>
		internal float VolumeLevelScalar
		{
			get
			{
				Marshal.ThrowExceptionForHR(audioEndpointVolume.GetChannelVolumeLevelScalar(channel, out var result));
				return result;
			}

			set => Marshal.ThrowExceptionForHR(audioEndpointVolume.SetChannelVolumeLevelScalar(channel, value, ref notificationGuid));
		}

		internal AudioEndpointVolumeChannel(IAudioEndpointVolume parent, int channel)
		{
			this.channel = (uint)channel;
			audioEndpointVolume = parent;
		}
	}

	/// <summary>
	/// Audio Endpoint Volume Channels
	/// </summary>
	internal class AudioEndpointVolumeChannels
	{
		private readonly IAudioEndpointVolume audioEndPointVolume;
		private readonly AudioEndpointVolumeChannel[] channels;

		/// <summary>
		/// Channel Count
		/// </summary>
		internal int Count
		{
			get
			{
				Marshal.ThrowExceptionForHR(audioEndPointVolume.GetChannelCount(out var result));
				return result;
			}
		}

		internal AudioEndpointVolumeChannels(IAudioEndpointVolume parent)
		{
			audioEndPointVolume = parent;
			var channelCount = Count;
			channels = new AudioEndpointVolumeChannel[channelCount];

			for (var i = 0; i < channelCount; i++)
			{
				channels[i] = new AudioEndpointVolumeChannel(audioEndPointVolume, i);
			}
		}

		/// <summary>
		/// Indexer - get a specific channel
		/// </summary>
		internal AudioEndpointVolumeChannel this[int index] => channels[index];
	}

	/// <summary>
	/// Audio Endpoint Volume Step Information
	/// </summary>
	internal class AudioEndpointVolumeStepInformation
	{
		private readonly uint step;
		private readonly uint stepCount;

		/// <summary>
		/// Step
		/// </summary>
		internal uint Step => step;

		/// <summary>
		/// StepCount
		/// </summary>
		internal uint StepCount => stepCount;

		internal AudioEndpointVolumeStepInformation(IAudioEndpointVolume parent) => Marshal.ThrowExceptionForHR(parent.GetVolumeStepInfo(out step, out stepCount));
	}

	/// <summary>
	/// Audio Endpoint Volume Volume Range
	/// </summary>
	internal class AudioEndpointVolumeVolumeRange
	{
		private readonly float volumeIncrementDecibels;
		private readonly float volumeMaxDecibels;
		private readonly float volumeMinDecibels;

		/// <summary>
		/// Increment Decibels
		/// </summary>
		internal float IncrementDecibels => volumeIncrementDecibels;

		/// <summary>
		/// Maximum Decibels
		/// </summary>
		internal float MaxDecibels => volumeMaxDecibels;

		/// <summary>
		/// Minimum Decibels
		/// </summary>
		internal float MinDecibels => volumeMinDecibels;

		internal AudioEndpointVolumeVolumeRange(IAudioEndpointVolume parent) => Marshal.ThrowExceptionForHR(parent.GetVolumeRange(out volumeMinDecibels, out volumeMaxDecibels, out volumeIncrementDecibels));
	}

	/// <summary>
	/// Audio Volume Notification Data
	/// </summary>
	internal class AudioVolumeNotificationData
	{
		/// <summary>
		/// Channels
		/// </summary>
		internal int Channels { get; }

		/// <summary>
		/// Channel Volume
		/// </summary>
		internal float[] ChannelVolume { get; }

		/// <summary>
		/// Event Context
		/// </summary>
		internal Guid EventContext { get; }

		/// <summary>
		/// Guid that raised the event
		/// </summary>
		internal Guid Guid { get; }

		/// <summary>
		/// Master Volume
		/// </summary>
		internal float MasterVolume { get; }

		/// <summary>
		/// Muted
		/// </summary>
		internal bool Muted { get; }

		/// <summary>
		/// Audio Volume Notification Data
		/// </summary>
		/// <param name="eventContext"></param>
		/// <param name="muted"></param>
		/// <param name="masterVolume"></param>
		/// <param name="channelVolume"></param>
		/// <param name="guid"></param>
		internal AudioVolumeNotificationData(Guid eventContext, bool muted, float masterVolume, float[] channelVolume, Guid guid)
		{
			EventContext = eventContext;
			Muted = muted;
			MasterVolume = masterVolume;
			Channels = channelVolume.Length;
			ChannelVolume = channelVolume;
			Guid = guid;
		}
	}

	/// <summary>
	/// MM Device
	/// </summary>
	internal class MMDevice : IDisposable
	{
		private static Guid IDD_IAudioSessionManager = new Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4");
		private static Guid IDD_IDeviceTopology = new Guid("2A07407E-6497-4A18-9787-32F79BD0D98F");
		private static Guid IID_IAudioClient = new Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");
		private static Guid IID_IAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
		private static Guid IID_IAudioMeterInformation = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");

		private readonly IMMDevice deviceInterface;
		private AudioEndpointVolume audioEndpointVolume;
		private PropertyStore propertyStore;
		/*    private AudioMeterInformation audioMeterInformation;
		 *    */
		/*
		    private AudioSessionManager audioSessionManager;
		    private DeviceTopology deviceTopology;
		*/
		// ReSharper restore InconsistentNaming

		/// <summary>
		/// Audio Endpoint Volume
		/// </summary>
		internal AudioEndpointVolume AudioEndpointVolume
		{
			get
			{
				if (audioEndpointVolume == null)
					GetAudioEndpointVolume();

				return audioEndpointVolume;
			}
		}

		/// <summary>
		/// Friendly name of device
		/// </summary>
		internal string DeviceFriendlyName
		{
			get
			{
				if (propertyStore == null)
				{
					GetPropertyInformation();
				}

				if (propertyStore.Contains(PropertyKeys.PKEY_DeviceInterface_FriendlyName))
				{
					return (string)propertyStore[PropertyKeys.PKEY_DeviceInterface_FriendlyName].Value;
				}
				else
				{
					return "Unknown";
				}
			}
		}

		/// <summary>
		/// Friendly name for the endpoint
		/// </summary>
		internal string FriendlyName
		{
			get
			{
				if (propertyStore == null)
				{
					GetPropertyInformation();
				}

				if (propertyStore.Contains(PropertyKeys.PKEY_Device_FriendlyName))
				{
					return (string)propertyStore[PropertyKeys.PKEY_Device_FriendlyName].Value;
				}
				else
					return "Unknown";
			}
		}

		internal MMDevice(IMMDevice realDevice) => deviceInterface = realDevice;

		/// <summary>
		/// Finalizer
		/// </summary>
		~MMDevice()
		{
			Dispose();
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			this.audioEndpointVolume?.Dispose();
			//this.audioSessionManager?.Dispose();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Initializes the device's property store.
		/// </summary>
		/// <param name="stgmAccess">The storage-access mode to open store for.</param>
		/// <remarks>Administrative client is required for Write and ReadWrite modes.</remarks>
		internal void GetPropertyInformation(StorageAccessMode stgmAccess = StorageAccessMode.Read)
		{
			Marshal.ThrowExceptionForHR(deviceInterface.OpenPropertyStore(stgmAccess, out var propstore));
			propertyStore = new PropertyStore(propstore);
		}

		/*
		    private AudioClient GetAudioClient()
		    {
		    Marshal.ThrowExceptionForHR(deviceInterface.Activate(ref IID_IAudioClient, ClsCtx.ALL, IntPtr.Zero, out var result));
		    return new AudioClient(result as IAudioClient);
		    }

		    private void GetAudioMeterInformation()
		    {
		    Marshal.ThrowExceptionForHR(deviceInterface.Activate(ref IID_IAudioMeterInformation, ClsCtx.ALL, IntPtr.Zero, out var result));
		    audioMeterInformation = new AudioMeterInformation(result as IAudioMeterInformation);
		    }
		*/

		/// <summary>
		/// To string
		/// </summary>
		public override string ToString() => FriendlyName;

		private void GetAudioEndpointVolume()
		{
			Marshal.ThrowExceptionForHR(deviceInterface.Activate(ref IID_IAudioEndpointVolume, ClsCtx.ALL, IntPtr.Zero, out var result));
			audioEndpointVolume = new AudioEndpointVolume(result as IAudioEndpointVolume);
		}

		/*
		    private void GetAudioSessionManager()
		    {
		    Marshal.ThrowExceptionForHR(deviceInterface.Activate(ref IDD_IAudioSessionManager, ClsCtx.ALL, IntPtr.Zero, out var result));
		    audioSessionManager = new AudioSessionManager(result as IAudioSessionManager);
		    }

		    private void GetDeviceTopology()
		    {
		    Marshal.ThrowExceptionForHR(deviceInterface.Activate(ref IDD_IDeviceTopology, ClsCtx.ALL, IntPtr.Zero, out var result));
		    deviceTopology = new DeviceTopology(result as IDeviceTopology);
		    }

		    /// <summary>
		    /// Audio Client
		    /// Makes a new one each call to allow caller to manage when to dispose
		    /// n.b. should probably not be a property anymore
		    /// </summary>
		    internal AudioClient AudioClient => GetAudioClient();

		    /// <summary>
		    /// Audio Meter Information
		    /// </summary>
		    internal AudioMeterInformation AudioMeterInformation
		    {
		    get
		    {
		        if (audioMeterInformation == null)
		            GetAudioMeterInformation();

		        return audioMeterInformation;
		    }
		    }
		*/
		/*
		    /// <summary>
		    /// AudioSessionManager instance
		    /// </summary>
		    internal AudioSessionManager AudioSessionManager
		    {
		    get
		    {
		        if (audioSessionManager == null)
		        {
		            GetAudioSessionManager();
		        }

		        return audioSessionManager;
		    }
		    }

		    /// <summary>
		    /// DeviceTopology instance
		    /// </summary>
		    internal DeviceTopology DeviceTopology
		    {
		    get
		    {
		        if (deviceTopology == null)
		        {
		            GetDeviceTopology();
		        }

		        return deviceTopology;
		    }
		    }

		    /// <summary>
		    /// Properties
		    /// </summary>
		    internal PropertyStore Properties
		    {
		    get
		    {
		        if (propertyStore == null)
		            GetPropertyInformation();

		        return propertyStore;
		    }
		    }
		*/
		/*
		    /// <summary>
		    /// Icon path of device
		    /// </summary>
		    internal string IconPath
		    {
		    get
		    {
		        if (propertyStore == null)
		        {
		            GetPropertyInformation();
		        }

		        if (propertyStore.Contains(PropertyKeys.PKEY_Device_IconPath))
		        {
		            return (string)propertyStore[PropertyKeys.PKEY_Device_IconPath].Value;
		        }

		        return "Unknown";
		    }
		    }

		    /// <summary>
		    /// Device Instance Id of Device
		    /// </summary>
		    internal string InstanceId
		    {
		    get
		    {
		        if (propertyStore == null)
		        {
		            GetPropertyInformation();
		        }

		        if (propertyStore.Contains(PropertyKeys.PKEY_Device_InstanceId))
		        {
		            return (string)propertyStore[PropertyKeys.PKEY_Device_InstanceId].Value;
		        }

		        return "Unknown";
		    }
		    }

		    /// <summary>
		    /// Device ID
		    /// </summary>
		    internal string ID
		    {
		    get
		    {
		        Marshal.ThrowExceptionForHR(deviceInterface.GetId(out var result));
		        return result;
		    }
		    }

		    /// <summary>
		    /// Data Flow
		    /// </summary>
		    internal DataFlow DataFlow
		    {
		    get
		    {
		        var ep = deviceInterface as IMMEndpoint;
		        ep.GetDataFlow(out var result);
		        return result;
		    }
		    }

		    /// <summary>
		    /// Device State
		    /// </summary>
		    internal DeviceState State
		    {
		    get
		    {
		        Marshal.ThrowExceptionForHR(deviceInterface.GetState(out var result));
		        return result;
		    }
		    }

		*/
	}

	/// <summary>
	/// Multimedia Device Collection
	/// </summary>
	internal class MMDeviceCollection : IEnumerable<MMDevice>
	{
		private readonly IMMDeviceCollection mmDeviceCollection;

		/// <summary>
		/// Device count
		/// </summary>
		internal int Count
		{
			get
			{
				Marshal.ThrowExceptionForHR(mmDeviceCollection.GetCount(out var result));
				return result;
			}
		}

		internal MMDeviceCollection(IMMDeviceCollection parent) => mmDeviceCollection = parent;

		/// <summary>
		/// Get Enumerator
		/// </summary>
		/// <returns>Device enumerator</returns>
		public IEnumerator<MMDevice> GetEnumerator()
		{
			for (var index = 0; index < Count; index++)
			{
				yield return this[index];
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Get device by index
		/// </summary>
		/// <param name="index">Device index</param>
		/// <returns>Device at the specified index</returns>
		internal MMDevice this[int index]
		{
			get
			{
				_ = mmDeviceCollection.Item(index, out var result);
				return new MMDevice(result);
			}
		}
	}

	/// <summary>
	/// MM Device Enumerator
	/// </summary>
	internal class MMDeviceEnumerator : IDisposable
	{
		private IMMDeviceEnumerator realEnumerator;

		/// <summary>
		/// Creates a new MM Device Enumerator
		/// </summary>
		internal MMDeviceEnumerator()
		{
			if (System.Environment.OSVersion.Version.Major < 6)
			{
				throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
			}

			realEnumerator = new MMDeviceEnumeratorComObject() as IMMDeviceEnumerator;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Enumerate Audio Endpoints
		/// </summary>
		/// <param name="dataFlow">Desired DataFlow</param>
		/// <param name="dwStateMask">State Mask</param>
		/// <returns>Device Collection</returns>
		internal MMDeviceCollection EnumerateAudioEndPoints(DataFlow dataFlow, DeviceState dwStateMask)
		{
			Marshal.ThrowExceptionForHR(realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out var result));
			return new MMDeviceCollection(result);
		}

		/// <summary>
		/// Get Default Endpoint
		/// </summary>
		/// <param name="dataFlow">Data Flow</param>
		/// <param name="role">Role</param>
		/// <returns>Device</returns>
		internal MMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role)
		{
			Marshal.ThrowExceptionForHR(((IMMDeviceEnumerator)realEnumerator).GetDefaultAudioEndpoint(dataFlow, role, out var device));
			return new MMDevice(device);
		}

		/// <summary>
		/// Get device by ID
		/// </summary>
		/// <param name="id">Device ID</param>
		/// <returns>Device</returns>
		internal MMDevice GetDevice(string id)
		{
			Marshal.ThrowExceptionForHR(((IMMDeviceEnumerator)realEnumerator).GetDevice(id, out var device));
			return new MMDevice(device);
		}

		/// <summary>
		/// Check to see if a default audio end point exists without needing an exception.
		/// </summary>
		/// <param name="dataFlow">Data Flow</param>
		/// <param name="role">Role</param>
		/// <returns>True if one exists, and false if one does not exist.</returns>
		internal bool HasDefaultAudioEndpoint(DataFlow dataFlow, Role role)
		{
			const int E_NOTFOUND = unchecked((int)0x80070490);
			var hresult = ((IMMDeviceEnumerator)realEnumerator).GetDefaultAudioEndpoint(dataFlow, role, out var device);

			if (hresult == 0x0)
			{
				_ = Marshal.ReleaseComObject(device);
				return true;
			}

			if (hresult == E_NOTFOUND)
			{
				return false;
			}

			Marshal.ThrowExceptionForHR(hresult);
			return false;
		}

		/// <summary>
		/// Registers a call back for Device Events
		/// </summary>
		/// <param name="client">Object implementing IMMNotificationClient type casted as IMMNotificationClient interface</param>
		/// <returns></returns>
		internal int RegisterEndpointNotificationCallback([In][MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client) => realEnumerator.RegisterEndpointNotificationCallback(client);

		/// <summary>
		/// Unregisters a call back for Device Events
		/// </summary>
		/// <param name="client">Object implementing IMMNotificationClient type casted as IMMNotificationClient interface </param>
		/// <returns></returns>
		internal int UnregisterEndpointNotificationCallback([In][MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client) => realEnumerator.UnregisterEndpointNotificationCallback(client);

		/// <summary>
		/// Called to dispose/finalize contained objects.
		/// </summary>
		/// <param name="disposing">True if disposing, false if called from a finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (realEnumerator != null)
				{
					// although GC would do this for us, we want it done now
					_ = Marshal.ReleaseComObject(realEnumerator);
					realEnumerator = null;
				}
			}
		}
	}

	/// <summary>
	/// Property Store class, only supports reading properties at the moment.
	/// </summary>
	internal class PropertyStore
	{
		private readonly IPropertyStore storeInterface;

		/// <summary>
		/// Property Count
		/// </summary>
		internal int Count
		{
			get
			{
				Marshal.ThrowExceptionForHR(storeInterface.GetCount(out var result));
				return result;
			}
		}

		/// <summary>
		/// Creates a new property store
		/// </summary>
		/// <param name="store">IPropertyStore COM interface</param>
		internal PropertyStore(IPropertyStore store) => storeInterface = store;

		/// <summary>
		/// Saves a property change.
		/// </summary>
		internal void Commit() => Marshal.ThrowExceptionForHR(storeInterface.Commit());

		/// <summary>
		/// Contains property guid
		/// </summary>
		/// <param name="key">Looks for a specific key</param>
		/// <returns>True if found</returns>
		internal bool Contains(PropertyKey key)
		{
			for (var i = 0; i < Count; i++)
			{
				var ikey = Get(i);

				if ((ikey.formatId == key.formatId) && (ikey.propertyId == key.propertyId))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets property key at sepecified index
		/// </summary>
		/// <param name="index">Index</param>
		/// <returns>Property key</returns>
		internal PropertyKey Get(int index)
		{
			Marshal.ThrowExceptionForHR(storeInterface.GetAt(index, out var key));
			return key;
		}

		/// <summary>
		/// Gets property value at specified index
		/// </summary>
		/// <param name="index">Index</param>
		/// <returns>Property value</returns>
		internal PropVariant GetValue(int index)
		{
			var key = Get(index);
			Marshal.ThrowExceptionForHR(storeInterface.GetValue(ref key, out var result));
			return result;
		}

		/// <summary>
		/// Sets property value at specified key.
		/// </summary>
		/// <param name="key">Key of property to set.</param>
		/// <param name="value">Value to write.</param>
		internal void SetValue(PropertyKey key, PropVariant value) => Marshal.ThrowExceptionForHR(storeInterface.SetValue(ref key, ref value));

		/// <summary>
		/// Gets property by index
		/// </summary>
		/// <param name="index">Property index</param>
		/// <returns>The property</returns>
		internal PropertyStoreProperty this[int index]
		{
			get
			{
				var key = Get(index);
				Marshal.ThrowExceptionForHR(storeInterface.GetValue(ref key, out var result));
				return new PropertyStoreProperty(key, result);
			}
		}

		/// <summary>
		/// Indexer by guid
		/// </summary>
		/// <param name="key">Property Key</param>
		/// <returns>Property or null if not found</returns>
		internal PropertyStoreProperty this[PropertyKey key]
		{
			get
			{
				for (var i = 0; i < Count; i++)
				{
					var ikey = Get(i);

					if ((ikey.formatId == key.formatId) && (ikey.propertyId == key.propertyId))
					{
						Marshal.ThrowExceptionForHR(storeInterface.GetValue(ref ikey, out var result));
						return new PropertyStoreProperty(ikey, result);
					}
				}

				return null;
			}
		}
	}

	/// <summary>
	/// is defined in propsys.h
	/// </summary>
	/// <summary>
	/// Property Store Property
	/// </summary>
	internal class PropertyStoreProperty
	{
		private PropVariant propertyValue;

		/// <summary>
		/// Property Key
		/// </summary>
		internal PropertyKey Key { get; }

		/// <summary>
		/// Property Value
		/// </summary>
		internal object Value => propertyValue.Value;

		internal PropertyStoreProperty(PropertyKey key, PropVariant value)
		{
			Key = key;
			propertyValue = value;
		}
	}

	// This class implements the IAudioEndpointVolumeCallback interface,
	// it is implemented in this class because implementing it on AudioEndpointVolume
	// (where the functionality is really wanted, would cause the OnNotify function
	// to show up in the internal API.
	internal class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
	{
		private readonly AudioEndpointVolume parent;

		internal AudioEndpointVolumeCallback(AudioEndpointVolume parent) => this.parent = parent;

		public void OnNotify(IntPtr notifyData)
		{
			//Since AUDIO_VOLUME_NOTIFICATION_DATA is dynamic in length based on the
			//number of audio channels available we cannot just call PtrToStructure
			//to get all data, thats why it is split up into two steps, first the static
			//data is marshalled into the data structure, then with some IntPtr math the
			//remaining floats are read from memory.
			//
			var data = Marshal.PtrToStructure<AudioVolumeNotificationDataStruct>(notifyData);
			//Determine offset in structure of the first float
			var offset = Marshal.OffsetOf<AudioVolumeNotificationDataStruct>("ChannelVolume");
			//Determine offset in memory of the first float
			var firstFloatPtr = (IntPtr)((long)notifyData + (long)offset);
			var voldata = new float[data.nChannels];

			//Read all floats from memory.
			for (var i = 0; i < data.nChannels; i++)
			{
				voldata[i] = Marshal.PtrToStructure<float>(firstFloatPtr);
			}

			//Create combined structure and Fire Event in parent class.
			var notificationData = new AudioVolumeNotificationData(data.guidEventContext, data.bMuted, data.fMasterVolume, voldata, data.guidEventContext);
			parent.FireNotification(notificationData);
		}
	}

	/// <summary>
	/// implements IMMDeviceEnumerator
	/// </summary>
	[ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	internal class MMDeviceEnumeratorComObject
	{
	}

	internal class PropVariantNative
	{
		[DllImport("ole32.dll")]
		internal static extern int PropVariantClear(ref PropVariant pvar);

		[DllImport("ole32.dll")]
		internal static extern int PropVariantClear(IntPtr pvar);
	}

	/// <summary>
	/// Endpoint Hardware Support
	/// </summary>
	[Flags]
	internal enum EEndpointHardwareSupport
	{
		/// <summary>
		/// Volume
		/// </summary>
		Volume = 0x00000001,

		/// <summary>
		/// Mute
		/// </summary>
		Mute = 0x00000002,

		/// <summary>
		/// Meter
		/// </summary>
		Meter = 0x00000004
	}

	/// <summary>
	/// The EDataFlow enumeration defines constants that indicate the direction
	/// in which audio data flows between an audio endpoint device and an application
	/// </summary>
	internal enum DataFlow
	{
		/// <summary>
		/// Audio rendering stream.
		/// Audio data flows from the application to the audio endpoint device, which renders the stream.
		/// </summary>
		Render,
		/// <summary>
		/// Audio capture stream. Audio data flows from the audio endpoint device that captures the stream,
		/// to the application
		/// </summary>
		Capture,
		/// <summary>
		/// Audio rendering or capture stream. Audio data can flow either from the application to the audio
		/// endpoint device, or from the audio endpoint device to the application.
		/// </summary>
		All
	}

	/// <summary>
	/// Device State
	/// </summary>
	[Flags]
	internal enum DeviceState
	{
		/// <summary>
		/// DEVICE_STATE_ACTIVE
		/// </summary>
		Active = 0x00000001,
		/// <summary>
		/// DEVICE_STATE_DISABLED
		/// </summary>
		Disabled = 0x00000002,
		/// <summary>
		/// DEVICE_STATE_NOTPRESENT
		/// </summary>
		NotPresent = 0x00000004,
		/// <summary>
		/// DEVICE_STATE_UNPLUGGED
		/// </summary>
		Unplugged = 0x00000008,
		/// <summary>
		/// DEVICE_STATEMASK_ALL
		/// </summary>
		All = 0x0000000F
	}

	/// <summary>
	/// MMDevice STGM enumeration
	/// </summary>
	internal enum StorageAccessMode
	{
		/// <summary>
		/// Read-only access mode.
		/// </summary>
		Read,
		/// <summary>
		/// Write-only access mode.
		/// </summary>
		Write,
		/// <summary>
		/// Read-write access mode.
		/// </summary>
		ReadWrite
	}

	/// <summary>
	/// is defined in WTypes.h
	/// </summary>
	[Flags]
	internal enum ClsCtx
	{
		INPROC_SERVER = 0x1,
		INPROC_HANDLER = 0x2,
		LOCAL_SERVER = 0x4,
		INPROC_SERVER16 = 0x8,
		REMOTE_SERVER = 0x10,
		INPROC_HANDLER16 = 0x20,
		//RESERVED1 = 0x40,
		//RESERVED2 = 0x80,
		//RESERVED3 = 0x100,
		//RESERVED4 = 0x200,
		NO_CODE_DOWNLOAD = 0x400,
		//RESERVED5 = 0x800,
		NO_CUSTOM_MARSHAL = 0x1000,
		ENABLE_CODE_DOWNLOAD = 0x2000,
		NO_FAILURE_LOG = 0x4000,
		DISABLE_AAA = 0x8000,
		ENABLE_AAA = 0x10000,
		FROM_DEFAULT_CONTEXT = 0x20000,
		ACTIVATE_32_BIT_SERVER = 0x40000,
		ACTIVATE_64_BIT_SERVER = 0x80000,
		ENABLE_CLOAKING = 0x100000,
		PS_DLL = unchecked((int)0x80000000),
		INPROC = INPROC_SERVER | INPROC_HANDLER,
		SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
		ALL = SERVER | INPROC_HANDLER
	}

	/// <summary>
	/// The ERole enumeration defines constants that indicate the role
	/// that the system has assigned to an audio endpoint device
	/// </summary>
	internal enum Role
	{
		/// <summary>
		/// Games, system notification sounds, and voice commands.
		/// </summary>
		Console,

		/// <summary>
		/// Music, movies, narration, and live music recording
		/// </summary>
		Multimedia,

		/// <summary>
		/// Voice communications (talking to another person).
		/// </summary>
		Communications,
	}

	/// <summary>
	/// Representation of binary large object container.
	/// </summary>
	internal struct Blob
	{
		/// <summary>
		/// Length of binary object.
		/// </summary>
		internal int Length;
		/// <summary>
		/// Pointer to buffer storing data.
		/// </summary>
		internal IntPtr Data;
	}

	/// <summary>
	/// PROPERTYKEY is defined in wtypes.h
	/// </summary>
	internal struct PropertyKey
	{
		/// <summary>
		/// Format ID
		/// </summary>
		internal Guid formatId;
		/// <summary>
		/// Property ID
		/// </summary>
		internal int propertyId;
		/// <summary>
		/// <param name="formatId"></param>
		/// <param name="propertyId"></param>
		/// </summary>
		internal PropertyKey(Guid formatId, int propertyId)
		{
			this.formatId = formatId;
			this.propertyId = propertyId;
		}
	}

	// <summary>
	/// from Propidl.h.
	/// http://msdn.microsoft.com/en-us/library/aa380072(VS.85).aspx
	/// contains a union so we have to do an explicit layout
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct PropVariant
	{
		/// <summary>
		/// Value type tag.
		/// </summary>
		[FieldOffset(0)] public short vt;
		/// <summary>
		/// Reserved1.
		/// </summary>
		[FieldOffset(2)] public short wReserved1;
		/// <summary>
		/// Reserved2.
		/// </summary>
		[FieldOffset(4)] public short wReserved2;
		/// <summary>
		/// Reserved3.
		/// </summary>
		[FieldOffset(6)] public short wReserved3;
		/// <summary>
		/// cVal.
		/// </summary>
		[FieldOffset(8)] public sbyte cVal;
		/// <summary>
		/// bVal.
		/// </summary>
		[FieldOffset(8)] public byte bVal;
		/// <summary>
		/// iVal.
		/// </summary>
		[FieldOffset(8)] public short iVal;
		/// <summary>
		/// uiVal.
		/// </summary>
		[FieldOffset(8)] public ushort uiVal;
		/// <summary>
		/// lVal.
		/// </summary>
		[FieldOffset(8)] public int lVal;
		/// <summary>
		/// ulVal.
		/// </summary>
		[FieldOffset(8)] public uint ulVal;
		/// <summary>
		/// intVal.
		/// </summary>
		[FieldOffset(8)] public int intVal;
		/// <summary>
		/// uintVal.
		/// </summary>
		[FieldOffset(8)] public uint uintVal;
		/// <summary>
		/// hVal.
		/// </summary>
		[FieldOffset(8)] public long hVal;
		/// <summary>
		/// uhVal.
		/// </summary>
		[FieldOffset(8)] public long uhVal;
		/// <summary>
		/// fltVal.
		/// </summary>
		[FieldOffset(8)] public float fltVal;
		/// <summary>
		/// dblVal.
		/// </summary>
		[FieldOffset(8)] public double dblVal;
		//VARIANT_BOOL boolVal;
		/// <summary>
		/// boolVal.
		/// </summary>
		[FieldOffset(8)] public short boolVal;
		/// <summary>
		/// scode.
		/// </summary>
		[FieldOffset(8)] public int scode;
		//CY cyVal;
		//[FieldOffset(8)] private DateTime date; - can cause issues with invalid value
		/// <summary>
		/// Date time.
		/// </summary>
		[FieldOffset(8)] public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
		//CLSID* puuid;
		//CLIPDATA* pclipdata;
		//BSTR bstrVal;
		//BSTRBLOB bstrblobVal;
		/// <summary>
		/// Binary large object.
		/// </summary>
		[FieldOffset(8)] public Blob blobVal;
		//LPSTR pszVal;
		/// <summary>
		/// Pointer value.
		/// </summary>
		[FieldOffset(8)] public IntPtr pointerValue; //LPWSTR
		//IUnknown* punkVal;
		/*  IDispatch* pdispVal;
		    IStream* pStream;
		    IStorage* pStorage;
		    LPVERSIONEDSTREAM pVersionedStream;
		    LPSAFEARRAY parray;
		    CAC cac;
		    CAUB caub;
		    CAI cai;
		    CAUI caui;
		    CAL cal;
		    CAUL caul;
		    CAH cah;
		    CAUH cauh;
		    CAFLT caflt;
		    CADBL cadbl;
		    CABOOL cabool;
		    CASCODE cascode;
		    CACY cacy;
		    CADATE cadate;
		    CAFILETIME cafiletime;
		    CACLSID cauuid;
		    CACLIPDATA caclipdata;
		    CABSTR cabstr;
		    CABSTRBLOB cabstrblob;
		    CALPSTR calpstr;
		    CALPWSTR calpwstr;
		    CAPROPVARIANT capropvar;
		    CHAR* pcVal;
		    UCHAR* pbVal;
		    SHORT* piVal;
		    USHORT* puiVal;
		    LONG* plVal;
		    ULONG* pulVal;
		    INT* pintVal;
		    UINT* puintVal;
		    FLOAT* pfltVal;
		    DOUBLE* pdblVal;
		    VARIANT_BOOL* pboolVal;
		    DECIMAL* pdecVal;
		    SCODE* pscode;
		    CY* pcyVal;
		    DATE* pdate;
		    BSTR* pbstrVal;
		    IUnknown** ppunkVal;
		    IDispatch** ppdispVal;
		    LPSAFEARRAY* pparray;
		    PROPVARIANT* pvarVal;
		*/

		/// <summary>
		/// Creates a new PropVariant containing a long value
		/// </summary>
		internal static PropVariant FromLong(long value) => new PropVariant() { vt = (short)VarEnum.VT_I8, hVal = value };

		/// <summary>
		/// Helper method to gets blob data
		/// </summary>
		private byte[] GetBlob()
		{
			var blob = new byte[blobVal.Length];
			Marshal.Copy(blobVal.Data, blob, 0, blob.Length);
			return blob;
		}

		/// <summary>
		/// Interprets a blob as an array of structs
		/// </summary>
		internal T[] GetBlobAsArrayOf<T>()
		{
			var blobByteLength = blobVal.Length;
			var singleInstance = (T)Activator.CreateInstance(typeof(T));
			var structSize = Marshal.SizeOf(singleInstance);

			if (blobByteLength % structSize != 0)
			{
				throw new InvalidDataException(string.Format("Blob size {0} not a multiple of struct size {1}", blobByteLength, structSize));
			}

			var items = blobByteLength / structSize;
			var array = new T[items];

			for (var n = 0; n < items; n++)
			{
				array[n] = (T)Activator.CreateInstance(typeof(T));
				Marshal.PtrToStructure(new IntPtr((long)blobVal.Data + n * structSize), array[n]);
			}

			return array;
		}

		/// <summary>
		/// Gets the type of data in this PropVariant
		/// </summary>
		internal VarEnum DataType => (VarEnum)vt;

		/// <summary>
		/// Property value
		/// </summary>
		internal object Value
		{
			get
			{
				var ve = DataType;

				switch (ve)
				{
					case VarEnum.VT_I1:
						return bVal;

					case VarEnum.VT_I2:
						return iVal;

					case VarEnum.VT_I4:
						return lVal;

					case VarEnum.VT_I8:
						return hVal;

					case VarEnum.VT_INT:
						return iVal;

					case VarEnum.VT_UI4:
						return ulVal;

					case VarEnum.VT_UI8:
						return uhVal;

					case VarEnum.VT_LPWSTR:
						return Marshal.PtrToStringUni(pointerValue);

					case VarEnum.VT_BLOB:
					case VarEnum.VT_VECTOR | VarEnum.VT_UI1:
						return GetBlob();

					case VarEnum.VT_CLSID:
						return Marshal.PtrToStructure<Guid>(pointerValue);

					case VarEnum.VT_BOOL:
						switch (boolVal)
						{
							case -1:
								return true;

							case 0:
								return false;

							default:
								throw new NotSupportedException("PropVariant VT_BOOL must be either -1 or 0");
						}

					case VarEnum.VT_FILETIME:
						return DateTime.FromFileTime((((long)filetime.dwHighDateTime) << 32) + filetime.dwLowDateTime);
				}

				throw new NotImplementedException("PropVariant " + ve);
			}
		}

		/// <summary>
		/// allows freeing up memory, might turn this into a Dispose method?
		/// </summary>
		[Obsolete("Call with pointer instead")]
		internal void Clear() => PropVariantNative.PropVariantClear(ref this);

		/// <summary>
		/// Clears with a known pointer
		/// </summary>
		internal static void Clear(IntPtr ptr) => PropVariantNative.PropVariantClear(ptr);
	}

	/// <summary>
	/// Audio Endpoint Volume Notifiaction Delegate
	/// </summary>
	/// <param name="data">Audio Volume Notification Data</param>
	internal delegate void AudioEndpointVolumeNotificationDelegate(AudioVolumeNotificationData data);
}