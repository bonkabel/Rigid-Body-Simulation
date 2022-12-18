using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigid_Body_Simulation
{
	/// <summary>
	/// A class for comparing Bodys
	/// </summary>
	class BodyXComparer : IComparer<Body>
	{
		/// <summary>
		/// Compares the x coordinates of two Bodys
		/// </summary>
		/// <param name="body1">A Body</param>
		/// <param name="body2">A Body</param>
		/// <returns></returns>
		public int Compare(Body body1, Body body2)
		{
			int result;
			if (body1.ellipseGeometry.Bounds.BottomLeft.X > body2.ellipseGeometry.Bounds.BottomLeft.X)
			{
				result = 1;
			}
			else if (body1.ellipseGeometry.Bounds.BottomLeft.X < body2.ellipseGeometry.Bounds.BottomLeft.X)
			{
				result = -1;
			}
			else
			{
				result = 0;
			}

			return result;
		}
	}
}
