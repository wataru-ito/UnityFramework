using System;
using UnityEditor;

namespace DefineSymbolEditor
{
	public static class DefineSymbolEditorCommandLineTool
	{
		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		/// <returns>null:保存されてない string.Empty:見つかったけど空だっただけ</returns>
		public static string GetScriptDefineSymbol(string presetName, BuildTargetGroup targetGroup)
		{
			var data = DefineSymbolData.Load();

			var preset = data.presets.Find(i => i.name == presetName);
			if (preset == null) return null;

			var index = Array.FindIndex(preset.symbols, i => i.target == targetGroup);
			return index >= 0 ? preset.symbols[index].symbol : null;
		}
	}
}
