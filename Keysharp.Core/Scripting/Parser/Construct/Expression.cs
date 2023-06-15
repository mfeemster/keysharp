using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using Keysharp.Core;
using static System.Formats.Asn1.AsnWriter;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private bool memberVarsStatic = false;

		private CodeExpression ParseExpression(List<object> parts, bool create)
		{
			RemoveExcessParentheses(parts);
			var rescanned = false;
			start:
			var rescan = false;

			for (var i = 0; i < parts.Count; i++)
			{
				if (parts[i] is string part)
				{
					if (part[0] == ParenOpen)
					{
						var n = i + 1;
						var paren = ExtractRange(parts, n, Set(parts, i));
						parts.RemoveAt(n);
						n -= 2;
						var call = n > -1 && parts[n] is CodeExpression && !(parts[n] is CodePrimitiveExpression);

						if (call && parts[n] is CodeMethodInvokeExpression cmie && cmie.Parameters[0] is CodeFieldReferenceExpression)
							call = false;

						if (call)
						{
							var invoke = (CodeMethodInvokeExpression)InternalMethods.Invoke;
							CodeMethodInvokeExpression tempinvoke = null;

							//Distinguish between Index which is an array or dictionary lookup and GetMethodOrProperty which is getting a method or property to be called.
							if (parts[n] is CodeMethodInvokeExpression indexcmie)
							{
								tempinvoke = (CodeMethodInvokeExpression)InternalMethods.GetMethodOrProperty;
								tempinvoke.Parameters.Clear();
								tempinvoke.Parameters.AddRange(indexcmie.Parameters);
								parts[n] = tempinvoke;//Replace Index with MethodIndex.
								getMethodCalls[typeStack.Peek()].GetOrAdd(Scope.ToLower()).Add(tempinvoke);
							}

							_ = invoke.Parameters.Add((CodeExpression)parts[n]);

							if (paren.Count != 0)
							{
								var passed = ParseMultiExpression(paren.ToArray(), create);
								invoke.Parameters.AddRange(passed);
							}

							if (tempinvoke != null)
								tempinvoke.Parameters.Add(new CodePrimitiveExpression(invoke.Parameters.Count - 1));

							parts[i] = invoke;
							parts.RemoveAt(n);
							i--;//Do this to ensure we don't miss a token, particularly in the case of chained method calls such as: MyGui.Add("Button",, "Click Me").OnEvent("Click", "MenuHandler").
						}
						else
						{
							if (i < parts.Count - 1 && parts[i + 1] as string == "=>")
							{
								parts[i] = paren;
							}
							else if (paren.Count == 0)
							{
								parts.RemoveAt(i);
							}
							else
								parts[i] = ParseExpression(paren, create);
						}
					}
					else if (part[0] == ParenClose)
						rescan = true;
					else if (part.Length > 1 && part[0] == StringBound && part[part.Length - 1] == StringBound)
						parts[i] = new CodePrimitiveExpression(EscapedString(part.Substring(1, part.Length - 2), false));
					else if (PrimitiveToExpression(part) is CodeExpression result)
					{
						parts[i] = result;
						//if (result != null)
						//{
						//  var longresult = result.ParseLong(false);//Also supports hex.
						//
						//  if (longresult.HasValue)
						//      parts[i] = new CodeSnippetExpression($"{longresult.Value}L");
						//  else
						//      parts[i] = new CodePrimitiveExpression(result);
						//}
						//else
						//  parts[i] = new CodePrimitiveExpression(result);
					}
					else if (IsIdentifier(part, true) && !IsKeyword(part))
					{
						var low = part.ToLowerInvariant();
						parts[i] = libProperties.TryGetValue(low, out var pi)
								   ? new CodeVariableReferenceExpression(pi.Name)//Using static declarations obviate the need for specifying the static class type.
								   //Check for function or property calls on an object, which only count as read operations.
								   : VarIdOrConstant(part, i == 0 && create && (i == parts.Count - 1 || (i < parts.Count - 1 && parts[i + 1] is string s && !s.StartsWith("["))), false);
					}
					else if (part.Length == 1 && part[0] == BlockOpen)
					{
						var n = i + 1;
						var paren = ExtractRange(parts, n, Set(parts, i));
						var invoke = (CodeMethodInvokeExpression)InternalMethods.Dictionary;
						ParseObject(paren, out var keys, out var values, create);//Might want this to always be false, because it would seem weird to create variable here inside of the arguments passed to Map().
						_ = invoke.Parameters.Add(new CodeArrayCreateExpression(typeof(object), keys));
						_ = invoke.Parameters.Add(new CodeArrayCreateExpression(typeof(object), values));
						parts[i] = invoke;
						parts.RemoveAt(n);
						i--;
					}
					else if (part.Length > 0 && part == "[" || part == "[*")
					{
						var n = i + 1;

						if (part == "[*")//Special case for accessing a property. This is similar to the array construction one below, but different enough to warrant its own section.
						{
							//var closing = parts.FirstIndexOf(ob => ob.ToString() == "*]", n);
							var closing = parts.FindIndex(n, ob => ob.ToString() == "*]");

							if (closing != -1)
							{
								var proptokens = ExtractRange(parts, n, closing);
								parts.RemoveAt(n);

								if (i > 0 && parts[i - 1] is CodeExpression)
								{
									CodeMethodInvokeExpression invoke;

									if (n + 1 < parts.Count && parts[n].ToString() == ":=")//Detect if this was a property assignment by searching for a := then a token after it, which is the value to assign.
									{
										var valtokens = ExtractRange(parts, n + 1, parts.Count);
										var val = ParseMultiExpression(valtokens.ToArray(), create);
										invoke = (CodeMethodInvokeExpression)InternalMethods.SetPropertyValue;
										n = i - 1;

										if (parts[n] is CodeVariableReferenceExpression cvre && string.Compare(cvre.VariableName, "this", true) == 0)
										{
											var cse = new CodeSnippetExpression("this");//Required because otherwise it will be @this, which fails to compile.
											parts[n] = cse;
											_ = invoke.Parameters.Add(cse);
										}
										else
											_ = invoke.Parameters.Add((CodeExpression)parts[n]);

										var index = ParseMultiExpression(proptokens.ToArray(), create);

										if (index.Length > 1)
											throw new ParseException("Cannot have multipart expression in a property name.");
										else if (index.Length == 0)
											throw new ParseException("Cannot access an empty property.");
										else if (index[0] is CodeArrayIndexerExpression caie)
											_ = invoke.Parameters.Add(caie.Indices[0]);//Don't want to use Vars if the property was dynamically specified, rather use the expression that was returned as the index to Vars.
										else
											_ = invoke.Parameters.Add(index[0]);

										_ = invoke.Parameters.Add(val[0]);
										parts.RemoveAt(n);//Remove first element (done again below).
										setPropertyValueCalls[typeStack.Peek()].GetOrAdd(Scope.ToLower()).Add(invoke);
									}
									else
									{
										invoke = (CodeMethodInvokeExpression)InternalMethods.GetPropertyValue;
										n = i - 1;

										if (parts[n] is CodeVariableReferenceExpression cvre && string.Compare(cvre.VariableName, "this", true) == 0)
										{
											var cse = new CodeSnippetExpression("this");//Required because otherwise it will be @this, which fails to compile.
											parts[n] = cse;
											_ = invoke.Parameters.Add(cse);
										}
										else
											_ = invoke.Parameters.Add((CodeExpression)parts[n]);

										var index = ParseMultiExpression(proptokens.ToArray(), create);

										if (index.Length > 1)
											throw new ParseException("Cannot have multipart expression in a property name.");
										else if (index.Length == 0)
											throw new ParseException("Cannot access an empty property.");
										else if (index[0] is CodeArrayIndexerExpression caie)
											_ = invoke.Parameters.Add(caie.Indices[0]);//Don't want to use Vars if the property was dynamically specified, rather use the expression that was returned as the index to Vars.
										else
											_ = invoke.Parameters.Add(index[0]);//The problem is that this will have quotes on it at this point and be treated as a string no matter what, even if it's a dynamic var.//TODO

										getPropertyValueCalls[typeStack.Peek()].GetOrAdd(Scope.ToLower()).Add(invoke);
									}

									parts[i] = invoke;
									parts.RemoveAt(n);
									i--;
								}
							}
							else
								throw new ParseException("[* did not have a closing *].");//This should never happen because it's autogenerated by this parser, so a user would never normally enter it.
						}
						else
						{
							var paren = ExtractRange(parts, n, Set(parts, i));
							parts.RemoveAt(n);

							if (i > 0 && parts[i - 1] is CodeExpression)
							{
								var invoke = (CodeMethodInvokeExpression)InternalMethods.Index;
								n = i - 1;
								_ = invoke.Parameters.Add((CodeExpression)parts[n]);
								var index = ParseMultiExpression(paren.ToArray(), create);

								if (index.Length == 0)
									_ = invoke.Parameters.Add(new CodeSnippetExpression("new object[] { }"));
								else if (index[0] is CodeBinaryOperatorExpression cbe && (cbe.Operator == CodeBinaryOperatorType.BooleanAnd || cbe.Operator == CodeBinaryOperatorType.BooleanOr))
									_ = invoke.Parameters.Add(new CodeMethodInvokeExpression(index[0], "ParseObject"));
								else
									foreach (var p in index)
										_ = invoke.Parameters.Add(p);

								parts[i] = invoke;
								parts.RemoveAt(n);
								i--;
							}
							else
								parts[i] = new CodeObjectCreateExpression(typeof(Core.Array), new CodeArrayCreateExpression(typeof(object[]), ParseMultiExpression(paren.ToArray(), create)));
						}
					}
					else if (part.Length > 1 && part[part.Length - 1] == ParenOpen)
					{
						var dynamic = false;
						var name = part.Substring(0, part.Length - 1);

						if (!IsIdentifier(name))
						{
							if (IsDynamicReference(name))
								dynamic = true;
							else
								throw new ParseException("Invalid function name.");
						}
						else
							CheckPersistent(name);

						var n = i + 1;
						var paren = ExtractRange(parts, n, Set(parts, i));
						parts.RemoveAt(n);
						CodeMethodInvokeExpression invoke = null;

						//Special processing is needed to convert an expression passed to a HotIf() to a separate function, then pass it with FuncObj().
						//Do not do this if a FuncObj is already being passed, whcih will be the case when #HotIf is found in the preprocessing stage.
						if (name == "HotIf" && paren.Count > 1 && !paren[0].ToString().StartsWith("FuncObj"))
						{
							var hotiffuncname = $"HotIf_{hotifcount++}";
							var hotifexpr = ParseExpression(paren, false);
							var hotifmethod = LocalMethod(hotiffuncname);
							_ = hotifmethod.Statements.Add(new CodeMethodReturnStatement(hotifexpr));
							methods[targetClass].Add(hotiffuncname, hotifmethod);
							invoke = LocalMethodInvoke(name);
							_ = invoke.Parameters.Add(new CodeSnippetExpression("FuncObj(\"" + hotiffuncname + "\")"));//Can't use interpolated string here because the AStyle formatter misinterprets it.
						}
						else//Any other function call aside from HotIf().
						{
							CodeExpression[] passed = null;
							var inparenct = 0;
							var lastisstar = paren.Count > 0 && paren.Last().ToString() == "*";
							//List<int> refIndexes = null;//This was to handle parameters where the caller passed & to signify it's meant to be passed by reference, but we're no longer supporting reference parameters.
							var paramIndex = 0;
							inparenct = 0;
							//refIndexes = new List<int>(paren.Count);//Refs can't be done in C# with variadic params, which we use for most calls.

							for (var i1 = 0; i1 < paren.Count; i1++)
							{
								var p = paren[i1].ToString();

								if (p.Contains('('))//Indicates the start of a new expression as a parameter, so don't count it, because it'll be handled recursively on its own.
									inparenct++;
								else if (p.Contains(')'))
									inparenct--;
								else if (inparenct == 0)
								{
									if (p == ",")//Reached the end of a parameter, so increment parameter index.
									{
										paramIndex++;
									}
									else if (lastisstar && p == "*")//p can be * if the param is just a variable reference, or if it's a function call whose result is to be expanded. Ex: func(func2(val)*)
									{
										paren.RemoveAt(i1);
										i1--;
									}

									//else if (p == "&" && (i1 == 0 || paren[i1 - 1].ToString() == ","))
									//{
									//  refIndexes.Add(paramIndex);
									//  paren.RemoveAt(i1);
									//}
								}
							}

							if (paren.Count != 0)
								passed = ParseMultiExpression(paren.ToArray(), /*create*/false);//override the value of create with false because the arguments passed into a function should never be created automatically.

							if (dynamic)
							{
								invoke = (CodeMethodInvokeExpression)InternalMethods.Invoke;
								var getmethod = (CodeMethodInvokeExpression)InternalMethods.GetMethodOrProperty;
								var expand = VarIdExpand(name);
								_ = getmethod.Parameters.Add(new CodePrimitiveExpression(null));

								//The returned object is a call to Vars, but since we are doing a function call here
								//we need to call Invoke() instead, but we still want the arguments that
								//were passed to Vars, so extract them.
								if (expand is CodeArrayIndexerExpression caie)
									foreach (CodeExpression index in caie.Indices)
									{
										if (index is CodeVariableReferenceExpression cvre)
											_ = getmethod.Parameters.Add(new CodeSnippetExpression($"{cvre.VariableName}.ToString()"));
										else
											_ = getmethod.Parameters.Add(index);
									}

								_ = invoke.Parameters.Add(getmethod);
							}
							else
								invoke = LocalMethodInvoke(name);

							var passedLen = passed != null ? passed.Length : 0;

							if (passedLen > 0)
							{
								if (passed.Length == 1 && passed[0] is CodeBinaryOperatorExpression cbe && (cbe.Operator == CodeBinaryOperatorType.BooleanAnd || cbe.Operator == CodeBinaryOperatorType.BooleanOr))
								{
									_ = invoke.Parameters.Add(new CodeMethodInvokeExpression(passed[0], "ParseObject"));
								}
								else
								{
									if (lastisstar)
									{
										invoke.Parameters.AddRange(passed.Take(passed.Length - 1).ToArray());
										var combineExpr = LocalMethodInvoke("FlattenParam");
										_ = combineExpr.Parameters.Add(passed[passed.Length - 1]);
										_ = invoke.Parameters.Add(combineExpr);
									}
									else
										invoke.Parameters.AddRange(passed);

									//for (var i1 = 0; i1 < invoke.Parameters.Count; i1++)
									//{
									//  if (refIndexes.Contains(i1))
									//  {
									//      var p = invoke.Parameters[i1];
									//      invoke.Parameters[i1] = new CodeDirectionExpression(FieldDirection.Ref, p);
									//  }
									//}
								}
							}

							if (dynamic)
							{
								if (invoke.Parameters[0] is CodeMethodInvokeExpression cmie)
									cmie.Parameters.Add(new CodePrimitiveExpression(passedLen));
							}

							allMethodCalls[typeStack.Peek()].GetOrAdd(Scope.ToLower()).Add(invoke);
						}

						parts[i] = invoke;
						invokes.Add(invoke);
						specialaddcall:
						;
					}
					//This is very strange and prevents single = from working in if statements when comparing variables such as if (x = y)
					//Not exactly sure what an "implicit assignment" is, since it's not mentioned anywhere in the documentation
					else if (IsAssignOp(part))// || IsImplicitAssignment(parts, i))
					{
						var n = i - 1;

						if (i > 0 && IsJsonObject(parts[n])) { }
						else if (n < 0 || !IsVarReference(parts[n]))
						{
							if (LaxExpressions)
							{
								if (parts[n] is CodePrimitiveExpression cpe)
								{
									if (cpe.Value is decimal decval)
										parts[n] = VarId(decval.ToString(), i == 0 && create);
									else if (cpe.Value is double d)
										parts[n] = VarId(d.ToString(), i == 0 && create);
								}
							}
							else
								throw new ParseException("Can only assign to a variable");
						}

						// (x += y) => (x = x + y)
						parts[i] = CodeBinaryOperatorType.Assign;

						if (part[0] != AssignPre && part.Length != 1)
						{
							parts.Insert(++i, ParenOpen.ToString());
							parts.Insert(++i, parts[i - 3]);

							if (part.Length > 1)
							{
								parts.Insert(++i, OperatorFromString(part.Substring(0, part.Length - 1)));
								parts.Insert(++i, ParenOpen.ToString());
								parts.Add(ParenClose.ToString());
							}

							parts.Add(ParenClose.ToString());

							//Need to take special action if it was +=, -= etc... on a property.
							if (parts[0] is CodeMethodInvokeExpression cmie && cmie.Method.MethodName == "GetPropertyValue" && parts[1] is CodeBinaryOperatorType cbot && cbot == CodeBinaryOperatorType.Assign)
							{
								var propobj = cmie.Parameters[0];
								var propname = cmie.Parameters[1];
								var invoke = (CodeMethodInvokeExpression)InternalMethods.SetPropertyValue;
								_ = invoke.Parameters.Add(propobj);
								_ = invoke.Parameters.Add(propname);
								var extract = ExtractRange(parts, 2, parts.Count);
								var val = ParseMultiExpression(extract.ToArray(), create);
								_ = invoke.Parameters.Add(val[0]);
								parts[0] = invoke;
								setPropertyValueCalls[typeStack.Peek()].GetOrAdd(Scope.ToLower()).Add(invoke);
								parts.RemoveAt(1);
							}
						}
					}
					else if (part.Length == 1 && part[0] == Multicast)
					{
						if (!LaxExpressions)
							throw new ParseException("Nested multipart expression not allowed.");

						// implement as: + Dummy(expr..)
						int z = i + 1, l = parts.Count - z;
						var sub = new List<object>(l);

						for (; z < parts.Count; z++)
							sub.Add(parts[z]);

						parts.RemoveRange(i, parts.Count - i);
						var invoke = (CodeMethodInvokeExpression)InternalMethods.OperateZero;
						_ = invoke.Parameters.Add(ParseExpression(sub, create));
						parts.Add(Script.Operator.Add);
						parts.Add(invoke);
					}
					else if (part == "=>")
					{
						var assignIndex = parts.FindIndex(0, parts.Count, o => o is CodeBinaryOperatorType cbot && cbot == CodeBinaryOperatorType.Assign);
						var cmieIndex = parts.FindIndex(0, parts.Count, o => o is CodeMethodInvokeExpression cmie);
						var cmd = new CodeMemberMethod()
						{
							Attributes = MemberAttributes.Public | MemberAttributes.Static,
							ReturnType = new CodeTypeReference(typeof(object))
						};
						void AddParts(CodeMemberMethod cmd)
						{
							var extracted = ExtractRange(parts, assignIndex + 1, parts.Count);
							parts.Add("FuncObj(");
							parts.Add("\"" + cmd.Name + "\"");
							parts.Add(",");
							parts.Add(typeStack.Peek().Name != mainClassName ? new CodeSnippetExpression("this") : "null");
							parts.Add(",");
							parts.Add($"{cmd.Parameters.Count}");
							parts.Add(")");
							i = assignIndex;//Move i back to force parsing of the tokens we just added.
						}

						if (cmieIndex != -1 && cmieIndex <  i)
						{
							var cmie = parts[cmieIndex] as CodeMethodInvokeExpression;
							var bodyParts = parts.Take(new Range(i + 1, parts.Count)).ToList();
							cmd.Name = cmie.Method.MethodName;

							foreach (var p in cmie.Parameters)
							{
								if (p is CodeVariableReferenceExpression cvre)
								{
									_ = cmd.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), cvre.VariableName));
								}
								//If a * was included, it will have been parsed above as a function argument, rather than a function parameter
								//and such arguments are passed as a call to FlattenParam(). So catch that here and create the appropriate variadic parameter.
								else if (p is CodeMethodInvokeExpression cmie2 && cmie2.Method.MethodName == "FlattenParam")
								{
									foreach (var p2 in cmie2.Parameters)
										if (p2 is CodeVariableReferenceExpression cvre2)
										{
											var cpde = new CodeParameterDeclarationExpression(typeof(object[]), cvre2.VariableName);
											_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ParamArrayAttribute))));
											_ = cmd.Parameters.Add(cpde);
										}
								}
							}

							_ = cmd.Statements.Add(new CodeMethodReturnStatement(ParseExpression(bodyParts, false)));
							methods[typeStack.Peek()][cmd.Name] = cmd;

							//Now need to handle creating a function object and assigning.
							if (assignIndex != -1 && assignIndex < i)
								AddParts(cmd);
						}
						else if (assignIndex != -1 && assignIndex < i)
						{
							//var paramParts = parts.Take(new Range(assignIndex + 1, i)).ToArray();
							if (parts[assignIndex + 1] is List<object> lo)
							{
								var funcParams = ParseFunctionParameters(string.Join("", lo));
								cmd.Name = $"anonfunc_{labelct++:X}";

								if (funcParams != null)
									foreach (var fp in funcParams)
										cmd.Parameters.Add(fp);

								var bodyParts = parts.Take(new Range(i + 1, parts.Count)).ToList();
								_ = cmd.Statements.Add(new CodeMethodReturnStatement(ParseExpression(bodyParts, false)));
								methods[typeStack.Peek()][cmd.Name] = cmd;
								AddParts(cmd);
							}
							else if (parts[assignIndex + 1] is CodeVariableReferenceExpression cvre)
							{
								cmd.Name = $"anonfunc_{labelct++:X}";

								if (parts[assignIndex + 2] as string == "*")
								{
									var cpde = new CodeParameterDeclarationExpression(typeof(object[]), cvre.VariableName);
									_ = cpde.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.ParamArrayAttribute))));
									cmd.Parameters.Add(cpde);
								}
								else
									cmd.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), cvre.VariableName));

								var bodyParts = parts.Take(new Range(i + 1, parts.Count)).ToList();
								_ = cmd.Statements.Add(new CodeMethodReturnStatement(ParseExpression(bodyParts, false)));
								methods[typeStack.Peek()][cmd.Name] = cmd;
								AddParts(cmd);
							}
						}
						else
							throw new Exception("Unsupported tokens surrounding a fat arrow function definition =>");
					}
					else if (i == parts.Count - 1 || (i < parts.Count - 1 && parts[i + 1] as string != "=>"))//Allow for a single no parentheses fat arrow function variadic parameter declaration: x := a* => 123.
					{
						var ops = OperatorFromString(part);

						if (ops == Script.Operator.Increment || ops == Script.Operator.Decrement)
						{
							int z = -1, x = i - 1, y = i + 1;
							var d = 1L;
							CodeMethodInvokeExpression shadow = null;

							if (x > -1 && parts[x] is CodeMethodInvokeExpression)
							{
								var sub = new List<object>(5)
								{
									parts[x],
										  CodeBinaryOperatorType.Assign,
										  parts[x],
										  Script.Operator.Add,
										  d
								};
								parts.RemoveAt(i);
								parts[x] = ParseExpression(sub, create);
								i = x;
								continue;
							}

							if (LaxExpressions)
							{
								while (y < parts.Count)
								{
									var nextOps = Script.Operator.ValueEquality;

									if (parts[y] is Script.Operator so)
										nextOps = so;
									else if (parts[y] is string os)
									{
										try { nextOps = OperatorFromString(os); }
										catch { break; }
									}
									else
										break;

									if (nextOps == Script.Operator.Increment)
										d++;
									else if (nextOps == Script.Operator.Decrement)
										d--;
									else
										break;

									parts.RemoveAt(y);
								}
							}

							if (x > -1 && (IsVarReference(parts[x]) || parts[x] is CodePropertyReferenceExpression))
								z = x;

							if (y < parts.Count && parts[y] is string sy && !IsOperator(sy))
							{
								if (z != -1)
								{
									if (LaxExpressions)
									{
										parts.Insert(y, Script.Operator.Concat);
										z = x;
									}
									else
										throw new ParseException("Cannot use both prefix and postfix operators on the same variable");
								}

								if (z == -1)
									z = y;

								if (LaxExpressions)
								{
									if (parts[z] is string psz && psz.Length == 1 && psz[0] == ParenOpen)
									{
										var zx = new[] { z + 1, z + 2 };

										if (zx[1] < parts.Count &&
												parts[zx[1]] is string zx1 && zx1.Length == 1 && zx1[0] == ParenClose &&
												(parts[zx[0]] is string zx0 && IsDynamicReference(zx0) || IsVarReference(parts[zx[0]])))
										{
											parts.RemoveAt(zx[1]);
											parts.RemoveAt(z);
										}
										else
										{
											parts.RemoveAt(i);
											i--;
											continue;
										}
									}
								}
							}

							if (z == -1)
							{
								if (LaxExpressions)
								{
									if ((x > 0 && (parts[x] is CodeBinaryOperatorExpression || parts[x] is CodeMethodInvokeExpression || parts[x] is CodePrimitiveExpression)) ||
											(y < parts.Count && (parts[y] is string && !IsOperator(parts[y] as string) || parts[y] is Script.Operator)))
									{
										parts.RemoveAt(i);
										i--;
										continue;
									}
								}
								else
									throw new ParseException("Neither left or right hand side of operator is a variable");
							}

							if (parts[z] is string sz && sz.Length > 0 && sz[0] == StringBound)
							{
								parts.RemoveAt(Math.Max(i, z));
								parts.RemoveAt(Math.Min(i, z));
								continue;
							}

							if (LaxExpressions)
							{
								var w = z + (z == x ? 2 : 1);

								if (w < parts.Count && ((parts[w] is string sw
														 && IsAssignOp(sw)) || IsVarAssignment(parts[w])))
								{
									var l = parts.Count - w;
									var sub = new List<object>(l + 1);
									sub.Add(parts[z]);

									for (var wx = w; wx < parts.Count; wx++)
										sub.Add(parts[wx]);

									shadow = (CodeMethodInvokeExpression)InternalMethods.OperateZero;
									_ = shadow.Parameters.Add(ParseExpression(sub, create));
									parts.RemoveRange(w, l);
								}
							}

							var list = new List<object>(9)
							{
								parts[z],
									  new string(new[] { Add, Equal }),
									  new CodeSnippetExpression($"{(ops == Script.Operator.Increment ? d : -d)}L")
							};

							if (shadow != null)
							{
								list.Add(Script.Operator.Add);
								list.Add(shadow);
							}

							if (z < i) // postfix, so adjust
							{
								list.Insert(0, ParenOpen.ToString());
								list.Add(ParenClose.ToString());
								list.Add(ops == Script.Operator.Increment ? Script.Operator.Minus : Script.Operator.Add);
								list.Add(new CodePrimitiveExpression(d));
							}

							x = Math.Min(i, z);
							y = Math.Max(i, z);
							parts[x] = ParseExpression(list, create);
							parts.RemoveAt(y);
							i = x;
						}
						else
						{
							if (part.Length == 1 && part[0] == Dereference)
							{
								var deref = false;

								if (i == 0)
									deref = true;
								else
								{
									var x = i - 1;
									deref = parts[x] is Script.Operator || IsVarAssignment(parts[x]) ||
											(parts[x] is string sx && sx.Length == 1 && sx[0] == '(');
								}

								if (deref)
								{
									var y = i + 1;

									if (y < parts.Count && (IsVarReference(parts[y]) ||
															(parts[y] is string sy && IsIdentifier(sy) && !IsKeyword(sy))))
										ops = Script.Operator.Dereference;
								}
							}

							parts[i] = ops;
						}
					}
				}
			}

			if (rescan && !rescanned)
			{
				rescanned = true;//Prevent infinite loop.
				goto start;
			}

			for (var i = 1; i < parts.Count; i++)
			{
				if (parts[i] is Script.Operator op &&
						(
							parts[i - 1] is Script.Operator ||
							parts[i - 1] as CodeBinaryOperatorType? == CodeBinaryOperatorType.Assign || IsVarAssignment(parts[i - 1])
						) &&
						IsUnaryOperator(op))
				{
					int n = i + 1, m = n + 1;
					var u = n;

					while (u < parts.Count && parts[u] is Script.Operator sou && IsUnaryOperator(sou)) u++;

					if (u == parts.Count)
					{
						if (LaxExpressions)
						{
							u--;

							while (parts[u] is Script.Operator sou && (sou == Script.Operator.Add || sou == Script.Operator.Subtract))
								parts.RemoveAt(u--);

							if (u + 1 < n)
							{
								i = u;
								continue;
							}
						}

						throw new ParseException("Compounding unary operator with no operand");
					}

					if (u > n)
					{
						var sub = new List<object>(++u - n);

						for (var x = n; x < u; x++)
							sub.Add(parts[x]);

						parts.RemoveRange(n, u - n);
						parts.Insert(n, ParseExpression(sub, create));
					}

					if (m + 1 < parts.Count && IsVarReference(parts[n]) && IsVarAssignment(parts[m]))
						MergeAssignmentAt(parts, i + 2);

					if (m > parts.Count)
						throw new ParseException("Unary operator without operand");

					//var op = (Script.Operator)parts[i];

					//if (parts.Count > n + 1
					//      && parts[n + 1] is Script.Operator opp1
					//      && opp1 == Script.Operator.Power)
					//{
					//  Console.WriteLine("power op is next");
					//}

					if (parts[n] is CodePrimitiveExpression parent && op == Script.Operator.Subtract)
					{
						if (parent.Value is long ll)
							parent.Value = -ll;
						else if (parent.Value is double dd)
							parent.Value = -dd;
						else if (parent.Value is string ss)
							parent.Value = string.Concat(Minus.ToString(), ss);
						else if (parent.Value is int ii)
							parent.Value = -ii;
						else if (parent.Value is decimal mm)
							parent.Value = -mm;
						else
							throw new ArgumentOutOfRangeException();

						parts.RemoveAt(i);
					}
					else if (parts[n] is CodeSnippetExpression cseparent && op == Script.Operator.Subtract)//Long requires snippet, since CodePrimitiveExpression does not append L.
					{
						cseparent.Value = cseparent.Value is string cses ? string.Concat(Minus.ToString(), cses) : throw new ArgumentOutOfRangeException();
						parts.RemoveAt(i);
					}
					else if (op == Script.Operator.Add)
					{
						parts.RemoveAt(i);
					}
					else
					{
						var invoke = (CodeMethodInvokeExpression)InternalMethods.OperateUnary;
						_ = invoke.Parameters.Add(OperatorAsFieldReference(op));
						_ = invoke.Parameters.Add(VarMixedExpr(parts[n]));
						next:
						parts[i] = invoke;
						parts.RemoveAt(n);
					}
				}
			}

			var scan = true;
			var level = -1;

			//Unsure what this final loop actually does.
			while (scan)
			{
				scan = false;

				for (var i = 0; i < parts.Count; i++)
				{
					if (parts[i] is Script.Operator op && op != Script.Operator.Assign)
					{
						scan = true;

						if (OperatorPrecedence(op) < level)
							continue;

						int x = i - 1, y = i + 1;
						var invoke = new CodeMethodInvokeExpression();

						if (i + 3 < parts.Count && IsVarReference(parts[i + 1]) && (parts[i + 2] as CodeBinaryOperatorType? ) == CodeBinaryOperatorType.Assign)
							MergeAssignmentAt(parts, i + 2);

						if (op == Script.Operator.TernaryA)
						{
							if (x < 0)
							{
								if (LaxExpressions)
									return new CodePrimitiveExpression(null);
								else
									throw new ParseException("Ternary with no condition.");
							}

							var cond = VarMixedExpr(parts[x]);
							var eval = (CodeMethodInvokeExpression)InternalMethods.IfElse;
							_ = eval.Parameters.Add(cond);
							var ternary = new CodeTernaryOperatorExpression { Condition = eval };
							int depth = 1, max = parts.Count - i, start = i;
							var branch = new[] { new List<object>(max), new List<object>(max) };

							for (i++; i < parts.Count; i++)
							{
								switch (parts[i] as Script.Operator? )
								{
									case Script.Operator.TernaryA:
										depth++;
										break;

									case Script.Operator.TernaryB:
										depth--;
										break;
								}

								if (depth == 0)
								{
									for (var n = i + 1; n < parts.Count; n++)
										branch[1].Add(parts[n]);

									break;
								}
								else
									branch[0].Add(parts[i]);
							}

							if (branch[0].Count == 0)
								throw new ParseException("Ternary operator must have at least one branch");

							if (branch[1].Count == 0)
								branch[1].Add(new CodePrimitiveExpression(null));

							ternary.TrueBranch = ParseExpression(branch[0], create);
							ternary.FalseBranch = ParseExpression(branch[1], create);
							//CodeDOM does not have built in support for ternary operators. So we must manually create the code string for the ternary,
							//then use a code snippet to hold the string. This is not ideal, but there is no other way.
							var evalstr = Ch.CodeToString(eval);
							var tbs = Ch.CodeToString(ternary.TrueBranch);
							var fbs = Ch.CodeToString(ternary.FalseBranch);
							parts[x] = new CodeSnippetExpression($"(_ = {evalstr} ? {tbs} : {fbs})");
							parts.RemoveRange(start, parts.Count - start);
							//i++;//Tried, but not needed.
							//continue;
						}
						else if (op == Script.Operator.NullAssign)//Unsure what this does.
						{
							if (x < 0)
								throw new ParseException("Nullable assignment with no condition.");

							var n = i + 1;

							if (n >= parts.Count)
								throw new ParseException("Nullable assignment with no right-hand operator");

							var result = InternalVariable;
							var left = new CodeBinaryOperatorExpression(result, CodeBinaryOperatorType.Assign, VarMixedExpr(parts[x]));
							var eval = (CodeMethodInvokeExpression)InternalMethods.IfElse;
							_ = eval.Parameters.Add(left);
							var ternary = new CodeTernaryOperatorExpression { Condition = eval, TrueBranch = result };
							var right = new List<object>();

							while (n < parts.Count)
								right.Add(parts[n++]);

							ternary.FalseBranch = ParseExpression(right, create);
							parts[x] = ternary;
							parts.RemoveRange(i, parts.Count - i);
						}
						else if (x == -1)
						{
							var z = y + 1;

							if (op == Script.Operator.LogicalNotEx && IsVarReference(parts[y]) && z < parts.Count)
								MergeAssignmentAt(parts, z);

							if (LaxExpressions)
							{
								if (y > parts.Count - 1)
									return new CodePrimitiveExpression(null);
							}

							invoke.Method = (CodeMethodReferenceExpression)InternalMethods.OperateUnary;
							_ = invoke.Parameters.Add(OperatorAsFieldReference(op));
							_ = invoke.Parameters.Add(VarMixedExpr(parts[y]));
							parts[i] = invoke;
							parts.RemoveAt(y);
						}
						else
						{
							if (op == Script.Operator.BooleanAnd || op == Script.Operator.BooleanOr)
							{
								var boolean = new CodeBinaryOperatorExpression();
								boolean.Operator = op == Script.Operator.BooleanAnd ? CodeBinaryOperatorType.BooleanAnd : CodeBinaryOperatorType.BooleanOr;
								var iftest = (CodeMethodInvokeExpression)InternalMethods.IfElse;
								_ = iftest.Parameters.Add(VarMixedExpr(parts[x]));
								boolean.Left = iftest;
								iftest = (CodeMethodInvokeExpression)InternalMethods.IfElse;
								var next = parts[y] as Script.Operator?;

								if (next == Script.Operator.BooleanAnd || next == Script.Operator.BooleanOr)
								{
									if (LaxExpressions)
										_ = iftest.Parameters.Add(new CodePrimitiveExpression(false));
									else
										throw new ParseException(ExInvalidExpression);
								}
								else
								{
									_ = iftest.Parameters.Add(VarMixedExpr(parts[y]));
									parts.RemoveAt(y);
								}

								boolean.Right = iftest;

								if (x > 0 && parts[x - 1] is CodeBinaryOperatorType cbot && cbot == CodeBinaryOperatorType.Assign)//z := x && y
									parts[x] = new CodeMethodInvokeExpression(boolean, "ParseObject");
								else if (x + 1 < parts.Count && parts[0] is CodeVariableReferenceExpression &&
										 parts[x + 1] is Script.Operator sop &&
										 (sop == Script.Operator.BooleanAnd || sop == Script.Operator.BooleanOr))//z := (x && y)
									parts[x] = new CodeMethodInvokeExpression(boolean, "ParseObject");
								else
									parts[x] = boolean;
							}
							else
							{
								if (LaxExpressions)
								{
									if (parts[x] is Script.Operator sox && sox == Script.Operator.TernaryA)
									{
										parts[x] = new CodePrimitiveExpression(null);
										goto next;
									}

									if (y > parts.Count - 1)
										return new CodePrimitiveExpression(null);
								}
								else
									throw new ParseException(ExInvalidExpression);

								invoke.Method = (CodeMethodReferenceExpression)InternalMethods.Operate;
								_ = invoke.Parameters.Add(OperatorAsFieldReference(op));

								if (LaxExpressions && parts[i] is Script.Operator soi
										&& soi == Script.Operator.Concat
										&& parts[x] as CodeBinaryOperatorType? == CodeBinaryOperatorType.Assign)
									_ = invoke.Parameters.Add(new CodePrimitiveExpression(string.Empty));
								else
									_ = invoke.Parameters.Add(VarMixedExpr(parts[x]));

								var py = parts[y];

								if (op == Script.Operator.Is)//When using "is", we pass the type as a string, which is then compared internally inside of IfLegacy().
								{
									if (py is CodeVariableReferenceExpression pycvre)
										_ = invoke.Parameters.Add(new CodePrimitiveExpression(pycvre.VariableName));
									else if (py is CodePrimitiveExpression pycpe)
										_ = invoke.Parameters.Add(new CodePrimitiveExpression(pycpe.Value.ToString()));
								}
								else
									_ = invoke.Parameters.Add(VarMixedExpr(py));

								parts[x] = invoke;
								next:
								parts.RemoveAt(y);
							}

							parts.RemoveAt(i);
						}

						i--;
					}
					else if (parts[i] as CodeBinaryOperatorType? != CodeBinaryOperatorType.Assign && !(parts[i] is CodeSnippetStatement))
					{
						var x = i - 1;

						if (x > 0 && !(parts[x] is Script.Operator || parts[x] is CodeBinaryOperatorType || parts[x] is CodeSnippetStatement))
						{
							parts.Insert(i, Script.Operator.Concat);
							i--;
							continue;
						}
					}
				}

				level--;
			}

			for (var i = parts.Count - 1; i > 0; i--)
				MergeAssignmentAt(parts, i);

			if (parts.Count > 1)
			{
				for (var i = 0; i < parts.Count; i++)
				{
					var typed = false;

					if (LaxExpressions)
						typed = IsVarAssignment(parts[i]) || IsVarReference(parts[i]);

					if (!(typed || parts[i] is CodeMethodInvokeExpression
							|| parts[i] is CodePrimitiveExpression
							|| parts[i] is CodeTernaryOperatorExpression
							|| parts[i] is CodeBinaryOperatorExpression
							|| parts[i] is CodePropertyReferenceExpression))
						throw new ArgumentOutOfRangeException();

					if (i % 2 == 1)
						parts.Insert(i, Script.Operator.Concat);
				}

				var concat = ParseExpression(parts, create);
				parts.Clear();
				parts.Add(concat);
			}

			if (parts.Count != 1)
				throw new ArgumentOutOfRangeException();

			return IsVarAssignment(parts[0]) ? (CodeBinaryOperatorExpression)parts[0] : (CodeExpression)parts[0];
		}

		private CodeExpressionStatement[] ParseMultiExpression(string code, bool create)
		{
			var tokens = SplitTokens(code);
			var n = tokens.Count - 2;

			if (tokens.Count > 1 && tokens[n] is string s
					&& s.Length > 0 && s[0] == Multicast)
			{
				var arg = ((string)tokens[n + 1]).ToUpperInvariant().Trim();
				arg = arg.Length == 1 ? arg : arg.TrimEnd('S');

				switch (arg)
				{
					case "S":
					case "SECOND":
					case "M":
					case "MINUTE":
					case "H":
					case "HOUR":
					case "D":
					case "DAY":
						return new[] { new CodeExpressionStatement(ParseDateExpression(code)) };
				}
			}

			//for (var i = 0; i < tokens.Count; i++)
			//{
			//  if (tokens[i] is string tok && tok == "**")
			//  {
			//  }
			//}
			var result = ParseMultiExpression(tokens.ToArray(), create);
			var statements = new CodeExpressionStatement[result.Length];

			for (var i = 0; i < result.Length; i++)
				statements[i] = new CodeExpressionStatement(result[i]);

			return statements;
		}

		private CodeExpression[] ParseMultiExpression(object[] parts, bool create)
		{
			var sub = new List<object>();
			var expr = new List<CodeExpression>();
			memberVarsStatic = false;

			for (var i = 0; i < parts.Length; i++)
			{
				if (!(parts[i] is string s) || s.Length == 0)
				{
					sub.Add(parts[i]);
					continue;
				}

				var part = parts[i] as string;

				if (i == 0 && part == FunctionStatic)
				{
					memberVarsStatic = true;
					continue;
				}

				var next = Set(parts, i);

				if (next > 0)
				{
					for (; i < next; i++)
						sub.Add(parts[i]);

					i--;
					continue;
				}

				var check = s;

				//This will properly handle inserting null when passing arguments like a,,c or ,b,c or a,b,
				//When not parsing arguments and instead just parsing regular expressions, it will cause
				//a compiler error because just saying "null" in the wrong place is a syntax error.
				//This is a good way to help catch syntax bugs in a script.
				if (check == ",")
				{
					if (sub.Count != 0)
					{
						expr.Add(ParseExpression(sub, create));
						sub.Clear();
					}
					else
					{
						expr.Add(new CodePrimitiveExpression(null));
						//expr.Add(new CodeSnippetExpression("UnsetArg.Default"));
					}

					if (i == parts.Length - 1)
						expr.Add(new CodePrimitiveExpression(null));

					//expr.Add(new CodeSnippetExpression("UnsetArg.Default"));
					continue;
				}
				else
					sub.Add(parts[i]);
			}

			if (sub.Count != 0)
				expr.Add(ParseExpression(sub, create));

			return expr.ToArray();
		}

		/*  private List<string> ParseArguments(List<object> paren)
		    {
		    var templist = new List<string>(paren.Count);

		    //When a comma is followed by another comma, it means it's an empty argument.
		    for (var p = 0; p < paren.Count; p++)
		    {
		        var pstr = paren[p].ToString();

		        if (pstr == ",")
		        {
		            if (p == 0)//First element was a comma
		            {
		                templist.Add("null");
		                templist.Add(",");
		            }

		            if (p == paren.Count - 1)//Last element was a comma
		            {
		                if (templist[templist.Count - 1] != ",")
		                    templist.Add(",");

		                templist.Add("null");
		            }
		            else if (p < paren.Count - 1 && (paren[p + 1].ToString() == ","))//This element and the previous one were commas.
		            {
		                if (templist[templist.Count - 1] != ",")
		                    templist.Add(",");

		                templist.Add("null");
		            }
		            else if (p != 0)
		                templist.Add(pstr);//Just add the comma.
		        }
		        else
		            templist.Add(pstr);
		    }

		    return templist;
		    }*/

		private CodeExpression ParseSingleExpression(string code, bool create)
		{
			var tokens = SplitTokens(code);
			return ParseExpression(tokens, create);
		}
	}
}