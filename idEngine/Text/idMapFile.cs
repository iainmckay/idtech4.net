using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Geometry;
using idTech4.Renderer;

namespace idTech4.Text
{
	/// <summary>
	/// Reads or writes the contents of .map files into a standard internal
	/// format, which can then be moved into private formats for collision
	/// detection, map processing, or editor use.
	/// <para/>
	/// No validation (duplicate planes, null area brushes, etc) is performed.
	/// There are no limits to the number of any of the elements in maps.
	/// The order of entities, brushes, and sides is maintained.
	/// </summary>
	public class idMapFile : IDisposable
	{
		#region Constants
		public const int OldMapVersion = 1;
		public const int CurrentMapVersion = 2;

		public const int DefaultCurveSubdivision = 4;
		public const float DefaultCurveMaxError = 4.0f;
		public const float DefaultCurveMaxErrorCD = 24.0f;
		public const float DefaultCurveMaxLength = -1.0f;
		public const float DefaultCurveMaxLengthCD = -1.0f;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the number of entities in the map.
		/// </summary>
		public int EntityCount
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _entities.Count;
			}
		}

		/// <summary>
		/// Gets the filename without an extension.
		/// </summary>
		public string Name
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _name;
			}
		}

		/// <summary>
		/// True if the file on disk changed.
		/// </summary>
		public bool NeedsReload
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idMapFile.NeedsReload");
				return false;
			}
		}
		#endregion

		#region Members
		private float _version;
		private DateTime _fileTime;
		private uint _geometryCRC;
		private string _name;
		private bool _hasPrimitiveData;

		private List<idMapEntity> _entities = new List<idMapEntity>();
		#endregion

		#region Constructor
		public idMapFile()
		{

		}

		~idMapFile()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Gets the entity at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public idMapEntity GetEntity(int index)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _entities[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Normally this will use a .reg file instead of a .map file if it exists,
		/// which is what the game and dmap want, but the editor will want to always
		/// load a .map file.
		/// </remarks>
		/// <param name="fileName">Does not require an extension.</param>
		/// <param name="ignoreRegion"></param>
		/// <param name="osPath"></param>
		/// <returns></returns>
		public bool Parse(string fileName, bool ignoreRegion = false, bool osPath = false)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_hasPrimitiveData = false;
			_name = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));

			string fullName = _name;

			// no string concatenation for epairs and allow path names for materials
			idLexer lexer = new idLexer(LexerOptions.NoStringConcatination | LexerOptions.NoStringEscapeCharacters | LexerOptions.AllowPathNames);
			idMapEntity mapEnt;

			if(ignoreRegion == false)
			{
				// try loading a .reg file first
				lexer.LoadFile(fullName + ".reg", osPath);
			}

			if(lexer.IsLoaded == false)
			{
				// now try a .map file
				lexer.LoadFile(fullName + ".map", osPath);

				if(lexer.IsLoaded == false)
				{
					// didn't get anything at all
					return false;
				}
			}

			_version = idMapFile.OldMapVersion;
			_fileTime = lexer.FileTime;
			_entities.Clear();

			if(lexer.CheckTokenString("Version") == true)
			{
				_version = lexer.ReadTokenOnLine().ToFloat();
			}

			while(true)
			{
				if((mapEnt = idMapEntity.Parse(lexer, (_entities.Count == 0), _version)) == null)
				{
					break;
				}

				_entities.Add(mapEnt);
			}

			idConsole.Warning("TODO: SetGeometryCRC();");

			// if the map has a worldspawn
			if(_entities.Count > 0)
			{
				// "removeEntities" "classname" can be set in the worldspawn to remove all entities with the given classname
				foreach(KeyValuePair<string, string> removeEntities in _entities[0].Dict.MatchPrefix("removeEntities"))
				{
					RemoveEntities(removeEntities.Value);
				}

				// "overrideMaterial" "material" can be set in the worldspawn to reset all materials
				string material;
				int entityCount = _entities.Count;
				int primitiveCount = 0;
				int sideCount = 0;

				if((material = (_entities[0].Dict.GetString("overrideMaterial", ""))) != string.Empty)
				{
					for(int i = 0; i < entityCount; i++)
					{
						mapEnt = _entities[i];
						primitiveCount = mapEnt.Primitives.Count;

						for(int j = 0; j < primitiveCount; j++)
						{
							idMapPrimitive mapPrimitive = mapEnt.GetPrimitive(j);

							switch(mapPrimitive.Type)
							{
								case MapPrimitiveType.Brush:
									idMapBrush mapBrush = (idMapBrush) mapPrimitive;
									sideCount = mapBrush.SideCount;

									for(int k = 0; k < sideCount; k++)
									{
										mapBrush.GetSide(k).Material = material;
									}
									break;

								case MapPrimitiveType.Patch:
									idConsole.Warning("TODO: PATCH");
									// TODO: ((idMapPatch) mapPrimitive).Material = material;
									break;
							}
						}
					}
				}

				// force all entities to have a name key/value pair
				if(_entities[0].Dict.GetBool("forceEntityNames") == true)
				{
					for(int i = 1; i < entityCount; i++)
					{
						mapEnt = _entities[i];

						if(mapEnt.Dict.ContainsKey("name") == false)
						{
							mapEnt.Dict.Set("name", string.Format("{0}{1}", mapEnt.Dict.GetString("classname", "forcedName"), i));
						}
					}
				}

				// move the primitives of any func_group entities to the worldspawn
				if(_entities[0].Dict.GetBool("moveFuncGroups") == true)
				{
					for(int i = 1; i < entityCount; i++)
					{
						mapEnt = _entities[i];

						if(mapEnt.Dict.GetString("classname").ToLower() == "func_group")
						{
							_entities[0].Primitives.AddRange(mapEnt.Primitives);
							mapEnt.Primitives.Clear();
						}
					}
				}
			}

			_hasPrimitiveData = true;

			return true;
		}

		public void RemoveEntities(string className)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			int count = _entities.Count;

			for(int i = 0; i < count; i++)
			{
				idMapEntity ent = _entities[i];

				if(ent.Dict.GetString("classname").Equals(className, StringComparison.OrdinalIgnoreCase) == true)
				{
					_entities.RemoveAt(i);

					i--;
				}
			}
		}

		public void RemovePrimitiveData()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			foreach(idMapEntity ent in _entities)
			{
				ent.RemovePrimitiveData();
			}

			_hasPrimitiveData = false;
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
			Dispose(false);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(disposing == true)
			{
				_entities = null;
			}

			_disposed = true;
		}
		#endregion
		#endregion
	}

	public class idMapEntity
	{
		#region Properties
		public idDict Dict
		{
			get
			{
				return _dict;
			}
		}

		public List<idMapPrimitive> Primitives
		{
			get
			{
				return _primitives;
			}
		}

		public int PrimitiveCount
		{
			get
			{
				return _primitives.Count;
			}
		}
		#endregion

		#region Members
		private idDict _dict = new idDict();
		private List<idMapPrimitive> _primitives = new List<idMapPrimitive>();
		#endregion

		#region Methods
		#region Public
		public void AddPrimitive(idMapPrimitive primitive)
		{
			_primitives.Add(primitive);
		}

		public idMapPrimitive GetPrimitive(int index)
		{
			return _primitives[index];
		}


		public void RemovePrimitiveData()
		{
			_primitives.Clear();
		}
		#endregion

		#region Static
		public static idMapEntity Parse(idLexer lexer, bool isWordSpawn = false, float version = idMapFile.CurrentMapVersion)
		{
			idToken token;

			if((token = lexer.ReadToken()) == null)
			{
				return null;
			}

			if(token.ToString() != "{")
			{
				lexer.Error("idMapEntity.Parse: {{ not found, found {0}", token.ToString());
				return null;
			}

			idMapEntity mapEnt = new idMapEntity();
			idMapBrush mapBrush = null;
			idMapPatch mapPatch = null;
			Vector3 origin = Vector3.Zero;
			bool worldEnt = false;
			string tokenValue;

			do
			{
				if((token = lexer.ReadToken()) == null)
				{
					lexer.Error("idMapEntity.Parse: EOF without closing brace");
					return null;
				}

				if(token.ToString() == "}")
				{
					break;
				}

				if(token.ToString() == "{")
				{
					// parse a brush or patch
					if((token = lexer.ReadToken()) == null)
					{
						lexer.Error("idMapEntity.Parse: unexpected EOF");
						return null;
					}

					if(worldEnt == true)
					{
						origin = Vector3.Zero;
					}

					tokenValue = token.ToString();

					// if is it a brush: brush, brushDef, brushDef2, brushDef3
					if(tokenValue.StartsWith("brush", StringComparison.OrdinalIgnoreCase) == true)
					{
						mapBrush = idMapBrush.Parse(lexer, origin, (tokenValue.Equals("brushDef2", StringComparison.OrdinalIgnoreCase) || tokenValue.Equals("brushDef3", StringComparison.OrdinalIgnoreCase)), version);

						if(mapBrush == null)
						{
							return null;
						}

						mapEnt.AddPrimitive(mapBrush);
					}
					// if is it a patch: patchDef2, patchDef3
					else if(tokenValue.StartsWith("patch", StringComparison.OrdinalIgnoreCase) == true)
					{
						mapPatch = idMapPatch.Parse(lexer, origin, tokenValue.Equals("patchDef3", StringComparison.OrdinalIgnoreCase), version);

						if(mapPatch == null)
						{
							return null;
						}

						mapEnt.AddPrimitive(mapPatch);
					}
					// assume it's a brush in Q3 or older style
					else
					{
						lexer.UnreadToken = token;

						mapBrush = idMapBrush.ParseQ3(lexer, origin);

						if(mapBrush == null)
						{
							return null;
						}

						mapEnt.AddPrimitive(mapBrush);
					}
				}
				else
				{
					// parse a key / value pair
					string key = token.ToString();
					token = lexer.ReadTokenOnLine();
					string value = token.ToString();

					// strip trailing spaces that sometimes get accidentally added in the editor
					value = value.Trim();
					key = key.Trim();

					mapEnt.Dict.Set(key, value);

					if(key.Equals("origin", StringComparison.OrdinalIgnoreCase) == true)
					{
						// scanf into doubles, then assign, so it is idVec size independent
						string[] parts = value.Split(' ');

						float.TryParse(parts[0], out origin.X);
						float.TryParse(parts[1], out origin.Y);
						float.TryParse(parts[2], out origin.Z);
					}
					else if((key.Equals("classname", StringComparison.OrdinalIgnoreCase) == true) && (value.Equals("worldspawn", StringComparison.OrdinalIgnoreCase) == true))
					{
						worldEnt = true;
					}
				}
			}
			while(true);

			return mapEnt;
		}
		#endregion
		#endregion
	}

	public class idMapPrimitive
	{
		#region Properties
		public idDict Dict
		{
			get
			{
				return _dict;
			}
			protected set
			{
				_dict = value;
			}
		}

		public MapPrimitiveType Type
		{
			get
			{
				return _type;
			}
			protected set
			{
				_type = value;
			}
		}
		#endregion

		#region Members
		private idDict _dict = new idDict();
		private MapPrimitiveType _type;
		#endregion

		#region Constructor
		public idMapPrimitive()
		{

		}
		#endregion
	}

	public class idMapBrush : idMapPrimitive
	{
		#region Properties
		public int SideCount
		{
			get
			{
				return _sides.Count;
			}
		}
		#endregion

		#region Members
		private List<idMapBrushSide> _sides = new List<idMapBrushSide>();
		#endregion

		#region Constructor
		public idMapBrush()
			: base()
		{
			this.Type = MapPrimitiveType.Brush;
		}
		#endregion

		#region Methods
		#region Public
		public void AddSide(idMapBrushSide side)
		{
			_sides.Add(side);
		}

		public idMapBrushSide GetSide(int index)
		{
			return _sides[index];
		}
		#endregion

		#region Static
		public static idMapBrush Parse(idLexer lexer, Vector3 origin, bool newFormat = true, float version = idMapFile.CurrentMapVersion)
		{
			idToken token;
			idMapBrushSide side;
			List<idMapBrushSide> sides = new List<idMapBrushSide>();
			idDict dict = new idDict();
			Vector3[] planePoints = new Vector3[3];

			if(lexer.ExpectTokenString("{") == false)
			{
				return null;
			}

			do
			{
				if((token = lexer.ReadToken()) == null)
				{
					lexer.Error("idMapBrush::Parse: unexpected EOF");
					return null;
				}

				if(token.ToString() == "}")
				{
					break;
				}

				// here we may have to jump over brush epairs ( only used in editor )
				do
				{
					// if token is a brace
					if(token.ToString() == "(")
					{
						break;
					}

					// the token should be a key string for a key/value pair
					if(token.Type != TokenType.String)
					{
						lexer.Error("idMapBrush::Parse: unexpected {0}, expected ( or epair key string", token.ToString());
						return null;
					}


					string key = token.ToString();

					if(((token = lexer.ReadTokenOnLine()) == null) || (token.Type != TokenType.String))
					{
						lexer.Error("idMapBrush::Parse: expected epair value string not found");
						return null;
					}

					dict.Set(key, token.ToString());

					// try to read the next key
					if((token = lexer.ReadToken()) == null)
					{
						lexer.Error("idMapBrush::Parse: unexpected EOF");
						return null;
					}
				}
				while(true);

				lexer.UnreadToken = token;

				side = new idMapBrushSide();
				sides.Add(side);

				if(newFormat == true)
				{
					float[] tmp = lexer.Parse1DMatrix(4);

					if(tmp == null)
					{
						lexer.Error("idMapBrush::Parse: unable to read brush side plane definition");
						return null;
					}
					else
					{
						side.Plane = new Plane(tmp[0], tmp[1], tmp[2], tmp[3]);
					}
				}
				else
				{
					// read the three point plane definition
					float[] tmp, tmp2, tmp3;

					if(((tmp = lexer.Parse1DMatrix(3)) == null)
						|| ((tmp2 = lexer.Parse1DMatrix(3)) == null)
						|| ((tmp3 = lexer.Parse1DMatrix(3)) == null))
					{
						lexer.Error("idMapBrush::Parse: unable to read brush side plane definition");
						return null;
					}

					planePoints[0] = new Vector3(tmp[0], tmp[1], tmp[2]) - origin;
					planePoints[1] = new Vector3(tmp2[0], tmp2[1], tmp2[2]) - origin;
					planePoints[2] = new Vector3(tmp3[0], tmp3[1], tmp3[2]) - origin;

					side.Plane.FromPoints(planePoints[0], planePoints[1], planePoints[2]);
				}

				// read the texture matrix
				// this is odd, because the texmat is 2D relative to default planar texture axis
				float[,] tmp5 = lexer.Parse2DMatrix(2, 3);

				if(tmp5 == null)
				{
					lexer.Error("idMapBrush::Parse: unable to read brush side texture matrix");
					return null;
				}

				side.TextureMatrix[0] = new Vector3(tmp5[0, 0], tmp5[0, 1], tmp5[0, 2]);
				side.TextureMatrix[1] = new Vector3(tmp5[1, 0], tmp5[1, 1], tmp5[1, 2]);
				side.Origin = origin;

				// read the material
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					lexer.Error("idMapBrush::Parse: unable to read brush side material");
					return null;
				}

				// we had an implicit 'textures/' in the old format...
				if(version < 2.0f)
				{
					side.Material = "textures/" + token.ToString();
				}
				else
				{
					side.Material = token.ToString();
				}

				// Q2 allowed override of default flags and values, but we don't any more
				if(lexer.ReadTokenOnLine() != null)
				{
					if(lexer.ReadTokenOnLine() != null)
					{
						if(lexer.ReadTokenOnLine() != null)
						{

						}
					}
				}
			}
			while(true);

			if(lexer.ExpectTokenString("}") == false)
			{
				return null;
			}

			idMapBrush brush = new idMapBrush();

			foreach(idMapBrushSide s in sides)
			{
				brush.AddSide(s);
			}

			brush.Dict = dict;

			return brush;
		}

		public static idMapBrush ParseQ3(idLexer lexer, Vector3 origin)
		{
			int rotate;
			int[] shift = new int[2];
			float[] scale = new float[2];

			Vector3[] planePoints = new Vector3[3];
			List<idMapBrushSide> sides = new List<idMapBrushSide>();
			idMapBrushSide side;
			idToken token;

			do
			{
				if(lexer.CheckTokenString("}") == true)
				{
					break;
				}

				side = new idMapBrushSide();
				sides.Add(side);

				// read the three point plane definition
				float[] tmp = lexer.Parse1DMatrix(3);
				float[] tmp2 = lexer.Parse1DMatrix(3);
				float[] tmp3 = lexer.Parse1DMatrix(3);

				if((tmp == null) || (tmp2 == null) || (tmp3 == null))
				{
					lexer.Error("idMapBrush::ParseQ3: unable to read brush side plane definition");
					return null;
				}

				planePoints[0] = new Vector3(tmp[0], tmp[1], tmp[2]) - origin;
				planePoints[1] = new Vector3(tmp2[0], tmp2[1], tmp2[2]) - origin;
				planePoints[2] = new Vector3(tmp3[0], tmp3[1], tmp3[2]) - origin;

				side.Plane.FromPoints(planePoints[0], planePoints[1], planePoints[2]);

				// read the material
				token = lexer.ReadTokenOnLine();

				if(token == null)
				{
					lexer.Error("idMapBrush::ParseQ3: unable to read brush side material");
					return null;
				}

				// we have an implicit 'textures/' in the old format
				side.Material = "textures/" + token.ToString();

				// read the texture shift, rotate and scale
				shift[0] = lexer.ParseInt();
				shift[1] = lexer.ParseInt();

				rotate = lexer.ParseInt();

				scale[0] = lexer.ParseFloat();
				scale[1] = lexer.ParseFloat();

				side.TextureMatrix[0] = new Vector3(0.03125f, 0.0f, 0.0f);
				side.TextureMatrix[1] = new Vector3(0.0f, 0.03125f, 0.0f);

				side.Origin = origin;

				// Q2 allowed override of default flags and values, but we don't any more
				if(lexer.ReadTokenOnLine() != null)
				{
					if(lexer.ReadTokenOnLine() != null)
					{
						if(lexer.ReadTokenOnLine() != null)
						{

						}
					}
				}
			}
			while(true);

			idMapBrush brush = new idMapBrush();

			for(int i = 0; i < sides.Count; i++)
			{
				brush.AddSide(sides[i]);
			}

			brush.Dict = new idDict();

			return brush;
		}
		#endregion
		#endregion
	}

	public class idMapBrushSide
	{
		#region Properties
		public string Material
		{
			get
			{
				return _material;
			}
			internal set
			{
				_material = value;
			}
		}

		public Vector3 Origin
		{
			get
			{
				return _origin;
			}
			internal set
			{
				_origin = value;
			}
		}

		public Plane Plane
		{
			get
			{
				return _plane;
			}
			internal set
			{
				_plane = value;
			}
		}

		public Vector3[] TextureMatrix
		{
			get
			{
				return _textureMatrix;
			}
		}
		#endregion

		#region Members
		private string _material;
		private Plane _plane;
		private Vector3 _origin;
		private Vector3[] _textureMatrix = new Vector3[2];
		#endregion

		#region Constructor
		public idMapBrushSide()
		{

		}
		#endregion

		#region Methods
		public Vector4[] GetTextureVectors()
		{
			throw new Exception("TODO: GetTextureVectors");
			/*Vector3 texX, texY, tmp;

			idHelper.ComputeAxisBase(_plane.Normal, out texX, out texY);

			Vector4[] vectors = new Vector4[2];

			for(int i = 0; i < 2; i++)
			{
				vectors[i].X = texX.X * _textureMatrix[i].X + texY.X * _textureMatrix[i].Y;
				vectors[i].Y = texX.Y * _textureMatrix[i].X + texY.Y * _textureMatrix[i].Y;
				vectors[i].Z = texX.Z * _textureMatrix[i].X + texY.Z * _textureMatrix[i].Y;

				tmp = new Vector3(vectors[i].X, vectors[i].Y, vectors[i].Z);

				vectors[i].W = _textureMatrix[i].Z + (_origin * tmp);
			}*/
		}
		#endregion
	}

	public class idMapPatch : idMapPrimitive
	{
		#region Properties
		public bool ExplicitlySubdivided
		{
			get
			{
				return _explicitSubdivisions;
			}
			set
			{
				_explicitSubdivisions = value;
			}
		}

		public int HorizontalSubdivisions
		{
			get
			{
				return _horizontalSubdivisions;
			}
			set
			{
				_horizontalSubdivisions = value;
			}
		}

		public int VerticalSubdivisions
		{
			get
			{
				return _verticalSubdivisions;
			}
			set
			{
				_verticalSubdivisions = value;
			}
		}

		public string Material
		{
			get
			{
				return _material;
			}
			set
			{
				_material = value;
			}
		}

		public int Width
		{
			get
			{
				return _surface.Width;
			}
		}

		public int Height
		{
			get
			{
				return _surface.Height;
			}
		}
		#endregion

		#region Members
		private idPatchSurface _surface;

		private string _material;
		private int _horizontalSubdivisions;
		private int _verticalSubdivisions;
		private bool _explicitSubdivisions;
		#endregion

		#region Constructor
		public idMapPatch()
		{
			this.Type = MapPrimitiveType.Patch;
			_surface = new idPatchSurface();
		}

		public idMapPatch(int maxWidth, int maxHeight)
		{
			this.Type = MapPrimitiveType.Patch;
			_surface = new idPatchSurface(maxWidth, maxHeight);	
		}
		#endregion

		#region Methods
		public void SetVertex(int index, Vertex vertex)
		{
			_surface.SetVertex(index, vertex);
		}

		public static idMapPatch Parse(idLexer lexer, Vector3 origin, bool patchDef3 = true, float version = idMapFile.CurrentMapVersion)
		{
			if(lexer.ExpectTokenString("{") == false)
			{
				return null;
			}

			// read the material (we had an implicit 'textures/' in the old format...)
			idToken token = lexer.ReadToken();

			if(token == null)
			{
				lexer.Error("idMapPatch::Parse: unexpected EOF");
				return null;
			}

			// Parse it
			float[] info;

			if(patchDef3 == true)
			{
				info = lexer.Parse1DMatrix(7);

				if(info == null)
				{
					lexer.Error("idMapPatch::Parse: unable to Parse patchDef3 info");
					return null;
				}
			} 
			else 
			{
				info = lexer.Parse1DMatrix(5);

				if(info == null)
				{
					lexer.Error("idMapPatch::Parse: unable to parse patchDef2 info");
					return null;
				}
			}

			idMapPatch patch = new idMapPatch((int) info[0], (int) info[1]);

			if(version < 2.0f)
			{
				patch.Material = "textures/" + token.ToString();
			}
			else
			{
				patch.Material = token.ToString();
			}

			if(patchDef3 == true)
			{
				patch.HorizontalSubdivisions = (int) info[2];
				patch.VerticalSubdivisions = (int) info[3];
				patch.ExplicitlySubdivided = true;
			}

			if((patch.Width < 0) || (patch.Height < 0))
			{
				lexer.Error("idMapPatch::Parse: bad size");
				return null;
			}

			// these were written out in the wrong order, IMHO
			if(lexer.ExpectTokenString("(") == false)
			{
				lexer.Error("idMapPatch::Parse: bad patch vertex data");
				return null;
			}

			for(int j = 0; j < patch.Width; j++)
			{
				if(lexer.ExpectTokenString("(") == false)
				{
					lexer.Error("idMapPatch::Parse: bad vertex row data");
					return null;
				}

				for(int i = 0; i < patch.Height; i++)
				{
					float[] v = lexer.Parse1DMatrix(5);

					if(v == null)
					{
						lexer.Error("idMapPatch::Parse: bad vertex column data");
						return null;
					}

					Vertex vert = new Vertex();
					vert.Position.X = v[0] - origin.X;
					vert.Position.Y = v[1] - origin.Y;
					vert.Position.Z = v[2] - origin.Z;
					vert.TextureCoordinates = new Vector2(v[3], v[4]);

					patch.SetVertex(i * patch.Width + j, vert);
				}

				if(lexer.ExpectTokenString(")") == false)
				{
					lexer.Error("idMapPatch::Parse: unable to parse patch control points");
					return null;
				}
			}

			if(lexer.ExpectTokenString(")") == false)
			{
				lexer.Error("idMapPatch::Parse: unable to parse patch control points, no closure" );
				return null;
			}

			// read any key/value pairs
			while((token = lexer.ReadToken()) != null)
			{
				if(token.ToString() == "}")
				{
					lexer.ExpectTokenString("}");
					break;
				}

				if(token.Type == TokenType.String)
				{
					string key = token.ToString();
					token = lexer.ExpectTokenType(TokenType.String, 0);

					patch.Dict.Set(key, token.ToString());
				}
			}
			
			return patch;
		}
		#endregion
	}

	public enum MapPrimitiveType
	{
		Invalid = -1,
		Brush,
		Patch
	}
}