using UnityEngine;

namespace Amber
{
	public static class ExtentionsMethods
	{
		//------------------------------------------------------
		// GameObject
		//------------------------------------------------------

		public static void SetLayerRecursively(this GameObject go, int layer)
		{
			go.layer = layer;
			foreach (Transform t in go.transform)
			{
				SetLayerRecursively(t.gameObject, layer);
			}
		}
	}
}