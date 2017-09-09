using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DefineSymbolEditor
{
	/// <summary>
	/// シンボルの設定項目情報
	/// </summary>
	[Serializable]
	class DefineSymbolContext
	{
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

		public void Split(out DefineSymbolContext common, out DefineSymbolContext indivisual)
		{
			common = new DefineSymbolContext();
			indivisual = new DefineSymbolContext();

			foreach (var toggle in toggles)
			{
				(toggle.individual ? indivisual : common).toggles.Add(toggle);
			}

			foreach (var dropdown in dropdowns)
			{
				(dropdown.individual ? indivisual : common).dropdowns.Add(dropdown);
			}
		}
	}
}