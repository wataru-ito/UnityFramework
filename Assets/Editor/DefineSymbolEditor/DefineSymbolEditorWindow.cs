using System;
using UnityEditor;
using UnityEngine;

namespace DefineSymbolEditor
{
	public class DefineSymbolPresetCreateWindow : EditorWindow
	{
		string m_name = "PRESET";
		Action<string> m_callbackl;

		public static DefineSymbolPresetCreateWindow Open(Action<string> callback)
		{
			var win = CreateInstance<DefineSymbolPresetCreateWindow>();
			win.m_callbackl = callback;
			win.Show(); //ShowPopup();
			return win;
		}

		void OnGUI()
		{
			m_name  = EditorGUILayout.TextField("名前", m_name);
			if (GUILayout.Button("決定"))
			{
				m_callbackl(m_name);
				Close();
			}
		}
	}

	public class DefineSymbolEditorWindow : EditorWindow
	{
		enum Mode
		{
			Symbol,
			Context,
		}

		readonly BuildTargetGroup[] kTargets =
		{
			BuildTargetGroup.Standalone,
			BuildTargetGroup.iOS,
			BuildTargetGroup.Android,
			BuildTargetGroup.WebGL,
			BuildTargetGroup.WSA,
			BuildTargetGroup.Tizen,
			BuildTargetGroup.XboxOne,
			BuildTargetGroup.PSP2,
			BuildTargetGroup.PS4,
			BuildTargetGroup.SamsungTV,
			BuildTargetGroup.N3DS,
			BuildTargetGroup.WiiU,
			BuildTargetGroup.tvOS,
			BuildTargetGroup.Facebook,
			BuildTargetGroup.Switch,
		};

		const float kTargetItemHeight = 36f;
		const float kTargetIconSize = 32f;

		DefineSymbolData m_data;
		DefineSymbolContext m_context;
		DefineSymbolStatus[] m_status; //kTargetsと対応

		string[] m_presetLabels;
		string[] m_presetDeleteLabels;

		int m_targetIndex;
		Mode m_mode;

		Texture[] m_targetIcons; //kTargetsと対応
		GUIStyle m_labelStyle;
		Rect m_buildTargetRect;
		Vector2 m_targetScrollPosition;
		Vector2 m_settingScrollPosition;


		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		static DefineSymbolEditorWindow s_instane;

		[MenuItem("Tools/Build/DefineSymbol Editor")]
		static void Open()
		{
			if (s_instane) return;

			s_instane = CreateInstance<DefineSymbolEditorWindow>();
			s_instane.ShowAuxWindow();
		}


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void OnEnable()
		{
			s_instane = this;

			titleContent = new GUIContent("ScriptDefineSymbol Editor");
			minSize = new Vector2(570f, 380f);

			m_data = DefineSymbolData.Load();
			UpdatePresetLabels();

			m_context = new DefineSymbolContext(m_data.context);
			m_targetIndex = Array.IndexOf(kTargets, EditorUserBuildSettings.selectedBuildTargetGroup);

			InitGUI();
			SetSymbolMode();
		}

		void OnGUI()
		{
			DrawBody();
			EventProcedure();
		}


		//------------------------------------------------------
		// gui
		//------------------------------------------------------

		void InitGUI()
		{
			m_targetIcons = Array.ConvertAll(kTargets, i => LoadIcon(i));


			// 以前は自前のguiskinを持っていたが、free/proのスキン切替失念してた。
			// 今のスキンから複製する方が安い
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			var style = skin.FindStyle("Hi Label");
			if (style == null) return;

			m_labelStyle = new GUIStyle(style);
			m_labelStyle.padding.left = 4;
		}

		static Texture LoadIcon(BuildTargetGroup target)
		{
			var name = target.ToString();

			// PlayerSettingsと似せるためにリネーム
			if (target == BuildTargetGroup.iOS)
				name = "iPhone";

			var icon = EditorGUIUtility.Load(string.Format("BuildSettings.{0}", name)) as Texture;
			if (icon == null)
				icon = EditorGUIUtility.Load(string.Format("d_BuildSettings.{0}", name)) as Texture;
			return icon;
		}

