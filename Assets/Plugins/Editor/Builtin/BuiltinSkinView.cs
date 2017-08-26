using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BuiltinSkinView : EditorWindow
{
	const float kItemHeight = 16f;

	readonly string[] kSkin =
	{
		"Game",
		"Inspector",
		"Scene",
	};

	EditorSkin m_skinType;
	GUISkin m_skin;
	SerializedObject m_skinObj;
	List<Texture> m_textureList;

	Vector2 m_listScrollPosition;
	Vector2 m_styleScrollPosition;
	string m_searchString = string.Empty;


	//------------------------------------------------------
	// static function
	//------------------------------------------------------

	[MenuItem("Window/Builtin/BuiltinSkinView")]
	static void Open()
	{
		GetWindow<BuiltinSkinView>();
	}


	//------------------------------------------------------
	// unity system function
	//------------------------------------------------------

	void OnGUI()
	{
		EditorGUI.BeginChangeCheck();
		m_skinType = (EditorSkin)GUILayout.Toolbar((int)m_skinType, kSkin);
		if (EditorGUI.EndChangeCheck() || m_skin == null || m_skinObj == null)
		{
			m_skin = EditorGUIUtility.GetBuiltinSkin(m_skinType);
			m_skinObj = new SerializedObject(m_skin);
		}

		DrawSearchBar();
		DrawSkin(m_skin);
		//DrawSelectedStyle();
		DrawAssetButton();
	}


	//------------------------------------------------------
	// gui
	//------------------------------------------------------

	void DrawSearchBar()
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			EditorGUILayout.LabelField("使用テクスチャ名", GUILayout.Width(100));
			GUILayout.FlexibleSpace();
			m_searchString = GUILayout.TextField(m_searchString, "SearchTextField", GUILayout.Width(150));
			if (GUILayout.Button(GUIContent.none, "SearchCancelButton"))
			{
				m_searchString = string.Empty;
				GUI.FocusControl(null);
			}
		}
	}

	void DrawSkin(GUISkin skin)
	{
		GUILayout.Box(GUIContent.none, GUILayout.MinHeight(100), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

		var scrollRect = GUILayoutUtility.GetLastRect();
		// boxの枠が消えちゃうから削っておく
		scrollRect.x += 1f;
		scrollRect.y += 1f;
		scrollRect.width -= 2f;
		scrollRect.height -= 2f;

		var targets = string.IsNullOrEmpty(m_searchString) ?
			skin.customStyles :
			skin.customStyles.Where(i => Contains(i, m_searchString)).ToArray();

		var viewRect = new Rect(0, 0, scrollRect.width - 16f, targets.Length * kItemHeight);
		using (var scroll = new GUI.ScrollViewScope(scrollRect, m_listScrollPosition, viewRect))
		{
			for (int i = 0; i < targets.Length; ++i)
			{
				var itemPosition = new Rect(0, i * kItemHeight, viewRect.width - 16f, kItemHeight);
				EditorGUI.LabelField(itemPosition, targets[i].name);
			}

			m_listScrollPosition = scroll.scrollPosition;
		}
	}

	void DrawSelectedStyle()
	{
		var prop = m_skinObj.FindProperty("m_CustomStyles");
		if (prop == null) return;

		using (var scroll = new EditorGUILayout.ScrollViewScope(m_styleScrollPosition))
		{
			prop = prop.GetArrayElementAtIndex(0);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, true);
			}

			m_styleScrollPosition = scroll.scrollPosition;
		}
	}

	void DrawAssetButton()
	{
		if (GUILayout.Button("アセット化"))
		{
			CreateBuiltinSkinAsset(m_skinType);
		}
	}


	//------------------------------------------------------
	// skin
	//------------------------------------------------------

	static bool Contains(GUIStyle style, string str)
	{
		return Contains(style.active, str) ||
			Contains(style.focused, str) ||
			Contains(style.hover, str) ||
			Contains(style.normal, str) ||
			Contains(style.onActive, str) ||
			Contains(style.onFocused, str) ||
			Contains(style.onHover, str) ||
			Contains(style.onNormal, str);
	}

	static bool Contains(GUIStyleState state, string str)
	{
		return state.background && state.background.name.Contains(str);
	}


	static void CreateBuiltinSkinAsset(EditorSkin skinType)
	{
		AssetDatabase.CreateAsset(
			Object.Instantiate(EditorGUIUtility.GetBuiltinSkin(skinType)),
			string.Format("Assets/BuiltinSkin_{0}.guiskin", skinType));
	}
}
