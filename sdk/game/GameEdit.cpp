// Copyright (C) 2004 Id Software, Inc.
//

#include "../idlib/precompiled.h"
#pragma hdrstop

#include "Game_local.h"

/*
===============================================================================

	idGameEdit

===============================================================================
*/

idGameEdit			gameEditLocal;
idGameEdit *		gameEdit = &gameEditLocal;


/*
=============
idGameEdit::GetSelectedEntities
=============
*/
int idGameEdit::GetSelectedEntities( idEntity *list[], int max ) {
	common->DPrintf("PROXY - idGameEdit::GetSelectedEntities()\n");
	return 0;
}

/*
=============
idGameEdit::TriggerSelected
=============
*/
void idGameEdit::TriggerSelected() {
	common->DPrintf("PROXY - idGameEdit::TriggerSelected()\n");
}

/*
================
idGameEdit::ClearEntitySelection
================
*/
void idGameEdit::ClearEntitySelection() {
	common->DPrintf("PROXY - idGameEdit::ClearEntitySelection()\n");
}

/*
================
idGameEdit::AddSelectedEntity
================
*/
void idGameEdit::AddSelectedEntity( idEntity *ent ) {
	common->DPrintf("PROXY - idGameEdit::AddSelectedEntity()\n");
}

/*
================
idGameEdit::FindEntityDefDict
================
*/
const idDict *idGameEdit::FindEntityDefDict( const char *name, bool makeDefault ) const {
	common->DPrintf("PROXY - idGameEdit::FindEntityDefDict()\n");
	return NULL;
}

/*
================
idGameEdit::SpawnEntityDef
================
*/
void idGameEdit::SpawnEntityDef( const idDict &args, idEntity **ent ) {
	common->DPrintf("PROXY - idGameEdit::SpawnEntityDef()\n");
}

/*
================
idGameEdit::FindEntity
================
*/
idEntity *idGameEdit::FindEntity( const char *name ) const {
	common->DPrintf("PROXY - idGameEdit::FindEntity()\n");
	return NULL;
}

/*
=============
idGameEdit::GetUniqueEntityName

generates a unique name for a given classname
=============
*/
const char *idGameEdit::GetUniqueEntityName( const char *classname ) const {
	common->DPrintf("PROXY - idGameEdit::GetUniqueEntityName()\n");
	return NULL;
}

/*
================
idGameEdit::EntityGetOrigin
================
*/
void  idGameEdit::EntityGetOrigin( idEntity *ent, idVec3 &org ) const {
	common->DPrintf("PROXY - idGameEdit::EntityGetOrigin()\n");
}

/*
================
idGameEdit::EntityGetAxis
================
*/
void idGameEdit::EntityGetAxis( idEntity *ent, idMat3 &axis ) const {
	common->DPrintf("PROXY - idGameEdit::EntityGetAxis()\n");
}

/*
================
idGameEdit::EntitySetOrigin
================
*/
void idGameEdit::EntitySetOrigin( idEntity *ent, const idVec3 &org ) {
	common->DPrintf("PROXY - idGameEdit::EntitySetOrigin()\n");
}

/*
================
idGameEdit::EntitySetAxis
================
*/
void idGameEdit::EntitySetAxis( idEntity *ent, const idMat3 &axis ) {
	common->DPrintf("PROXY - idGameEdit::EntitySetAxis()\n");
}

/*
================
idGameEdit::EntitySetColor
================
*/
void idGameEdit::EntitySetColor( idEntity *ent, const idVec3 color ) {
	common->DPrintf("PROXY - idGameEdit::EntitySetColor()\n");
}

/*
================
idGameEdit::EntityTranslate
================
*/
void idGameEdit::EntityTranslate( idEntity *ent, const idVec3 &org ) {
	common->DPrintf("PROXY - idGameEdit::EntityTranslate()\n");
}

/*
================
idGameEdit::EntityGetSpawnArgs
================
*/
const idDict *idGameEdit::EntityGetSpawnArgs( idEntity *ent ) const {
	common->DPrintf("PROXY - idGameEdit::EntityGetSpawnArgs()\n");
	return NULL;
}

/*
================
idGameEdit::EntityUpdateChangeableSpawnArgs
================
*/
void idGameEdit::EntityUpdateChangeableSpawnArgs( idEntity *ent, const idDict *dict ) {
	common->DPrintf("PROXY - idGameEdit::EntityUpdateChangeableSpawnArgs()\n");
}

