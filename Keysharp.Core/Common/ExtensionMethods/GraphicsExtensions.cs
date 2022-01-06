using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	/// <summary>
	/// An improved color structure that allows the values to be set after construction.
	/// Gotten from https://codereview.stackexchange.com/questions/98605/color-structure-with-single-field-for-multiple-properties
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct FastColor : IEquatable<FastColor>
	{
		[FieldOffset(0)] private uint value;
		[FieldOffset(0)] private byte b;
		[FieldOffset(1)] private byte g;
		[FieldOffset(2)] private byte r;
		[FieldOffset(3)] private byte a;

		public FastColor(uint val) : this(0, 0, 0, 0) => value = val;

		public FastColor(byte r, byte g, byte b) : this(r, g, b, 255)
		{
		}

		public FastColor(byte _r, byte _g, byte _b, byte _a)
		{
			value = 0;
			r = _r;
			g = _g;
			b = _b;
			a = _a;
		}

		public static bool operator ==(FastColor left, FastColor right) => left.Value == right.Value;

		public static bool operator !=(FastColor left, FastColor right) => !(left == right);

		public uint Value
		{
			get => value;
			set => this.value = value;
		}

		public byte R
		{
			get => r;
			set => r = value;
		}

		public byte G
		{
			get => g;
			set => g = value;
		}

		public byte B
		{
			get => b;
			set => b = value;
		}

		public byte A
		{
			get => a;
			set => a = value;
		}

		public bool Equals([AllowNull] FastColor other) => this == other;

		public override bool Equals([AllowNull] object other) => other is FastColor fc&& this == fc;

		public override int GetHashCode() => Value.GetHashCode();
	}

	public static class GraphicsExtensions
	{
		public static bool CompareWithVar(this FastColor col, FastColor match, int variation)
		{
			if (col.A == 0)
				return true;

			if (variation == 0)
				return col == match;

			var r = match.R >= col.R - variation && match.R <= col.R + variation;
			var g = match.G >= col.G - variation && match.G <= col.G + variation;
			var b = match.B >= col.B - variation && match.B <= col.B + variation;
			return r && g && b;
		}

		public static Bitmap Resize(this Bitmap bmp, int width, int height)
		{
			//AHK used these formulas and rounded.
			if (width == -1)
				width = (int)(((double)bmp.Width / bmp.Height * height) + 0.5);
			else if (height == -1)
				height = (int)(((double)bmp.Height / bmp.Width * width) + 0.5);

			var srcRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
			var destRect = new Rectangle(0, 0, width, height);
			var bmp2 = new Bitmap(width, height);

			using (var gr = Graphics.FromImage(bmp2))
			{
				gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
				gr.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
			}

			return bmp2;
		}
	}
}