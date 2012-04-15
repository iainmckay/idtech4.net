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

using Microsoft.Xna.Framework;

namespace idTech4.UI
{
	public sealed class idRegister
	{
		#region Properties
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public int[] Indexes
		{
			get
			{
				return _indexes;
			}
			set
			{
				_indexes = value;
			}
		}

		public idWindowVariable Variable
		{
			get
			{
				return _var;
			}
			set
			{
				_var = value;
			}
		}
		#endregion

		#region Members
		private bool _enabled;
		private RegisterType _type;
		private string _name;
		private int _registerCount;
		private int[] _indexes = new int[4];
		private idWindowVariable _var;

		public static int[] RegisterTypeCount = new int[] { 4, 1, 1, 1, 0, 2, 3, 4 };
		#endregion

		#region Constructor
		public idRegister()
		{

		}

		public idRegister(string name, RegisterType type, idWindowVariable var)
		{
			_name = name;
			_type = type;
			_var = var;

			_registerCount = RegisterTypeCount[(int) type];
			_enabled = (_type == RegisterType.String) ? false : true;
		}
		#endregion

		#region Methods
		#region Public
		public void SetToRegisters(ref float[] registers)
		{
			Vector4 v = Vector4.Zero;
			Vector2 v2 = Vector2.Zero;
			Vector3 v3 = Vector3.Zero;
			Rectangle rect = Rectangle.Empty;

			if((_enabled == false) || (_var == null) || ((_var != null) && ((_var.Dictionary != null) || (_var.Evaluate == false))))
			{
				// TODO: this seems to be breaking var parsing.  maybe it depends on other code that hasn't been ported.
				return;
			}

			switch(_type)
			{
				case RegisterType.Vector4:
					v = (idWinVector4) _var;
					break;

				case RegisterType.Rectangle:
					rect = (idWinRectangle) _var;
					v = new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
					break;

				case RegisterType.Vector2:
					v2 = (idWinVector2) _var;
					v.X = v2.X;
					v.Y = v2.Y;
					break;

				case RegisterType.Vector3:
					v3 = (idWinVector3) _var;
					v.X = v3.X;
					v.Y = v3.Y;
					v.Z = v3.Z;
					break;

				case RegisterType.Float:
					v.X = (idWinFloat) _var;
					break;

				case RegisterType.Integer:
					v.X = (idWinInteger) _var;
					break;

				case RegisterType.Bool:
					v.X = (((idWinBool) _var) == true) ? 1 : 0;
					break;
			}

			registers[_indexes[0]] = v.X;
			registers[_indexes[1]] = v.Y;
			registers[_indexes[2]] = v.Z;
			registers[_indexes[3]] = v.W;
		}

		public void GetFromRegisters(float[] registers)
		{
			Vector4 v;
			Rectangle rect;

			if((_enabled == false) || (_var == null) || ((_var != null) && ((_var.Dictionary != null) || (_var.Evaluate == false))))
			{
				// TODO: this seems to be breaking var parsing.  maybe it depends on other code that hasn't been ported.
				//return;
			}
			
			v.X = registers[_indexes[0]];
			v.Y = registers[_indexes[1]];
			v.Z = registers[_indexes[2]];
			v.W = registers[_indexes[3]];

			switch(_type)
			{
				case RegisterType.Vector4:
					((idWinVector4) _var).Set(v);
					break;

				case RegisterType.Rectangle:
					rect.X = (int) v.X;
					rect.Y = (int) v.Y;
					rect.Width = (int) v.Z;
					rect.Height = (int) v.W;
					
					((idWinRectangle) _var).Set(rect);
					break;

				case RegisterType.Vector2:
					((idWinVector2) _var).Set(new Vector2(v.X, v.Y));
					break;

				case RegisterType.Vector3:
					((idWinVector3) _var).Set(new Vector3(v.X, v.Y, v.Z));
					break;

				case RegisterType.Float:
					((idWinFloat) _var).Set(v.X);
					break;

				case RegisterType.Integer:
					((idWinInteger) _var).Set((int) v.X);
					break;

				case RegisterType.Bool:
					((idWinBool) _var).Set(v.X != 0.0f);
					break;
			}
		}
		#endregion
		#endregion
	}

	public enum RegisterType
	{
		Vector4,
		Float,
		Bool,
		Integer,
		String,
		Vector2,
		Vector3,
		Rectangle,
		Count
	}
}