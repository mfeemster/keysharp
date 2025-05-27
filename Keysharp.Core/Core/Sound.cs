namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for sound-related functions.
	/// </summary>
	public static class Sound
	{
#if LINUX
		private static Dictionary<int, string> GetDevices(bool sinks)
		{
			var devices = new Dictionary<int, string>();
			var arg = sinks ? "sinks" : "sources";
			var str = $"pactl list {arg} short".Bash();

			foreach (var line in str.SplitLines())
			{
				var splits = line.Split(SpaceTab, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

				if (splits.Length > 1)
				{
					if (int.TryParse(splits[0], out var index))
						_ = devices.GetOrAdd(index, splits[1]);
				}
			}

			return devices;
		}
#endif

		/// <summary>
		/// Emits a tone from the PC speaker.
		/// </summary>
		/// <param name="frequency">If omitted, it defaults to 523. Otherwise, specify the frequency of the sound, a number between 37 and 32767.</param>
		/// <param name="duration">If omitted, it defaults to 150. Otherwise, specify the duration of the sound, in milliseconds.</param>
		public static object SoundBeep(object frequency = null, object duration = null)
		{
			var freq = frequency.Ai(523);
			var time = duration.Ai(150);
#if LINUX
			var seconds = time / 1000.0;
			$"speaker-test -t sine -f {freq} -l 1 & sleep {seconds} && kill -9 $!".Bash();
#elif WINDOWS
			Console.Beep(freq, time);
#endif
			return null;
		}

#if WINDOWS

		/// <summary>
		/// Retrieves a native COM interface of a sound device or component.
		/// </summary>
		/// <param name="id">An interface identifier (GUID) in the form "{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}".</param>
		/// <param name="component">If blank or omitted, an interface implemented by the device itself will be retrieved. Otherwise, specify the component's display name and/or index, e.g. 1, "Line in" or "Line in:2".</param>
		/// <param name="device">If blank or omitted, it defaults to the system's default device for playback<br/>
		/// (which is not necessarily device 1). Otherwise, specify the device's display name and/or index,<br/>
		/// e.g. 1, "Speakers", "Speakers:2" or "Speakers (Example HD Audio)".
		/// </param>
		/// <returns>The COM interface for the specified sound interface.</returns>
		public static object SoundGetInterface(object id, object component = null, object device = null) => DoSound(SoundCommands.SoundGetInterface, id, component, device);

#endif

		/// <summary>
		/// Retrieves a mute setting of a sound device.
		/// </summary>
		/// <param name="component">If blank or omitted, it defaults to the master mute setting. Otherwise, specify the component's display name and/or index, e.g. 1, "Line in" or "Line in:2".</param>
		/// <param name="device">If blank or omitted, it defaults to the system's default device for playback<br/>
		/// (which is not necessarily device 1). Otherwise, specify the device's display name and/or index,<br/>
		/// e.g. 1, "Speakers", "Speakers:2" or "Speakers (Example HD Audio)".
		/// </param>
		/// <returns>0 for unmuted, else 1.</returns>
		public static object SoundGetMute(object component = null, object device = null) => DoSound(SoundCommands.SoundGetMute, component, device);

		/// <summary>
		/// Retrieves the name of a sound device or component.
		/// </summary>
		/// <param name="component">If blank or omitted, it defaults to the master mute setting. Otherwise, specify the component's display name and/or index, e.g. 1, "Line in" or "Line in:2".</param>
		/// <param name="device">If blank or omitted, it defaults to the system's default device for playback<br/>
		/// (which is not necessarily device 1). Otherwise, specify the device's display name and/or index,<br/>
		/// e.g. 1, "Speakers", "Speakers:2" or "Speakers (Example HD Audio)".
		/// </param>
		/// <returns>The name of the device or component, which can be empty.</returns>
		public static object SoundGetName(object component = null, object device = null) => DoSound(SoundCommands.SoundGetName, component, device);

		/// <summary>
		/// Retrieves a volume setting of a sound device.
		/// </summary>
		/// <param name="component">If blank or omitted, it defaults to the master mute setting. Otherwise, specify the component's display name and/or index, e.g. 1, "Line in" or "Line in:2".</param>
		/// <param name="device">If blank or omitted, it defaults to the system's default device for playback<br/>
		/// (which is not necessarily device 1). Otherwise, specify the device's display name and/or index,<br/>
		/// e.g. 1, "Speakers", "Speakers:2" or "Speakers (Example HD Audio)".
		/// </param>
		/// <returns>A floating point number between 0.0 and 100.0.</returns>
		public static object SoundGetVolume(object component = null, object device = null) => DoSound(SoundCommands.SoundGetVolume, component, device);

		/// <summary>
		/// Plays a sound, video, or other supported file type.
		/// </summary>
		/// <param name="filename">
		/// The name of the file to be played, which is assumed to be in <see cref="A_WorkingDir"/> if an absolute path isn't specified.<br/>
#if WINDOWS
		/// To produce standard system sounds, specify an asterisk followed by a number as shown below (note that the Wait parameter has no effect in this mode):<br/>
		/// *-1: simple beep<br/>
		/// *16: hand (stop/error)<br/>
		/// *32: question<br/>
		/// *48: exclamation<br/>
		/// *64: asterisk (info)<br/>
#endif
		/// </param>
		/// <param name="wait">If blank or omitted, it defaults to 0 (false). Otherwise, specify one of the following values:<br/>
		///     0 (false): The current thread will move on to the next statement(s) while the file is playing.<br/>
		///     1 (true) or Wait: The current thread waits until the file is finished playing before continuing.<br/>
		///     Even while waiting, new threads can be launched via hotkey, custom menu item, or timer.<br/>
		///     Known limitation: If the Wait parameter is not used, the system might consider the playing file to<br/>
		///     be "in use" until the script closes or until another file is played(even a nonexistent file).<br/>
		/// </param>
		/// <exception cref="Error">An <see cref="Error"/> exception is thrown on failure.</exception>
		public static void SoundPlay(object filename, object wait = null)
		{
			var file = filename.As();
			var w = wait.As();
#if WINDOWS

			if (file.Length > 1 && file[0] == '*')
			{
				if (!int.TryParse(file.AsSpan(1), out var n))
				{
					return;
				}

				switch (n)
				{
					case -1: SystemSounds.Beep.Play(); return;

					case 16: SystemSounds.Hand.Play(); return;

					case 32: SystemSounds.Question.Play(); return;

					case 48: SystemSounds.Exclamation.Play(); return;

					case 64: SystemSounds.Asterisk.Play(); return;

					default: return;
				}
			}

#endif

			try
			{
				var doWait = w == "1" || string.Compare(w, "WAIT", true) == 0;
#if WINDOWS
				var sound = new SoundPlayer(file);

				if (doWait)
					sound.PlaySync();
				else
					sound.Play();

#else
				$"aplay --quiet {filename}".Bash(doWait);
#endif
			}
			catch (Exception ex)
			{
				Error err;
				_ = Errors.ErrorOccurred(err = new Error(ex.Message)) ? throw err : "";
			}
		}

		/// <summary>
		/// Changes a mute setting of a sound device.
		/// </summary>
		/// <param name="newSetting">One of the following values:<br/>
		///     1 or True: turns on the setting.<br/>
		///     0 or False: turns off the setting.<br/>
		///    -1: toggles the setting(sets it to the opposite of its current state).
		/// </param>
		/// <param name="component">If blank or omitted, it defaults to the master mute setting. Otherwise, specify the component's display name and/or index, e.g. 1, "Line in" or "Line in:2".</param>
		/// <param name="device">If blank or omitted, it defaults to the system's default device for playback<br/>
		/// (which is not necessarily device 1). Otherwise, specify the device's display name and/or index,<br/>
		/// e.g. 1, "Speakers", "Speakers:2" or "Speakers (Example HD Audio)".
		/// </param>
		public static object SoundSetMute(object newSetting, object component = null, object device = null)
		{
			_ = DoSound(SoundCommands.SoundSetMute, newSetting, component, device);
			return null;
		}


		/// <summary>
		/// Changes a volume setting of a sound device.
		/// </summary>
		/// <param name="newSetting">A string containing a percentage number between -100 and 100 inclusive.<br/>
		/// If the number begins with a plus or minus sign, the current setting will be adjusted up or down by the indicated amount.<br/>
		/// Otherwise, the setting will be set explicitly to the level indicated by newSetting.<br/>
		/// If the percentage number begins with a minus sign or is unsigned, it does not need to be enclosed in quotation marks.
		/// </param>
		/// <param name="component">If blank or omitted, it defaults to the master mute setting. Otherwise, specify the component's display name and/or index, e.g. 1, "Line in" or "Line in:2".</param>
		/// <param name="device">If blank or omitted, it defaults to the system's default device for playback<br/>
		/// (which is not necessarily device 1). Otherwise, specify the device's display name and/or index,<br/>
		/// e.g. 1, "Speakers", "Speakers:2" or "Speakers (Example HD Audio)".
		/// </param>
		public static object SoundSetVolume(object newSetting, object component = null, object device = null)
		{
			_ = DoSound(SoundCommands.SoundSetVolume, newSetting, component, device);
			return null;
		}

#if LINUX
		private static object DoSound(SoundCommands soundCmd, object obj0, object obj1 = null, object obj2 = null)
		{
			Error err;
			var soundSet = false;
			var device = obj1;
			SoundControlType type;
			var sink = true;

			if (soundCmd >= SoundCommands.SoundSetVolume)
			{
				soundSet = true;
				type = (SoundControlType)((int)soundCmd - (int)SoundCommands.SoundSetVolume);
				sink = obj1.Ab(true);
				device = obj2;
			}
			else
			{
				sink = obj0.Ab(true);
				type = (SoundControlType)(int)soundCmd;
			}

			var settingScalar = 0.0;

			if (soundSet)
				settingScalar = Math.Clamp(obj0.Ad() * 0.01 * 65536.0, -65536.0, 65536.0);//pactl uses a range of 0-65536.

			var valStr = obj0 == null ? "" : obj0.ToString();
			var adjust = valStr.Length > 0 && (valStr[0] == '-' || valStr[0] == '+');
			var found = false;
			var sinkStr = sink ? "Sink" : "Source";
			var devices = GetDevices(sink);
			var devStr = "";

			if (device == null)
			{
				devStr = sink ? "@DEFAULT_SINK@" : "@DEFAULT_SOURCE@";
				found = true;
			}
			else
			{
				devStr = device.ToString();

				if (int.TryParse(devStr, out var deviceIndex) && devices.TryGetValue(deviceIndex, out var _))
				{
					found = true;
				}
				else
				{
					foreach (var devKv in devices)
					{
						if (devKv.Value.StartsWith(devStr, StringComparison.OrdinalIgnoreCase))
						{
							found = true;
							break;
						}
					}
				}

				if (!found)
					return Errors.ErrorOccurred(err = new TargetError($"{sinkStr} device {device} not found.")) ? throw err : null;
			}

			sinkStr = sinkStr.ToLower();

			switch (soundCmd)
			{
				case SoundCommands.SoundGetVolume:
				{
					var ret = $"pactl get-{sinkStr}-volume {devStr}".Bash();
					var lines = ret.SplitLines().ToList();

					if (lines.Count > 1)
					{
						var lines0 = lines[0];
						var firstPercent = lines0.IndexOf('%');
						var lastPercent = lines0.LastIndexOf('%');
						double prc1 = 1.0, prc2 = 1.0;

						if (firstPercent != -1)
						{
							var val1Index = lines0.AsSpan(0, firstPercent).LastIndexOf(' ');

							if (val1Index != -1)
							{
								var val1 = lines0.AsSpan(val1Index + 1, (firstPercent - val1Index) - 1);

								if (!double.TryParse(val1, out prc1))
									return Errors.ErrorOccurred(err = new OSError("", $"Could not parse first volume value of {val1}.")) ? throw err : null;
							}
						}

						if (lastPercent != -1 && lastPercent > firstPercent)
						{
							var val2Index = lines0.AsSpan(0, lastPercent).LastIndexOf(' ');

							if (val2Index != -1)
							{
								var val2 = lines0.AsSpan(val2Index + 1, (lastPercent - val2Index) - 1);

								if (!double.TryParse(val2, out prc2))
									return Errors.ErrorOccurred(err = new OSError("", $"Could not parse second volume value of {val2}.")) ? throw err : null;
							}
						}

						return (prc1 + prc2) / 2.0;
					}
				}
				break;

				case SoundCommands.SoundGetMute:
				{
					var ret = $"pactl get-{sinkStr}-mute {devStr}".Bash();
					return ret.EndsWith("yes", StringComparison.OrdinalIgnoreCase) ? 1L : 0L;
				}

				case SoundCommands.SoundGetName:
				{
					if (device == null)
					{
						return "pactl get-default-sink".Bash();
					}
					else if (int.TryParse(devStr, out var deviceIndex))
					{
						if (!devices.TryGetValue(deviceIndex, out var deviceName))
							return Errors.ErrorOccurred(err = new TargetError($"{sinkStr} device {device} not found.")) ? throw err : null;
						else
							return deviceName;
					}
					else
						return Errors.ErrorOccurred(err = new TargetError($"{devStr} was not a valid integer.")) ? throw err : null;
				}

				case SoundCommands.SoundSetVolume:
				{
					if (adjust)
					{
						var currentVolume = SoundGetVolume(obj0, obj1).Ad() * 0.01 * 65536.0;
						settingScalar = Math.Clamp(currentVolume + settingScalar, 0.0, 65536.0);
					}

					_ = $"pactl set-{sinkStr}-volume {devStr} {(int)settingScalar}".Bash();
				}
				break;

				case SoundCommands.SoundSetMute:
				{
					var act = Conversions.ConvertOnOffToggle(obj0);
					_ = $"pactl set-{sinkStr}-mute {devStr} {(act == ToggleValueType.On ? "1" : act == ToggleValueType.Toggle ? "-1" : "0")}".Bash();
				}
				break;

				default:
					break;
			}

			return "";
		}

#elif WINDOWS

		/// <summary>
		/// Internal helper to help with various sound processing commands.
		/// </summary>
		/// <param name="soundCmd">The sound command to perform.</param>
		/// <param name="obj0">The sound component to operate on, or the value to use.</param>
		/// <param name="obj1">The sound device to operate on, or the sound component.</param>
		/// <param name="obj2">The sound device to operate on.</param>
		/// <returns>Various values depending on the sound command being processed.</returns>
		/// <exception cref="TargetError">A <see cref="TargetError"/> exception is thrown if the component/device cannot not found.</exception>
		/// <exception cref="Error">An <see cref="Error"/> exception is thrown if the channels, levels or range cannot not found.</exception>
		private static object DoSound(SoundCommands soundCmd, object obj0, object obj1 = null, object obj2 = null)
		{
			Error err;
			var soundSet = false;
			var search = new SoundComponentSearch();
			var comp = obj0;
			var dev = obj1;

			if (soundCmd >= SoundCommands.SoundSetVolume)
			{
				soundSet = true;
				search.targetControl = (SoundControlType)((int)soundCmd - (int)SoundCommands.SoundSetVolume);
				comp = obj1;
				dev = obj2;
			}
			else
			{
				search.targetControl = (SoundControlType)(int)soundCmd;
			}

			switch (search.targetControl)
			{
				case SoundControlType.Volume:
					search.targetIid = new Guid("7FB7B48F-531D-44A2-BCB3-5AD5A134B3DC");
					break;

				case SoundControlType.Mute:
					search.targetIid = new Guid("DF45AEEA-B74A-4B6B-AFAD-2366B6AA012E");
					break;

				case SoundControlType.IID:
					search.targetIid = new Guid(obj0.As());
					comp = obj1;
					dev = obj2;
					break;
			}

			var settingScalar = 0.0f;

			if (soundSet)
				settingScalar = Math.Clamp((float)(obj0.Ad() * 0.01), -1.0f, 1.0f);

			var resultFloat = 0.0f;
			var resultBool = false;
			var valStr = obj0 == null ? "" : obj0.ToString();
			var adjust = valStr.Length > 0 && (valStr[0] == '-' || valStr[0] == '+');
			var mmDev = GetDevice(dev);

			if (mmDev == null)
				return Errors.ErrorOccurred(err = new TargetError($"Component {comp}, device {dev} not found.")) ? throw err : null;

			if (comp == null || comp.ToString().Length == 0)//Component is Master (omitted).
			{
				if (search.targetControl == SoundControlType.IID)
				{
					_ = mmDev.deviceInterface.Activate(ref search.targetIid, ClsCtx.ALL, IntPtr.Zero, out var result);
					//Need the specific interface pointer, else ComCall() will fail when using IAudioMeterInformation.
					var iptr = Marshal.GetIUnknownForObject(result);

					if (Marshal.QueryInterface(iptr, in search.targetIid, out var ptr) >= 0)
						result = ptr;

					_ = Marshal.Release(iptr);
					return result;
				}
				else if (search.targetControl == SoundControlType.Name)
				{
					return mmDev.FriendlyName;
				}
				else
				{
					var aev = mmDev.AudioEndpointVolume;

					if (search.targetControl == SoundControlType.Volume)
					{
						if (!soundSet || adjust)
						{
							resultFloat = aev.MasterVolumeLevelScalar;
						}

						if (soundSet)
						{
							if (adjust)
								settingScalar = Math.Clamp(settingScalar + resultFloat, 0.0f, 1.0f);

							aev.MasterVolumeLevelScalar = settingScalar;
						}
						else
						{
							resultFloat *= 100;
							return (double)resultFloat;
						}
					}
					else//Mute.
					{
						if (!soundSet || adjust)
							resultBool = aev.Mute;

						if (soundSet)
							aev.Mute = adjust ? !resultBool : settingScalar > 0;
						else
							return resultBool;
					}
				}
			}
			else
			{
				if (comp is string cs && cs.Length > 0)
				{
					var splits = search.targetName.Split(':');

					if (splits.Length > 1)
					{
						search.targetName = splits[0];
						search.targetInstance = splits[1].Ai();
					}
					else
					{
						search.name = cs;
						search.targetInstance = 1;
					}
				}
				else
				{
					search.targetName = "";
					search.targetInstance = comp.Ai();
				}

				if (!FindComponent(mmDev, search))
				{
					return Errors.ErrorOccurred(err = new TargetError($"Component {comp} not found.")) ? throw err : null;
				}
				else if (search.targetControl == SoundControlType.IID)
				{
					return search.control;//The IntPtr.
				}
				else if (search.targetControl == SoundControlType.Name)
				{
					return search.name;
				}
				else if (search.control == null)
				{
					//Throw?
				}
				else if (search.targetControl == SoundControlType.Volume)
				{
					object comobj = search.control is IntPtr ip ? Marshal.GetObjectForIUnknown(ip) : search.control;

					if (comobj is IAudioVolumeLevel avl)
					{
						if (avl.GetChannelCount(out var channelCount) < 0)
							return Errors.ErrorOccurred(err = new Error("Could not get channel count.")) ? throw err : null;

						float[] level = new float[3 * channelCount];
						float f, maxLevel = 0;

						for (var ii = 0u; ii < 0; ++ii)
						{
							if (avl.GetLevel(ii, out var db) < 0 ||
									avl.GetLevelRange(ii, out var minDb, out var maxDb, out f) < 0)
								return Errors.ErrorOccurred(err = new Error("Could not get level or level range.")) ? throw err : null;

							//Convert dB to scalar.
							var levelMin = 0 + ii;
							var levelRange = levelMin + 0;
							level[levelMin] = (float)Math.Pow(10.0, minDb / 20.0);
							level[levelRange] = (float)Math.Pow(10.0, maxDb / 20.0) - level[levelMin];
							//Compensate for differing level ranges. (No effect if range is -96..0 dB.)
							level[ii] = ((float)Math.Pow(10.0, db / 20.0) - level[levelMin]) / level[levelRange];

							// Windows reports the highest level as the overall volume.
							if (maxLevel < level[ii])
								maxLevel = level[ii];
						}

						if (soundSet)
						{
							if (adjust)
								settingScalar = Math.Clamp(settingScalar + maxLevel, 0.0f, 1.0f);

							for (var ii = 0; ii < (uint)0; ++ii)
							{
								var levelMin = (uint)0 + ii;
								var levelRange = levelMin + 0;
								f = settingScalar;

								if (maxLevel != 0)
									f *= level[ii] / maxLevel;//Preserve balance.

								f = level[levelMin] + f * level[levelRange];//Compensate for differing level ranges.
								level[ii] = 20 * (float)Math.Log(10.0, f);//Convert scalar to dB.
							}

							Guid guid = Guid.Empty;
							_ = avl.SetLevelAllChannel(level, 0, ref guid);
						}
						else
							resultFloat = maxLevel * 100;
					}
				}
				else if (search.targetControl == SoundControlType.Mute)
				{
					object comobj = search.control is IntPtr ip ? Marshal.GetObjectForIUnknown(ip) : search.control;

					if (comobj is IAudioMute am)
					{
						var res = 0;

						if (!soundSet || adjust)
							res = am.GetMute(out resultBool);

						if (soundSet && res >= 0)
						{
							Guid guid = Guid.Empty;
							_ = am.SetMute(adjust ? !resultBool : settingScalar > 0, ref guid);
						}
					}
				}
			}

			return search.targetControl switch
		{
				SoundControlType.Volume => (double)resultFloat,
					SoundControlType.Mute => resultBool,
					_ => null,
			};
		}

		/// <summary>
		/// Internal helper to determine whether a specific device exists.
		/// </summary>
		/// <param name="mmDev">The device to search for.</param>
		/// <param name="search">The type of search to do.</param>
		/// <returns>True if found, else false.</returns>
		private static bool FindComponent(MMDevice mmDev, SoundComponentSearch search)
		{
			search.count = 0;
			search.control = null;
			search.name = null;
			search.ignoreRemainingSubunits = false;
			var top = mmDev.DeviceTopology;

			if (top.GetConnector(0, out var conn) >= 0)
			{
				if (conn.GetDataFlow(out var flow) >= 0)
				{
					if (conn.GetConnectedTo(out var conTo) >= 0)
					{
						if (conTo is IPart part)
							_ = FindComponent(part, search);
					}
				}
			}

			return search.count == search.targetInstance;
		}

		/// <summary>
		/// Internal helper to determine whether a specific component exists.
		/// </summary>
		/// <param name="root">The root of the device hierarchy.</param>
		/// <param name="search">The type of search to do.</param>
		/// <returns>True if found, else false.</returns>
		private static bool FindComponent(IPart root, SoundComponentSearch search)
		{
			IPartsList partsList;

			if ((search.dataFlow == DataFlow.Render ?
					root.EnumPartsIncoming(out partsList) :
					root.EnumPartsOutgoing(out partsList)) < 0)
				return false;

			if (partsList.GetCount(out var partCount) < 0)
				partCount = 0;

			for (var i = 0u; i < partCount; i++)
			{
				if (partsList.GetPart(i, out var part) < 0)
					continue;

				if (root.GetPartType(out var partType) >= 0)
				{
					if (partType == PartTypeEnum.Connector)
					{
						if (partCount == 1//Ignore Connectors with no Subunits of their own.
								&& (!string.IsNullOrEmpty(search.targetName) ||
									(part.GetName(out var partName) >= 0 && partName.StartsWith(search.targetName, StringComparison.OrdinalIgnoreCase))
								   )
						   )
						{
							if (++search.count == search.targetInstance)
							{
								switch (search.targetControl)
								{
									case SoundControlType.Volume:
										break;

									case SoundControlType.Mute:
										break;

									case SoundControlType.Name:
										_ = part.GetName(out search.name);
										break;

									case SoundControlType.IID:
									{
										//Permit retrieving the IPart or IConnector itself.  Since there may be
										//multiple connected Subunits (and they can be enumerated or retrieved
										//via the Connector IPart), this is only done for the Connector.
										//Need the specific interface pointer, else ComCall() will fail when using IAudioMeterInformation.
										var iptr = Marshal.GetIUnknownForObject(part);

										if (Marshal.QueryInterface(iptr, in search.targetIid, out var ptr) >= 0)
										{
											if (ptr != IntPtr.Zero)
												search.control = ptr;
										}

										_ = Marshal.Release(iptr);
										break;
									}
								}

								return true;
							}
						}
						else//Subunit.
						{
							//Recursively find the Connector nodes linked to this part.
							if (FindComponent(part, search))
							{
								//A matching connector part has been found with this part as one of the nodes used
								//to reach it.  Therefore, if this part supports the requested control interface,
								//it can in theory be used to control the component.  An example path might be:
								//   Output < Master Mute < Master Volume < Sum < Mute < Volume < CD Audio
								//Parts are considered from right to left, as we return from recursion.
								if (search.control == null && !search.ignoreRemainingSubunits)
								{
									//Query this part for the requested interface and let caller check the result.
									_ = part.Activate(ClsCtx.ALL, ref search.targetIid, out search.control);
									//Need the specific interface pointer, else ComCall() will fail when using IAudioMeterInformation.
									var iptr = Marshal.GetIUnknownForObject(search.control);

									if (Marshal.QueryInterface(iptr, in search.targetIid, out var ptr) >= 0)
									{
										if (ptr != IntPtr.Zero)
											search.control = ptr;
									}

									_ = Marshal.Release(iptr);

									//If this subunit has siblings, ignore any controls further up the line
									//as they're likely shared by other components (i.e. master controls).
									if (partCount > 1)
										search.ignoreRemainingSubunits = true;
								}

								return true;
							}
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Internal helper to get a device from a string description or a number.
		/// </summary>
		/// <param name="obj0">The name or number of the device to search for.</param>
		/// <returns>The device if found, else null.</returns>
		private static MMDevice GetDevice(object obj0)
		{
			var deviceEnum = new MMDeviceEnumerator();
			MMDevice mmDev = null;

			if (obj0 == null || obj0.ToString() == "")
			{
				mmDev = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
			}
			else
			{
				var targetIndex = 0;
				var targetName = "";

				if (obj0 is string ds && ds.Length > 0)
				{
					var splits = ds.Split(':');

					if (splits.Length > 1)
					{
						targetName = splits[0];
						targetIndex = splits[1].Ai() - 1;
					}
					else
						targetName = ds;
				}
				else
				{
					targetName = "";
					targetIndex = obj0.Ai() - 1;
				}

				var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active | DeviceState.Unplugged).ToList();

				if (targetName.Length > 0)
				{
					foreach (var device in devices)
					{
						//Keysharp.Core.Dialogs.MsgBox(device.FriendlyName
						//                           + "\r\n" + device.DeviceFriendlyName
						//                           + "\r\n" + device.ID
						//                           + "\r\n" + device.InstanceId);
						if (device.FriendlyName.StartsWith(targetName, StringComparison.OrdinalIgnoreCase) && targetIndex-- == 0)
						{
							mmDev = device;
							break;
						}
					}
				}
				else
				{
					if (targetIndex < devices.Count)
						mmDev = devices[targetIndex];
				}
			}

			return mmDev;
		}

		/// <summary>
		/// Internal helper to aid in searching for devices and components.
		/// </summary>
		private class SoundComponentSearch
		{
			//Internal use/results:
			internal object control;

			internal int count;

			//Internal use:
			internal DataFlow dataFlow = DataFlow.Render;

			internal bool ignoreRemainingSubunits;
			internal string name;
			internal SoundControlType targetControl;

			//Parameters of search:
			internal Guid targetIid;

			internal int targetInstance;
			internal string targetName;
			// Valid only when target_control == SoundControlType::Name.
		};
#else
		private static object DoSound(SoundCommands soundCmd, object obj0, object obj1 = null, object obj2 = null)
		{
			return null;
		}
#endif

			/// <summary>
			/// Enum for specifying different sound operations which will be passed to <see cref="DoSound(SoundCommands, object, object, object)"/>
			/// </summary>
		private enum SoundCommands
		{
			SoundGetVolume = 0, SoundGetMute, SoundGetName
#if WINDOWS
			, SoundGetInterface
#endif
			, SoundSetVolume, SoundSetMute
		}

		/// <summary>
		/// Enum for specifying different sound control types.
		/// </summary>
		private enum SoundControlType
		{
			Volume,
			Mute,
			Name
#if WINDOWS
			, IID
#endif
		}
	}
}