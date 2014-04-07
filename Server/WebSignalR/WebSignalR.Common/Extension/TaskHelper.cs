using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace WebSignalR.Common.Extension
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

		public static Task FromAction<T>(Action<T> action, T input)
		{
			return Task.Factory.StartNew(() => action(input));
		}

		// Asynchronous NON-BLOCKING method
		public static Task Delay(double milliseconds)
		{
			var tcs = new TaskCompletionSource<bool>();
			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Elapsed += (obj, args) =>
			{
				tcs.TrySetResult(true);
			};
			timer.Interval = milliseconds;
			timer.AutoReset = false;
			timer.Start();
			return tcs.Task;
		}

		// Asynchronous NON-BLOCKING method
		public static Task Delay(TimeSpan delay)
		{
			var tcs = new TaskCompletionSource<object>();
			new Timer(_ => tcs.SetResult(null)).Change((int)delay.TotalMilliseconds, -1);
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

		public static Task<T> HandleContinuation<T>(this Task<T> mainRunner)
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();



			return tcs.Task;
		}

		public static Task ContinueWithEx(this Task task, Action<Task> continuation)
		{
			var tcs = new TaskCompletionSource<object>();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
					tcs.TrySetException(t.Exception);
				continuation(t);
				tcs.TrySetResult(default(object));
			});
			return tcs.Task;
		}

		public static Task<T> ContinueWithEx<T>(this Task<T> task, Func<Task, T> continuation)
		{
			var tcs = new TaskCompletionSource<T>();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
					tcs.TrySetException(t.Exception);
				T result = continuation(t);
				tcs.TrySetResult(result);
			});
			return tcs.Task;
		}

		public static Task<T> ContinueWithExt<T>(this Task task, Func<Task, T> continuation)
		{
			var tcs = new TaskCompletionSource<T>();
			task.ContinueWith(ant => tcs.TrySetException(ant.Exception), TaskContinuationOptions.OnlyOnFaulted);
			task.ContinueWith(ant => tcs.TrySetResult(continuation(task)), TaskContinuationOptions.OnlyOnRanToCompletion);
			task.ContinueWith(ant => tcs.SetCanceled(), TaskContinuationOptions.OnlyOnCanceled);
			return tcs.Task;
		}

		public static Task<TOut> ContinueWithExt<TOut, TIn>(this Task<TIn> task, Func<Task<TIn>, TOut> continuation)
		{
			var tcs = new TaskCompletionSource<TOut>();
			task.ContinueWith(ant => tcs.TrySetException(ant.Exception), TaskContinuationOptions.OnlyOnFaulted);
			task.ContinueWith(ant => tcs.TrySetResult(continuation(task)), TaskContinuationOptions.OnlyOnRanToCompletion);
			task.ContinueWith(ant => tcs.SetCanceled(), TaskContinuationOptions.OnlyOnCanceled);
			return tcs.Task;
		}

		// Then extesions
		public static Task Then(this Task task, Action successor)
		{
			switch (task.Status)
			{
				case TaskStatus.Faulted:
					return FromError(task.Exception);

				case TaskStatus.Canceled:
					return Canceled();

				case TaskStatus.RanToCompletion:
					return FromMethod(successor);

				default:
					return RunTask(task, successor);
			}
		}

		public static Task<TResult> Then<TResult>(this Task task, Func<TResult> successor)
		{
			switch (task.Status)
			{
				case TaskStatus.Faulted:
					return FromError<TResult>(task.Exception);

				case TaskStatus.Canceled:
					return Canceled<TResult>();

				case TaskStatus.RanToCompletion:
					return FromMethod(successor);

				default:
					return TaskRunners<object, TResult>.RunTask(task, successor);
			}
		}

		public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> successor)
		{
			switch (task.Status)
			{
				case TaskStatus.Faulted:
					return FromError<TResult>(task.Exception);

				case TaskStatus.Canceled:
					return Canceled<TResult>();

				case TaskStatus.RanToCompletion:
					return FromMethod(successor, task.Result);

				default:
					return TaskRunners<T, TResult>.RunTask(task, t => successor(t.Result));
			}
		}

		internal static Task FromError(Exception e)
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetException(e);
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

		private static Task Canceled()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetCanceled();
			return tcs.Task;
		}

		private static Task<T> Canceled<T>()
		{
			var tcs = new TaskCompletionSource<T>();
			tcs.SetCanceled();
			return tcs.Task;
		}

		internal static Task<TResult> FromCancelled<TResult>(TResult value)
		{
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			tcs.SetCanceled();
			return tcs.Task;
		}

		public static Task FromMethod(Action func)
		{
			try
			{
				func();
				return Empty;
			}
			catch (Exception ex)
			{
				return FromError(ex);
			}
		}

		public static Task<TResult> FromMethod<TResult>(Func<TResult> func)
		{
			try
			{
				return FromResult<TResult>(func());
			}
			catch (Exception ex)
			{
				return FromError<TResult>(ex);
			}
		}

		public static Task FromMethod<T1>(Action<T1> func, T1 arg)
		{
			try
			{
				func(arg);
				return Empty;
			}
			catch (Exception ex)
			{
				return FromError(ex);
			}
		}

		public static Task<TResult> FromMethod<T1, TResult>(Func<T1, TResult> func, T1 arg)
		{
			try
			{
				return FromResult<TResult>(func(arg));
			}
			catch (Exception ex)
			{
				return FromError<TResult>(ex);
			}
		}

		private static Task RunTask(Task task, Action successor)
		{
			var tcs = new TaskCompletionSource<object>();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					tcs.SetException(t.Exception);
				}
				else if (t.IsCanceled)
				{
					tcs.SetCanceled();
				}
				else
				{
					try
					{
						successor();
						tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.SetException(ex);
					}
				}
			});

			return tcs.Task;
		}

		private static class TaskRunners<T, TResult>
		{
			internal static Task RunTask(Task<T> task, Action<T> successor)
			{
				var tcs = new TaskCompletionSource<object>();
				task.ContinueWith(t =>
				{
					if (t.IsFaulted)
					{
						tcs.SetException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						tcs.SetCanceled();
					}
					else
					{
						try
						{
							successor(t.Result);
							tcs.SetResult(null);
						}
						catch (Exception ex)
						{
							tcs.SetException(ex);
						}
					}
				});

				return tcs.Task;
			}

			internal static Task<TResult> RunTask(Task task, Func<TResult> successor)
			{
				var tcs = new TaskCompletionSource<TResult>();
				task.ContinueWith(t =>
				{
					if (t.IsFaulted)
					{
						tcs.SetException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						tcs.SetCanceled();
					}
					else
					{
						try
						{
							tcs.SetResult(successor());
						}
						catch (Exception ex)
						{
							tcs.SetException(ex);
						}
					}
				});

				return tcs.Task;
			}

			internal static Task<TResult> RunTask(Task<T> task, Func<Task<T>, TResult> successor)
			{
				var tcs = new TaskCompletionSource<TResult>();
				task.ContinueWith(t =>
				{
					if (task.IsFaulted)
					{
						tcs.SetException(t.Exception);
					}
					else if (task.IsCanceled)
					{
						tcs.SetCanceled();
					}
					else
					{
						try
						{
							tcs.SetResult(successor(t));
						}
						catch (Exception ex)
						{
							tcs.SetException(ex);
						}
					}
				});

				return tcs.Task;
			}
		}
	}
}