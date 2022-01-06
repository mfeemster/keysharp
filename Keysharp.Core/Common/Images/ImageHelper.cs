using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Windows;

namespace Keysharp.Core.Common
{
	internal static class ImageHelper
	{
		internal static bool IsIcon(string filename)
		{
			var ext = System.IO.Path.GetExtension(filename).ToLower();
			return ext == ".exe" || ext == ".dll" || ext == ".icl" || ext == ".cpl" || ext == ".scr" || ext == ".ico";
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

		internal static Bitmap LoadImage(string filename, int w, int h, int iconindex)
		{
			Bitmap bmp = null;

			try
			{
				if (filename.StartsWith("HICON:", StringComparison.OrdinalIgnoreCase))
				{
					var hstr = filename.Substring(6);

					if (int.TryParse(hstr, out var handle))
					{
						var tempico = Icon.FromHandle(new IntPtr(handle));
						bmp = tempico.ToBitmap();
						_ = WindowsAPI.DestroyIcon(tempico.Handle);
					}
				}
				else if (filename.StartsWith("HBITMAP:", StringComparison.OrdinalIgnoreCase))
				{
					var hstr = filename.Substring(8);

					if (int.TryParse(hstr, out var handle))
						bmp = System.Drawing.Image.FromHbitmap(new IntPtr(handle));
				}

				if (bmp == null)//Wasn't a handle, and instead was a filename.
				{
					var ext = System.IO.Path.GetExtension(filename).ToLower();

					if (ext == ".exe" || ext == ".dll" || ext == ".icl" || ext == ".cpl" || ext == ".scr")
					{
						var ico = GuiHelper.GetIcon(filename, iconindex);
						bmp = ico?.ToBitmap();

						if (w > 0 || h > 0)
						{
							if (bmp.Width != w || bmp.Height != h)
								bmp = bmp.Resize(w, h);
						}
						else if (bmp.Size != SystemInformation.IconSize)
						{
							bmp = bmp.Resize(SystemInformation.IconSize.Width, SystemInformation.IconSize.Height);
						}
					}
					else if (ext == ".ico")
					{
						var ico = new Icon(filename);
						var icos = GuiHelper.SplitIcon(ico);

						if (w > 0 && h > 0)
						{
							var tempico = icos.FirstOrDefault(tempico => tempico.Width == w && tempico.Height == h);
							bmp = tempico?.ToBitmap();
						}
						else
						{
							if (iconindex < icos.Count)
								bmp = icos[iconindex].ToBitmap();
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
						}
					}
					else
					{
						bmp = (Bitmap)System.Drawing.Image.FromFile(filename);

						if (w > 0 || h > 0)
						{
							if (bmp.Width != w || bmp.Height != h)
								bmp = bmp.Resize(w, h);
						}
					}
				}
			}
			catch (FormatException)
			{
				Accessors.A_ErrorLevel = 2;
				return null;
			}

			return bmp;
		}
	}
}
