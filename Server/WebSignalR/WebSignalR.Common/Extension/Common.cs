using System;
using System.Text;
using System.Text.RegularExpressions;

namespace WebSignalR.Common.Extension
{
	public static class Common
	{
		public static void DisposeExt<T>(this T context)
		{
			if (context != null && context is IDisposable)
			{
				(context as IDisposable).Dispose();
			}
		}

		public static string toBase64Utf8(this string strOriginal)
		{
			byte[] byt = System.Text.Encoding.UTF8.GetBytes(strOriginal);

			// convert the byte array to a Base64 string
			return Convert.ToBase64String(byt);
		}

		public static T CastTo<T>(this Object value, T targetType)
		{
			// targetType above is just for compiler magic
			// to infer the type to cast x to
			return (T)value;
		}

		public static bool IsPositiveInt(string id)
		{
			var positiveIntRegex = new Regex(@"^0*[1-9][0-9]*$");
			return positiveIntRegex.IsMatch(id);
		}

		public static StringBuilder AppendMessage(this StringBuilder sb, string text, Func<string, bool> predicate = null)
		{
			if (predicate != null)
			{
				if (predicate(text))
				{
					sb.Append("  ");
					sb.Append(text);
				}
			}
			else
			{
				sb.Append("  ");
				sb.Append(text);
			}

			return sb;
		}

		public static Func<string, bool> notEmpty = (text) => !string.IsNullOrWhiteSpace(text);
	}
}