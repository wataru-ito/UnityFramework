using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace DefineSymbolEditor
{
	/// <summary>
	/// 編集用のデータ
	/// </summary>
	class DefineSymbolStatus
	{
		public class Toggle
		{
			public readonly DefineSymbolContext.Toggle context;
			public bool enabled;

			public Toggle(DefineSymbolContext.Toggle context, string[] symbols)
			{
				this.context = context;
				enabled = Array.IndexOf(symbols, context.name) >= 0;
			}

			public override string ToString()
			{
				return enabled ? context.name : string.Empty;
			}
		}

		public class Dropdown
		{
			public readonly DefineSymbolContext.Dropdown context;
			public int index;

			public Dropdown(DefineSymbolContext.Dropdown context, string[] symbols)
			{
				this.context = context;
				index = -1;

				var symbolIndex = Array.FindIndex(symbols, i => i.StartsWith(context.name));
				if (symbolIndex >= 0)
				{
					var itemName = symbols[symbolIndex].Substring(context.name.Length + 1);
					index = context.items.IndexOf(itemName);
				}
			}

			public override string ToString()
			{
				return index >= 0 ? string.Format("{0}_{1}", context.name, context.items[index]) : string.Empty;
			}
		}

		public readonly BuildTargetGroup target;
		public DefineSymbolStatus common;
		public List<Toggle> toggles;
		public List<Dropdown> dropdowns;


		//------------------------------------------------------
		// factory
		//------------------------------------------------------

		public DefineSymbolStatus(BuildTargetGroup target, DefineSymbolStatus common, DefineSymbolContext context, string symbol)
		{
			this.target = target;
			this.common = common;

			var symbols = symbol.Split(';');
			toggles = context.toggles.ConvertAll(i => new Toggle(i, symbols));
			dropdowns = context.dropdowns.ConvertAll(i => new Dropdown(i, symbols));
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public string ToSymbols()
		{
			var sb = new StringBuilder();

			if (common != null)
			{
				CollectSymbol(sb, common.toggles);
				CollectSymbol(sb, common.dropdowns);
			}

			CollectSymbol(sb, toggles);
			CollectSymbol(sb, dropdowns);

			return sb.ToString();
		}

		static void CollectSymbol<T>(StringBuilder sb, List<T> stateList) 
			where T : class
		{
			foreach (var state in stateList)
			{
				var symbol = state.ToString();
				if (!string.IsNullOrEmpty(symbol))
				{
					if (sb.Length > 0) sb.Append(";");
					sb.Append(symbol);
				}
			}
		}
	}
}