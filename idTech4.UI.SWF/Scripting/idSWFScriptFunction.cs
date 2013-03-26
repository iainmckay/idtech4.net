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

namespace idTech4.UI.SWF.Scripting
{
	public abstract class idSWFScriptFunction
	{
		#region Members
		protected List<idSWFParameterInfo> _parameters = new List<idSWFParameterInfo>();
		#endregion

		public abstract idSWFScriptVariable Invoke(idSWFScriptObject scriptObj, idSWFParameterList parms);

		#region idSWFParameterInfo
		protected struct idSWFParameterInfo
		{
			public string Name;
			public byte Register;
		}
		#endregion
	}

	public class idSWFParameterList
	{
		#region Properties
		public int Count
		{
			get
			{
				return _list.Count;
			}
		}

		public idSWFScriptVariable this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
			}
		}
		#endregion

		#region Members
		private List<idSWFScriptVariable> _list;
		#endregion

		#region Constructor
		public idSWFParameterList()
		{
			_list = new List<idSWFScriptVariable>();
		}

		public idSWFParameterList(int count)
		{
			_list = new List<idSWFScriptVariable>(count);
		}
		#endregion
	}

	public class idSWFStack : List<idSWFScriptVariable>
	{
		public idSWFScriptVariable A
		{
			get
			{
				return this[this.Count - 1];
			}
		}

		public idSWFScriptVariable B
		{
			get
			{
				return this[this.Count - 2];
			}
		}

		public idSWFScriptVariable C
		{
			get
			{
				return this[this.Count - 3];
			}
		}

		public idSWFScriptVariable D
		{
			get
			{
				return this[this.Count - 4];
			}
		}

		public void Pop(int n)
		{
			this.RemoveRange(this.Count - n, n);
		}

		public idSWFScriptVariable Alloc()
		{
			idSWFScriptVariable var = new idSWFScriptVariable();
			this.Add(var);

			return var;
		}

		public idSWFStack(int count)
			: base(count)
		{
			for(int i = 0; i < count; i++)
			{
				this.Add(new idSWFScriptVariable());
			}
		}
	}

	/// <summary>
	/// Action Scripts can define a pool of constants then push values from that pool.
	/// </summary>
	/// <remarks>
	/// The documentation isn't very clear on the scope of these things, but from what
	/// I've gathered by testing, pool is per-function and copied into the function
	/// whenever that function is declared.
	/// </remarks>
	public class idSWFConstantPool
	{
		#region Members
		private List<string> _pool = new List<string>();
		#endregion

		#region Constructor
		public idSWFConstantPool()
		{

		}
		#endregion

		#region Methods
		public string Get(int n)
		{
			return _pool[n];
		}
		#endregion
	}
}