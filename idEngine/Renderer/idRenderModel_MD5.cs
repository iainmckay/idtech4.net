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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Geometry;
using idTech4.Text;

namespace idTech4.Renderer
{
	public class idRenderModel_MD5 : idRenderModel_Static
	{
		#region Constants
		public const int Version = 10;
		public const string VersionString = "MD5Version";
		public const string MeshExtension = ".md5mesh";
		public const string MeshAnimationExtension = ".md5anim";
		public const string MeshCameraExtension = ".md5camera";
		#endregion

		#region Members
		private idMD5Joint[] _joints;
		private idMD5Mesh[] _meshes;
		private idJointQuaternion[] _defaultPose;
		#endregion

		#region Constructor
		public idRenderModel_MD5()
			: base()
		{

		}
		#endregion

		#region Methods
		#region Private
		private void CalculateBounds(idJointMatrix[] entityJoints)
		{
			_bounds.Clear();

			foreach(idMD5Mesh mesh in _meshes)
			{
				_bounds.AddBounds(mesh.CalculateBounds(entityJoints));
			}
		}

		private void ParseJoint(idLexer lexer, idMD5Joint joint, ref idJointQuaternion defaultPose)
		{
			//
			// parse name
			//
			joint.Name = lexer.ReadToken().ToString();

			//
			// parse parent
			//
			int parentIndex = lexer.ParseInt();

			if(parentIndex >= 0)
			{
				if(parentIndex >= (_joints.Length - 1))
				{
					lexer.Error("Invalid parent for joint '{0}'", joint.Name);
				}

				joint.Parent = _joints[parentIndex];
			}
		
			//
			// parse default pose
			//
			float[] tmp = lexer.Parse1DMatrix(3);
			defaultPose.Translation = new Vector3(tmp[0], tmp[1], tmp[2]);

			tmp = lexer.Parse1DMatrix(3);
			defaultPose.Quaternion = new Quaternion(tmp[0], tmp[1], tmp[2], 0);
			defaultPose.Quaternion.W = idHelper.CalculateW(defaultPose.Quaternion);
		}
		#endregion
		#endregion

		#region idRenderModel_Static implementation
		#region Properties
		public override idJointQuaternion[] DefaultPose
		{
			get
			{
				return _defaultPose;
			}
		}

		public override int JointCount
		{
			get 
			{ 
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				if(_joints == null)
				{
					return 0;
				}

				return _joints.Length;
			}
		}

		public override idMD5Joint[] Joints
		{
			get 
			{ 
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _joints;
			}
		}

