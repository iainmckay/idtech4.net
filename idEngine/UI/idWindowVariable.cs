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

using idTech4.Renderer;

namespace idTech4.UI
{
	public abstract class idWindowVariable
	{
		#region Constants
		public const string Prefix = "gui::";
		#endregion

		#region Properties
		public idDict Dictionary
		{
			get
			{
				return _guiDict;
			}
		}

		public bool Evaluate
		{
			get
			{
				return _eval;
			}
			set
			{
				_eval = value;
			}
		}

		public string Name
		{
			get
			{
				if(_name != null)
				{
					if((_guiDict != null) && (_name == "*"))
					{
						return _guiDict.GetString(_name[1].ToString());
					}

					return _name;
				}

				return string.Empty;
			}

			set
			{
				_name = null;

				if(value != null)
				{
					_name = value;
				}
			}
		}

		public bool NeedsUpdate
		{
			get
			{
				return (_guiDict != null);
			}
		}

		public abstract float X
		{
			get;
		}
		#endregion

		#region Members
		protected idDict _guiDict;
		protected string _name;
		protected bool _eval;
		#endregion

		#region Constructor
		public idWindowVariable()
		{

		}

		public idWindowVariable(string name)
		{
			_name = name;
		}
		#endregion

		#region Methods
		#region Public
		public virtual void Init(string name, idWindow win)
		{
			string key = name;
			int length = key.Length;

			_guiDict = null;

			if((length > Prefix.Length) && (key.StartsWith(Prefix) == true))
			{
				key = key.Substring(length - Prefix.Length);

				SetGuiInfo(win.UserInterface.State, key);
				win.AddUpdateVariable(this);
			}
			else
			{
				Set(name);
			}
		}

		public abstract void Update();
		public abstract void Set(string value);
		#endregion

		#region Protected
		protected virtual void SetGuiInfo(idDict dict, string name)
		{
			_guiDict = dict;
			this.Name = name;
		}
		#endregion
		#endregion
	}

	public sealed class idWinBool : idWindowVariable
	{
		#region Members
		private bool _data;
		#endregion

