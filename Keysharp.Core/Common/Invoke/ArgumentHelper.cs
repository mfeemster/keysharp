#if WINDOWS
namespace Keysharp.Core.Common.Invoke
{
	internal class ArgumentHelper
	{
		protected bool cdecl = false;
		protected List<GCHandle> gcHandles = [];
		protected ScopeHelper gcHandlesScope;
		protected bool hasreturn = false;
		protected Type returnType = typeof(int);
		internal Dictionary<int, Type> outputVars = [];
		internal bool CDecl => cdecl;
		internal bool HasReturn => hasreturn;
		internal Type ReturnType => returnType;
		internal long[] args;
		// contains bitwise info about the location of float and double type arguments, as well as the return type
		// bit i = 1 if argTypes[i] is float or double  
		// bit n = 1 if returnType is float or double
		internal ulong floatingTypeMask = 0;

		// Storage for pinned BSTR pointers, to be released at disposal
		private readonly List<IntPtr> _bstrs = new List<IntPtr>();
		private bool _isDisposed;

		internal ArgumentHelper(object[] parameters)
		{
			ConvertParameters(parameters);
		}

		~ArgumentHelper() => Dispose();

		public void Dispose()
		{
			if (!_isDisposed)
			{
				// free BSTRs
				for (int i = 0; i < _bstrs.Count; i++)
					Marshal.FreeBSTR(_bstrs[i]);

				// free GCHandles
				for (int i = 0; i < gcHandles.Count; i++)
					gcHandles[i].Free();

				_isDisposed = true;
			}
		}

