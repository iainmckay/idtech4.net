using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Game.Entities
{
	public class idWorldSpawn : idEntity
	{
		// TODO
		/*CLASS_DECLARATION( idEntity, idWorldspawn )
			EVENT( EV_Remove,				idWorldspawn::Event_Remove )
			EVENT( EV_SafeRemove,			idWorldspawn::Event_Remove )
		END_CLASS*/

		#region Constructor
		public idWorldSpawn()
			: base()
		{

		}
		#endregion

		// TODO
		/*void idWorldspawn::Event_Remove( void ) {
	gameLocal.Error( "Tried to remove world" );
}*/

		#region idEntity implementation
		public override void Spawn()
		{
			base.Spawn();

			idR.Game.World = this;

			idConsole.Warning("TODO: idWordSpawn.Spawn");

			// TODO
			/*idStr				scriptname;
	idThread			*thread;
	const function_t	*func;
	const idKeyValue	*kv;


	g_gravity.SetFloat( spawnArgs.GetFloat( "gravity", va( "%f", DEFAULT_GRAVITY ) ) );

	// disable stamina on hell levels
	if ( spawnArgs.GetBool( "no_stamina" ) ) {
		pm_stamina.SetFloat( 0.0f );
	}

	// load script
	scriptname = gameLocal.GetMapName();
	scriptname.SetFileExtension( ".script" );
	if ( fileSystem->ReadFile( scriptname, NULL, NULL ) > 0 ) {
		gameLocal.program.CompileFile( scriptname );

		// call the main function by default
		func = gameLocal.program.FindFunction( "main" );
		if ( func != NULL ) {
			thread = new idThread( func );
			thread->DelayedStart( 0 );
		}
	}

	// call any functions specified in worldspawn
	kv = spawnArgs.MatchPrefix( "call" );
	while( kv != NULL ) {
		func = gameLocal.program.FindFunction( kv->GetValue() );
		if ( func == NULL ) {
			gameLocal.Error( "Function '%s' not found in script for '%s' key on worldspawn", kv->GetValue().c_str(), kv->GetKey().c_str() );
		}

		thread = new idThread( func );
		thread->DelayedStart( 0 );
		kv = spawnArgs.MatchPrefix( "call", kv );
	}*/
		}

		// TODO
		/*void idWorldspawn::Restore( idRestoreGame *savefile ) {
	assert( gameLocal.world == this );

	g_gravity.SetFloat( spawnArgs.GetFloat( "gravity", va( "%f", DEFAULT_GRAVITY ) ) );

	// disable stamina on hell levels
	if ( spawnArgs.GetBool( "no_stamina" ) ) {
		pm_stamina.SetFloat( 0.0f );
	}
}*/
		
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if(idR.Game.World == this)
			{
				idR.Game.World = null;
			}
		}
		#endregion
	}
}
