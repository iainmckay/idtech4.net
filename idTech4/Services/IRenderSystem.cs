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
		#region Initialization
		#region Properties
		bool IsInitialized { get; }
		#endregion

		#region Methods
		void Initialize();
		LinkedListNode<idRenderCommand> SwapCommandBuffers(out ulong frontEnd, out ulong backEnd, out ulong shadow, out ulong gpu);
		LinkedListNode<idRenderCommand> SwapCommandBuffers_FinishCommandBuffers();
		void SwapCommandBuffers_FinishRendering(out ulong frontEnd, out ulong backEnd, out ulong shadow, out ulong gpu);
		#endregion
		#endregion
		
		#region Other
		#region Methods
		/// <summary>
		/// Returns the current cropped pixel coordinates.
		/// </summary>
		/// <returns></returns>
		idScreenRect GetCroppedViewport();
		#endregion
		#endregion

		#region Rendering
		#region Properties
		Color Color { get; set; }
		int FrameCount { get; }
		int Width { get; }
		int Height { get; }
		float PixelAspect { get; }
		
		idMaterial DefaultMaterial { get; }
		idViewDefinition ViewDefinition { get; set; }
		idRenderCapabilities Capabilities { get; }
		#endregion

		#region Methods
		DynamicIndexBuffer CreateDynamicIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage);
		DynamicVertexBuffer CreateDynamicVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage);
		IndexBuffer CreateIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage);
		VertexBuffer CreateVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage);

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

		#region Texturing
		Texture2D CreateTexture(int width, int height, bool mipmap = false, SurfaceFormat format = SurfaceFormat.Color);
		#endregion
	}
}