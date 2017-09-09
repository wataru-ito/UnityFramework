#if DEBUG
using System;
using UnityEngine;

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

		public static DebugMenuWindow Create(GameObject prefab)
		{
			var go = Instantiate(prefab) as GameObject;
			go.hideFlags |= HideFlags.DontSave;

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