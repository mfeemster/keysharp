using System;
using Keysharp.Scripting;

namespace Keysharp.Core.Common.Window
{
	public class SearchCriteria
	{
		public bool Active { get; private set; }
		public string ClassName { get; set; }
		public string ExcludeText { get; set; }
		public string ExcludeTitle { get; set; }
		public string Group { get; set; }
		public bool HasExcludes => !string.IsNullOrEmpty(ExcludeTitle) || !string.IsNullOrEmpty(ExcludeText);
		public bool HasID => ID != IntPtr.Zero || PID != IntPtr.Zero;
		public IntPtr ID { get; set; }
		public bool IsEmpty => !HasID&& !HasExcludes&& string.IsNullOrEmpty(Group)&& string.IsNullOrEmpty(Title)&& string.IsNullOrEmpty(Text)&& string.IsNullOrEmpty(ClassName)&& string.IsNullOrEmpty(Path);
		public string Path { get; set; }
		public IntPtr PID { get; set; }
		public string Text { get; set; }
		public string Title { get; set; }

		public static SearchCriteria FromString(object obj)
		{
			var criteria = new SearchCriteria();

			if (obj == null)
				return criteria;

			if (obj is long l)
			{
				criteria.ID = new IntPtr(l);
				return criteria;
			}

			if (obj is IntPtr ptr1)
			{
				criteria.ID = ptr1;
				return criteria;
			}

			object hwnd = null;

			try
			{
				hwnd = Script.GetPropertyValue(obj, "Hwnd");
			}
			catch
			{
			}

			if (hwnd is long ll)
			{
				criteria.ID = new IntPtr(ll);
				return criteria;
			}

			if (hwnd is IntPtr ptr2)
			{
				criteria.ID = ptr2;
				return criteria;
			}

			var mixed = obj as string;

			if (mixed.IndexOf(Core.Keyword_ahk, StringComparison.OrdinalIgnoreCase) == -1)
				return new SearchCriteria { Title = mixed };

			var i = 0;

			var t = false;

			while ((i = mixed.IndexOf(Core.Keyword_ahk, i, StringComparison.OrdinalIgnoreCase)) != -1)
			{
				if (!t)
				{
					var pre = i == 0 ? string.Empty : mixed.Substring(0, i).Trim(Core.Keyword_Spaces);

					if (pre.Length != 0)
						criteria.Title = pre;

					t = true;
				}

				var z = mixed.IndexOfAny(Core.Keyword_Spaces, i);

				if (z == -1)
					break;

				var word = mixed.Substring(i, z - i);
				var e = mixed.IndexOf(Core.Keyword_ahk, ++i, StringComparison.OrdinalIgnoreCase);
				var arg = (e == -1 ? mixed.Substring(z) : mixed.Substring(z, e - z)).Trim();
				long n;

				switch (word.ToLowerInvariant())
				{
					case Core.Keyword_ahk_class: criteria.ClassName = arg; break;

					case Core.Keyword_ahk_group: criteria.Group = arg; break;

					case Core.Keyword_ahk_id:
						if (long.TryParse(arg, out n))
							criteria.ID = new IntPtr(n);

						break;

					case Core.Keyword_ahk_exe:
						criteria.Path = arg;
						break;

					case Core.Keyword_A:
						criteria.Active = true;
						break;

					case Core.Keyword_ahk_pid:
						if (long.TryParse(arg, out n))
							criteria.PID = new IntPtr(n);

						break;
				}

				i++;
			}

			return criteria;
		}

		internal static SearchCriteria FromString(object title, string text, string excludeTitle, string excludeText)
		{
			var criteria = FromString(title);
			criteria.Text = text;
			criteria.ExcludeTitle = excludeTitle;
			criteria.ExcludeText = excludeText;
			return criteria;
		}
	}
}