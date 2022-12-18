using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rigid_Body_Simulation
{
	/// <summary>
	/// An implementation of a broadphase for collision detection
	/// Goes through Bodies and determines which need to be checked for collision
	/// </summary>
	class Broadphase
	{
		BodyXComparer comparer = new BodyXComparer();

		/// <summary>
		/// Takes a list of bodies and returns it sorted by the x coordinates of the bottom left of the bounding box
		/// </summary>
		/// <param name="bodies">The list of bodies</param>
		public void SortBodies(List<Body> bodies)
		{
			bodies.Sort(comparer);
		}

		/// <summary>
		/// Sweeps through a sorted list of bodies and returns the bodies that have intersecting bounding boxes
		/// </summary>
		/// <param name="sortedBodies">The sortedlist of bodies to sweep through</param>
		/// <returns>A List of Lists of bodies that have intersecting bounding boxes</returns>
		public List<List<Body>> SweepBodies(List<Body> sortedBodies)
		{
			//The bodies
			List<List<Body>> bodiesToCheck = new List<List<Body>>();

			int body1Index = 0;

			//Is there a better way of doing these loops, seems messy?
			while (body1Index < sortedBodies.Count - 1)
			{
				int body2Index = body1Index + 1;
				while (body2Index < sortedBodies.Count)
				{
					if (TestBoundingOverlap(sortedBodies[body1Index], sortedBodies[body2Index]))
					{
						bodiesToCheck.Add(new List<Body> { sortedBodies[body1Index], sortedBodies[body2Index] });
					}
					body2Index++;
				}
				body1Index++;
			}

			return bodiesToCheck;
		}

		/// <summary>
		/// Sorts the bodies then sweeps through them and returns the ones with intersecting bounding boxes
		/// </summary>
		/// <param name="bodies">The bodies to sort and sweep</param>
		/// <returns>A list of lists, each containing two bodies that have colliding bounding boxes</returns>
		public List<List<Body>> SortAndSweep(List<Body> bodies)
		{
			SortBodies(bodies);
			return SweepBodies(bodies);
		}

		/// <summary>
		/// Adds bodiesToAdd to sortedBodies, keeping it sorted the sweeps through sortedBodies and returns the ones with intersecting bounding boxes
		/// </summary>
		/// <param name="sortedBodies">The bodies to add to</param>
		/// <param name="bodiesToAdd">The bodies to add</param>
		/// <returns>A list of lists, each containing two bodies that have colliding bounding boxes</returns>
		public List<List<Body>> AddAndSweep(List<Body> sortedBodies, List<Body> bodiesToAdd)
		{
			AddBodies(sortedBodies, bodiesToAdd);

			return SweepBodies(sortedBodies);
		}

		/// <summary>
		/// Adds each body in bodiesToAdd to sortedBodies, keeping it sorted
		/// </summary>
		/// <param name="sortedBodies">The list of bodies, sorted</param>
		/// <param name="bodiesToAdd">The bodies to add to sortedBodies</param>
		public void AddBodies(List<Body> sortedBodies, List<Body> bodiesToAdd)
		{
			foreach (Body body in bodiesToAdd)
			{
				int index = sortedBodies.BinarySearch(body, comparer);

				if (index >= 0)
				{
					sortedBodies.Insert(index, body);
				}
				else
				{
					sortedBodies.Insert(~index, body);
				}

			}
		}

		/// <summary>
		/// Tests to see if the boundingCircleRadius of the body1 and body2 are overlapping
		/// </summary>
		/// <param name="body1">A Body</param>
		/// <param name="body2">A Body</param>
		/// <returns>a boolean value representing if two bodies are overlapping or not</returns>
		public bool TestBoundingOverlap(Body body1, Body body2)
		{
			bool overlap = false;
			if (Math.Abs(Point.Subtract(body1.Coordinates, body2.Coordinates).Length) - body1.BoundingCircleRadius - body2.BoundingCircleRadius <= 0)
			{
				overlap = true;
			}

			return overlap;
		}

	}
}
