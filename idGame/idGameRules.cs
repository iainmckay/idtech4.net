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
				throw new ObjectDisposedException("idGameRules");
			}
		}

		public virtual void EnterGame(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameRules");
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
				throw new ObjectDisposedException("idGameRules");
			}
		}

		public virtual bool Draw(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameRules");
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
				throw new ObjectDisposedException("idGameRules");
			}

			return false;
		}

		public virtual bool IsInGame(int clientIndex)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameRules");
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
				throw new ObjectDisposedException("idGameRules");
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
				throw new ObjectDisposedException("idGameRules");
			}
		}

		public virtual void Reset()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameRules");
			}
		}

		public virtual void Precache()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameRules");
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
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idGameRules");
			}

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
