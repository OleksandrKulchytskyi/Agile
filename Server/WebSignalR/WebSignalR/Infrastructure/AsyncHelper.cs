using Microsoft.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;

namespace WebSignalR.Infrastructure
{
	/// <summary>
	/// Provides support for asynchronous lazy initialization.
	/// </summary>
	/// <remarks>
	/// Based on Stephen Toub's article:
	/// http://blogs.msdn.com/b/pfxteam/archive/2011/01/15/10116210.aspx
	/// </remarks>
	/// <typeparam name="T">
	/// The type of object that is being initialized.
	/// </typeparam>
	internal sealed class AsyncLazy<T> : Lazy<Task<T>>
	{
		/// <summary>
		/// Initializes a new instance of the AsyncLazy class.
		/// </summary>
		/// <param name="valueFactory">
		/// The delegate that is invoked to produce the lazily initialized
		/// value when it is needed.
		/// </param>
		public AsyncLazy(Func<T> valueFactory) :
			base(() => Task.Factory.StartNew(valueFactory)) { }

		/// <summary>
		/// Initializes a new instance of the AsyncLazy class.
		/// </summary>
		/// <param name="taskFactory">
		/// The asynchronous delegate that is invoked to produce the lazily
		/// initialized value when it is needed.
		/// </param>
		public AsyncLazy(Func<Task<T>> taskFactory) :
			base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap()) { }

		/// <summary>
		/// Returns the awaiter used to await the lazy initialized value.
		/// </summary>
		/// <returns>An awaiter instance.</returns>
		public TaskAwaiter<T> GetAwaiter()
		{
			return this.Value.GetAwaiter();
		}
	}
}