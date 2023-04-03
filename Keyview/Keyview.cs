using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Scripting;

namespace Keyview
{
	public partial class Keyview : Form
	{
		private readonly CompilerHelper ch = new CompilerHelper();
		private readonly CheckBox chkFullCode = new CheckBox();
		private readonly string lastrun = $"{Accessors.A_AppData}/Keysharp/lastkeyviewrun.txt";
		private readonly System.Windows.Forms.Timer timer = new Timer();
		private readonly char[] trimend = new char[] { '\n', '\r' };
		private readonly double updateFreqSeconds = 1;
		private bool force = false;
		private string fullCode = "";
		private DateTime lastCompileTime = DateTime.Now;
		private DateTime lastKeyTime = DateTime.Now;

		//private bool status = false;
		private string trimmedCode = "";

		private string trimstr = "{}\t";

		public Keyview()
		{
			InitializeComponent();
			Icon = Keysharp.Core.Properties.Resources.Keysharp_ico;
			chkFullCode.Text = "Full code";
			chkFullCode.CheckStateChanged += chkFullCode_CheckStateChanged;
			var host = new ToolStripControlHost(chkFullCode)
			{
				Alignment = ToolStripItemAlignment.Right
			};
			_ = toolStrip1.Items.Add(host);
			txtIn.AllowDrop = true;
			txtIn.DragEnter += TxtIn_DragEnter;
			txtIn.DragDrop += TxtIn_DragDrop;
			var tabList = new List<int>(32);
			var width = 50;

			for (var i = 1; i <= 32; i++)
				tabList.Add(i * width);

			txtIn.SelectionTabs = txtOut.SelectionTabs = tabList.ToArray();
		}

		private void chkFullCode_CheckStateChanged(object sender, EventArgs e) => txtOut.Text = chkFullCode.Checked ? fullCode : trimmedCode;

		private void Keyview_FormClosing(object sender, FormClosingEventArgs e)
		{
			timer.Stop();
			System.IO.File.WriteAllText(lastrun, txtIn.Text);
		}

		private void Keyview_Load(object sender, EventArgs e)
		{
			lines1.Font = lines2.Font = txtIn.Font;

			if (System.IO.File.Exists(lastrun))
				txtIn.Text = System.IO.File.ReadAllText(lastrun);

			timer.Interval = 1000;
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Keyview_ResizeEnd(object sender, EventArgs e) => splitContainer.SplitterDistance = Width / 2;

		private void SetFailure()
		{
			tslCodeStatus.ForeColor = Color.Red;
			tslCodeStatus.Text = "Error";
			txtOut.Text = "";
			Refresh();
		}

		private void SetStart()
		{
			fullCode = trimmedCode = "";
			tslCodeStatus.ForeColor = Color.Black;
			tslCodeStatus.Text = "";
			//Don't clear txtOut, it causs flicker.
			Refresh();
		}

		private void SetSuccess(double seconds)
		{
			tslCodeStatus.ForeColor = Color.Green;
			tslCodeStatus.Text = $"Ok ({seconds:F1}s)";
			Refresh();
		}

		private void splitContainer_DoubleClick(object sender, EventArgs e) => splitContainer.SplitterDistance = Width / 2;

		private void Timer_Tick(object sender, EventArgs e)
		{
			if ((force || ((DateTime.Now - lastKeyTime).TotalSeconds >= updateFreqSeconds && lastKeyTime > lastCompileTime)) && txtIn.Text != "")
			{
				lastCompileTime = DateTime.Now;
				var oldIndex = txtOut.GetCharIndexFromPosition(new Point(0, 0)) + 4;//Magic number, it scrolls backward on each update with smaller numbers.
				SetStart();
				tslCodeStatus.Text = "Creating DOM from script...";
				Refresh();
				var (domunits, domerrs) = ch.CreateDomFromFile(new string[] { txtIn.Text });

				if (domerrs.HasErrors)
				{
					var (errors, warnings) = CompilerHelper.GetCompilerErrors(domerrs);
					SetFailure();
					txtOut.Text = $"Error creating DOM from script:\n\n{errors}\n\n\n{warnings}";
					goto theend;
				}

				tslCodeStatus.Text = "Creating C# code from DOM...";
				Refresh();
				var (code, exc) = ch.CreateCodeFromDom(domunits);

				if (exc is Exception ex)
				{
					SetFailure();
					txtOut.Text = $"Error creating C# code from DOM:\n{ex.Message}";
					goto theend;
				}

				tslCodeStatus.Text = "Trimming parents from C# code...";
				Refresh();
				code = CompilerHelper.UsingStr + Keysharp.Scripting.Parser.TrimParens(code);//Need to manually add the using static statements.
				tslCodeStatus.Text = "Compiling C# code...";
				var asm = Assembly.GetExecutingAssembly();
				var (results, ms, compileexc) = ch.Compile(code, "Keyview", Path.GetFullPath(Path.GetDirectoryName(asm.Location)));

				if (results == null)
				{
					SetFailure();
					txtOut.Text = $"Error compiling C# code to executable: {(compileexc != null ? compileexc.Message : string.Empty)}\n\n{code}";
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
						_ = sb.AppendLine(line.TrimNofAny(trimstr, 2));

					trimmedCode = sb.ToString().TrimEnd(trimend);
					txtOut.Text = chkFullCode.Checked ? fullCode : trimmedCode;
					txtOut.Select(oldIndex, 0);
					txtOut.ScrollToCaret();
					System.IO.File.WriteAllText(lastrun, txtIn.Text);
				}
				else
				{
					SetFailure();
					txtOut.Text = CompilerHelper.HandleCompilerErrors(results.Diagnostics, "Keyview", "Compiling C# code to executable", compileexc != null ? compileexc.Message : string.Empty) + "\n\n" + code;
				}
			}

			theend:

			if (force)
				force = false;
		}

		private void TxtIn_DragDrop(object sender, DragEventArgs e)
		{
			var data = e.Data.GetData(DataFormats.FileDrop);

			if (data is string[] filenames)
			{
				try
				{
					if (filenames.Length > 0)
						txtIn.Text = System.IO.File.ReadAllText(filenames[0]);
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
			else
				lastKeyTime = DateTime.Now;
		}

		private void txtIn_TextChanged(object sender, EventArgs e) => lastKeyTime = DateTime.Now;
	}
}