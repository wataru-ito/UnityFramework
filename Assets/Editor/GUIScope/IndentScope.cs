using UnityEngine;
using UnityEditor;

public class IndentScope : GUI.Scope
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
