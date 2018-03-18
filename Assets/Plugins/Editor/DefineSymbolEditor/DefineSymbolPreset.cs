using System;
using UnityEditor;

namespace DefineSymbolEditor
{
	[Serializable]
	class DefineSymbolPreset : IDefineSymbolData
	{
		[Serializable]
		public struct Symbol
		{
			public BuildTargetGroup target;
			public string symbol;
		}

		public string name;
		public string commonSymbols;
		public Symbol[] symbols;

		public static DefineSymbolPreset Create(string name, DefineSymbolStatus common, DefineSymbolStatus[] status)
		{
			var preset = new DefineSymbolPreset();
			preset.name = name;
			preset.commonSymbols = common.ToSymbols();
			preset.symbols = Array.ConvertAll(status, i => new Symbol { target = i.target, symbol = i.ToSymbols() });
			return preset;
		}


		//------------------------------------------------------
		// IDefineSymbolData
		//------------------------------------------------------

		public string GetCommonSymbols()
		{
			return commonSymbols;
		}

		public string GetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup)
		{
			var index = Array.FindIndex(symbols, i => i.target == targetGroup);
			return index >= 0 ? symbols[index].symbol : string.Empty;
		}
	}
}