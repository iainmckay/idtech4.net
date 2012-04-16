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

namespace idTech4.Sound
{
	public class idSoundSystem
	{
		#region Constructor
		public idSoundSystem()
		{
			InitCvars();
		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			// TODO
			idConsole.WriteLine("----- Initializing Sound System ------");

			/*isInitialized = false;
			muted = false;
			shutdown = false;

			currentSoundWorld = NULL;
			soundCache = NULL;

			olddwCurrentWritePos = 0;
			buffers = 0;
			CurrentSoundTime = 0;

			nextWriteBlock = 0xffffffff;

			memset( meterTops, 0, sizeof( meterTops ) );
			memset( meterTopsTime, 0, sizeof( meterTopsTime ) );

			for( int i = -600; i < 600; i++ ) {
				float pt = i * 0.1f;
				volumesDB[i+600] = pow( 2.0f,( pt * ( 1.0f / 6.0f ) ) );
			}

			// make a 16 byte aligned finalMixBuffer
			finalMixBuffer = (float *) ( ( ( (int)realAccum ) + 15 ) & ~15 );

			graph = NULL;

			if ( !s_noSound.GetBool() ) {
				idSampleDecoder::Init();
				soundCache = new idSoundCache();
			}

			// set up openal device and context
			common->StartupVariable( "s_useOpenAL", true );
			common->StartupVariable( "s_useEAXReverb", true );

			if ( idSoundSystemLocal::s_useOpenAL.GetBool() || idSoundSystemLocal::s_useEAXReverb.GetBool() ) {
				if ( !Sys_LoadOpenAL() ) {
					idSoundSystemLocal::s_useOpenAL.SetBool( false );
				} else {
					common->Printf( "Setup OpenAL device and context... " );
					openalDevice = alcOpenDevice( NULL );
					openalContext = alcCreateContext( openalDevice, NULL );
					alcMakeContextCurrent( openalContext );
					common->Printf( "Done.\n" );

					// try to obtain EAX extensions
					if ( idSoundSystemLocal::s_useEAXReverb.GetBool() && alIsExtensionPresent( ID_ALCHAR "EAX4.0" ) ) {
						idSoundSystemLocal::s_useOpenAL.SetBool( true );	// EAX presence causes AL enable
						alEAXSet = (EAXSet)alGetProcAddress( ID_ALCHAR "EAXSet" );
						alEAXGet = (EAXGet)alGetProcAddress( ID_ALCHAR "EAXGet" );
						common->Printf( "OpenAL: found EAX 4.0 extension\n" );
					} else {
						common->Printf( "OpenAL: EAX 4.0 extension not found\n" );
						idSoundSystemLocal::s_useEAXReverb.SetBool( false );
						alEAXSet = (EAXSet)NULL;
						alEAXGet = (EAXGet)NULL;
					}

					// try to obtain EAX-RAM extension - not required for operation
					if ( alIsExtensionPresent( ID_ALCHAR "EAX-RAM" ) == AL_TRUE ) {
						alEAXSetBufferMode = (EAXSetBufferMode)alGetProcAddress( ID_ALCHAR "EAXSetBufferMode" );
						alEAXGetBufferMode = (EAXGetBufferMode)alGetProcAddress( ID_ALCHAR "EAXGetBufferMode" );
						common->Printf( "OpenAL: found EAX-RAM extension, %dkB\\%dkB\n", alGetInteger( alGetEnumValue( ID_ALCHAR "AL_EAX_RAM_FREE" ) ) / 1024, alGetInteger( alGetEnumValue( ID_ALCHAR "AL_EAX_RAM_SIZE" ) ) / 1024 );
					} else {
						alEAXSetBufferMode = (EAXSetBufferMode)NULL;
						alEAXGetBufferMode = (EAXGetBufferMode)NULL;
						common->Printf( "OpenAL: no EAX-RAM extension\n" );
					}

					if ( !idSoundSystemLocal::s_useOpenAL.GetBool() ) {
						common->Printf( "OpenAL: disabling ( no EAX ). Using legacy mixer.\n" );

						alcMakeContextCurrent( NULL );
		
						alcDestroyContext( openalContext );
						openalContext = NULL;
		
						alcCloseDevice( openalDevice );
						openalDevice = NULL;
					} else {

						ALuint handle;		
						openalSourceCount = 0;
				
						while ( openalSourceCount < 256 ) {
							alGetError();
							alGenSources( 1, &handle );
							if ( alGetError() != AL_NO_ERROR ) {
								break;
							} else {
								// store in source array
								openalSources[openalSourceCount].handle = handle;
								openalSources[openalSourceCount].startTime = 0;
								openalSources[openalSourceCount].chan = NULL;
								openalSources[openalSourceCount].inUse = false;
								openalSources[openalSourceCount].looping = false;

								// initialise sources
								alSourcef( handle, AL_ROLLOFF_FACTOR, 0.0f );

								// found one source
								openalSourceCount++;
							}
						}

						common->Printf( "OpenAL: found %s\n", alcGetString( openalDevice, ALC_DEVICE_SPECIFIER ) );
						common->Printf( "OpenAL: found %d hardware voices\n", openalSourceCount );

						// adjust source count to allow for at least eight stereo sounds to play
						openalSourceCount -= 8;

						EAXAvailable = 1;
					}
				}
			}

			useOpenAL = idSoundSystemLocal::s_useOpenAL.GetBool();
			useEAXReverb = idSoundSystemLocal::s_useEAXReverb.GetBool();

			cmdSystem->AddCommand( "listSounds", ListSounds_f, CMD_FL_SOUND, "lists all sounds" );
			cmdSystem->AddCommand( "listSoundDecoders", ListSoundDecoders_f, CMD_FL_SOUND, "list active sound decoders" );
			cmdSystem->AddCommand( "reloadSounds", SoundReloadSounds_f, CMD_FL_SOUND|CMD_FL_CHEAT, "reloads all sounds" );
			cmdSystem->AddCommand( "testSound", TestSound_f, CMD_FL_SOUND | CMD_FL_CHEAT, "tests a sound", idCmdSystem::ArgCompletion_SoundName );
			cmdSystem->AddCommand( "s_restart", SoundSystemRestart_f, CMD_FL_SOUND, "restarts the sound system" );*/

			idConsole.WriteLine("sound system initialized.");
			idConsole.WriteLine("--------------------------------------");
		}
		#endregion

