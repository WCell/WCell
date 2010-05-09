/*************************************************************************
 *
 *   file		: ThreadMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-01 08:52:17 +0200 (ti, 01 sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1061 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Threading;
using NLog;
using System.Collections.Generic;

namespace Cell.Core
{
    /// <summary>
    /// This class manages the application thread pool.
    /// <seealso cref="IExecutionObject"/>
    /// </summary>
    /// <remarks>
    /// This class manages the queueing and execution of execution objects. If <see cref="ThreadMgr"/> is running with single-threading disabled, execution objects
    /// will be executed asynchronously and should be thread-safe. If not single-threaded <see cref="ThreadMgr"/> uses a system managed thread pool that scales according
    /// to the number of CPU's and the amount of data to be processed. See <see cref="System.Threading.ThreadPool"/> for more information.
    /// </remarks>
    public static class ThreadMgr
    {
        /// <summary>
        /// A delegate for the function that will handle an object after it has been executed.
        /// </summary>
        /// <param name="obj">The object that has been executed.</param>
        public delegate void ObjectExecuted(object obj);

        /// <summary>
        /// The delegate that holds the function for handling objects that have been executed.
        /// </summary>
        private static ObjectExecuted _objExec;

        /// <summary>
        /// Thread for single-threaded execution.
        /// </summary>
        private static Thread _thread;

        /// <summary>
        /// True if the application is to run with only 1 thread.
        /// </summary>
        private static bool _singleThread;

        /// <summary>
        /// True if the <see cref="ThreadMgr"/> has been started.
        /// <seealso cref="ThreadMgr.Start()"/>
        /// </summary>
        private static bool _running;

        /// <summary>
        /// Queue of execution objects to be executed in a single-threaded enviroment.
        /// <seealso cref="IExecutionObject"/>
        /// </summary>
        private static List<IExecutionObject> _queue = new List<IExecutionObject>();

        /// <summary>
        /// Spin wait lock for execution object queue synchronization
        /// </summary>
        private static SpinWaitLock _queueSpinLock = new SpinWaitLock();

        /// <summary>
        /// Gets/Sets the threading state of the application.
        /// <seealso cref="ThreadMgr.SwitchThreadModel"/>
        /// </summary>
        public static bool SingleThreaded
        {
            get { return _singleThread; }
            set
            {
                if (value != _singleThread)
                {
                    SwitchThreadModel();
                }
            }
        }

        /// <summary>
        /// Gets the current state of the <see cref="ThreadMgr"/>.
        /// </summary>
        public static bool Running
        {
            get { return _running; }
        }

        /// <summary>
        /// Switches the current application thread model.
        /// </summary>
        private static void SwitchThreadModel()
        {
            if (!_running)
            {
                return;
            }

            if (_singleThread)
            {
                _thread.Abort();
                _thread = null;
                _singleThread = false;
            }
            else
            {
                _thread = new Thread(ThreadMain);
                _thread.Start();
                _singleThread = true;
            }
        }

        /// <summary>
        /// Called when running in the single-threaded execution model.
        /// </summary>
        private static void ThreadMain()
        {
            try
            {
                while (_running)
                {
                    try
                    {
                        PurgeQueue();
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger(CellDef.CORE_LOG_FNAME).Error(ex.ToString());
                    }
                }
            }
            catch (ThreadAbortException)
            {
                PurgeQueue();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(CellDef.CORE_LOG_FNAME).Error(e.ToString());
            }
        }

        /// <summary>
        /// Purges the internal queue of execution objects.
        /// <seealso cref="IExecutionObject"/>
        /// </summary>
        private static void PurgeQueue()
        {
            IExecutionObject[] execObjs;

            _queueSpinLock.Enter();

            try
            {
                execObjs = _queue.ToArray();
                _queue.Clear();
            }
            finally
            {
                _queueSpinLock.Exit();
            }

            foreach (IExecutionObject obj in execObjs)
            {
                try
                {
                    obj.Execute();

                    if (_objExec != null)
                    {
                        _objExec(obj);
                    }

					if (obj is IDisposable)
					{
						(obj as IDisposable).Dispose();
					}
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(CellDef.CORE_LOG_FNAME).Error(e.ToString());
                }
            }
        }

        /// <summary>
        /// Starts thread execution and <see cref="IExecutionObject"/> queueing.
        /// </summary>
        public static void Start()
        {
            _objExec = null;
            _running = true;

            if (_singleThread)
            {
                _thread = new Thread(new ThreadStart(ThreadMain));
                _thread.Start();
            }
        }

        /// <summary>
        /// Starts thread execution and <see cref="IExecutionObject"/> queueing.
        /// </summary>
        public static void Start(ObjectExecuted postObjExecFunc)
        {
            _objExec = postObjExecFunc;
            _running = true;

            if (_singleThread)
            {
                _thread = new Thread(new ThreadStart(ThreadMain));
                _thread.Start();
            }
        }

        /// <summary>
        /// Stops thread execution and purges the internal queue (<see cref="ThreadMgr.PurgeQueue"/>).
        /// </summary>
        public static void Stop()
        {
            _running = false;

            PurgeQueue();

            if (_singleThread)
            {
                _thread.Abort();
                _thread = null;
            }
        }

        /// <summary>
        /// Queue's an <see cref="IExecutionObject"/> for execution.
        /// <seealso cref="IExecutionObject"/>
        /// </summary>
        /// <param name="obj">The object to be executed</param>
        /// <returns>True if the object was successfully queued.</returns>
        /// <remarks>
        /// If the <see cref="ThreadMgr"/> hasn't been started yet or has been stopped the return value will be false. Otherwise it's always true.
        /// </remarks>
        public static bool QueueJob(IExecutionObject obj)
        {
            if (_running)
            {
                if (_singleThread)
                {
                    _queueSpinLock.Enter();

                    try
                    {
                        _queue.Add(obj);
                    }
                    finally
                    {
                        _queueSpinLock.Exit();
                    }
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(PoolMain), obj);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Asynchronously executes an execution object.
        /// <seealso cref="IExecutionObject"/>
        /// <seealso cref="ThreadMgr.QueueJob"/>
        /// </summary>
        /// <param name="state">The object to be executed.</param>
        private static void PoolMain(object state)
        {
            IExecutionObject obj = (IExecutionObject) state;

            try
            {
                obj.Execute();

                if (_objExec != null)
                {
                    _objExec(obj);
                }

				if (obj is IDisposable)
				{
					(obj as IDisposable).Dispose();
				}
            }
            catch (Exception e)
            {
                LogManager.GetLogger(CellDef.CORE_LOG_FNAME).Error(e.ToString());
            }
        }
    }
}