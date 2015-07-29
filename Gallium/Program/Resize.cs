using System.Drawing;
using OpenTK;

namespace Gallium.Program
{
	public partial class Game
	{
		public override void Resize(Rectangle newDimensions)
		{
			crosshair.position = new Vector2(newDimensions.Width / 2 - 16, newDimensions.Height / 2 - 16);
			ActionTrigger.textField.position = new Vector2((newDimensions.Width - ActionTrigger.textField.sizeX * ActionTrigger.textField.text.Length) / 2, (newDimensions.Height - ActionTrigger.textField.sizeY) * 3 / 4);
		}
	}
}