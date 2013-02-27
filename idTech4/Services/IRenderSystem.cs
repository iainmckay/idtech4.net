using System.Collections.Generic;

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
		void SwapCommandBuffers_FinishRendering(out ulong frontEnd, out ulong backEnd, out ulong shadow, out ulong gpu);
		#endregion
		#endregion
	}
}