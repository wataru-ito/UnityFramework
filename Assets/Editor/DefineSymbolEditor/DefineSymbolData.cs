using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace DefineSymbolEditor
{
	[Serializable]
	class DefineSymbolPreset
	{
		[Serializable]
		public struct Symbol
		{
			public BuildTargetGroup target;
			public string symbol;
		}

		public string name;
		public Symbol[] symbols;

		public static DefineSymbolPreset Create(string name, DefineSymbolStatus[] status)
		{
			var preset = new DefineSymbolPreset();
			preset.name = name;
			preset.symbols = Array.ConvertAll(status, i => new Symbol { target = i.target, symbol = i.ToSymbol() });
			return preset;
		}

		public string GetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup)
		{
			var index = Array.FindIndex(symbols, i => i.target == targetGroup);
			return index >= 0 ? symbols[index].symbol : string.Empty;
		}
	}


	[Serializable]
	class DefineSymbolData
	{
		const string kFilePath = "ProjectSettings/DefineSymbolEditor.txt";

		public DefineSymbolContext context;
		public List<DefineSymbolPreset> presets;

		public DefineSymbolData()
		{
			context = new DefineSymbolContext();
			presets = new List<DefineSymbolPreset>();
		}


		//------------------------------------------------------
		// 保存/復元
		//------------------------------------------------------

		public static DefineSymbolData Load()
		{
			try
			{
				if (!File.Exists(kFilePath))
				{
					return new DefineSymbolData();
				}

				var json = File.ReadAllText(kFilePath);
				return JsonUtility.FromJson<DefineSymbolData>(json);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				return new DefineSymbolData();
			}
		}

		public void Save()
		{
			try
			{
				var json = EditorJsonUtility.ToJson(this);
				File.WriteAllText(kFilePath, json, System.Text.Encoding.UTF8);
			}
			catch (Exception)
			{
			}
		}


		//------------------------------------------------------
		// Preset
		//------------------------------------------------------

		/// <returns>null:保存されてない string.Empty:見つかったけど空だっただけ</returns>
		public static string GetScriptDefineSymbolPreset(string presetName, BuildTargetGroup targetGroup)
		{
			var data = Load();

			var preset = data.presets.Find(i => i.name == presetName);
			if (preset == null) return null;

			var index = Array.FindIndex(preset.symbols, i => i.target == targetGroup);
			return index >= 0 ? preset.symbols[index].symbol : null;
		}
	}
}