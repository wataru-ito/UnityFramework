#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DebugMenuSystem
{
	public static class DebugMenuManager
	{
		static DebugMenuDirectory s_root = new DebugMenuDirectory(string.Empty, null);
		static DebugMenuDirectory s_current = s_root;
		static GUISkin s_skin;

		//------------------------------------------------------
		// from directory/item
		//------------------------------------------------------

		internal static void SetCurrent(DebugMenuDirectory dir)
		{
			s_current = dir ?? s_root;
		}

		internal static DebugMenuDirectory GetDirectory(string path)
		{
			return string.IsNullOrEmpty(path) ?
				s_root : s_root.GetDirectory(ToStack(path));
		}

		static Stack<string> ToStack(string path)
		{
			var stack = new Stack<string>();
			while (!string.IsNullOrEmpty(path))
			{
				stack.Push(Path.GetFileName(path));
				path = Path.GetDirectoryName(path);
			}
			return stack;
		}


		//------------------------------------------------------
		// settings
		//------------------------------------------------------

		public static void SetSkin(GUISkin skin)
		{
			s_skin = skin;
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
			return string.IsNullOrEmpty(path) ? s_root : s_root.GetItem(ToStack(path));
		}

		public static void OnGUI(Action onClose = null)
		{
			GUI.skin = s_skin;

			const float kScrollBarWidth = 16f;
			using (new GUILayout.VerticalScope("box", GUILayout.Width(Screen.width - kScrollBarWidth), GUILayout.Height(Screen.height)))
			{
				using (new GUILayout.HorizontalScope())
				{
					if (s_current != s_root)
					{
						if (GUILayout.Button("戻る", GUILayout.Width(Screen.width * 0.5f)))
						{
							s_current = s_current.directory;
						}
					}

					if (GUILayout.Button("閉じる") && onClose != null)
					{
						onClose();
						return;
					}
				}

				s_current.OnGUI();
			}
		}
	}
}
#endif