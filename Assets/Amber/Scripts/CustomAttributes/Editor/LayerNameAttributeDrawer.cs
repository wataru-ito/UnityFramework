using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Amber
{
	[CustomPropertyDrawer(typeof(LayerNameAttribute))]
	public class LayerNameAttributeDrawer : PropertyDrawer
	{
		int[] m_layers;
		string[] m_layerNames;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.Integer)
			{
				EditorGUI.LabelField(position, property.displayName, "Use LayerName with integer.");
				return;
			}

			if (m_layers == null)
			{
				m_layerNames = Enumerable.Range(0, 32)
					.Select(i => LayerMask.LayerToName(i))
					.Where(i => !string.IsNullOrEmpty(i))
					.ToArray();

				m_layers = System.Array.ConvertAll(m_layerNames, i => LayerMask.NameToLayer(i));
			}

			property.intValue = EditorGUI.IntPopup(position,
				property.displayName,
				property.intValue,
				m_layerNames,
				m_layers);
		}
	}
}