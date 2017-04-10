using System;
using System.Collections.Generic;
using OpenTK;

namespace GraphicsLibrary.Core
{
	public class Node
	{
		/// <summary>
		/// Maximum rotational velocity in revolations per second, lower values are more accurate.
		/// </summary>
		public const float rotationalVelocityScale = 16f;
		/// <summary>
		/// Position relative to parent.
		/// </summary>
		public Vector3 position = Vector3.Zero;
		/// <summary>
		/// Position relative to the world.
		/// </summary>
		public Vector3 derivedPosition = Vector3.Zero;
		public Vector3 prevRelativeToCam = Vector3.Zero;

		/// <summary>
		/// Velocity relative to parent.
		/// </summary>
		public Vector3 velocity = Vector3.Zero;
		/// <summary>
		/// Velocity relative to world.
		/// </summary>
		public Vector3 derivedVelocity = Vector3.Zero;

		public Vector3 acceleration = Vector3.Zero;
		/// <summary>
		/// Friction, use (1,1,1) to disable.
		/// </summary>
		public Vector3 friction = new Vector3(1, 1, 1);

		/// <summary>
		/// Own mass in kg.
		/// </summary>
		public float mass = 1f;

		/// <summary>
		/// Combined mass of itself and all contained nodes in kg.
		/// </summary>
		public float derivedMass = 125f; //TODO: recursive update

		/// <summary>
		/// Whether or not the derived mass requires an update.
		/// </summary>
		public bool derivedMassNeedsUpdate = true;

		/// <summary>
		/// Approximate inertia at any rotation angle through the mass center in kg*m^2.
		/// </summary>
		public float inertia = 0f;

		/// <summary>
		/// Scale relative to parent.
		/// </summary>
		public Vector3 scale = new Vector3(1, 1, 1);
		/// <summary>
		/// Scale relative to the world.
		/// </summary>
		public Vector3 derivedScale = new Vector3(1, 1, 1);

		/// <summary>
		/// Orientation relative to parent.
		/// </summary>
		public Quaternion orientation = Quaternion.Identity;
		/// <summary>
		/// Orientation relative to the world.
		/// </summary>
		public Quaternion derivedOrientation = Quaternion.Identity;

		public Quaternion rotationalVelocity = Quaternion.Identity;

		/// <summary>
		/// The parent node.
		/// </summary>
		public Node parent;
		/// <summary>
		/// Node name (must be unique).
		/// </summary>
		public string name;

		/// <summary>
		/// Debug rendering.
		/// </summary>
		public bool debugRendering = false;
		/// <summary>
		/// Rendering pass, starting at 0.
		/// </summary>
		public int renderPass = 0;
		public bool ignoreDrawDistance = true;

		/// <summary>
		/// Entity visibility.
		/// </summary>
		public bool isVisible = true;

		public bool occlude = false;
		public Vector3 occlusionOffset = Vector3.Zero;
		public float occlusionRadius = 16f;

		/// <summary>
		/// All children attached to this node.
		/// </summary>
		public Dictionary<string, Node> children = new Dictionary<string, Node>();

		public Node(string name)
		{
			this.name = name;
		}

		private List<QueueItem> eventQueue = new List<QueueItem>();

		/// <summary>
		/// Add a method to be queued. The method will be called when the event (traveling at c) has reached the camera.
		/// </summary>
		/// <param name="method">The method to be called</param>
		public void QueueMethod(QueueEvent method)
		{
			eventQueue.Add(new QueueItem(method));
		}

