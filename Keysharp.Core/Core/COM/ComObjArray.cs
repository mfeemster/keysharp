namespace Keysharp.Core.COM
{
	/// <summary>
	/// Describes the bounds (element count and lower bound) of a single dimension of a SAFEARRAY.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct SAFEARRAYBOUND
	{
		/// <summary>
		/// Number of elements in this dimension.
		/// </summary>
		public uint cElements;
		/// <summary>
		/// Lower bound (starting index) of this dimension.
		/// </summary>
		public int lLbound;
	}

	/// <summary>
	/// Contains P/Invoke declarations for OLE Automation SafeArray APIs.
	/// </summary>
	static class OleAuto
	{
		/// <summary>
		/// Creates a new SafeArray of the specified variant type and dimensions.
		/// </summary>
		/// <param name="vt">The VARTYPE of each element (e.g. VT_VARIANT).</param>
		/// <param name="cDims">The number of dimensions (1-8).</param>
		/// <param name="rgsabound">Array of bounds for each dimension.</param>
		/// <returns>A pointer to the new SAFEARRAY; IntPtr.Zero on failure.</returns>
		[DllImport(WindowsAPI.oleaut)]
		public static extern nint SafeArrayCreate(
			short vt,
			uint cDims,
			[In] SAFEARRAYBOUND[] rgsabound);
		/// <summary>
		/// Retrieves the number of dimensions in a SafeArray.
		/// </summary>
		/// <param name="psa">Pointer to the SAFEARRAY.</param>
		/// <returns>The number of dimensions, or a negative error code.</returns>
		[DllImport(WindowsAPI.oleaut)]
		public static extern int SafeArrayGetDim(
			nint psa);
		/// <summary>
		/// Retrieves the upper bound (max index) for a specified dimension.
		/// </summary>
		[DllImport(WindowsAPI.oleaut)]
		public static extern int SafeArrayGetUBound(
			nint psa,
			uint nDim,
			out int plUbound);
		/// <summary>
		/// Retrieves the lower bound for a specified dimension.
		/// </summary>
		[DllImport(WindowsAPI.oleaut)]
		public static extern int SafeArrayGetLBound(
			nint psa,
			uint nDim,
			out int plLbound);
		/// <summary>
		/// Creates a copy of a SafeArray.
		/// </summary>
		[DllImport(WindowsAPI.oleaut)]
		public static extern int SafeArrayCopy(
			nint psa,
			out nint ppsaOut);
		/// <summary>
		/// Destroys a SafeArray, releasing its memory.
		/// </summary>
		[DllImport(WindowsAPI.oleaut)]
		public static extern int SafeArrayDestroy(
			nint psa);
		/// <summary>
		/// Retrieves an element from a SafeArray by index.
		/// </summary>
		[DllImport(WindowsAPI.oleaut)]
		public static extern int SafeArrayGetElement(
			nint psa,
			[In] int[] rgIndices,
			out object pv);
		/// <summary>
		/// Stores an element into a SafeArray by index.
		/// </summary>
		[DllImport(WindowsAPI.oleaut, PreserveSig = true)]
		public static extern int SafeArrayPutElement(
			nint psa,
			[In] int[] rgIndices,
			[MarshalAs(UnmanagedType.Struct)] object pv
		);
	}

	/// <summary>
	/// Enumerator for iterating (index, value) pairs in a COM SafeArray.
	/// </summary>
	public class ComArrayIndexValueEnumerator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		private readonly ComObjArray _owner;
		private readonly int _count;
		private readonly int[] _indices;
		private readonly int[] _flows; // upper bounds per dimension
		private bool _done;

		/// <summary>
		/// Initializes a new enumerator over the specified ComObjArray.
		/// </summary>
		/// <param name="owner">The array wrapper to enumerate.</param>
		/// <param name="c">Number of returned items per iteration (1 for value, 2 for index+value).</param>
		public ComArrayIndexValueEnumerator(ComObjArray owner, int c)
			: base(null, c)
		{
			_owner = owner;
			_count = c;
			int d = owner._dimensions;
			_indices = new int[d];
			_flows = new int[d];

			for (int i = 0; i < d; i++)
			{
				// start one _below_ zero so first MoveNext() bumps to 0
				_indices[i] = -1;
				_flows[i] = (int)owner.MaxIndex(i + 1);
			}

			var script = Script.TheScript;
			var p = c <= 1 ? script.ComArrayIndexValueEnumeratorData.p1 : script.ComArrayIndexValueEnumeratorData.p2;
			fo = (FuncObj)p.Clone();
			fo.Inst = this;
		}

		public override object Call([ByRef] object pos)
		{
			if (MoveNext())
			{
				Script.SetPropertyValue(pos, "__Value", Current.Item1);
				return true;
			}

			return false;
		}

		public override object Call([ByRef] object pos, [ByRef] object val)
		{
			if (MoveNext())
			{
				Script.SetPropertyValue(pos, "__Value", Current.Item1);
				Script.SetPropertyValue(val, "__Value", Current.Item2);
				return true;
			}

			return false;
		}

		public (object, object) Current
		{
			get
			{
				// 1-based AHK index is (idx+1) for the first dimension
				object idx1 = (long)(_indices[0] + 1);
				_ = OleAuto.SafeArrayGetElement(
						_owner._psa,
						_indices,
						out object val
					);
				return (idx1, val);
			}
		}

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			if (_done) return false;

			int d = _owner._dimensions;

			// increment the n-dimensional counter:
			for (int dim = d - 1; dim >= 0; dim--)
			{
				_indices[dim]++;

				if (_indices[dim] <= _flows[dim])
				{
					// done incrementing
					break;
				}

				if (dim == 0)
				{
					// we overflowed the first dimension ⇒ end
					_done = true;
					return false;
				}

				// reset this dim and carry to next
				_indices[dim] = 0;
			}

			return true;
		}

		public void Reset()
		{
			System.Array.Clear(_indices, 0, _indices.Length);
			_done = false;
		}

		public void Dispose() => Reset();
	}

	internal class ComArrayIndexValueEnumeratorData : BaseIteratorData<ComArrayIndexValueEnumerator>
	{
	}

	/// <summary>
	/// A COM wrapper around a native SAFEARRAY, exposing AHK-friendly APIs.
	/// </summary>
	public class ComObjArray : ComObject, I__Enum, IEnumerable<(object, object)>
	{
		internal nint _psa;         // pointer to the native SAFEARRAY
		internal int _dimensions;   // number of dimensions
		internal VarEnum _baseType; // element VARTYPE, e.g. VT_VARIANT

		public long Dimensions => _dimensions;
		public long Vt => (long)_baseType;

		/// <summary>
		/// Creates a new SafeArray of the specified element type and dimension sizes.
		/// </summary>
		/// <param name="baseType">The element VARTYPE (e.g. VT_VARIANT).</param>
		/// <param name="counts">Sizes for each dimension (1 to 8 values).</param>
		public ComObjArray(VarEnum baseType, params int[] counts)
		{
			if (counts == null || counts.Length == 0 || counts.Length > 8)
				throw new ArgumentException("Must supply 1–8 dimension sizes", nameof(counts));

			_baseType = baseType;
			_dimensions = counts.Length;
			// Build SAFEARRAYBOUND array, all zero‐based:
			var sab = new SAFEARRAYBOUND[_dimensions];

			for (int i = 0; i < _dimensions; i++)
			{
				sab[i].cElements = (uint)counts[i];
				sab[i].lLbound = 0;
			}

			// Create the native SafeArray:
			_psa = OleAuto.SafeArrayCreate(
					   (short)baseType,
					   (uint)_dimensions,
					   sab
				   );

			if (_psa == 0)
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

			// Tell ComObject to own and destroy the SafeArray:
			this.vt = VarEnum.VT_ARRAY | baseType;
			this.Flags = F_OWNVALUE;
			this.Ptr = _psa.ToInt64();
		}

		public IFuncObj __Enum(object count) => new ComArrayIndexValueEnumerator(this, count.Ai()).fo;

		public IEnumerator<(object, object)> GetEnumerator() => new ComArrayIndexValueEnumerator(this, 2);

		IEnumerator IEnumerable.GetEnumerator() => new ComArrayIndexValueEnumerator(this, 2);

		/// <summary>
		/// Gets the upper bound (inclusive) of the specified dimension.
		/// </summary>
		public object MaxIndex(object dim = null)
		{
			int d = dim.Ai(1);

			if (d < 1 || d > _dimensions)
				return Errors.ValueErrorOccurred($"Argument out of range.");

			_ = OleAuto.SafeArrayGetUBound(_psa, (uint)d, out int ub);
			return (long)ub;
		}

		/// <summary>
		/// Gets the lower bound (inclusive) of the specified dimension (1-based).
		/// </summary>
		public object MinIndex(object dim = null)
		{
			int d = dim.Ai(1);

			if (d < 1 || d > _dimensions)
				return Errors.ValueErrorOccurred($"Argument out of range.");

			_ = OleAuto.SafeArrayGetLBound(_psa, (uint)d, out int lb);
			return (long)lb;
		}

		/// <summary>
		/// Indexer for direct element access using 1-based indices.
		/// Negative indices count from the end.
		/// </summary>
		public object this[params object[] indices]
		{
			get
			{
				int[] idx = ConvertIndices(indices);
				int hr = OleAuto.SafeArrayGetElement(_psa, idx, out object val);
				Marshal.ThrowExceptionForHR(hr);
				return val;
			}
			set
			{
				int[] idx = ConvertIndices(indices);
				int hr = OleAuto.SafeArrayPutElement(_psa, idx, value!);
				Marshal.ThrowExceptionForHR(hr);
			}
		}

		/// <summary>
		/// Converts indices to ints and handles negative indices
		/// </summary>
		private int[] ConvertIndices(object[] indices)
		{
			int[] idx = new int[indices.Length];

			for (int i = 0; i < idx.Length; i++)
			{
				int temp = indices[i].Ai();

				if (temp < 0)
					temp = Convert.ToInt32(MaxIndex((long)(i + 1))) + temp + 1;

				idx[i] = temp;
			}

			return idx;
		}

		/// <summary>
		/// Returns a new wrapper around a copy of this SafeArray.
		/// </summary>
		public new object Clone()
		{
			int hr = OleAuto.SafeArrayCopy(_psa, out nint psaCopy);
			Marshal.ThrowExceptionForHR(hr);
			var copy = (ComObjArray)RuntimeHelpers
					   .GetUninitializedObject(typeof(ComObjArray));
			copy.vt = this.vt;
			copy.Flags = this.Flags;
			copy._dimensions = this._dimensions;
			copy._baseType = this._baseType;
			copy._psa = psaCopy;
			copy.Ptr = psaCopy.ToInt64();
			return copy;
		}

		public override void Dispose()
		{
			if ((Flags & F_OWNVALUE) != 0 && _psa != 0)
			{
				_ = OleAuto.SafeArrayDestroy(_psa);
				// Clear the flag so we don't double‐destroy:
				Flags &= ~F_OWNVALUE;
			}

			base.Dispose();
		}
	}
}
