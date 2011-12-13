using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Threading;

namespace idTech4.Game
{
	public class idConsole
	{
		public static void Write(string format, params object[] args)
		{
			idE.Write(format, args);
		}

		public static void WriteLine(string format, params object[] args)
		{
			idE.WriteLine(format, args);
		}

		public static void DWrite(string format, params object[] args)
		{
			idE.DWrite(format, args);
		}

		public static void DWriteLine(string format, params object[] args)
		{
			idE.DWriteLine(format, args);
		}

		public static void Warning(string format, params object[] args)
		{
			idThread thread = idThread.CurrentThread;

			if(thread != null)
			{
				thread.Warning(format, args);
			}
			else
			{
				idE.Warning(format, args);
			}
		}

		public static void DWarning(string format, params object[] args)
		{
			idThread thread = idThread.CurrentThread;

			if(thread != null)
			{
				thread.DWarning(format, args);
			}
			else
			{
				idE.DWarning(format, args);
			}
		}

		public static void Error(string format, params object[] args)
		{
			idThread thread = idThread.CurrentThread;

			if(thread != null)
			{
				thread.Error(format, args);
			}
			else
			{
				idE.Error(format, args);
			}
		}
	}
}
