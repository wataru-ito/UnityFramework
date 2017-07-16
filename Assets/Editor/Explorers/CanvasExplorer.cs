﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class CanvasExplorer : EditorWindow
{
	class Column
	{
		public readonly string name;
		public readonly GUIContent sepalatorContent;
		public float width;
		public Action<Rect,Canvas> DrawField;

		public Column(string name, float width, Action<Rect,Canvas> DrawField)
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

	readonly string[] m_renderModeOptions =
	{
		"ScreenSpaceOverlay",
		"ScreenSpaceCamera",
		"WorldSpace",
	};

	Column[] m_columnList;
	string[] m_sortingLayerNames;
	int[] m_sortingLayerUniquIDs;
	RenderMode m_renderMode;
	string m_searchString = string.Empty;
	List<Canvas> m_canvasList;
	bool m_lockList;

	GUISkin m_skin;
	GUIStyle m_labelStyle;
	Vector2 m_scrollPosition;
	Rect m_scrollRect;


	//------------------------------------------------------
	// static function
	//------------------------------------------------------

	[MenuItem("Window/Canvas Explorer")]
	public static CanvasExplorer Open()
	{
		return GetWindow<CanvasExplorer>();
	}


	//------------------------------------------------------
	// unity system function
	//------------------------------------------------------

	void OnEnable()
	{
		titleContent = new GUIContent("Canvas Explorer");
		minSize = new Vector2(minSize.x, 150);
		InitGUI();
	}

	void OnFocus()
	{
		GUI.FocusControl(string.Empty);
	}

	void OnSelectionChange()
	{
		Repaint();
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
		// 表示しながらレイヤーを編集している可能性も考慮して毎回更新する
		m_sortingLayerNames = GetSortingLayerNames();
		m_sortingLayerUniquIDs = GetSortingLayerUniqueIDs();

		if (m_canvasList == null || !m_lockList)
			m_canvasList = GetCanvasList();
		else
			m_canvasList.RemoveAll(i => i == null);
		
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Space(12);
			using (new EditorGUILayout.VerticalScope())
			{
                GUILayout.Space(8);
				DrawToolbar();
				DrawSearchBar();
				DrawCanvasList();
				GUILayout.Space(4);
			}
			GUILayout.Space(12);
		}

		EventProcedure();
	}


	//------------------------------------------------------
	// canvas list
	//------------------------------------------------------

	List<Canvas> GetCanvasList()
	{
		var tmp = new List<Canvas>(Resources.FindObjectsOfTypeAll<Canvas>().Where(i => i.renderMode == m_renderMode));
		if (!string.IsNullOrEmpty(m_searchString))
		{
			tmp.RemoveAll(i => !i.name.Contains(m_searchString));
		}

		tmp.Sort(CanvasCompareTo);

		return tmp;
	}

	int CanvasCompareTo(Canvas x, Canvas y)
	{
		int result = 0;

		switch (m_renderMode)
		{
			case RenderMode.ScreenSpaceCamera:
			case RenderMode.WorldSpace:
				result = GetCameraDepth(x).CompareTo(GetCameraDepth(y));
				if (result != 0) return result;
				break;
		}

		result = Array.IndexOf(m_sortingLayerUniquIDs, x.sortingLayerID).CompareTo(Array.IndexOf(m_sortingLayerUniquIDs, y.sortingLayerID));
		if (result != 0) return result;

		result = x.sortingOrder.CompareTo(y.sortingOrder);
		if (result != 0) return result;
	
		return x.name.CompareTo(y.name);
	}

	static float GetCameraDepth(Canvas canvas)
	{
		return canvas.worldCamera ? canvas.worldCamera.depth : 0f;
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
					OnCanvasSelected(Event.current);
					Repaint();
				}
				break;
		}
	}

	void OnCanvasSelected(Event ev)
	{
		var index = Mathf.FloorToInt((ev.mousePosition.y - m_scrollRect.y + m_scrollPosition.y) / kItemHeight);
		if (index >= m_canvasList.Count)
		{
			Selection.activeGameObject = null;
			return;
		}

		if (IsSelectionAdditive(ev))
		{
			var targetGO = m_canvasList[index].gameObject;
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
			var firstCanvas = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Canvas>() : null;
			var firstIndex = m_canvasList.IndexOf(firstCanvas);
			if (firstIndex >= 0 && index != firstIndex)
			{
				var diff = index-firstIndex;
				var objects = new UnityEngine.Object[Mathf.Abs(diff)+1];
				var step = diff > 0 ? 1 : -1;
				for (int i = 0; i < objects.Length; ++i, firstIndex+=step)
				{
					objects[i] = m_canvasList[firstIndex].gameObject;
				}						
				Selection.objects = objects;
				return;
			}
		}
		
		Selection.activeGameObject = m_canvasList[index].gameObject;
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

	void InitGUI()
	{
		m_columnList = new Column[]
		{
			new Column("Name", 120f, NameField),
			new Column("On", 26f, EnabledField),
			new Column("Camera", 100f, CameraField),
			new Column("Sorting Layer", 100f, SortingLayerField),
			new Column("Order in Layer", 100f, SortingOrderField),
		};

		var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
		m_skin = AssetDatabase.LoadAssetAtPath<GUISkin>(
			string.Format("{0}/CanvasExplorer.guiskin", Path.GetDirectoryName(scriptPath)));
		m_labelStyle = m_skin.FindStyle("Hi Label");
	}

	void DrawToolbar()
	{
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(30);
			m_renderMode = (RenderMode)GUILayout.Toolbar((int)m_renderMode, m_renderModeOptions, GUILayout.Height(24));
			GUILayout.Space(30);
		}
		
	}
	void DrawSearchBar()
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			m_lockList = GUILayout.Toggle(m_lockList, "Lock List");

			m_searchString = GUILayout.TextField(m_searchString, "SearchTextField", GUILayout.Width(300));
			if (GUILayout.Button(GUIContent.none, "SearchCancelButton"))
			{
				m_searchString = string.Empty;
				GUI.FocusControl(null);
			}
		}
	}

	void DrawCanvasList()
	{
		GUILayout.Box(GUIContent.none, 
			GUILayout.ExpandWidth(true),
			GUILayout.ExpandHeight(true));
		
		var r = GUILayoutUtility.GetLastRect();
		r = DrawHeader(r);

		// この後描画されるbackgrounで枠線が消えてしまうので削る
		r.x += 1f;
		r.width -= 2f;
		r.height -= 1f;
		m_scrollRect = r;

		// アイテムが少なくても全域に表示させる必要があるのでアイテム描画と分けている
		// > スクロールしてると背景と情報表示がずれる…
		DrawBackground();

		var viewRect = new Rect(0, 0, GetListWidth(), m_canvasList.Count * kItemHeight);
		using (var scroll = new GUI.ScrollViewScope(m_scrollRect, m_scrollPosition, viewRect))
		{
            m_scrollPosition = scroll.scrollPosition;

			var itemPosition = new Rect(0, 0, Mathf.Max(viewRect.width, m_scrollRect.width), kItemHeight);
			foreach (var canvas in m_canvasList)
			{
				itemPosition = DrawCanvasField(itemPosition, canvas);
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
			"DopesheetBackground");

		r.x += kSepalatorWidth;
		return r;
	}

	void DrawBackground()
	{
		var prev = GUI.color;
		var gray = new Color(0.95f, 0.95f, 0.95f);
		float y = m_scrollRect.yMin - m_scrollPosition.y;
		for (int i = 0; y < m_scrollRect.yMax; ++i, y += kItemHeight)
		{
			if (y + kItemHeight <= m_scrollRect.yMin) continue;
			if (y >= m_scrollRect.yMax) continue;

			var itemPisition = new Rect(m_scrollRect.x,
				Mathf.Max(y, m_scrollRect.y),
				m_scrollRect.width,
				Mathf.Min(kItemHeight, m_scrollRect.yMax - y));

			GUI.color = i % 2 == 1 ? prev : gray;
			GUI.Box(itemPisition, GUIContent.none, "CN EntryBackOdd");
		}
		GUI.color = prev;
	}

	Rect DrawCanvasField(Rect itemPosition, Canvas Canvas)
	{		
		var styleState = GetStyleState(Selection.gameObjects.Contains(Canvas.gameObject));
		if (styleState.background)
			GUI.DrawTexture(itemPosition, styleState.background);

		var r = itemPosition;
		r.x += kItemPaddingX;
		foreach (var column in m_columnList)
		{
			r.width = column.width;
			column.DrawField(r, Canvas);
			r.x += (r.width + kSepalatorWidth);
		}

		itemPosition.y += r.height;
		return itemPosition;
	}

	GUIStyleState GetStyleState(bool selected)
	{
		if (selected)
			return EditorWindow.focusedWindow == this ? m_labelStyle.onActive : m_labelStyle.onNormal;
		return m_labelStyle.normal;
	}


	//------------------------------------------------------
	// Canvas column field
	//------------------------------------------------------

	void NameField(Rect r, Canvas canvas)
	{
		EditorGUI.LabelField(r, canvas.name, m_labelStyle);
	}

	void EnabledField(Rect r, Canvas canvas)
	{
		canvas.enabled = EditorGUI.Toggle(r, canvas.enabled);
	}

	void CameraField(Rect r, Canvas canvas)
	{
		canvas.worldCamera = EditorGUI.ObjectField(r, canvas.worldCamera, typeof(Camera), true) as Camera;
	}

	void SortingLayerField(Rect r, Canvas canvas)
	{
		canvas.sortingLayerID = EditorGUI.IntPopup(r, canvas.sortingLayerID, 
			m_sortingLayerNames, 
			m_sortingLayerUniquIDs);
	}

	void SortingOrderField(Rect r, Canvas canvas)
	{
		canvas.sortingOrder = EditorGUI.IntField(r, canvas.sortingOrder);
	}


	//------------------------------------------------------
	// unity internals
	//------------------------------------------------------

	static string[] GetSortingLayerNames()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
	}

	static int[] GetSortingLayerUniqueIDs()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
	}
}
