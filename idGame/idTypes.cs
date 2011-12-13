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