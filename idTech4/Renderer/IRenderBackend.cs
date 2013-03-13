using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace idTech4.Renderer
{
	public interface IRenderBackend
	{
		idRenderCapabilities Capabilities { get; }
		float PixelAspect { get; }
		ulong State { get; set; }

		Texture2D CreateTexture(int width, int height, bool mipmap = false, SurfaceFormat format = SurfaceFormat.Color);

		DynamicIndexBuffer CreateDynamicIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage);
		DynamicVertexBuffer CreateDynamicVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage);
		IndexBuffer CreateIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage);
		VertexBuffer CreateVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage);

		void Init();
		
		/// <summary>
		/// We want to exit this with the GPU idle, right at vsync
		/// </summary>
		void BlockingSwapBuffers();

		void ExecuteBackendCommands(LinkedListNode<idRenderCommand> commands);
	}
}
