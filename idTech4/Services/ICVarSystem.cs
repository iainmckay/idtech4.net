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

namespace idTech4.Services
{
	public interface ICVarSystem
	{
		#region Methods
		#region Command Completion
		string[] ArgumentCompletion(string name, string argText);
		string[] CommandCompletion(Predicate<string> filter);		
		#endregion

		#region Find
		idCVar Find(string name, bool ignoreMissing = false);
		#endregion

		#region Initialization
		#region Properties
		bool IsInitialized { get; }
		#endregion

		#region Methods
		void Initialize();
		#endregion
		#endregion

		#region Misc.
		bool Command(CommandArguments args);
		void ListByFlags(string[] args, CVarFlags flags);
		void Restart();
		#endregion

		#region Modification
		void ClearModified(string name);
		void ClearModifiedFlags(CVarFlags flags);
		bool IsModified(string name);

		void Set(string name, string value);
		void Set(string name, bool value);
		void Set(string name, int value);
		void Set(string name, float value);

		void SetModified(string name);
		void SetModifiedFlags(CVarFlags flags);
		#endregion

		#region Registration
		idCVar Register(string name, string value, string description, CVarFlags flags);
		idCVar Register(string name, string value, string description, CVarFlags flags, ArgCompletion valueCompletion);
		idCVar Register(string name, string value, float valueMin, float valueMax, string description, CVarFlags flags);
		idCVar Register(string name, string value, float valueMin, float valueMax, string description, CVarFlags flags, ArgCompletion valueCompletion);
		idCVar Register(string name, string value, string description, string[] valueStrings, CVarFlags flags);
		idCVar Register(string name, string value, string[] valueStrings, string description, CVarFlags flags, ArgCompletion valueCompletion);
		#endregion

		#region Value Retrieval
		string GetString(string name);
		bool GetBool(string name);
		int GetInt(string name);
		long GetInt64(string name);
		float GetFloat(string name);
		#endregion
		#endregion
	}
}