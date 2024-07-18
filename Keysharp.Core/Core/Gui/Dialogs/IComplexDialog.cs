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

		object Invoke(Delegate method, params object[] obj);

		object Invoke(Delegate method);

		//DialogResult ShowDialog();
		void Show();
	}
}