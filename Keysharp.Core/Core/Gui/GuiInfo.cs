using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Keysharp.Core
{
	public class GuiInfo
	{
		public Stack<Control> Controls { get; set; }
		public Stack<Control> Containers { get; set; }

		public char Delimiter { get; set; }

		public Font Font { get; set; }

		public Control LastControl
		{
			get => Controls.TryPeek(out var c) ? c : null;
			set => Controls.Push(value);
		}

		public Control LastContainer
		{
			get => Containers.TryPeek(out var c) ? c : null;
			set => Containers.Push(value);
		}

		public Point Section { get; set; }

		public StatusStrip StatusBar { get; set; }

		public GuiInfo()
		{
			Controls = new Stack<Control>();
			Containers = new Stack<Control>();
		}

		public (Control, Control) RightBottomMost()
		{
			var maxx = 0;
			var maxy = 0;
			(Control right, Control bottom) p = (null, null);

			foreach (var ctrl in Controls)
			{
				var temp = ctrl.Location.X + ctrl.Width;

				if (temp > maxx)
				{
					maxx = temp;
					p.right = ctrl;
				}

				temp = ctrl.Location.Y + ctrl.Height;

				if (temp > maxy)
				{
					maxy = temp;
					p.bottom = ctrl;
				}
			}

			return p;
		}
	}
}