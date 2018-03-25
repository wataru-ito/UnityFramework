using UnityEngine;
using UnityEditor;

namespace Amber
{
	public sealed class LabelWidthScope : GUI.Scope
	{
		readonly float m_labelWidth;

		public LabelWidthScope(float labelWidth)
		{
			m_labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = labelWidth;
		}

		protected override void CloseScope()
		{
			EditorGUIUtility.labelWidth = m_labelWidth;
		}
	}
}