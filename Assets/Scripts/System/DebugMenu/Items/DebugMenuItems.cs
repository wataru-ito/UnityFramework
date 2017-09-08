#if DEBUG
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace DebugMenuSystem
{
	/// <summary>
	/// 好きなように表示できるデバッグメニューアイテム
	/// </summary>
	public class DebugMenu : DebugMenuItem
	{
		Action m_drawer;

		public DebugMenu(string path, Action drawer)
			: base(path)
		{
			Assert.IsNotNull(drawer);
			m_drawer = drawer;
		}

		public override void OnGUI()
		{
			GUILayout.Label(name, "box");
			m_drawer();
		}
	}


	/// <summary>
	/// 関数実行ボタン
	/// </summary>
	public class DebugMenuButton : DebugMenuItem
	{
		Action m_action;

		public DebugMenuButton(string path, Action action)
			: base(path)
		{
			Assert.IsNotNull(action);
			m_action = action;
		}

		public override void OnGUI()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(name);
				if (GUILayout.Button("実行", GUILayout.Width(Screen.width * 0.5f)))
				{
					m_action();
				}
			}
		}
	}
}
#endif