using System;
using GraphicsLibrary;

namespace Gallium.Program
{
	public partial class Game:GraphicsProgram
	{
		public Game(string[] arguments, bool enableLogging, string logFilename)
			: base(arguments, enableLogging, logFilename)
		{

		}

		public Game(string[] arguments)
			: this(arguments, true, "game.log")
		{

		}

		[STAThread]
		static void Main(string[] args)
		{
			using(Game game = new Game(args))
			{
				game.Run();
			}

			Debug.WriteLine("Closing..");
		}
	}
}