		#region Private
		private void InitCvars()
		{
#if ID_DEDICATED
new idCvar("s_noSound", "1", CVAR_SOUND | CVAR_BOOL | CVAR_ROM);
#else
			new idCvar("s_noSound", "0", "", CvarFlags.Sound | CvarFlags.Bool | CvarFlags.NoCheat);
#endif
			new idCvar("s_quadraticFalloff", "1", "", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_drawSounds", "0", 0, 2, "", new ArgCompletion_Integer(0, 2), CvarFlags.Sound | CvarFlags.Integer);
			new idCvar("s_showStartSound", "0", "", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_useOcclusion", "1", "", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_maxSoundsPerShader", "0", 0, 10, "", new ArgCompletion_Integer(0, 10), CvarFlags.Sound | CvarFlags.Archive);
			new idCvar("s_showLevelMeter", "0", "", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_constantAmplitude", "-1", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_minVolume6", "0", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_dotbias6", "0.8", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_minVolume2", "0.25", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_dotbias2", "1.1", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_spatializationDecay", "2", "", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("s_reverse", "0", "", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("s_meterTopTime", "2000", "", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("s_volume_dB", "0", "volume in dB", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("s_playDefaultSound", "1", "play a beep for missing sounds", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("s_subFraction", "0.75", "volume to subwoofer in 5.1", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("s_globalFraction", "0.8", "volume to all speakers when not spatialized", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("s_doorDistanceAdd", "150", "reduce sound volume with this distance when going through a door", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("s_singleEmitter", "0", "mute all sounds but this emitter", CvarFlags.Sound | CvarFlags.Integer);
			new idCvar("s_numberOfSpeakers", "2", "number of speakers", CvarFlags.Sound | CvarFlags.Archive);
			new idCvar("s_force22kHz", "0", "", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_clipVolumes", "1", "", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_realTimeDecoding", "1", "", CvarFlags.Sound | CvarFlags.Bool | CvarFlags.Init);

			new idCvar("s_slowAttenuate", "1", "slowmo sounds attenuate over shorted distance", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_enviroSuitCutoffFreq", "2000", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_enviroSuitCutoffQ", "2", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_reverbTime", "1000", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_reverbFeedback", "0.333", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_enviroSuitVolumeScale", "0.9", "", CvarFlags.Sound | CvarFlags.Float);
			new idCvar("s_skipHelltimeFX", "0", "", CvarFlags.Sound | CvarFlags.Bool);

			new idCvar("s_libOpenAL", "openal32.dll", "OpenAL DLL name/path", CvarFlags.Sound | CvarFlags.Archive);
			new idCvar("s_useOpenAL", "0", "use OpenAL", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("s_useEAXReverb", "0", "use EAX reverb", CvarFlags.Sound | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("s_muteEAXReverb", "0", "mute eax reverb", CvarFlags.Sound | CvarFlags.Bool);
			new idCvar("s_decompressionLimit", "6", "specifies maximum uncompressed sample length in seconds", CvarFlags.Sound | CvarFlags.Integer | CvarFlags.Archive);
		}
		#endregion
		#endregion
	}
}
