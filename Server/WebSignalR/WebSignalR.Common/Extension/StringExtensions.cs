namespace WebSignalR.Common.Extension
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.RegularExpressions;

	public static class StringExtensions
	{
		public static bool Contains(this string source, string input, StringComparison comparison)
		{
			return (source.IndexOf(input, comparison) >= 0);
		}

		public static string FormatWith(this string format, params object[] args)
		{
			return string.Format(format, args);
		}

		private static string GenerateSlug(string value, int? maxLength = new int?())
		{
			string input = Regex.Replace(Regex.Replace(RemoveAccent(value).Replace("-", " ").ToLowerInvariant(), @"[^a-z0-9\s-]", string.Empty), @"\s+", " ").Trim();
			if (maxLength.HasValue)
			{
				int length = input.Length;
				input = input.Substring(0, (length <= maxLength) ? input.Length : maxLength.Value).Trim();
			}
			return Regex.Replace(input, @"\s", "-");
		}

		public static bool IsNotNullOrEmpty(this string value)
		{
			return !value.IsNullOrEmpty();
		}

		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static string Limit(this string source, int maxLength, string suffix = null)
		{
			if (suffix.IsNotNullOrEmpty())
			{
				maxLength -= suffix.Length;
			}
			if (source.Length <= maxLength)
			{
				return source;
			}
			return (source.Substring(0, maxLength).Trim() + (suffix ?? string.Empty));
		}

		public static string NullIfEmpty(this string value)
		{
			if (value == string.Empty)
			{
				return null;
			}
			return value;
		}

		private static string RemoveAccent(string value)
		{
			byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);
			return Encoding.ASCII.GetString(bytes);
		}

		public static string SeparatePascalCase(this string value)
		{
			Ensure.Argument.NotNullOrEmpty(value, "value");
			return Regex.Replace(value, "([A-Z])", " $1").Trim();
		}

		public static IEnumerable<string> SplitAndTrim(this string value, params char[] separators)
		{
			Ensure.Argument.NotNull(value, "source");
			return (from s in value.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries) select s.Trim());
		}
	}
}