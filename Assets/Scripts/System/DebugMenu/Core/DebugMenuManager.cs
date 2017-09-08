#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DebugMenuSystem
{
	public static class DebugMenuManager
	{
		static DebugMenuDirectory m_root = new DebugMenuDirectory(string.Empty, null);
		static DebugMenuDirectory m_current = m_root;


		//------------------------------------------------------
		// from directory/item
		//------------------------------------------------------

		internal static void SetCurrent(DebugMenuDirectory dir)
		{
			m_current = dir ?? m_root;
		}

		internal static DebugMenuDirectory GetDirectory(string path)
		{
			return string.IsNullOrEmpty(path) ?
				m_root : m_root.GetDirectory(ToStack(path));
		}

		static Stack<string> ToStack(string path)
		{
			var stack = new Stack<string>();

			stack.Push(Path.GetFileName(path));

			var dir = Path.GetDirectoryName(path);
			while (!string.IsNullOrEmpty((dir)))
			{
				stack.Push(dir);
				dir = Path.GetDirectoryName(dir);
			}

			return stack;
		}

		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public static void Remove(string path)
		{
			var item = GetItem(path);
			if (item != null)
			{
				item.Remove();
			}
		}

		static DebugMenuItem GetItem(string path)
		{
			return string.IsNullOrEmpty(path) ? m_root : m_root.GetItem(ToStack(path));
		}

		public static void OnGUI(Action onClose = null)
		{
			const float kScrollBarWidth = 16f;
			using (new GUILayout.VerticalScope("box", GUILayout.Width(Screen.width - kScrollBarWidth), GUILayout.Height(Screen.height)))
			{
				using (new GUILayout.HorizontalScope())
				{
					if (m_current != m_root)
					{
						if (GUILayout.Button("戻る", GUILayout.Width(Screen.width * 0.5f)))
						{
							m_current = m_current.directory;
						}
					}

					if (GUILayout.Button("閉じる") && onClose != null)
					{
						onClose();
						return;
					}
				}

				m_current.OnGUI();
			}
		}
	}
}
#endif