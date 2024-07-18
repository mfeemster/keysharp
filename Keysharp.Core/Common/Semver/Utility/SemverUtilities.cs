namespace Semver.Utility
{
	/// <summary>
	/// Combine hash codes in a good way since <c>System.HashCode</c> isn't available.
	/// </summary>
	/// <remarks>Algorithm based on HashHelpers previously used in the core CLR.
	/// https://github.com/dotnet/coreclr/blob/456afea9fbe721e57986a21eb3b4bb1c9c7e4c56/src/System.Private.CoreLib/shared/System/Numerics/Hashing/HashHelpers.cs
	/// </remarks>
	internal struct CombinedHashCode
	{
		private static readonly int RandomSeed = new Random().Next(int.MinValue, int.MaxValue);

		private int hash;

		private CombinedHashCode(int hash)
		{
			this.hash = hash;
		}

		public static CombinedHashCode Create<T1>(T1 value1)
		=> new CombinedHashCode(CombineValue(RandomSeed, value1));

		public static CombinedHashCode Create<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
		{
			var hash = RandomSeed;
			hash = CombineValue(hash, value1);
			hash = CombineValue(hash, value2);
			hash = CombineValue(hash, value3);
			return new CombinedHashCode(hash);
		}

		public static CombinedHashCode Create<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
		{
			var hash = RandomSeed;
			hash = CombineValue(hash, value1);
			hash = CombineValue(hash, value2);
			hash = CombineValue(hash, value3);
			hash = CombineValue(hash, value4);
			hash = CombineValue(hash, value5);
			return new CombinedHashCode(hash);
		}

		public static implicit operator int(CombinedHashCode hashCode)
		{
			return hashCode.hash;
		}

		public void Add<T>(T value) => hash = CombineValue(hash, value);

		[Obsolete("HashCode is a mutable struct and should not be compared with other HashCodes. Use ToHashCode to retrieve the computed hash code.", true)]
		public override int GetHashCode() => throw new NotSupportedException();

#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CombineValue<T>(int hash1, T value)
		{
			uint rotateLeft5 = ((uint)hash1 << 5) | ((uint)hash1 >> 27);
			return ((int)rotateLeft5 + hash1) ^ (value?.GetHashCode() ?? 0);
		}
	}

	/// <summary>
	/// Struct used as a marker to differentiate constructor overloads that would
	/// otherwise be the same as safe overloads.
	/// </summary>
	internal readonly struct UnsafeOverload
	{
		public static readonly UnsafeOverload Marker = default;
	}

	internal static class CharExtensions
	{
		/// <summary>
		/// Is this character and ASCII alphabetic character or hyphen [A-Za-z-]
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlphaOrHyphen(this char c)
		=> (c >= 'A'&& c <= 'Z') || (c >= 'a'&& c <= 'z') || c == '-';
	}

	internal static class EnumerableExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> values)
		=> values.ToList().AsReadOnly();
	}

	/// <summary>
	/// Methods for working with the strings that make up identifiers
	/// </summary>
	internal static class IdentifierString
	{
		/// <summary>
		/// Compare two strings as they should be compared as identifiers.
		/// </summary>
		/// <remarks>This enforces ordinal comparision. It also fixes a technically
		/// correct but odd thing where the comparision result can be a number
		/// other than -1, 0, or 1.</remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Compare(string left, string right)
		=> Math.Sign(string.CompareOrdinal(left, right));
	}

	internal static class IntExtensions
	{
		/// <summary>
		/// The number of digits in a non-negative number. Returns 1 for all
		/// negative numbers. That is ok because we are using it to calculate
		/// string length for a <see cref="StringBuilder"/> for numbers that
		/// aren't supposed to be negative, but when they are it is just a little
		/// slower.
		/// </summary>
		/// <remarks>
		/// This approach is based on https://stackoverflow.com/a/51099524/268898
		/// where the poster offers performance benchmarks showing this is the
		/// fastest way to get a number of digits.
		/// </remarks>
		public static int DecimalDigits(this int n)
		{
			if (n < 10) return 1;

			if (n < 100) return 2;

			if (n < 1_000) return 3;

			if (n < 10_000) return 4;

			if (n < 100_000) return 5;

			if (n < 1_000_000) return 6;

			if (n < 10_000_000) return 7;

			if (n < 100_000_000) return 8;

			if (n < 1_000_000_000) return 9;

			return 10;
		}
	}

	/// <summary>
	/// Internal helper for efficiently creating empty read only lists
	/// </summary>
	internal static class ReadOnlyList<T>
	{
		public static readonly IReadOnlyList<T> Empty = new List<T>().AsReadOnly();
	}

	internal static class StringExtensions
	{
		/// <summary>
		/// Is this string composed entirely of ASCII alphanumeric characters and hyphens?
		/// </summary>
		public static bool IsAlphanumericOrHyphens(this string value)
		{
			foreach (var c in value)
				if (!c.IsAlphaOrHyphen() && !char.IsDigit(c))
					return false;

			return true;
		}

		/// <summary>
		/// Is this string composed entirely of ASCII digits '0' to '9'?
		/// </summary>
		public static bool IsDigits(this string value)
		{
			foreach (var c in value)
				if (!char.IsDigit(c))
					return false;

			return true;
		}

		/// <summary>
		/// Split a string, map the parts, and return a read only list of the values.
		/// </summary>
		/// <remarks>Splitting a string, mapping the result and storing into a <see cref="IReadOnlyList{T}"/>
		/// is a common operation in this package. This method optimizes that. It avoids the
		/// performance overhead of:
		/// <list type="bullet">
		///   <item><description>Constructing the params array for <see cref="string.Split(char[])"/></description></item>
		///   <item><description>Constructing the intermediate <see cref="T:string[]"/> returned by <see cref="string.Split(char[])"/></description></item>
		///   <item><description><see cref="System.Linq.Enumerable.Select{TSource,TResult}(IEnumerable{TSource},Func{TSource,TResult})"/></description></item>
		///   <item><description>Not allocating list capacity based on the size</description></item>
		/// </list>
		/// Benchmarking shows this to be 30%+ faster and that may not reflect the whole benefit
		/// since it doesn't fully account for reduced allocations.
		/// </remarks>
		public static IReadOnlyList<T> SplitAndMapToReadOnlyList<T>(
			this string value,
			char splitOn,
			Func<string, T> func)
		{
			if (value.Length == 0) return ReadOnlyList<T>.Empty;

			// Figure out how many items the resulting list will have
			int count = 1; // Always one more item than there are separators

			for (int i = 0; i < value.Length; i++)
				if (value[i] == splitOn)
					count++;

			// Allocate enough capacity for the items
			var items = new List<T>(count);
			int start = 0;

			for (int i = 0; i < value.Length; i++)
				if (value[i] == splitOn)
				{
					items.Add(func(value.Substring(start, i - start)));
					start = i + 1;
				}

			// Add the final items from the last separator to the end of the string
			items.Add(func(value.Substring(start, value.Length - start)));
			return items.AsReadOnly();
		}

		/// <summary>
		/// Trim leading zeros from a numeric string. If the string consists of all zeros, return
		/// <c>"0"</c>.
		/// </summary>
		/// <remarks>The standard <see cref="string.TrimStart"/> method handles all zeros
		/// by returning <c>""</c>. This efficiently handles the kind of trimming needed.</remarks>
		public static string TrimLeadingZeros(this string value)
		{
			int start;
			var searchUpTo = value.Length - 1;

			for (start = 0; start < searchUpTo; start++)
				if (value[start] != '0')
					break;

			return value.Substring(start);
		}
	}
}