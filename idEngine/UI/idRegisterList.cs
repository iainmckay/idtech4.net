/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Text;

namespace idTech4.UI
{
	public sealed class idRegisterList
	{
		#region Members
		private List<idRegister> _registers = new List<idRegister>();
		private Dictionary<string, idRegister> _registerDict = new Dictionary<string, idRegister>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Constructor
		public idRegisterList()
		{

		}
		#endregion

		#region Methods
		#region Public
		public void AddRegister(string name, RegisterType type, idScriptParser parser, idWindow window, idWindowVariable var)
		{
			idRegister register = FindRegister(name);

			if(register == null)
			{
				int regCount = idRegister.RegisterTypeCount[(int) type];
				register = new idRegister(name, type, var);
				
				if(type == RegisterType.String)
				{
					idToken token;
					
					if((token = parser.ReadToken()) != null)
					{
						var.Init(idE.Language.Get(token.ToString()), window);
					}
				}
				else
				{
					for(int i = 0; i < regCount; i++)
					{
						register.Indexes[i] = window.ParseExpression(parser, null);

						if(i < (regCount - 1))
						{
							parser.ExpectTokenString(",");
						}
					}
				}

				_registers.Add(register);
				_registerDict.Add(name, register);
			}
			else
			{
				int regCount = idRegister.RegisterTypeCount[(int) type];

				register.Variable = var;

				if(type == RegisterType.String)
				{
					idToken token = parser.ReadToken();

					if(token != null)
					{
						var.Init(token.ToString(), window);
					}
				}
				else
				{
					for(int i = 0; i < regCount; i++)
					{
						register.Indexes[i] = window.ParseExpression(parser);

						if(i < (regCount - 1))
						{
							parser.ExpectTokenString(",");
						}
					}
				}
			}
		}

		public idRegister FindRegister(string name)
		{
			idRegister reg;

			if(_registerDict.TryGetValue(name, out reg) == true)
			{
				return reg;
			}

			return null;
		}

		public void SetToRegisters(ref float[] registers)
		{
			foreach(idRegister reg in _registers)
			{
				reg.SetToRegisters(ref registers);
			}
		}

		public void GetFromRegisters(float[] registers)
		{
			foreach(idRegister reg in _registers)
			{
				reg.GetFromRegisters(registers);
			}
		}

		public void Reset()
		{
			_registers.Clear();
		}
		#endregion
		#endregion
	}
}