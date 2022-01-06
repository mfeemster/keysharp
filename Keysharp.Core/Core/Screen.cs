using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Keysharp.Core.Common;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace Keysharp.Core
{
	public static class Screen
	{
		/// <summary>
		/// Searches a region of the screen for an image.
		/// </summary>
		/// <param name="OutputVarX">
		/// The X and Y coordinates of the upper left corner of the rectangle to search, which can be expressions. Coordinates are relative to the active window unless CoordMode was used to change that.
		/// </param>
		/// <param name="OutputVarY">See <paramref name="OutputVarX"/>.</param>
		/// <param name="X1">The X and Y coordinates of the upper left corner of the rectangle to search, which can be expressions. Coordinates are relative to the active window unless CoordMode was used to change that.</param>
		/// <param name="Y1">See <paramref name="X1"/>.</param>
		/// <param name="X2">The X and Y coordinates of the lower right corner of the rectangle to search, which can be expressions. Coordinates are relative to the active window unless CoordMode was used to change that.</param>
		/// <param name="Y2">See <paramref name="X2"/>.</param>
		/// <param name="OptionsImageFile">
		/// <para>The file name of an image, which is assumed to be in %A_WorkingDir% if an absolute path isn't specified. All operating systems support GIF, JPG, BMP, ICO, CUR, and ANI images (BMP images must be 16-bit or higher). Other sources of icons include the following types of files: EXE, DLL, CPL, SCR, and other types that contain icon resources. On Windows XP or later, additional image formats such as PNG, TIF, Exif, WMF, and EMF are supported. Operating systems older than XP can be given support by copying Microsoft's free GDI+ DLL into the AutoHotkey.exe folder (but in the case of a compiled script, copy the DLL into the script's folder). To download the DLL, search for the following phrase at www.microsoft.com: gdi redistributable</para>
		/// <para>Options: Zero or more of the following strings may be also be present immediately before the name of the file. Separate each option from the next with a single space or tab. For example: *2 *w100 *h-1 C:\Main Logo.bmp</para>
		/// <para>*IconN: To use an icon group other than the first one in the file, specify *Icon followed immediately by the number of the group. For example, *Icon2 would load the default icon from the second icon group.</para>
		/// <para>*n (variation): Specify for n a number between 0 and 255 (inclusive) to indicate the allowed number of shades of variation in either direction for the intensity of the red, green, and blue components of each pixel's color. For example, *2 would allow two shades of variation. This parameter is helpful if the coloring of the image varies slightly or if ImageFile uses a format such as GIF or JPG that does not accurately represent an image on the screen. If you specify 255 shades of variation, all colors will match. The default is 0 shades.</para>
		/// <para>*TransN: This option makes it easier to find a match by specifying one color within the image that will match any color on the screen. It is most commonly used to find PNG, GIF, and TIF files that have some transparent areas (however, icons do not need this option because their transparency is automatically supported). For GIF files, *TransWhite might be most likely to work. For PNG and TIF files, *TransBlack might be best. Otherwise, specify for N some other color name or RGB value (see the color chart for guidance, or use PixelGetColor in its RGB mode). Examples: *TransBlack, *TransFFFFAA, *Trans0xFFFFAA</para>
		/// <para>*wn and *hn: Width and height to which to scale the image (this width and height also determines which icon to load from a multi-icon .ICO file). If both these options are omitted, icons loaded from ICO, DLL, or EXE files are scaled to the system's default small-icon size, which is usually 16 by 16 (you can force the actual/internal size to be used by specifying *w0 *h0). Images that are not icons are loaded at their actual size. To shrink or enlarge the image while preserving its aspect ratio, specify -1 for one of the dimensions and a positive number for the other. For example, specifying *w200 *h-1 would make the image 200 pixels wide and cause its height to be set automatically.</para>
		/// </param>
		public static PixelSearchOutput ImageSearch(params object[] obj)
		{
			var(x1, y1, x2, y2, filename, options) = obj.L().I4S2();
			var optsItems = new Dictionary<string, Regex>
			{
				{ Core.Keyword_Icon, new Regex(@"\*Icon([0-9]*)") },
				{ Core.Keyword_Trans, new Regex(@"\*Trans([0-9a-zA-Z]*)") },
				{ Core.Keyword_Variation, new Regex(@"\*([0-9]*)") },
				{ "w", new Regex(@"\*w([-0-9]*)") },
				{ "h", new Regex(@"\*h([-0-9]*)") }
			};
			var opts = Options.ParseOptionsRegex(ref options, optsItems, false);
			Point start;
			Size bound;
			Bitmap bmp;
			var iconindex = 1;
			int w = 0, h = 0;
			long trans = -1;
			byte variation = 0;

			if (opts.TryGetValue(Core.Keyword_Icon, out var iconopt) && iconopt != "")
				_ = int.TryParse(iconopt, out iconindex);

			if (opts.TryGetValue(Core.Keyword_Variation, out var varopt) && varopt != "")
				_ = byte.TryParse(varopt, out variation);

			if (opts.TryGetValue(Core.Keyword_Trans, out var vartrans) && vartrans != "")//You should use parsecolor() here.//MATT
			{
				if (int.TryParse(vartrans, out var transopt))//You should use parseint here.//MATT
					trans = transopt;
				else if (vartrans.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
						 int.TryParse(vartrans.AsSpan(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var trans0xHex))
					trans = trans0xHex;
				else if (int.TryParse(vartrans, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var transHex))
					trans = transHex;
				else
				{
					var tempcol = Color.FromName(vartrans);
					trans = tempcol.ToArgb();
				}
			}

			if (opts.TryGetValue("w", out var wopt) && wopt != "")
				_ = int.TryParse(wopt, out w);

			if (opts.TryGetValue("h", out var hopt) && hopt != "")
				_ = int.TryParse(hopt, out h);

			Mouse.AdjustRect(ref x1, ref y1, ref x2, ref y2);
			start = new Point(x1, y1);
			bound = new Size(x2 - start.X, y2 - start.Y);

			try
			{
				if (iconindex > 0)
					--iconindex;

				bmp = ImageHelper.LoadImage(filename, w, h, iconindex);

				if (bmp == null)
					throw new FormatException($"Loading icon or bitmap from {filename} failed.");
			}
			catch (FormatException)
			{
				Accessors.A_ErrorLevel = 2;
				return null;
			}

			var source = GuiHelper.GetScreen(new Rectangle(start, bound));
			var searchImg = new ImageFinder(source) { Variation = variation };
			Point? location;

			try
			{
				location = searchImg.Find(bmp, trans);
			}
			catch (Exception)//e)
			{
				//Console.WriteLine(e);
				Accessors.A_ErrorLevel = 2;
				return null;
			}

			if (location.HasValue)
			{
				location = Mouse.RevertPoint(location.Value, Mouse.Coords.Mouse);
				Accessors.A_ErrorLevel = 0;
				return new PixelSearchOutput()
				{
					OutputVarX = (long)location.Value.X,
					OutputVarY = (long)location.Value.Y
				};
			}
			else
			{
				Accessors.A_ErrorLevel = 1;
				return new PixelSearchOutput()
				{
					OutputVarX = "",
					OutputVarY = ""
				};
			}
		}


		/// <summary>
		/// Retrieves the color of the pixel at the specified x,y screen coordinates.
		/// </summary>
		/// <param name="x">The X coordinate of the pixel, which can be expressions. Coordinates are relative to the active window unless CoordMode was used to change that.</param>
		/// <param name="y">The Y coordinate of the pixel, see <paramref name="X"/>.</param>
		/// <returns>The color as a hexadecimal string in red-green-blue (RGB) format. For example, the color purple is defined 0x800080 because it has an intensity of 80 for its blue and red components but an intensity of 00 for its green component.</returns>
		public static string PixelGetColor(params object[] obj)
		{
			var (x, y) = obj.L().I2();
			PixelFormat format;
			int pixel;

			switch (System.Windows.Forms.Screen.PrimaryScreen.BitsPerPixel)
			{
				case 8:
				case 16:
					format = PixelFormat.Format16bppRgb565;
					break;

				case 24:
					format = PixelFormat.Format24bppRgb;
					break;

				case 32:
					format = PixelFormat.Format32bppArgb;
					break;

				default:
					format = PixelFormat.Format32bppArgb;
					break;
			}

			using (var bmp = new Bitmap(1, 1, format))
			{
				Mouse.AdjustPoint(ref x, ref y);

				using (var g = Graphics.FromImage(bmp))
				{
					g.CopyFromScreen(x, y, 0, 0, new Size(1, 1), CopyPixelOperation.SourceCopy);
					pixel = bmp.GetPixel(0, 0).ToArgb() & 0xffffff;
				}
			}

			return $"0x{pixel:X6}";
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static Bitmap GetScreenClip(params object[] obj)
		{
			var (x, y, width, height, filename) = obj.L().I4S1();
			PixelFormat format;

			switch (System.Windows.Forms.Screen.PrimaryScreen.BitsPerPixel)
			{
				case 8:
				case 16:
					format = PixelFormat.Format16bppRgb565;
					break;

				case 24:
					format = PixelFormat.Format24bppRgb;
					break;

				case 32:
					format = PixelFormat.Format32bppArgb;
					break;

				default:
					format = PixelFormat.Format32bppArgb;
					break;
			}

			var bmp = new Bitmap(width, height, format);
			Mouse.AdjustPoint(ref x, ref y);

			using (var g = Graphics.FromImage(bmp))
			{
				g.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
				bmp.Save(filename);
			}

			return bmp;
		}

		/// <summary>
		/// Searches a region of the screen for a pixel of the specified color.
		/// </summary>
		/// <param name="X1">The X and Y coordinates of the upper left corner of the rectangle to search. Coordinates are relative to the active window unless CoordMode was used to change that.</param>
		/// <param name="Y1">See <paramref name="X1"/>.</param>
		/// <param name="X2">The X and Y coordinates of the lower right corner of the rectangle to search. Coordinates are relative to the active window unless CoordMode was used to change that.</param>
		/// <param name="Y2">See <paramref name="X2"/>.</param>
		/// <param name="ColorID">The decimal or hexadecimal color ID to search for, in Red-Green-Blue (RGB) format, which can be an expression. Color IDs can be determined using PixelGetColor. For example: 0x9d6346</param>
		/// <param name="Variation">A number between 0 and 255 (inclusive) to indicate the allowed number of shades of variation in either direction for the intensity of the red, green, and blue components of the color (can be an expression). This parameter is helpful if the color sought is not always exactly the same shade. If you specify 255 shades of variation, all colors will match. The default is 0 shades.</param>
		/// <returns>The coordinates of the first pixel that matches ColorID (if no match is found, a dictionary with empty values is returned). Coordinates are relative to the active window unless CoordMode was used to change that. If an exception is thrown, null is returned.</returns>
		public static PixelSearchOutput PixelSearch(params object[] obj)
		{
			var (x1, y1, x2, y2, ColorID, Variation) = obj.L().I6();
			Variation = Math.Clamp(Variation, byte.MinValue, byte.MaxValue);
			Mouse.AdjustRect(ref x1, ref y1, ref x2, ref y2);
			var ltr = x1 <= x2;
			var ttb = y1 <= y2;
			var x1temp = Math.Min(x1, x2);
			var x2temp = Math.Max(x1, x2);
			var y1temp = Math.Min(y1, y2);
			var y2temp = Math.Max(y1, y2);
			x1 = x1temp;
			x2 = x2temp;
			y1 = y1temp;
			y2 = y2temp;
			var region = new Rectangle(x1, y1, x2 - x1, y2 - y1);
			var finder = new ImageFinder(GuiHelper.GetScreen(region)) { Variation = (byte)Variation };
			var needle = Color.FromArgb((int)((uint)ColorID | 0xFF000000));
			Point? location;

			try
			{
				location = finder.Find(needle, ltr, ttb);
			}
			catch
			{
				Accessors.A_ErrorLevel = 2;
				return null;
			}

			if (location.HasValue)
			{
				location = Mouse.RevertPoint(location.Value, Mouse.Coords.Mouse);
				Accessors.A_ErrorLevel = 0;
				return new PixelSearchOutput()
				{
					OutputVarX =  (long)location.Value.X,
					OutputVarY =  (long)location.Value.Y
				};
			}
			else
			{
				Accessors.A_ErrorLevel = 1;
				return new PixelSearchOutput()
				{
					OutputVarX = "",
					OutputVarY = ""
				};
			}
		}
	}

	public class PixelSearchOutput
	{
		public object OutputVarX { get; set; }
		public object OutputVarY { get; set; }
	}
}