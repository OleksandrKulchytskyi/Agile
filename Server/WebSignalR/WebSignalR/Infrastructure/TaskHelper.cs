using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace WebSignalR.Infrastructure
{
	public static class TaskHelper
	{
		private static readonly Task _emptyTask = MakeEmpty();


		private static Task MakeEmpty()
		{
			return FromResult<object>(null);
		}

		public static Task Empty
		{
			get
			{
				return _emptyTask;
			}
		}

		public static Task<T> FromResult<T>(T value)
		{
			var tcs = new TaskCompletionSource<T>();
			tcs.SetResult(value);
			return tcs.Task;
		}

		// <summary>
		// Returns an error task of the given type. The task is Completed, IsCanceled = False, IsFaulted = True
		// </summary>
		// <typeparam name="TResult"></typeparam>
		internal static Task<TResult> FromError<TResult>(Exception exception)
		{
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			tcs.SetException(exception);
			return tcs.Task;
		}

		internal static Task<TResult> FromCancelled<TResult>(TResult value)
		{
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			tcs.SetCanceled();
			return tcs.Task;
		}

		public static TTask Catch<TTask>(this TTask task) where TTask : Task
		{
			if (task != null && task.Status != TaskStatus.RanToCompletion)
			{
				task.ContinueWith(innerTask =>
				{
					var ex = innerTask.Exception;
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
			return task;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
		public static Task RunSynchronously(Action action, CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested)
				return FromCancelled<object>(null);

			try
			{
				action();
				return Empty;
			}
			catch (Exception e)
			{
				return FromError<object>(e);
			}
		}
	}
}