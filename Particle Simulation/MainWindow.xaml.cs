using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Rigid_Body_Simulation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// The broadphase for collision detection
		/// </summary>
		private Broadphase broadphase = new Broadphase();

		/// <summary>
		/// The narrowphase for collision detection
		/// </summary>
		private Narrowphase narrowphase = new Narrowphase();

		/// <summary>
		/// List of the Paths of all the Bodies that are Moving bodies
		/// </summary>
		private List<Path> movingBodyPaths = new List<Path>();

		/// <summary>
		/// List of the Paths of all the Bodies
		/// </summary>
		private List<Path> bodyPaths = new List<Path>();


		/// <summary>
		/// List of all the Bodies
		/// </summary>
		private List<Body> bodies = new List<Body>();


		/// <summary>
		/// The Body that is being dragged, else null
		/// </summary>
		private Body dragging;

		/// <summary>
		/// The Body that is being editted, else null
		/// </summary>
		private Body editing;

		/// <summary>
		/// The currently selected ball
		/// </summary>
		private Body selectedBody;

		/// <summary>
		/// The time of the last render
		/// </summary>
		private TimeSpan lastRender;

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			SetDefaults();
		}

		/// <summary>
		/// Sets the default values for WPF elements
		/// </summary>
		public void SetDefaults()
		{
			//Setting the size of the window to 80% of the screen
			this.Height = (System.Windows.SystemParameters.FullPrimaryScreenHeight * 0.80);
			this.Width = (System.Windows.SystemParameters.FullPrimaryScreenWidth * 0.80);

			///Setting lastRender
			lastRender = TimeSpan.FromTicks(DateTime.Now.Ticks);


			CompositionTarget.Rendering += CompositionTarget_Rendering;
		}


		/// <summary>
		/// Handler for the rendering event
		/// Updates the positions of the Bodies
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			//Casting to RenderingEventArgs to get the rendering time
			RenderingEventArgs renderArgs = (RenderingEventArgs)e;

			//Gets the time until next render
			double dt = (renderArgs.RenderingTime - lastRender).TotalSeconds;
			lastRender = renderArgs.RenderingTime;

			ExecuteFrame(dt, bodies);

		}

		/// <summary>
		/// Executes every frame
		/// </summary>
		/// <param name="dt">Time change since last frame</param>
		/// <param name="bodies">The Bodys</param>
		public void ExecuteFrame(double dt, List<Body> bodies)
		{

			//If a Body is being dragged, update its coordinates to the current mouse coordinates
			if (dragging != null)
			{
				dragging.UpdateCoordinates(Mouse.GetPosition(drawingArea));
			}

			//If a Body is being edited
			if (editing != null)
			{
				UpdateEditingVelocity(editing, Mouse.GetPosition(drawingArea));
				//Add the previous coordinates of the tracked body to its trajectory
				editing.SetTrajectory(CreateTrajectory(bodies, editing, dt));

			}

			MotionAndCollision(dt, bodies);

			DisplaySelectedInfo();
		}
		/// <summary>
		/// Sets the velocity of editing based on the distance and position of the mouse
		/// </summary>
		/// <param name="body">The Body</param>
		/// <param name="mouseCoordinates">The coordinates of the mouse</param>
		private void UpdateEditingVelocity(Body body, Point mouseCoordinates)
		{
			if (mouseCoordinates != body.Coordinates)
			{
				Vector newVelocity = body.DistanceFrom(mouseCoordinates);
				double distance = newVelocity.Length;
				newVelocity.Normalize();
				newVelocity = newVelocity * distance;

				body.Velocity = newVelocity;
			}
		}




		/// <summary>
		/// Creates the trajectory for the body
		/// Will lag if using more than ~10 Bodies due to WPF get_coordinates
		/// </summary>
		/// <param name="bodies">The bodies to simulate</param>
		/// <param name="body">The body to create the trajectory for</param>
		/// <returns>The trajectory of body</returns>
		private Point[] CreateTrajectory(List<Body> bodies, Body body, double dt)
		{
			Point[] points = new Point[body.TrajectoryCircles.Length];

			(List<Body> bodiesToSimulate, Body bodyToTrack) = CopyBodies(bodies, body.Guid);



			int pointsAdded = 0;
			double timeElapsed = 0;

			double timeChange = 0.016665;
			double timeBetweenBodies = 0.1;


			while (pointsAdded < points.Length)
			{
				MotionAndCollision(timeChange, bodiesToSimulate);

				if (timeElapsed >= timeBetweenBodies)
				{
					points[pointsAdded] = bodyToTrack.Coordinates;
					pointsAdded++;
					timeElapsed = 0;
				}

				timeElapsed = timeElapsed + timeChange;
			}

			return points;

		}

		/// <summary>
		/// Creates a deep copy of all the bodys in Bodies in newBodies
		/// </summary>
		/// <param name="newBodies"></param>
		private (List<Body>, Body) CopyBodies(List<Body> bodiesToCopy, Guid idToTrack)
		{
			List<Body> newBodies = new List<Body>();

			Body bodyToTrack = null;


			foreach (Body body in bodiesToCopy)
			{
				if (body.Guid == idToTrack)
				{

					bodyToTrack = body.DeepCopy();
					newBodies.Add(bodyToTrack);
					bodyToTrack.Moving = true;
					bodyToTrack.CurrentlyMoving = true;
				}
				else
				{
					newBodies.Add(body.DeepCopy());
				}

			}

			return (newBodies, bodyToTrack);
		}


		/// <summary>
		/// Determines which integration method to use and whether or not to include collision
		/// </summary>
		/// <param name="dt">Time until the next render</param>
		/// <param name="bodies">The list of Bodies</param>
		public void MotionAndCollision(double dt, List<Body> bodies)
		{
			//Determine which numerical method to use to integrate motion
			if (movementMethodComboBox.SelectedValue.ToString() == "Velocity Verlet")
			{
				VelocityVerlet(dt, bodies);
			}
			else if (movementMethodComboBox.SelectedValue.ToString() == "Euler")
			{
				Euler(dt, bodies);
			}

			if (collisionCheckbox.IsChecked.Value)
			{
				List<List<Body>> bodiesTocheck = broadphase.SweepBodies(bodies);

				narrowphase.FindColliding(bodiesTocheck);

			}
		}

		public void PaintBody(Body body)
		{
			if (body.Moving)
			{
				body.FillColor = (Color)movingBodyColorPicker.SelectedColor;
			}
			else
			{
				body.FillColor = (Color)bodyColorPicker.SelectedColor;
			}
		}

		/// <summary>
		/// On MouseLeftButtonDown in the drawingArea, decides what to do based on the selection in onClickComboBox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">The mouse event that triggered the handler</param>
		private void DrawingArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (onClickListBox.SelectedValue.ToString() == "Select")
			{
				foreach (Body body in bodies)
				{
					if (body.IsInside(e.GetPosition(drawingArea)))
					{
						if (selectedBody != null)
						{
							PaintBody(selectedBody);
						}
						selectedBody = body;
						selectedBody.FillColor = ((SolidColorBrush)selectedBodyLabel.Background).Color;
					}
				}

			}
			else if (onClickListBox.SelectedValue.ToString() == "Create Static Body")
			{
				double radius = (double)staticBodyRadiusIUD.Value;
				Point coordinates = e.GetPosition(drawingArea);

				AddBody(radius, coordinates);
			}
			else if (onClickListBox.SelectedValue.ToString() == "Create Moving Body")
			{
				double radius = (double)movingBodyRadiusIUD.Value;
				Point coordinates = e.GetPosition(drawingArea);

				if (EditBodyCheckbox.IsChecked.HasValue && EditBodyCheckbox.IsChecked.Value)
				{
					drawingArea.CaptureMouse();
					editing = AddMovingEditing(radius, coordinates);
				}
				else
				{
					AddMovingBody(radius, coordinates);
				}


			}
			else if (onClickListBox.SelectedValue.ToString() == "Drag Body" && dragging is null)
			{
				foreach (Body body in bodies)
				{
					if (body.IsInside(e.GetPosition(drawingArea)))
					{
						dragging = body;
						if (dragging.CurrentlyMoving)
						{
							dragging.Velocity.Normalize();
							dragging.Velocity = Vector.Multiply(dragging.Velocity, 0);
							drawingArea.CaptureMouse();
						}
					}
				}
			}
		}



		/// <summary>
		/// Adds a Body to the geometryGroup
		/// </summary>
		/// <param name="radius">The radius of the particle</param>
		/// <param name="coordinates">The cooridnates of the particle</param>
		public void AddBody(double radius, Point coordinates)
		{
			Body body = new Body(coordinates, radius, (double)staticBodyMassDUD.Value, bodyColorPicker.SelectedColor.Value, false, false);

			body.AddToDrawing(drawingArea);
			bodyPaths.Add(body.path);


			broadphase.AddBodies(bodies, new List<Body> { body });
		}

		/// <summary>
		/// Adds a Body to the GeometryGroup[ that can be edited
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="coordinates"></param>
		/// <returns></returns>
		public Body AddMovingEditing(double radius, Point coordinates)
		{
			Body body = new Body(coordinates, radius, (double)movingBodyMassDUD.Value, movingBodyColorPicker.SelectedColor.Value, true, false);

			body.AddToDrawing(drawingArea);
			body.InitializeTrajectory();
			movingBodyPaths.Add(body.path);

			broadphase.AddBodies(bodies, new List<Body> { body });


			return body;
		}


		/// <summary>
		/// Adds a MovingBody to the GeometryGroup
		/// </summary>
		/// <param name="radius">The radius of the particle</param>
		/// <param name="radius">The radius of the particle</param>
		/// <param name="coordinates">The cooridnates of the particle</param>
		public void AddMovingBody(double radius, Point coordinates)
		{
			Body body = new Body(coordinates, radius, (double)movingBodyMassDUD.Value, movingBodyColorPicker.SelectedColor.Value, true, true);

			body.AddToDrawing(drawingArea);
			movingBodyPaths.Add(body.path);

			broadphase.AddBodies(bodies, new List<Body> { body });
		}

		/// <summary>
		/// Handler for the left mouse button being released
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DrawingArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (dragging != null)
			{
				dragging = null;
				drawingArea.ReleaseMouseCapture();
			}

			if (editing != null)
			{
				editing.RemoveTrajectory();
				editing.CurrentlyMoving = true;
				editing = null;


				drawingArea.ReleaseMouseCapture();

			}
		}

		/// <summary>
		/// Handler for a click on the clearButton
		/// On click, clears the Bodies in bodyGroup and the movingBodies in movingBodyGroup
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			movingBodyPaths.Clear();
			bodyPaths.Clear();
			bodies.Clear();
			drawingArea.Children.Clear();
		}


		/// <summary>
		/// An implementation of the Verlocity Verlet method for integrating motion
		/// </summary>
		private void VelocityVerlet(double dt, List<Body> bodies)
		{
			//For every Body that is Moving and isn't being dragged
			//go through all the bodies in allbodies,
			//and update the Bodys velocity and coordinates if the body is not the movingBody
			foreach (Body body in bodies)
			{
				if (body != dragging && body.CurrentlyMoving)
				{
					foreach (Body otherBody in bodies)
					{
						if (otherBody != body)
						{
							body.VerletUpdateInitialVelocity(otherBody, dt);
						}
					}

					body.UpdateMovingCoordinates(dt);
				}
			}

			//For every Moving Body that isn't being dragged, 
			//go through all the bodies in allbodies,
			//and update the Bodys velocity if the body is not the movingBody
			foreach (Body body in bodies)
			{
				if (body != dragging && body.CurrentlyMoving)
				{
					foreach (Body otherBody in bodies)
					{
						if (otherBody != body)
						{
							body.VerletUpdateFinalVelocity(otherBody, dt);
						}

					}

				}
			}
		}

		/// <summary>
		/// An implementaion of Euler's method for integrating motion
		/// </summary>
		public void Euler(double dt, List<Body> bodies)
		{
			//For every movingbody that isn't being dragged, 
			//go through all the bodies in allbodies,
			//and update the movingBodys velocity and coordinates if the body is not the movingBody
			foreach (Body body in bodies)
			{
				if (body != dragging && body.CurrentlyMoving)
				{
					foreach (Body otherBody in bodies)
					{
						if (otherBody != body)
						{
							body.EulerUpdateVelocity(otherBody, dt);
							body.UpdateMovingCoordinates(dt);
						}
					}


				}
			}
		}

		/// <summary>
		/// Calculates the new normal velocity of a body after collision with another body
		/// </summary>
		/// <param name="normalVelocity1">The normal velocity to update</param>
		/// <param name="body1">The body with the velocity to update</param>
		/// <param name="normalVelocity2">The other bodies normal velocity</param>
		/// <param name="body2">The other body</param>
		/// <returns>The updated normalVelocity1</returns>
		public double CalculateNewNormalVelocity(double normalVelocity1, Body body1, double normalVelocity2, Body body2)
		{
			return normalVelocity1 * (body1.Mass - body2.Mass) + 2 * body2.Mass * normalVelocity2 / body1.Mass + body2.Mass;
		}

		/// <summary>
		/// Prints the body information
		/// Debugging purposes
		/// </summary>
		/// <param name="bodies"></param>
		public void PrintBodies(List<Body> bodies)
		{
			Console.WriteLine("Printing Body Information:");
			foreach (Body body in bodies)
			{
				Console.WriteLine("Body:");
				foreach (Point point in body.PreviousCoordinates)
				{
					Console.WriteLine(point);
				}
			}
		}

		/// <summary>
		/// Displays information on selectedBody
		/// </summary>
		public void DisplaySelectedInfo()
		{
			if (selectedBody is Body)
			{
				selectedVelocityLabel.Content = "" + Math.Round(selectedBody.Velocity.Length, 2);


				double xCoordinate = Math.Round(selectedBody.Coordinates.X, 2);
				double yCoordinate = Math.Round(selectedBody.Coordinates.Y, 2);

				selectedXLabel.Content = xCoordinate;
				selectedYLabel.Content = yCoordinate;


			}

		}
	}
}