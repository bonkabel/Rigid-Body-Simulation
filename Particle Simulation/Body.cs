using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Rigid_Body_Simulation
{
	/// <summary>
	/// An implementation of a rigid Body
	/// </summary>
	public class Body
	{

		/// <summary>
		/// The Color of the inside of the Body
		/// </summary>
		public Color FillColor { get { return ((SolidColorBrush)path.Fill).Color; } set { path.Fill = new SolidColorBrush(value); } }

		/// <summary>
		/// The color of the outline of the Body
		/// </summary>
		//public Color OutlineColor { get { return ((SolidColorBrush)path.Stroke).Color; } set { path.Stroke = new SolidColorBrush(Brushes.Black.Color); } }

		/// <summary>
		/// The mass of the particle
		/// </summary>
		private double mass;

		/// <summary>
		/// Property for the mass of the rigid body
		/// </summary>
		public double Mass { get { return mass; } set { mass = value; } }

		/// <summary>
		/// A boolean representing whether or not this Body is a moving body
		/// </summary>
		private bool moving;

		/// <summary>
		/// Property for moving
		/// </summary>
		public bool Moving { get { return moving; } set { moving = value; } }

		/// <summary>
		/// A boolean representing whether or not this Body is currently moving
		/// </summary>
		private bool currentlyMoving;

		/// <summary>
		/// Property for currentlyMoving
		/// </summary>
		public bool CurrentlyMoving { get { return currentlyMoving; } set { currentlyMoving = value; } }

        /// <summary>
        /// The velocity of the rigid body
        /// </summary>
        private Vector velocity;

        /// <summary>
        /// Property for the velocity of the rigid body
        /// </summary>
        public Vector Velocity { get { return velocity; } set { velocity = value; } }

		/// <summary>
		/// Property for the radius of the Body
		/// </summary>
		public double Radius { get { return ellipseGeometry.RadiusY; } set { ellipseGeometry.RadiusX = value; ellipseGeometry.RadiusY = value; } }

		/// <summary>
		/// A unique ID for the Body
		/// </summary>
		private Guid guid;

		/// <summary>
		/// Property for guid
		/// </summary>
		public Guid Guid { get { return guid; } }

		/// <summary>
		/// Property for the x,y coordinates of the centre of the Particleview
		/// </summary>
		public Point Coordinates { get { return ellipseGeometry.Center; } set { ellipseGeometry.Center = value; } }

		/// <summary>
		/// The previous 20 coordinates of the Body
		/// </summary>
		private Point[] previousCoordinates;

		/// <summary>
		/// Property for previousCoordinates
		/// </summary>
		public Point[] PreviousCoordinates { get { return previousCoordinates; } }

		/// <summary>
		/// The radius of the bounding circle
		/// </summary>
		public double BoundingCircleRadius { get { return Radius * 2; } }

		/// <summary>
		/// The gravitational constant
		/// </summary>
		private const double gravitationalConstant = 6.674e-11;


		/// <summary>
		/// EllipseGeometry for the Body
		/// </summary>
		public EllipseGeometry ellipseGeometry = new EllipseGeometry();

		/// <summary>
		/// The Trajectory of the Body
		/// </summary>
		public GeometryGroup trajectory = new GeometryGroup();

		/// <summary>
		/// A list of ellipseGeometries that make up a trajectory
		/// For quick access and editing of the ellipses
		/// </summary>
		private EllipseGeometry[] trajectoryCircles;

		/// <summary>
		/// A list of ellipseGeometries that make up a trajectory
		/// For quick access and editing of the ellipses
		/// property for trajectoryCircles
		/// </summary>
		public EllipseGeometry[] TrajectoryCircles { set { trajectoryCircles = value; } get { return trajectoryCircles; } }

		/// <summary>
		/// The ellipse and the trajectoryCircles
		/// </summary>
		private GeometryGroup ellipseAndTrajectory = new GeometryGroup();

		/// <summary>
		/// The Path of the Body
		/// </summary>
		public Path path = new Path();

		/// <summary>
		/// The size to set previousCoordinates to
		/// </summary>
		int previouscoordinatesSize;

		/// <summary>
		/// The length to trajectory should be
		/// </summary>
		private int trajectoryLength;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="radius">Radius of the particle</param>
		/// <param name="coordinates">Coordinates of the particle</param>
		public Body(Point coordinates, double radius, double mass, Color fillColor, bool moving, bool currentlyMoving)
		{
			Coordinates = coordinates;
			Radius = radius;
			Velocity = new Vector();
			this.Moving = moving;
			this.CurrentlyMoving = currentlyMoving;
			Mass = mass;
			previouscoordinatesSize = 20;
			guid = Guid.NewGuid();
			trajectoryLength = 20;

			//Colours
			FillColor = fillColor;
			//OutlineColor = outlineColor;

			path.Data = ellipseAndTrajectory;





			ellipseAndTrajectory.Children.Add(ellipseGeometry);

			InitializePreviousCoordinates();
		}


		/// <summary>
		/// Initializes the Previous Coordinates
		/// </summary>
		private void InitializePreviousCoordinates()
		{
			previousCoordinates = new Point[previouscoordinatesSize];
			for (int i = 0; i < previouscoordinatesSize; i++)
			{
				previousCoordinates[i] = new Point(Coordinates.X, Coordinates.Y);
			}
		}

		/// <summary>
		/// Calculates the distance of this Body form another Body otherBody, taking into account radius
		/// </summary>
		/// <param name="otherBody">The other Body to calculate the distance from</param>
		/// <returns>The distance between this Body and otherBody in terms of coordinates</returns>
		public double DistanceFrom(Body otherBody)
		{
			double distance = Point.Subtract(Coordinates, otherBody.Coordinates).Length - Radius - otherBody.Radius;

			return distance;
		}

		public double DistanceFromCenter(Point point)
		{
			return Point.Subtract(Coordinates, point).Length;
		}


		/// <summary>
		/// The distyance of this Body from otherCoordinates
		/// </summary>
		/// <param name="otherCoordinates">A Point</param>
		/// <returns>The distance of this Body from otherCoordinates</returns>
		public Vector DistanceFrom(Point otherCoordinates)
		{
			Vector difference = Point.Subtract(Coordinates, otherCoordinates);

			return difference;
		}


		/// <summary>
		/// Determines if a x,y coordinate is inside the MovingBody
		/// </summary>
		/// <param name="otherX">The other x coordinate</param>
		/// <param name="otherY">The other y coordinate</param>
		/// <returns>true if the given coordinate is inside the Particle, otherwise false</returns>
		public bool IsInside(Point otherCoordinates)
		{
			bool inside = false;

			if (DistanceFromCenter(otherCoordinates) <= Radius)
			{
				inside = true;
			}
			return inside;
		}

		/// <summary>
		/// Adds the GeometryDrawing to the DrawingGroup
		/// </summary>
		/// <param name="drawingGroup">The group of geometry drawings</param>
		public void AddToDrawing(Canvas drawingArea)
		{
			drawingArea.Children.Add(path);
		}

		public void InitializeTrajectory()
		{
			ellipseAndTrajectory.Children.Add(trajectory);
			TrajectoryCircles = new EllipseGeometry[trajectoryLength];

			for (int i = 0; i < trajectoryLength; i++)
			{
				EllipseGeometry ellipseGeometry = new EllipseGeometry(new Point(0, 0), 2, 2);
				trajectory.Children.Add(ellipseGeometry);
				trajectoryCircles[i] = ellipseGeometry;

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		public void SetTrajectory(Point[] points)
		{
			int size = trajectoryCircles.Count();
			for (int i = 0; i < size; i++)
			{
				trajectoryCircles[i].Center = points[i];
			}
		}

		public void RemoveTrajectory()
		{
			ellipseAndTrajectory.Children.Remove(trajectory);
			trajectory.Children.Clear();
			trajectoryCircles = null;

		}

		/// <summary>
		/// An implementation of Eulers method to update the position and account for lag
		/// Error per step is propertional to dt squared
		/// </summary>
		/// <param name="body">The body applying force to this MovingBody</param>
		/// <param name="dt">The change in time</param>
		public void Euler(Body body, double dt)
		{
			//Normalizing a Vector of the distance between the movingBody and the Body
			Vector unitVector = Point.Subtract(body.Coordinates, Coordinates);
			unitVector.Normalize();

			//Calculating the acceleration with the unitVector
			Vector acceleration = Vector.Multiply(1000, unitVector);

			//Update the Coordinates, integrategrating the Velocity
			Coordinates = Vector.Add(Vector.Multiply(Velocity, dt), Coordinates);

			//Update the Velocity, integrating the Acceleration
			Velocity = Vector.Add(Velocity, Vector.Multiply(acceleration, dt));
		}


		/// <summary>
		/// Calculates the force due to gravity on the movingBody
		/// </summary>
		/// <param name="body">The body applying the force to this MovingBody</param>
		/// <returns>The force being applied to this Body from body</returns>
		public Vector CalculateForce(Body body)
		{
			Vector unitVector = Point.Subtract(body.Coordinates, Coordinates);
			double distance = unitVector.Length;
			unitVector.Normalize();
			return Vector.Multiply((gravitationalConstant * Mass * body.Mass / distance * distance), unitVector);
		}

		/// <summary>
		/// Calculates the intial force for Verlocity Verlet
		/// </summary>
		/// <param name="body">The body applying force to this MovingBody</param>
		/// <returns>The force being applied to this Body from body</returns>
		public Vector CalculateInitialForce(Body body)
		{
			Vector unitVector = Point.Subtract(body.PreviousCoordinates[0], Coordinates);
			double distance = unitVector.Length;
			unitVector.Normalize();
			return Vector.Multiply((gravitationalConstant * Mass * body.Mass / distance * distance), unitVector);
		}

		/// <summary>
		/// A Euler's implementation of updating the initial velocity
		/// </summary>
		/// <param name="body">The body applying force to this MovingBody</param>
		/// <param name="dt">The change in time</param>
		public void EulerUpdateVelocity(Body body, double dt)
		{
			//Calculating Acceleration
			Vector acceleration = Vector.Divide(CalculateForce(body), Mass);

			//Updating Velocity, integrating acceleration to get the increase in velocity
			Velocity = Vector.Add(Velocity, Vector.Multiply(dt, acceleration));
		}

		public void UpdateMovingCoordinates(double dt)
		{
			UpdateCoordinates(Vector.Add(Vector.Multiply(Velocity, dt), Coordinates));
		}

		/// <summary>
		/// Updates the Body's coordinates
		/// </summary>
		/// <param name="dt">The change in time</param>
		public void UpdateCoordinates(Point newCoordinates)
		{
			Point[] newPreviousCoordinates = new Point[previousCoordinates.Length];

			newPreviousCoordinates[0] = Coordinates;

			for (int i = 1; i < previousCoordinates.Length; i++)
			{
				newPreviousCoordinates[i] = previousCoordinates[i - 1];
			}

			previousCoordinates = newPreviousCoordinates;
			Coordinates = newCoordinates;
		}

		/// <summary>
		/// A Velocity Verlet implementation of updating the initial velocity
		/// </summary>
		/// <param name="body">The body applying force to this MovingBody</param>
		/// <param name="dt">The change in time</param>
		public void VerletUpdateInitialVelocity(Body body, double dt)
		{
			//Calculating Acceleration
			Vector acceleration = Vector.Divide(CalculateInitialForce(body), Mass);

			//Updating Velocity
			Velocity = Vector.Add(Velocity, Vector.Multiply(0.5 * dt, acceleration));
		}

		/// <summary>
		/// Updates the final verlocity for Verlocity Verlet
		/// </summary>
		/// <param name="body">The body applying force</param>
		/// <param name="dt">The change in time</param>
		public void VerletUpdateFinalVelocity(Body body, double dt)
		{
			//Calculating Acceleration
			Vector acceleration = Vector.Divide(CalculateForce(body), Mass);

			//Updating Velocity
			Velocity = Vector.Add(Velocity, Vector.Multiply(0.5 * dt, acceleration));
		}

		/// <summary>
		/// Returns a deep copy of the Body
		/// </summary>
		/// <returns>A deep copy of the Body</returns>
		public Body DeepCopy()
		{
			Body body = new Body(Coordinates, Radius, Mass, FillColor, moving, currentlyMoving);
			body.guid = guid;
			body.Velocity = Velocity;
			body.CurrentlyMoving = CurrentlyMoving;
			return body;
		}

        /// <summary>
        /// </summary>
        /// <returns>information about the Body</returns>
        public override String ToString()
		{
			return "Body: " +
				"\n\tCoordinates: (" + Math.Round(Coordinates.X, 2) + ", " + Math.Round(Coordinates.Y, 2) + ") " +
				"\n\tMoving Body: " + Moving + " " +
				"\n\tCurrently Moving: " + currentlyMoving + " " +
				"\n\tVelocity: " + Velocity;
		}
	}
}
