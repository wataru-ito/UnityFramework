using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace DefineSymbolEditor
{
	[Serializable]
	class DefineSymbolContext
	{
		const float kBtnWidth = 38f;

		[Serializable]
		public class Symbol
		{
			public string name;
			public string description;
			public bool individual; // プラットフォーム別の設定か

			public Symbol()
			{}

			public Symbol(Symbol src)
			{
				this.name = src.name;
				this.description = src.description;
				this.individual = src.individual;
			}

			public GUIContent content
			{
				get { return new GUIContent(name, description); }
			}

			public virtual bool DrawEdit()
			{
				var deleteFlag = false;
				using (new EditorGUILayout.HorizontalScope())
				{
					name = EditorGUILayout.TextField("名前", name);
					deleteFlag = GUILayout.Button("削除", EditorStyles.miniButton, GUILayout.Width(kBtnWidth));
				}
				description = EditorGUILayout.TextField("説明", description);
				individual = EditorGUILayout.Toggle("Platform別", individual);

				return deleteFlag;
			}
		}

		[Serializable]
		public class Toggle : Symbol
		{
			public Toggle()
			{}

			public Toggle(Toggle src) 
				: base(src)
			{}
		}

		[Serializable]
		public class Dropdown : Symbol
		{
			public List<string> items;

			public Dropdown()
			{
				items = new List<string>();
			}

			public Dropdown(Dropdown src)
				: base(src)
			{
				items = new List<string>(src.items);
			}

			public GUIContent[] displayedOptions
			{
				get
				{
					var array = new GUIContent[items.Count];
					for (int i = 0; i < items.Count; ++i)
					{
						array[i] = new GUIContent(items[i]);
					}
					return array;
				}
			}

			public override bool DrawEdit()
			{
				var deleteFlg = base.DrawEdit();
				++EditorGUI.indentLevel;

				for (int j = 0; j < items.Count; ++j)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						items[j] = EditorGUILayout.TextField(items[j]);
						if (GUILayout.Button("削除", EditorStyles.miniButton, GUILayout.Width(kBtnWidth)))
						{
							items.RemoveAt(j--);
						}
					}
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("選択追加"))
					{
						items.Add("ITEM" + items.Count);
					}
				}

				--EditorGUI.indentLevel;

				return deleteFlg;
			}
		}

		public List<Toggle> toggles;
		public List<Dropdown> dropdowns;


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public DefineSymbolContext()
		{
			toggles = new List<Toggle>();
			dropdowns = new List<Dropdown>();
		}

		public DefineSymbolContext(DefineSymbolContext src)
		{
			toggles = new List<Toggle>();
			dropdowns = new List<Dropdown>();

			src.toggles.ForEach(i => toggles.Add(new Toggle(i)));
			src.dropdowns.ForEach(i => dropdowns.Add(new Dropdown(i)));
		}

		public void Split(out DefineSymbolContext general, out DefineSymbolContext indivisual)
		{
			general = new DefineSymbolContext();
			indivisual = new DefineSymbolContext();

			foreach (var toggle in toggles)
			{
				(toggle.individual ? indivisual : general).toggles.Add(toggle);
			}

			foreach (var dropdown in dropdowns)
			{
				(dropdown.individual ? indivisual : general).dropdowns.Add(dropdown);
			}
		}

		//------------------------------------------------------
		// gui
		//------------------------------------------------------

		public void DrawEdit()
		{
			DrawEdit("Toggle", toggles, CreateToggle);
			DrawEdit("Dropdown", dropdowns, CreateDropdown);
		}

		void DrawEdit<T>(string label, List<T> list, Func<T> createInstance)
			where T : Symbol
		{
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			for (int i = 0; i < list.Count; ++i)
			{
				using (new EditorGUILayout.VerticalScope("box"))
				{
					var deleteFlag = list[i].DrawEdit();
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

		Toggle CreateToggle()
		{
			var toggle = new Toggle();
			toggle.name = "TOGGLE" + toggles.Count;
			toggle.description = string.Empty;
			return toggle;
		}

		Dropdown CreateDropdown()
		{
			var dropdown = new Dropdown();
			dropdown.name = "DROPDOWN" + dropdowns.Count;
			dropdown.description = string.Empty;
			dropdown.items.Add("ITEM0");
			return dropdown;
		}
	}
}