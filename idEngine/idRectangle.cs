using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4
{
	public struct idRectangle
	{
		public float Bottom
		{
			get
			{
				return (this.Y + this.Height);
			}
		}

		public float Right
		{
			get
			{
				return (this.X + this.Width);
			}
		}

		public float X;
		public float Y;
		public float Width;
		public float Height;

		public static idRectangle Empty = new idRectangle();

		public idRectangle(float x, float y, float width, float height)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}

		public void Offset(float x, float y)
		{
			this.X += x;
			this.Y += y;
		}

		public override bool Equals(object obj)
		{
			if(obj is idRectangle)
			{
				idRectangle r = (idRectangle) obj;

				return ((this.X == r.X) && (this.Y == r.Y) && (this.Width == r.Width) && (this.Height == r.Height));
			}

			return false;
		}

		public static bool operator ==(idRectangle r1, idRectangle r2)
		{
			return ((r1.X == r2.X) && (r1.Y == r2.Y) && (r1.Width == r2.Width) && (r1.Height == r2.Height));
		}

		public static bool operator !=(idRectangle r1, idRectangle r2)
		{
			return ((r1.X != r2.X) || (r1.Y != r2.Y) || (r1.Width != r2.Width) || (r1.Height != r2.Height));
		}
	}
}