		/// <summary>
		/// Updates position, velocity, orientation and queued methods of this node, and all its children. Override Update to add custom update functionality.
		/// </summary>
		/// <param name="timeSinceLastUpdate">Time since last update, in seconds</param>
		public virtual void UpdateNode(float timeSinceLastUpdate)
		{
			velocity += Vector3.Multiply(acceleration, timeSinceLastUpdate);
			velocity = Vector3.Multiply(velocity, new Vector3((float)Math.Pow(friction.X, timeSinceLastUpdate / RenderWindow.Instance.lf), (float)Math.Pow(friction.Y, timeSinceLastUpdate / RenderWindow.Instance.lf), (float)Math.Pow(friction.Z, timeSinceLastUpdate / RenderWindow.Instance.lf)));
			position += Vector3.Multiply(velocity, timeSinceLastUpdate);
			orientation = Quaternion.Multiply(orientation, Quaternion.Slerp(Quaternion.Identity, rotationalVelocity, timeSinceLastUpdate * rotationalVelocityScale));

			if(parent == null)
			{
				derivedOrientation = orientation;
				derivedPosition = position;
				derivedScale = scale;
				derivedVelocity = velocity;
			}
			else
			{
				if(parent == Camera.Instance)
				{
					derivedOrientation = Quaternion.Conjugate(parent.derivedOrientation) * orientation;
				}
				else
				{
					derivedOrientation = (orientation * parent.derivedOrientation).Normalized();
				}
				/*Vector3 t = 2 * Vector3.Cross(parent.derivedOrientation.Xyz, position);
				derivedPosition = parent.derivedPosition + (position + parent.derivedOrientation.W * t + Vector3.Cross(parent.derivedOrientation.Xyz, t));*/
				/*derivedPosition = parent.derivedPosition + (parent.derivedOrientation * new Quaternion(position, 0f) * Quaternion.Conjugate(parent.derivedOrientation)).Xyz;*/
				derivedPosition = parent.derivedPosition + RotateVector(position, parent.derivedOrientation);
				derivedScale = Vector3.Multiply(parent.derivedScale, scale);
				derivedVelocity = parent.derivedVelocity + velocity;

			}

			if(derivedMassNeedsUpdate)
			{
				UpdateMass();
			}

			foreach(Node n in children.Values)
			{
				n.UpdateNode(timeSinceLastUpdate);
			}

			Vector3 relativeToCam = derivedPosition - Camera.Instance.position;



			for(int i = 0; i < eventQueue.Count; i++)
			{
				if(eventQueue[i].Update(timeSinceLastUpdate / RenderWindow.Instance.lf, relativeToCam.Length))
				{
					eventQueue[i].method(this);
					eventQueue.RemoveAt(i);
					i--;
				}
			}

			// Relativity of time and space:
			// TODO: mult by tau?
			if(RenderWindow.Instance.enableRelativity)
			{
				float tau = (prevRelativeToCam.Length - relativeToCam.Length) / RenderWindow.Instance.c;
				Update(timeSinceLastUpdate + tau);
			}
			else
			{
				Update(timeSinceLastUpdate);
			}
			prevRelativeToCam = relativeToCam;
		}

		/// <summary>
		/// Override this method to add custom update functionality.
		/// </summary>
		/// <param name="timeSinceLastUpdate">Time since last update, in seconds</param>
		public virtual void Update(float timeSinceLastUpdate)
		{

		}

		public void UpdateMass()
		{
			derivedMass = mass;
			foreach(Node n in children.Values)
			{
				if(n.derivedMassNeedsUpdate)
				{
					n.UpdateMass();
				}
				derivedMass += n.derivedMass;
			}
			derivedMassNeedsUpdate = false;
		}

		public float DeriveInertia(Vector3 rotationAngle) // TODO: recursive
		{
			float output = inertia;
			rotationAngle.Normalize();
			foreach(Node n in children.Values)
			{
				if(n.inertia > 0f)
				{
					float dist = 0.01f * Vector3.Cross(rotationAngle, n.position).Length;
					output += dist * dist * n.inertia;
				}
			}
			return output;
		}

		/// <summary>
		/// Add specified node as child, and rename it.
		/// </summary>
		/// <param name="node">The node to be added</param>
		/// <param name="newName">The new node name</param>
		public void Add(Node node, string newName)
		{
			node.name = newName;
			if(node == this)
			{

			}
			else
			{
				node.parent = this;
				children.Add(node.name, node);
				if(node.derivedMassNeedsUpdate)
				{
					node.UpdateMass();
				}
				if(node.mass != 0f)
				{
					UpdateMass();
				}
			}
		}

		/// <summary>
		/// Add specified node as child.
		/// </summary>
		/// <param name="node">The node to be added</param>
		public void Add(Node node)
		{
			if(node == this)
			{

			}
			else
			{
				node.parent = this;
				children.Add(node.name, node);
			}
		}

