namespace Keyview
{
	/// <summary>
	/// Much of the Scintilla-related code was taken from: https://github.com/robinrodricks/ScintillaNET.Demo
	/// </summary>
	internal partial class Keyview : Form
	{
		/// <summary>
		/// the background color of the text area
		/// </summary>
		//private const int BACK_COLOR = 0x2A211C;
		private const int BACK_COLOR = 0xEEEEEE;

		/// <summary>
		/// change this to whatever margin you want the bookmarks/breakpoints to show in
		/// </summary>
		private const int BOOKMARK_MARGIN = 2;

		private const int BOOKMARK_MARKER = 2;

		/// <summary>
		/// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
		/// </summary>
		private const bool CODEFOLDING_CIRCULAR = true;

		/// <summary>
		/// change this to whatever margin you want the code folding tree (+/-) to show in
		/// </summary>
		private const int FOLDING_MARGIN = 3;

		/// <summary>
		/// default text color of the text area
		/// </summary>
		private const int FORE_COLOR = 0x858585;

		/// <summary>
		/// change this to whatever margin you want the line numbers to show in
		/// </summary>
		private const int NUMBER_MARGIN = 1;

		private static readonly string keywords1 = "true false this thishotkey super unset isset " + Parser.GetKeywords();
		private static readonly string keywords2 = Script.GetPublicStaticPropertyNames();
		private readonly Button btnCopyFullCode = new Button();
		private readonly CheckBox chkFullCode = new CheckBox();
		private readonly string lastrun = $"{Accessors.A_AppData}/Keysharp/lastkeyviewrun.txt";
		private readonly System.Windows.Forms.Timer timer = new Timer();
		private readonly char[] trimend = ['\n', '\r'];
		private readonly double updateFreqSeconds = 1;
		private readonly CompilerHelper ch = new CompilerHelper();
		private readonly CSharpStyler csStyler = new CSharpStyler();
		private bool force = false;
		private string fullCode = "";
		private DateTime lastCompileTime = DateTime.Now;
		private DateTime lastKeyTime = DateTime.Now;
		private bool SearchIsOpen = false;
		private string trimmedCode = "";
		private readonly string trimstr = "{}\t";
		private Process scriptProcess = null;
		private readonly Button btnRunScript = new Button();
		private Dictionary<string, string> btnRunScriptText = new Dictionary<string, string>()
		{
			{ "Run", "▶ Run script (F9)" },
			{ "Stop", "⏹ Stop script (F9)" }
		};

		public Keyview()
		{
			InitializeComponent();
			Icon = Keysharp.Core.Properties.Resources.Keysharp_ico;
			btnCopyFullCode.Text = "Copy full code";
			btnCopyFullCode.Click += CopyFullCode_Click;
			btnCopyFullCode.Margin = new Padding(15);
			var host = new ToolStripControlHost(btnCopyFullCode)
			{
				Alignment = ToolStripItemAlignment.Right
			};
			_ = toolStrip1.Items.Add(host);
			chkFullCode.Text = "Full code";
			chkFullCode.CheckStateChanged += chkFullCode_CheckStateChanged;
			host = new ToolStripControlHost(chkFullCode)
			{
				Alignment = ToolStripItemAlignment.Right
			};
			_ = toolStrip1.Items.Add(host);
			Text += $" {Assembly.GetExecutingAssembly().GetName().Version}";
			btnRunScript.Text = btnRunScriptText["Run"];
			btnRunScript.Click += CopyFullCode_Click;
			btnRunScript.Margin = new Padding(15);
			host = new ToolStripControlHost(btnRunScript)
			{
				Alignment = ToolStripItemAlignment.Right
			};
			_ = toolStrip1.Items.Add(host);
			btnRunScript.Enabled = false;
			btnRunScript.Click += RunScript_Click;
		}

		private static Color IntToColor(int rgb) => Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

		//private void TxtIn_StyleNeeded(object sender, StyleNeededEventArgs e)
		//{
		//  var scintilla = sender as Scintilla;
		//  var startPos = scintilla.GetEndStyled();
		//  var endPos = e.Position;
		//  // Start styling
		//  //scintilla.StartStyling(startPos);
		//  //while (startPos < endPos)
		//  //{
		//  //  var c = (char)scintilla.GetCharAt(startPos);
		//  //  if (c == ';')
		//  //  {
		//  //      scintilla.SetStyling(endPos - startPos, Style.Cpp.CommentLine);
		//  //      break;
		//  //  }
		//  //  startPos++;
		//  //}
		//}
		private void BtnClearSearch_Click(object sender, EventArgs e) => CloseSearch();

