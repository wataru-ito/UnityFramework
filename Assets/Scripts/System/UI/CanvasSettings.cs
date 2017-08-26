using UnityEngine;

namespace Framework.UI
{
	/// <summary>
	/// Canvasよりも先に実行されること
	/// </summary>
	[RequireComponent(typeof(Canvas)), ExecuteInEditMode]
	public class CanvasSettings : MonoBehaviour
	{
		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void Awake()
		{
			var canvas = GetComponent<Canvas>();
			canvas.worldCamera = GetCanvasCamera();
		}

		Camera GetCanvasCamera()
		{
			var mask = 1 << gameObject.layer;
			return System.Array.Find(Camera.allCameras, i => (i.cullingMask & mask) != 0);
		}
	}
}