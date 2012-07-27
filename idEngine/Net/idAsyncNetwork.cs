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

namespace idTech4.Net
{
	public sealed class idAsyncNetwork
	{
		#region Properties
		public idAsyncClient Client
		{
			get
			{
				return _client;
			}
		}

		public bool IsActive
		{
			get
			{
				return ((this.Server.IsActive == true) || (this.Client.IsActive == true));
			}
		}

		public idAsyncServer Server
		{
			get
			{
				return _server;
			}
		}
		#endregion

		#region Members
		private int _realTime;

		private idAsyncClient _client;
		private idAsyncServer _server;
		#endregion

		#region Constructor
		public idAsyncNetwork()
		{
			_client = new idAsyncClient();
			_server = new idAsyncServer();

			new idCvar("net_verbose", "0", 0, 2, "1 = verbose output, 2 = even more verbose output", new ArgCompletion_Integer(0, 2), CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_allowCheats", "0", "Allow cheats in network game", CvarFlags.System | CvarFlags.Bool | CvarFlags.NetworkSync);
#if ID_DEDICATED
			// dedicated executable can only have a value of 1 for net_serverDedicated
			new idCvar("net_serverDedicated", "1", "", CvarFlags.ServerInfo | CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat | CvarFlags.ReadOnly);
#else
			new idCvar("net_serverDedicated", "0", 0, 2, "1 = text console dedicated server, 2 = graphical dedicated server", new ArgCompletion_Integer(0, 2), CvarFlags.ServerInfo | CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
#endif
			new idCvar("net_serverSnapshotDelay", "50", "delay between snapshots in milliseconds", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_serverMaxClientRate", "16000", "maximum rate to a client in bytes/sec", CvarFlags.System | CvarFlags.Integer | CvarFlags.Archive | CvarFlags.NoCheat);
			new idCvar("net_clientMaxRate", "16000", "maximum rate requested by client from server in bytes/sec", CvarFlags.System | CvarFlags.Integer | CvarFlags.Archive | CvarFlags.NoCheat);
			new idCvar("net_serverMaxUsercmdRelay", "5", 1, idE.MaxUserCommandRelay, "maximum number of usercmds from other clients the server relays to a client", new ArgCompletion_Integer(0, idE.MaxUserCommandRelay), CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_serverZombieTimeout", "5", "disconnected client timeout in seconds", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_serverClientTimeout", "40", "client time out in seconds", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_clientServerTimeout", "40", "server time out in seconds", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_serverDrawClient", "-1", "number of client for which to draw view on server", CvarFlags.System | CvarFlags.Integer);
			new idCvar("net_serverRemoteConsolePassword", "", "remote console password", CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("net_clientPrediction", "16", "additional client side prediction in milliseconds", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_clientMaxPrediction", "1000", "maximum number of milliseconds a client can predict ahead of server.", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_clientUsercmdBackup", "5", "number of usercmds to resend", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_clientRemoteConsoleAddress", "localhost", "remote console address", CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("net_clientRemoteConsolePassword", "", "remote console password", CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("net_master0", string.Format("{0}:{1}", idE.MasterServerAddress, idE.MasterServerPort), "idnet master server address", CvarFlags.System | CvarFlags.ReadOnly);
			new idCvar("net_master1", "", "1st master server address", CvarFlags.System | CvarFlags.Archive);
			new idCvar("net_master2", "", "2nd master server address", CvarFlags.System | CvarFlags.Archive);
			new idCvar("net_master3", "", "3rd master server address", CvarFlags.System | CvarFlags.Archive);
			new idCvar("net_master4", "", "4th master server address", CvarFlags.System | CvarFlags.Archive);
			new idCvar("net_LANServer", "0", "config LAN games only - affects clients and servers", CvarFlags.System | CvarFlags.Bool | CvarFlags.NoCheat);
			new idCvar("net_serverReloadEngine", "0", "perform a full reload on next map restart (including flushing referenced pak files) - decreased if > 0", CvarFlags.System | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("net_serverAllowServerMod", "0", "allow server-side mods", CvarFlags.System | CvarFlags.Bool | CvarFlags.NoCheat);
			new idCvar("si_idleServer", "0", "game clients are idle", CvarFlags.System | CvarFlags.Bool | CvarFlags.Init | CvarFlags.ServerInfo);
			new idCvar("net_clientDownload", "1", "client pk4 downloads policy: 0 - never, 1 - ask, 2 - always (will still prompt for binary code)", CvarFlags.System | CvarFlags.Integer | CvarFlags.Archive);
		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			_realTime = 0;

			// TODO: masters + cmds
			/*memset( masters, 0, sizeof( masters ) );
			masters[0].var = &master0;
			masters[1].var = &master1;
			masters[2].var = &master2;
			masters[3].var = &master3;
			masters[4].var = &master4;

		#ifndef	ID_DEMO_BUILD
			cmdSystem->AddCommand( "spawnServer", SpawnServer_f, CMD_FL_SYSTEM, "spawns a server", idCmdSystem::ArgCompletion_MapName );
			cmdSystem->AddCommand( "nextMap", NextMap_f, CMD_FL_SYSTEM, "loads the next map on the server" );
			cmdSystem->AddCommand( "connect", Connect_f, CMD_FL_SYSTEM, "connects to a server" );
			cmdSystem->AddCommand( "reconnect", Reconnect_f, CMD_FL_SYSTEM, "reconnect to the last server we tried to connect to" );
			cmdSystem->AddCommand( "serverInfo", GetServerInfo_f, CMD_FL_SYSTEM, "shows server info" );
			cmdSystem->AddCommand( "LANScan", GetLANServers_f, CMD_FL_SYSTEM, "scans LAN for servers" );
			cmdSystem->AddCommand( "listServers", ListServers_f, CMD_FL_SYSTEM, "lists scanned servers" );
			cmdSystem->AddCommand( "rcon", RemoteConsole_f, CMD_FL_SYSTEM, "sends remote console command to server" );
			cmdSystem->AddCommand( "heartbeat", Heartbeat_f, CMD_FL_SYSTEM, "send a heartbeat to the the master servers" );
			cmdSystem->AddCommand( "kick", Kick_f, CMD_FL_SYSTEM, "kick a client by connection number" );
			cmdSystem->AddCommand( "checkNewVersion", CheckNewVersion_f, CMD_FL_SYSTEM, "check if a new version of the game is available" );
			cmdSystem->AddCommand( "updateUI", UpdateUI_f, CMD_FL_SYSTEM, "internal - cause a sync down of game-modified userinfo" );
		#endif*/
		}
		#endregion
		#endregion
	}
}