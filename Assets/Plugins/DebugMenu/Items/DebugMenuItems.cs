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
				if (GUILayout.Button("実行", GUILayout.Width(Screen.width * 0.35f)))
				{
					m_action();
				}
			}
		}
	}

	/// <summary>
	/// Toggleボタンが小さくなるから別途用意する
	/// </summary>
	public class DebugMenuToggle : DebugMenuItem
	{
		string[] m_labels; // 0:OFF 1:ON
		Func<bool> m_getter;
		Action<bool> m_setter;

		public DebugMenuToggle(string path, Func<bool> getter, Action<bool> setter, params string[] labels)
			: base(path)
		{
			m_labels = new string[2]
			{
				labels.Length > 0 ? labels[0] : "OFF",
				labels.Length > 1 ? labels[1] : "ON",
			};
			m_getter = getter;
			m_setter = setter;
		}

		public override void OnGUI()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(name);

				var flag = m_getter();
				if (GUILayout.Button(m_labels[flag ? 1 : 0], GUILayout.Width(Screen.width * 0.35f)))
				{
					m_setter(!flag);
				}
			}
		}
	}

}
#endif