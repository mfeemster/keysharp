namespace Keysharp.Core.Common.Window
{
	internal class SearchCriteria
	{
		internal bool Active { get; private set; }
		internal string ClassName { get; set; }
		internal string ExcludeText { get; set; }
		internal string ExcludeTitle { get; set; }
		internal string Group { get; set; }
		internal bool HasExcludes => !string.IsNullOrEmpty(ExcludeTitle) || !string.IsNullOrEmpty(ExcludeText);
		internal bool HasID => ID != IntPtr.Zero || PID != IntPtr.Zero;
		internal IntPtr ID { get; set; }
		internal bool IsEmpty => !HasID&& !HasExcludes&& string.IsNullOrEmpty(Group)&& string.IsNullOrEmpty(Title)&& string.IsNullOrEmpty(Text)&& string.IsNullOrEmpty(ClassName)&& string.IsNullOrEmpty(Path);
		internal string Path { get; set; }
		internal long PID { get; set; }
		internal string Text { get; set; }
		internal string Title { get; set; }

		internal static SearchCriteria FromString(object obj)
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
				hwnd = Script.GetPropertyValue(obj, "Hwnd", false);
			}
			catch
			{
			}

			if (hwnd != null)
			{
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
			}

			var mixed = obj.ToString();

			if (!mixed.Contains(Keyword_ahk, StringComparison.OrdinalIgnoreCase))
				return new SearchCriteria { Title = mixed };

			var i = 0;

			var t = false;

			while ((i = mixed.IndexOf(Keyword_ahk, i, StringComparison.OrdinalIgnoreCase)) != -1)
			{
				if (!t)
				{
					var pre = i == 0 ? string.Empty : mixed.Substring(0, i).Trim(Spaces);

					if (pre.Length != 0)
						criteria.Title = pre;

					t = true;
				}

				var z = mixed.IndexOfAny(Spaces, i);

				if (z == -1)
					break;

				var word = mixed.Substring(i, z - i);
				var e = mixed.IndexOf(Keyword_ahk, ++i, StringComparison.OrdinalIgnoreCase);
				var arg = (e == -1 ? mixed.Substring(z) : mixed.Substring(z, e - z)).Trim();
				long n;

				switch (word.ToLowerInvariant())
				{
					case Keyword_ahk_class: criteria.ClassName = arg; break;

					case Keyword_ahk_group: criteria.Group = arg; break;

					case Keyword_ahk_id:
						if (long.TryParse(arg, out n))
							criteria.ID = new IntPtr(n);

						break;

					case Keyword_ahk_exe:
						criteria.Path = arg;
						break;

					case Keyword_A:
						criteria.Active = true;
						break;

					case Keyword_ahk_pid:
						if (long.TryParse(arg, out n))
							criteria.PID = new IntPtr(n);

						break;
				}

				i++;
			}

			return criteria;
		}

		internal static SearchCriteria FromString(object title, object text, object excludeTitle, object excludeText)
		{
			var criteria = FromString(title);
			criteria.Text = text.As();
			criteria.ExcludeTitle = excludeTitle.As();
			criteria.ExcludeText = excludeText.As();
			return criteria;
		}
	}
}