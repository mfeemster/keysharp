namespace Keysharp.Core.Common.Images
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
			var ext = Path.GetExtension(filename).ToLower();
			return ext == ".exe" || ext == ".dll" || ext == ".icl" || ext == ".cpl" || ext == ".scr" || ext == ".ico";
		}

		internal static Icon LoadIconFromAssembly(string path, string iconName)
		{
			Icon icon = null;

			if (Script.TheScript.ReflectionsData.loadedAssemblies.TryGetValue(path, out var assembly))
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
				var resource = new System.Resources.ResourceManager(trimmedName, assembly);

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
						var tempico = Icon.FromHandle(new nint(handle));
						bmp = tempico.ToBitmap();
						bmp = ResizeBitmap(bmp, w, h);

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
							var ptr = new nint(handle);
#if WINDOWS
							bmp = GetBitmapFromHBitmap(ptr);
							bmp = ResizeBitmap(bmp, w, h);

							if (!dontClear)
									_ = WindowsAPI.DeleteObject(ptr);

#else
							bmp = Image.FromHbitmap(ptr);
#endif
						}
					}

				if (bmp == null)//Wasn't a handle, and instead was a filename.
				{
					var ext = Path.GetExtension(filename).ToLower();

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
						else
						{
							var idx = iconindex.Ai();
							ico = ExtractIconWithSizeFromModule(filename, idx, w, h) ?? GuiHelper.GetIcon(filename, idx);
						}

#endif

						if (ico != null)
						{
							bmp = ico.ToBitmap();

							if (w > 0 || h > 0)
							{
								bmp = ResizeBitmap(bmp, w, h);
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
						if (w > 0 && h < 0) h = w;
						if (h > 0 && w < 0) w = h;

						Icon ico = (w <= 0 || h <= 0) ? new Icon(filename) : new Icon(filename, w, h);

						var icos = GuiHelper.SplitIcon(ico);

						if (w > 0 || h > 0)
						{
							var tempIcoBmp = icos.FirstOrDefault(tempico => (w <= 0 || tempico.Item1.Width == w) && (h <= 0 || tempico.Item1.Height == h));
							var tempIco = tempIcoBmp.Item1;

							if (tempIco == null)
								tempIco = icos[0].Item1;

							temp = tempIco;
							bmp = tempIcoBmp.Item2;
						}
						else
						{
							var iconint = iconindex.Ai(int.MaxValue);

							if (iconint < icos.Count)
							{
								var tempIcoBmp = icos[iconint];
								temp = tempIcoBmp.Item1;
								bmp = tempIcoBmp.Item2;
							}
						}

						if (bmp == null)
						{
							var tempIcoBmp = icos[0];
							temp = tempIcoBmp.Item1;
							bmp = tempIcoBmp.Item2;
						}

						if (w > 0 || h > 0)
							bmp = ResizeBitmap(bmp, w, h);
						else if (bmp.Size != SystemInformation.IconSize)
							bmp = bmp.Resize(SystemInformation.IconSize.Width, SystemInformation.IconSize.Height);
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
						using (var tempBmp = (Bitmap)Image.FromFile(filename))//Must make a copy because the original will keep the file locked.
						{
							bmp = new Bitmap(tempBmp);
							bmp = ResizeBitmap(bmp, w, h);
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

		private static Bitmap ResizeBitmap(Bitmap bmp, int w, int h)
		{
			if (w <= 0 && h <= 0)
				return bmp;

			if (w <= 0) w = h > 0 && w != 0 ? h : bmp.Width;
			if (h <= 0) h = w > 0 && h != 0 ? w : bmp.Height;

			if (bmp.Width != w || bmp.Height != h)
				bmp = bmp.Resize(w, h);

			return bmp;
		}

#if WINDOWS
		internal static Icon ExtractIconWithSizeFromModule(string path, int index, int w, int h)
		{
			if (w <= 0 && h > 0) w = h;
			if (h <= 0 && w > 0) h = w;
			if (w <= 0 || h <= 0)
			{
				w = SystemInformation.IconSize.Width;
				h = SystemInformation.IconSize.Height;
			}

			var hicons = new nint[1];
			var ids = new uint[1];
			var count = WindowsAPI.PrivateExtractIcons(path, index, w, h, hicons, ids, 1, 0);

			return (count > 0 && hicons[0] != 0) ? Icon.FromHandle(hicons[0]) : null;
		}

		// Image.FromHbitmap doesn't support transparency, so the following code is used as a workaround.
		// Source: https://stackoverflow.com/questions/9275738/convert-hbitmap-to-bitmap-preserving-alpha-channel
		internal static Bitmap GetBitmapFromHBitmap(nint nativeHBitmap)
		{
			Bitmap bmp = Bitmap.FromHbitmap(nativeHBitmap);

			if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
				return bmp;

			BitmapData bmpData;

			if (IsAlphaBitmap(bmp, out bmpData))
				return GetlAlphaBitmapFromBitmapData(bmpData);

			return bmp;
		}

		private static Bitmap GetlAlphaBitmapFromBitmapData(BitmapData bmpData)
		{
			return new Bitmap(
				bmpData.Width,
				bmpData.Height,
				bmpData.Stride,
				PixelFormat.Format32bppArgb,
				bmpData.Scan0);
		}

		private static bool IsAlphaBitmap(Bitmap bmp, out BitmapData bmpData)
		{
			Rectangle bmBounds = new Rectangle(0, 0, bmp.Width, bmp.Height);

			bmpData = bmp.LockBits(bmBounds, ImageLockMode.ReadOnly, bmp.PixelFormat);

			try
			{
				for (int y = 0; y <= bmpData.Height - 1; y++)
				{
					for (int x = 0; x <= bmpData.Width - 1; x++)
					{
						Color pixelColor = Color.FromArgb(
							Marshal.ReadInt32(bmpData.Scan0, (bmpData.Stride * y) + (4 * x)));

						if (pixelColor.A > 0 & pixelColor.A < 255)
						{
							return true;
						}
					}
				}
			}
			finally
			{
				bmp.UnlockBits(bmpData);
			}

			return false;
		}
#endif

		internal static object PrepareIconNumber(object iconnumber)
		{
			if (iconnumber == null)
				return 0;
			else if (iconnumber.ParseLong(false) is long l && l > 0)//Note this allows us to pass the icon number as a string, however that also prevents us from loading an icon from a .NET DLL that happens to be named that same number. This is an extremely unlikely scenario.
				return l - 1;
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
}