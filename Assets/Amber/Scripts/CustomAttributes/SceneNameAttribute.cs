using UnityEngine;

namespace Amber
{
	public class SceneNameAttribute : PropertyAttribute
	{
		public bool findFromAssetDatabase;

		public SceneNameAttribute(bool findFromAssetDatabase = false)
		{
			this.findFromAssetDatabase = findFromAssetDatabase;
		}
	}
}