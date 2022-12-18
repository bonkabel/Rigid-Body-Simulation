using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Rigid_Body_Simulation
{
	/// <summary>
	/// An implmenentation of a narrowphase
	/// Goes through a List<List<Body>> of potentially colliding Bodys and determines which List<Body> are actually colliding
	/// </summary>
	class Narrowphase
	{
		//The List<Body>s that are already colliding
		private List<List<Body>> alreadyColliding = new List<List<Body>>();

		/// <summary>
		/// Determines which List<Body>s should collide 
		/// Checks if each potentially colliding bodies are colliding
		/// And updates velocity accordingly
		/// </summary>
		/// <param name="bodiesToCheck"></param>
		public void FindColliding(List<List<Body>> bodiesToCheck)
		{
			UpdateAlreadyColliding();


			foreach (List<Body> currentBodiesToCheck in bodiesToCheck)
			{
				if (CheckCollision(currentBodiesToCheck[0], currentBodiesToCheck[1]))
				{
					Collision(currentBodiesToCheck[0], currentBodiesToCheck[1]);
					alreadyColliding.Add(currentBodiesToCheck);

				}
			}
		}

		/// <summary>
		/// Goes through the List<Body>s in alreadyColliding and removes the ones no longer colliding
		/// </summary>
		public void UpdateAlreadyColliding()
		{
			//A List of lists of Bodys that are no longer colliding
			List<List<Body>> bodiesToRemove = new List<List<Body>>();
			foreach (List<Body> bodies in alreadyColliding)
			{
				if (!CheckCollision(bodies[0], bodies[1]))
				{
					bodiesToRemove.Add(bodies);
				}
			}

			foreach (List<Body> bodies in bodiesToRemove)
			{
				alreadyColliding.Remove(bodies);
			}
		}

		/// <summary>
		/// Checks if body1 and body2 are colliding
		/// </summary>
		/// <param name="body1">A body</param>
		/// <param name="body2">A body</param>
		/// <returns>Whether or not they are colliding</returns>
		public bool CheckCollision(Body body1, Body body2)
		{
			bool colliding = false;

			if (Math.Abs(Point.Subtract(body1.Coordinates, body2.Coordinates).Length) - body1.Radius - body2.Radius <= 0)
			{
				colliding = true;
			}

			return colliding;
		}

		/// <summary>
		/// Handles a collision between two Bodys and updates the Bodys velocty
		/// </summary>
		/// <param name="body1"></param>
		/// <param name="body2"></param>
		public void Collision(Body body1, Body body2)
		{
			//Getting the unit normal vector
			Vector unitNormalVector = Point.Subtract(body1.Coordinates, body2.Coordinates);
			unitNormalVector.Normalize();

			//Getting the unit tangent vector
			Vector unitTangentVector = new Vector(-unitNormalVector.Y, unitNormalVector.X);
			Vector body1Velocity = new Vector();
			Vector body2Velocity = new Vector();

			if (body1.CurrentlyMoving && body2.CurrentlyMoving)
			{
				body1Velocity = CalculateCollisionVelocity(body1, body2, unitNormalVector, unitTangentVector);
				body2Velocity = CalculateCollisionVelocity(body2, body1, unitNormalVector, unitTangentVector);
				CorrectPositionTwoBody(body1, body2);

			}
			else
			{
				if (body1.CurrentlyMoving)
				{
					body1Velocity = CalculateCollisionVelocity(body1, body2, unitNormalVector, unitTangentVector);
					CorrectPositionOneBody(body1, body2);
				}

				if (body2.CurrentlyMoving)
				{
					body2Velocity = CalculateCollisionVelocity(body2, body1, unitNormalVector, unitTangentVector);
					CorrectPositionOneBody(body2, body1);
				}
			}


			body1.Velocity = body1Velocity;
			body2.Velocity = body2Velocity;
		}

		/// <summary>
		/// Calculates the new Velocity of the Body after collision
		/// </summary>
		/// <param name="body">The body to update Velocity on</param>
		/// <param name="otherBody">The Body body is colliding with</param>
		/// <param name="unitNormalVector">The unit normal vector</param>
		/// <param name="unitTangentVector">The unit tangent vector</param>
		public Vector CalculateCollisionVelocity(Body body, Body otherBody, Vector unitNormalVector, Vector unitTangentVector)
		{
			//Projecting the velocity vectors of body onto the unitNormalVector and unitTangentVector ofthe body
			double initialNormalVelocity = Vector.Multiply(unitNormalVector, body.Velocity);
			double tangentVelocity = Vector.Multiply(unitTangentVector, body.Velocity);

			//Calculating the new normal velocity
			double normalVelocity = (initialNormalVelocity * (body.Mass - otherBody.Mass) + 2 * otherBody.Mass * Vector.Multiply(unitNormalVector, otherBody.Velocity)) / (body.Mass + otherBody.Mass);
			//Setting the velocity of the body to the new velocity after the collision
			return normalVelocity * unitNormalVector + tangentVelocity * unitTangentVector;
		}

		/// <summary>
		/// Checks if two Bodys are in a resting collision state
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="velocity"></param>
		/// <returns>Whether or not there is a resting collision</returns>
		public bool CheckRestingCollision(double distance, Vector velocity)
		{
			bool resting = false;
			double maxRestingDistance = 10;
			double maxRestingVelocity = 3;
			distance = Math.Abs(distance);

			if (distance < maxRestingDistance && distance > 0 && Math.Abs(velocity.Length) < maxRestingVelocity)
			{
				resting = true;
			}

			return resting;
		}


		/// <summary>
		/// Corrects the inteperpenetration of two Bodys by equally moving them apart
		/// </summary>
		/// <param name="body">A Body</param>
		/// <param name="otherBody">A Body</param>
		public void CorrectPositionTwoBody(Body body, Body otherBody)
		{
			double distance = body.DistanceFrom(otherBody);

			if (distance < 0)
			{
				Vector coordinatesCorrection = Point.Subtract(otherBody.Coordinates, body.Coordinates);
				coordinatesCorrection.Normalize();
				coordinatesCorrection = coordinatesCorrection * (distance / 2);
				body.Coordinates = Point.Add(body.Coordinates, coordinatesCorrection);
				otherBody.Coordinates = Point.Subtract(otherBody.Coordinates, coordinatesCorrection);


			}

		}

		/// <summary>
		/// Correct the interpenetration of two bodys by moving one apart from the other
		/// </summary>
		/// <param name="body">A Body</param>
		/// <param name="otherBody">A Body</param>
		public void CorrectPositionOneBody(Body body, Body otherBody)
		{
			double distance = body.DistanceFrom(otherBody);

			if (distance < 0)
			{
				Vector coordinatesCorrection = Point.Subtract(otherBody.Coordinates, body.Coordinates);
				coordinatesCorrection.Normalize();
				coordinatesCorrection = coordinatesCorrection * distance;
				body.Coordinates = Point.Add(body.Coordinates, coordinatesCorrection);
			}
		}

		/// <summary>
		/// Handles if a body is in a resting collision
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="body1"></param>
		public void RestingCollision(double distance, Body body1)
		{
			if (CheckRestingCollision(distance, body1.Velocity))
			{
				body1.Velocity = new Vector();
			}
		}
	}
}
