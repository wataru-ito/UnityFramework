#if DEBUG
using UnityEngine;

namespace DebugMenuSystem
{
	/// <summary>
	/// OnGUI()の描画は存在するだけでパフォーマンスを食うので
	/// DebugMenuの描画はコンポーネントの生成/破棄で管理する
	/// </summary>
	public class DebugMenuWindow : MonoBehaviour
	{
		Object m_destroyTarget;

		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		public static DebugMenuWindow Open(GameObject owner = null)
		{
			if (owner)
			{
				var win = owner.AddComponent<DebugMenuWindow>();
				win.m_destroyTarget = win;
				return win;
			}
			else
			{
				var go = new GameObject("DebugMenuWindow");
				go.hideFlags |= HideFlags.DontSave;

				var win = go.AddComponent<DebugMenuWindow>();
				win.m_destroyTarget = go;
				return win;
			}
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
			Destroy(m_destroyTarget);
		}
	}
}
#endif