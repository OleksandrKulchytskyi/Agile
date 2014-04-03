using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Extension
{
	public static class TypeExtensions
	{
		public static bool Implements<T>(this Type type)
		{
			Ensure.Argument.NotNull(type, "type");
			return typeof(T).IsAssignableFrom(type);
		}
	}

}