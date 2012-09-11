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

using idTech4.Renderer;
using idTech4.Text;

namespace idTech4.Collision
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Trace model vs. polygonal model collision detection.
	/// <p/>
	/// Short translations are the least expensive. Retrieving contact points is
	/// about as cheap as a short translation. Position tests are more expensive
	/// and rotations are most expensive.
	/// <p/>
	/// There is no position test at the start of a translation or rotation. In other
	/// words if a translation with start != end or a rotation with angle != 0 starts
	/// in solid, this goes unnoticed and the collision result is undefined.
	/// <p/>
	/// A translation with start == end or a rotation with angle == 0 performs
	/// a position test and fills in the trace_t structure accordingly.
	/// </remarks>
	public sealed class idCollisionModelManager
	{
		#region Constants
		public const string Extension = ".cm";
		public const string TokenFileID = "CM";
		public const string FileVersion = "1.00";
		#endregion

		#region Members
		private int _checkCount;
		private bool _isLoaded;
		private string _mapName = string.Empty;

		private CollisionModel[] _models;
		private int _modelCount;

		private idMaterial _traceModelMaterial;
		#endregion

		#region Constructor
		public idCollisionModelManager()
		{

		}
		#endregion

		#region Methods
		#region Public
		public CollisionModel FindModel(string name)
		{
			// check if this model is already loaded
			int i, count = _models.Length;

			for(i = 0; i < count; i++)
			{
				if((_models[i] != null) && (_models[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true))
				{
					return _models[i];
				}
			}

			return null;
		}

		public void LoadMap(idMapFile mapFile)
		{
			if(mapFile == null)
			{
				idConsole.Error("idCollisionModelManager::LoadMap: NULL mapFile");
			}

			// check whether we can keep the current collision map based on the mapName and mapFileTime
			if(_isLoaded == true)
			{
				if(_mapName.Equals(mapFile.Name, StringComparison.OrdinalIgnoreCase) == true)
				{
					idConsole.Warning("TODO: loadmap load check");
					/*if ( mapFile->GetFileTime() == mapFileTime ) {
						common->DPrintf( "Using loaded version\n" );
						return;
					}*/

					idConsole.DeveloperWriteLine("Reloading modified map");
				}

				idConsole.Warning("TODO: FreeMap();");
			}

			// clear the collision map
			Clear();

			// models
			_models = new CollisionModel[idE.MaxSubModels + 1];
			_modelCount = 0;

			// setup hash to speed up finding shared vertices and edges
			idConsole.Warning("TODO: SetupHash();");

			// setup trace model structure
			SetupTraceModelStructure();

			// build collision models
			BuildModels(mapFile);

			// save name and time stamp
			_mapName = mapFile.Name;
			idConsole.Warning("TODO: mapFileTime = mapFile->GetFileTime();");

			_isLoaded = true;

			// shutdown the hash
			idConsole.Warning("TODO: ShutdownHash();");
		}

		public CollisionModel LoadModel(string model, bool precache)
		{
			CollisionModel collisionModel = FindModel(model);

			if(collisionModel != null)
			{
				return collisionModel;
			}


			// try to load a .cm file
			if(LoadCollisionModelFile(model, 0) == true)
			{
				collisionModel = FindModel(model);

				if(collisionModel != null)
				{
					return collisionModel;
				}
				else
				{
					idConsole.Warning("idCollisionModelManagerLocal::LoadModel: collision file for '{0}' contains different model", model);
				}
			}

			// if only precaching .cm files do not waste memory converting render models
			if(precache == true)
			{
				return null;
			}

			// try to load a .ASE or .LWO model and convert it to a collision model
			idConsole.Warning("TODO: collisionModel = LoadRenderModel(model);");

			if(collisionModel != null)
			{
				_models[_modelCount++] = collisionModel;
			}

			return collisionModel;
		}
		#endregion

		#region Private
		private void BuildModels(idMapFile mapFile)
		{
			idConsole.Warning("TODO: idTimer");
			/*idTimer timer;
			timer.Start();*/

			if(LoadCollisionModelFile(mapFile.Name, 0 /* TODO: mapFile->GetGeometryCRC()*/) == false)
			{
				idConsole.Warning("TODO: no collision model, compile");

				/*if ( !mapFile->GetNumEntities() ) {
					return;
				}

				// load the .proc file bsp for data optimisation
				LoadProcBSP( mapFile->GetName() );

				// convert brushes and patches to collision data
				for ( i = 0; i < mapFile->GetNumEntities(); i++ ) {
					mapEnt = mapFile->GetEntity(i);

					if ( numModels >= MAX_SUBMODELS ) {
						common->Error( "idCollisionModelManagerLocal::BuildModels: more than %d collision models", MAX_SUBMODELS );
						break;
					}
					models[numModels] = CollisionModelForMapEntity( mapEnt );
					if ( models[ numModels] ) {
						numModels++;
					}
				}

				// free the proc bsp which is only used for data optimization
				Mem_Free( procNodes );
				procNodes = NULL;

				// write the collision models to a file
				WriteCollisionModelsToFile( mapFile->GetName(), 0, numModels, mapFile->GetGeometryCRC() );*/
			}

			idConsole.Warning("TODO: timer.Stop();");

			// print statistics on collision data
			/*cm_model_t model;
			AccumulateModelInfo( &model );
			common->Printf( "collision data:\n" );
			common->Printf( "%6i models\n", numModels );
			PrintModelInfo( &model );
			common->Printf( "%.0f msec to load collision data.\n", timer.Milliseconds() );*/
		}

		private void Clear()
		{
			idConsole.Warning("TODO: idCollisionModelManager.Clear");		
	
			_mapName = string.Empty;
			_isLoaded = false;
			_checkCount = 0;
			_models = null;
			_modelCount = 0;

			/*mapFileTime = 0;			
			memset( trmPolygons, 0, sizeof( trmPolygons ) );
			trmBrushes[0] = NULL;
			trmMaterial = NULL;
			numProcNodes = 0;
			procNodes = NULL;
			getContacts = false;
			contacts = NULL;
			maxContacts = 0;
			numContacts = 0;*/
		}

		private ContentFlags ContentsFromString(string str)
		{
			idLexer lexer = new idLexer();
			lexer.LoadMemory(str, "ContentsFromString");

			idToken token;
			ContentFlags contents = ContentFlags.None;
			string tmp;

			while((token = lexer.ReadToken()) != null)
			{
				if(token.ToString() == ",")
				{
					continue;
				}

				tmp = token.ToString();

				switch(tmp)
				{
					case "aas_solid":
						tmp = "AasSolid";
						break;

					case "aas_obstacle":
						tmp = "AasObstacle";
						break;

					case "flashlight_trigger":
						tmp = "FlashlightTrigger";
						break;
				}

				contents |= (ContentFlags) Enum.Parse(typeof(ContentFlags), tmp, true);
			}

			return contents;
		}

		private void FilterBrushIntoTree(CollisionModel model, CollisionModelNode node, CollisionModelBrush b)
		{
			while(node.PlaneType != -1)
			{
				if(InsideAllChildren(node, b.Bounds) == true)
				{
					break;
				}

				float v = (node.PlaneType == 0) ? b.Bounds.Min.X : (node.PlaneType == 1) ? b.Bounds.Min.Y : b.Bounds.Min.Z;
				float v2 = (node.PlaneType == 0) ? b.Bounds.Max.X : (node.PlaneType == 1) ? b.Bounds.Max.Y : b.Bounds.Max.Z;

				if(v >= node.PlaneDistance)
				{
					node = node.Children[0];
				}
				else if(v2 <= node.PlaneDistance)
				{
					node = node.Children[1];
				}
				else
				{
					FilterBrushIntoTree(model, node.Children[1], b);
					node = node.Children[0];
				}
			}

			node.Brushes.Add(b);
		}

		private void FilterPolygonIntoTree(CollisionModel model, CollisionModelNode node, CollisionModelPolygon p)
		{
			while(node.PlaneType != -1)
			{
				if(InsideAllChildren(node, p.Bounds) == true)
				{					
					break;
				}

				float v = (node.PlaneType == 0) ? p.Bounds.Min.X : (node.PlaneType == 1) ? p.Bounds.Min.Y : p.Bounds.Min.Z;
				float v2 = (node.PlaneType == 0) ? p.Bounds.Max.X : (node.PlaneType == 1) ? p.Bounds.Max.Y : p.Bounds.Max.Z;

				if(v >= node.PlaneDistance)
				{
					node = node.Children[0];
				}
				else if(v2 <= node.PlaneDistance)
				{
					node = node.Children[1];
				}
				else
				{
					FilterPolygonIntoTree(model, node.Children[1], p);
					node = node.Children[0];
				}
			}

			node.Polygons.Add(p);
		}

		private idBounds GetNodeBounds(CollisionModelNode node)
		{
			idBounds bounds = idBounds.Zero;
			bounds.Clear();

			GetNodeBounds_R(ref bounds, node);

			if(bounds.IsCleared == true)
			{
				bounds = idBounds.Zero;
			}

			return bounds;
		}

		private void GetNodeBounds_R(ref idBounds bounds, CollisionModelNode node)
		{
			while(true)
			{
				foreach(CollisionModelPolygon poly in node.Polygons)
				{
					bounds.AddPoint(poly.Bounds.Min);
					bounds.AddPoint(poly.Bounds.Max);
				}

				foreach(CollisionModelBrush brush in node.Brushes)
				{
					bounds.AddPoint(brush.Bounds.Min);
					bounds.AddPoint(brush.Bounds.Max);
				}

				if(node.PlaneType == -1)
				{
					break;
				}

				GetNodeBounds_R(ref bounds, node.Children[1]);
				node = node.Children[0];
			}
		}

		private ContentFlags GetNodeContents(CollisionModelNode node)
		{
			ContentFlags contents = 0;
			
			while(true)
			{
				foreach(CollisionModelPolygon p in node.Polygons)
				{
					contents |= p.Contents;
				}

				foreach(CollisionModelBrush b in node.Brushes)
				{
					contents |= b.Contents;
				}

				if(node.PlaneType == -1)
				{
					break;
				}

				contents |= GetNodeContents(node.Children[1]);
				node = node.Children[0];
			}

			return contents;
		}

		private bool InsideAllChildren(CollisionModelNode node, idBounds bounds)
		{
			if(node.PlaneType != -1)
			{
				float v = (node.PlaneType == 0) ? bounds.Min.X : (node.PlaneType == 1) ? bounds.Min.Y : bounds.Min.Z;
				float v2 = (node.PlaneType == 0) ? bounds.Max.X : (node.PlaneType == 1) ? bounds.Max.Y : bounds.Max.Z;

				if(v >= node.PlaneDistance)
				{
					return false;
				}
				else if(v2 <= node.PlaneDistance)
				{
					return false;
				}
				else if(InsideAllChildren(node.Children[0], bounds) == false)
				{
					return false;
				}
				else if(InsideAllChildren(node.Children[1], bounds) == false)
				{
					return false;
				}
			}

			return true;
		}

		private bool LoadCollisionModelFile(string name, ulong mapFileCRC)
		{
			// load it
			string fileName = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name) + Extension);

			idLexer lexer = new idLexer(LexerOptions.NoStringConcatination | LexerOptions.NoDollarPrecompilation);
			
			if(lexer.LoadFile(fileName) == false)
			{
				return false;
			}

			idToken token;

			if(lexer.ExpectTokenString(TokenFileID) == false)
			{
				idConsole.Warning("{0} is not a CM file.", fileName);
			}
			else if(((token = lexer.ReadToken()) == null) || (token.ToString() != FileVersion))
			{
				idConsole.Warning("{0} has version {1} instead of {2}", fileName, token, FileVersion);
			}
			else if((token = lexer.ExpectTokenType(TokenType.Number, TokenSubType.Integer)) == null)
			{
				idConsole.Warning("{0} has no map file CRC", fileName);
			}
			else
			{
				ulong crc = token.ToUInt64();

				if((mapFileCRC != 0) && (crc != mapFileCRC))
				{
					idConsole.WriteLine("{0} is out of date", fileName);
				}
				else
				{
					// parse the file
					while(true)
					{
						if((token = lexer.ReadToken()) == null)
						{
							break;
						}

						if(token.ToString().ToLower() == "collisionmodel")
						{
							if(ParseCollisionModel(lexer) == false)
							{
								return false;
							}
						}
						else
						{
							lexer.Error("idCollisionModelManagerLocal::LoadCollisionModelFile: bad token \"{0}\"", token);
						}
					}

					return true;
				}
			}
			
			return false;
		}

		private void ParseBrushes(idLexer lexer, CollisionModel model)
		{
			idToken token = lexer.CheckTokenType(TokenType.Number, 0);
			int planeCount;
			CollisionModelBrush b;
			float[] tmp;

			lexer.ExpectTokenString("{");

			while(lexer.CheckTokenString("}") == false)
			{
				// parse brush
				planeCount = lexer.ParseInt();

				b = new CollisionModelBrush();
				b.Contents = ContentFlags.All;
				b.Material = _traceModelMaterial;
				b.Planes = new Plane[planeCount];

				lexer.ExpectTokenString("{");

				for(int i = 0; i < planeCount; i++)
				{
					tmp = lexer.Parse1DMatrix(3);

					b.Planes[i].Normal = new Vector3(tmp[0], tmp[1], tmp[2]);
					b.Planes[i].D = lexer.ParseFloat();
				}

				lexer.ExpectTokenString("}");

				tmp = lexer.Parse1DMatrix(3);
				b.Bounds.Min = new Vector3(tmp[0], tmp[1], tmp[2]);

				tmp = lexer.Parse1DMatrix(3);
				b.Bounds.Max = new Vector3(tmp[0], tmp[1], tmp[2]);

				token = lexer.ReadToken();

				if(token.Type == TokenType.Number)
				{
					b.Contents = (ContentFlags) token.ToInt32(); // old .cm files use a single integer
				}
				else
				{
					b.Contents = ContentsFromString(token.ToString());
				}

				b.CheckCount = 0;
				b.PrimitiveCount = 0;

				// filter brush into tree
				FilterBrushIntoTree(model, model.Node, b);
			}
		}

		private bool ParseCollisionModel(idLexer lexer) 
		{
			CollisionModel model = new CollisionModel();

			_models[_modelCount++] = model;

			// parse the file
			idToken token = lexer.ExpectTokenType(TokenType.String, 0);
			string tokenLower;

			model.Name = token.ToString();
			lexer.ExpectTokenString("{");

			while(lexer.CheckTokenString("}") == false)
			{
				token = lexer.ReadToken();
				tokenLower = token.ToString().ToLower();

				if(tokenLower == "vertices")
				{
					ParseVertices(lexer, model);
				}
				else if(tokenLower == "edges")
				{
					ParseEdges(lexer, model);
				}
				else if(tokenLower == "nodes")
				{
					lexer.ExpectTokenString("{");
					model.Node = ParseNodes(lexer, model, null);
					lexer.ExpectTokenString("}");
				}
				else if(tokenLower == "polygons")
				{
					ParsePolygons(lexer, model);
				}
				else if(tokenLower == "brushes")
				{
					ParseBrushes(lexer, model);
				}
				else
				{
					lexer.Error("ParseCollisionModel: bad token \"{0}\"", token);
				}
			}

			// calculate edge normals
			_checkCount++;

			idConsole.Warning("TODO: CalculateEdgeNormals(model, model.Node);");

			// get model bounds from brush and polygon bounds
			model.Bounds = GetNodeBounds(model.Node);

			// get model contents
			model.Contents = GetNodeContents(model.Node);

			idConsole.Warning("TODO: used memory");

			// total memory used by this model
			/*model->usedMemory = model->numVertices * sizeof(cm_vertex_t) +
								model->numEdges * sizeof(cm_edge_t) +
								model->polygonMemory +
								model->brushMemory +
									model->numNodes * sizeof(cm_node_t) +
								model->numPolygonRefs * sizeof(cm_polygonRef_t) +
								model->numBrushRefs * sizeof(cm_brushRef_t);*/

			return true;
		}

		private void ParseEdges(idLexer lexer, CollisionModel model)
		{
			lexer.ExpectTokenString("{");

			int edgeCount = lexer.ParseInt();

			model.Edges = new CollisionModelEdge[edgeCount];

			for(int i = 0; i < edgeCount; i++)
			{
				lexer.ExpectTokenString("(");
				model.Edges[i].VertexCount = new int[] { lexer.ParseInt(), lexer.ParseInt() };
				lexer.ExpectTokenString(")");

				model.Edges[i].Side = 0;
				model.Edges[i].SideSet = 0;
				model.Edges[i].Internal = (ushort) lexer.ParseInt();
				model.Edges[i].UserCount = (ushort) lexer.ParseInt();
				model.Edges[i].Normal = Vector3.Zero;
				model.Edges[i].CheckCount = 0;
				model.InternalEdgeCount += model.Edges[i].Internal;
			}

			lexer.ExpectTokenString("}");
		}

		private CollisionModelNode ParseNodes(idLexer lexer, CollisionModel model, CollisionModelNode parent)
		{
			model.NodeCount++;

			lexer.ExpectTokenString("(");

			CollisionModelNode node = new CollisionModelNode();
			node.Parent = parent;
			node.PlaneType = lexer.ParseInt();
			node.PlaneDistance = lexer.ParseFloat();

			lexer.ExpectTokenString(")");

			if(node.PlaneType != -1)
			{
				node.Children[0] = ParseNodes(lexer, model, node);
				node.Children[1] = ParseNodes(lexer, model, node);
			}

			return node;
		}

		private void ParsePolygons(idLexer lexer, CollisionModel model)
		{
			idToken token = lexer.CheckTokenType(TokenType.Number, 0);
			float[] tmp;
			Vector3 normal;

			lexer.ExpectTokenString("{");

			while(lexer.CheckTokenString("}") == false)
			{
				// parse polygon
				int edgeCount = lexer.ParseInt();

				CollisionModelPolygon p = new CollisionModelPolygon();
				p.Material = _traceModelMaterial;
				p.Contents = ContentFlags.All;
				p.Edges = new int[edgeCount];

				lexer.ExpectTokenString("(");

				for(int i = 0; i < edgeCount; i++)
				{
					p.Edges[i] = lexer.ParseInt();
				}

				lexer.ExpectTokenString(")");

				tmp = lexer.Parse1DMatrix(3);
				normal = new Vector3(tmp[0], tmp[1], tmp[2]);

				p.Plane.Normal = normal;
				p.Plane.D = lexer.ParseFloat();

				tmp = lexer.Parse1DMatrix(3);
				p.Bounds.Min = new Vector3(tmp[0], tmp[1], tmp[2]);

				tmp = lexer.Parse1DMatrix(3);
				p.Bounds.Max = new Vector3(tmp[0], tmp[1], tmp[2]);

				token = lexer.ExpectTokenType(TokenType.String, 0);

				// get material
				p.Material = idE.DeclManager.FindMaterial(token.ToString());
				p.Contents = p.Material.ContentFlags;
				p.CheckCount = 0;

				// filter polygon into tree
				FilterPolygonIntoTree(model, model.Node, p);
			}
		}

		private void ParseVertices(idLexer lexer, CollisionModel model)
		{
			lexer.ExpectTokenString("{");

			int vertexCount = lexer.ParseInt();
			model.Vertices = new CollisionModelVertex[vertexCount];

			for(int i = 0; i < vertexCount; i++)
			{
				float[] tmp = lexer.Parse1DMatrix(3);

				model.Vertices[i].Point = new Vector3(tmp[0], tmp[1], tmp[2]);
				model.Vertices[i].Side = 0;
				model.Vertices[i].SideSet = 0;
				model.Vertices[i].CheckCount = 0;
			}

			lexer.ExpectTokenString("}");
		}

		private void SetupTraceModelStructure()
		{
			// setup model
			CollisionModel model = new CollisionModel();

			_models[idE.MaxSubModels] = model;

			// create node to hold the collision data
			CollisionModelNode node = new CollisionModelNode();
			node.PlaneType = -1;
			model.Node = node;

			// allocate vertex and edge arrays
			//model.Vertices = new CollisionModelVertex[idE.MaxTraceModelVertices];
			//model->edges = (cm_edge_t *) Mem_ClearedAlloc( model->maxEdges * sizeof(cm_edge_t) );

			// create a material for the trace model polygons
			_traceModelMaterial = idE.DeclManager.FindMaterial("_tracemodel", false);

			if(_traceModelMaterial == null)
			{
				idConsole.FatalError("_tracemodel material not found");
			}

			// allocate polygons
			/*for ( i = 0; i < MAX_TRACEMODEL_POLYS; i++ ) {
				trmPolygons[i] = AllocPolygonReference( model, MAX_TRACEMODEL_POLYS );
				trmPolygons[i]->p = AllocPolygon( model, MAX_TRACEMODEL_POLYEDGES );
				trmPolygons[i]->p->bounds.Clear();
				trmPolygons[i]->p->plane.Zero();
				trmPolygons[i]->p->checkcount = 0;
				trmPolygons[i]->p->contents = -1;		// all contents
				trmPolygons[i]->p->material = trmMaterial;
				trmPolygons[i]->p->numEdges = 0;
			}
			// allocate brush for position test
			trmBrushes[0] = AllocBrushReference( model, 1 );
			trmBrushes[0]->b = AllocBrush( model, MAX_TRACEMODEL_POLYS );
			trmBrushes[0]->b->primitiveNum = 0;
			trmBrushes[0]->b->bounds.Clear();
			trmBrushes[0]->b->checkcount = 0;
			trmBrushes[0]->b->contents = -1;		// all contents
			trmBrushes[0]->b->numPlanes = 0;*/
		}
		#endregion
		#endregion
	}

	public enum ContactType
	{
		/// <summary>No contact.</summary>
		None,
		/// <summary>Trace model edge hits model edge.</summary>
		Edge,
		/// <summary>Model vertex hits trace model polygon.</summary>
		ModelVertex,
		/// <summary>Trace model vertex hits model polygon.</summary>
		TraceModelVertex
	}

	public struct ContactInfo
	{
		/// <summary>Contact type.</summary>
		public ContactType Type;
		/// <summary>Point of contact.</summary>
		public Vector3	Point;
		/// <summary>Contact plane normal.</summary>
		public Vector3 Normal;
		/// <summary>Contact plane distance.</summary>
		public float Distance;
		/// <summary>Contents at other side of surface.</summary>
		public int	Contents;
		/// <summary>Surface material.</summary>
		public idMaterial Material;
		/// <summary>Contact feature on model.</summary>
		public int	ModelFeature;
		/// <summary>Contact feature on trace model.</summary>
		public int	TraceModelFeature;
		/// <summary>Entity the contact surface is a part of.</summary>
		public int	EntityIndex;
		/// <summary>ID of the clip model the contact surface is part of.</summary>
		public int ID;
	}

	public class TraceResult
	{
		/// <summary>Fraction of movement completed, 1.0 = didn't hit anything.</summary>
		public float Fraction;
		/// <summary>Final position of trace model.</summary>
		public Vector3 EndPosition;
		/// <summary>Final axis of trace model.</summary>
		public Matrix EndAxis;
		/// <summary>Contact information, only valid if fraction < 1.0.</summary>
		public ContactInfo ContactInformation;
	}

	public class CollisionModel
	{
		/// <summary>Model name.</summary>
		public string Name = string.Empty;

		/// <summary>Model bounds.</summary>
		public idBounds Bounds;

		/// <summary>All contents of the model OR'ed together.</summary>
		public ContentFlags Contents;

		/// <summary>Set if model is convex.</summary>
		public bool IsConvex;

		/// <summary>Array with all vertices used by the model.</summary>
		public CollisionModelVertex[] Vertices;

		/// <summary>Array with all edges used by the model.</summary>
		public CollisionModelEdge[] Edges;

		public List<CollisionModelPolygon> Polygons = new List<CollisionModelPolygon>();
		public List<CollisionModelBrush> Brushes = new List<CollisionModelBrush>();

		/// <summary>First node of spatial subdivision.</summary>
		public CollisionModelNode Node;

		public int InternalEdgeCount;
		public int NodeCount;
	}

	public class CollisionModelNode
	{
		/// <summary>Node axial plane type.</summary>
		public int PlaneType;

		/// <summary>Node plane distance.</summary>
		public float PlaneDistance;

		/// <summary>Polygons in node.</summary>
		public List<CollisionModelPolygon> Polygons = new List<CollisionModelPolygon>();

		/// <summary>Brushes in node.</summary>
		public List<CollisionModelBrush> Brushes = new List<CollisionModelBrush>();

		/// <summary>Parent of this node.</summary>
		public CollisionModelNode Parent;

		/// <summary>Node children.</summary>
		public CollisionModelNode[] Children = new CollisionModelNode[2];
	}

	public struct CollisionModelVertex
	{
		/// <summary>Vertex point.</summary>
		public Vector3 Point;

		/// <summary>For multi-check avoidance.</summary>
		public int CheckCount;

		/// <summary>Each bit tells at which side this vertex passes one of the trace model edges.</summary>
		public ulong Side;

		/// <summary>Each bit tells if sidedness for the trace model edge has been calculated yet.</summary>
		public ulong SideSet;
	}

	public struct CollisionModelEdge
	{
		/// <summary>For multi-check avoidance.</summary>
		public int CheckCount;

		/// <summary>A trace model can never collide with internal edges.</summary>
		public ushort Internal;

		/// <summary>Number of polygons using this edge.</summary>
		public ushort UserCount;

		/// <summary>Each bit tells at which side of this edge one of the trace model vertices passes.</summary>
		public ulong Side;

		/// <summary>Each bit tells if sidedness for the trace model vertex has been calculated yet.</summary>
		public ulong SideSet;

		/// <summary>Start and end point of edge.</summary>
		public int[] VertexCount;

		/// <summary>Edge normal.</summary>
		public Vector3 Normal;
	}

	public struct CollisionModelPolygon
	{
		/// <summary>Polygon bounds.</summary>
		public idBounds Bounds;

		/// <summary>For multi-check avoidance.</summary>
		public int CheckCount;

		/// <summary>Contents behind polygon.</summary>
		public ContentFlags Contents;

		/// <summary>Material.</summary>
		public idMaterial Material;

		/// <summary>Polygon plane.</summary>
		public Plane Plane;

		/// <summary>Variable sized, indexes into cm_edge_t list.</summary>
		public int[] Edges;
	}

	public struct CollisionModelBrush
	{
		/// <summary>For multi-check avoidance.</summary>
		public int CheckCount;

		/// <summary>Brush bounds.</summary>
		public idBounds Bounds;

		/// <summary>Contents of brush.</summary>
		public ContentFlags Contents;

		/// <summary>Material.</summary>
		public idMaterial Material;

		/// <summary>Number of brush primitive.</summary>
		public int PrimitiveCount;

		/// <summary>Variable sized.</summary>
		public Plane[] Planes;
	}
}