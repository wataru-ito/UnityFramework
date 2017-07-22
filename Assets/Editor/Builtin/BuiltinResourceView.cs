using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BuiltinResourceView : EditorWindow
{
	const float kItemSize = 128f;
	const float kPadding = 4f;
	const float kPreviewHeight = 128f;
	Texture[] m_textures;

	Vector2 m_scrollPosition;
	int m_selectIndex;
	string m_searchString = string.Empty;


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
		var ab = GetBuiltinAssetBundle();
		m_textures = ab.LoadAllAssets(typeof(Texture))
			.Select(i => i as Texture)
			.Where(i => i != null)
			.ToArray();
	}

	void OnGUI()
	{
		DrawSearchBar();
		DrawTextureList();
	}


	//------------------------------------------------------
	// gui
	//------------------------------------------------------

	void DrawSearchBar()
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			m_searchString = GUILayout.TextField(m_searchString, "SearchTextField", GUILayout.Width(300));
			if (GUILayout.Button(GUIContent.none, "SearchCancelButton"))
			{
				m_searchString = string.Empty;
				GUI.FocusControl(null);
			}
		}
	}

	void DrawTextureList()
	{
		GUILayout.Box(GUIContent.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

		var scrollRect = GUILayoutUtility.GetLastRect();
		scrollRect.x += 1f;
		scrollRect.y += 1f;
		scrollRect.width -= 2f;
		scrollRect.height -= 2f;


		var targets = string.IsNullOrEmpty(m_searchString) ?
			m_textures :
			Array.FindAll(m_textures, i => i.name.Contains(m_searchString));

		var columnCount = Mathf.FloorToInt(scrollRect.width / kItemSize);
		var rawCount = Mathf.CeilToInt(targets.Length / (float)columnCount);


		var viewRect = new Rect(0, 0, scrollRect.width - 16, rawCount * (kItemSize + kPadding));
		using (var scroll = new GUI.ScrollViewScope(scrollRect, m_scrollPosition, viewRect))
		{
			for (int i = 0; i < rawCount; ++i)
			{
				DrawColumn(i * (kItemSize + kPadding), targets, i * columnCount, columnCount);
			}

			m_scrollPosition = scroll.scrollPosition;
		}
	}

	void DrawColumn(float y, Texture[] textures, int textureIndex, int count)
	{
		count = Mathf.Min(count, textures.Length - 1 - textureIndex);
		for (int i = 0; i < count; ++i)
		{
			var rect = new Rect(i * (kItemSize + kPadding), y, kItemSize, kItemSize);

			var texture = textures[textureIndex + i];
			if (texture.width >= texture.height)
			{
				rect.height = texture.height / (float)texture.width * kItemSize;
			}
			else
			{
				rect.width = texture.width / (float)texture.height * kItemSize;
			}

			EditorGUI.ObjectField(rect, texture, typeof(Texture), false);
			//GUI.DrawTexture(rect, texture);
		}
	}
}
