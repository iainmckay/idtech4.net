using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace idTech4.Game.Entities
{
	/// <summary>
	/// Every map should have at least one worldspawn.
	/// </summary>
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
	const idKeyValue	*kv;*/

			idR.CvarSystem.SetFloat("g_gravity", this.SpawnArgs.GetFloat("gravity", idR.DefaultGravity));

			// disable stamina on hell levels
			if(this.SpawnArgs.GetBool("no_stamina") == true)
			{
				idR.CvarSystem.SetFloat("pm_stamina", 0.0f);
			}

			// load script
			string scriptName = idR.Game.MapName;
			scriptName = Path.Combine(Path.GetDirectoryName(scriptName), Path.GetFileNameWithoutExtension(scriptName));

			if(idR.FileSystem.FileExists(scriptName) == true)
			{
				idConsole.Warning("TODO: script compilefile");

				/*gameLocal.program.CompileFile( scriptname );

				// call the main function by default
				func = gameLocal.program.FindFunction( "main" );
				if ( func != NULL ) {
					thread = new idThread( func );
					thread->DelayedStart( 0 );
				}*/
			}

			// call any functions specified in worldspawn
			if(this.SpawnArgs.ContainsKey("call") == true)
			{
				idConsole.Warning("TODO: wordspawn call");

				/*kv = spawnArgs.MatchPrefix( "call" );
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
		}

		public override void Restore(object savefile)
		{
			base.Restore(savefile);

			idR.CvarSystem.SetFloat("g_gravity", this.SpawnArgs.GetFloat("gravity", idR.DefaultGravity));

			// disable stamina on hell levels
			if(this.SpawnArgs.GetBool("no_stamina") == true)
			{
				idR.CvarSystem.SetFloat("pm_stamina", 0.0f);
			}
		}
		
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