/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Diagnostics;
using System.Threading;

namespace idTech4.Threading
{

	/// <summary>
	/// An abstract base class, to be extended by classes implementing the Run() method. 
	/// </summary>
	/// <remarks>	
	/// A worker thread is a thread that waits in place (without consuming CPU)
	/// until work is available. A worker thread is implemented as normal, except that, instead of
	/// calling the Start() method, the StartWorker() method is called to start the thread.
	/// Note the Sys_CreateThread function does not support the concept of worker threads.
	/// <para/>
	/// class idMyWorkerThread : public idSysThread {
	/// public:
	///		virtual int Run() {
	/// 	// run thread code here
	/// 	return 0;
	/// }
	/// // specify thread data here
	/// };
	/// <para/>
	/// idMyWorkerThread thread;
	/// thread.StartThread( "myWorkerThread" );
	/// <para/> 
	// main thread loop
	/// for ( ; ; ) {
	/// 	// setup work for the thread here (by modifying class data on the thread)
	/// 	thread.SignalWork();           // kick in the worker thread
	/// 	// run other code in the main thread here (in parallel with the worker thread)
	/// 	thread.WaitForThread();        // wait for the worker thread to finish
	/// 	// use results from worker thread here
	/// }
	/// <para/>
	/// In the above example, the thread does not continuously run in parallel with the main Thread,
	/// but only for a certain period of time in a very controlled manner. Work is set up for the
	/// Thread and then the thread is signalled to process that work while the main thread continues.
	/// After doing other work, the main thread can wait for the worker thread to finish, if it has not
	/// finished already. When the worker thread is done, the main thread can safely use the results
	/// from the worker thread.
	/// <para/>
	/// Note that worker threads are useful on all platforms but they do not map to the SPUs on the PS3.
	/// </summary>
	public abstract class idSysThread
	{
		#region Members
		private string _name;
		private Thread _thread;

		private bool _isWorker;
		private bool _isRunning;
		private volatile bool _isTerminating;
		private volatile bool _moreWorkToDo;

		private ManualResetEvent _signalWorkerDone;
		private AutoResetEvent _signalMoreWorkToDo;
		private object _signalMutex;
		#endregion

		#region Constructor
		public idSysThread()
		{
			_signalWorkerDone   = new ManualResetEvent(false);
			_signalMoreWorkToDo = new AutoResetEvent(false);
			_signalMutex        = new object();
		}

		// TODO: cleanup
		/*idSysThread::~idSysThread() {
			StopThread( true );
			if ( threadHandle ) {
				Sys_DestroyThread( threadHandle );
			}
		}*/
		#endregion

		#region Methods
		public bool StartThread(string name, ThreadCore core, ThreadPriority priority, int stackSize)
		{
			if(_isRunning == true)
			{
				return false;
			}

			_name          = name;
			_isTerminating = false;

			if(_thread != null)
			{
				_thread.Abort();
			}

			ThreadStart threadStart = new ThreadStart(ThreadProc);

			_thread              = new Thread(threadStart, stackSize);
			_thread.Priority     = priority;
			_thread.Name         = name;
			_thread.IsBackground = true;
			_thread.Start();

			// Under Windows, we don't set the thread affinity and let the OS deal with scheduling
			// FIXME: xbox
	
			_isRunning = true;

			return true;
		}

		public bool StartWorkerThread(string name, ThreadCore core, ThreadPriority priority, int stackSize)
		{
			if(_isRunning == true)
			{
				return false;
			}

			_isWorker = true;

			bool result = StartThread(name, core, priority, stackSize);

			_signalWorkerDone.WaitOne(-1);

			return result;
		}

		public void WaitForThread()
		{
			if(_isWorker == true)
			{
				_signalWorkerDone.WaitOne(-1);
			}
			else if(_isRunning == true)
			{
				_thread.Abort();
				_thread = null;
			}
		}

		public void SignalWork()
		{
			if(_isWorker == true)
			{
				lock(_signalMutex)
				{
					_moreWorkToDo = true;

					_signalWorkerDone.Reset();
					_signalMoreWorkToDo.Set();
				}
			}
		}

		protected abstract int Run();

		private void ThreadProc()
		{
			try
			{
				bool wait = false;

				if(_isWorker == true)
				{
					for( ; ; ) 
					{
						lock(_signalMutex)
						{
							if(_moreWorkToDo == true)
							{
								_moreWorkToDo = false;										
								_signalMoreWorkToDo.Reset();
							} 
							else 
							{
								_signalWorkerDone.Set();
								wait = true;
							}
						}

						if(wait == true)
						{
							wait = false;
							_signalMoreWorkToDo.WaitOne(-1);
							continue;
						}

						if(_isTerminating == true)
						{
							break;
						}

						Run();
					}

					_signalWorkerDone.Set();
				} 
				else 
				{
					Run();
				}
			}
			catch(Exception x)
			{
				idLog.Warning("Fatal error in thread {0}: {1}", _name, x.Message);

				// We don't handle threads terminating unexpectedly very well, so just terminate the whole process
				Environment.Exit(0);
			}

			_isRunning = false;
		}
		#endregion
	}

	public enum ThreadCore
	{
		Any = -1,
		C_0A,
		C_0B,
		C_1A,
		C_1B,
		C_2A,
		C_2B
	}
}