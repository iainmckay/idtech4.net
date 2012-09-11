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
using System.IO;
using System.Linq;
using System.Text;

namespace idTech4.Renderer
{
	public sealed class idRenderModelManager
	{
		#region Properties
		public idRenderModel DefaultModel
		{
			get
			{
				return _defaultModel;
			}
		}
		#endregion

		#region Members
		private bool _insideLevelLoad;
		private Dictionary<string, idRenderModel> _models = new Dictionary<string, idRenderModel>(StringComparer.OrdinalIgnoreCase);

		private idRenderModel _defaultModel;
		#endregion

		#region Constructor
		public idRenderModelManager()
		{
			new idCvar("r_mergeModelSurfaces", "1", "combine model surfaces with the same material", CvarFlags.Bool | CvarFlags.Renderer);
			new idCvar("r_slopVertex", "0.01", "merge xyz coordinates this far apart", CvarFlags.Renderer);
			new idCvar("r_slopTexCoord", "0.001", "merge texture coordinates this far apart", CvarFlags.Renderer);
			new idCvar("r_slopNormal", "0.02", "merge normals that dot less than this", CvarFlags.Renderer);
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// World map parsing will add all the inline models with this call.
		/// </summary>
		/// <param name="model"></param>
		public void AddModel(idRenderModel model)
		{
			_models.Add(model.Name, model);
		}

		/// <summary>
		/// Allocates a new empty render model.
		/// </summary>
		/// <returns></returns>
		public idRenderModel AllocateModel()
		{
			return new idRenderModel_Static();
		}

		public void BeginLevelLoad()
		{
			_insideLevelLoad = true;

			foreach(idRenderModel model in _models.Values)
			{
				if((idE.CvarSystem.GetBool("com_purgeAll") == true) && (model.IsReloadable == true))
				{
					idConsole.Warning("TODO: R_CheckForEntityDefsUsingModel( model );");
					model.Purge();
				}

				model.IsLevelLoadReferenced = true;
			}
	
			// purge unused triangle surface memory
			idConsole.Warning("TODO: PurgeTriSurfData");
			// TODO: R_PurgeTriSurfData( frameData );
		}

		public void EndLevelLoad()
		{
			idConsole.WriteLine("----- idRenderModelManager::EndLevelLoad -----");

			int start = idE.System.Milliseconds;
			int purgeCount = 0;
			int keepCount = 0;
			int loadCount = 0;

			_insideLevelLoad = false;
			
			// purge any models not touched
			foreach(idRenderModel model in _models.Values)
			{
				if((model.IsLevelLoadReferenced == false) && (model.IsLoaded == true) && (model.IsReloadable == true))
				{
					// common->Printf( "purging %s\n", model->Name() );
					purgeCount++;

					idConsole.Warning("TODO: R_CheckForEntityDefsUsingModel( model );");

					model.Purge();
				}
				else
				{
					// common->Printf( "keeping %s\n", model->Name() );
					keepCount++;
				}
			}

			// purge unused triangle surface memory
			idConsole.Warning("TODO: PurgeTriSurfData");
			// TODO: R_PurgeTriSurfData( frameData );

			// load any new ones
			foreach(idRenderModel model in _models.Values)
			{
				if((model.IsLevelLoadReferenced == true) && (model.IsLoaded == false) && (model.IsReloadable == true))
				{
					loadCount++;

					model.Load();

					if((loadCount & 15) == 0)
					{
						idConsole.Warning("TODO: PacifierUpdate");
						// TODO: idE.Session.PacifierUpdate();
					}
				}
			}

			// _D3XP added this
			int end = idE.System.Milliseconds;

			idConsole.WriteLine("{0} models purged from previous level, {1} models kept.", purgeCount, keepCount);

			if(loadCount > 0)
			{
				idConsole.WriteLine("{0} new models loaded in {0:0} seconds", loadCount, (end - start) * 0.001);
			}

			idConsole.WriteLine("---------------------------------------------------");
		}

		public idRenderModel FindModel(string name)
		{
			return GetModel(name, true);
		}

		public void Init()
		{
			// TODO: cmds
			/*cmdSystem->AddCommand( "listModels", ListModels_f, CMD_FL_RENDERER, "lists all models" );
			cmdSystem->AddCommand( "printModel", PrintModel_f, CMD_FL_RENDERER, "prints model info", idCmdSystem::ArgCompletion_ModelName );
			cmdSystem->AddCommand( "reloadModels", ReloadModels_f, CMD_FL_RENDERER|CMD_FL_CHEAT, "reloads models" );
			cmdSystem->AddCommand( "touchModel", TouchModel_f, CMD_FL_RENDERER, "touches a model", idCmdSystem::ArgCompletion_ModelName );*/

			_insideLevelLoad = false;
			
			// create a default model
			idRenderModel_Static model = new idRenderModel_Static();
			model.InitEmpty("_DEFAULT");
			model.MakeDefault();
			model.IsLevelLoadReferenced = true;

			_defaultModel = model;

			AddModel(model);

			// create the beam model
			// TODO: beam
			/*idRenderModelStatic *beam = new idRenderModelBeam;
			beam->InitEmpty( "_BEAM" );
			beam->SetLevelLoadReferenced( true );
			beamModel = beam;
			AddModel( beam );*/

			// TODO: sprite
			/*idRenderModelStatic *sprite = new idRenderModelSprite;
			sprite->InitEmpty( "_SPRITE" );
			sprite->SetLevelLoadReferenced( true );
			spriteModel = sprite;
			AddModel( sprite );*/
		}

		public void RemoveModel(idRenderModel model)
		{
			_models.Remove(model.Name);
		}
		#endregion

		#region Private
		private idRenderModel GetModel(string name, bool createIfNotFound)
		{
			idRenderModel model;
			string extension;

			if(_models.ContainsKey(name) == true)
			{
				model = _models[name];

				if(model.IsLoaded == false)
				{
					// reload it if it was purged
					model.Load();
				}
				else if((_insideLevelLoad == true) && (model.IsLevelLoadReferenced == false))
				{
					// we are reusing a model already in memory, but
					// touch all the materials to make sure they stay
					// in memory as well
					model.TouchData();
				}

				model.IsLevelLoadReferenced = true;

				return model;
			}
		
			model = null;
			extension = Path.GetExtension(name).ToLower();

			// see if we can load it	
			// determine which subclass of idRenderModel to initialize
			switch(extension)
			{
				case ".ase":
				case ".lwo":
				case ".flt":
				case ".ma":
					model = new idRenderModel_Static();
					model.InitFromFile(name);
					break;

				case ".md5mesh":
					model = new idRenderModel_MD5();
					model.InitFromFile(name);
					break;

				case ".md3":
					idConsole.Warning("TODO: md3");
					break;

				case ".prt":
					model = new idRenderModel_PRT();
					model.InitFromFile(name);
					break;

				case ".liquid":
					idConsole.Warning("TODO: liquid");
					break;
			}

			if(model == null)
			{
				if(extension != string.Empty)
				{
					idConsole.Warning("unknown model type '{0}'", name);
				}

				if(createIfNotFound == false)
				{
					return null;
				}

				model = new idRenderModel_Static();
				model.InitEmpty(name);
				model.MakeDefault();
			}

			model.IsLevelLoadReferenced = true;

			if((createIfNotFound == false) && (model.IsDefault == true))
			{
				model.Dispose();

				return null;
			}


			AddModel(model);

			return model;
		}
		#endregion
		#endregion
	}

	public struct RenderModelSurface
	{
		public int ID;
		public idMaterial Material;
		public Surface Geometry;
	}
}