using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(POTAttribute))]
public class POTAttributeDrawer : PropertyDrawer
{
	string[] m_displayOptions;
	int[] m_optionValues;
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.propertyType != SerializedPropertyType.Integer)
		{
			EditorGUI.LabelField(position, property.displayName, "Use POT with integer.");
			return;
		}
		
		if (m_displayOptions == null)
		{	
			var potAttribute = attribute as POTAttribute;
			var values = new List<int>();
			for (int i = 1; i <= potAttribute.max; i <<= 1)
			{
				if (potAttribute.min > i) continue;
				values.Add(i);				
			}

			m_displayOptions = values.ConvertAll<string>(i => i.ToString()).ToArray();
			m_optionValues = values.ToArray();
		}

		if (m_displayOptions.Length == 0)
		{
			EditorGUI.LabelField(position, property.displayName, "Invalid Range.");
			return;
		}
		
		property.intValue = EditorGUI.IntPopup(position, 
			property.displayName, 
			property.intValue, 
			m_displayOptions, 
			m_optionValues);
	}
}
