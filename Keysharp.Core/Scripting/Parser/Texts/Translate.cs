using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		[Conditional(Legacy)]
		internal static void ParameterPrepend(ref string param, string insert)
		{
			if (IsExpressionParameter(param))
			{
				var buffer = new StringBuilder(param.Length + insert.Length + 2);
				_ = buffer.Append(Resolve);
				_ = buffer.Append(SingleSpace);
				_ = buffer.Append(StringBound);
				_ = buffer.Append(insert);
				_ = buffer.Append(StringBound);
				_ = buffer.Append(SingleSpace);
				_ = buffer.Append(Concatenate);
				_ = buffer.Append(SingleSpace);
				_ = buffer.Append(param, 2, param.Length - 2);
				param = buffer.ToString();
			}
			else
				param = string.Concat(insert, param);
		}

		[Conditional(Legacy)]
		internal static void Translate(CodeLine codeLine, ref string code)
		{
			string cmd, param;
			var cspan = code.AsSpan();
			var z = cspan.IndexOfAny(SpaceMultiDelimSv);

			if (z == -1)
			{
				cmd = code;
				param = string.Empty;
			}
			else
			{
				cmd = code.Substring(0, z);
				param = cspan.Slice(z).TrimStart(SpaceMultiDelim).ToString();
			}

			var replaced = new StringBuilder(code.Length);
			var parts = SplitCommandParameters(param);

			if (parts.Length > 0)
				parts[parts.Length - 1] = StripCommentSingle(parts[parts.Length - 1]);

			for (var i = 0; i < parts.Length; i++)
			{
				if (IsExpressionParameter(parts[i]))
				{
					var e = parts[i].IndexOf(Resolve) + 1;
					parts[i] = e < parts[i].Length ? parts[i].Substring(e) : new string(StringBound, 2);
				}
				else
				{
					parts[i] = parts[i].TrimStart(Spaces);
					var l = parts[i].Length;

					if (l > 1 && parts[i][0] == Resolve && parts[i][l - 1] == Resolve)
						parts[i] = parts[i].Substring(1, l - 2);
					else
					{
						var str = StringBound.ToString();
						parts[i] = string.Concat(str, parts[i], str);
					}
				}
			}

			switch (cmd.ToLowerInvariant())
			{
				case "repeat":
					param = StripCommentSingle(param);

					if (param.Length > 0 && !IsPrimitiveObject(param))
						param = string.Empty;

					_ = replaced.Append("Loop ");
					_ = replaced.Append(param);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(BlockOpen);
					break;

				case "endrepeat":
					_ = replaced.Append(BlockClose);
					_ = replaced.Append(param);
					break;

				case "setworkingdir":
					_ = replaced.Append($"A_WorkingDir := {param}");
					break;

				case "setenv":
					_ = replaced.Append(parts[0].Substring(1, parts[0].Length - 2));
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = parts.Length > 1 ? replaced.Append(parts[1]) : replaced.Append(NullTxt);
					break;

				case "ifequal":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Equal);

					if (parts.Length > 1)
						_ = replaced.Append(parts[1]);

					break;

				case "ifnotequal":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Not);
					_ = replaced.Append(Equal);

					if (parts.Length > 1)
						_ = replaced.Append(parts[1]);

					break;

				case "ifgreater":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Greater);

					if (parts.Length > 1)
						_ = replaced.Append(parts[1]);

					break;

				case "ifgreaterorequal":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Greater);
					_ = replaced.Append(Equal);

					if (parts.Length > 1)
						_ = replaced.Append(parts[1]);

					break;

				case "ifless":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Less);

					if (parts.Length > 1)
						_ = replaced.Append(parts[1]);

					break;

				case "iflessorequal":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Less);
					_ = replaced.Append(Equal);

					if (parts.Length > 1)
						_ = replaced.Append(parts[1]);

					break;

				case "ifexist":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append("FileExist");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(ParenClose, 2);
					break;

				case "ifnotexist":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(Not);
					_ = replaced.Append("FileExist");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(ParenClose, 2);
					break;

				case "ifinstring":
					if (parts.Length < 2)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append("InStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(ParenClose, 2);
					break;

				case "ifnotinstring":
					if (parts.Length < 2)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(Not);
					_ = replaced.Append("InStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(ParenClose, 2);
					break;

				case "ifmsgbox":
					if (parts.Length < 1)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append("A_MsgBox");
					_ = replaced.Append(Equal);
					_ = replaced.Append(parts[0]);
					_ = replaced.Append(ParenClose);
					break;

				case "ifwinactive":
					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append("WinActive");
					_ = replaced.Append(ParenOpen);

					foreach (var part in parts)
					{
						_ = replaced.Append(part);
						_ = replaced.Append(Multicast);
					}

					if (parts.Length > 1)
						_ = replaced.Remove(replaced.Length - 1, 1);

					_ = replaced.Append(ParenClose, 2);
					break;

				case "ifwinexist":
					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append("WinExist");
					_ = replaced.Append(ParenOpen);

					foreach (var part in parts)
					{
						_ = replaced.Append(part);
						_ = replaced.Append(Multicast);
					}

					if (parts.Length > 1)
						_ = replaced.Remove(replaced.Length - 1, 1);

					_ = replaced.Append(ParenClose, 2);
					break;

				case "ifwinnotactive":
					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(Not);
					_ = replaced.Append("WinActive");
					_ = replaced.Append(ParenOpen);

					foreach (var part in parts)
					{
						_ = replaced.Append(part);
						_ = replaced.Append(Multicast);
					}

					if (parts.Length > 1)
						_ = replaced.Remove(replaced.Length - 1, 1);

					_ = replaced.Append(ParenClose, 2);
					break;

				case "ifwinnotexist":
					_ = replaced.Append(FlowIf);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(Not);
					_ = replaced.Append("WinExist");
					_ = replaced.Append(ParenOpen);

					foreach (var part in parts)
					{
						_ = replaced.Append(part);
						_ = replaced.Append(Multicast);
					}

					if (parts.Length > 1)
						_ = replaced.Remove(replaced.Length - 1, 1);

					_ = replaced.Append(ParenClose, 2);
					break;

				// HACK: convert L/R paramter for legacy StringGetPos command
				case "stringgetpos":
					if (parts.Length < 3)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(parts[0].Trim(StringBound));
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = replaced.Append("InStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append(parts[2]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append(FalseTxt);
					_ = replaced.Append(Multicast);
					_ = replaced.Append(parts.Length > 4 ? parts[4] : "0");
					_ = replaced.Append(ParenClose);
					break;

				case "stringleft":
					if (parts.Length < 3)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(parts[0]);
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = replaced.Append("SubStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append("1");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(parts[2]);
					_ = replaced.Append(ParenClose);
					break;

				case "stringlen":
					if (parts.Length < 2)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(parts[0]);
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = replaced.Append("StrLen");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(ParenClose);
					break;

				case "stringmid":
					if (parts.Length < 3)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(parts[0]);
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = replaced.Append("SubStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append(parts[2]);

					if (parts.Length > 3)
					{
						_ = replaced.Append(Multicast);
						_ = replaced.Append(parts[3]);
					}

					if (parts.Length > 4)
					{
						_ = replaced.Append(Multicast);
						_ = replaced.Append(parts[4]);
					}

					_ = replaced.Append(ParenClose);
					break;

				case "stringright":
					if (parts.Length < 3)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(parts[0]);
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = replaced.Append("SubStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append("1");
					_ = replaced.Append(Add);
					_ = replaced.Append(Minus);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[2]);
					_ = replaced.Append(ParenClose);
					_ = replaced.Append(ParenClose);
					break;

				case "stringtrimleft":
					if (parts.Length < 3)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(parts[0]);
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = replaced.Append("SubStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append("1");
					_ = replaced.Append(Add);
					_ = replaced.Append(parts[2]);
					_ = replaced.Append(ParenClose);
					break;

				case "stringtrimright":
					if (parts.Length < 3)
						throw new ParseException(ExTooFewParams, codeLine);

					_ = replaced.Append(parts[0]);
					_ = replaced.Append(AssignPre);
					_ = replaced.Append(Equal);
					_ = replaced.Append("SubStr");
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[1]);
					_ = replaced.Append(Multicast);
					_ = replaced.Append("1");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(Minus);
					_ = replaced.Append(ParenOpen);
					_ = replaced.Append(parts[2]);
					_ = replaced.Append(ParenClose);
					_ = replaced.Append(ParenClose);
					break;

				//case "sendevent":
				//case "sendinput":
				//case "sendplay":
				//  _ = replaced.Append("Send");
				//  _ = replaced.Append(Multicast);
				//  _ = replaced.Append(SingleSpace);
				//  _ = replaced.Append(param);
				//  break;

				case "sendraw":
					_ = replaced.Append("Send");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(SingleSpace);
					ParameterPrepend(ref param, "{Raw}");
					_ = replaced.Append(param);
					break;

				case "controlsendraw":
					_ = replaced.Append("ControlSend");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(SingleSpace);
					ParameterPrepend(ref param, "{Raw}");
					_ = replaced.Append(param);
					break;

				case "sendmode":
					code = string.Empty;
					break;

				case "setcapslockstate":
				case "setnumlockstate":
				case "setscrolllockstate":
					_ = replaced.Append("SetLockState");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(cmd, 3, cmd.Length - 3 - 5);
					_ = replaced.Append(Multicast);
					_ = replaced.Append(SingleSpace);
					_ = replaced.Append(param);
					break;

				case "leftclick":
				case "mouseclick":
					_ = replaced.Append("Click");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(param);
					break;

				case "leftclickdrag":
					_ = replaced.Append("MouseClickDrag");
					_ = replaced.Append(Multicast);
					_ = replaced.Append("Left");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(param);
					break;

				case "mousemove":
					_ = replaced.Append("Click");
					_ = replaced.Append(Multicast);
					_ = replaced.Append(param);
					_ = replaced.Append(Multicast);
					_ = replaced.Append("0");
					break;

				case "edit":
				case "listlines":
				case "listvars":
					replaced = null;
					break;

				case "filegetattrib":
					if (parts.Length != 2)
						replaced = null;
					else
					{
						_ = replaced.Append(parts[0].Substring(1, parts[0].Length - 2));
						_ = replaced.Append(AssignPre);
						_ = replaced.Append(Equal);
						_ = replaced.Append("FileExist");
						_ = replaced.Append(ParenOpen);
						_ = replaced.Append(parts[1]);
						_ = replaced.Append(ParenClose);
					}

					break;
			}

			if (replaced == null)
				code = string.Empty;
			else if (replaced.Length > 0)
				code = replaced.ToString();
		}
	}
}