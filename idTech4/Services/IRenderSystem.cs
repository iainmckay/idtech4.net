/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using idTech4.Renderer;

namespace idTech4.Services
{
	/// <summary>
	/// Responsible for managing the screen, which can have multiple idRenderWorld and 2D drawing done on it.
	/// </summary>
	public interface IRenderSystem
	{
		#region Fonts
		#region Methods
		idFont LoadFont(string fontName);
		#endregion
		#endregion

		#region Initialization
		#region Properties
		bool IsInitialized { get; }
		#endregion

		#region Methods
		void Initialize();
		LinkedListNode<idRenderCommand> SwapCommandBuffers(out long frontEnd, out long backEnd, out long shadow, out long gpu);
		LinkedListNode<idRenderCommand> SwapCommandBuffers_FinishCommandBuffers();
		void SwapCommandBuffers_FinishRendering(out long frontEnd, out long backEnd, out long shadow, out long gpu);
		#endregion
		#endregion
		
		#region Other
		#region Methods
		void BeginLevelLoad();
		void EndLevelLoad();

		/// <summary>
		/// Returns the current cropped pixel coordinates.
		/// </summary>
		/// <returns></returns>
		idScreenRect GetCroppedViewport();
		#endregion
		#endregion

		#region Rendering
		#region Properties
		Vector4 Color { get; set; }
		int FrameCount { get; }
		int Width { get; }
		int Height { get; }
		float PixelAspect { get; }
		
		idMaterial DefaultMaterial { get; }
		idViewDefinition ViewDefinition { get; set; }
		idRenderCapabilities Capabilities { get; }
		#endregion

		#region Methods
		void AddPrimitive(idVertex[] vertices, ushort[] indexes, idMaterial material, StereoDepthType stereoType);
		DynamicIndexBuffer CreateDynamicIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage);
		DynamicVertexBuffer CreateDynamicVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage);
		IndexBuffer CreateIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage);
		VertexBuffer CreateVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage);

		void DrawBigCharacter(int x, int y, char c);
		void DrawBigString(int x, int y, string str, Vector4 color, bool forceColor);
		void DrawFilled(Vector4 color, float x, float y, float w, float h);
		void DrawSmallCharacter(int x, int y, char c);
		void DrawSmallString(int x, int y, string str, Vector4 color, bool forceColor);
		void DrawStretchPicture(float x, float y, float w, float h, float s1, float t1, float s2, float t2, idMaterial material);
		void DrawStretchPicture(Vector4 topLeft, Vector4 topRight, Vector4 bottomRight, Vector4 bottomLeft, idMaterial material);
		void DrawView(idViewDefinition view, bool guiOnly);

		/// <summary>
		/// Issues GPU commands to render a built up list of command buffers returned
		/// by SwapCommandBuffers().
		/// </summary>
		/// <remarks>
		/// No references should be made to the current frameData, so new scenes and GUIs can be built up in parallel with the rendering.
		/// </remarks>
		/// <param name="commandBuffers"></param>
		void RenderCommandBuffers(LinkedListNode<idRenderCommand> commandBuffers);
		#endregion
		#endregion

		#region State
		void SetRenderState(ulong state);
		#endregion

		#region Texturing
		Texture2D CreateTexture(int width, int height, bool mipmap = false, SurfaceFormat format = SurfaceFormat.Color);
		#endregion
	}
}