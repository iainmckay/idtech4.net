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

namespace idTech4
{
	/// <summary>
	/// This is a dictionary class that tracks an arbitrary number of key / value
	/// pair combinations. It is used for map entity spawning, GUI state management,
	/// and other things.
	/// </summary>
	/// <remarks>
	/// Keys are compared case-insensitive.
	/// </remarks>
	public sealed class idDict
	{
		#region Members
		private Dictionary<string, string> _dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		#endregion 

		#region Constructor
		public idDict()
		{
			
		}
		#endregion

		#region Methods
		#region Public
		public void Clear()
		{
			_dict.Clear();
		}

		public bool ContainsKey(string key)
		{
			return _dict.ContainsKey(key);
		}

		public bool GetBool(string key, bool defaultValue = false)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				if(str.ToString() == "1")
				{
					return true;
				}
				else if(str.ToString() == "0")
				{
					return false;
				}
				else
				{
					bool tmp;
					bool.TryParse(str.ToString(), out tmp);

					return tmp;
				}
			}

			return defaultValue;
		}
		
		public float GetFloat(string key, float defaultValue = 0)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				float tmp;
				float.TryParse(str, out tmp);

				return tmp;
			}

			return defaultValue;
		}

		public int GetInteger(string key, int defaultValue = 0)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				int tmp;
				int.TryParse(str, out tmp);

				return tmp;
			}

			return defaultValue;
		}

		public string GetString(string key)
		{
			return GetString(key, string.Empty);
		}

		public string GetString(string key, string defaultString)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				return str;
			}

			return defaultString;
		}

		public Vector2 GetVector2(string key)
		{
			return GetVector2(key, Vector2.Zero);
		}

		public Vector2 GetVector2(string key, Vector2 defaultValue)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				return idHelper.ParseVector2(str);
			}

			return defaultValue;
		}

		public Vector3 GetVector3(string key)
		{
			return GetVector3(key, Vector3.Zero);
		}

		public Vector3 GetVector3(string key, Vector3 defaultValue)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				return idHelper.ParseVector3(str);
			}

			return defaultValue;
		}

		public Vector4 GetVector4(string key)
		{
			return GetVector4(key, Vector4.Zero);
		}

		public Vector4 GetVector4(string key, Vector4 defaultValue)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				return idHelper.ParseVector4(str);
			}

			return defaultValue;
		}

		public idRectangle GetRectangle(string key)
		{
			return GetRectangle(key, idRectangle.Empty);
		}

		public idRectangle GetRectangle(string key, idRectangle defaultValue)
		{
			string str;

			if(_dict.TryGetValue(key, out str) == true)
			{
				return idHelper.ParseRectangle(str);
			}

			return defaultValue;
		}

		public IEnumerable<KeyValuePair<string, string>> MatchPrefix(string prefix)
		{
			return _dict.Where(o => o.Key.StartsWith(prefix));
		}

		public void Remove(string key)
		{
			_dict.Remove(key);
		}

		public void Remove(string[] keys)
		{
			foreach(string key in keys)
			{
				Remove(key);
			}
		}

		public void Set(string key, string value)
		{
			if((key == null) || (key == string.Empty))
			{
				return;
			}

			if(_dict.ContainsKey(key) == true)
			{
				_dict[key] = value;
			}
			else
			{
				_dict.Add(key, value);
			}
		}

		public void Set(string key, int value)
		{
			Set(key, value.ToString());
		}

		public void Set(string key, float value)
		{
			Set(key, value.ToString());
		}

		public void Set(string key, bool value)
		{
			Set(key, value.ToString());
		}

		public void Set(string key, Vector2 value)
		{
			Set(key, string.Format("{0} {1}", value.X, value.Y));
		}

		public void Set(string key, Vector3 value)
		{
			Set(key, string.Format("{0} {1} {2}", value.X, value.Y, value.Z));
		}

		public void Set(string key, Vector4 value)
		{
			Set(key, string.Format("{0} {1} {2} {3}", value.X, value.Y, value.Z, value.W));
		}

		public void Set(string key, idRectangle value)
		{
			Set(key, string.Format("{0} {1} {2} {3}", value.X, value.Y, value.Width, value.Height));
		}

		/// <summary>
		/// Copy key/value pairs from other another dict not present in this instance.
		/// </summary>
		/// <param name="dict"></param>
		public void SetDefaults(idDict dict)
		{
			foreach(KeyValuePair<string, string> kvp in dict._dict)
			{
				if(_dict.ContainsKey(kvp.Key) == false)
				{
					_dict.Add(kvp.Key, kvp.Value);
				}
			}
		}

		public void TransferKeyValues(idDict source)
		{
			_dict.Clear();

			foreach(KeyValuePair<string, string> kvp in source._dict)
			{
				_dict.Add(kvp.Key, kvp.Value);
			}
		}
		#endregion
		#endregion
	}
}