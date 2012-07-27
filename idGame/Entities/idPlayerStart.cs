using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Game.Entities
{
	public class idPlayerStart : idEntity
	{
		#region Members
		private int _teleportStage;
		#endregion

		#region Constructor
		public idPlayerStart()
			: base()
		{

		}
		#endregion

		// TODO
		/*CLASS_DECLARATION( idEntity, idPlayerStart )
			EVENT( EV_Activate,			idPlayerStart::Event_TeleportPlayer )
			EVENT( EV_TeleportStage,	idPlayerStart::Event_TeleportStage )
		END_CLASS*/

		#region idEntity implementation
		public override void Spawn()
		{
			base.Spawn();

			_teleportStage = 0;
		}
		#endregion

		// TODO

		/*void idPlayerStart::Save( idSaveGame *savefile ) const {
	savefile->WriteInt( teleportStage );
}*/

		/*void idPlayerStart::Restore( idRestoreGame *savefile ) {
			savefile->ReadInt( teleportStage );
		}*/

		/*bool idPlayerStart::ClientReceiveEvent( int event, int time, const idBitMsg &msg ) {
			int entityNumber;

			switch( event ) {
				case EVENT_TELEPORTPLAYER: {
					entityNumber = msg.ReadBits( GENTITYNUM_BITS );
					idPlayer *player = static_cast<idPlayer *>( gameLocal.entities[entityNumber] );
					if ( player != NULL && player->IsType( idPlayer::Type ) ) {
						Event_TeleportPlayer( player );
					}
					return true;
				}
				default: {
					return idEntity::ClientReceiveEvent( event, time, msg );
				}
			}
			return false;
		}*/

		/*void idPlayerStart::Event_TeleportStage( idEntity *_player ) {
			idPlayer *player;
			if ( !_player->IsType( idPlayer::Type ) ) {
				common->Warning( "idPlayerStart::Event_TeleportStage: entity is not an idPlayer\n" );
				return;
			}
			player = static_cast<idPlayer*>(_player);
			float teleportDelay = spawnArgs.GetFloat( "teleportDelay" );
			switch ( teleportStage ) {
				case 0:
					player->playerView.Flash( colorWhite, 125 );
					player->SetInfluenceLevel( INFLUENCE_LEVEL3 );
					player->SetInfluenceView( spawnArgs.GetString( "mtr_teleportFx" ), NULL, 0.0f, NULL );
					gameSoundWorld->FadeSoundClasses( 0, -20.0f, teleportDelay );
					player->StartSound( "snd_teleport_start", SND_CHANNEL_BODY2, 0, false, NULL );
					teleportStage++;
					PostEventSec( &EV_TeleportStage, teleportDelay, player );
					break;
				case 1:
					gameSoundWorld->FadeSoundClasses( 0, 0.0f, 0.25f );
					teleportStage++;
					PostEventSec( &EV_TeleportStage, 0.25f, player );
					break;
				case 2:
					player->SetInfluenceView( NULL, NULL, 0.0f, NULL );
					TeleportPlayer( player );
					player->StopSound( SND_CHANNEL_BODY2, false );
					player->SetInfluenceLevel( INFLUENCE_NONE );
					teleportStage = 0;
					break;
				default:
					break;
			}
		}*/

		/*void idPlayerStart::TeleportPlayer( idPlayer *player ) {
			float pushVel = spawnArgs.GetFloat( "push", "300" );
			float f = spawnArgs.GetFloat( "visualEffect", "0" );
			const char *viewName = spawnArgs.GetString( "visualView", "" );
			idEntity *ent = viewName ? gameLocal.FindEntity( viewName ) : NULL;

			if ( f && ent ) {
				// place in private camera view for some time
				// the entity needs to teleport to where the camera view is to have the PVS right
				player->Teleport( ent->GetPhysics()->GetOrigin(), ang_zero, this );
				player->StartSound( "snd_teleport_enter", SND_CHANNEL_ANY, 0, false, NULL );
				player->SetPrivateCameraView( static_cast<idCamera*>(ent) );
				// the player entity knows where to spawn from the previous Teleport call
				if ( !gameLocal.isClient ) {
					player->PostEventSec( &EV_Player_ExitTeleporter, f );
				}
			} else {
				// direct to exit, Teleport will take care of the killbox
				player->Teleport( GetPhysics()->GetOrigin(), GetPhysics()->GetAxis().ToAngles(), NULL );

				// multiplayer hijacked this entity, so only push the player in multiplayer
				if ( gameLocal.isMultiplayer ) {
					player->GetPhysics()->SetLinearVelocity( GetPhysics()->GetAxis()[0] * pushVel );
				}
			}
		}
		*/
		/*void idPlayerStart::Event_TeleportPlayer( idEntity *activator ) {
			idPlayer *player;

			if ( activator->IsType( idPlayer::Type ) ) {
				player = static_cast<idPlayer*>( activator );
			} else {
				player = gameLocal.GetLocalPlayer();
			}
			if ( player ) {
				if ( spawnArgs.GetBool( "visualFx" ) ) {

					teleportStage = 0;
					Event_TeleportStage( player );

				} else {

					if ( gameLocal.isServer ) {
						idBitMsg	msg;
						byte		msgBuf[MAX_EVENT_PARAM_SIZE];

						msg.Init( msgBuf, sizeof( msgBuf ) );
						msg.BeginWriting();
						msg.WriteBits( player->entityNumber, GENTITYNUM_BITS );
						ServerSendEvent( EVENT_TELEPORTPLAYER, &msg, false, -1 );
					}

					TeleportPlayer( player );
				}
			}
		}*/
	}
}