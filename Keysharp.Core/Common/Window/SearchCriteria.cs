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

			while (i < mixed.Length && (i = mixed.IndexOf(Keyword_ahk, i, StringComparison.OrdinalIgnoreCase)) != -1)
			{
				if (!t)
				{
					var pre = i == 0 ? string.Empty : mixed.Substring(0, i).Trim(Spaces);

					if (pre.Length != 0)
						criteria.Title = pre;

					t = true;
				}

				var span = mixed.AsSpan(i + Keyword_ahk.Length);

				if (span.StartsWith("class", StringComparison.OrdinalIgnoreCase))
				{
					criteria.ClassName = span.Slice(5).TrimStart().ParseUntilSpace();
					i += 9 + criteria.ClassName.Length;//Might be off by a space, but still reduces the next comparison.
				}
				else if (span.StartsWith("group", StringComparison.OrdinalIgnoreCase))
				{
					criteria.Group = span.Slice(5).TrimStart().ParseUntilSpace();
					i += 9 + criteria.Group.Length;
				}
				else if (span.StartsWith("exe", StringComparison.OrdinalIgnoreCase))
				{
					criteria.Path = span.Slice(3).TrimStart().ParseUntilSpace();
					i += 7 + criteria.Path.Length;
				}
				else if (span.StartsWith("id", StringComparison.OrdinalIgnoreCase))
				{
					var val = span.Slice(2).TrimStart().ParseUntilSpace();

					if (long.TryParse(val, out var id))
						criteria.ID = new IntPtr(id);

					i += 6 + val.Length;
				}
				else if (span.StartsWith("A", StringComparison.OrdinalIgnoreCase))
				{
					criteria.Active = true;
					i += 5;
				}
				else if (span.StartsWith("pid", StringComparison.OrdinalIgnoreCase))
				{
					var val = span.Slice(3).TrimStart().ParseUntilSpace();

					if (long.TryParse(val, out var id))
						criteria.PID = new IntPtr(id);

					i += 7 + val.Length;
				}
				else
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