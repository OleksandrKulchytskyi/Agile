using System;
using System.Collections.Generic;
using System.Linq;

namespace WebSignalR.Common.Extension
{
	public static class Ensure
	{
		public static void Contains<T>(IEnumerable<T> collection, Func<T, bool> predicate, string message = "")
		{
			That((collection != null) && collection.Any<T>(predicate), message);
		}

		public static void Equal<T>(T left, T right, string message = "Values must be equal")
		{
			That(((left != null) && (right != null)) && left.Equals(right), message);
		}

		public static void Items<T>(IEnumerable<T> collection, Func<T, bool> predicate, string message = "")
		{
			That((collection != null) && !collection.Any<T>(x => !predicate(x)), "");
		}

		public static void Not(bool condition, string message = "")
		{
			Not<Exception>(condition, message);
		}

		public static void Not<TException>(bool condition, string message = "") where TException : Exception
		{
			That<TException>(!condition, message);
		}

		public static void NotEqual<T>(T left, T right, string message = "Values must not be equal")
		{
			That(((left != null) && (right != null)) && !left.Equals(right), message);
		}

		public static void NotNull(object value, string message = "")
		{
			That<NullReferenceException>(value != null, message);
		}

		public static void NotNullOrEmpty(string value, string message = "String cannot be null or empty")
		{
			That(value.IsNotNullOrEmpty(), message);
		}

		public static void That(bool condition, string message = "")
		{
			That<Exception>(condition, message);
		}

		public static void That<TException>(bool condition, string message = "") where TException : Exception
		{
			if (!condition)
			{
				throw ((TException)Activator.CreateInstance(typeof(TException), new object[] { message }));
			}
		}

		public static class Argument
		{
			public static void Is(bool condition, string message = "")
			{
				Ensure.That<ArgumentException>(condition, message);
			}

			public static void IsNot(bool condition, string message = "")
			{
				Is(!condition, message);
			}

			public static void NotNull(object value, string paramName = "")
			{
				Ensure.That<ArgumentNullException>(value != null, paramName);
			}

			public static void NotNullOrEmpty(string value, string paramName = "")
			{
				if (value.IsNullOrEmpty())
				{
					if (paramName.IsNullOrEmpty())
					{
						throw new ArgumentException("String value cannot be empty");
					}
					throw new ArgumentException("String parameter " + paramName + " cannot be null or empty", paramName);
				}
			}
		}
	}
}