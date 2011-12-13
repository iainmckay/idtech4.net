// Copyright (C) 2004 Id Software, Inc.
//

#ifndef __GAME_LOCAL_H__
#define	__GAME_LOCAL_H__

/*
===============================================================================

	Local implementation of the public game interface.

===============================================================================
*/

// if set to 1 the server sends the client PVS with snapshots and the client compares against what it sees
#ifndef ASYNC_WRITE_PVS
	#define ASYNC_WRITE_PVS 0
#endif

#ifdef ID_DEBUG_UNINITIALIZED_MEMORY
// This is real evil but allows the code to inspect arbitrary class variables.
#define private		public
#define protected	public
#endif

extern idRenderWorld *				gameRenderWorld;
extern idSoundWorld *				gameSoundWorld;

// the "gameversion" client command will print this plus compile date
#define	GAME_VERSION		"baseDOOM-1"

//============================================================================

//============================================================================

#include "../managed/AutoPtr.h"
#include "../managed/idDict.h"
#include "../managed/idRenderView.h"
#include "../managed/idLangDict.h"
#include "../managed/idBitMsg.h"
#include "../managed/idSoundWorld.h"
#include "../managed/idMapFile.h"
#include "../managed/idCollisionModelManager.h"
#include "../managed/idRenderModelManager.h"
#include "../managed/idUIManager.h"
#include "../managed/idDeclManager.h"
#include "../managed/idRenderEntity.h"
#include "../managed/idRenderWorld.h"
#include "../managed/idGame.h"
#include "../managed/idGameEdit.h"
#include "../managed/idNetworkSystem.h"
#include "../managed/idCmdSystem.h"
#include "../managed/idCvarSystem.h"
#include "../managed/idFileSystem.h"
#include "../managed/idE.h"
#include <vcclr.h>

#using <mscorlib.dll>

class idGameLocal : public idGame {
private:
	gcroot<idTech4::idGame^> _game;

	::idDict newInfo;

public:
	// ---------------------- Public idGame Interface -------------------

							idGameLocal();

	virtual void			Init( void );
	virtual void			Shutdown( void );
	virtual void			SetLocalClient( int clientNum );
	virtual void			ThrottleUserInfo( void );
	virtual const idDict *	SetUserInfo( int clientNum, const idDict &userInfo, bool isClient, bool canModify );
	virtual const idDict *	GetUserInfo( int clientNum );
	virtual void			SetServerInfo( const idDict &serverInfo );

	virtual const idDict &	GetPersistentPlayerInfo( int clientNum );
	virtual void			SetPersistentPlayerInfo( int clientNum, const idDict &playerInfo );
	virtual void			InitFromNewMap( const char *mapName, idRenderWorld *renderWorld, idSoundWorld *soundWorld, bool isServer, bool isClient, int randSeed );
	virtual bool			InitFromSaveGame( const char *mapName, idRenderWorld *renderWorld, idSoundWorld *soundWorld, idFile *saveGameFile );
	virtual void			SaveGame( idFile *saveGameFile );
	virtual void			MapShutdown( void );
	virtual void			CacheDictionaryMedia( const idDict *dict );
	virtual void			SpawnPlayer( int clientNum );
	virtual gameReturn_t	RunFrame( const usercmd_t *clientCmds );
	virtual bool			Draw( int clientNum );
	virtual escReply_t		HandleESC( idUserInterface **gui );
	virtual idUserInterface	*StartMenu( void );
	virtual const char *	HandleGuiCommands( const char *menuCommand );
	virtual void			HandleMainMenuCommands( const char *menuCommand, idUserInterface *gui );
	virtual allowReply_t	ServerAllowClient( int numClients, const char *IP, const char *guid, const char *password, char reason[MAX_STRING_CHARS] );
	virtual void			ServerClientConnect( int clientNum, const char *guid );
	virtual void			ServerClientBegin( int clientNum );
	virtual void			ServerClientDisconnect( int clientNum );
	virtual void			ServerWriteInitialReliableMessages( int clientNum );
	virtual void			ServerWriteSnapshot( int clientNum, int sequence, idBitMsg &msg, byte *clientInPVS, int numPVSClients );
	virtual bool			ServerApplySnapshot( int clientNum, int sequence );
	virtual void			ServerProcessReliableMessage( int clientNum, const idBitMsg &msg );
	virtual void			ClientReadSnapshot( int clientNum, int sequence, const int gameFrame, const int gameTime, const int dupeUsercmds, const int aheadOfServer, const idBitMsg &msg );
	virtual bool			ClientApplySnapshot( int clientNum, int sequence );
	virtual void			ClientProcessReliableMessage( int clientNum, const idBitMsg &msg );
	virtual gameReturn_t	ClientPrediction( int clientNum, const usercmd_t *clientCmds, bool lastPredictFrame );

	virtual void			GetClientStats( int clientNum, char *data, const int len );
	virtual void			SwitchTeam( int clientNum, int team );

	virtual bool			DownloadRequest( const char *IP, const char *guid, const char *paks, char urls[ MAX_STRING_CHARS ] );

	void					SelectTimeGroup( int timeGroup );
	int						GetTimeGroupTime( int timeGroup );
	void					GetBestGameType( const char* map, const char* gametype, char buf[ MAX_STRING_CHARS ] );
	
	void					GetMapLoadingGUI( char gui[ MAX_STRING_CHARS ] );
};

//============================================================================

extern idGameLocal			gameLocal;

//============================================================================

#include "GameEdit.h"

#endif	/* !__GAME_LOCAL_H__ */
