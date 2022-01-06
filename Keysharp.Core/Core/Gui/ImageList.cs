using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core.Common;

namespace Keysharp.Core
{
	public static class ImageLists
	{
		private static ConcurrentDictionary<long, ImageList> imageLists;

		static ImageLists()
		{
			if (imageLists == null)
				imageLists = new ConcurrentDictionary<long, ImageList>();
		}

		public static long IL_Add(params object[] obj)
		{
			var (id, filename, icon, resizeNonIcon) = obj.L().Isi2(0, "", 1);
			var il = imageLists.GetOrAdd(id);
			var icontouse = icon;

			if (icon > 0)
				--icontouse;

			if (ImageHelper.LoadImage(filename, 0, 0, icontouse) is Bitmap bmp)
			{
				if (!ImageHelper.IsIcon(filename))
				{
					var color = Color.FromArgb(icon);

					if (resizeNonIcon == 0)
					{
						var splitbmps = ImageHelper.SplitBitmap(bmp, il.ImageSize.Width, il.ImageSize.Height);

						foreach (var newbmp in splitbmps)
							_ = il.Images.Add(newbmp, color);
					}
					else
						bmp = bmp.Resize(il.ImageSize.Width, il.ImageSize.Height);

					_ = il.Images.Add(bmp, color);
				}
				else
					il.Images.Add(bmp);
			}

			return il.Images.Count;
		}

		/// <summary>
		/// Creates a new ImageList.
		/// </summary>
		/// <returns>The unique ID of the ImageList.</returns>
		public static long IL_Create(params object[] obj)
		{
			var largeIcons = obj.L().I1();
			var il = new ImageList
			{
				ImageSize = largeIcons == 0 ? SystemInformation.SmallIconSize : SystemInformation.IconSize
			};//initialCount and growCount are unused. Memory is handled internally.
			var ptr = il.Handle.ToInt64();
			return imageLists.TryAdd(ptr, il) ? ptr : 0;
		}

		/// <summary>
		/// Deletes an ImageList.
		/// </summary>
		/// <param name="id">The ImageList ID.</param>
		/// <returns><c>1</c> if the specified ImageList was deleted, <c>0</c> otherwise.</returns>
		public static long IL_Destroy(params object[] obj)
		{
			var id = obj.L().L1();
			return imageLists.TryRemove(id, out _) ? 1 : 0;
		}

		/// <summary>
		/// Gets an ImageList based on the ID that was returned when it was created.
		/// </summary>
		/// <param name="id">The ID of the ImageList to retrieve</param>
		/// <returns>The ImageList if found, else null.</returns>
		internal static ImageList IL_Get(long id) => imageLists.TryGetValue(id, out var il) ? il : null;

		internal static long IL_GetId(ImageList il)
		{
			if (il != null)
				foreach (var kv in imageLists)
					if (kv.Value == il)
						return kv.Key;

			return 0;
		}
	}
}