		/// <summary>
		/// Checks if this node contains the specified node. 
		/// </summary>
		/// <param name="node">The node</param>
		/// <returns>True if this node contains the specified node</returns>
		public bool HasChild(Node node)
		{
			return this == node.parent; // TODO: performance check
			//return children.ContainsValue(node);
		}

		/// <summary>
		/// Checks if this node contains the specified node. 
		/// </summary>
		/// <param name="childName">The node's name</param>
		/// <returns>True if this node contains the specified node</returns>
		public bool HasChild(string childName)
		{
			return children.ContainsKey(childName);
		}

		/// <summary>
		/// Gets a child.
		/// </summary>
		/// <param name="childName">The child name</param>
		/// <returns>Child</returns>
		public Node GetChild(string childName)
		{
			return children[childName];
		}

		/// <summary>
		/// Delete and detach specified child.
		/// </summary>
		/// <param name="childName">The child name</param>
		public void RemoveChild(string childName)
		{
			if(children.ContainsKey(childName))
			{
				Node child = children[childName];
				children.Remove(childName);
				child = null;
			}
		}

		/// <summary>
		/// Delete and detach specified child.
		/// </summary>
		/// <param name="node">The child</param>
		public void RemoveChild(Node node)
		{
			if(children.ContainsValue(node))
			{
				children.Remove(node.name); //TODO: performance
				node = null;
			}
		}

		public bool Equals(Node obj)
		{
			return name == obj.name;
		}

		public void MoveLocal(Vector3 delta)
		{
			position += RotateVector(delta, derivedOrientation);
		}

		public void MoveLocal(float x, float y, float z)
		{
			MoveLocal(new Vector3(x, y, z));
		}

		public void Rotate(Vector3 axis, float angle)
		{
			Quaternion q = Quaternion.FromAxisAngle(axis, angle);
			Rotate(q);
		}

		public void Rotate(Quaternion quaternion)
		{
			orientation.Normalize(); //Fix drift
			orientation = orientation * quaternion;
		}

		public void Yaw(float angle)
		{
			Rotate(Vector3.UnitY, angle);
		}

		public void Pitch(float angle)
		{
			Rotate(Vector3.UnitX, angle);
		}

		public void Roll(float angle)
		{
			Rotate(Vector3.UnitZ, angle);
		}

		public void ResetOrientation()
		{
			orientation = Quaternion.Identity;
		}

		public void ApplyForce(Vector3 origin, Vector3 force, float time) //TODO: impulse
		{
			velocity += 100f * time * force / derivedMass;

			Vector3 rotationAngle = Vector3.Cross(origin, force);
			float rotationalAcceleration = Vector3.Cross(origin * 0.01f, force).Length / DeriveInertia(rotationAngle) / rotationalVelocityScale;
			rotationalVelocity *= Quaternion.FromAxisAngle(rotationAngle, rotationalAcceleration * time);
			rotationalVelocity.Normalize();
		}

		public void ApplyForce(Vector3 force, float time)
		{
			velocity += 100f * time * force / derivedMass;
		}

		public void ApplyLocalForce(Vector3 origin, Vector3 force, float time) //TODO: impulse
		{
			ApplyForce(RotateVector(origin, derivedOrientation), RotateVector(force, derivedOrientation), time);
		}

		public void ApplyLocalForce(Vector3 force, float time)
		{
			ApplyForce(RotateVector(force, derivedOrientation), time);
		}

		/// <summary>
		/// Move the node through space, without moving through time.
		/// </summary>
		/// <param name="newPos">New position</param>
		public void Teleport(Vector3 newPos)
		{
			prevRelativeToCam += newPos - position;
			position = newPos;
		}


		public static Vector3 directionUp, directionRight, directionFront, occlusionTop, occlusionRight, occlusionBottom, occlusionLeft,
			occlusionTAlt, occlusionRAlt, occlusionBAlt, occlusionLAlt;
		internal static bool directionsComputed = false;
		//private static int dominantOcclusionCoordinate;

