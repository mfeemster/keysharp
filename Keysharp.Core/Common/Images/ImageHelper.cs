namespace Keysharp.Core.Common
{
	internal static class ImageHelper
	{
		internal static Bitmap ConvertCursorToBitmap(Cursor c)
		{
			var bmp = new Bitmap(c.Size.Width, c.Size.Height);

			using (var g = Graphics.FromImage(bmp))
			{
				c.Draw(g, new Rectangle(0, 0, c.Size.Width, c.Size.Height));
			}

			return bmp;
		}

		internal static bool IsIcon(string filename)
		{
			var ext = System.IO.Path.GetExtension(filename).ToLower();
			return ext == ".exe" || ext == ".dll" || ext == ".icl" || ext == ".cpl" || ext == ".scr" || ext == ".ico";
		}

		internal static Icon LoadIconFromAssembly(string path, string iconName)
		{
			Icon icon = null;

			if (Reflections.loadedAssemblies.TryGetValue(path, out var assembly))
			{
				icon = LoadIconHelper(assembly, iconName);
			}
			else//Hasn't been loaded, so temporarily load it.
			{
				try
				{
					var ac = new UnloadableAssemblyLoadContext(path);
					assembly = ac.LoadFromAssemblyPath(path);
					icon = LoadIconHelper(assembly, iconName);
					ac.Unload();
				}
				catch
				{
				}
			}

			return icon;
		}

		internal static Icon LoadIconHelper(Assembly assembly, string iconName)
		{
			Icon icon = null;
			var resourceNames = assembly.GetManifestResourceNames();
			var trim = ".resources";

			foreach (var resourceName in resourceNames)
			{
				var trimmedName = resourceName.EndsWith(".resources", StringComparison.CurrentCulture) ? resourceName.Substring(0, resourceName.Length - trim.Length) : resourceName;
				var resource = new global::System.Resources.ResourceManager(trimmedName, assembly);

				try
				{
					icon = (Icon)resource.GetObject(iconName);

					if (icon != null)
						break;
				}
				catch { }
			}

			return icon;
		}

		internal static (Bitmap, object) LoadImage(string filename, int w, int h, object iconindex)
		{
			Bitmap bmp = null;
			object temp = null;

			try
			{
#if WINDOWS

				if (filename.StartsWith("HICON:", StringComparison.OrdinalIgnoreCase))
				{
					var hstr = filename.AsSpan(6);
					var dontClear = hstr[0] == '*';

					if (dontClear)
						hstr = hstr.Trim('*');

					if (long.TryParse(hstr, out var handle))
					{
						var tempico = Icon.FromHandle(new IntPtr(handle));
						bmp = tempico.ToBitmap();

						if (!dontClear)
							_ = WindowsAPI.DestroyIcon(tempico.Handle);
						else
							temp = tempico;
					}
				}
				else
#endif
					if (filename.StartsWith("HBITMAP:", StringComparison.OrdinalIgnoreCase))
					{
						var hstr = filename.AsSpan(8);
						var dontClear = hstr[0] == '*';

						if (dontClear)
							hstr = hstr.Trim('*');

						if (long.TryParse(hstr, out var handle))
						{
							var ptr = new IntPtr(handle);
							bmp = System.Drawing.Image.FromHbitmap(ptr);
#if WINDOWS

							if (!dontClear)
								_ = WindowsAPI.DeleteObject(ptr);

#endif
						}
					}

				if (bmp == null)//Wasn't a handle, and instead was a filename.
				{
					var ext = System.IO.Path.GetExtension(filename).ToLower();

					if (ext == ".dll"
#if WINDOWS
							|| ext == ".exe" || ext == ".icl" || ext == ".cpl" || ext == ".scr"
#endif
					   )
					{
						Icon ico = null;

						if (iconindex is string iconstr)
							ico = LoadIconFromAssembly(filename, iconstr);

#if WINDOWS
						else if (iconindex is int iconint)
							ico = GuiHelper.GetIcon(filename, iconint);

#endif

						if (ico != null)
						{
							bmp = ico.ToBitmap();

							if (w > 0 || h > 0)
							{
								if (bmp.Width != w || bmp.Height != h)
									bmp = bmp.Resize(w, h);
							}
							else if (bmp.Size != SystemInformation.IconSize)
							{
								bmp = bmp.Resize(SystemInformation.IconSize.Width, SystemInformation.IconSize.Height);
							}

							temp = ico;
						}
					}
					else if (ext == ".ico")
					{
						var ico = new Icon(filename);
						var icos = GuiHelper.SplitIcon(ico);

						if (w > 0 && h > 0)
						{
							var tempico = icos.FirstOrDefault(tempico => tempico.Width == w && tempico.Height == h);

							if (tempico == null)
								tempico = icos[0];

							temp = tempico;
							bmp = tempico?.ToBitmap();
						}
						else if (iconindex.Ai() is int iconint)
						{
							if (iconint < icos.Count)
							{
								temp = icos[iconint];
								bmp = icos[iconint].ToBitmap();
							}
						}

						if (bmp == null)
						{
							temp = icos[0];
							bmp = icos[0].ToBitmap();
						}

						if (w > 0 || h > 0)
						{
							if (bmp.Width > w || bmp.Height > h)
								bmp = bmp.Resize(w, h);
						}
						else if (bmp.Size != SystemInformation.IconSize)
						{
							bmp = bmp.Resize(SystemInformation.IconSize.Width, SystemInformation.IconSize.Height);
						}
					}
					else if (ext == ".cur")
					{
						var tempcur = new Cursor(filename);
						var curbm = new Bitmap(tempcur.Size.Width, tempcur.Size.Height);

						using (var gr = Graphics.FromImage(curbm))
						{
							tempcur.Draw(gr, new Rectangle(0, 0, tempcur.Size.Width, tempcur.Size.Height));
							bmp = curbm;
							temp = tempcur;
						}
					}
					else
					{
						using (var tempBmp = (Bitmap)System.Drawing.Image.FromFile(filename))//Must make a copy because the original will keep the file locked.
						{
							bmp = new Bitmap(tempBmp);

							if (w > 0 || h > 0)
							{
								if (bmp.Width != w || bmp.Height != h)
									bmp = bmp.Resize(w, h);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new TypeError(ex.Message);
			}

			return (bmp, temp);
		}

		internal static object PrepareIconNumber(object iconnumber)
		{
			if (iconnumber == null)
				return 0;
			else if (iconnumber.ParseLong(false) is long l && l > 0)//Note this allows us to pass the icon number as a string, however that also prevents us from loading an icon from a .NET DLL that happens to be named that same number. This is an extremely unlikely scenario.
				return (int)l - 1;
			else
				return iconnumber;
		}

		internal static List<Bitmap> SplitBitmap(Bitmap bmp, int w, int h)
		{
			var list = new List<Bitmap>();

			for (var i = 0; i < bmp.Height; i += h)
				for (var j = 0; j < bmp.Width; j += w)
					if (i + h < bmp.Height && j + w < bmp.Width)
						list.Add(bmp.Clone(new Rectangle(j, i, w, h), bmp.PixelFormat));

			return list;
		}
	}

	internal sealed class GdiHandleHolder : KeysharpObject
	{
		private readonly bool disposeHandle = true;
		private readonly IntPtr handle;

		internal GdiHandleHolder(IntPtr h, bool d)
		{
			handle = h;
			disposeHandle = d;
		}

		~GdiHandleHolder()
		{
#if WINDOWS

			if (disposeHandle && handle != IntPtr.Zero)
				_ = WindowsAPI.DeleteObject(handle);//Windows specific, figure out how to do this, or if it's even needed on other platforms.//TODO

#endif
		}

		public static implicit operator long(GdiHandleHolder holder) => holder.handle.ToInt64();

		public override string ToString() => handle.ToInt64().ToString();
	}
}