		public override int MemoryUsage
		{
			get 
			{				 
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idRenderModel_MD5.MemoryUsage");
				return 0;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Calculates a rough bounds by using the joint radii without
		/// transforming all the points.
		/// </summary>
		/// <param name="renderEntity"></param>
		/// <returns></returns>
		public override idBounds GetBounds(RenderEntityComponent renderEntity = null)
		{ 
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(renderEntity == null)
			{
				// this is the bounds for the reference pose
				return _bounds;
			}

			return renderEntity.Bounds;
		}

		public override int GetJointIndex(string name)
		{
			for(int i = 0; i < _joints.Length; i++)
			{
				if(_joints[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
				 {
					 return i;
				 }
			 }

			return -1;
		}

		public override int GetJointIndex(idMD5Joint joint)
		{
			for(int i = 0; i < _joints.Length; i++)
			{
				if(_joints[i] == joint)
				{
					return i;
				}
			}

			return -1;
		}

		public override string GetJointName(int index)
		{
			if((index < 0) || (index >= _joints.Length))
			{
				return "<invalid joint>";
			}

			return _joints[index].Name;
		}

		public override int GetNearestJoint(int surfaceIndex, int a, int c, int b)
		{
			if(surfaceIndex > _meshes.Length)
			{
				idConsole.Error("idRenderModel_MD5::NearestJoint: surfaceIndex > meshes.Length");
			}

			foreach(idMD5Mesh mesh in _meshes)
			{
				if(mesh.SurfaceIndex == surfaceIndex)
				{
					return mesh.GetNearestJoint(a, b, c);
				}
			}

			return 0;
		}

		public override void InitFromFile(string fileName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

 			_name = fileName;

			Load();
		}

		public override idRenderModel InstantiateDynamicModel(idRenderEntity renderEntity, View view, idRenderModel cachedModel)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idRenderModel_MD5.InstantiateDynamicModel");
			return null;
		}

		/// <summary>
		/// Used for initial loads, reloadModel, and reloading the data of purged models.
		/// </summary>
		/// <remarks>
		/// Upon exit, the model will absolutely be valid, but possibly as a default model.
		/// </remarks>
		public override void Load()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(_purged == false)
			{
				Purge();
			}

			_purged = false;

			idLexer lexer = new idLexer(LexerOptions.AllowPathNames | LexerOptions.NoStringEscapeCharacters);

			if(lexer.LoadFile(Name) == false)
			{
				MakeDefault();
				return;
			}

			lexer.ExpectTokenString(VersionString);

			int version = lexer.ParseInt();
			int count = 0;
			idToken token;

			if(version != Version)
			{
				lexer.Error("Invalid version {0}. Should be version {1}", version, Version);
			}

			//
			// skip commandline
			//
			lexer.ExpectTokenString("commandline");
			lexer.ReadToken();

			// parse num joints
			lexer.ExpectTokenString("numJoints");

			count = lexer.ParseInt();

			_joints = new idMD5Joint[count];
			_defaultPose = new idJointQuaternion[count];
			
			idJointMatrix[] poseMat3 = new idJointMatrix[count];

			// parse num meshes
			lexer.ExpectTokenString("numMeshes");
			count = lexer.ParseInt();

			if(count < 0)
			{
				lexer.Error("Invalid size: {0}", count);
			}

			_meshes = new idMD5Mesh[count];

			//
			// parse joints
			//
			lexer.ExpectTokenString("joints");
			lexer.ExpectTokenString("{");

			for(int i = 0; i < _joints.Length; i++)
			{
				idMD5Joint joint = _joints[i] = new idMD5Joint();
				idJointQuaternion pose = new idJointQuaternion();

				ParseJoint(lexer, joint, ref pose);

				poseMat3[i] = idJointMatrix.Zero;
				poseMat3[i].Rotation = Matrix.CreateFromQuaternion(pose.Quaternion);
				poseMat3[i].Translation = pose.Translation;

				if(joint.Parent != null)
				{
					int parentIndex = GetJointIndex(joint.Parent);

					pose.Quaternion = Quaternion.CreateFromRotationMatrix(poseMat3[i].ToMatrix() 
										* Matrix.Transpose(poseMat3[parentIndex].ToMatrix()));
					pose.Translation = Vector3.Transform(poseMat3[i].ToVector3() - poseMat3[parentIndex].ToVector3(), 
										Matrix.Transpose(poseMat3[parentIndex].ToMatrix()));
				}

				_defaultPose[i] = pose;
			}

			lexer.ExpectTokenString("}");

			for(int i = 0; i < _meshes.Length; i++)
			{
				lexer.ExpectTokenString("mesh");

				_meshes[i] = new idMD5Mesh();
				_meshes[i].Parse(lexer, poseMat3);
			}

			//
			// calculate the bounds of the model
			//
			CalculateBounds(poseMat3);

			// set the timestamp for reloadmodels
			idConsole.Warning("TODO: fileSystem->ReadFile( name, NULL, &timeStamp );");
		}

		public override void Print()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

 			 idConsole.Warning("TODO: idRenderModel_MD5.Print");
		}

		public override void Purge()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_purged = true;
			_joints = null;
			_defaultPose = null;
			_meshes = null;
		}

