using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BuiltinResourceView : EditorWindow
{
	enum TextureViewType
	{
		RGB,
		Transparent,
		Alpha,
	}


	const float kItemSizeMin = 64f;
	const float kItemSizeMax = 128f;
	const float kScrollBarWidth = 16f;
	const float kPaddingMin = 16f;


	Texture[] m_textures;
	
	TextureViewType m_type;
	GenericMenu m_displayTypeMenu;
	Texture[] m_displayed;
	Texture m_selected;

	string m_searchString = string.Empty;
	float m_itemSize = kItemSizeMin;
	int m_rawNum;
	int m_columnNum;
	Vector2 m_padding;

	Rect m_scrollRect;
	Vector2 m_scrollPosition;
	

	//------------------------------------------------------
	// static function
	//------------------------------------------------------

	[MenuItem("Window/Builtin/BuiltinResourceView")]
	static void Open()
	{
		GetWindow<BuiltinResourceView>();
	}

	static AssetBundle GetBuiltinAssetBundle()
	{
		var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", BindingFlags.Static | BindingFlags.NonPublic);
		return info.Invoke(null, new object[0]) as AssetBundle;
	}


	//------------------------------------------------------
	// unity system function
	//------------------------------------------------------

	void OnEnable()
	{
		m_textures = GetBuiltinAssetBundle()
			.LoadAllAssets(typeof(Texture))
			.OfType<Texture>()
			.ToArray();

		minSize = new Vector2(200, 150);

		m_displayTypeMenu = new GenericMenu();
		foreach (TextureViewType type in Enum.GetValues(typeof(TextureViewType)))
		{
			var _type = type;
			m_displayTypeMenu.AddItem(new GUIContent(type.ToString()), false, () => m_type = _type);
		}
	}

	void OnGUI()
	{
		DrawSearchBar();
		DrawTextureList();
		DrawFooter();

		switch (Event.current.type)
		{
			case EventType.MouseDown:
				if (m_scrollRect.Contains(Event.current.mousePosition))
				{
					OnClicked(Event.current);
					Repaint();
				}
				break;
		}
	}



	//------------------------------------------------------
	// gui
	//------------------------------------------------------

	void DrawSearchBar()
	{
		GUI.Box(new Rect(0, 0, position.width, 16), GUIContent.none, "Toolbar");
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Space(8f);
			if (GUILayout.Button(m_type.ToString(), "ToolbarPopup", GUILayout.Width(80)))
			{
				var r = GUILayoutUtility.GetLastRect(); // これで取得できるのはどうやら直前のBoxのようだ...
				r.x += 16f;
				r.y += 16f;
				m_displayTypeMenu.DropDown(r);
			}
			GUILayout.Space(8f);
			GUILayout.FlexibleSpace();
			m_searchString = GUILayout.TextField(m_searchString, "ToolbarSeachTextField", GUILayout.MinWidth(80), GUILayout.MaxWidth(300));
			if (GUILayout.Button(GUIContent.none, "ToolbarSeachCancelButton"))
			{
				m_searchString = string.Empty;
				GUI.FocusControl(null);
			}
			GUILayout.Space(8f);
		}
	}

	void DrawTextureList()
	{
		m_scrollRect = GUILayoutUtility.GetRect(GUIContent.none, "ScrollView", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

		m_displayed = GetTargetTextures();
		var viewRectWidth = m_scrollRect.width - kScrollBarWidth;

		m_columnNum = Mathf.FloorToInt((viewRectWidth - kPaddingMin) / (m_itemSize+ kPaddingMin));
		m_rawNum = m_displayed.Length / m_columnNum + (m_displayed.Length % m_columnNum == 0 ? 0 : 1);
		m_padding.x = (viewRectWidth - m_itemSize * m_columnNum) / (m_columnNum + 1);
		m_padding.y = kPaddingMin;

		using (var scroll = new GUI.ScrollViewScope(m_scrollRect, m_scrollPosition, 
			new Rect(0, 0, viewRectWidth, m_rawNum * (m_itemSize + m_padding.y) + m_padding.y)))
		{
			var top = m_scrollRect.y + m_scrollPosition.y;
			var bottom = top + m_scrollRect.height;
			var y = kPaddingMin;
			for (int i = 0; i < m_rawNum; ++i, y += m_itemSize + kPaddingMin)
			{
				if (y >= bottom || y + m_itemSize <= top) continue;

				DrawColumn(y, m_displayed, i * m_columnNum, m_columnNum, m_padding.x);
			}

			m_scrollPosition = scroll.scrollPosition;
		}
	}

	Texture[] GetTargetTextures()
	{
		return string.IsNullOrEmpty(m_searchString) ?
			m_textures :
			Array.FindAll(m_textures, i => i.name.Contains(m_searchString));
	}

	void DrawColumn(float y, Texture[] textures, int textureIndex, int count, float itemPadding)
	{
		var itemPosition = new Rect(itemPadding, y, m_itemSize, m_itemSize);

		count = Mathf.Min(count, textures.Length - 1 - textureIndex);
		for (int i = 0; i < count; ++i)
		{
			DrawTexture(itemPosition, textures[textureIndex + i]);
			itemPosition.x += itemPosition.width + itemPadding;
		}
	}

	void DrawTexture(Rect itemPosition, Texture texture)
	{
		// ProjectBrowserIconDropShadow の OnFocus が選択時青いテクスチャが設定されている
		// しかしどうすれば OnFocus 状態にできるのか？
		// わからないからとりあえず枠を描画する...orz
		if (texture == m_selected)
		{
			var r = itemPosition;
			r.x -= kPaddingMin * 0.5f;
			r.y -= kPaddingMin * 0.5f;
			r.width += kPaddingMin;
			r.height += kPaddingMin;
			GUI.Box(r, GUIContent.none, "ProjectBrowserTextureIconDropShadow");
		}

		GUI.SetNextControlName(texture.name);
		switch (m_type)
		{
			case TextureViewType.RGB:
				//GUI.Box(itemPosition, texture, "ProjectBrowserIconDropShadow"); 選択時の青表示これにしたい...
				GUI.DrawTexture(itemPosition, texture);
				break;
				
			case TextureViewType.Alpha:
				EditorGUI.DrawTextureAlpha(itemPosition, texture);
				break;

			case TextureViewType.Transparent:
				EditorGUI.DrawTextureTransparent(itemPosition, texture);
				break;
		}		
	}

	void DrawFooter()
	{
		const float kPaddingL = 8f;
		const float kPaddingR = 16f;
		const float kSliderWidth = 64f;

		var itemPosition = GUILayoutUtility.GetRect(GUIContent.none, "Toolbar", GUILayout.ExpandWidth(true));
		GUI.Box(itemPosition, GUIContent.none, "Toolbar");

		itemPosition.x = kPaddingL;
		itemPosition.width = position.width - kSliderWidth - kPaddingR;
		EditorGUI.LabelField(itemPosition, m_selected ? m_selected.name : "");

		itemPosition.x = position.width - kSliderWidth - kPaddingR;
		itemPosition.width = kSliderWidth;
		m_itemSize = GUI.HorizontalSlider(itemPosition, m_itemSize, kItemSizeMin, kItemSizeMax);
	}


	//------------------------------------------------------
	// events
	//------------------------------------------------------

	void OnClicked(Event ev)
	{
		// 間の空白領域は選択と判定しない

		var y = (ev.mousePosition.y - m_scrollRect.y + m_scrollPosition.y);
		var blockHeight = m_itemSize + m_padding.y;
		var raw = Mathf.FloorToInt(y / blockHeight);
		if (y - blockHeight * raw <= m_padding.y)
			return;
		
		var x = (ev.mousePosition.x - m_scrollRect.x + m_scrollPosition.x);
		var blockWidth = m_itemSize + m_padding.x;
		var column = Mathf.FloorToInt(x / blockWidth);
		if (x - blockWidth * column <= m_padding.x)
			return;

		if (raw >= m_rawNum || column >= m_columnNum)
			return;

		m_selected = m_displayed[raw * m_columnNum + column];
		Selection.activeObject = m_selected;

		// どうすれば GUIStyle を OnFocused にできるんだ...
		//GUI.FocusControl(m_selected.name);
		//EditorGUI.FocusTextInControl(m_selected.name);
	}
}
