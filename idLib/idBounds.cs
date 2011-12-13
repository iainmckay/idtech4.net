﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4
{
	public struct idBounds
	{
		#region Properties
		public bool IsCleared
		{
			get
			{
				return (this.Min.X > this.Max.X);
			}
		}

		public static idBounds Zero
		{
			get
			{
				return new idBounds();
			}
		}
		#endregion

		#region Fields
		public Vector3 Min;
		public Vector3 Max;
		#endregion

		#region Constructor
		public idBounds(Vector3 min, Vector3 max)
		{
			this.Min = min;
			this.Max = max;
		}
		#endregion

		#region Methods
		public float GetRadius()
		{
			float total = 0.0f;

			for(int i = 0; i < 3; i++)
			{
				float b0 = (float) idMath.Abs(((i == 0) ? this.Min.X : ((i == 1) ? this.Min.Y : this.Min.Z)));
				float b1 = (float) idMath.Abs(((i == 0) ? this.Max.X : ((i == 1) ? this.Max.Y : this.Max.Z)));

				if(b0 > b1)
				{
					total += b0 * b0;
				}
				else
				{
					total += b1 * b1;
				}
			}

			return idMath.Sqrt(total);
		}

		public float GetRadius(Vector3 center)
		{
			float total = 0.0f;

			for(int i = 0; i < 3; i++)
			{
				float c = ((i == 0) ? center.X : ((i == 1) ? center.Y : center.Z));
				float b0 = (float) idMath.Abs(c - ((i == 0) ? this.Min.X : ((i == 1) ? this.Min.Y : this.Min.Z)));
				float b1 = (float) idMath.Abs(((i == 0) ? this.Max.X : ((i == 1) ? this.Max.Y : this.Max.Z)) - c);

				if(b0 > b1)
				{
					total += b0 * b0;
				}
				else
				{
					total += b1 * b1;
				}
			}

			return idMath.Sqrt(total);
		}

		/// <summary>
		/// Most tight bounds for the rotational movement of the given point.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static idBounds FromPointRotation(Vector3 point, idRotation rotation)
		{
			if(idMath.Abs(rotation.Angle) < 180.0f)
			{
				return BoundsForPointRotation(point, rotation);
			}
			else
			{
				float radius = (point - rotation.Origin).Length();

				// FIXME: these bounds are usually way larger
				idBounds result = new idBounds();
				result.Min = new Vector3(-radius, -radius, -radius);
				result.Max = new Vector3(radius, radius, radius);

				return result;
			}
		}

		/// <summary>
		/// Most tight bounds for the translational movement of the given bounds.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="axis"></param>
		/// <param name="translation"></param>
		/// <returns></returns>
		public static idBounds FromBoundsTranslation(idBounds bounds, Vector3 origin, Matrix axis, Vector3 translation)
		{
			idBounds result;

			if(axis != Matrix.Identity)
			{
				result = FromTransformedBounds(bounds, origin, axis);
			}
			else
			{
				result = new idBounds(bounds.Min + origin, bounds.Max + origin);
			}

			if(translation.X < 0.0f)
			{
				bounds.Min.X += translation.X;
			}
			else
			{
				bounds.Max.X += translation.X;
			}

			if(translation.Y < 0.0f)
			{
				bounds.Min.Y += translation.Y;
			}
			else
			{
				bounds.Max.Y += translation.Y;
			}

			if(translation.Z < 0.0f)
			{
				bounds.Min.Z += translation.Z;
			}
			else
			{
				bounds.Max.Z += translation.Z;
			}

			return bounds;
		}
		
		/// <summary>
		/// Most tight bounds for the rotational movement of the given bounds.
		/// </summary>
		/// <param name="idBounds"></param>
		/// <returns></returns>
		public static idBounds FromBoundsRotation(idBounds bounds, Vector3 origin, Matrix axis, idRotation rotation)
		{
			idBounds result = idBounds.Zero;
			Vector3 point = Vector3.Zero;
			float radius;

			if(idMath.Abs(rotation.Angle) < 180.0f)
			{
				// TODO: result = BoundsForPointRotation(bounds.Min * axis + origin, rotation);
				point = Vector3.Zero;

				for(int i = 1; i < 8; i++)
				{
					point.X = (((i ^ (i >> 1)) & 1) == 0) ? bounds.Min.X : bounds.Max.X;
					point.Y = (((i >> 1) & 1) == 0) ? bounds.Min.Y : bounds.Max.Y;
					point.Z = (((i >> 2) & 1) == 0) ? bounds.Min.Z : bounds.Max.Z;

					result = idBounds.Zero;
					//TODO : result += BoundsForPointRotation(point * axis + origin, rotation);
				}
			}
			else
			{
				point = (bounds.Max - bounds.Min) * 0.5f;
				radius = (bounds.Max - point).Length() + (point - rotation.Origin).Length();

				result = new idBounds();
				result.Min = new Vector3(-radius, -radius, -radius);
				result.Max = new Vector3(radius, radius, radius);
			}

			return result;
		}

		/// <summary>
		/// Most tight bounds for the translational movement of the given point.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="translation"></param>
		/// <returns></returns>
		public static idBounds FromPointTranslation(Vector3 point, Vector3 translation)
		{
			idBounds result = new idBounds();

			if(translation.X < 0.0f)
			{
				result.Min.X = point.X + translation.X;
				result.Max.X = point.X;
			}
			else
			{
				result.Min.X = point.X;
				result.Max.X = point.X + translation.X;
			}

			if(translation.Y < 0.0f)
			{
				result.Min.Y = point.Y + translation.Y;
				result.Max.Y = point.Y;
			}
			else
			{
				result.Min.Y = point.Y;
				result.Max.Y = point.Y + translation.Y;
			}

			if(translation.Z < 0.0f)
			{
				result.Min.Z = point.Z + translation.Z;
				result.Max.Z = point.Z;
			}
			else
			{
				result.Min.Z = point.Z;
				result.Max.Z = point.Z + translation.Z;
			}

			return result;
		}

		public static idBounds FromTransformedBounds(idBounds bounds, Vector3 origin, Matrix axis)
		{
			Vector3 center = (bounds.Min + bounds.Max) * 0.5f;
			Vector3 extents = bounds.Max - center;
			Vector3 rotatedExtents = Vector3.Zero;

			rotatedExtents.X = idMath.Abs(extents.X * axis.M11) + idMath.Abs(extents.Y * axis.M21) + idMath.Abs(extents.Y * axis.M31);
			rotatedExtents.Y = idMath.Abs(extents.X * axis.M12) + idMath.Abs(extents.Y * axis.M22) + idMath.Abs(extents.Y * axis.M32);
			rotatedExtents.Z = idMath.Abs(extents.X * axis.M13) + idMath.Abs(extents.Y * axis.M23) + idMath.Abs(extents.Y * axis.M33);

			center = origin; // TODO: + center * axis;

			idBounds result = new idBounds();
			result.Min = center - rotatedExtents;
			result.Max = center + rotatedExtents;

			return result;
		}

		public static idBounds BoundsForPointRotation(Vector3 start, idRotation rotation)
		{
			Vector3 end = /*TODO start * rotation*/ Vector3.Zero;
			Vector3 axis = rotation.Vector;
			Vector3 origin = rotation.Origin + axis * (axis * (start - rotation.Origin));

			float radiusSqr = (start - origin).LengthSquared();

			Vector3 v1 = Vector3.Cross(start - origin, axis);
			Vector3 v2 = Vector3.Cross(end - origin, axis);

			idBounds result = new idBounds();

			// if the derivative changes sign along this axis during the rotation from start to end
			if(((v1.X > 0.0f) && (v2.X < 0.0f)) || ((v1.X < 0.0f) && (v2.X > 0.0f)))
			{
				if((0.5f * (start.X + end.X) - origin.X) > 0.0f)
				{
					result.Min.X = idMath.Min(start.X, end.X);
					result.Max.X = origin.X + idMath.Sqrt(radiusSqr * (1.0f - axis.X * axis.X));
				}
				else
				{
					result.Min.X = origin.X - idMath.Sqrt(radiusSqr * (1.0f - axis.X * axis.X));
					result.Max.X = idMath.Max(start.X, end.X);
				}
			}
			else if(start.X > end.X)
			{
				result.Min.X = end.X;
				result.Max.X = start.X;
			}
			else
			{
				result.Min.X = start.X;
				result.Max.X = end.X;
			}

			if(((v1.Y > 0.0f) && (v2.Y < 0.0f)) || ((v1.Y < 0.0f) && (v2.Y > 0.0f)))
			{
				if((0.5f * (start.Y + end.Y) - origin.Y) > 0.0f)
				{
					result.Min.Y = idMath.Min(start.Y, end.Y);
					result.Max.Y = origin.Y + idMath.Sqrt(radiusSqr * (1.0f - axis.Y * axis.Y));
				}
				else
				{
					result.Min.Y = origin.Y - idMath.Sqrt(radiusSqr * (1.0f - axis.Y * axis.Y));
					result.Max.Y = idMath.Max(start.Y, end.Y);
				}
			}
			else if(start.Y > end.Y)
			{
				result.Min.Y = end.Y;
				result.Max.Y = start.Y;
			}
			else
			{
				result.Min.Y = start.Y;
				result.Max.Y = end.Y;
			}

			if(((v1.Z > 0.0f) && (v2.Z < 0.0f)) || ((v1.Z < 0.0f) && (v2.Z > 0.0f)))
			{
				if((0.5f * (start.Z + end.Z) - origin.Z) > 0.0f)
				{
					result.Min.Z = idMath.Min(start.Z, end.Z);
					result.Max.Z = origin.Z + idMath.Sqrt(radiusSqr * (1.0f - axis.Z * axis.Z));
				}
				else
				{
					result.Min.Z = origin.Z - idMath.Sqrt(radiusSqr * (1.0f - axis.Z * axis.Z));
					result.Max.Z = idMath.Max(start.Z, end.Z);
				}
			}
			else if(start.Z > end.Z)
			{
				result.Min.Z = end.Z;
				result.Max.Z = start.Z;
			}
			else
			{
				result.Min.Z = start.Z;
				result.Max.Z = end.Z;
			}

			return result;
		}

		public static idBounds Expand(float d)
		{
			return idBounds.Expand(idBounds.Zero, d);
		}

		public static idBounds Expand(idBounds bounds, float d)
		{
			return new idBounds(new Vector3(bounds.Min.X - d, bounds.Min.Y - d, bounds.Min.Z - d),
				new Vector3(bounds.Max.X + d, bounds.Max.Y + d, bounds.Max.Z + d));
		}
		#endregion
	}
}