#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Core.Windows
{
	[Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
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
		int SetMute([MarshalAs(UnmanagedType.Bool)] Boolean bMute, ref Guid pguidEventContext);
		int GetMute(out bool pbMute);
		int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
		int VolumeStepUp(ref Guid pguidEventContext);
		int VolumeStepDown(ref Guid pguidEventContext);
		int QueryHardwareSupport(out uint pdwHardwareSupportMask);
		int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
	}

	[Guid("657804FA-D6AD-4496-8A60-352752AF4F89"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IAudioEndpointVolumeCallback
	{
		void OnNotify(IntPtr notifyData);
	};

	[Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IAudioMeterInformation
	{
		int GetPeakValue(out float pfPeak);
		int GetMeteringChannelCount(out int pnChannelCount);
		int GetChannelsPeakValues(int u32ChannelCount, [In] IntPtr afPeakValues);
		int QueryHardwareSupport(out int pdwHardwareSupportMask);
	};

	///// <summary>
	///// Windows CoreAudio IAudioClient interface
	///// Defined in AudioClient.h
	///// </summary>
	//[Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2"),
	// InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	// ComImport]
	//internal interface IAudioClient
	//{
	//  [PreserveSig]
	//  int Initialize(AudioClientShareMode shareMode,
	//                 AudioClientStreamFlags streamFlags,
	//                 long hnsBufferDuration, // REFERENCE_TIME
	//                 long hnsPeriodicity, // REFERENCE_TIME
	//                 [In] WaveFormat pFormat,
	//                 [In] ref Guid audioSessionGuid);

	//  /// <summary>
	//  /// The GetBufferSize method retrieves the size (maximum capacity) of the endpoint buffer.
	//  /// </summary>
	//  int GetBufferSize(out uint bufferSize);

	//  [return: MarshalAs(UnmanagedType.I8)]
	//  long GetStreamLatency();

	//  int GetCurrentPadding(out int currentPadding);

	//  [PreserveSig]
	//  int IsFormatSupported(
	//      AudioClientShareMode shareMode,
	//      [In] WaveFormat pFormat,
	//      IntPtr closestMatchFormat); // or outIntPtr??

	//  int GetMixFormat(out IntPtr deviceFormatPointer);

	//  // REFERENCE_TIME is 64 bit int
	//  int GetDevicePeriod(out long defaultDevicePeriod, out long minimumDevicePeriod);

	//  int Start();

	//  int Stop();

	//  int Reset();

	//  int SetEventHandle(IntPtr eventHandle);

	//  /// <summary>
	//  /// The GetService method accesses additional services from the audio client object.
	//  /// </summary>
	//  /// <param name="interfaceId">The interface ID for the requested service.</param>
	//  /// <param name="interfacePointer">Pointer to a pointer variable into which the method writes the address of an instance of the requested interface. </param>
	//  [PreserveSig]
	//  int GetService([In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId, [Out, MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
	//}

	/// <summary>
	/// Windows CoreAudio IAudioSessionControl interface
	/// Defined in AudioPolicy.h
	/// </summary>
	[Guid("24918ACC-64B3-37C1-8CA9-74A66E9957A8"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IAudioSessionEvents
	{
		/// <summary>
		/// Notifies the client that the display name for the session has changed.
		/// </summary>
		/// <param name="displayName">The new display name for the session.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int OnDisplayNameChanged(
			[In][MarshalAs(UnmanagedType.LPWStr)] string displayName,
			[In] ref Guid eventContext);

		/// <summary>
		/// Notifies the client that the display icon for the session has changed.
		/// </summary>
		/// <param name="iconPath">The path for the new display icon for the session.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int OnIconPathChanged(
			[In][MarshalAs(UnmanagedType.LPWStr)] string iconPath,
			[In] ref Guid eventContext);

		/// <summary>
		/// Notifies the client that the volume level or muting state of the session has changed.
		/// </summary>
		/// <param name="volume">The new volume level for the audio session.</param>
		/// <param name="isMuted">The new muting state.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int OnSimpleVolumeChanged(
			[In][MarshalAs(UnmanagedType.R4)] float volume,
			[In][MarshalAs(UnmanagedType.Bool)] bool isMuted,
			[In] ref Guid eventContext);

		/// <summary>
		/// Notifies the client that the volume level of an audio channel in the session submix has changed.
		/// </summary>
		/// <param name="channelCount">The channel count.</param>
		/// <param name="newVolumes">An array of volumnes cooresponding with each channel index.</param>
		/// <param name="channelIndex">The number of the channel whose volume level changed.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int OnChannelVolumeChanged(
			[In][MarshalAs(UnmanagedType.U4)] UInt32 channelCount,
			[In][MarshalAs(UnmanagedType.SysInt)] IntPtr newVolumes, // Pointer to float array
			[In][MarshalAs(UnmanagedType.U4)] UInt32 channelIndex,
			[In] ref Guid eventContext);

		/// <summary>
		/// Notifies the client that the grouping parameter for the session has changed.
		/// </summary>
		/// <param name="groupingId">The new grouping parameter for the session.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int OnGroupingParamChanged(
			[In] ref Guid groupingId,
			[In] ref Guid eventContext);

		/// <summary>
		/// Notifies the client that the stream-activity state of the session has changed.
		/// </summary>
		/// <param name="state">The new session state.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int OnStateChanged(
			[In] AudioSessionState state);

		/// <summary>
		/// Notifies the client that the session has been disconnected.
		/// </summary>
		/// <param name="disconnectReason">The reason that the audio session was disconnected.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int OnSessionDisconnected(
			[In] AudioSessionDisconnectReason disconnectReason);
	}

	/// <summary>
	/// Windows CoreAudio IAudioSessionControl interface
	/// Defined in AudioPolicy.h
	/// </summary>
	[Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IAudioSessionControl
	{
		/// <summary>
		/// Retrieves the current state of the audio session.
		/// </summary>
		/// <param name="state">Receives the current session state.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetState(
			[Out] out AudioSessionState state);

		/// <summary>
		/// Retrieves the display name for the audio session.
		/// </summary>
		/// <param name="displayName">Receives a string that contains the display name.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetDisplayName(
			[Out][MarshalAs(UnmanagedType.LPWStr)] out string displayName);

		/// <summary>
		/// Assigns a display name to the current audio session.
		/// </summary>
		/// <param name="displayName">A string that contains the new display name for the session.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetDisplayName(
			[In][MarshalAs(UnmanagedType.LPWStr)] string displayName,
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

		/// <summary>
		/// Retrieves the path for the display icon for the audio session.
		/// </summary>
		/// <param name="iconPath">Receives a string that specifies the fully qualified path of the file that contains the icon.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetIconPath(
			[Out][MarshalAs(UnmanagedType.LPWStr)] out string iconPath);

		/// <summary>
		/// Assigns a display icon to the current session.
		/// </summary>
		/// <param name="iconPath">A string that specifies the fully qualified path of the file that contains the new icon.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetIconPath(
			[In][MarshalAs(UnmanagedType.LPWStr)] string iconPath,
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

		/// <summary>
		/// Retrieves the grouping parameter of the audio session.
		/// </summary>
		/// <param name="groupingId">Receives the grouping parameter ID.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetGroupingParam(
			[Out] out Guid groupingId);

		/// <summary>
		/// Assigns a session to a grouping of sessions.
		/// </summary>
		/// <param name="groupingId">The new grouping parameter ID.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetGroupingParam(
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid groupingId,
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

		/// <summary>
		/// Registers the client to receive notifications of session events, including changes in the session state.
		/// </summary>
		/// <param name="client">A client-implemented <see cref="IAudioSessionEvents"/> interface.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int RegisterAudioSessionNotification(
			[In] IAudioSessionEvents client);

		/// <summary>
		/// Deletes a previous registration by the client to receive notifications.
		/// </summary>
		/// <param name="client">A client-implemented <see cref="IAudioSessionEvents"/> interface.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int UnregisterAudioSessionNotification(
			[In] IAudioSessionEvents client);
	}

	/// <summary>
	/// Windows CoreAudio ISimpleAudioVolume interface
	/// Defined in AudioClient.h
	/// </summary>
	[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface ISimpleAudioVolume
	{
		/// <summary>
		/// Sets the master volume level for the audio session.
		/// </summary>
		/// <param name="levelNorm">The new volume level expressed as a normalized value between 0.0 and 1.0.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetMasterVolume(
			[In][MarshalAs(UnmanagedType.R4)] float levelNorm,
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

		/// <summary>
		/// Retrieves the client volume level for the audio session.
		/// </summary>
		/// <param name="levelNorm">Receives the volume level expressed as a normalized value between 0.0 and 1.0. </param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetMasterVolume(
			[Out][MarshalAs(UnmanagedType.R4)] out float levelNorm);

		/// <summary>
		/// Sets the muting state for the audio session.
		/// </summary>
		/// <param name="isMuted">The new muting state.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetMute(
			[In][MarshalAs(UnmanagedType.Bool)] bool isMuted,
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

		/// <summary>
		/// Retrieves the current muting state for the audio session.
		/// </summary>
		/// <param name="isMuted">Receives the muting state.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetMute(
			[Out][MarshalAs(UnmanagedType.Bool)] out bool isMuted);
	}

	/// <summary>
	/// Windows CoreAudio IAudioSessionManager interface
	/// Defined in AudioPolicy.h
	/// </summary>
	[Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IAudioSessionManager
	{
		/// <summary>
		/// Retrieves an audio session control.
		/// </summary>
		/// <param name="sessionId">A new or existing session ID.</param>
		/// <param name="streamFlags">Audio session flags.</param>
		/// <param name="sessionControl">Receives an <see cref="IAudioSessionControl"/> interface for the audio session.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetAudioSessionControl(
			[In, Optional][MarshalAs(UnmanagedType.LPStruct)] Guid sessionId,
			[In][MarshalAs(UnmanagedType.U4)] UInt32 streamFlags,
			[Out][MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl sessionControl);

		/// <summary>
		/// Retrieves a simple audio volume control.
		/// </summary>
		/// <param name="sessionId">A new or existing session ID.</param>
		/// <param name="streamFlags">Audio session flags.</param>
		/// <param name="audioVolume">Receives an <see cref="ISimpleAudioVolume"/> interface for the audio session.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetSimpleAudioVolume(
			[In, Optional][MarshalAs(UnmanagedType.LPStruct)] Guid sessionId,
			[In][MarshalAs(UnmanagedType.U4)] UInt32 streamFlags,
			[Out][MarshalAs(UnmanagedType.Interface)] out ISimpleAudioVolume audioVolume);
	}

	[Guid("D666063F-1587-4E43-81F1-B948E807363F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
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
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IMMDeviceCollection
	{
		int GetCount(out int numDevices);
		int Item(int deviceNumber, out IMMDevice device);
	}

	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
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

	[Guid("82149A85-DBA6-4487-86BB-EA8F7FEFCC71"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface ISubunit
	{
		// Stub, Not Implemented
	}

	[Guid("45d37c3f-5140-444a-ae24-400789f3cbf3"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IControlInterface
	{
	}

	/// <summary>
	/// Windows CoreAudio IPartsList interface
	/// Defined in devicetopology.h
	/// </summary>
	[Guid("6DAA848C-5EB0-45CC-AEA5-998A2CDA1FFB"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IPartsList
	{
		int GetCount(out uint count);
		int GetPart(uint index, out IPart part);
	}

	[Guid("9c2c4058-23f5-41de-877a-df3af236a09e"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	interface IControlChangeNotify
	{
		[PreserveSig]
		int OnNotify(
			[In] uint controlId,
			[In] IntPtr context);
	}

	/// <summary>
	/// Windows CoreAudio IPart interface
	/// Defined in devicetopology.h
	/// </summary>
	[Guid("AE2DE0E4-5BCA-4F2D-AA46-5D13F8FDB3A9"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IPart
	{
		int GetName(
			[Out, MarshalAs(UnmanagedType.LPWStr)] out string name);

		int GetLocalId(
			[Out] out uint id);

		int GetGlobalId(
			[Out, MarshalAs(UnmanagedType.LPWStr)] out string id);

		int GetPartType(
			[Out] out PartTypeEnum partType);

		int GetSubType(
			out Guid subType);

		int GetControlInterfaceCount(
			[Out] out uint count);

		int GetControlInterface(
			[In] uint index,
			[Out, MarshalAs(UnmanagedType.IUnknown)] out IControlInterface controlInterface);

		[PreserveSig]
		int EnumPartsIncoming(
			[Out] out IPartsList parts);

		[PreserveSig]
		int EnumPartsOutgoing(
			[Out] out IPartsList parts);

		int GetTopologyObject(
			[Out] out object topologyObject);

		[PreserveSig]
		int Activate(
			[In] ClsCtx dwClsContext,
			[In] ref Guid refiid,
			[MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

		int RegisterControlChangeCallback(
			[In] ref Guid refiid,
			[In] IControlChangeNotify notify);

		int UnregisterControlChangeCallback(
			[In] IControlChangeNotify notify);
	}

	/// <summary>
	/// Windows CoreAudio IDeviceTopology interface
	/// Defined in devicetopology.h
	/// </summary>
	[Guid("2A07407E-6497-4A18-9787-32F79BD0D98F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IDeviceTopology
	{
		int GetConnectorCount(out uint count);
		int GetConnector(uint index, out IConnector connector);
		int GetSubunitCount(out uint count);
		int GetSubunit(uint index, out ISubunit subunit);
		int GetPartById(uint id, out IPart part);
		int GetDeviceId([MarshalAs(UnmanagedType.LPWStr)] out string id);
		int GetSignalPath(IPart from, IPart to, bool rejectMixedPaths, out IPartsList parts);
	}

	/// <summary>
	/// Windows CoreAudio IConnector interface
	/// Defined in devicetopology.h
	/// </summary>
	[Guid("9C2C4058-23F5-41DE-877A-DF3AF236A09E"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IConnector
	{
		int GetType(out ConnectorType type);
		int GetDataFlow(out DataFlow flow);
		int ConnectTo([In] IConnector connectTo);
		int Disconnect();
		int IsConnected(out bool connected);
		int GetConnectedTo(out IConnector conTo);
		int GetConnectorIdConnectedTo([MarshalAs(UnmanagedType.LPWStr)] out string id);
		int GetDeviceIdConnectedTo([MarshalAs(UnmanagedType.LPWStr)] out string id);
	}

	/// <summary>
	/// IMMNotificationClient
	/// </summary>
	[Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
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

	/// <summary>
	/// is defined in propsys.h
	/// </summary>
	[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IPropertyStore
	{
		int GetCount(out int propCount);
		int GetAt(int property, out PropertyKey key);
		int GetValue(ref PropertyKey key, out PropVariant value);
		int SetValue(ref PropertyKey key, ref PropVariant value);
		int Commit();
	}

	[Guid("DF45AEEA-B74A-4B6B-AFAD-2366B6AA012E"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IAudioMute
	{
		[PreserveSig]
		int GetMute(
			[Out, MarshalAs(UnmanagedType.Bool)] out bool mute);

		[PreserveSig]
		int SetMute(
			[In, MarshalAs(UnmanagedType.Bool)] bool mute,
			[In] ref Guid eventContext);
	}

	[Guid("7FB7B48F-531D-44A2-BCB3-5AD5A134B3DC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IPerChannelDbLevel
	{
		int GetChannelCount(out uint channels);
		int GetLevelRange(uint channel, out float minLevelDb, out float maxLevelDb, out float stepping);
		int GetLevel(uint channel, out float levelDb);
		int SetLevel(uint channel, float levelDb, ref Guid eventGuidContext);
		int SetLevelUniform(float levelDb, ref Guid eventGuidContext);
		int SetLevelAllChannel(float[] levelsDb, uint channels, ref Guid eventGuidContext);
	}

	[Guid("7FB7B48F-531D-44A2-BCB3-5AD5A134B3DC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IAudioVolumeLevel : IPerChannelDbLevel
	{

	}

	/// <summary>
	/// implements IMMDeviceEnumerator
	/// </summary>
	[ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	internal class MMDeviceEnumeratorComObject
	{
	}

	/// <summary>
	/// defined in MMDeviceAPI.h
	/// </summary>
	[Guid("1BE09788-6894-4089-8586-9A2A6C265AC5"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 ComImport]
	internal interface IMMEndpoint
	{
		int GetDataFlow(out DataFlow dataFlow);
	}
}
#endif