/*
================
idGameEdit::EntityChangeSpawnArgs
================
*/
void idGameEdit::EntityChangeSpawnArgs( idEntity *ent, const idDict *newArgs ) {
	common->DPrintf("PROXY - idGameEdit::EntityChangeSpawnArgs()\n");
}

/*
================
idGameEdit::EntityUpdateVisuals
================
*/
void idGameEdit::EntityUpdateVisuals( idEntity *ent ) {
	common->DPrintf("PROXY - idGameEdit::EntityUpdateVisuals()\n");
}

/*
================
idGameEdit::EntitySetModel
================
*/
void idGameEdit::EntitySetModel( idEntity *ent, const char *val ) {
	common->DPrintf("PROXY - idGameEdit::EntitySetModel()\n");
}

/*
================
idGameEdit::EntityStopSound
================
*/
void idGameEdit::EntityStopSound( idEntity *ent ) {
	common->DPrintf("PROXY - idGameEdit::EntityStopSound()\n");
}

/*
================
idGameEdit::EntityDelete
================
*/
void idGameEdit::EntityDelete( idEntity *ent ) {
	common->DPrintf("PROXY - idGameEdit::EntityDelete()\n");
}

/*
================
idGameEdit::PlayerIsValid
================
*/
bool idGameEdit::PlayerIsValid() const {
	common->DPrintf("PROXY - idGameEdit::PlayerIsValid()\n");
	return false;
}

/*
================
idGameEdit::PlayerGetOrigin
================
*/
void idGameEdit::PlayerGetOrigin( idVec3 &org ) const {
	common->DPrintf("PROXY - idGameEdit::PlayerGetOrigin()\n");
}

/*
================
idGameEdit::PlayerGetAxis
================
*/
void idGameEdit::PlayerGetAxis( idMat3 &axis ) const {
	common->DPrintf("PROXY - idGameEdit::PlayerGetaxis()\n");
}

/*
================
idGameEdit::PlayerGetViewAngles
================
*/
void idGameEdit::PlayerGetViewAngles( idAngles &angles ) const {
	common->DPrintf("PROXY - idGameEdit::PlayerGetViewAngles()\n");
}

/*
================
idGameEdit::PlayerGetEyePosition
================
*/
void idGameEdit::PlayerGetEyePosition( idVec3 &org ) const {
	common->DPrintf("PROXY - idGameEdit::PlayerGetEyePosition()\n");
}


/*
================
idGameEdit::MapGetEntityDict
================
*/
const idDict *idGameEdit::MapGetEntityDict( const char *name ) const {
	common->DPrintf("PROXY - idGameEdit::MapGetEntityDict()\n");
	return NULL;
}

/*
================
idGameEdit::MapSave
================
*/
void idGameEdit::MapSave( const char *path ) const {
	common->DPrintf("PROXY - idGameEdit::MapSave()\n");
}

/*
================
idGameEdit::MapSetEntityKeyVal
================
*/
void idGameEdit::MapSetEntityKeyVal( const char *name, const char *key, const char *val ) const {
	common->DPrintf("PROXY - idGameEdit::MapSetEntityKeyVal()\n");
}

/*
================
idGameEdit::MapCopyDictToEntity
================
*/
void idGameEdit::MapCopyDictToEntity( const char *name, const idDict *dict ) const {
	common->DPrintf("PROXY - idGameEdit::MapCopyDictToEntity()\n");
}



/*
================
idGameEdit::MapGetUniqueMatchingKeyVals
================
*/
int idGameEdit::MapGetUniqueMatchingKeyVals( const char *key, const char *list[], int max ) const {
	common->DPrintf("PROXY - idGameEdit::MapGetUniqueMatchingKeyVals()\n");
	return 0;
}

/*
================
idGameEdit::MapAddEntity
================
*/
void idGameEdit::MapAddEntity( const idDict *dict ) const {
	common->DPrintf("PROXY - idGameEdit::MapAddEntity()\n");
}

/*
================
idGameEdit::MapRemoveEntity
================
*/
void idGameEdit::MapRemoveEntity( const char *name ) const {
	common->DPrintf("PROXY - idGameEdit::MapRemoveEntity()\n");
}


