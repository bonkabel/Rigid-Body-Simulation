using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Rigid_Body_Simulation
{
	public class Trajectory
	{
		private double ellipseRadius;
		private GeometryGroup trajectory = new GeometryGroup();

		public Trajectory()
		{
			ellipseRadius = 10;
		}

		public void AddPoints(Point[] points)
		{
			foreach (Point point in points)
			{
				trajectory.Children.Add(new EllipseGeometry(point, ellipseRadius, ellipseRadius));
			}
		}

		public void AddTrajectory(GeometryGroup geometryGroup)
		{
			geometryGroup.Children.Add(trajectory);
		}
	}
}
