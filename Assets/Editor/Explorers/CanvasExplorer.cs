using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

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

	Vector2 m_scrollPosition;
	Rect m_scrollRect;
	string m_searchString = string.Empty;
	int m_renderMode;

	Column[] m_columnList;

	readonly string[] m_renderModeOptions =
	{
		"ScreenSpaceOverlay",
		"ScreenSpaceCamera",
		"WorldSpace"
	};
	string[] m_layerOptions;

	List<Canvas> m_canvasList;

	GUIStyle m_labelStyle;


	//------------------------------------------------------
	// static function
	//------------------------------------------------------

	[MenuItem("Window/Canvas Explorer")]
	public static CanvasExplorer Open()
	{
		var win = GetWindow<CanvasExplorer>();
		win.titleContent = new GUIContent("Canvas Explorer");
		win.minSize = new Vector2(win.minSize.x, 150);
		return win;
	}

	static GUIStyle GetStyle(string styleName)
	{
		return GUI.skin.FindStyle(styleName);
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
			new Column("Camera", 100f, CameraField),
			new Column("Sorting Layer", 100f, SortingLayerField),
			new Column("Order in Layer", 100f, SortingOrderField),
		};
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
		if (m_labelStyle == null)
		{
			var template = 	GUI.skin.FindStyle("Hi Label");
			if (template == null)
			{
				EditorGUILayout.HelpBox("GUIStyle not found in BuiltInSkin.¥nWait to vertion up.", MessageType.Warning);
				return;
			}

			m_labelStyle = new GUIStyle(template);
			m_labelStyle.padding.left = 0;
		}

		// Update Parameter
		{
			m_layerOptions = Enumerable.Range(0,32)
				.Select(i => LayerMask.LayerToName(i))
				.ToArray();
		}

		UpdateCanvasList();
		
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Space(12);
			using (new EditorGUILayout.VerticalScope())
			{
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

	void DrawToolbar()
	{
		m_renderMode = GUILayout.Toolbar(m_renderMode, m_renderModeOptions);
		
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

	void UpdateCanvasList()
	{
		var renderMode = RenderMode.ScreenSpaceOverlay;
		switch (m_renderMode)
		{
			case 1: renderMode = RenderMode.ScreenSpaceCamera; break;
			case 2: renderMode = RenderMode.WorldSpace; break;
		}

		var tmp = new List<Canvas>(Resources.FindObjectsOfTypeAll<Canvas>().Where(i => i.renderMode == renderMode));
		if (!string.IsNullOrEmpty(m_searchString))
		{
			tmp.RemoveAll(i => !i.name.Contains(m_searchString));
		}

		m_canvasList = tmp;
	}
	
	static int CanvasCompareTo(Canvas x, Canvas y)
	{
		var result = x.sortingOrder.CompareTo(y.sortingOrder);
		return result != 0 ? result : x.name.CompareTo(y.name);
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

		// background
		// アイテムが少なくても全域に表示させる必要があるのでアイテム描画と分けている
		// > スクロールしてると背景と情報表示がずれる…
		{
			var prev = GUI.color;
			var gray = new Color(0.95f, 0.95f, 0.95f);
			float y = m_scrollRect.yMin - m_scrollPosition.y;
			for (int i = 0; y < m_scrollRect.yMax; ++i, y+=kItemHeight)
			{
				if (y+kItemHeight <= m_scrollRect.yMin) continue;
				if (y >= m_scrollRect.yMax) continue;
				
				var itemPisition = new Rect(m_scrollRect.x, 
					Mathf.Max(y, m_scrollRect.y), 
					m_scrollRect.width,
					Mathf.Min(kItemHeight, m_scrollRect.yMax-y));
				
				GUI.color = i%2 == 1 ? prev : gray;
				GUI.Box(itemPisition, GUIContent.none, "CN EntryBackOdd");
			}
			GUI.color = prev;
		}

		// Canvass
		{
			if (m_canvasList.Count == 0)
			{
				ShowNotification(new GUIContent("Canvas not exists."));
			}
			else
			{
				RemoveNotification();

				var viewRect = new Rect(0, 0, GetListWidth(), m_canvasList.Count * kItemHeight);
				m_scrollPosition = GUI.BeginScrollView(m_scrollRect, m_scrollPosition, viewRect);
				{
					var itemPosition = new Rect(0, 0, Mathf.Max(viewRect.width, m_scrollRect.width), kItemHeight);
					foreach (var canvas in m_canvasList)
					{
						itemPosition = DrawCanvasField(itemPosition, canvas);
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
		EditorGUI.LabelField(r, canvas.sortingLayerName);
	}

	void SortingOrderField(Rect r, Canvas canvas)
	{
		canvas.sortingOrder = EditorGUI.IntField(r, canvas.sortingOrder);
	}
}
