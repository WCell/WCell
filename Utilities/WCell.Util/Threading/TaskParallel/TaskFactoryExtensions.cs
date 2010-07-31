using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>Extensions for TaskFactory.</summary>
    public static class TaskFactoryExtensions
    {
        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory<TResult> ToGeneric<TResult>(this TaskFactory factory)
        {
            return new TaskFactory<TResult>(
                factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory ToNonGeneric<TResult>(this TaskFactory<TResult> factory)
        {
            return new TaskFactory(
                factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler(this TaskFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return factory.Scheduler ?? TaskScheduler.Current;
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler<TResult>(this TaskFactory<TResult> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return factory.Scheduler != null ? factory.Scheduler : TaskScheduler.Current;
        }

        /// <summary>Converts TaskCreationOptions into TaskContinuationOptions.</summary>
        /// <param name="creationOptions"></param>
        /// <returns></returns>
        private static TaskContinuationOptions ContinuationOptionsFromCreationOptions(TaskCreationOptions creationOptions)
        {
            return (TaskContinuationOptions)
                ((creationOptions & TaskCreationOptions.AttachedToParent) |
                 (creationOptions & TaskCreationOptions.PreferFairness) |
                 (creationOptions & TaskCreationOptions.LongRunning));
        }

        /// <summary>Asynchronously executes a sequence of tasks, maintaining a list of all tasks processed.</summary>
        /// <param name="factory">The TaskFactory to use to create the task.</param>
        /// <param name="functions">
        /// The functions that generate the tasks through which to iterate sequentially.
        /// Iteration will cease if a task faults.
        /// </param>
        /// <returns>A Task that will return the list of tracked tasks iterated.</returns>
        public static Task<IList<Task>> TrackedSequence(this TaskFactory factory, params Func<Task>[] functions)
        {
            var tcs = new TaskCompletionSource<IList<Task>>();
            factory.Iterate(TrackedSequenceInternal(functions, tcs));
            return tcs.Task;
        }

        /// <summary>Creates the enumerable to iterate through with Iterate.</summary>
        /// <param name="functions">
        /// The functions that generate the tasks through which to iterate sequentially.
        /// Iteration will cease if a task faults.
        /// </param>
        /// <param name="tcs">The TaskCompletionSource to resolve with the asynchronous results.</param>
        /// <returns>The enumerable through which to iterate.</returns>
        private static IEnumerable<Task> TrackedSequenceInternal(
            IEnumerable<Func<Task>> functions, TaskCompletionSource<IList<Task>> tcs)
        {
            // Store a list of all tasks iterated through.  This will be provided
            // to the resulting task when we're done.
            var tasks = new List<Task>();

            // Run seqeuentially through all of the provided functions.
            foreach (var func in functions)
            {
                // Get the next task.  If we get an exception while trying to do so,
                // an invalid function was provided.  Fault the TCS and break out.
                Task nextTask = null;
                try { nextTask = func(); }
                catch (Exception exc) { tcs.TrySetException(exc); }
                if (nextTask == null) yield break;

                // Store the task that was generated and yield it from the sequence.  If the task
                // faults, break out of the loop so that no more tasks are processed.
                tasks.Add(nextTask);
                yield return nextTask;
                if (nextTask.IsFaulted) break;
            }

            // We're done.  Transfer all tasks we iterated through.
            tcs.TrySetResult(tasks);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source, object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, state, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source, object state,
            CancellationToken cancellationToken)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, state, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source, object state,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, state, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source, object state,
            TaskScheduler scheduler)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, state, factory.CancellationToken, factory.CreationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source, object state,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            // Validate/update parameters
            if (factory == null) throw new ArgumentNullException("factory");
            if (source == null) throw new ArgumentNullException("asyncIterator");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            // Get an enumerator from the enumerable
            var enumerator = source.GetEnumerator();
            if (enumerator == null) throw new InvalidOperationException("Invalid enumerable - GetEnumerator returned null");

            // Create the task to be returned to the caller.  And ensure
            // that when everything is done, the enumerator is cleaned up.
            var trs = new TaskCompletionSource<object>(state, creationOptions);
            trs.Task.ContinueWith(_ => enumerator.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            // This will be called every time more work can be done.
            Action<Task> recursiveBody = null;
            recursiveBody = antecedent =>
            {
                try
                {
                    // If we should continue iterating and there's more to iterate
                    // over, create a continuation to continue processing.  We only
                    // want to continue processing once the current Task (as yielded
                    // from the enumerator) is complete.
                    if (enumerator.MoveNext())
                    {
                        var nextItem = enumerator.Current;

                        // If we got a Task, continue from it to continue iterating
                        if (nextItem is Task)
                        {
                            var nextTask = (Task)nextItem;
                            /**/
                            nextTask.IgnoreExceptions(); // TODO: Is this a good idea?
                            nextTask.ContinueWith(recursiveBody).IgnoreExceptions();
                        }
                        // If we got a scheduler, continue iterating under the new scheduler,
                        // enabling hopping between contexts.
                        else if (nextItem is TaskScheduler)
                        {
                            Task.Factory.StartNew(() => recursiveBody(null), CancellationToken.None, TaskCreationOptions.None, (TaskScheduler)nextItem).IgnoreExceptions();
                        }
                        // Anything else is invalid
                        else trs.TrySetException(new InvalidOperationException("Task or TaskScheduler object expected in Iterate"));
                    }

                    // Otherwise, we're done!
                    else trs.TrySetResult(null);
                }
                // If MoveNext throws an exception, propagate that to the user,
                // either as cancellation or as a fault
                catch (Exception exc)
                {
                    var oce = exc as OperationCanceledException;
                    if (oce != null && oce.CancellationToken == cancellationToken)
                    {
                        trs.TrySetCanceled();
                    }
                    else trs.TrySetException(exc);
                }
            };

            // Get things started by launching the first task
            factory.StartNew(() => recursiveBody(null), CancellationToken.None, TaskCreationOptions.None, scheduler).IgnoreExceptions();

            // Return the representative task to the user
            return trs.Task;
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, null, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source,
            CancellationToken cancellationToken)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, null, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, null, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source,
            TaskScheduler scheduler)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return Iterate(factory, source, null, factory.CancellationToken, factory.CreationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(
            this TaskFactory factory,
            IEnumerable<object> source,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return Iterate(factory, source, null, cancellationToken, creationOptions, scheduler);
        }

        /// <summary>Creates a Task that will be completed when the specified WaitHandle is signaled.</summary>
        /// <param name="waitHandle">The WaitHandle.</param>
        /// <returns>The created Task.</returns>
        public static Task FromAsync(WaitHandle waitHandle)
        {
            var tcs = new TaskCompletionSource<object>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, delegate { tcs.TrySetResult(null); }, null, -1, true);
            var t = tcs.Task;
            t.ContinueWith(_ => rwh.Unregister(null), TaskContinuationOptions.ExecuteSynchronously);
            return t;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task FromException(this TaskFactory factory, Exception exception)
        {
            var tcs = new TaskCompletionSource<object>(factory.CreationOptions);
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory factory, Exception exception)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>Creates a Task that has completed in the RanToCompletion state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="result">The result with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory factory, TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetResult(result);
            return tcs.Task;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory<TResult> factory, Exception exception)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>Creates a Task that has completed in the RanToCompletion state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="result">The result with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory<TResult> factory, TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetResult(result);
            return tcs.Task;
        }

        /// <summary>Creates a Task that will complete after the specified delay.</summary>
        /// <param name="factory">The TaskFactory.</param>
        /// <param name="millisecondsDelay">The delay after which the Task should transition to RanToCompletion.</param>
        /// <returns>A Task that will be completed after the specified duration.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory, int millisecondsDelay)
        {
            return StartNewDelayed(factory, millisecondsDelay, CancellationToken.None);
        }

        /// <summary>Creates a Task that will complete after the specified delay.</summary>
        /// <param name="factory">The TaskFactory.</param>
        /// <param name="millisecondsDelay">The delay after which the Task should transition to RanToCompletion.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the timed task.</param>
        /// <returns>A Task that will be completed after the specified duration and that's cancelable with the specified token.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, CancellationToken cancellationToken)
        {
            // Validate arguments
            if (factory == null) throw new ArgumentNullException("factory");
            if (millisecondsDelay < 0) throw new ArgumentOutOfRangeException("millisecondsDelay");

            // Create the timed task
            var tcs = new TaskCompletionSource<object>(factory.CreationOptions);
            var ctr = default(CancellationTokenRegistration);

            // Create the timer but don't start it yet.  If we start it now,
            // it might fire before ctr has been set to the right registration.
            var timer = new Timer(self =>
            {
                // Clean up both the cancellation token and the timer, and try to transition to completed
                ctr.Dispose();
                ((Timer)self).Dispose();
                tcs.TrySetResult(null);
            });

            // Register with the cancellation token.
            if (cancellationToken.CanBeCanceled)
            {
                // When cancellation occurs, cancel the timer and try to transition to canceled.
                // There could be a race, but it's benign.
                ctr = cancellationToken.Register(() =>
                {
                    timer.Dispose();
                    tcs.TrySetCanceled();
                });
            }

            // Start the timer and hand back the task...
            timer.Change(millisecondsDelay, Timeout.Infinite);
            return tcs.Task;
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action action)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, action, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action action,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, action, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action action,
            CancellationToken cancellationToken)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, action, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action action,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (millisecondsDelay < 0) throw new ArgumentOutOfRangeException("millisecondsDelay");
            if (action == null) throw new ArgumentNullException("action");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            return factory
                .StartNewDelayed(millisecondsDelay, cancellationToken)
                .ContinueWith(_ => action(), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action<object> action, object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, action, state, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action<object> action, object state,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, action, state, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action<object> action, object state,
            CancellationToken cancellationToken)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, action, state, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(
            this TaskFactory factory,
            int millisecondsDelay, Action<object> action, object state,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (millisecondsDelay < 0) throw new ArgumentOutOfRangeException("millisecondsDelay");
            if (action == null) throw new ArgumentNullException("action");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            // Create the task that will be returned; workaround for no ContinueWith(..., state) overload.
            var result = new TaskCompletionSource<object>(state);

            // Delay a continuation to run the action
            factory
                .StartNewDelayed(millisecondsDelay, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled) result.TrySetCanceled();
                    else
                    {
                        try
                        {
                            action(state);
                        }
                        catch (Exception exc) { result.TrySetException(exc); }
						finally
						{
							result.TrySetResult(null);
                        	if (result.Task.Status == TaskStatus.RanToCompletion)
                        	{
                        		result.Task.Dispose();
                        	}
                    }
                    }
                }, scheduler);

            // Return the task
            return result.Task;
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<TResult> function)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, function, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<TResult> function,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, function, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<TResult> function,
            CancellationToken cancellationToken)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, function, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<TResult> function,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (millisecondsDelay < 0) throw new ArgumentOutOfRangeException("millisecondsDelay");
            if (function == null) throw new ArgumentNullException("function");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            // Create the trigger and the timer to start it
            var tcs = new TaskCompletionSource<object>();
            var timer = new Timer(obj => ((TaskCompletionSource<object>)obj).SetResult(null),
                tcs, millisecondsDelay, Timeout.Infinite);

            // Return a task that executes the function when the trigger fires
            return tcs.Task.ContinueWith(_ =>
            {
                timer.Dispose();
                return function();
            }, cancellationToken, ContinuationOptionsFromCreationOptions(creationOptions), scheduler);
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<object, TResult> function, object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, function, state, factory.CancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<object, TResult> function, object state,
            CancellationToken cancellationToken)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, function, state, cancellationToken, factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<object, TResult> function, object state,
            TaskCreationOptions creationOptions)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return StartNewDelayed(factory, millisecondsDelay, function, state, factory.CancellationToken, creationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(
            this TaskFactory<TResult> factory,
            int millisecondsDelay, Func<object, TResult> function, object state,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (millisecondsDelay < 0) throw new ArgumentOutOfRangeException("millisecondsDelay");
            if (function == null) throw new ArgumentNullException("action");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            // Create the task that will be returned
            var result = new TaskCompletionSource<TResult>(state);
            Timer timer = null;

            // Create the task that will run the user's function
            var functionTask = new Task<TResult>(function, state, creationOptions);

            // When the function task completes, transfer the results to the returned task
            functionTask.ContinueWith(t =>
            {
                result.SetFromTask(t);
                timer.Dispose();
            }, cancellationToken, ContinuationOptionsFromCreationOptions(creationOptions) | TaskContinuationOptions.ExecuteSynchronously, scheduler);

            // Start the timer for the trigger
            timer = new Timer(obj => ((Task)obj).Start(scheduler),
                functionTask, millisecondsDelay, Timeout.Infinite);

            return result.Task;
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(
            this TaskFactory factory, Action action)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new Task(action, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(
            this TaskFactory factory, Action action, TaskCreationOptions creationOptions)
        {
            return new Task(action, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(
            this TaskFactory factory, Action<Object> action, object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new Task(action, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(
            this TaskFactory factory, Action<Object> action, object state, TaskCreationOptions creationOptions)
        {
            return new Task(action, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory factory, Func<TResult> function)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new Task<TResult>(function, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory factory, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory factory, Func<Object, TResult> function, object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new Task<TResult>(function, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory factory, Func<Object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory<TResult> factory, Func<TResult> function)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new Task<TResult>(function, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory<TResult> factory, Func<TResult> function, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory<TResult> factory, Func<Object, TResult> function, object state)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            return new Task<TResult>(function, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(
            this TaskFactory<TResult> factory, Func<Object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>
        /// Creates a continuation Task that will compplete upon
        /// the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the array of completed tasks.</returns>
        public static Task<Task[]> WhenAll(
            this TaskFactory factory, params Task[] tasks)
        {
            return factory.ContinueWhenAll(tasks, completedTasks => completedTasks);
        }

        /// <summary>
        /// Creates a continuation Task that will compplete upon
        /// the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the array of completed tasks.</returns>
        public static Task<Task<TAntecedentResult>[]> WhenAll<TAntecedentResult>(
            this TaskFactory factory, params Task<TAntecedentResult>[] tasks)
        {
            return factory.ContinueWhenAll(tasks, completedTasks => completedTasks);
        }

        /// <summary>
        /// Creates a continuation Task that will complete upon
        /// the completion of any one of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the completed task.</returns>
        public static Task<Task> WhenAny(
            this TaskFactory factory, params Task[] tasks)
        {
            return factory.ContinueWhenAny(tasks, completedTask => completedTask);
        }

        /// <summary>
        /// Creates a continuation Task that will complete upon
        /// the completion of any one of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the completed task.</returns>
        public static Task<Task<TAntecedentResult>> WhenAny<TAntecedentResult>(
            this TaskFactory factory, params Task<TAntecedentResult>[] tasks)
        {
            return factory.ContinueWhenAny(tasks, completedTask => completedTask);
        }
    }
}