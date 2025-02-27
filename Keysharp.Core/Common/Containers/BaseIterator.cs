namespace Keysharp.Core.Common.Containers
{
    public class KeysharpEnumerator
	{
		private IFuncObj fo;
		private object[] args;

		/// <summary>
		/// The number of items to return for each iteration. Allowed values are 1 and 2:
		/// 1: return just the value in the first position
		/// 2: return the index in the first position and the value in the second.
		/// </summary>
		public int Count { get; private set; }

		public IFuncObj CallFunc { get; protected set; }

		public KeysharpEnumerator(IFuncObj f, int count)
		{
			fo = f;
			Count = count;//Unsure what happens when this differs from the number of parameters fo expects. MethodPropertyHolder probably just fills them in.
			args = new object[count];
		}

		/*
		    Call() method overloads were generated with this code:
		    var sb = new StringBuilder(10000);

		    for (var i = 1; i <= 19; ++i)
		    {
		        var paramsSb = new StringBuilder(256);
		        paramsSb.Append($"ref object ovar1");

		        for (var j = 2; j <= i; ++j)
		        {
		            paramsSb.Append($",ref object ovar{j}");
		        }

		        paramsSb.Append(")");
		        var assignToArgsSb = new StringBuilder(256);

		        for (var j = 0; j < i; ++j)
		        {
		            assignToArgsSb.AppendLine($"\targs[{j}] = ovar{j + 1};");
		        }

		        var assignToParamsSb = new StringBuilder(256);

		        for (var j = 0; j < i; ++j)
		        {
		            assignToParamsSb.AppendLine($"\tovar{j + 1} = args[{j}];");
		        }

		        sb.Append("public virtual object Call(");
		        sb.AppendLine(paramsSb.ToString());
		        sb.AppendLine("{");
		        sb.AppendLine("\ttry");
		        sb.AppendLine("\t{");
		        sb.Append(assignToArgsSb.ToString());
		        sb.AppendLine("\tvar ret = fo.CallWithRefs(args);");
		        sb.Append(assignToParamsSb.ToString());
		        sb.AppendLine("\treturn ret;");
		        sb.AppendLine("\t}");
		        sb.AppendLine("\tcatch (Exception e)");
		        sb.AppendLine("\t{");
		        sb.AppendLine("\t\tthrow new Error(e.Message);");
		        sb.AppendLine("\t}");
		        sb.AppendLine("}\n");
		    }

		    File.WriteAllText("./enumcalls.txt", sb.ToString());
		*/
		public virtual object Call(params object[] args) => fo.Call(args);

		public virtual object Call(ref object ovar1)
		{
			try
			{
				args[0] = ovar1;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12, ref object ovar13)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				args[12] = ovar13;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				ovar13 = args[12];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12, ref object ovar13, ref object ovar14)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				args[12] = ovar13;
				args[13] = ovar14;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				ovar13 = args[12];
				ovar14 = args[13];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12, ref object ovar13, ref object ovar14, ref object ovar15)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				args[12] = ovar13;
				args[13] = ovar14;
				args[14] = ovar15;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				ovar13 = args[12];
				ovar14 = args[13];
				ovar15 = args[14];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12, ref object ovar13, ref object ovar14, ref object ovar15, ref object ovar16)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				args[12] = ovar13;
				args[13] = ovar14;
				args[14] = ovar15;
				args[15] = ovar16;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				ovar13 = args[12];
				ovar14 = args[13];
				ovar15 = args[14];
				ovar16 = args[15];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12, ref object ovar13, ref object ovar14, ref object ovar15, ref object ovar16, ref object ovar17)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				args[12] = ovar13;
				args[13] = ovar14;
				args[14] = ovar15;
				args[15] = ovar16;
				args[16] = ovar17;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				ovar13 = args[12];
				ovar14 = args[13];
				ovar15 = args[14];
				ovar16 = args[15];
				ovar17 = args[16];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12, ref object ovar13, ref object ovar14, ref object ovar15, ref object ovar16, ref object ovar17, ref object ovar18)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				args[12] = ovar13;
				args[13] = ovar14;
				args[14] = ovar15;
				args[15] = ovar16;
				args[16] = ovar17;
				args[17] = ovar18;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				ovar13 = args[12];
				ovar14 = args[13];
				ovar15 = args[14];
				ovar16 = args[15];
				ovar17 = args[16];
				ovar18 = args[17];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call(ref object ovar1, ref object ovar2, ref object ovar3, ref object ovar4, ref object ovar5, ref object ovar6, ref object ovar7, ref object ovar8, ref object ovar9, ref object ovar10, ref object ovar11, ref object ovar12, ref object ovar13, ref object ovar14, ref object ovar15, ref object ovar16, ref object ovar17, ref object ovar18, ref object ovar19)
		{
			try
			{
				args[0] = ovar1;
				args[1] = ovar2;
				args[2] = ovar3;
				args[3] = ovar4;
				args[4] = ovar5;
				args[5] = ovar6;
				args[6] = ovar7;
				args[7] = ovar8;
				args[8] = ovar9;
				args[9] = ovar10;
				args[10] = ovar11;
				args[11] = ovar12;
				args[12] = ovar13;
				args[13] = ovar14;
				args[14] = ovar15;
				args[15] = ovar16;
				args[16] = ovar17;
				args[17] = ovar18;
				args[18] = ovar19;
				var ret = fo.CallWithRefs(args);
				ovar1 = args[0];
				ovar2 = args[1];
				ovar3 = args[2];
				ovar4 = args[3];
				ovar5 = args[4];
				ovar6 = args[5];
				ovar7 = args[6];
				ovar8 = args[7];
				ovar9 = args[8];
				ovar10 = args[9];
				ovar11 = args[10];
				ovar12 = args[11];
				ovar13 = args[12];
				ovar14 = args[13];
				ovar15 = args[14];
				ovar16 = args[15];
				ovar17 = args[16];
				ovar18 = args[17];
				ovar19 = args[18];
				return ret;
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}
	}
}
