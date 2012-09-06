using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using idTech4.Renderer;

namespace idTech4.Game.Animation
{
	public class idAnimManager : IDisposable
	{
		#region Members
		private Dictionary<string, idMD5Anim> _animations = new Dictionary<string, idMD5Anim>();
		private List<string> _jointNames = new List<string>();
		#endregion

		#region Constructor
		public idAnimManager()
		{

		}

		~idAnimManager()
		{
			Dispose(false);
		}
		#endregion

		#region Constructor
		#region Public
		public idMD5Anim GetAnimation(string name)
		{
			idMD5Anim anim;

			if(_animations.ContainsKey(name) == true)
			{
				anim = _animations[name];
			}
			else
			{
				if(Path.GetExtension(name) != idRenderModel_MD5.MeshAnimationExtension)
				{
					return null;
				}

				anim = new idMD5Anim();

				if(anim.LoadAnimation(name) == false)
				{
					idConsole.Warning("Couldn't load anim: '{0}'", name);
					anim = null;
				}

				_animations.Add(name, anim);
			}

			return anim;
		}

		public int GetJointIndex(string name)
		{
			int count = _jointNames.Count;

			for(int i = 0; i < count; i++)
			{
				if(_jointNames[i].Equals(name) == true)
				{
					return i;
				}
			}

			int index = _jointNames.Count;
			_jointNames.Add(name);

			return index;
		}

		public string GetJointName(int index)
		{
			return _jointNames[index];
		}
		#endregion
		#endregion

		#region IDisposable implementation
		#region Properties
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion

		#region Members
		private bool _disposed;
		#endregion

		#region Methods
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(disposing == true)
			{
				_animations.Clear();
				_jointNames.Clear();
			}

			_disposed = true;
		}
		#endregion
		#endregion
	}
}
