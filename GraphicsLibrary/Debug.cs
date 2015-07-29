using System;
using OpenTK.Graphics;

namespace GraphicsLibrary
{
	public static class Debug
	{
		public static void Write(string text, Color4 color)
		{
			System.Diagnostics.Debug.Write(text);
			RenderWindow.Instance.hudConsole.AddText(text, color);
		}

		public static void Write(string text)
		{
			Write(text, new Color4(.7f, .7f, .7f, 1.0f));
		}

		public static void WriteLine(string text, Color4 color)
		{
			System.Diagnostics.Debug.WriteLine(text);
			RenderWindow.Instance.hudConsole.AddText(text, color);
		}

		public static void WriteLine(string text)
		{
			WriteLine(text, new Color4(.7f, .7f, .7f, 1.0f));
		}
	}
}