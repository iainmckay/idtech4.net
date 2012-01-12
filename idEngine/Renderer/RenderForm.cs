using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK.Graphics;

namespace idTech4.Renderer
{
	public partial class RenderForm : Form
	{
		private OpenTK.GLControl _glControl;

		public RenderForm(int width, int height, bool fullscreen, int refreshRate, int multiSamples, bool stereoMode)
		{
			InitializeComponent();

			this.Name = idE.GameName;
			this.Width = width;
			this.Height = height;

			if(stereoMode == true)
			{
				idConsole.WriteLine("...attempting to use stereo");
			}

			idE.GLConfig.ColorBits = 32;
			idE.GLConfig.DepthBits = 24;
			idE.GLConfig.StencilBits = 8;

			idConsole.Write("...creating GL context: ");

			GraphicsMode mode = new GraphicsMode(idE.GLConfig.ColorBits, idE.GLConfig.DepthBits, idE.GLConfig.StencilBits, multiSamples);

			_glControl = new OpenTK.GLControl(mode);
			_glControl.Dock = DockStyle.Fill;
			
			this.Controls.Add(_glControl);

			idConsole.WriteLine("succeeded");

			// TODO
			/*idConsole.Write("...making context current: ");

			_glControl.MakeCurrent();

			idConsole.WriteLine("succeeded");*/
		}

		public void SwapBuffers()
		{
			_glControl.SwapBuffers();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			
			idE.System.Quit();
		}
	}
}
