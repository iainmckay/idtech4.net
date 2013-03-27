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
using System.Collections.Generic;

namespace idTech4.Game.Menus
{
	/// <summary>
	/// We're using a model/view architecture, so this is the combination of both model and view.  The
	/// other part of the view is the SWF itself.
	/// </summary>
	public abstract class idMenuWidget
	{
		#region Properties
		public idMenuWidget[] Children
		{
			get
			{
				return _children.ToArray();
			}
		}
		#endregion

		#region Members
		protected idMenuHandler _menuData;

		private List<idMenuWidget> _children  = new List<idMenuWidget>();
		private List<idMenuWidget> _observers = new List<idMenuWidget>();
		#endregion

		#region Constructor
		public idMenuWidget()
		{
			// TODO:
			/*eventActionLookup.SetNum( eventActionLookup.Max() );
			for ( int i = 0; i < eventActionLookup.Num(); ++i ) {
				eventActionLookup[ i ] = INVALID_ACTION_INDEX;
			}*/
		}
		#endregion

		#region Initialization
		public void Initialize(idMenuHandler data)
		{
			_menuData = data;
		}
		#endregion

		#region Frame
		public virtual void Update() 
		{ 
		
		}
		#endregion
	}
}