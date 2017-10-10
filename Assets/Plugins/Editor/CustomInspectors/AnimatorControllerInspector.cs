using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;


[CustomEditor(typeof(AnimatorController))]
public class AnimatorControllerInspector : Editor
{
	//------------------------------------------------------
	// unity system function
	//------------------------------------------------------

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.Space();

		if (GUILayout.Button("CombineAnimationclip"))
		{
			ContainClip.Create(target as AnimatorController);
		}
	}
}