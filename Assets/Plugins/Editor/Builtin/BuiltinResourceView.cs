using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BuiltinResourceView : EditorWindow
{
	const float kThumbSize = 128f;
	const float kLabelHeight = 16f;
	const float kPadding = 4f;
	const float kItemWidth = kThumbSize + kPadding;
	const float kItemHeight = kThumbSize + kLabelHeight + kPadding;
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

		minSize = new Vector2(300, 200);
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
		// boxの枠が消えちゃうから削っておく
		scrollRect.x += 1f;
		scrollRect.y += 1f;
		scrollRect.width -= 2f;
		scrollRect.height -= 2f;


		var targets = string.IsNullOrEmpty(m_searchString) ?
			m_textures :
			Array.FindAll(m_textures, i => i.name.Contains(m_searchString));

		var column = Mathf.FloorToInt((scrollRect.width - 16f) / kItemWidth);
		var raw = Mathf.CeilToInt(targets.Length / (float)column);

		var viewRect = new Rect(0, 0, scrollRect.width - 16f, raw * kItemHeight);
		using (var scroll = new GUI.ScrollViewScope(scrollRect, m_scrollPosition, viewRect))
		{
			var top = scrollRect.y + m_scrollPosition.y;
			var bottom = top + scrollRect.height;

			for (int i = 0; i < raw; ++i)
			{
				var y = i * kItemHeight;
				if (y >= bottom || y + kItemHeight <= top) continue;

				DrawColumn(y, targets, i * column, column);
			}

			m_scrollPosition = scroll.scrollPosition;
		}

		EditorGUILayout.HelpBox("ダブルクリックでInpsectorにPreview表示", MessageType.Info);
	}

	void DrawColumn(float y, Texture[] textures, int textureIndex, int count)
	{
		count = Mathf.Min(count, textures.Length - 1 - textureIndex);
		for (int i = 0; i < count; ++i)
		{
			var texture = textures[textureIndex + i];
			
			var rect = new Rect(i * kItemWidth, y, kThumbSize, kLabelHeight);
			EditorGUI.LabelField(rect, texture.name);

			rect.y += rect.height;
			rect.height = kThumbSize;
			if (texture.width >= texture.height)
			{
				rect.height = texture.height / (float)texture.width * kThumbSize;
			}
			else
			{
				rect.width = texture.width / (float)texture.height * kThumbSize;
			}

			EditorGUI.ObjectField(rect, texture, typeof(Texture), false);
		}
	}
}
