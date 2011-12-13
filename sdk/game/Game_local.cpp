// Copyright (C) 2004 Id Software, Inc.
//


#include "../idlib/precompiled.h"
#pragma hdrstop

#include "Game_local.h"

#ifdef GAME_DLL

idSys *						sys = NULL;
idCommon *					common = NULL;
idCmdSystem *				cmdSystem = NULL;
idCVarSystem *				cvarSystem = NULL;
idFileSystem *				fileSystem = NULL;
idNetworkSystem *			networkSystem = NULL;
idRenderSystem *			renderSystem = NULL;
idSoundSystem *				soundSystem = NULL;
idRenderModelManager *		renderModelManager = NULL;
idUserInterfaceManager *	uiManager = NULL;
idDeclManager *				declManager = NULL;
idAASFileManager *			AASFileManager = NULL;
idCollisionModelManager *	collisionModelManager = NULL;
idCVar *					idCVar::staticVars = NULL;

#endif

idRenderWorld *				gameRenderWorld = NULL;		// all drawing is done to this world
idSoundWorld *				gameSoundWorld = NULL;		// all audio goes to this world

static gameExport_t			gameExport;

// the rest of the engine will only reference the "game" variable, while all local aspects stay hidden
idGameLocal					gameLocal;
idGame *					game = &gameLocal;	// statically pointed at an idGameLocal

/*
===========
GetGameAPI
============
*/
#if __MWERKS__
#pragma export on
#endif
#if __GNUC__ >= 4
#pragma GCC visibility push(default)
#endif
extern "C" gameExport_t *GetGameAPI( gameImport_t *import ) {
#if __MWERKS__
#pragma export off
#endif

	if ( import->version == GAME_API_VERSION ) {

		// set interface pointers used by the game
		sys							= import->sys;
		common						= import->common;
		cmdSystem					= import->cmdSystem;
		cvarSystem					= import->cvarSystem;
		fileSystem					= import->fileSystem;
		networkSystem				= import->networkSystem;
		renderSystem				= import->renderSystem;
		soundSystem					= import->soundSystem;
		renderModelManager			= import->renderModelManager;
		uiManager					= import->uiManager;
		declManager					= import->declManager;
		AASFileManager				= import->AASFileManager;
		collisionModelManager		= import->collisionModelManager;
	}
	
	// set interface pointers used by idLib
	idLib::sys					= sys;
	idLib::common				= common;
	idLib::cvarSystem			= cvarSystem;
	idLib::fileSystem			= fileSystem;

	// setup export interface
	gameExport.version = GAME_API_VERSION;
	gameExport.game = game;
	gameExport.gameEdit = gameEdit;

	return &gameExport;
}
#if __GNUC__ >= 4
#pragma GCC visibility pop
#endif

/*
===========
idGameLocal::idGameLocal
============
*/
idGameLocal::idGameLocal() {
	
}


using namespace System;
using namespace System::Reflection;
using namespace idTech4;

/*
===========
idGameLocal::Init

  initialize the game object, only happens once at startup, not each level load
============
*/
void idGameLocal::Init( void ) {
	idLib::Init();

	common->DPrintf("Attemping to load .net library, assuming everything is 100% OK...\n");

	Assembly^ assembly = Assembly::LoadFrom(String::Format("{0}/idGame.dll", gcnew String(cvarSystem->GetCVarString("fs_game"))));

	idE::_game = safe_cast<idTech4::idGame^>(assembly->CreateInstance("idTech4.Game.idGameLocal"));
	idE::_game->Init();

	//idE::_gameEdit = gcnew idTech4::idGameEdit();
}

/*
===========
idGameLocal::Shutdown

  shut down the entire game
============
*/
void idGameLocal::Shutdown( void ) {
	common->DPrintf("PROXY - idGameLocal::Shutdown()\n");
}

/*
===========
idGameLocal::SaveGame

save the current player state, level name, and level state
the session may have written some data to the file already
============
*/
void idGameLocal::SaveGame( ::idFile *f ) {
	common->DPrintf("PROXY - idGameLocal::SaveGame()\n");
}

/*
===========
idGameLocal::GetPersistentPlayerInfo
============
*/
const ::idDict &idGameLocal::GetPersistentPlayerInfo( int clientNum ) {
	common->DPrintf("PROXY - idGameLocal::GetPersistentPlayerInfo()\n");
	return ::idDict();
}

/*
===========
idGameLocal::SetPersistentPlayerInfo
============
*/
void idGameLocal::SetPersistentPlayerInfo( int clientNum, const ::idDict &playerInfo ) {
	common->DPrintf("PROXY - idGameLocal::SetPersistentPlayerInfo()\n");
}

/*
===========
idGameLocal::SetLocalClient
============
*/
void idGameLocal::SetLocalClient( int clientNum ) {
	idE::_game->SetLocalClient(clientNum);
}

