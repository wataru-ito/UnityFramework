using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace DefineSymbolEditor
{
	class DefineSymbolStatus
	{
		interface IStatus
		{
			void DrawEdit();
			string ToSymbol();
		}

		class ToggleStatus : IStatus
		{
			public readonly DefineSymbolContext.Toggle toggle;
			public bool enabled;

			public ToggleStatus(DefineSymbolContext.Toggle toggle, string[] symbols)
			{
				this.toggle = toggle;
				enabled = Array.IndexOf(symbols, toggle.name) >= 0;
			}

			public void DrawEdit()
			{
				enabled = EditorGUILayout.Toggle(toggle.content, enabled);
			}

			public string ToSymbol()
			{
				return enabled ? toggle.name : string.Empty;
			}
		}

		class DropdownStatus : IStatus
		{
			public readonly DefineSymbolContext.Dropdown dropdown;
			public int index;

			public DropdownStatus(DefineSymbolContext.Dropdown dropdown, string[] symbols)
			{
				this.dropdown = dropdown;
				index = -1;

				var symbolIndex = Array.FindIndex(symbols, i => i.StartsWith(dropdown.name));
				if (symbolIndex >= 0)
				{
					var itemName = symbols[symbolIndex].Substring(dropdown.name.Length + 1);
					index = dropdown.items.IndexOf(itemName);
				}
			}

			public void DrawEdit()
			{
				index = EditorGUILayout.Popup(dropdown.content, index, dropdown.displayedOptions);
			}

			public string ToSymbol()
			{
				return index >= 0 ? string.Format("{0}_{1}", dropdown.name, dropdown.items[index]) : string.Empty;
			}
		}

		public BuildTargetGroup target;
		DefineSymbolStatus m_common;
		List<ToggleStatus> m_toggles;
		List<DropdownStatus> m_dropdowns;


		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		public static DefineSymbolStatus[] Create(DefineSymbolContext context, BuildTargetGroup[] targets)
		{
			DefineSymbolContext commonContext, indivisualContext;
			context.Split(out commonContext, out indivisualContext);

			var commonStatus = new DefineSymbolStatus(BuildTargetGroup.Unknown, null, commonContext,
				PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone));

			return Array.ConvertAll(targets, i =>
			{
				return new DefineSymbolStatus(i, commonStatus, indivisualContext,
					PlayerSettings.GetScriptingDefineSymbolsForGroup(i));
			});
		}

		public static DefineSymbolStatus[] Create(DefineSymbolContext context, BuildTargetGroup[] targets, DefineSymbolPreset preset)
		{
			DefineSymbolContext commonContext, indivisualContext;
			context.Split(out commonContext, out indivisualContext);

			var commonStatus = new DefineSymbolStatus(BuildTargetGroup.Unknown, null, commonContext,
				PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone));

			return Array.ConvertAll(targets, i =>
			{
				return new DefineSymbolStatus(i, commonStatus, indivisualContext,
					preset.GetScriptingDefineSymbolsForGroup(i));
			});
		}

		DefineSymbolStatus(BuildTargetGroup target, DefineSymbolStatus common, DefineSymbolContext context, string symbol)
		{
			this.target = target;
			m_common = common;

			var symbols = symbol.Split(';');
			m_toggles = context.toggles.ConvertAll(i => new ToggleStatus(i, symbols));
			m_dropdowns = context.dropdowns.ConvertAll(i => new DropdownStatus(i, symbols));
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public void DrawEdit()
		{
			if (m_common != null)
			{
				EditorGUILayout.LabelField("共通");
				m_common.DrawEdit();
				EditorGUILayout.Space();
			}

			m_toggles.ForEach(i => i.DrawEdit());
			m_dropdowns.ForEach(i => i.DrawEdit());
		}

		public string ToSymbol()
		{
			var sb = new StringBuilder();

			if (m_common != null)
			{
				CollectSymbol(sb, m_common.m_toggles);
				CollectSymbol(sb, m_common.m_dropdowns);
			}

			CollectSymbol(sb, m_toggles);
			CollectSymbol(sb, m_dropdowns);

			return sb.ToString();
		}

		static void CollectSymbol<T>(StringBuilder sb, List<T> stateList) 
			where T : IStatus
		{
			foreach (var state in stateList)
			{
				var symbol = state.ToSymbol();
				if (!string.IsNullOrEmpty(symbol))
				{
					if (sb.Length > 0) sb.Append(";");
					sb.Append(symbol);
				}
			}
		}
	}
}