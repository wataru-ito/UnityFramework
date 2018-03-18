using System;

namespace SceneWindow
{
	public class OpenAttribute : Attribute
	{
		public readonly string[] sceneNames;

		public OpenAttribute(params string[] sceneNames)
		{
			this.sceneNames = sceneNames;
		}
	}
}