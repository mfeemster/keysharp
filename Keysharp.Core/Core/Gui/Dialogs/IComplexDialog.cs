using System;

namespace Keysharp.Core
{
	public interface IComplexDialog
	{
		bool InvokeRequired { get; }
		string MainText { get; set; }
		string SubText { get; set; }
		string Title { get; set; }

		bool TopMost { get; set; }

		bool Visible { get; set; }

		void Close();

		void Dispose();

		object Invoke(Delegate Method, params object[] obj);

		object Invoke(Delegate Method);

		//DialogResult ShowDialog();
		void Show();
	}
}