/*
================
idGameEdit::MapGetEntitiesMatchignClassWithString
================
*/
int idGameEdit::MapGetEntitiesMatchingClassWithString( const char *classname, const char *match, const char *list[], const int max ) const {
	common->DPrintf("PROXY - idGameEdit::MapGetEntitiesMatchingClassWithString()\n");
	return 0;
}


/*
================
idGameEdit::MapEntityTranslate
================
*/
void idGameEdit::MapEntityTranslate( const char *name, const idVec3 &v ) const {
	common->DPrintf("PROXY - idGameEdit::MapEntityTranslate()\n");
}

bool idGameEdit::AF_SpawnEntity( const char *fileName ) {
	common->DPrintf("PROXY - idGameEdit::AF_SpawnEntity()\n");
	return false;
}

void idGameEdit::AF_UpdateEntities( const char *fileName ) {
	common->DPrintf("PROXY - idGameEdit::AF_UpdateEntities()\n");
}

void idGameEdit::AF_UndoChanges( void ) {
	common->DPrintf("PROXY - idGameEdit::AF_UndoChanges()\n");
}

idRenderModel *	idGameEdit::AF_CreateMesh( const idDict &args, idVec3 &meshOrigin, idMat3 &meshAxis, bool &poseIsSet ) {
	common->DPrintf("PROXY - idGameEdit::AF_CreateMesh()\n");
	return NULL;
}

	// Animation system calls for non-game based skeletal rendering.
idRenderModel* idGameEdit::ANIM_GetModelFromEntityDef( const char *classname ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetModelFromEntityDef()\n");
	return NULL;
}

const idVec3& idGameEdit::ANIM_GetModelOffsetFromEntityDef( const char *classname ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetModelOffsetFromEntityDef()\n");
	return idVec3();
}

idRenderModel* idGameEdit::ANIM_GetModelFromEntityDef( const idDict *args ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetModelFromEntityDef()\n");
	return NULL;
}

idRenderModel* idGameEdit::ANIM_GetModelFromName( const char *modelName ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetModelFromname()\n");
	return NULL;
}

const idMD5Anim* idGameEdit::ANIM_GetAnimFromEntityDef( const char *classname, const char *animname ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetAnimFromEntityDef()\n");
	return NULL;
}

int	idGameEdit::ANIM_GetNumAnimsFromEntityDef( const idDict *args ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetNumAnimsFromEntityDef()\n");
	return 0;
}

const char*	idGameEdit::ANIM_GetAnimNameFromEntityDef( const idDict *args, int animNum ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetAnimNameFromEntityDef()\n");
	return NULL;
}

const idMD5Anim* idGameEdit::ANIM_GetAnim( const char *fileName ) {
	common->DPrintf("PROXY - idGameEdit::NIM_GetAnim()\n");
	return NULL;
}

int	idGameEdit::ANIM_GetLength( const idMD5Anim *anim ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetLength()\n");
	return 0;
}

int	idGameEdit::ANIM_GetNumFrames( const idMD5Anim *anim ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_GetNumFrames()\n");
	return 0;
}

void idGameEdit::ANIM_CreateAnimFrame( const idRenderModel *model, const idMD5Anim *anim, int numJoints, idJointMat *frame, int time, const idVec3 &offset, bool remove_origin_offset ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_CreateAnimFrame()\n");
}

idRenderModel* idGameEdit::ANIM_CreateMeshForAnim( idRenderModel *model, const char *classname, const char *animname, int frame, bool remove_origin_offset ) {
	common->DPrintf("PROXY - idGameEdit::ANIM_CreateMeshForAnim()\n");
	return NULL;
}

void idGameEdit::ParseSpawnArgsToRenderLight( const ::idDict *args, renderLight_t *renderLight ) {
	common->DPrintf("PROXY - idGameEdit::ParseSpawnArgsToRenderLight()\n");
}

void idGameEdit::ParseSpawnArgsToRenderEntity( const ::idDict *args, renderEntity_t *renderEntity ) 
{
	common->DPrintf("PROXY - idGameEdit::ParseSpawnArgsToRefEntity()\n");
	/*::idDict* tmp = const_cast<::idDict*>(dict);

	idE::_gameEdit->ParseSpawnArgsToRenderEntity(gcnew idTech4::idDict(*tmp), gcnew idRenderEntity(renderEntity));*/
}

void idGameEdit::ParseSpawnArgsToRefSound( const ::idDict *args, refSound_t *refSound ) {
	common->DPrintf("PROXY - idGameEdit::ParseSpawnArgsToRefSound()\n");
}