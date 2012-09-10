using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4.Geometry
{
	/// <summary>
	/// idFixedWinding is a fixed buffer size winding not using memory allocations.
	/// </summary>
	/// <remarks>
	/// When an operation would overflow the fixed buffer a warning
	/// is printed and the operation is safely cancelled.
	/// </remarks>
	public class idFixedWinding : idWinding
	{
		#region Constants
		public const int MaxPointsOnWinding = 64;
		#endregion

		#region Constructor
		public idFixedWinding()
			: base(MaxPointsOnWinding)
		{

		}
		#endregion
	}
}