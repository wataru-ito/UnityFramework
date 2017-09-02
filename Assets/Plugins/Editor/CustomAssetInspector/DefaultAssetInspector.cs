// UNIBOOK 総集編 アセット編 (https://unity-bu.booth.pm/items/392553) より
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DefaultAsset))]
public class DefaultAssetInspector : Editor
{
	static Type[] s_customAssetTypes;
	Editor m_editor;


	//------------------------------------------------------
	// static function
	//------------------------------------------------------

	/// <summary>
	/// DLL のType を全チェックするのはそれなりに重いのでInitializeOnLoadMethod でコンパイル直後のみ実行する
	/// </summary>
	[InitializeOnLoadMethod]
	static void Init()
	{
		s_customAssetTypes = GetCustomAssetTypes();
	}

	/// <summary>
	/// CustomAsset 属性のついたクラスを取得する
	/// </summary>
	static Type[] GetCustomAssetTypes()
	{
		//ユーザーの作成したDLL 内から取得する
		var assemblyPaths = Directory.GetFiles("Library/ScriptAssemblies", "*.dll");
		var types = new List<Type>();
		var customAssetTypes = new List<Type>();
		foreach (var assembly in assemblyPaths.Select(assemblyPath => Assembly.LoadFile(assemblyPath)))
		{
			types.AddRange(assembly.GetTypes());
		}
		foreach (var type in types)
		{
			var customAttributes = type.GetCustomAttributes(typeof(CustomAssetAttribute), false) as CustomAssetAttribute[];
			if (0 < customAttributes.Length)
				customAssetTypes.Add(type);
		}
		return customAssetTypes.ToArray();
	}

	/// <summary>
	/// 拡張子に対応したCustomAsset 属性のついたクラスをすべて取得する
	/// </summary>
	/// <param name="extension">拡張子（例: .zip）</param>
	Type GetCustomAssetEditorType(string extension)
	{
		foreach (var type in s_customAssetTypes)
		{
			var customAttributes = type.GetCustomAttributes(typeof(CustomAssetAttribute), false) as CustomAssetAttribute[];
			foreach (var customAttribute in customAttributes)
			{
				if (customAttribute.extensions.Contains(extension))
					return type;
			}
		}
		return null;
	}


	//------------------------------------------------------
	// unity system function
	//------------------------------------------------------

	void OnEnable()
	{
		var assetPath = AssetDatabase.GetAssetPath(target);
		var extension = Path.GetExtension(assetPath);
		var customAssetEditorType = GetCustomAssetEditorType(extension);
		if (customAssetEditorType != null)
			m_editor = CreateEditor(target, customAssetEditorType);
	}

	public override void OnInspectorGUI()
	{
		if (m_editor == null) return;
			
		GUI.enabled = true;
		m_editor.OnInspectorGUI();
	}
}