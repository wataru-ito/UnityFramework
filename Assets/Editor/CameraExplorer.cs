using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

public class CameraExplorer : EditorWindow
{
	class Column
	{
		public readonly string name;
		public readonly GUIContent sepalatorContent;
		public float width;
		public Action<Rect,Camera> DrawField;

		public Column(string name, float width, Action<Rect,Camera> DrawField)
		{
			this.name = name;
			this.sepalatorContent = new GUIContent();
			this.width = width;
			this.DrawField = DrawField;
		}
	}

	const float kHeaderHeight = 28f;
	const float kItemHeight = 16f;
	const float kSepalatorWidth = 4;
	const float kItemPaddingX = 4;

	Vector2 m_scrollPosition;
	string m_searchString = string.Empty;

	Column[] m_columnList;
	Camera m_selected;

	string[] m_layerOptions;

	List<Camera> m_sleepCamera = new List<Camera>();


	//------------------------------------------------------
	// static function
	//------------------------------------------------------

	[MenuItem("Window/Camera Explorer")]
	public static CameraExplorer Open()
	{
		var win = GetWindow<CameraExplorer>();
		win.titleContent = new GUIContent("Camera Explorer");
		win.minSize = new Vector2(win.minSize.x, 150);
		win.Show();
		return win;
	}

	static GUIStyle GetStyle(string styleName)
	{
		return GUI.skin.FindStyle(styleName) ?? GUI.skin.label;
	}

	//------------------------------------------------------
	// unity system function
	//------------------------------------------------------

	void OnEnable()
	{	
		m_columnList = new Column[]
		{
			new Column("Name", 120f, NameField),
			new Column("On", 26f, EnabledField),
			new Column("Depth", 60f, DepthField),
			new Column("Culling Mask", 120f, CullingMaskField),
			new Column("Clear Flags", 200f, ClearFlagsField),
		};
	}

	void OnGUI()
	{
		// Update Parameter
		{
			m_layerOptions = Enumerable.Range(0,32)
				.Select(i => LayerMask.LayerToName(i))
				.ToArray();

			m_sleepCamera.RemoveAll(i => i.enabled);
		}
		
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Space(12);
			using (new EditorGUILayout.VerticalScope())
			{
				DrawSearchBar();

				GUILayout.Box(GUIContent.none, 
					GUILayout.ExpandWidth(true),
					GUILayout.ExpandHeight(true));
				
				DrawCameraList(GUILayoutUtility.GetLastRect());
			}
			GUILayout.Space(12);
		}
	}

	void DrawSearchBar()
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Space(position.width * 0.5f);
			m_searchString = GUILayout.TextField(m_searchString, GetStyle("SearchTextField"));
			if (GUILayout.Button(GUIContent.none, GetStyle("SearchCancelButton")))
			{
				m_searchString = string.Empty;
				GUI.FocusControl(null);
			}
		}
	}

	//------------------------------------------------------
	// camera list
	//------------------------------------------------------

	void DrawCameraList(Rect r)
	{
		r = DrawHeader(r);

		var viewRect = new Rect(0, 0, GetListWidth(), GetListHeight());
		
		m_scrollPosition = GUI.BeginScrollView(r, m_scrollPosition, viewRect);
		{
			var itemPosition = new Rect(0, 0, viewRect.width, kItemHeight);

			var cameraList = GetCameraList();
			foreach (var camera in cameraList)
			{
				itemPosition = DrawCameraField(itemPosition, camera);
			}
			m_sleepCamera = cameraList.FindAll(i => !i.enabled);
		}
		GUI.EndScrollView();
	}

	List<Camera>　GetCameraList()
	{
		var cameraList = new List<Camera>(Camera.allCameras);
		cameraList.AddRange(m_sleepCamera);

		if (!string.IsNullOrEmpty(m_searchString))
		{
			cameraList.RemoveAll(i => !i.name.Contains(m_searchString));
		}

		return cameraList;
	}

	float GetListWidth()
	{
		float width = kItemPaddingX * 2f;
		foreach (var column in m_columnList)
		{
			width += column.width + kSepalatorWidth;
		}
		return width;
	}

	float GetListHeight()
	{
		return m_columnList.Length * kItemHeight;
	}

	Rect DrawHeader(Rect area)
	{
		var position = new Rect(area.x, area.y, area.width, kHeaderHeight);
		var viewRect = new Rect(0, 0, area.width, kHeaderHeight);

		GUI.Box(position, GUIContent.none);
		GUI.BeginScrollView(position, m_scrollPosition, viewRect);
		{
			var r = new Rect(
				kItemPaddingX - m_scrollPosition.x, 
				kHeaderHeight - kItemHeight - 2,
				0,
				kItemHeight);

			foreach (var column in m_columnList)
			{
				r.width = column.width;
				EditorGUI.LabelField(r, column.name);
				r.x += r.width;

				r = DrawColumSeparator(r, column);
			}
		}
		GUI.EndScrollView();

		area.y += kHeaderHeight+2;
		area.height -= kHeaderHeight;
		return area;
	}

	Rect DrawColumSeparator(Rect r, Column column)
	{
		EditorGUI.LabelField(
			new Rect(
				r.x,
				r.y - 6,
				kSepalatorWidth,
				r.height + 4), 
			column.sepalatorContent, 
			GetStyle("DopesheetBackground"));

		r.x += kSepalatorWidth;
		return r;
	}

	Rect DrawCameraField(Rect itemPosition, Camera camera)
	{
		// 現状ベースとなるToggleの上に乗っているコントロールが選択されない…。
		// どうすればいい？

		if (GUI.Toggle(itemPosition, 
			m_selected == camera, 
			GUIContent.none,
			GetStyle("PlayerSettingsLevel"))
			&& m_selected != camera)
		{
			m_selected = camera;
			Selection.activeGameObject = camera.gameObject;
			GUI.FocusControl(string.Empty);
		}

		var r = itemPosition;
		r.x += kItemPaddingX;

		foreach (var column in m_columnList)
		{
			r.width = column.width;
			column.DrawField(r, camera);
			r.x += r.width;

			r.x += kSepalatorWidth;
		}

		itemPosition.y += r.height;
		return itemPosition;
	}

	//------------------------------------------------------
	// camera column field
	//------------------------------------------------------

	void NameField(Rect r, Camera camera)
	{
		EditorGUI.LabelField(r, camera.name);
	}

	void EnabledField(Rect r, Camera camera)
	{
		camera.enabled = EditorGUI.Toggle(r, camera.enabled);
	}

	void DepthField(Rect r, Camera camera)
	{
		camera.depth = EditorGUI.FloatField(r, camera.depth);
	}

	void CullingMaskField(Rect r, Camera camera)
	{
		camera.cullingMask = EditorGUI.MaskField(r, GUIContent.none, camera.cullingMask, m_layerOptions);
	}

	void ClearFlagsField(Rect r, Camera camera)
	{
		camera.clearFlags = (CameraClearFlags)EditorGUI.EnumPopup(r, camera.clearFlags);
	}
}
