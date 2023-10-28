using System.Drawing;

namespace Keysharp.Core.Common
{
	/// <summary>
	/// Create a global Instance of CoordProvider and use it multithreaded
	/// </summary>
	internal sealed class CoordProvider
	{
		private object Locker = new object();
		private Point mCurrent;
		private bool mDone;
		private Size mMaxMovement;

		/// <summary>
		/// Create new CoordProvider with given Settings.
		/// </summary>
		/// <param name="uSourceSize">Size of Searchable Image Area</param>
		/// <param name="uNeedleSize">Size of Needle Image</param>
		public CoordProvider(Size uSourceSize, Size uNeedleSize)
		{
			mMaxMovement = new Size(uSourceSize.Width - uNeedleSize.Width, uSourceSize.Height - uNeedleSize.Height);
			mCurrent = new Point(-1, 0);
			mDone = false;
		}

		/// <summary>
		/// Returns the next Workitem (thread save)
		/// </summary>
		/// <returns>Next Coord (Point) or Null if the work is done.</returns>
		public Point? Next()
		{
			lock (Locker)
			{
				if (mDone)
					return null;

				mCurrent.X++;

				if (mCurrent.X > mMaxMovement.Width)
				{
					mCurrent.X = 0;
					mCurrent.Y++;

					if (mCurrent.Y > mMaxMovement.Height)
					{
						mDone = true;
						return null;
					}
				}

				return mCurrent;
			}
		}
	}
}