		#region Constructor
		public idWinBool(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return ((_data == true) ? 1.0f : 0);
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public void Set(bool value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Set(string value)
		{
			bool.TryParse(value, out _data);

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				_data = _guiDict.GetBool(s);
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return ((_data == true) ? 1 : 0).ToString();
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinBool b1, bool b2)
		{
			return (b1._data == b2);
		}

		public static bool operator !=(idWinBool b1, bool b2)
		{
			return (b1._data != b2);
		}

		public static implicit operator bool(idWinBool v)
		{
			return v._data;
		}
		#endregion
	}

	public sealed class idWinString : idWindowVariable
	{
		#region Members
		private string _data;
		#endregion

		#region Constructor
		public idWinString(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return ((_data.Length > 0) ? 1.0f : 0);
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			if(_guiDict != null)
			{
				_data = _guiDict.GetString(this.Name);
			}
		}

		public override void Set(string value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				_data = _guiDict.GetString(s);
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data;
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinString v1, string v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinString v1, string v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator string(idWinString v)
		{
			return v._data;
		}
		#endregion
	}

	public sealed class idWinInteger : idWindowVariable
	{
		#region Members
		private int _data;
		#endregion

		#region Constructor
		public idWinInteger(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return _data;
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public void Set(int value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Set(string value)
		{
			int.TryParse(value, out _data);

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				_data = _guiDict.GetInteger(s);
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data.ToString();
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinInteger v1, float v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinInteger v1, float v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator float(idWinInteger v)
		{
			return v._data;
		}
		#endregion
	}

	public sealed class idWinFloat : idWindowVariable
	{
		#region Members
		private float _data;
		#endregion

		#region Constructor
		public idWinFloat(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return _data;
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public void Set(float value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Set(string value)
		{
			float.TryParse(value, out _data);

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				_data = _guiDict.GetFloat(s);
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data.ToString();
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinFloat v1, float v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinFloat v1, float v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator float(idWinFloat v)
		{
			return v._data;
		}
		#endregion
	}

	public sealed class idWinRectangle : idWindowVariable
	{
		#region Properties
		public Rectangle Data
		{
			get
			{
				return _data;
			}
		}

		public float Y
		{
			get
			{
				return _data.Y;
			}
		}

		public float Width
		{
			get
			{
				return _data.Width;
			}
		}

		public float Height
		{
			get
			{
				return _data.Height;
			}
		}
		#endregion

		#region Members
		private Rectangle _data;
		#endregion

		#region Constructor
		public idWinRectangle(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return _data.X;
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public void Set(Rectangle value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Set(string value)
		{
			_data = idHelper.ParseRectangle(value);

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				Rectangle r = _guiDict.GetRectangle(this.Name);
				_data.X = r.X;
				_data.Y = r.Y;
				_data.Width = r.Width;
				_data.Height = r.Height;
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data.ToString();
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinRectangle v1, Rectangle v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinRectangle v1, Rectangle v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator Rectangle(idWinRectangle v)
		{
			return v._data;
		}
		#endregion
	}

	public sealed class idWinVector2 : idWindowVariable
	{
		#region Properties
		public float Y
		{
			get
			{
				return _data.Y;
			}
		}
		#endregion

		#region Members
		private Vector2 _data;
		#endregion

		#region Constructor
		public idWinVector2(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return _data.X;
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public void Set(Vector2 value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Set(string value)
		{
			_data = idHelper.ParseVector2(value);

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				Vector2 v = _guiDict.GetVector2(this.Name);
				_data.X = v.X;
				_data.Y = v.Y;
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data.ToString();
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinVector2 v1, Vector2 v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinVector2 v1, Vector2 v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator Vector2(idWinVector2 v)
		{
			return v._data;
		}
		#endregion
	}

	public sealed class idWinVector3 : idWindowVariable
	{
		#region Properties
		public float Y
		{
			get
			{
				return _data.Y;
			}
		}

		public float Z
		{
			get
			{
				return _data.Z;
			}
		}
		#endregion

		#region Members
		private Vector3 _data;
		#endregion

		#region Constructor
		public idWinVector3(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return _data.X;
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public void Set(Vector3 value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Set(string value)
		{
			_data = idHelper.ParseVector3(value);

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				Vector3 v = _guiDict.GetVector3(this.Name);
				_data.X = v.X;
				_data.Y = v.Y;
				_data.Z = v.Z;
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data.ToString();
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinVector3 v1, Vector3 v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinVector3 v1, Vector3 v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator Vector3(idWinVector3 v)
		{
			return v._data;
		}
		#endregion
	}

	public sealed class idWinVector4 : idWindowVariable
	{
		#region Properties
		public float Y
		{
			get
			{
				return _data.Y;
			}
		}

		public float Z
		{
			get
			{
				return _data.Z;
			}
		}

		public float W
		{
			get
			{
				return _data.W;
			}
		}
		#endregion

		#region Members
		private Vector4 _data;
		#endregion

		#region Constructor
		public idWinVector4(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return _data.X;
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public void Set(Vector4 value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Set(string value)
		{
			_data = idHelper.ParseVector4(value);

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				Vector4 v = _guiDict.GetVector4(this.Name);
				_data.X = v.X;
				_data.Y = v.Y;
				_data.Z = v.Z;
				_data.W = v.W;
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data.ToString();
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinVector4 v1, Vector4 v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinVector4 v1, Vector4 v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator Vector4(idWinVector4 v)
		{
			return v._data;
		}

		public static implicit operator Color(idWinVector4 v)
		{
			return new Color(v._data);
		}
		#endregion
	}

	public sealed class idWinBackground : idWindowVariable
	{
		#region Properties
		public idMaterial Material
		{
			get
			{
				return _material;
			}
			set
			{
				_material = value;
			}
		}
		#endregion

		#region Members
		private string _data;
		private idMaterial _material;
		#endregion

		#region Constructor
		public idWinBackground(string name)
			: base(name)
		{

		}
		#endregion

		#region idWindowVariable implementation
		#region Properties
		public override float X
		{
			get
			{
				return ((_data.Length > 0) ? 1.0f : 0);
			}
		}

		#region Methods
		#region Public
		public override void Init(string name, idWindow win)
		{
			base.Init(name, win);

			Update();
		}

		public override void Set(string value)
		{
			_data = value;

			if(_guiDict != null)
			{
				_guiDict.Set(this.Name, _data);
			}

			if(_material != null)
			{
				if(_data == string.Empty)
				{
					_material = null;
				}
				else
				{
					_material = idE.DeclManager.FindMaterial(_data);
				}
			}
		}

		public override void Update()
		{
			string s = this.Name;

			if((_guiDict != null) && (s != string.Empty))
			{
				_data = _guiDict.GetString(s);

				if(_data == string.Empty)
				{
					_material = null;
				}
				else
				{
					_material = idE.DeclManager.FindMaterial(_data);
				}
			}
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region Overloads
		public override string ToString()
		{
			return _data;
		}

		public override bool Equals(object obj)
		{
			return _data.Equals(obj);
		}

		public static bool operator ==(idWinBackground v1, string v2)
		{
			return (v1._data == v2);
		}

		public static bool operator !=(idWinBackground v1, string v2)
		{
			return (v1._data != v2);
		}

		public static implicit operator string(idWinBackground v)
		{
			return v._data;
		}
		#endregion
	}
}