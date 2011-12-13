using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Threading
{
	public class idThread
	{
		#region Properties
		#region Static
		public static idThread CurrentThread
		{
			get
			{
				return null;
			}
		}
		#endregion
		#endregion

		#region Constructor
		public idThread()
		{

		}
		#endregion

		#region Methods
		public void Warning(string format, params object[] args)
		{
			throw new NotImplementedException();
		}

		public void DWarning(string format, params object[] args)
		{
			throw new NotImplementedException();
		}
		
		public void Error(string format, params object[] args)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
