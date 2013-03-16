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
using idTech4.Math;
using idTech4.Services;

namespace idTech4.UI
{
	public class idUserInterfaceManager : IUserInterfaceManager
	{
		#region Members
		private int _deviceContextToggle;

		private idDeviceContext _deviceContext;

		private idDeviceContext _deviceContextOld;
		private idDeviceContextOptimized _deviceContextOptimized;

		private idRectangle	_screenRect;
		#endregion

		#region Constructor
		public idUserInterfaceManager()
		{

		}
		#endregion

		#region Initialization
		public void Init()
		{
			_screenRect = new idRectangle(0, 0, 640, 480);

			_deviceContextOld = new idDeviceContext();
			_deviceContextOld.Init();

			_deviceContextOptimized = new idDeviceContextOptimized();
			_deviceContextOptimized.Init();

			SetDrawingContext();
		}

		private void SetDrawingContext()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			// to make it more obvious that there is a difference between the old and
			// new paths, toggle between them every frame if g_useNewGuiCode is set to 2
			_deviceContextToggle++;

			if((cvarSystem.GetInt("g_useNewGuiCode") == 1)
				|| ((cvarSystem.GetInt("g_useNewGuiCode") == 2) && ((_deviceContextToggle & 1) != 0)))
			{
				_deviceContext = _deviceContextOptimized;
			}
			else
			{
				_deviceContext = _deviceContextOld;
			}
		}
		#endregion

		#region Loading
		public void BeginLevelLoad()
		{
			idLog.Warning("TODO: ui.BeginLevelLoad");
			/*foreach(idUserInterface userInterface in _interfaces)
			{
				userInterface.ClearReferences();
			}*/
		}
 
		public void EndLevelLoad(string mapName)
		{
			idLog.Warning("TODO: idUserInterface.EndLevelLoad");

			/*int c = guis.Num();
			for ( int i = 0; i < c; i++ ) {
				if ( guis[i]->GetRefs() == 0 ) {
					//common->Printf( "purging %s.\n", guis[i]->GetSourceFile() );

					// use this to make sure no materials still reference this gui
					bool remove = true;
					for ( int j = 0; j < declManager->GetNumDecls( DECL_MATERIAL ); j++ ) {
						const idMaterial *material = static_cast<const idMaterial *>(declManager->DeclByIndex( DECL_MATERIAL, j, false ));
						if ( material->GlobalGui() == guis[i] ) {
							remove = false;
							break;
						}
					}
					if ( remove ) {
						delete guis[ i ];
						guis.RemoveIndex( i );
						i--; c--;
					}
				}
				common->UpdateLevelLoadPacifier();
			}*/

			_deviceContextOld.Init();
			_deviceContextOptimized.Init();
		}
		#endregion
	}
}