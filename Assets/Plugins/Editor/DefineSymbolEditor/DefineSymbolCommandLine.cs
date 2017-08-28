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
			return data.GetPresetSymbols(presetName, targetGroup);
		}
	}
}