/*
===========
idGameLocal::SetUserInfo
============
*/
const ::idDict* idGameLocal::SetUserInfo( int clientNum, const ::idDict &userInfo, bool isClient, bool canModify ) 
{
	idTech4::idDict^ ret = idE::_game->SetUserInfo(clientNum, gcnew idTech4::idDict(const_cast<::idDict&>(userInfo)), isClient, canModify);
	
	if(ret != nullptr)
	{
		newInfo = ::idDict();
		newInfo.Copy(*ret->GetNative());

		return &newInfo;
	}

	return NULL;
}

/*
===========
idGameLocal::GetUserInfo
============
*/
const ::idDict* idGameLocal::GetUserInfo( int clientNum ) {
	common->DPrintf("PROXY - idGameLocal::GetUserInfo()\n");
	return NULL;
}

/*
===========
idGameLocal::SetServerInfo
============
*/
void idGameLocal::SetServerInfo( const ::idDict &serverInfo ) 
{
	idE::_game->SetServerInfo(gcnew idTech4::idDict(const_cast<::idDict&>(serverInfo)));
}

/*
===================
idGameLocal::InitFromNewMap
===================
*/
void idGameLocal::InitFromNewMap( const char *mapName, ::idRenderWorld *renderWorld, ::idSoundWorld *soundWorld, bool isServer, bool isClient, int randseed ) 
{
	idE::_game->InitFromNewMap(gcnew String(mapName), gcnew idTech4::idRenderWorld(renderWorld), gcnew idTech4::idSoundWorld(soundWorld), isServer, isClient, randseed);
}

/*
=================
idGameLocal::InitFromSaveGame
=================
*/
bool idGameLocal::InitFromSaveGame( const char *mapName, ::idRenderWorld *renderWorld, ::idSoundWorld *soundWorld, ::idFile *saveGameFile ) {
	common->DPrintf("PROXY - idGameLocal::InitFromSaveGame()\n");
	return false;
}

/*
===========
idGameLocal::MapShutdown
============
*/
void idGameLocal::MapShutdown( void ) {
	common->DPrintf("PROXY - idGameLocal::MapShutdown()\n");
}

/*
===================
idGameLocal::CacheDictionaryMedia

This is called after parsing an EntityDef and for each entity spawnArgs before
merging the entitydef.  It could be done post-merge, but that would
avoid the fast pre-cache check associated with each entityDef
===================
*/
void idGameLocal::CacheDictionaryMedia( const ::idDict *dict ) 
{
	::idDict* tmp = const_cast<::idDict*>(dict);

	idE::_game->CacheDictionaryMedia(gcnew idTech4::idDict(*tmp));
}

/*
===========
idGameLocal::SpawnPlayer
============
*/
void idGameLocal::SpawnPlayer( int clientNum ) {
	common->DPrintf("PROXY - idGameLocal::SpawnPlayer()\n");
}

/*
================
idGameLocal::RunFrame
================
*/
gameReturn_t idGameLocal::RunFrame( const usercmd_t *clientCmds ) 
{
	int clientCount = sizeof(*clientCmds) / sizeof(usercmd_t);

	array<idUserCommand^>^ cmds = gcnew array<idUserCommand^>(clientCount);

	for(int i = 0; i < clientCount; i++)
	{
		cmds[i] = gcnew idUserCommand(clientCmds[i]);
	}

	idGameReturn^ ret = idE::_game->RunFrame(cmds);	
	gameReturn_t g = gameReturn_t();

	if(ret->SessionCommand != nullptr)
	{
		char* tmpCmd = (char*) Marshal::StringToHGlobalAnsi(ret->SessionCommand).ToPointer();
	
		strncpy(g.sessionCommand, tmpCmd, MAX_STRING_CHARS);
		Marshal::FreeHGlobal((IntPtr) tmpCmd);
	}

	g.consistencyHash = ret->ConsistencyHash;
	g.health = ret->Health;
	g.heartRate = ret->HeartRate;
	g.stamina = ret->Stamina;
	g.combat = ret->Combat;
	g.syncNextGameFrame = ret->SyncNextGameFrame;

	return g;
}

/*
================
idGameLocal::Draw

makes rendering and sound system calls
================
*/
bool idGameLocal::Draw( int clientNum ) 
{
	return idE::_game->Draw(clientNum);
}

/*
================
idGameLocal::HandleESC
================
*/
escReply_t idGameLocal::HandleESC( ::idUserInterface **gui ) {
	common->DPrintf("PROXY - idGameLocal::HandleESC()\n");
	return ESC_MAIN;
}

/*
================
idGameLocal::StartMenu
================
*/
::idUserInterface* idGameLocal::StartMenu( void ) {
	common->DPrintf("PROXY - idGameLocal::StartMenu()\n");
	return NULL;
}

/*
================
idGameLocal::HandleGuiCommands
================
*/
const char* idGameLocal::HandleGuiCommands( const char *menuCommand ) {
	common->DPrintf("PROXY - idGameLocal::HandleGuiCommands()\n");
	return NULL;
}

/*
================
idGameLocal::HandleMainMenuCommands
================
*/
void idGameLocal::HandleMainMenuCommands( const char *menuCommand, ::idUserInterface *gui ) 
{ 
	idE::_game->HandleMainMenuCommands(gcnew String(menuCommand), gcnew idTech4::idUserInterface(gui));
}

