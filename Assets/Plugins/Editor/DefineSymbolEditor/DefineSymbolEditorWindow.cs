using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DefineSymbolEditor
{
	class DefineSymbolEditorWindow : EditorWindow
	{
		class PresetCreateWindow : EditorWindow
		{
			string m_name = "PRESET";
			Action<string> m_callbackl;

			public static PresetCreateWindow Open(Action<string> callback)
			{
				var win = CreateInstance<PresetCreateWindow>();
				win.m_callbackl = callback;
				win.ShowAuxWindow();
				return win;
			}

			void OnEnable()
			{
				titleContent = new GUIContent("プリセット作成");
				minSize =
				maxSize = new Vector2(250, 40);
			}

			void OnGUI()
			{
				EditorGUIUtility.labelWidth = 70f;
				m_name = EditorGUILayout.TextField("プリセット名", m_name);
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("決定", "ButtonLeft"))
					{
						m_callbackl(m_name);
						Close();
					}
					if (GUILayout.Button("戻る", "ButtonRight"))
					{
						Close();
					}
				}
			}
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
		DefineSymbolStatus m_statusCommon;
		DefineSymbolStatus[] m_status; //kTargetsと対応

		string[] m_presetLabels;
		string[] m_presetDeleteLabels;

		int m_targetIndex;
		Action m_mode;

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
			s_instane.ShowUtility();
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
			icon.x += 2;
			icon.y += (icon.height - kTargetIconSize) * 0.5f;
			icon.width = icon.height = kTargetIconSize;
			GUI.DrawTexture(icon, m_targetIcons[index]);

			itemPosition.x = icon.xMax + 4f;
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
								m_mode();
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
				GUI.enabled = m_mode == DrawSymbolMode;
				if (GUILayout.Button("シンボル定義編集", kFooterBtnWidth))
				{
					SetContextMode();
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Apply", kFooterBtnWidth))
				{
					OnApply();
				}
				if (GUILayout.Button("Revert", kFooterBtnWidth))
				{
					OnRevert();
				}
			}
		}

		void OnApply()
		{
			if (m_mode == DrawContextMode)
			{
				m_data.context = new DefineSymbolContext(m_context);
				m_data.Save();
				SetSymbolMode();
			}
			else
			{
				m_data.commonSymbols = m_statusCommon.ToSymbols();
				m_data.Save();
				foreach (var target in kTargets)
				{
					PlayerSettings.SetScriptingDefineSymbolsForGroup(target, GetScriptingDefineSymbols(target));
				}
				Close();
			}
		}

		string GetScriptingDefineSymbols(BuildTargetGroup target)
		{
			if (!m_data.targets.Contains(target))
				return string.Empty;

			var index = Array.FindIndex(m_status, i => i.target == target);
			return index >= 0 ? m_status[index].ToSymbols() : string.Empty;
		}

		void OnRevert()
		{
			if (m_mode == DrawContextMode)
			{
				m_context = new DefineSymbolContext(m_data.context);
				SetSymbolMode();
			}
			else
			{
				Close();
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
			m_presetLabels[m_presetLabels.Length - 1] = "保存";


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
				m_status = CreateStatus(m_data.presets[index]);
				return;
			}

			PresetCreateWindow.Open(name =>
			{
				var preset = DefineSymbolPreset.Create(name, m_statusCommon, m_status);
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
			m_buildTargetRect.width -= 1f;
			m_buildTargetRect.height -= 1f;

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

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var togglePosition = new Rect(
					itemPosition.xMax - 32f,
					itemPosition.y + (itemPosition.height - 16f) * 0.5f,
					16f, 16f);
				
				var selected = GUI.Toggle(togglePosition, m_data.targets.Contains(kTargets[index]), GUIContent.none);
				if (check.changed)
				{
					if (selected)
					{
						m_data.targets.Add(kTargets[index]);
						m_data.targets.Sort((x, y) => Array.IndexOf(kTargets, x).CompareTo(Array.IndexOf(kTargets, y)));
					}
					else
					{
						m_data.targets.Remove(kTargets[index]);
					}
				}
			}
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
			m_mode = DrawSymbolMode;
			m_status = CreateStatus();
		}

		DefineSymbolStatus[] CreateStatus()
		{
			DefineSymbolContext commonContext, indivisualContext;
			m_context.Split(out commonContext, out indivisualContext);

			m_statusCommon = new DefineSymbolStatus(BuildTargetGroup.Unknown, null, commonContext, m_data.commonSymbols);

			return Array.ConvertAll(kTargets, i =>
			{
				return new DefineSymbolStatus(i, m_statusCommon, indivisualContext,
					PlayerSettings.GetScriptingDefineSymbolsForGroup(i));
			});
		}

		DefineSymbolStatus[] CreateStatus(DefineSymbolPreset preset)
		{
			DefineSymbolContext commonContext, indivisualContext;
			m_context.Split(out commonContext, out indivisualContext);

			m_statusCommon = new DefineSymbolStatus(BuildTargetGroup.Unknown, null, commonContext, preset.commonSymbols);

			return Array.ConvertAll(kTargets, i =>
			{
				return new DefineSymbolStatus(i, m_statusCommon, indivisualContext,
					preset.GetScriptingDefineSymbolsForGroup(i));
			});
		}

		void DrawSymbolMode()
		{
			var targetEnabled = m_data.targets.Contains(kTargets[m_targetIndex]);
			GUI.enabled = targetEnabled;

			GUILayout.Box(GUIContent.none, "Label", GUILayout.Height(32), GUILayout.ExpandWidth((true)));
			DrawBuildTargetIcon(GUILayoutUtility.GetLastRect(), m_targetIndex);

			EditorGUILayout.Space();
			DrawEditStatus(m_status[m_targetIndex]);

			GUI.enabled = true;
			if (!targetEnabled)
			{
				EditorGUILayout.HelpBox("このプラットフォームを有効にするにはチェックを入れてください", MessageType.Info);
			}
		}

		void DrawEditStatus(DefineSymbolStatus status)
		{
			if (status.common != null)
			{
				EditorGUILayout.LabelField("共通");
				DrawEditStatus(status.common);
				EditorGUILayout.Space();
			}

			status.toggles.ForEach(EditStatusToggle);
			status.dropdowns.ForEach(EditStatusDropdown);
		}

		void EditStatusToggle(DefineSymbolStatus.Toggle toggle)
		{
			toggle.enabled = EditorGUILayout.Toggle(toggle.context.content, toggle.enabled);
		}

		void EditStatusDropdown(DefineSymbolStatus.Dropdown dropdown)
		{
			dropdown.index = EditorGUILayout.Popup(dropdown.context.content, dropdown.index, dropdown.context.displayedOptions);
		}


		//------------------------------------------------------
		// context mode
		//------------------------------------------------------

		void SetContextMode()
		{
			m_mode = DrawContextMode;
		}

		void DrawContextMode()
		{
			var prev = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80f;

			DrawEdit("Toggle", m_context.toggles, DrawEditToggle, CreateToggle);
			DrawEdit("Dropdown", m_context.dropdowns, DrawEditDropdown, CreateDropdown);

			EditorGUIUtility.labelWidth = prev;
		}

		void DrawEdit<T>(string label, List<T> list, Func<T,bool> drawer, Func<T> createInstance)
			where T : DefineSymbolContext.Symbol
		{
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			for (int i = 0; i < list.Count; ++i)
			{
				using (new EditorGUILayout.VerticalScope("box"))
				{
					var deleteFlag = drawer(list[i]);
					if (deleteFlag)
					{
						list.RemoveAt(i--);
					}
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("定義追加"))
				{
					list.Add(createInstance());
				}
			}
		}

		const float kBtnWidth = 38f;

		bool DrawEditSymbol(DefineSymbolContext.Symbol symbol)
		{
			var deleteFlag = false;
			using (new EditorGUILayout.HorizontalScope())
			{
				symbol.name = EditorGUILayout.TextField("名前", symbol.name);
				deleteFlag = GUILayout.Button("削除", EditorStyles.miniButton, GUILayout.Width(kBtnWidth));
			}
			symbol.description = EditorGUILayout.TextField("説明", symbol.description);
			symbol.individual = EditorGUILayout.Toggle("Platform別", symbol.individual);

			return deleteFlag;
		}

		bool DrawEditToggle(DefineSymbolContext.Toggle toggle)
		{
			return DrawEditSymbol(toggle);
		}

		bool DrawEditDropdown(DefineSymbolContext.Dropdown dropdown)
		{
			var deleteFlg = DrawEditSymbol(dropdown);
			++EditorGUI.indentLevel;

			for (int j = 0; j < dropdown.items.Count; ++j)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					dropdown.items[j] = EditorGUILayout.TextField(dropdown.items[j]);
					if (GUILayout.Button("削除", EditorStyles.miniButton, GUILayout.Width(kBtnWidth)))
					{
						dropdown.items.RemoveAt(j--);
					}
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("選択追加"))
				{
					dropdown.items.Add("ITEM" + dropdown.items.Count);
				}
			}

			--EditorGUI.indentLevel;

			return deleteFlg;
		}

		DefineSymbolContext.Toggle CreateToggle()
		{
			var toggle = new DefineSymbolContext.Toggle();
			toggle.name = "TOGGLE" + m_context.toggles.Count;
			toggle.description = string.Empty;
			return toggle;
		}

		DefineSymbolContext.Dropdown CreateDropdown()
		{
			var dropdown = new DefineSymbolContext.Dropdown();
			dropdown.name = "DROPDOWN" + m_context.dropdowns.Count;
			dropdown.description = string.Empty;
			dropdown.items.Add("ITEM0");
			return dropdown;
		}
	}
}