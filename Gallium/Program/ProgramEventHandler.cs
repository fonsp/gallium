using System;
using System.Globalization;
using System.Threading;
using GraphicsLibrary;
using GraphicsLibrary.Core;
using OpenTK;
using OpenTK.Graphics;

namespace Gallium.Program
{
	public partial class Game
	{
		public override bool ExecuteCommand(string command)
		{
			string[] inputArray = command.Split(new char[] { ' ' });

			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			if(inputArray.Length > 0 && !String.IsNullOrEmpty(inputArray[0]))
			{
				switch(inputArray[0].ToLower())
				{
					case "set":
						if(inputArray.Length > 1 && !String.IsNullOrEmpty(inputArray[1]))
						{
							try
							{
								switch(inputArray[1].ToLower())
								{
									case "walkspeed":
										if(inputArray.Length > 2 && !String.IsNullOrEmpty(inputArray[2]))
										{
											walkSpeed = (int)Convert.ToDouble(inputArray[2]);
											Debug.WriteLine("walkSpeed was set to " + walkSpeed, Color4.Orange);
											return true;
										}
										break;
									case "mouse":
										if(inputArray.Length > 2 && !String.IsNullOrEmpty(inputArray[2]))
										{
											mouseSensitivityFactor = (float)Convert.ToDouble(inputArray[2]);
											Debug.WriteLine("mouse sensitivity was set to " + mouseSensitivityFactor, Color4.Orange);
											return true;
										}
										break;
									case "showskybox":
										if(inputArray.Length > 2 && !String.IsNullOrEmpty(inputArray[2]))
										{
											try
											{
												RootNode.Instance.GetChild("skybox").isVisible = bool.Parse(inputArray[2]);
											}
											catch(Exception exception)
											{
											}
											Debug.WriteLine("Skybox visibilty was set to " + RootNode.Instance.GetChild("skybox").isVisible, Color4.Orange);
											return true;
										}
										break;
								}
							}
							catch(Exception exception)
							{
							}
						}
						break;
					case "reset":
						if(inputArray.Length > 1 && !String.IsNullOrEmpty(inputArray[1]))
						{
							switch(inputArray[1].ToLower())
							{
								case "walkspeed":
									walkSpeed = 400;
									Debug.WriteLine("walkSpeed was reset to " + walkSpeed, Color4.Orange);
									return true;
									break;
								case "mouse":
									mouseSensitivityFactor = 1f;
									Debug.WriteLine("mouse sensitivity was reset to " + mouseSensitivityFactor, Color4.Orange);
									return true;
									break;
								case "showskybox":
									RootNode.Instance.GetChild("skybox").isVisible = true;
									Debug.WriteLine("Skybox visibilty was reset to " + RootNode.Instance.GetChild("skybox").isVisible, Color4.Orange);
									return true;
									break;
							}
						}
						break;
					case "get":
						if(inputArray.Length > 1 && !String.IsNullOrEmpty(inputArray[1]))
						{
							switch(inputArray[1].ToLower())
							{
								case "walkspeed":
									Debug.WriteLine("walkSpeed = " + walkSpeed, Color4.Orange);
									return true;
									break;
								case "mouse":
									Debug.WriteLine("mouse = " + mouseSensitivityFactor, Color4.Orange);
									return true;
									break;
								case "fov":
									Debug.WriteLine("fov = " + Camera.Instance.Fov, Color4.Orange);
									return true;
									break;
								case "showkybox":
									Debug.WriteLine("showSkybox = " + RootNode.Instance.GetChild("skybox").isVisible, Color4.Orange);
									return true;
									break;
								default:
									Debug.WriteLine("Usage: get [timeMult|walkSpeed|VSync|mouse|c|fov|showSkybox]", Color4.LightBlue);
									return true;
									break;
							}
						}
						break;
					case "reload":
						config.Reload();
						fpsCam = Vector2.Zero;
						walkSpeed = 400;
						break;
					case "list":
					case "help":
						Debug.WriteLine("Available commands: stop set get reset reload list clear", Color4.LightBlue);
						break;

				}
			}

			return false;
		}
	}
}