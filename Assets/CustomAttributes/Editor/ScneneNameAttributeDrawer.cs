using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class ScneneNameAttributeDrawer : PropertyDrawer
{
	string[] m_sceneNames;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.propertyType != SerializedPropertyType.String)
		{
			EditorGUI.LabelField(position, property.displayName, "Use SceneName with string.");
			return;
		}
		
		if (m_sceneNames == null)
		{	
			var sceneNameAttribute = attribute as SceneNameAttribute;
			m_sceneNames = sceneNameAttribute.findFromAssetDatabase ? 
				GetSceneNamesFromAssetDatabase() :
				GetSceneNamesFromBuildSettings();
		}

		if (m_sceneNames.Length == 0)
		{
			EditorGUI.LabelField(position, property.displayName, "Scene not found.");
			return;
		}
		
		var index = System.Array.IndexOf(m_sceneNames, property.stringValue);
		index = EditorGUI.Popup(position, property.displayName, index, m_sceneNames);
		if (index >= 0)
		{
			property.stringValue = m_sceneNames[index];
		}
	}

	string[] GetSceneNamesFromBuildSettings()
	{
		return EditorBuildSettings.scenes
			.Select(i => Path.GetFileNameWithoutExtension(i.path))
			.ToArray();
	}

	string[] GetSceneNamesFromAssetDatabase()
	{
		return AssetDatabase.FindAssets("t:scene")
			.Select(i => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(i)))
			.ToArray();
	}
}