/*
================
idGameLocal::ThrottleUserInfo
================
*/
void idGameLocal::ThrottleUserInfo( void ) {
	common->DPrintf("PROXY - idGameLocal::ThrottleUserInfo()\n");
}

/*
===========
idGameLocal::SelectTimeGroup
============
*/
void idGameLocal::SelectTimeGroup( int timeGroup ) { 
	common->DPrintf("PROXY - idGameLocal::SelectTimeGroup()\n");
}

/*
===========
idGameLocal::GetTimeGroupTime
============
*/
int idGameLocal::GetTimeGroupTime( int timeGroup ) {
	common->DPrintf("PROXY - idGameLocal::GetTimeGroupTime()\n");
	return 0;
}

/*
===========
idGameLocal::GetBestGameType
============
*/
void idGameLocal::GetBestGameType( const char* map, const char* gametype, char buf[ MAX_STRING_CHARS ] ) 
{
	String^ ret = idE::_game->GetBestGameType(gcnew String(map), gcnew String(gametype));
	char* tmpRet = (char*) Marshal::StringToHGlobalAnsi(ret).ToPointer();

	strncpy(buf, tmpRet, MAX_STRING_CHARS);

	Marshal::FreeHGlobal((IntPtr) tmpRet);
}

/*
================
idGameLocal::GetClientStats
================
*/
void idGameLocal::GetClientStats( int clientNum, char *data, const int len ) {
	common->DPrintf("PROXY - idGameLocal::GetClientStats()\n");
}


/*
================
idGameLocal::SwitchTeam
================
*/
void idGameLocal::SwitchTeam( int clientNum, int team ) {
	common->DPrintf("PROXY - idGameLocal::SwitchTeam()\n");
}

/*
===============
idGameLocal::GetMapLoadingGUI
===============
*/
void idGameLocal::GetMapLoadingGUI( char gui[ MAX_STRING_CHARS ] ) 
{
	String^ ret = idE::_game->GetMapLoadingGui(gcnew String(gui));
	char* tmpRet = (char*) Marshal::StringToHGlobalAnsi(ret).ToPointer();

	strncpy(gui, tmpRet, MAX_STRING_CHARS);

	Marshal::FreeHGlobal((IntPtr) tmpRet);
}

bool idGameLocal::ClientApplySnapshot( int clientNum, int sequence ) {
	common->DPrintf("PROXY - idGameLocal::ClientApplySnapshot()\n");
	return false;
}

void idGameLocal::ClientReadSnapshot( int clientNum, int sequence, const int gameFrame, const int gameTime, const int dupeUsercmds, const int aheadOfServer, const ::idBitMsg &msg ) {
	common->DPrintf("PROXY - idGameLocal::ClientReadSnapshot()\n");
}

void idGameLocal::ClientProcessReliableMessage( int clientNum, const ::idBitMsg &msg ) {
	common->DPrintf("PROXY - idGameLocal::ClientProcessReliableMessage()\n");
}

gameReturn_t idGameLocal::ClientPrediction( int clientNum, const usercmd_t *clientCmds, bool lastPredictFrame ) {
	common->DPrintf("PROXY - idGameLocal::ClientPrediction()\n");
	return gameReturn_t();
}

bool idGameLocal::DownloadRequest( const char *IP, const char *guid, const char *paks, char urls[ MAX_STRING_CHARS ] ) {
	common->DPrintf("PROXY - idGameLocal::DownloadRequest()\n");
	return false;
}

allowReply_t idGameLocal::ServerAllowClient( int numClients, const char *IP, const char *guid, const char *password, char reason[MAX_STRING_CHARS] ) {
	common->DPrintf("PROXY - idGameLocal::ServerAllowClient()\n");
	return ALLOW_NO;
}

void idGameLocal::ServerClientConnect( int clientNum, const char *guid ) 
{
	idE::_game->ServerClientConnect(clientNum, gcnew String(guid));
}

void idGameLocal::ServerClientBegin( int clientNum ) 
{
	idE::_game->ServerClientBegin(clientNum);
}

void idGameLocal::ServerClientDisconnect( int clientNum ) {
	common->DPrintf("PROXY - idGameLocal::ServerClientDisconnect()\n");
}

bool idGameLocal::ServerApplySnapshot( int clientNum, int sequence ) {
	common->DPrintf("PROXY - idGameLocal::ServerApplySnapshot()\n");
	return false;
}

void idGameLocal::ServerWriteInitialReliableMessages( int clientNum ) {
	common->DPrintf("PROXY - idGameLocal::ServerWriteInitialReliableMessages()\n");
}

void idGameLocal::ServerWriteSnapshot( int clientNum, int sequence, ::idBitMsg &msg, byte *clientInPVS, int numPVSClients ) {
	common->DPrintf("PROXY - idGameLocal::ServerWriteSnapshot()\n");
}

void idGameLocal::ServerProcessReliableMessage( int clientNum, const ::idBitMsg &msg ) {
	common->DPrintf("PROXY - idGameLocal::ServerProcessReliableMessage()\n");
}