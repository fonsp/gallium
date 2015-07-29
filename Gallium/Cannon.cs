using GraphicsLibrary.Core;
using OpenTK;

namespace Gallium
{
	public class Cannon:Entity
	{
		public Vector3 newVelocity = new Vector3(-1000f, 2000f, -900f);
		public Cannon(string name) : base(name)
		{
		}

		public override void Update(float timeSinceLastUpdate)
		{
			if((derivedPosition - Camera.Instance.position).Length < ActionTrigger.maxDistance)
			{
				ActionTrigger.Display("jump");
				if(ActionTrigger.onActive)
				{
					Camera.Instance.velocity = newVelocity;
				}
			}
		}
	}
}
