using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

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
			var min = Bit(potAttribute.min);
			var max = Bit(potAttribute.max);
			m_optionValues = Enumerable.Range(min, max - min).Select(i => 1 << i).ToArray();
			m_displayOptions = Array.ConvertAll(m_optionValues, i => i.ToString());
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

	static int Bit(int n)
	{
		int bit = 0;
		for (; n > 1; ++bit) n >>= 1;
		return bit;		
	}
}
