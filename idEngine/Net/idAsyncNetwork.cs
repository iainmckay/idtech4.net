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
	internal sealed class idAsyncNetwork
	{
		#region Properties
		public bool IsActive
		{
			get
			{
				// TODO: return (server.IsActive() || client.IsActive());
				return false;
			}
		}
		#endregion

		#region Constructor
		public idAsyncNetwork()
		{
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
	}
}