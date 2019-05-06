using System;
using OpenTK.Graphics;

namespace GraphicsLibrary
{
	public static class Debug
	{
		public static void Write(string text, Color4 color, params object[] args)
		{
			string formatted = String.Format(text, args);

			System.Diagnostics.Debug.Write(formatted);
			RenderWindow.Instance.hudConsole.AddText(formatted, color);
		}

		public static void Write(string text, params object[] args)
		{
			Write(text, new Color4(.7f, .7f, .7f, 1.0f), args);
		}

		public static void WriteLine(string text, Color4 color, params object[] args)
		{
			string formatted = String.Format(text, args);

			System.Diagnostics.Debug.WriteLine(formatted);
			RenderWindow.Instance.hudConsole.AddText(formatted, color);
		}

		public static void WriteLine(string text, params object[] args)
		{
			WriteLine(text, new Color4(.7f, .7f, .7f, 1.0f), args);
		}
	}
}