using UnityEngine;
using UnityEditor;

namespace Amber
{
	public sealed class IndentScope : GUI.Scope
	{
		public IndentScope()
		{
			++EditorGUI.indentLevel;
		}

		protected override void CloseScope()
		{
			--EditorGUI.indentLevel;
		}
	}
}