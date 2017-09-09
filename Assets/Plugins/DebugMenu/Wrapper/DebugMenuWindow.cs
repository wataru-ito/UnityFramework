#if DEBUG
using UnityEngine;
using UnityEngine.UI;

namespace DebugMenuSystem
{
	/// <summary>
	/// OnGUI()の描画は存在するだけでパフォーマンスを食うので
	/// DebugMenuの描画はコンポーネントの生成/破棄で管理する
	/// </summary>
	public class DebugMenuWindow : MonoBehaviour
	{
		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		public static DebugMenuWindow Create()
		{
			var go = new GameObject("DebugMenuWindow", 
				typeof(DebugMenuWindow),
				typeof(Canvas),
				typeof(GraphicRaycaster));
			go.hideFlags |= HideFlags.DontSave;
			DontDestroyOnLoad(go);

			// UIのタッチイベントを塞ぐ
			var canvas = go.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = short.MaxValue;

			var touchGuard = new GameObject("TouchGuard", typeof(Image));

			var rectTrans = touchGuard.GetComponent<RectTransform>();
			rectTrans.SetParent(go.transform);
			rectTrans.anchorMin = Vector2.zero;
			rectTrans.anchorMax = Vector2.one;
			rectTrans.anchoredPosition = Vector2.zero;
			rectTrans.sizeDelta = Vector2.zero;

			var image = touchGuard.GetComponent<Image>();
			image.color = Color.clear;

			return go.GetComponent<DebugMenuWindow>();
		}


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void OnGUI()
		{
			DebugMenuManager.OnGUI(Close);
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public void Close()
		{
			Destroy(gameObject);
		}
	}
}
#endif