		void DrawBuildTargetIcon(Rect itemPosition, int index)
		{
			var icon = itemPosition;
			icon.y += (icon.height - kTargetIconSize) * 0.5f;
			icon.width = icon.height = kTargetIconSize;
			GUI.DrawTexture(icon, m_targetIcons[index]);

			itemPosition.x += icon.width + 4f;
			itemPosition.y += (itemPosition.height - 16f) * 0.5f;
			itemPosition.height = 16f;
			GUI.Label(itemPosition, kTargets[index].ToString());
		}

		void DrawBody()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				const float kPaddingSide = 12f;
				GUILayout.Space(kPaddingSide);
				using (new EditorGUILayout.VerticalScope())
				{
					GUILayout.Space(8);
					DrawHeader();
					GUILayout.Space(2);
					using (new EditorGUILayout.HorizontalScope())
					{
						using (new EditorGUILayout.VerticalScope(GUILayout.Width(250f + 8f)))
						{
							DrawTargetSelect();
						}

						using (new EditorGUILayout.VerticalScope())
						{
							using (var scroll = new EditorGUILayout.ScrollViewScope(m_settingScrollPosition))
							{
								switch (m_mode)
								{
									case Mode.Symbol:
										DrawSymbolMode();
										break;
									case Mode.Context:
										DrawContextMode();
										break;
								}
								m_settingScrollPosition = scroll.scrollPosition;
							}
						}
					}

					GUILayout.Space(8);
					DrawFooter();
					GUILayout.Space(8);
				}
				GUILayout.Space(kPaddingSide);
			}
		}

		void DrawHeader()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(" Platform", EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var index = EditorGUILayout.Popup(0, m_presetLabels, GUILayout.Width(90));
					if (check.changed)
					{
						OnPresetSelected(index);
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var index = EditorGUILayout.Popup(0, m_presetDeleteLabels, GUILayout.Width(40));
					if (check.changed)
					{
						OnDeletePresetSelected(index);
					}
				}
			}
		}

		void DrawFooter()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				var kFooterBtnWidth = GUILayout.Width(108);
				GUI.enabled = m_mode == Mode.Symbol;
				if (GUILayout.Button("シンボル定義編集", kFooterBtnWidth))
				{
					SetContextMode();
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Apply", kFooterBtnWidth))
				{
					if (m_mode == Mode.Context)
					{
						m_data.context = new DefineSymbolContext(m_context);
						m_data.Save();
						SetSymbolMode();
					}
					else
					{
						ApplySymbol();
						Close();
					}
				}
				if (GUILayout.Button("Revert", kFooterBtnWidth))
				{
					if (m_mode == Mode.Context)
					{
						m_context = new DefineSymbolContext(m_data.context);
						SetSymbolMode();
					}
					else
					{
						Close();
					}
				}
			}
		}


		//------------------------------------------------------
		// preset
		//------------------------------------------------------

		void UpdatePresetLabels()
		{
			m_presetLabels = new string[m_data.presets.Count + 4];
			m_presetLabels[0] = "プリセット選択";
			m_presetLabels[1] = string.Empty;
			for (int i = 0; i < m_data.presets.Count; ++i)
			{
				m_presetLabels[2 + i] = m_data.presets[i].name;
			}
			m_presetLabels[m_presetLabels.Length - 2] = string.Empty;
			m_presetLabels[m_presetLabels.Length - 1] = "新規保存";


			m_presetDeleteLabels = new string[m_data.presets.Count + 2];
			m_presetDeleteLabels[0] = "削除";
			m_presetDeleteLabels[1] = string.Empty;
			for (int i = 0; i < m_data.presets.Count; ++i)
			{
				m_presetDeleteLabels[2 + i] = m_data.presets[i].name;
			}
		}

		void OnPresetSelected(int index)
		{
			index -= 2;
			if (index < 0) return;

			if (index < m_data.presets.Count)
			{
				m_status = DefineSymbolStatus.Create(m_context, kTargets, m_data.presets[index]);
				return;
			}

			DefineSymbolPresetCreateWindow.Open(name =>
			{
				var preset = DefineSymbolPreset.Create(name, m_status);
				m_data.presets.Add(preset);
				m_data.Save();
				UpdatePresetLabels();
			});
		}

		void OnDeletePresetSelected(int index)
		{
			index -= 2;
			if (index < 0) return;

			if (index < m_data.presets.Count)
			{
				m_data.presets.RemoveAt(index);
				m_data.Save();
				UpdatePresetLabels();
			}
		}


		//------------------------------------------------------
		// build target select
		//------------------------------------------------------

		void DrawTargetSelect()
		{
			GUILayout.Box(GUIContent.none, GUILayout.Width(250f), GUILayout.ExpandHeight(true));
			m_buildTargetRect = GUILayoutUtility.GetLastRect();
			m_buildTargetRect.x += 1f;
			m_buildTargetRect.y += 1f;
			m_buildTargetRect.width -= 2f;
			m_buildTargetRect.height -= 2f;

			var viewRect = new Rect(0, 0, m_buildTargetRect.width - 16f, kTargetItemHeight * kTargets.Length);
			using (var scroll = new GUI.ScrollViewScope(m_buildTargetRect, m_targetScrollPosition, viewRect))
			{
				for (int i = 0; i < kTargets.Length; ++i)
				{
					var itemRect = new Rect(0, kTargetItemHeight * i, viewRect.width, kTargetItemHeight);
					DrawTagetSelectItem(itemRect, i);
				}

				m_targetScrollPosition = scroll.scrollPosition;
			}
		}

		void DrawTagetSelectItem(Rect itemPosition, int index)
		{
			var styleState = GetStyleState(m_targetIndex == index);
			if (styleState.background)
			{
				GUI.DrawTexture(itemPosition, styleState.background);
			}
			else
			{
				var prev = GUI.color;
				var gray = new Color(prev.r * 0.95f, prev.g * 0.95f, prev.b * 0.95f);
				var style = GUI.skin.FindStyle("CN EntryBackOdd");
				GUI.color = index % 2 == 0 ? prev : gray;
				GUI.Box(itemPosition, GUIContent.none, style);
				GUI.color = prev;
			}

			DrawBuildTargetIcon(itemPosition, index);
		}

		GUIStyleState GetStyleState(bool selected)
		{
			if (selected)
				return focusedWindow == this ? m_labelStyle.onActive : m_labelStyle.onNormal;
			return m_labelStyle.normal;
		}


		//------------------------------------------------------
		// events
		//------------------------------------------------------

		void EventProcedure()
		{
			switch (Event.current.type)
			{
				case EventType.MouseDown:
					if (m_buildTargetRect.Contains(Event.current.mousePosition))
					{
						OnBuildTargetSelected(Event.current);
						Repaint();
					}
					break;
			}
		}

		void OnBuildTargetSelected(Event ev)
		{
			var index = Mathf.FloorToInt((ev.mousePosition.y - m_buildTargetRect.y + m_targetScrollPosition.y) / kTargetItemHeight);
			if (index >= kTargets.Length)
			{
				return;
			}

			m_targetIndex = index;
		}


		//------------------------------------------------------
		// symbol mode
		//------------------------------------------------------

		void SetSymbolMode()
		{
			m_mode = Mode.Symbol;
			m_status = DefineSymbolStatus.Create(m_context, kTargets);
		}

		void ApplySymbol()
		{
			for (int i = 0; i < kTargets.Length; ++i)
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(kTargets[i], m_status[i].ToSymbol());
			}
		}

		void DrawSymbolMode()
		{
			GUILayout.Box(GUIContent.none, "Label", GUILayout.Height(32), GUILayout.ExpandWidth((true)));
			DrawBuildTargetIcon(GUILayoutUtility.GetLastRect(), m_targetIndex);

			EditorGUILayout.Space();
			m_status[m_targetIndex].DrawEdit();
		}


		//------------------------------------------------------
		// context mode
		//------------------------------------------------------

		void SetContextMode()
		{
			m_mode = Mode.Context;
		}

		void DrawContextMode()
		{
			m_context.DrawEdit();
		}
	}
}