		private bool IsOccluded()
		{
			//return false;
			Vector3 gramResult;
			Vector3 pos = derivedPosition + occlusionOffset - Camera.Instance.position;
			gramResult = GramSchmidt3(directionUp, directionRight, pos);
			if(Vector3.Dot(gramResult, directionFront) < -occlusionRadius)
			{
				return true;
			}
			/*gramResult = GramSchmidt3(occlusionTop, directionRight, pos);
			if(Vector3.Dot(gramResult, occlusionTAlt) < -occlusionRadius)
			{
				return true;
			}
			gramResult = GramSchmidt3(occlusionBottom, directionRight, pos);
			if(Vector3.Dot(gramResult, occlusionBAlt) < -occlusionRadius)
			{
				return true;
			}*/
			gramResult = GramSchmidt3(occlusionRight, directionUp, pos);
			if(Vector3.Dot(gramResult, occlusionRAlt) < -occlusionRadius)
			{
				return true;
			}
			gramResult = GramSchmidt3(occlusionLeft, directionUp, pos);
			if(Vector3.Dot(gramResult, occlusionLAlt) < -occlusionRadius)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Renders the Node (if possible) and all its children. Recursive call.
		/// </summary>
		public void StartRender(int pass)
		{
			if(isVisible)
			{
				if(occlude)
				{
					if(!directionsComputed)
					{
						Camera.Instance.GetDirections(out directionUp, out directionRight, out directionFront);
						float tan = (float)Math.Tan(Camera.Instance.Fov * 3.14159f / 360f);
						occlusionTop = Vector3.Normalize(directionFront + tan * directionUp); // TODOO: Normalize necessary?
						occlusionRight = Vector3.Normalize(directionFront + tan * directionRight * Camera.Instance.width / Camera.Instance.height); // TODOO: Normalize necessary?
						occlusionBottom = Vector3.Normalize(directionFront - tan * directionUp); // TODOO: Normalize necessary?
						occlusionLeft = Vector3.Normalize(directionFront - tan * directionRight * Camera.Instance.width / Camera.Instance.height); // TODOO: Normalize necessary?

						occlusionTAlt = Vector3.Cross(occlusionTop, directionRight);
						occlusionBAlt = Vector3.Cross(directionRight, occlusionBottom);
						occlusionLAlt = Vector3.Cross(occlusionLeft, directionUp);
						occlusionRAlt = Vector3.Cross(directionUp, occlusionRight);

						directionsComputed = true;
					}
					if(IsOccluded())
					{
						//Console.WriteLine(name + "occluded!");
						return;
					}
				}

				/*if(!ignoreDrawDistance && (Camera.Instance.position - derivedPosition).Length > RenderWindow.Instance.drawDistance)
				{
					
				}*/
				Render(pass);
				lock(children)
				{
					foreach(Node n in children.Values)
					{
						n.StartRender(pass);
					}
				}
			}
		}



		public virtual void Render(int pass)
		{
		}

		public static Vector3 RotateVector(Vector3 vector, Quaternion quaternion) // TODO: Should be conj(q) * v * q
		{
			Vector3 t = 2 * Vector3.Cross(quaternion.Xyz, vector);
			return vector + quaternion.W * t + Vector3.Cross(quaternion.Xyz, t);
		}

		private static Vector3 GramSchmidt3(Vector3 baseA, Vector3 baseB, Vector3 v)
		{
			v = v - GramSchmidtProject(baseA, v);
			return v - GramSchmidtProject(baseB, v);
		}

		private static Vector3 GramSchmidtProject(Vector3 basis, Vector3 toProj)
		{
			return Vector3.Dot(toProj, basis) / Vector3.Dot(basis, basis) * basis;
		}
	}

	public delegate void QueueEvent(Node node);

	/// <summary>
	/// QueueItem holds the method to be called, along with the time since the call.
	/// </summary>
	public class QueueItem
	{
		public QueueEvent method;
		public float age = 0f;

		public QueueItem(QueueEvent method)
		{
			this.method = method;
		}

		public bool Update(float localTimeSinceLastUpdate, float currentDistance)
		{
			age += localTimeSinceLastUpdate;
			return age * RenderWindow.Instance.c >= currentDistance;
		}
	}
}