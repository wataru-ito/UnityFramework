using UnityEditor;
using UnityEngine;

namespace ToolboxEditor
{
	public class FocusedWindowInfo : EditorWindow
	{
		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		[MenuItem("Window/Focused Window Info")]
		static void Open()
		{
			GetWindow<FocusedWindowInfo>();
		}


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void OnEnable()
		{
			titleContent = new GUIContent("FocusedWindowInfo");			
		}

		void OnGUI()
		{
			var win = focusedWindow;
			if (!win)
			{
				EditorGUILayout.LabelField("FocusWindow is null");
				return;
			}

			EditorGUILayout.LabelField("Class", win.GetType().FullName);
			EditorGUILayout.LabelField("Title", win.titleContent.text);
			EditorGUILayout.LabelField("position", string.Format("{0} x {1}", win.position.x, win.position.y));
			EditorGUILayout.LabelField("size", string.Format("{0} x {1}", win.position.width, win.position.height));
			EditorGUILayout.LabelField("minSize", win.minSize.ToString());
			EditorGUILayout.LabelField("maxSize", win.maxSize.ToString());	
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}
	}
}