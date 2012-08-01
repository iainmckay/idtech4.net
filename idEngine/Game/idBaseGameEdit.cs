using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Renderer;

namespace idTech4.Game
{
	public abstract class idBaseGameEdit
	{
		#region Constructor
		public idBaseGameEdit()
		{

		}
		#endregion

		#region Methods
		#region idDict parameter parsing
		public abstract idRenderEntity ParseSpawnArgsToRenderEntity(idDict args);
		#endregion
		#endregion
	}
}