		/// <summary>
		/// Models that are already loaded at level start time will still touch their materials to make sure they
		/// are kept loaded.
		/// </summary>
		public override void TouchData()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			foreach(idMD5Mesh mesh in _meshes)
			{
				idE.DeclManager.FindMaterial(mesh.Material.Name);
			}
		}
		#endregion
		#endregion
	}

	public class idMD5Mesh : IDisposable
	{
		#region Properties
		public idMaterial Material
		{
			get
			{
				return _material;
			}
		}

		public int SurfaceIndex
		{
			get
			{
				return _surfaceIndex;
			}
		}
		#endregion

		#region Members
		private int _weightCount;
		private int _triangleCount;

		//private DeformInformation _deformInfo; // used to create srfTriangles_t from base frames and new vertexes
		private idMaterial _material;
		private Vector2[] _texCoords;
		private Vector4[] _scaledWeights;
		private int[] _weightIndex; // pairs of: joint offset + bool true if next weight is for next vertex
		private int _surfaceIndex; // number of the static surface created for this mesh
		#endregion

		#region Constructor
		public idMD5Mesh()
		{

		}

		~idMD5Mesh()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		public idBounds CalculateBounds(idJointMatrix[] joints)
		{
			Vertex[] verts = new Vertex[_texCoords.Length];
			idBounds bounds = idBounds.Zero;

			TransformVertices(verts, joints);

			idHelper.MinMax(ref bounds.Min, ref bounds.Max, verts, _texCoords.Length);

			return bounds;
		}

		public int GetNearestJoint(int a, int b, int c)
		{
			int i, bestJoint, vertNum, weightVertCount;
			float bestWeight;

			// duplicated vertices might not have weights
			if((a >= 0) && (a < _texCoords.Length))
			{
				vertNum = a;
			}
			else if((b >= 0) && (b < _texCoords.Length))
			{
				vertNum = b;
			}
			else if((c >= 0) && (c < _texCoords.Length))
			{
				vertNum = c;
			}
			else
			{
				// all vertices are duplicates which shouldn't happen
				return 0;
			}

			// find the first weight for this vertex
			weightVertCount = 0;

			for(i = 0; weightVertCount < vertNum; i++)
			{
				weightVertCount += _weightIndex[i * 2 + 1];
			}

			// get the joint for the largest weight
			bestWeight = _scaledWeights[i].W;
			bestJoint = _weightIndex[i * 2 + 0];

			for(; _weightIndex[i * 2 + 1] == 0; i++)
			{
				if(_scaledWeights[i].W > bestWeight)
				{
					bestWeight = _scaledWeights[i].W;
					bestJoint = _weightIndex[i * 2 + 0];
				}
			}

			return bestJoint;
		}

		public void Parse(idLexer lexer, idJointMatrix[] joints)
		{
			lexer.ExpectTokenString("{");

			//
			// parse name
			//
			if(lexer.CheckTokenString("name") == true)
			{
				lexer.ReadToken();
			}

			//
			// parse shader
			//
			lexer.ExpectTokenString("shader");

			idToken token = lexer.ReadToken();
			string materialName = token.ToString();

			_material = idE.DeclManager.FindMaterial(materialName);

			//
			// parse texture coordinates
			//
			lexer.ExpectTokenString("numverts");
			int count = lexer.ParseInt();

			if(count < 0)
			{
				lexer.Error("Invalid size: {0}", token.ToString());
			}

			_texCoords = new Vector2[count];

			int[] firstWeightForVertex = new int[count];
			int[] weightCountForVertex = new int[count];
			int maxWeight = 0;

			_weightCount = 0;

			for(int i = 0; i < _texCoords.Length; i++)
			{
				lexer.ExpectTokenString("vert");
				lexer.ParseInt();

				float[] tmp = lexer.Parse1DMatrix(2);

				_texCoords[i] = new Vector2(tmp[0], tmp[1]);
				
				firstWeightForVertex[i] = lexer.ParseInt();
				weightCountForVertex[i] = lexer.ParseInt();

				if(weightCountForVertex[i] == 0)
				{
					lexer.Error("Vertex without any joint weights.");
				}

				_weightCount += weightCountForVertex[i];

				if((weightCountForVertex[i] + firstWeightForVertex[i]) > maxWeight)
				{
					maxWeight = weightCountForVertex[i] + firstWeightForVertex[i];
				}
			}

			//
			// parse tris
			//
			lexer.ExpectTokenString("numtris");
			_triangleCount = lexer.ParseInt();

			if(_triangleCount < 0)
			{
				lexer.Error("Invalid size: {0}", _triangleCount);
			}

			int[] tris = new int[_triangleCount * 3];

			for(int i = 0; i < _triangleCount; i++)
			{
				lexer.ExpectTokenString("tri");
				lexer.ParseInt();

				tris[i * 3 + 0] = lexer.ParseInt();
				tris[i * 3 + 1] = lexer.ParseInt();
				tris[i * 3 + 2] = lexer.ParseInt();
			}

			//
			// parse weights
			//
			lexer.ExpectTokenString("numweights");
			count = lexer.ParseInt();

			if(count < 0)
			{
				lexer.Error("Invalid size: {0}", count);
			}

			if(maxWeight > count)
			{
				lexer.Warning("Vertices reference out of range weights in model ({0} of {1} weights).", maxWeight, count);
			}

			VertexWeight[] tempWeights = new VertexWeight[count];

			for(int i = 0; i < count; i++)
			{
				lexer.ExpectTokenString("weight");
				lexer.ParseInt();

				int jointIndex = lexer.ParseInt();

				if((jointIndex < 0) || (jointIndex >= joints.Length))
				{
					lexer.Error("Joint index out of range({0}): {1}", joints.Length, jointIndex);
				}

				tempWeights[i].JointIndex = jointIndex;
				tempWeights[i].JointWeight = lexer.ParseFloat();

				float[] tmp = lexer.Parse1DMatrix(3);

				tempWeights[i].Offset = new Vector3(tmp[0], tmp[1], tmp[2]);
			}

			// create pre-scaled weights and an index for the vertex/joint lookup
			_scaledWeights = new Vector4[_weightCount];
			_weightIndex = new int[_weightCount * 2];

			count = 0;

			for(int i = 0; i < _texCoords.Length; i++)
			{
				int num = firstWeightForVertex[i];

				for(int j = 0; j < weightCountForVertex[i]; j++, num++, count++)
				{
					Vector3 tmp = tempWeights[num].Offset * tempWeights[num].JointWeight;

					_scaledWeights[count].X = tmp.X;
					_scaledWeights[count].Y = tmp.Y;
					_scaledWeights[count].Z = tmp.Z;
					_scaledWeights[count].W = tempWeights[num].JointWeight;

					_weightIndex[count * 2 + 0] = tempWeights[num].JointIndex;
				}

				_weightIndex[count * 2 - 1] = 1;
			}

			lexer.ExpectTokenString("}");

			// update counters
			idConsole.Warning("TODO: idRenderModel_MD5 update counters");

			/*c_numVerts += texCoords.Num();
			c_numWeights += numWeights;
			c_numWeightJoints++;
			for ( i = 0; i < numWeights; i++ ) {
				c_numWeightJoints += weightIndex[i*2+1];
			}*/

			//
			// build the information that will be common to all animations of this mesh:
			// silhouette edge connectivity and normal / tangent generation information
			//
			Vertex[] verts = new Vertex[_texCoords.Length];

			for(int i = 0; i < verts.Length; i++)
			{
				verts[i].TextureCoordinates = _texCoords[i];
			}

			TransformVertices(verts, joints);

			idConsole.Warning("TODO: idMD5Mesh Deform");
			//_deformInfo = idE.RenderSystem.BuildDeformInformation(verts, tris, _material.UseUnsmoothedTangents);
		}
		#endregion

		#region Private
		private void TransformVertices(Vertex[] verts, idJointMatrix[] entityJoints)
		{
			int j, i;

			for(j = i = 0; i < verts.Length; i++)
			{
				Vector3 w = new Vector3(_scaledWeights[j].X, _scaledWeights[j].Y, _scaledWeights[j].Z);
				Vector3 v = entityJoints[_weightIndex[j * 2 + 0]].ToVector3() * w;

				while(_weightIndex[j * 2 + 1] == 0)
				{
					j++;
					v += entityJoints[_weightIndex[j * 2 + 0]].ToVector3() * w;
				}

				j++;
				verts[i].Position = v;
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

			if(disposing == true)
			{
				
			}

			idConsole.Warning("TODO: idMD5Mesh.Dispose");
			/*Mem_Free16(scaledWeights);
				Mem_Free16(weightIndex);
				if(deformInfo)
				{
					R_FreeDeformInfo(deformInfo);
					deformInfo = NULL;
				}*/

			_disposed = true;
		}
		#endregion
		#endregion
	}

	public class idMD5Joint
	{
		public string Name;
		public idMD5Joint Parent;
	}

	public struct VertexWeight
	{
		public int Vertice;
		public int JointIndex;
		public float JointWeight;
		public Vector3 Offset;
	}
}