		protected unsafe void ConvertParameters(object[] parameters)
		{
			Error err;
			Type type = null;
			int paramCount = parameters.Length;
			bool hasReturn = (paramCount & 1) != 0;
			int lastIdx = paramCount - 1;
			int argCount = paramCount / 2;
			args = new long[argCount];
			object p = null;
			int n = -1;
			void SetupPointerArg()
			{
				var gch = GCHandle.Alloc(p, GCHandleType.Pinned);
				gcHandles.Add(gch);
				args[n] = gch.AddrOfPinnedObject();
			}

			for (int paramIndex = 0; paramIndex < paramCount; paramIndex++)
			{
				bool isReturn = hasReturn && paramIndex == lastIdx;
				bool parseType = isReturn;
				// Read the tag and value
				string tag = parameters[paramIndex++] as string ?? string.Empty;
				// Trim whitespace around tag
				ReadOnlySpan<char> span = tag.AsSpan().Trim();
				int len = span.Length;

				if (len == 0)
					goto InvalidType;

				// Lowercase-first-char for fast case-insensitive dispatch
				char c0 = (char)(span[0] | 0x20);

				if (isReturn)
				{
					if (c0 == 'c' && len >= 5
							&& ((span[1] | 0x20) == 'd')
							&& ((span[2] | 0x20) == 'e')
							&& ((span[3] | 0x20) == 'c')
							&& ((span[4] | 0x20) == 'l'))
					{
						span = span.Slice(5).TrimStart();
						cdecl = true;
						len = span.Length;

						if (len == 0)
						{
							hasreturn = false;
							break;
						}

						c0 = (char)(span[0] | 0x20);
					}
				}
				else
				{
					n++;
					p = parameters[paramIndex];

					if (p is KeysharpObject kso)
					{
						object kptr;

						if ((kso is IPointable ip && (kptr = ip.Ptr) != null)
								|| (Script.GetPropertyValue(kso, "ptr", false) is object tmp && (kptr = tmp) != null))
						{
							// Need to track this separately, because we later need to update ComObject.Ptr in FixParamTypesAndCopyBack
							if (kso is ComObject)
								outputVars[paramIndex] = typeof(nint);

							p = kptr;
						}
					}
				}

				// Check for pointer suffix: '*' or 'P'/'p'
				char last = span[len - 1];

				if (last == '*' || (char)(last | 0x20) == 'p')
				{
					// Remove the suffix
					span = span.Slice(0, --len);
					// Pin the object and store its address
					SetupPointerArg();
					// Determine the type only
					parseType = true;
				}

				// BSTR
				if (c0 == 'b' && len == 4
						&& ((span[1] | 0x20) == 's')
						&& ((span[2] | 0x20) == 't')
						&& ((span[3] | 0x20) == 'r'))
				{
					if (parseType)
					{
						type = typeof(string);
						goto TypeDetermined;
					}

					if (p is string s)
					{
						IntPtr bstr = Marshal.StringToBSTR(s);
						_bstrs.Add(bstr);
						args[n] = bstr;
					}
					else
					{
						_ = Errors.ErrorOccurred(err = new TypeError($"Argument had type {tag} but was not a string.")) ? throw err : "";
						return;
					}

					continue;
				}
				// WSTR or STR
				else if ((c0 == 'w' && len == 4 && ((span[1] | 0x20) == 's') && ((span[2] | 0x20) == 't') && ((span[3] | 0x20) == 'r'))
						 || (c0 == 's' && len == 3 && ((span[1] | 0x20) == 't') && ((span[2] | 0x20) == 'r')))
				{
					if (parseType)
					{
						type = isReturn ? typeof(string) : typeof(nint);
						goto TypeDetermined;
					}

					if (p is string s)
					{
						SetupPointerArg();
					}
					else
					{
						ConvertPtr();
					}

					continue;
				}

				// ASTR
				if (c0 == 'a' && len == 4
						&& ((span[1] | 0x20) == 's')
						&& ((span[2] | 0x20) == 't')
						&& ((span[3] | 0x20) == 'r'))
				{
					if (parseType)
					{
						type = isReturn ? typeof(string) : typeof(nint);
						goto TypeDetermined;
					}

					if (p is string s)
					{
						byte[] ascii = Encoding.ASCII.GetBytes(s);
						var gch = GCHandle.Alloc(ascii, GCHandleType.Pinned);
						gcHandles.Add(gch);
						args[n] = gch.AddrOfPinnedObject();
					}
					else
					{
						_ = Errors.ErrorOccurred(err = new TypeError($"Argument had type {tag} but was not a string.")) ? throw err : "";
						return;
					}

					continue;
				}

				// Numeric and pointer types
				switch (c0)
				{
					case 'p':
						if (len == 3
								&& ((span[1] | 0x20) == 't')
								&& ((span[2] | 0x20) == 'r'))
						{
							if (parseType)
							{
								type = typeof(nint);
								goto TypeDetermined;
							}

							ConvertPtr();
							continue;
						}

						break;

					case 'i': // INT or INT64
						if (len == 5 && span[3] == '6' && span[4] == '4') // "int64"
						{
							if (parseType)
							{
								type = typeof(long);
								goto TypeDetermined;
							}

							args[n] = p is IntPtr ip ? ip : p.Al();
							continue;
						}
						else if (len == 3) // "int"
						{
							if (parseType)
							{
								type = typeof(int);
								goto TypeDetermined;
							}

							args[n] = p is IntPtr ip2 ? ip2 : p.Ai();
							continue;
						}

						break;

					case 'h': // HRESULT
						if (len == 7) // "hresult"
						{
							if (parseType)
							{
								if (isReturn)
									hasreturn = false; // needed for ComCall OSError
								type = typeof(int);
								goto TypeDetermined;
							}

							args[n] = p is IntPtr ip3 ? ip3 : p.Ai();
							continue;
						}

						break;

					case 'u': // UINT, USHORT, UCHAR, UPTR
						char c1u = (char)(span[1] | 0x20);

						if (c1u == 'i')
						{
							if (len == 6) // "uint64"
							{
								if (parseType)
								{
									type = typeof(ulong);
									goto TypeDetermined;
								}

								args[n] = p is IntPtr ip4 ? ip4 : p.Al();
								continue;
							}
							else if (len == 4) // "uint"
							{
								if (parseType)
								{
									type = typeof(uint);
									goto TypeDetermined;
								}

								args[n] = p is IntPtr ip5 ? ip5 : p.Aui();
								continue;
							}
						}
						else if (c1u == 's' && len == 6) // "ushort"
						{
							if (parseType)
							{
								type = typeof(ushort);
								goto TypeDetermined;
							}

							args[n] = p is IntPtr ip6 ? ip6 : (ushort)p.Al();
							continue;
						}
						else if (c1u == 'c' && len == 5) // "uchar"
						{
							if (parseType)
							{
								type = typeof(byte);
								goto TypeDetermined;
							}

							args[n] = p is IntPtr ip7 ? ip7 : (byte)p.Al();
							continue;
						}
						else if (c1u == 'p' && len == 4) // "uptr"
						{
							if (parseType)
							{
								type = typeof(nint);
								goto TypeDetermined;
							}

							ConvertPtr();
							continue;
						}

						break;

					case 's': // SHORT
						if (len == 5) // "short"
						{
							if (parseType)
							{
								type = typeof(short);
								goto TypeDetermined;
							}

							args[n] = p is IntPtr ip9 ? ip9 : (short)p.Al();
							continue;
						}

						break;

					case 'c': // CHAR
						if (len == 4) // "char"
						{
							if (parseType)
							{
								type = typeof(sbyte);
								goto TypeDetermined;
							}

							args[n] = p is IntPtr ipA ? ipA : (sbyte)p.Al();
							continue;
						}

						break;

					case 'f': // FLOAT
						if (len == 5) // "float"
						{
							if (parseType)
							{
								type = typeof(float);
								goto TypeDetermined;
							}
							floatingTypeMask |= 1UL << n;
							float f = p.Af();
							args[n] = *(int*)&f;
							continue;
						}

						break;

					case 'd': // DOUBLE
						if (len == 6) // "double"
						{
							if (parseType)
							{
								type = typeof(double);
								goto TypeDetermined;
							}
							floatingTypeMask |= 1UL << n;
							double d = p.Ad();
							args[n] = *(long*)&d;
							continue;
						}

						break;
				}

				InvalidType:
				// Invalid type tag
				var ex = new ValueError($"Arg or return type of {tag} is invalid.");

				if (Errors.ErrorOccurred(ex))
					throw ex;

				TypeDetermined:

				if (isReturn)
				{
					returnType = type;
					if (type == typeof(float) || type == typeof(double))
						floatingTypeMask |= 1UL << n;
				}
				else
					outputVars[paramIndex] = type;
			}

			void ConvertPtr()
			{
				if (p is IntPtr ipPtr)
					args[n] = ipPtr;
				else if (p is long lptr)
					args[n] = lptr;
				else if (p is Array arrPtr)
				{
					SetupPointerArg();
				}
				else if (Marshal.IsComObject(p))
				{
					var pUnk = Marshal.GetIUnknownForObject(p);
					args[n] = pUnk;
					Marshal.Release(pUnk);
				}
				else
				{
					SetupPointerArg();
				}
			}
		}
	}
}
#endif