		private void BtnNextSearch_Click(object sender, EventArgs e) => SearchManager.Find(true, false);

		private void BtnPrevSearch_Click(object sender, EventArgs e) => SearchManager.Find(false, false);

		private void chkFullCode_CheckStateChanged(object sender, EventArgs e) => SetTxtOut(chkFullCode.Checked ? fullCode : trimmedCode);

		private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			txtIn.SetEmptySelection(0);
			lastKeyTime = DateTime.Now;
		}

		private void CloseSearch()
		{
			if (SearchIsOpen)
			{
				SearchIsOpen = false;
				InvokeIfNeeded(delegate ()
				{
					PanelSearch.Visible = false;
				});
			}
		}

		private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e) => txtIn.FoldAll(FoldAction.Contract);

		private void CopyFullCode_Click(object sender, EventArgs e)
		{
			try
			{
				if (fullCode != "")
					Clipboard.SetText(fullCode);
				else
					Clipboard.SetText(txtOut.Text);
			}
			catch (Exception ex)
			{
				_ = MessageBox.Show($"Copying code failed: {ex.Message}", "Keyview", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			txtIn.Copy();
			lastKeyTime = DateTime.Now;
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			txtIn.Cut();
			lastKeyTime = DateTime.Now;
		}

		private void expandAllToolStripMenuItem_Click(object sender, EventArgs e) => txtIn.FoldAll(FoldAction.Expand);

		private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e) => OpenReplaceDialog();

		private void findDialogToolStripMenuItem_Click(object sender, EventArgs e) => OpenFindDialog();

		private void findToolStripMenuItem_Click(object sender, EventArgs e) => OpenSearch();

		private void GenerateKeystrokes(string keys)
		{
			HotKeyManager.Enable = false;
			_ = txtIn.Focus();
			SendKeys.Send(keys);
			HotKeyManager.Enable = true;
		}

		private void hiddenCharactersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			hiddenCharactersItem.Checked = !hiddenCharactersItem.Checked;
			txtIn.ViewWhitespace = hiddenCharactersItem.Checked ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
		}

		private void Indent()
		{
			//We use this hack to send "Shift+Tab" to scintilla, since there is no known API to indent,
			//although the indentation function exists. Pressing TAB with the editor focused confirms this.
			GenerateKeystrokes("{TAB}");
			lastKeyTime = DateTime.Now;
		}

		private void indentGuidesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			indentGuidesItem.Checked = !indentGuidesItem.Checked;
			txtIn.IndentationGuides = indentGuidesItem.Checked ? IndentView.LookBoth : IndentView.None;
		}

		private void indentSelectionToolStripMenuItem_Click(object sender, EventArgs e) => Indent();

		//private void InitBookmarkMargin()
		//{
		//  //TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
		//  var margin = txtIn.Margins[BOOKMARK_MARGIN];
		//  margin.Width = 20;
		//  margin.Sensitive = true;
		//  margin.Type = MarginType.Symbol;
		//  margin.Mask = 1 << BOOKMARK_MARKER;
		//  //margin.Cursor = MarginCursor.Arrow;
		//  var marker = txtIn.Markers[BOOKMARK_MARKER];
		//  marker.Symbol = MarkerSymbol.Circle;
		//  marker.SetBackColor(IntToColor(0xFF003B));
		//  marker.SetForeColor(IntToColor(0x000000));
		//  marker.SetAlpha(100);
		//}
		private void InitCodeFolding(Scintilla txt)
		{
			txt.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
			txt.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));
			//Enable code folding.
			txt.SetProperty("fold", "1");
			txt.SetProperty("fold.compact", "1");
			// Configure a margin to display folding symbols
			txt.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
			txt.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
			txt.Margins[FOLDING_MARGIN].Sensitive = true;
			txt.Margins[FOLDING_MARGIN].Width = 20;

			//Set colors for all folding markers.
			for (int i = 25; i <= 31; i++)
			{
				txt.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
				txt.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
			}

			//Configure folding markers with respective symbols.
			txt.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
			txt.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
			txt.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
			txt.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
			txt.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
			txt.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
			txt.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
			//Enable automatic folding.
			txt.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;
		}

		private void InitColors(Scintilla txt) => txt.CaretForeColor = Color.Black;

		private void InitDragDropFile()
		{
			txtIn.AllowDrop = true;
			txtIn.DragEnter += TxtIn_DragEnter;
			txtIn.DragDrop += TxtIn_DragDrop;
		}

		private void InitHotkeys()
		{
			// register the hotkeys with the form
			HotKeyManager.AddHotKey(this, OpenSearch, Keys.F, true);
			HotKeyManager.AddHotKey(this, OpenFindDialog, Keys.F, true, false, true);
			HotKeyManager.AddHotKey(this, OpenReplaceDialog, Keys.R, true);
			HotKeyManager.AddHotKey(this, OpenReplaceDialog, Keys.H, true);
			HotKeyManager.AddHotKey(this, Uppercase, Keys.U, true);
			HotKeyManager.AddHotKey(this, Lowercase, Keys.L, true);
			HotKeyManager.AddHotKey(this, ZoomIn, Keys.Oemplus, true);
			HotKeyManager.AddHotKey(this, ZoomOut, Keys.OemMinus, true);
			HotKeyManager.AddHotKey(this, ZoomDefault, Keys.D0, true);
			HotKeyManager.AddHotKey(this, CloseSearch, Keys.Escape);
			HotKeyManager.AddHotKey(this, RunStopScript, Keys.F9);
			//Remove conflicting hotkeys from scintilla.
			txtIn.ClearCmdKey(Keys.Control | Keys.F);
			txtIn.ClearCmdKey(Keys.Control | Keys.R);
			txtIn.ClearCmdKey(Keys.Control | Keys.H);
			txtIn.ClearCmdKey(Keys.Control | Keys.L);
			txtIn.ClearCmdKey(Keys.Control | Keys.U);
		}

		private void InitNumberMargin(Scintilla txt)
		{
			txt.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
			txt.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
			txt.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
			txt.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);
			var nums = txt.Margins[NUMBER_MARGIN];
			nums.Width = 30;
			nums.Type = MarginType.Number;
			nums.Sensitive = true;
			nums.Mask = 0;
			txt.MarginClick += txtIn_MarginClick;
		}

		private void InitSyntaxColoring(Scintilla txt)
		{
			//Configure the default style.
			txt.StyleResetDefault();
			txt.Styles[Style.Default].Font = "Consolas";
			txt.Styles[Style.Default].Size = 10;
			//txt.Styles[Style.Default].BackColor = IntToColor(0xFFFCE1);
			//txt.Styles[Style.Default].BackColor = IntToColor(0x212121);
			//txt.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
			txt.StyleClearAll();
			var orig = false;

			if (orig)
			{
				txt.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
				txt.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
				txt.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
				txt.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
				txt.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
				txt.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
				txt.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
				txt.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
				txt.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
				txt.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
				txt.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
				txt.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
				txt.SelectionBackColor = IntToColor(0x114D9C);
			}
			else
			{
				txt.Styles[Style.Cpp.Default].ForeColor = Color.Black;
				txt.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
				txt.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
				txt.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(0, 128, 0); // Green
				txt.Styles[Style.Cpp.Number].ForeColor = Color.DarkOliveGreen;
				txt.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
				txt.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
				txt.Styles[Style.Cpp.Preprocessor].ForeColor = Color.FromArgb(128, 128, 128); // Gray
				txt.Styles[Style.Cpp.Operator].ForeColor = Color.FromArgb(0, 0, 120); // Dark Blue
				txt.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
				txt.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
				txt.Styles[Style.Cpp.Word2].ForeColor = Color.FromArgb(52, 146, 184); // Turqoise
				txt.SelectionBackColor = Color.FromArgb(153, 201, 239);
			}

			//Extras.
			txt.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
			txt.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
			txt.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);
			txt.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
			txt.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
			txt.LexerName = "cpp";
			//txt.LexerName = "";
			txt.SetKeywords(0, keywords1);
			txt.SetKeywords(1, keywords2);
		}

		private void InvokeIfNeeded(Action action)
		{
			if (InvokeRequired)
			{
				_ = BeginInvoke(action);
			}
			else
			{
				action.Invoke();
			}
		}

		private void Keyview_FormClosing(object sender, FormClosingEventArgs e)
		{
			timer.Stop();
			var dir = Path.GetDirectoryName(lastrun);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			File.WriteAllText(lastrun, txtIn.Text);
		}

		private void Keyview_Load(object sender, EventArgs e)
		{
			InitColors(txtIn);
			InitColors(txtOut);
			InitSyntaxColoring(txtIn);//Keysharp syntax for txtIn.
			//InitSyntaxColoring(txtOut);
			txtOut.StyleResetDefault();
			txtOut.Styles[Style.Default].Font = "Consolas";
			txtOut.Styles[Style.Default].Size = 10;
			//txt.Styles[Style.Default].BackColor = IntToColor(0xFFFCE1);
			//txt.Styles[Style.Default].BackColor = IntToColor(0x212121);
			//txt.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
			txtOut.StyleClearAll();
			csStyler.ApplyStyle(txtOut);//C# syntax for txtOut.
			csStyler.SetKeywords(txtOut);
			InitNumberMargin(txtIn);
			InitNumberMargin(txtOut);
			//InitBookmarkMargin();
			InitCodeFolding(txtIn);
			InitCodeFolding(txtOut);
			InitDragDropFile();
			InitHotkeys();
			//txtIn.StyleNeeded += TxtIn_StyleNeeded;

			if (File.Exists(lastrun))
				txtIn.Text = File.ReadAllText(lastrun);

			timer.Interval = 1000;
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Keyview_ResizeEnd(object sender, EventArgs e) => splitContainer.SplitterDistance = Width / 2;

		private void LoadDataFromFile(string path)
		{
			if (File.Exists(path))
			{
				FileName.Text = Path.GetFileName(path);
				txtIn.Text = File.ReadAllText(path);
				lastKeyTime = DateTime.Now;
			}
		}

		private void Lowercase()
		{
			var start = txtIn.SelectionStart;
			var end = txtIn.SelectionEnd;
			txtIn.ReplaceSelection(txtIn.GetTextRange(start, end - start).ToLower());
			txtIn.SetSelection(start, end);
			lastKeyTime = DateTime.Now;
		}

		private void lowercaseSelectionToolStripMenuItem_Click(object sender, EventArgs e) => Lowercase();

		private void OpenFindDialog()
		{
		}

		private void OpenReplaceDialog()
		{
		}

		private void OpenSearch()
		{
			SearchManager.SearchBox = TxtSearch;
			SearchManager.TextArea = txtIn;

			if (!SearchIsOpen)
			{
				SearchIsOpen = true;
				InvokeIfNeeded(delegate ()
				{
					PanelSearch.Visible = true;
					TxtSearch.Text = SearchManager.LastSearch;
					_ = TxtSearch.Focus();
					TxtSearch.SelectAll();
				});
			}
			else
			{
				InvokeIfNeeded(delegate ()
				{
					_ = TxtSearch.Focus();
					TxtSearch.SelectAll();
				});
			}
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				LoadDataFromFile(openFileDialog.FileName);
			}
		}

		private void Outdent()
		{
			// we use this hack to send "Shift+Tab" to scintilla, since there is no known API to outdent,
			// although the indentation function exists. Pressing Shift+Tab with the editor focused confirms this.
			GenerateKeystrokes("+{TAB}");
			lastKeyTime = DateTime.Now;
		}

		private void outdentSelectionToolStripMenuItem_Click(object sender, EventArgs e) => Outdent();

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			txtIn.Paste();
			lastKeyTime = DateTime.Now;
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) => txtIn.SelectAll();

		private void selectLineToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Line line = txtIn.Lines[txtIn.CurrentLine];
			txtIn.SetSelection(line.Position + line.Length, line.Position);
		}

		private void SetFailure()
		{
			tslCodeStatus.ForeColor = Color.Red;
			tslCodeStatus.Text = "Error";
			SetTxtOut("");
			Refresh();
		}

		private void SetStart()
		{
			fullCode = trimmedCode = "";
			tslCodeStatus.ForeColor = Color.Black;
			tslCodeStatus.Text = "";
			//Don't clear txtOut, it causes flicker.
			Refresh();
		}

		private void SetSuccess(double seconds)
		{
			tslCodeStatus.ForeColor = Color.Green;
			tslCodeStatus.Text = $"Ok ({seconds:F1}s)";
			Refresh();
		}

		private void SetTxtOut(string txt)
		{
			txtOut.ReadOnly = false;
			txtOut.Text = txt;
			txtOut.ReadOnly = true;
		}

		private void splitContainer_DoubleClick(object sender, EventArgs e) => splitContainer.SplitterDistance = Width / 2;

		//private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
		//{
		//  if (e.Margin == BOOKMARK_MARGIN)
		//  {
		//      // Do we have a marker for this line?
		//      const uint mask = 1 << BOOKMARK_MARKER;
		//      var line = txtIn.Lines[txtIn.LineFromPosition(e.Position)];

		//      if ((line.MarkerGet() & mask) > 0)
		//      {
		//          // Remove existing bookmark
		//          line.MarkerDelete(BOOKMARK_MARKER);
		//      }
		//      else
		//      {
		//          // Add bookmark
		//          line.MarkerAdd(BOOKMARK_MARKER);
		//      }
		//  }
		//}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if ((force || ((DateTime.Now - lastKeyTime).TotalSeconds >= updateFreqSeconds && lastKeyTime > lastCompileTime)) && txtIn.Text != "")
			{
				timer.Enabled = false;

				try
				{
					lastCompileTime = DateTime.Now;
					CompilerHelper.compiledasm = null;
					btnRunScript.Enabled = false;
					var oldIndex = txtOut.FirstVisibleLine;
					SetStart();
					tslCodeStatus.Text = "Creating DOM from script...";
					Refresh();
					var (domunits, domerrs) = ch.CreateDomFromFile([txtIn.Text]);

					if (domerrs.HasErrors)
					{
						var (errors, warnings) = CompilerHelper.GetCompilerErrors(domerrs);
						SetFailure();
						var txt = "Error creating DOM from script.";

						if (errors.Length > 0)
							txt += $"\n\n{errors}";

						if (warnings.Length > 0)
							txt += $"\n\n{warnings}";

						SetTxtOut(txt);
						goto theend;
					}

					tslCodeStatus.Text = "Creating C# code from DOM...";
					Refresh();
					var (code, exc) = ch.CreateCodeFromDom(domunits);

					if (exc is Exception ex)
					{
						SetFailure();
						SetTxtOut($"Error creating C# code from DOM:\n{ex.Message}");
						goto theend;
					}

					code = CompilerHelper.UsingStr + code;
					tslCodeStatus.Text = "Compiling C# code...";
					var asm = Assembly.GetExecutingAssembly();
					var (results, ms, compileexc) = ch.Compile(code, "Keyview", Path.GetFullPath(Path.GetDirectoryName(asm.Location)));

					if (results == null)
					{
						SetFailure();
						SetTxtOut($"Error compiling C# code to executable: {(compileexc != null ? compileexc.Message : string.Empty)}\n\n{code}");
					}
					else if (results.Success)
					{
						SetSuccess((DateTime.Now - lastCompileTime).TotalSeconds);
						fullCode = code;
						var token = "[System.STAThreadAttribute()]";
						var start = code.IndexOf(token);
						code = code.AsSpan(start + token.Length + 2).TrimEnd(trimend).ToString();
						var sb = new StringBuilder(code.Length);

						foreach (var line in code.SplitLines())
							_ = sb.AppendLine(line.TrimNofAnyFromStart(trimstr, 2));

						trimmedCode = sb.ToString().TrimEnd(trimend);
						SetTxtOut(chkFullCode.Checked ? fullCode : trimmedCode);
						txtOut.FirstVisibleLine = oldIndex;
						File.WriteAllText(lastrun, txtIn.Text);
						ms.Seek(0, SeekOrigin.Begin);
						var arr = ms.ToArray();
						CompilerHelper.compiledBytes = arr;
						CompilerHelper.compiledasm = Assembly.Load(arr);
					}
					else
					{
						SetFailure();
						SetTxtOut(CompilerHelper.HandleCompilerErrors(results.Diagnostics, "Keyview", "Compiling C# code to executable", compileexc != null ? compileexc.Message : string.Empty) + "\n" + code);
					}
				}
				catch
				{
				}

				theend:
				btnRunScript.Enabled = true;
				timer.Enabled = true;
			}

			if (force)
				force = false;
		}

		private void RunScript_Click(object sender, EventArgs e) => RunStopScript();

		private void RunStopScript()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			if (scriptProcess != null)
			{
				scriptProcess.Kill();
				scriptProcess = null;
			}

			if (btnRunScript.Text == btnRunScriptText["Stop"])
				return;

			if (CompilerHelper.compiledasm == null)
			{
				MessageBox.Show("Please wait, code is still compiling...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			scriptProcess = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "Keysharp.exe",
					Arguments = "--assembly *",
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			scriptProcess.EnableRaisingEvents = true;
			scriptProcess.Exited += (object sender, EventArgs e) =>
			{
				toolStrip1.Invoke((() =>
				{
					btnRunScript.Text = btnRunScriptText["Run"];
				}));
				scriptProcess = null;
			};
			scriptProcess.Start();

			using (var writer = new BinaryWriter(scriptProcess.StandardInput.BaseStream))
			{
				writer.Write(CompilerHelper.compiledBytes.Length);
				writer.Write(CompilerHelper.compiledBytes);
				writer.Flush();
			}

			btnRunScript.Text = btnRunScriptText["Stop"];
		}


		private void TxtIn_DragDrop(object sender, DragEventArgs e)
		{
			var data = e.Data.GetData(DataFormats.FileDrop);

			if (data is string[] filenames)
			{
				try
				{
					if (filenames.Length > 0)
						txtIn.Text = File.ReadAllText(filenames[0]);
				}
				catch (Exception ex)
				{
					_ = Dialogs.MsgBox($"Unable to load file: {ex.Message}");
				}
			}
		}

		private void TxtIn_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}

		private void txtIn_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
				force = true;
			else if (ReferenceEquals(sender, txtIn))
				lastKeyTime = DateTime.Now;
		}

		private void txtIn_MarginClick(object sender, MarginClickEventArgs e)
		{
			var txt = sender as Scintilla;

			if (e.Margin == BOOKMARK_MARGIN)
			{
				// Do we have a marker for this line?
				const uint mask = 1 << BOOKMARK_MARKER;
				var line = txt.Lines[txt.LineFromPosition(e.Position)];

				if ((line.MarkerGet() & mask) > 0)
				{
					// Remove existing bookmark
					line.MarkerDelete(BOOKMARK_MARKER);
				}
				else
				{
					// Add bookmark
					_ = line.MarkerAdd(BOOKMARK_MARKER);
				}
			}
		}

		private void txtIn_TextChanged(object sender, EventArgs e) => lastKeyTime = DateTime.Now;

		private void txtOut_KeyDown(object sender, KeyEventArgs e) => txtIn_KeyDown(sender, e);

		private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (HotKeyManager.IsHotkey(e, Keys.Enter))
			{
				SearchManager.Find(true, false);
			}

			if (HotKeyManager.IsHotkey(e, Keys.Enter, true) || HotKeyManager.IsHotkey(e, Keys.Enter, false, true))
			{
				SearchManager.Find(false, false);
			}
		}

		private void TxtSearch_TextChanged(object sender, EventArgs e) => SearchManager.Find(true, true);

		private void Uppercase()
		{
			var start = txtIn.SelectionStart;
			var end = txtIn.SelectionEnd;
			txtIn.ReplaceSelection(txtIn.GetTextRange(start, end - start).ToUpper());
			txtIn.SetSelection(start, end);
			lastKeyTime = DateTime.Now;
		}

		private void uppercaseSelectionToolStripMenuItem_Click(object sender, EventArgs e) => Uppercase();

		private void wordWrapToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			wordWrapItem.Checked = !wordWrapItem.Checked;
			txtOut.WrapMode = txtIn.WrapMode = wordWrapItem.Checked ? WrapMode.Word : WrapMode.None;
		}

		private void zoom100ToolStripMenuItem_Click(object sender, EventArgs e) => ZoomDefault();

		private void ZoomDefault() => txtIn.Zoom = 0;

		private void ZoomIn() => txtIn.ZoomIn();

		private void zoomInToolStripMenuItem_Click(object sender, EventArgs e) => ZoomIn();

		private void ZoomOut() => txtIn.ZoomOut();

		private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e) => ZoomOut();
	}
}