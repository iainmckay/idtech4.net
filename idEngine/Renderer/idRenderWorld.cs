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

using Microsoft.Xna.Framework;

using idTech4;
using idTech4.Geometry;
using idTech4.Text;

namespace idTech4.Renderer
{
	public sealed class idRenderWorld : IDisposable
	{
		#region Constants
		public const int ChildrenHaveMultipleAreas = -2;
		public const int AreaSolid = -1;
		#endregion

		#region Properties
		#region Scene Rendering
		/// <summary>
		/// Sets the current view so any calls to the render world will use the correct parms.
		/// </summary>
		/// <remarks>
		/// Some calls to material functions use the current renderview time when servicing cinematics.  
		/// This ensures that any parms accessed (such as time) are properly set.
		/// </remarks>
		public idRenderView RenderView
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return idE.RenderSystem.PrimaryRenderView;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idE.RenderSystem.PrimaryRenderView = value;
			}
		}
		#endregion
		#endregion

		#region Members
		private string _mapName; // ie: maps/tim_dm2.proc, written to demoFile
		// TODO: ID_TIME_T					mapTimeStamp;			// for fast reloads of the same level

		private AreaNode[] _areaNodes = null;
		private PortalArea[] _portalAreas = null;
		private DoublePortal[] _doublePortals = null;

		private int _areaNodeCount;
		private int _portalAreaCount;
		private int _interAreaPortalCount;
		
		private int _connectedAreaNumber; // incremented every time a door portal state changes

		private idScreenRect[] _areaScreenRect = null;
		private List<idRenderModel> _localModels = new List<idRenderModel>();
		private List<idRenderEntity> _entityDefinitions = new List<idRenderEntity>();
		/*
		idList<idRenderLightLocal*>		lightDefs;
		*/

		// all light / entity interactions are referenced here for fast lookup without
		// having to crawl the doubly linked lists.  EnntityDefs are sequential for better
		// cache access, because the table is accessed by light in idRenderWorldLocal::CreateLightDefInteractions()
		// Growing this table is time consuming, so we add a pad value to the number
		// of entityDefs and lightDefs
		/*private idInteraction[] _interactionTable;
		private int _interactionTableWidth;	 // entityDefs
		private int _interactionTableHeight;*/ // lightDefs*/


		private bool _generateInteractionsCalled;
		#endregion

		#region Constructor
		public idRenderWorld()
		{
			// TODO: mapTimeStamp = FILE_NOT_FOUND_TIMESTAMP;
		}

		~idRenderWorld()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region General
		public RenderEntityComponent GetRenderEntity(int handle)
		{
			if((handle < 0) || (handle >= _entityDefinitions.Count))
			{
				idConsole.WriteLine("idRenderWord::GetRenderEntity: invalid handle {0} [0, {1}]", handle, _entityDefinitions.Count);
				return null;
			}

			idRenderEntity def = _entityDefinitions[handle];

			if(def == null)
			{
				idConsole.WriteLine("idRenderWord::GetRenderEntity: handle {0} is null", handle);
				return null;
			}

			return def.Parameters;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// A NULL or empty name will make a world without a map model, which
		/// is still useful for displaying a bare model.
		/// </remarks>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool InitFromMap(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// if this is an empty world, initialize manually
			if((name == null) || (name == string.Empty))
			{
				FreeWorld();
				ClearWorld();

				_mapName = string.Empty;

				return true;
			}

			// load it
			string fileName = string.Format("{0}.{1}",
				Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name)),
				idE.ProcFileExtension);

			// if we are reloading the same map, check the timestamp
			// and try to skip all the work
			// TODO: timestamps
			/*ID_TIME_T currentTimeStamp;
			fileSystem->ReadFile( filename, NULL, &currentTimeStamp );*/

			/*if ( name == mapName ) {
				if ( currentTimeStamp != FILE_NOT_FOUND_TIMESTAMP && currentTimeStamp == mapTimeStamp ) {
					common->Printf( "idRenderWorldLocal::InitFromMap: retaining existing map\n" );
					FreeDefs();
					TouchWorldModels();
					AddWorldModelEntities();
					ClearPortalStates();
					return true;
				}
				common->Printf( "idRenderWorldLocal::InitFromMap: timestamp has changed, reloading.\n" );
			}*/

			FreeWorld();

			idLexer lexer = new idLexer(LexerOptions.NoStringConcatination | LexerOptions.NoDollarPrecompilation);

			if(lexer.LoadFile(fileName) == false)
			{
				idConsole.WriteLine("idRenderWorld.InitFromMap: {0} not found", fileName);
				ClearWorld();

				return false;
			}

			_mapName = name;
			// TODO: mapTimeStamp = currentTimeStamp;

			// if we are writing a demo, archive the load command
			// TODO: demo
			/*if ( session->writeDemo ) {
				WriteLoadMap();
			}*/

			idToken token;
			idRenderModel lastModel;
			string tokenValue;

			if(((token = lexer.ReadToken()) == null) || (token.ToString().Equals(idE.ProcFileID, StringComparison.OrdinalIgnoreCase) == false))
			{
				idConsole.WriteLine("idRenderWorld.InitFromMap: bad id '{0}' instead of '{1}'", token, idE.ProcFileID);
				return false;
			}

			// parse the file
			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString();

				if((tokenValue == "model") || (tokenValue == "shadowModel"))
				{
					if(tokenValue == "model")
					{
						lastModel = ParseModel(lexer);
					}
					else
					{
						lastModel = ParseShadowModel(lexer);
					}

					// add it to the model manager list
					idE.RenderModelManager.AddModel(lastModel);

					// save it in the list to free when clearing this map
					_localModels.Add(lastModel);
				}
				else if(tokenValue == "interAreaPortals")
				{
					ParseInterAreaPortals(lexer);
				}
				else if(tokenValue == "nodes")
				{
					ParseNodes(lexer);
				}
				else
				{
					lexer.Error("idRenderWorld.InitFromMap: bad token \"{0}\"", tokenValue);
				}
			}

			// if it was a trivial map without any areas, create a single area
			if(_portalAreaCount == 0)
			{
				ClearWorld();
			}

			// find the points where we can early-our of reference pushing into the BSP tree
			CommonChildrenArea(_areaNodes[0]);

			AddWorldModelEntities();
			ClearPortalStates();

			// done!
			return true;
		}
		#endregion

		#region Entity and Light Defs
		/// <summary>
		/// All the modelrefs and lightrefs that are in visible areas
		/// will have viewEntitys and viewLights created for them.
		/// </summary>
		/// <remarks>
		/// The scissorRects on the viewEntitys and viewLights may be empty if
		/// they were considered, but not actually visible.
		/// </remarks>
		public void FindViewLightsAndEntities()
		{
			View viewDef = idE.RenderSystem.ViewDefinition;

			// clear the visible lightDef and entityDef lists
			// TODO: viewDef.ViewLights.Clear();
			viewDef.ViewEntities.Clear();

			// find the area to start the portal flooding in
			if(idE.CvarSystem.GetBool("r_usePortals") == false)
			{
				// debug tool to force no portal culling
				viewDef.AreaNumber = -1;
			}
			else
			{
				viewDef.AreaNumber = PointInArea(viewDef.InitialViewAreaOrigin);
			}

			// determine all possible connected areas for light-behind-door culling
			BuildConnectedAreas();

			// bump the view count, invalidating all visible areas
			idE.RenderSystem.ViewCount++;

			// flow through all the portals and add models / lights
			if(idE.CvarSystem.GetBool("r_singleArea") == true)
			{
				// if debugging, only mark this area
				// if we are outside the world, don't draw anything
				if(viewDef.AreaNumber >= 0)
				{
					idConsole.Warning("TODO: r_singleArea  important!!!!");
					/*portalStack_t	ps;
					int				i;
					static int lastPrintedAreaNum;

					if ( tr.viewDef->areaNum != lastPrintedAreaNum ) {
						lastPrintedAreaNum = tr.viewDef->areaNum;
						common->Printf( "entering portal area %i\n", tr.viewDef->areaNum );
					}

					for ( i = 0 ; i < 5 ; i++ ) {
						ps.portalPlanes[i] = tr.viewDef->frustum[i];
					}
					ps.numPortalPlanes = 5;
					ps.rect = tr.viewDef->scissor;

					AddAreaRefs( tr.viewDef->areaNum, &ps );*/
				}
			} 
			else 
			{
				// note that the center of projection for flowing through portals may
				// be a different point than initialViewAreaOrigin for subviews that
				// may have the viewOrigin in a solid/invalid area
				FlowViewThroughPortals(viewDef.RenderView.ViewOrigin, 5, viewDef.Frustum);
			}
		}


		/// <summary>
		/// Force the generation of all light / surface interactions at the start of a level.
		/// If this isn't called, they will all be dynamically generated
		/// </summary>
		/// <remarks>
		/// This really isn't all that helpful anymore, because the calculation of shadows
		/// and light interactions is deferred from idRenderWorld::CreateLightDefInteractions(), but we
		/// use it as an oportunity to size the interactionTable.
		/// </remarks>
		public void GenerateInteractions()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(idE.RenderSystem.IsRunning == false)
			{
				return;
			}

			int start = idE.System.Milliseconds;

			_generateInteractionsCalled = false;

			// watch how much memory we allocate
			// TODO: tr.staticAllocCount = 0;

			// let idRenderWorld::CreateLightDefInteractions() know that it shouldn't
			// try and do any view specific optimizations
			idE.RenderSystem.ViewDefinition = null;

			idConsole.Warning("TODO: light interactions");

			/*for ( int i = 0 ; i < this->lightDefs.Num() ; i++ ) {
				idRenderLightLocal	*ldef = this->lightDefs[i];
				if ( !ldef ) {
					continue;
				}
				this->CreateLightDefInteractions( ldef );
			}*/

			int end = idE.System.Milliseconds;
			int msec = end - start;

			idConsole.WriteLine("idRenderWorld::GenerateAllInteractions, msec = {0}, staticAllocCount = {1}.", msec, 0 /* TODO: tr.staticAllocCount*/);
	
			// build the interaction table
			/*if(idE.CvarSystem.GetBool("r_useInteractionTable") == true)
			{
				_interactionTableWidth = _entityDefinitions.Count + 100;
				_interactionTableHeight = /* TODO: lightDefs *//* 0 + 100;

				_interactionTable = new idInteraction[_interactionTableWidth * _interactionTableHeight];

				int count = 0;

				idConsole.Warning("TODO: light interactions");
				
				/*for ( int i = 0 ; i < this->lightDefs.Num() ; i++ ) {
					idRenderLightLocal	*ldef = this->lightDefs[i];
					if ( !ldef ) {
						continue;
					}
					idInteraction	*inter;
					for ( inter = ldef->firstInteraction; inter != NULL; inter = inter->lightNext ) {
						idRenderEntityLocal	*edef = inter->entityDef;
						int index = ldef->index * interactionTableWidth + edef->index;

						interactionTable[ index ] = inter;
						count++;
					}
				}*/

				/*common->Printf( "interactionTable size: %i bytes\n", size );
				comon->Printf( "%i interaction take %i bytes\n", count, count * sizeof( idInteraction ) );*//*
			}

			// entities flagged as noDynamicInteractions will no longer make any*/
			_generateInteractionsCalled = true;
		}
		#endregion

		#region Scene Rendering
		/// <summary>
		/// Rendering a scene may actually render multiple subviews for mirrors and portals, and
		/// may render composite textures for gui console screens and light projections.
		/// It would also be acceptable to render a scene multiple times, for "rear view mirrors", etc.
		/// </summary>
		/// <param name="renderView"></param>
		public void RenderScene(idRenderView renderView)
		{
#if ID_DEDICATED

#else
			if(idE.RenderSystem.IsRunning == false)
			{
				return;
			}

			// skip front end rendering work, which will result
			// in only gui drawing
			if(idE.CvarSystem.GetBool("r_skipFrontEnd") == true)
			{
				return;
			}

			if((renderView.FovX <= 0) || (renderView.FovY <= 0))
			{
				idConsole.Error("idRenderWorld::RenderScene: bad FOVS: {0}, {1}", renderView.FovX, renderView.FovY);
			}

			idRenderView copy = renderView.Copy();

			// close any gui drawing
			idE.RenderSystem.GuiModel.EmitFullScreen();
			idE.RenderSystem.GuiModel.Clear();

			int startTime = idE.System.Milliseconds;

			// setup view parms for the initial view
			//
			View parms = new View();
			parms.RenderView = copy;

			// TODO: screenshot
			/*if ( tr.takingScreenshot ) {
				parms->renderView.forceUpdate = true;
			}*/

			// set up viewport, adjusted for resolution and OpenGL style 0 at the bottom
			parms.ViewPort = idE.RenderSystem.RenderViewToViewPort(parms.RenderView);

			// the scissor bounds may be shrunk in subviews even if
			// the viewport stays the same
			// this scissor range is local inside the viewport
			parms.Scissor.X1 = 0;
			parms.Scissor.Y1 = 0;
			parms.Scissor.X2 = (short) (parms.ViewPort.X2 - parms.ViewPort.X1);
			parms.Scissor.Y2 = (short) (parms.ViewPort.Y2 - parms.ViewPort.Y1);

			parms.IsSubview = false;
			parms.InitialViewAreaOrigin = renderView.ViewOrigin;
			parms.FloatTime = parms.RenderView.Time * 0.001f;
			parms.RenderWorld = this;

			// use this time for any subsequent 2D rendering, so damage blobs/etc 
			// can use level time
			idE.RenderSystem.FrameShaderTime = parms.FloatTime;

			// see if the view needs to reverse the culling sense in mirrors
			// or environment cube sides
			Vector3 tmp = new Vector3(parms.RenderView.ViewAxis.M21, parms.RenderView.ViewAxis.M22, parms.RenderView.ViewAxis.M23);
			Vector3 tmp2 = new Vector3(parms.RenderView.ViewAxis.M31, parms.RenderView.ViewAxis.M32, parms.RenderView.ViewAxis.M33);
			Vector3 tmp3 = new Vector3(parms.RenderView.ViewAxis.M11, parms.RenderView.ViewAxis.M12, parms.RenderView.ViewAxis.M13);
			Vector3 cross = Vector3.Cross(tmp, tmp2);

			if((cross * tmp3).Length() > 0)
			{
				parms.IsMirror = false;
			}
			else
			{
				parms.IsMirror = true;
			}

			if(idE.CvarSystem.GetBool("r_lockSurfaces") == true)
			{
				idConsole.Warning("TODO: R_LockSurfaceScene( parms );");
				return;
			}

			// save this world for use by some console commands
			idE.RenderSystem.PrimaryRenderWorld = this;
			idE.RenderSystem.PrimaryRenderView = copy;
			idE.RenderSystem.PrimaryView = parms;
			
			// rendering this view may cause other views to be rendered
			// for mirrors / portals / shadows / environment maps
			// this will also cause any necessary entities and lights to be
			// updated to the demo file
			idE.RenderSystem.RenderView(parms);

			// now write delete commands for any modified-but-not-visible entities, and
			// add the renderView command to the demo
			
			// TODO: demo
			/*if ( session->writeDemo ) {
				WriteRenderView( renderView );
			}*/

			int endTime = idE.System.Milliseconds;

			// TODO: tr.pc.frontEndMsec += endTime - startTime;

			// prepare for any 2D drawing after this
			idE.RenderSystem.GuiModel.Clear();
#endif
		}
		#endregion

		#region Debug Visualization
		public void DebugClearLines(int time)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idE.RenderSystem.DebugClearLines(time);
		}

		public void DebugClearPolygons(int time)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idE.RenderSystem.DebugClearPolygons(time);
		}
		#endregion

		#region Private
		/// <summary>
		/// Any models that are visible through the current portalStack will have their scissor.
		/// </summary>
		/// <param name="areaNumber"></param>
		/// <param name="portalStack"></param>
		private void AddAreaEntityReferences(int areaNumber, PortalStack portalStack)
		{
			PortalArea area = _portalAreas[areaNumber];

			for(AreaReference areaRef = area.EntityReference.NextArea; areaRef != area.EntityReference; areaRef = areaRef.NextArea)
			{
				idRenderEntity entity = areaRef.Entity;
				
				// debug tool to allow viewing of only one entity at a time
				if((idE.CvarSystem.GetInteger("r_singleEntity") > 0) && (idE.CvarSystem.GetInteger("r_singleEntity") != entity.EntityIndex))
				{
					continue;
				}

				// remove decals that are completely faded away
				// TODO: R_FreeEntityDefFadedDecals( entity, tr.viewDef->renderView.time );

				// check for completely suppressing the model
				if(idE.CvarSystem.GetBool("r_skipSuppress") == false)
				{
					if(entity.Parameters.SuppressSurfaceInViewID == idE.RenderSystem.ViewDefinition.RenderView.ViewID)
					{
						continue;
					}
					else if(entity.Parameters.AllowSurfaceInViewID != idE.RenderSystem.ViewDefinition.RenderView.ViewID)
					{
						continue;
					}
				}

				// cull reference bounds
				if(CullEntityByPortals(entity, portalStack) == true)
				{
					// we are culled out through this portal chain, but it might
					// still be visible through others
					continue;
				}

				ViewEntity viewEntity = SetEntityDefViewEntity(entity);

				// possibly expand the scissor rect
				viewEntity.ScissorRectangle.Union(portalStack.Rectangle);
			}
		}

		/// <summary>
		/// This may be entered multiple times with different planes if more than
		/// one portal sees into the area.
		/// </summary>
		/// <param name="areaNumber"></param>
		/// <param name="portalStack"></param>
		private void AddAreaReferences(int areaNumber, PortalStack portalStack)
		{
			// mark the viewCount, so r_showPortals can display the
			// considered portals
			_portalAreas[areaNumber].ViewCount = idE.RenderSystem.ViewCount;
			
			// add the models and lights, using more precise culling to the planes
			AddAreaEntityReferences(areaNumber, portalStack);
			// TODO: AddAreaLightRefs( areaNum, ps );
		}

		/// <summary>
		/// This is called by R_PushVolumeIntoTree and also directly
		/// for the world model references that are precalculated.
		/// </summary>
		/// <param name="def"></param>
		/// <param name="area"></param>
		private void AddEntityRefToArea(idRenderEntity def, PortalArea area)
		{
			if(def == null)
			{
				idConsole.Error("idRenderWorld::AddEntityRefToArea: null def");
			}

			AreaReference areaRef = new AreaReference();

			// TODO: counters tr.pc.c_entityReferences++;

			areaRef.Entity = def;

			// link to entityDef
			areaRef.NextOwner = def.EntityReference;
			def.EntityReference = areaRef;

			// link to end of area list
			areaRef.Area = area;
			areaRef.NextArea = area.EntityReference;
			areaRef.PreviousArea = area.EntityReference.PreviousArea;
			areaRef.NextArea.PreviousArea = areaRef;
			areaRef.PreviousArea.NextArea = areaRef;
		}

		private void AddWorldModelEntities()
		{
			// add the world model for each portal area
			// we can't just call AddEntityDef, because that would place the references
			// based on the bounding box, rather than explicitly into the correct area
			for(int i = 0; i < _portalAreaCount; i++)
			{
				idRenderEntity def = new idRenderEntity();
				int index = _entityDefinitions.FindIndex(x => x == null);

				if(index == -1)
				{
					index = _entityDefinitions.Count;
					_entityDefinitions.Add(def);
				}
				else
				{
					_entityDefinitions[index] = def;
				}

				def.EntityIndex = index;
				def.World = this;
				def.Parameters.Model = idE.RenderModelManager.FindModel(string.Format("_area{0}", i));

				if((def.Parameters.Model.IsDefaultModel == true) || (def.Parameters.Model.IsStaticWordModel == false))
				{
					idConsole.Error("idRenderWorld::InitFromMap: bad area model lookup");
				}

				idRenderModel model = def.Parameters.Model;

				for(int j = 0; j < model.SurfaceCount; j++)
				{
					RenderModelSurface surf = model.GetSurface(j);

					if(surf.Material.Name == "textures/smf/portal_sky")
					{
						def.NeedsPortalSky = true;
					}
				}

				def.ReferenceBounds = def.Parameters.Model.GetBounds();
				def.Parameters.Axis = new Matrix(
					1, 0, 0, 0, 
					0, 1, 0, 0, 
					0, 0, 1, 0, 
					0, 0, 0, 0);

				def.ModelMatrix = idHelper.AxisToModelMatrix(def.Parameters.Axis, def.Parameters.Origin);

				// in case an explicit shader is used on the world, we don't
				// want it to have a 0 alpha or color
				def.Parameters.MaterialParameters[0] =
					def.Parameters.MaterialParameters[1] =
					def.Parameters.MaterialParameters[2] =
					def.Parameters.MaterialParameters[3] = 1;

				AddEntityRefToArea(def, _portalAreas[i]);
			}
		}

		/// <summary>
		/// This is only valid for a given view, not all views in a frame.
		/// </summary>
		private void BuildConnectedAreas()
		{
			View viewDef = idE.RenderSystem.ViewDefinition;
			viewDef.ConnectedAreas = new bool[_portalAreaCount];

			// if we are outside the world, we can see all areas
			if(viewDef.AreaNumber == -1)
			{
				for(int i = 0; i < _portalAreaCount; i++)
				{
					viewDef.ConnectedAreas[i] = true;
				}

				return;
			}

			// start with none visible, and flood fill from the current area
			BuildConnectedAreas_r(viewDef.AreaNumber);
		}

		private void BuildConnectedAreas_r(int areaNumber)
		{
			if(idE.RenderSystem.ViewDefinition.ConnectedAreas[areaNumber] == true)
			{
				return;
			}

			idE.RenderSystem.ViewDefinition.ConnectedAreas[areaNumber] = true;

			// flood through all non-blocked portals
			PortalArea area = _portalAreas[areaNumber];

			for(Portal portal = area.Portals; portal != null; portal = portal.Next)
			{
				if((portal.DoublePortal.BlockingBits & PortalConnection.BlockView) != PortalConnection.BlockView)
				{
					BuildConnectedAreas_r(portal.IntoArea);
				}
			}
		}

		private void ClearPortalStates()
		{
			// all portals start off open
			for(int i = 0; i < _interAreaPortalCount; i++)
			{
				_doublePortals[i].BlockingBits = PortalConnection.BlockNone;
			}

			// flood fill all area connections
			for(int i = 0; i < _portalAreaCount; i++)
			{
				for(int j = 0; j < 3; j++)
				{
					_connectedAreaNumber++;

					FloodConnectedAreas(_portalAreas[i], j);
				}
			}
		}

		/// <summary>
		/// Sets up for a single area world.
		/// </summary>
		private void ClearWorld()
		{
			_portalAreaCount = 1;
			_areaScreenRect = new idScreenRect[] { new idScreenRect() };
			_portalAreas = new PortalArea[] { new PortalArea() };

			SetupAreaReferences();

			// even though we only have a single area, create a node
			// that has both children pointing at it so we don't need to
			AreaNode areaNode = new AreaNode();
			areaNode.Plane.D = 1;
			areaNode.Children[0] = -1;
			areaNode.Children[1] = -1;

			_areaNodes = new AreaNode[1];
			_areaNodes[0] = areaNode;
		}

		private int CommonChildrenArea(AreaNode node)
		{
			int[] nums = new int[2];
			int count = nums.Length;

			for(int i = 0; i < count; i++)
			{
				if(node.Children[i] <= 0)
				{
					nums[i] = -1 - node.Children[i];
				}
				else
				{
					nums[i] = CommonChildrenArea(_areaNodes[node.Children[i]]);
				}
			}

			// solid nodes will match any area
			if(nums[0] == idRenderWorld.AreaSolid)
			{
				nums[0] = nums[1];
			}

			if(nums[1] == idRenderWorld.AreaSolid)
			{
				nums[1] = nums[0];
			}

			int common;

			if(nums[0] == nums[1])
			{
				common = nums[0];
			}
			else
			{
				common = idRenderWorld.ChildrenHaveMultipleAreas;
			}

			return (node.CommonChildrenArea = common);
		}

		/// <summary>
		/// Return true if the entity reference bounds do not intersect the current portal chain.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="portalStack"></param>
		/// <returns></returns>
		private bool CullEntityByPortals(idRenderEntity entity, PortalStack portalStack)
		{
			if(idE.CvarSystem.GetBool("r_useEntityCulling") == false)
			{
				return false;
			}

			// try to cull the entire thing using the reference bounds.
			// we do not yet do callbacks or dynamic model creation,
			// because we want to do all touching of the model after
			// we have determined all the lights that may effect it,
			// which optimizes cache usage
			return idHelper.CullLocalBox(entity.ReferenceBounds, entity.ModelMatrix, portalStack.PortalPlaneCount, portalStack.PortalPlanes);
		}

		private void FloodConnectedAreas(PortalArea area, int portalAttributeIndex)
		{
			if(area.ConnectedAreaNumber[portalAttributeIndex] == _connectedAreaNumber)
			{
				return;
			}

			area.ConnectedAreaNumber[portalAttributeIndex] = _connectedAreaNumber;

			for(Portal p = area.Portals; p != null; p = p.Next)
			{
				if((p.DoublePortal.BlockingBits & ((PortalConnection) (1 << portalAttributeIndex))) == 0)
				{
					FloodConnectedAreas(_portalAreas[p.IntoArea], portalAttributeIndex);
				}
			}
		}

		private void FloodViewThroughArea(Vector3 origin, int areaNumber, PortalStack portalStack)
		{
			int i, j;
			idWinding	winding; // we won't overflow because MAX_PORTAL_PLANES = 20
	
			PortalStack check;
			PortalStack newStack;
			PortalArea area = _portalAreas[areaNumber];			

			// cull models and lights to the current collection of planes
			AddAreaReferences(areaNumber, portalStack);

			if(_areaScreenRect[areaNumber].IsEmpty == true)
			{
				_areaScreenRect[areaNumber] = portalStack.Rectangle;
			}
			else
			{
				_areaScreenRect[areaNumber].Union(portalStack.Rectangle);
			}
	

			// go through all the portals
			Portal portal;

			for(portal = area.Portals; portal != null; portal = portal.Next)
			{
				// an enclosing door may have sealed the portal off
				if((portal.DoublePortal.BlockingBits  & PortalConnection.BlockView) == PortalConnection.BlockView)
				{
					continue;
				}

				// make sure this portal is facing away from the view
				float d = portal.Plane.Distance(origin);
					
				if(d < -0.1f)
				{
					continue;
				}

				// make sure the portal isn't in our stack trace,
				// which would cause an infinite loop
				for(check = portalStack; check != null; check = check.Next)
				{
					if(check.Portal == portal)
					{
						break; // don't recursively enter a stack
					}
				}
		
				if(check != null)
				{
					continue; // already in stack
				}

				// if we are very close to the portal surface, don't bother clipping
				// it, which tends to give epsilon problems that make the area vanish
				if(d < 1.0f)
				{
					// go through this portal
					newStack = portalStack;
					newStack.Portal = portal;
					newStack.Next = portalStack;

					FloodViewThroughArea(origin, portal.IntoArea, newStack);

					continue;
				}

				// clip the portal winding to all of the planes
				winding = portal.Winding;

				for(j = 0; j < portalStack.PortalPlaneCount; j++)
				{
					Plane neg = new Plane(-portalStack.PortalPlanes[j].Normal, -portalStack.PortalPlanes[j].D);

					if(winding.ClipInPlace(neg, 0) == false)
					{
						break;
					}
				}

				if(winding.PointCount == 0)
				{
					continue;	// portal not visible
				}

				// see if it is fogged out
				if(PortalIsFoggedOut(portal) == true)
				{
					continue;
				}

				// go through this portal
				newStack = new PortalStack();
				newStack.Portal = portal;
				newStack.Next = portalStack;

				// find the screen pixel bounding box of the remaining portal
				// so we can scissor things outside it
				newStack.Rectangle = ScreenRectangleFromWinding(winding, idE.RenderSystem.IdentitySpace);
		
				// slop might have spread it a pixel outside, so trim it back
				newStack.Rectangle.Intersect(portalStack.Rectangle);

				// generate a set of clipping planes that will further restrict
				// the visible view beyond just the scissor rect
				int addPlanes = winding.PointCount;

				if(addPlanes > idE.MaxPortalPlanes)
				{
					addPlanes = idE.MaxPortalPlanes;
				}

				newStack.PortalPlaneCount = 0;

				for(i = 0; i < addPlanes; i++)
				{
					j = i + 1;

					if(j == winding.PointCount)
					{
						j = 0;
					}

					Vector3 v1 = origin - winding[i];
					Vector3 v2 = origin - winding[j];

					newStack.PortalPlanes[newStack.PortalPlaneCount].Normal = Vector3.Cross( v2, v1 );
					
					// if it is degenerate, skip the plane
					newStack.PortalPlanes[newStack.PortalPlaneCount].Normalize();
					
					if(newStack.PortalPlanes[newStack.PortalPlaneCount].Normal.Length() < 0.01f)
					{
						continue;
					}

					newStack.PortalPlanes[newStack.PortalPlaneCount].FitThroughPoint(origin);
					newStack.PortalPlaneCount++;
				}

				// the last stack plane is the portal plane
				newStack.PortalPlanes[newStack.PortalPlaneCount] = portal.Plane;
				newStack.PortalPlaneCount++;

				FloodViewThroughArea(origin, portal.IntoArea, newStack);
			}
		}

		/// <summary>
		/// Finds viewLights and viewEntities by flowing from an origin through the visible portals.
		/// origin point can see into.  The planes array defines a volume (positive
		/// sides facing in) that should contain the origin, such as a view frustum or a point light box.
		/// Zero planes assumes an unbounded volume.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="planes"></param>
		private void FlowViewThroughPortals(Vector3 origin, int planeCount, Plane[] planes)
		{
			View viewDef = idE.RenderSystem.ViewDefinition;

			PortalStack portalStack = new PortalStack();
			portalStack.Rectangle = viewDef.Scissor;
			portalStack.PortalPlaneCount = planeCount;

			for(int i = 0; i < planeCount; i++)
			{
				portalStack.PortalPlanes[i] = planes[i];
			}

			if(viewDef.AreaNumber < 0)
			{
				for(int i = 0; i < _portalAreaCount; i++)
				{
					_areaScreenRect[i] = viewDef.Scissor;
				}

				// if outside the world, mark everything
				for(int i = 0; i < _portalAreaCount; i++)
				{
					AddAreaReferences(i, portalStack);
				}
			}
			else
			{
				for(int i = 0; i < _portalAreaCount; i++)
				{
					_areaScreenRect[i] = new idScreenRect();
				}

				// flood out through portals, setting area viewCount
				FloodViewThroughArea(origin, viewDef.AreaNumber, portalStack);
			}		
		}	

		private void FreeDefs()
		{
			_generateInteractionsCalled = false;
			int count;

			/*if(_interactionTable != null)
			{
				_interactionTable = null;
			}*/

			// free all lightDefs
			idConsole.Warning("TODO: free light defs");

			/*for(i = 0; i < lightDefs.Num(); i++)
			{
				idRenderLightLocal* light;

				light = lightDefs[i];
				if(light && light->world == this)
				{
					FreeLightDef(i);
					lightDefs[i] = NULL;
				}
			}*/

			// free all entityDefs
			count = _entityDefinitions.Count;

			for(int i = 0; i < count; i++)
			{
				idRenderEntity mod = _entityDefinitions[i];

				if((mod != null) && (mod.World == this))
				{
					FreeEntityDef(i);
					_entityDefinitions[i] = null;
				}
			}
		}

		private void FreeEntityDef(int index)
		{
			if((index < 0) || (index > _entityDefinitions.Count))
			{
				idConsole.WriteLine("idRenderWorld::FreeEntityDef: handle {0} > {1}", index, _entityDefinitions.Count);
				return;
			}

			idRenderEntity def = _entityDefinitions[index];

			if(def == null)
			{
				idConsole.WriteLine("idRenderWorld::FreeEntityDef: handle {0} is null", index);
				return;
			}

			FreeEntityDefDerivedData(def, false, false);

			// TODO
			/*if ( session->writeDemo && def->archived ) {
				WriteFreeEntity( entityHandle );
			}*/

			// if we are playing a demo, these will have been freed
			// in R_FreeEntityDefDerivedData(), otherwise the gui
			// object still exists in the game

			def.Parameters.Gui[0] = null;
			def.Parameters.Gui[1] = null;
			def.Parameters.Gui[2] = null;
			
			_entityDefinitions[index] = null;
		}

		private void FreeEntityDefDerivedData(idRenderEntity def, bool keepDecals, bool keepCachedDynamicModel) 
		{
			// TODO:

			// demo playback needs to free the joints, while normal play
			// leaves them in the control of the game			
			/*if ( session->readDemo ) {
				if ( def->parms.joints ) {
					Mem_Free16( def->parms.joints );
					def->parms.joints = NULL;
				}
				if ( def->parms.callbackData ) {
					Mem_Free( def->parms.callbackData );
					def->parms.callbackData = NULL;
				}
				for ( i = 0; i < MAX_RENDERENTITY_GUI; i++ ) {
					if ( def->parms.gui[ i ] ) {
						delete def->parms.gui[ i ];
						def->parms.gui[ i ] = NULL;
					}
				}
			}*/

			// free all the interactions
			/*while ( def->firstInteraction != NULL ) {
				def->firstInteraction->UnlinkAndFree();
			}*/

			// clear the dynamic model if present
			if(def.DynamicModel != null)
			{
				def.DynamicModel = null;
			}

			if(keepDecals == false)
			{
				idConsole.Warning("TODO: free decals");

				/*R_FreeEntityDefDecals( def );
				R_FreeEntityDefOverlay( def );*/
			}

			if(keepCachedDynamicModel == false)
			{
				if(def.CachedDynamicModel != null)
				{
					def.CachedDynamicModel.Dispose();
					def.CachedDynamicModel = null;
				}
			}

			// free the entityRefs from the areas
			AreaReference areaRef, next;

			for(areaRef = def.EntityReference; areaRef != null;  areaRef = next)
			{
				next = areaRef.NextOwner;

				// unlink from the area
				areaRef.NextArea.PreviousArea = areaRef.PreviousArea;
				areaRef.PreviousArea.NextArea = areaRef.NextArea;
			}	

			def.EntityReference = null;
		}

		private void FreeWorld()
		{
			// this will free all the lightDefs and entityDefs
			FreeDefs();

			// free all the portals and check light/model references
			if(_portalAreas != null)
			{
				_portalAreas = null;
				_portalAreaCount = 0;
			}

			if(_portalAreas != null)
			{
				_portalAreas = null;
				_portalAreaCount = 0;
				_areaScreenRect = null;
			}

			if(_doublePortals != null)
			{
				_doublePortals = null;
				_interAreaPortalCount = 0;
			}

			if(_areaNodes != null)
			{
				_areaNodes = null;
			}

			// free all the inline idRenderModels 
			foreach(idRenderModel model in _localModels)
			{
				model.Dispose();
				idE.RenderModelManager.RemoveModel(model);
			}

			_localModels.Clear();
			_mapName = "<FREED>";
		}

		private void ParseInterAreaPortals(idLexer lexer)
		{
			lexer.ExpectTokenString("{");

			_portalAreaCount = lexer.ParseInt();

			if(_portalAreaCount < 0)
			{
				lexer.Error("ParseInterAreaPortals: bad portalAreaCount");
			}

			_portalAreas = new PortalArea[_portalAreaCount];
			_areaScreenRect = new idScreenRect[_portalAreaCount];

			for(int i = 0; i < _portalAreaCount; i++)
			{
				_portalAreas[i] = new PortalArea();
				_areaScreenRect[i] = new idScreenRect();
			}

			// set the doubly linked lists
			SetupAreaReferences();

			_interAreaPortalCount = lexer.ParseInt();

			if(_interAreaPortalCount < 0)
			{
				lexer.Error("ParseInterAreaPortals: bad interAreaPortalCount");
			}

			_doublePortals = new DoublePortal[_interAreaPortalCount];

			for(int i = 0; i < _interAreaPortalCount; i++)
			{
				int pointCount = lexer.ParseInt();
				int a1 = lexer.ParseInt();
				int a2 = lexer.ParseInt();

				idWinding w = new idWinding(pointCount);

				for(int j = 0; j < pointCount; j++)
				{
					float[] tmp = lexer.Parse1DMatrix(3);

					w[j,0] = tmp[0];
					w[j,1] = tmp[1];
					w[j,2] = tmp[2];

					// no texture coordinates
					w[j,3] = 0;
					w[j,4] = 0;
				}

				// add the portal to a1
				Portal p = new Portal();
				p.IntoArea = a2;
				p.DoublePortal = _doublePortals[i];
				p.Winding = w;
				p.Plane = w.GetPlane();
				p.Next = _portalAreas[a1].Portals;

				_portalAreas[a1].Portals = p;
				_doublePortals[i].Portals[0] = p;
			}

			lexer.ExpectTokenString("}");
		}

		private idRenderModel ParseModel(idLexer lexer)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			RenderModelSurface modelSurface;

			lexer.ExpectTokenString("{");

			// parse the name
			idToken token = lexer.ExpectAnyToken();

			idRenderModel model = idE.RenderModelManager.AllocateModel();
			model.InitEmpty(token.ToString());

			int surfaceCount = lexer.ParseInt();
			int loopCount = 0;

			if(surfaceCount < 0)
			{
				lexer.Error("ParseModel: bad surfaceCount");
			}

			for(int i = 0; i < surfaceCount; i++)
			{
				lexer.ExpectTokenString("{");
				token = lexer.ExpectAnyToken();

				modelSurface = new RenderModelSurface();
				modelSurface.Material = idE.DeclManager.FindMaterial(token.ToString());
				modelSurface.Material.AddReference();

				modelSurface.Geometry = new Surface();
				modelSurface.Geometry.Vertices = new Vertex[lexer.ParseInt()];
				modelSurface.Geometry.Indexes = new int[lexer.ParseInt()];

				loopCount = modelSurface.Geometry.Vertices.Length;

				for(int j = 0; j < loopCount; j++)
				{
					float[] vec = lexer.Parse1DMatrix(8);

					modelSurface.Geometry.Vertices[j].Position = new Vector3(vec[0], vec[1], vec[2]);
					modelSurface.Geometry.Vertices[j].TextureCoordinates = new Vector2(vec[3], vec[4]);
					modelSurface.Geometry.Vertices[j].Normal = new Vector3(vec[5], vec[6], vec[7]);
				}

				loopCount = modelSurface.Geometry.Indexes.Length;

				for(int j = 0; j < loopCount; j++)
				{
					modelSurface.Geometry.Indexes[j] = lexer.ParseInt();
				}

				lexer.ExpectTokenString("}");

				// add the completed surface to the model
				model.AddSurface(modelSurface);
			}

			lexer.ExpectTokenString("}");
			model.FinishSurfaces();

			return model;
		}

		private void ParseNodes(idLexer lexer)
		{
			lexer.ExpectTokenString("{");

			_areaNodeCount = lexer.ParseInt();

			if(_areaNodeCount < 0)
			{
				lexer.Error("ParseNodes: bad areaNodeCount");
			}

			_areaNodes = new AreaNode[_areaNodeCount];

			float[] tmp;
			AreaNode node;

			for(int i = 0; i < _areaNodeCount; i++)
			{
				node = _areaNodes[i];
				tmp = lexer.Parse1DMatrix(4);
				
				node.Plane = new Plane(tmp[0], tmp[1], tmp[2], tmp[3]);
				node.Children[0] = lexer.ParseInt();
				node.Children[1] = lexer.ParseInt();
			}

			lexer.ExpectTokenString("}");
		}

		private idRenderModel ParseShadowModel(idLexer lexer)
		{
			lexer.ExpectTokenString("{");

			// parse the name
			idToken token = lexer.ExpectAnyToken();

			idRenderModel model = idE.RenderModelManager.AllocateModel();
			model.InitEmpty(token.ToString());

			RenderModelSurface modelSurface = new RenderModelSurface();
			modelSurface.Material = idE.RenderSystem.DefaultMaterial;

			modelSurface.Geometry = new Surface();
			modelSurface.Geometry.ShadowVertices = new ShadowVertex[lexer.ParseInt()];
			modelSurface.Geometry.ShadowIndexesNoCapsCount = lexer.ParseInt();
			modelSurface.Geometry.ShadowIndexesNoFrontCapsCount = lexer.ParseInt();
			modelSurface.Geometry.Indexes = new int[lexer.ParseInt()];
			modelSurface.Geometry.ShadowCapPlaneBits = lexer.ParseInt();

			int count = modelSurface.Geometry.Vertices.Length;

			for(int j = 0; j < count; j++)
			{
				float[] vec = lexer.Parse1DMatrix(8);

				modelSurface.Geometry.ShadowVertices[j].Position = new Vector4(vec[0], vec[1], vec[2], 1);
				modelSurface.Geometry.Bounds.AddPoint(modelSurface.Geometry.ShadowVertices[j].Position);
			}

			count = modelSurface.Geometry.Indexes.Length;

			for(int j = 0; j < count; j++)
			{
				modelSurface.Geometry.Indexes[j] = lexer.ParseInt();
			}

			// add the completed surface to the model
			model.AddSurface(modelSurface);

			lexer.ExpectTokenString("}");

			// we do NOT do a model->FinishSurfaces, because we don't need sil edges, planes, tangents, etc.
			//	model.FinishSurfaces();

			return model;
		}

		/// <summary>
		/// Will return -1 if the point is not in an area, otherwise
		/// it will return 0 <= value < tr.world->numPortalAreas.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public int PointInArea(Vector3 point)
		{
			AreaNode node = _areaNodes[0];
			int nodeNumber = -1;

			if(node == null)
			{
				return -1;
			}

			while(true)
			{
				float d = (point * node.Plane.Normal + new Vector3(node.Plane.Normal.Z, node.Plane.Normal.Z, node.Plane.Normal.Z)).Length();
				
				if(d > 0)
				{
					nodeNumber = node.Children[0];
				}
				else
				{
					nodeNumber = node.Children[1];
				}

				if(nodeNumber == 0)
				{
					return -1; // in solid
				}

				if(nodeNumber < 0)
				{
					nodeNumber = -1 - nodeNumber;

					if(nodeNumber > _portalAreaCount)
					{
						idConsole.Error("idRenderWorld::PointInArea: area out of range");
					}

					return nodeNumber;
				}

				node = _areaNodes[nodeNumber];
			}	
	
			return -1;
		}

		private bool PortalIsFoggedOut(Portal portal)
		{
			// TODO: need lighting

			return false;
		}

		private idScreenRect ScreenRectangleFromWinding(idWinding winding, ViewEntity space)
		{
			idScreenRect rect = new idScreenRect();
			Vector3 v, ndc;

			View viewDef = idE.RenderSystem.ViewDefinition;

			for(int i = 0; i < winding.PointCount; i++)
			{
				idHelper.LocalPointToGlobal(space.ModelMatrix, winding[i], out v);
				idHelper.GlobalToNormalizedDeviceCoordinates(v, out ndc);

				float windowX = 0.5f * (1.0f + ndc.X) * (viewDef.ViewPort.X2 - viewDef.ViewPort.X1);
				float windowY = 0.5f * (1.0f + ndc.Y) * (viewDef.ViewPort.Y2 - viewDef.ViewPort.Y1);

				rect.AddPoint(windowX, windowY);
			}

			rect.Expand();
		
			return rect;
		}

		/// <summary>
		/// If the entityDef isn't already on the viewEntity list, create
		/// a viewEntity and add it to the list with an empty scissor rect.
		/// </summary>
		/// <remarks>
		/// This does not instantiate dynamic models for the entity yet.
		/// </remarks>
		/// <param name="def"></param>
		/// <returns></returns>
		private ViewEntity SetEntityDefViewEntity(idRenderEntity def)
		{
			if(def.ViewCount == idE.RenderSystem.ViewCount)
			{
				return def.ViewEntity;
			}

			def.ViewCount = idE.RenderSystem.ViewCount;

			// set the model and modelview matricies
			ViewEntity viewModel = new ViewEntity();
			viewModel.EntityDef = def;

			// the scissorRect will be expanded as the model bounds is accepted into visible portal chains

			// copy the model and weapon depth hack for back-end use
			viewModel.ModelDepthHack = def.Parameters.ModelDepthHack;
			viewModel.WeaponDepthHack = def.Parameters.WeaponDepthHack;

			viewModel.ModelMatrix = idHelper.AxisToModelMatrix(def.Parameters.Axis, def.Parameters.Origin);

			// we may not have a viewDef if we are just creating shadows at entity creation time
			if(idE.RenderSystem.ViewDefinition != null)
			{
				idHelper.ConvertMatrix(viewModel.ModelMatrix, idE.RenderSystem.ViewDefinition.WorldSpace.ModelViewMatrix, out viewModel.ModelViewMatrix);
				idE.RenderSystem.ViewDefinition.ViewEntities.Add(viewModel);
			}

			def.ViewEntity = viewModel;

			return viewModel;
		}

		private void SetupAreaReferences()
		{
			_connectedAreaNumber = 0;

			idConsole.Warning("TODO: light refs");
	
			for(int i = 0; i < _portalAreaCount; i++)
			{
				_portalAreas[i].AreaNumber = i;
				
				/*portalAreas[i].lightRefs.areaNext =
				portalAreas[i].lightRefs.areaPrev =
					&portalAreas[i].lightRefs;*/

				_portalAreas[i].EntityReference.NextArea 
					= _portalAreas[i].EntityReference.PreviousArea
						= _portalAreas[i].EntityReference;
			}
		}
		#endregion
		#endregion

		#region IDisposable implementation
		#region Properties
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion

		#region Members
		private bool _disposed;
		#endregion

		#region Methods
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// free all the entityDefs, lightDefs, portals, etc
			if(disposing == true)
			{
				FreeWorld();
			}

			// free up the debug lines, polys, and text
			idE.RenderSystem.DebugClearPolygons(0);
			idE.RenderSystem.DebugClearLines(0);
			idE.RenderSystem.DebugClearText(0);
		}
		#endregion
		#endregion
	}

	[Flags]
	public enum PortalConnection : byte
	{
		BlockNone = 0,

		BlockView = 1,

		/// <summary>Game map location strings often stop in hallways.</summary>
		BlockLocation = 2,
		/// <summary>Windows between pressurized and unpresurized areas.</summary>
		BlockAir = 4,

		BlockAll = (1 << 3) - 1
	}

	public class Portal
	{
		/// <summary>
		/// Area this portal leads to.
		/// </summary>
		public int IntoArea;

		/// <summary>
		/// Winding points have counter clockwise ordering seen this area.
		/// </summary>
		public idWinding Winding;

		/// <summary>
		/// View must be on the positive side of the plane to cross.
		/// </summary>
		public Plane Plane;

		/// <summary>
		/// Next portal of the area.
		/// </summary>
		public Portal Next; 

		public DoublePortal DoublePortal;
	}

	public class DoublePortal
	{
		public Portal[] Portals = new Portal[2];

		/// <summary>
		/// PS_BLOCK_VIEW, PS_BLOCK_AIR, etc, set by doors that shut them off.
		/// </summary>
		public PortalConnection BlockingBits;

		// a portal will be considered closed if it is past the
		// fog-out point in a fog volume.  We only support a single
		// fog volume over each portal.
		// TODO: idRenderLightLocal *		fogLight;

		public DoublePortal NextFoggedPortal;
	}

	public class PortalArea
	{
		public int AreaNumber;

		/// <summary>
		/// If two areas have matching connectedAreaNum, they are not separated by a portal with the apropriate PS_BLOCK_* blockingBits.
		/// </summary>
		public int[] ConnectedAreaNumber = new int[3];

		/// <summary>
		/// Set by FindViewLightsAndEntities.
		/// </summary>
		public int ViewCount;

		/// <summary>
		/// Never changes after load.
		/// </summary>
		public Portal Portals;

		/// <summary>
		/// Head/tail of doubly linked list, may change.
		/// </summary>
		public AreaReference EntityReference = new AreaReference();

		/// <summary>
		/// Head/tail of doubly linked list, may change.
		/// </summary>
		public AreaReference LightReference = new AreaReference();
	}

	public class AreaNode
	{
		public Plane Plane;

		/// <summary>
		/// Negative numbers are (-1 - areaNumber), 0 = solid.
		/// </summary>
		public int[] Children = new int[2];

		/// <summary>
		/// If all children are either solid or a single area, this is the area number, else CHILDREN_HAVE_MULTIPLE_AREAS.
		/// </summary>
		public int CommonChildrenArea;
	}

	/// <summary>
	/// Areas have references to hold all the lights and entities in them.
	/// </summary>
	public class AreaReference
	{
		public AreaReference NextArea;
		public AreaReference PreviousArea;
		public AreaReference NextOwner;

		public idRenderEntity Entity;
		// idRenderLightLocal *	light;					// only one of entity / light will be non-NULL
		public PortalArea Area;
	}
	
	public class PortalStack
	{
		public Portal Portal;
		public PortalStack Next;

		public idScreenRect Rectangle;

		public int PortalPlaneCount;
		public Plane[] PortalPlanes = new Plane[idE.MaxPortalPlanes + 1];
	}
}