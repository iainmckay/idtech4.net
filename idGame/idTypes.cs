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

namespace idTech4.Game
{
	public class idStrings
	{
		public readonly static string[] GameTypes = new string[] {
			"singleplayer",
			"deathmatch",
			"Tourney",
			"Team DM",
			"Last Man"
		};

		public readonly static string[] Ready = new string[] {
			"Not Ready",
			"Ready"
		};

		public readonly static string[] Spectate = new string[] {
			"Play",
			"Spectate"
		};

		public readonly static string[] Skins = new string[] {
			"skins/characters/player/marine_mp", 
			"skins/characters/player/marine_mp_red", 
			"skins/characters/player/marine_mp_blue", 
			"skins/characters/player/marine_mp_green", 
			"skins/characters/player/marine_mp_yellow"
		};

		public readonly static string[] Teams = new string[] {
			"Red",
			"Blue"
		};

		public readonly static string[] GlobalSoundStrings = new string[] {
			"sound/feedback/voc_youwin.wav",
			"sound/feedback/voc_youlose.wav",
			"sound/feedback/fight.wav",
			"sound/feedback/vote_now.wav",
			"sound/feedback/vote_passed.wav",
			"sound/feedback/vote_failed.wav",
			"sound/feedback/three.wav",
			"sound/feedback/two.wav",
			"sound/feedback/one.wav",
			"sound/feedback/sudden_death.wav"
		};

		public readonly static string[] MultiplayerInterfaces = new string[] {
			"guis/mphud.gui",
			"guis/mpmain.gui",
			"guis/mpmsgmode.gui",
			"guis/netmenu.gui"
		};
	}

	public enum GameReliableMessage : int
	{
		InitDeclRemap = 0,
		RemapDecl,
		SpawnPlayer,
		DeleteEntity,
		Chat,
		TeamChat,
		SoundEvent,
		SoundIndex,
		Db,
		Kill,
		DropWeapon,
		Restart,
		ServerInfo,
		TourneyLine,
		CallVote,
		CastVote,
		StartVote,
		UpdateVote,
		PortalStates,
		Portal,
		VoiceChat,
		StartState,
		Menu,
		WarmpupTime,
		Event
	}

	public struct SpawnPoint
	{
		public idEntity Entity;
		public int Distance;
	}
}