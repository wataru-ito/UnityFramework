﻿using UnityEngine;
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
	Rect m_scrollRect;
	string m_searchString = string.Empty;

	Column[] m_columnList;

	string[] m_layerOptions;

	List<Camera> m_cameraList;

	Texture m_selectedImage;
	Texture m_unselectedImage;


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

		m_selectedImage = EditorGUIUtility.LoadRequired("selected.png") as Texture;
		m_unselectedImage = EditorGUIUtility.LoadRequired("unselected.png") as Texture;
	}

	void OnFocus()
	{
	}

	void OnLostFocus()
	{
	}

	void OnInspectorUpdate()
	{
		if (EditorApplication.isPlaying)
		{
			Repaint();
		}
	}

	void OnGUI()
	{
		// Update Parameter
		{
			m_layerOptions = Enumerable.Range(0,32)
				.Select(i => LayerMask.LayerToName(i))
				.ToArray();
		}

		UpdateCameraList();
		
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Space(12);
			using (new EditorGUILayout.VerticalScope())
			{
				DrawSearchBar();
				DrawCameraList();
			}
			GUILayout.Space(12);
		}

		EventProcedure();
	}


	//------------------------------------------------------
	// events
	//------------------------------------------------------

	void EventProcedure()
	{
		switch (Event.current.type)
		{
			case EventType.MouseDown:
				if (m_scrollRect.Contains(Event.current.mousePosition))
				{
					OnCameraSelected(Event.current);
					Repaint();
				}
				break;
		}
	}

	void OnCameraSelected(Event ev)
	{
		var index = Mathf.FloorToInt((ev.mousePosition.y - m_scrollRect.y) / kItemHeight);
		if (index >= m_cameraList.Count)
		{
			Selection.activeGameObject = null;
			return;
		}

		if (IsSelectionAdditive(ev))
		{
			var targetGO = m_cameraList[index].gameObject;
			var gos = new List<GameObject>(Selection.gameObjects);
			if (gos.Contains(targetGO))
			{
				gos.Remove(targetGO);
				if (Selection.activeGameObject == targetGO)
				{
					Selection.activeGameObject = gos.Count > 0 ? gos[0] : null;
				}
			}
			else
			{
				gos.Add(targetGO);
			}
			Selection.objects = gos.ToArray();
			return;
		}
		else if (ev.shift)
		{
			var firstCamera = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Camera>() : null;
			var firstIndex = m_cameraList.IndexOf(firstCamera);
			if (firstIndex >= 0 && index != firstIndex)
			{
				var diff = index-firstIndex;
				var objects = new UnityEngine.Object[Mathf.Abs(diff)+1];
				var step = diff > 0 ? 1 : -1;
				for (int i = 0; i < objects.Length; ++i, firstIndex+=step)
				{
					objects[i] = m_cameraList[firstIndex].gameObject;
				}						
				Selection.objects = objects;
				return;
			}
		}
		
		Selection.activeGameObject = m_cameraList[index].gameObject;
	}

	bool IsSelectionAdditive(Event ev)
	{
		#if UNITY_EDITOR_OSX
		return ev.command;
		#else
		return ev.control;
		#endif
	}
	

	//------------------------------------------------------
	// gui
	//------------------------------------------------------

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

	void UpdateCameraList()
	{
		var tmp = new List<Camera>(Camera.allCameras);

		// ここで寝かせた奴はここで有効にしたいので追加しておく
		if (m_cameraList != null)
		{
			tmp.AddRange(m_cameraList.Where(i => !i.enabled));
			tmp.Sort((x,y) => x.depth.CompareTo(y.depth));
		}

		if (!string.IsNullOrEmpty(m_searchString))
		{
			tmp.RemoveAll(i => !i.name.Contains(m_searchString));
		}

		m_cameraList = tmp;
	}

	void DrawCameraList()
	{
		GUILayout.Box(GUIContent.none, 
			GUILayout.ExpandWidth(true),
			GUILayout.ExpandHeight(true));
		
		var r = GUILayoutUtility.GetLastRect();
		r = DrawHeader(r);

		// この後描画されるboxで枠線が消えてしまうので削る
		r.x += 1f;
		r.width -= 2f;
		m_scrollRect = r;

		// background
		// アイテムが少なくても全域に表示させる必要があるのでアイテム描画と分けている
		// > スクロールしてると背景と情報表示がずれる…
		{
			var prev = GUI.color;
			var gray = new Color(0.95f, 0.95f, 0.95f);

			float y = r.y - m_scrollPosition.y;
			for (int i = 0; y < r.y+r.height; ++i, y+=kItemHeight)
			{
				if (y + kItemHeight < r.y) continue;
				GUI.color = i%2 == 1 ? prev : gray;
				var diff = Mathf.Max(0, r.y - y);
				GUI.Box(new Rect(r.x, y+diff, r.width, kItemHeight-diff), GUIContent.none, "CN EntryBackOdd");
			}
			GUI.color = prev;
		}

		// cameras
		{
			if (m_cameraList.Count == 0)
			{
				ShowNotification(new GUIContent("Camera not exists."));
			}
			else
			{
				RemoveNotification();

				var viewRect = new Rect(0, 0, GetListWidth(), m_cameraList.Count * kItemHeight);
				m_scrollPosition = GUI.BeginScrollView(m_scrollRect, m_scrollPosition, viewRect);
				{
					var itemPosition = new Rect(0, 0, viewRect.width, kItemHeight);
					foreach (var camera in m_cameraList)
					{
						itemPosition = DrawCameraField(itemPosition, camera);
					}
				}
				GUI.EndScrollView();
			}
		}
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

		area.y += kHeaderHeight;
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
		// 選択時の背景色
		// > 選択されてる時、文字は白にしないといけない。やっぱりGUIStyleでやってるんだろうか？
		if (Selection.gameObjects.Contains(camera.gameObject))
		{
			var prev = GUI.color;
			GUI.color = Color.white;
			bool focused = EditorWindow.focusedWindow == this;
			GUI.DrawTexture(itemPosition, 
				focused ? m_selectedImage : m_unselectedImage);
			GUI.color = prev;
		}
		GUI.enabled = true;

		var r = itemPosition;
		r.x += kItemPaddingX;
		foreach (var column in m_columnList)
		{
			r.width = column.width;
			column.DrawField(r, camera);
			r.x += (r.width + kSepalatorWidth);
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