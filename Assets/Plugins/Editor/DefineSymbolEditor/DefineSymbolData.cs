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

		public List<BuildTargetGroup> targets;
		public DefineSymbolContext context;
		public List<DefineSymbolPreset> presets;

		public DefineSymbolData()
		{
			targets = new List<BuildTargetGroup>();
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
		// preset
		//------------------------------------------------------

		public string GetPresetSymbols(string presetName, BuildTargetGroup targetGroup)
		{
			var preset = presets.Find(i => i.name == presetName);
			if (preset == null) 
				return string.Empty;
			
			var index = Array.FindIndex(preset.symbols, i => i.target == targetGroup);
			if (index < 0)
				return string.Empty;
			
			// プリセット保存時には有効だったプラットフォームが今は無効化されている事もある
			if (!targets.Contains(targetGroup))
				return string.Empty;

			return preset.symbols[index].symbol;
		}
	}
}