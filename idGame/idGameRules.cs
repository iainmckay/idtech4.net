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

using idTech4.Game.Entities;

namespace idTech4.Game
{
	public abstract class idGameRules : IDisposable
	{
		#region Members
	
		#endregion

		#region Constructor
		public idGameRules()
		{

		}
		#endregion

		#region Methods
		public virtual void ClientConnect(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}
		}

		public virtual void EnterGame(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}

			if(idR.Game.PlayerStates[clientIndex].InGame == false)
			{
				idR.Game.PlayerStates[clientIndex].InGame = true;
			}
		}

		public virtual void Run()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}
		}

		public virtual bool Draw(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}

			idPlayer player = idR.Game.Entities[clientIndex] as idPlayer;

			if(player == null)
			{
				return false;
			}

			// render the scene
			player.View.Draw(player.Hud);

			return true;
		}

		public virtual bool UserInfoChanged(int clientIndex, bool canModify)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}

			return false;
		}

		public virtual bool IsInGame(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}

			if((clientIndex > idR.Game.PlayerStates.Length) || (clientIndex < 0))
			{
				return false;
			}

			return idR.Game.PlayerStates[clientIndex].InGame;
		}

		public virtual void SpawnPlayer(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}

			bool inGame = idR.Game.PlayerStates[clientIndex].InGame;

			idR.Game.PlayerStates[clientIndex].Clear();

			if(idR.Game.IsClient == false)
			{
				idR.Game.PlayerStates[clientIndex].InGame = inGame;
			}
		}

		public virtual void MapPopulate()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}
		}

		public virtual void Reset()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}
		}

		public virtual void Precache()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameType");
			}
		}
		#endregion

		#region IDisposable implementation
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			_disposed = true;
		}
		#endregion
	}

	public enum PlayerVote
	{
		None,
		No,
		Yes,
		Wait
	}

	public class PlayerState
	{
		public int Ping;
		public int FragCount;
		public int TeamFragCount;
		public int Wins;
		public PlayerVote Vote;
		public bool IsScoreBoardUp;
		public bool InGame;

		public void Clear()
		{
			Ping = 0;
			FragCount = 0;
			TeamFragCount = 0;
			Wins = 0;
			Vote = PlayerVote.None;
			IsScoreBoardUp = false;
			InGame = false;
		}
	}
}
