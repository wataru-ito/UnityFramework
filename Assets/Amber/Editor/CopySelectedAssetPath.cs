using System.Linq;
using UnityEditor;

namespace Amber
{
	static class CopySelectedAssetPath
	{
		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		[MenuItem("Assets/Copy Selected AssetPath", validate = true)]
		static bool IsCopiable()
		{
			return Selection.assetGUIDs.Length > 0;
		}

		[MenuItem("Assets/Copy Selected AssetPath", priority = int.MaxValue)]
		static void Start()
		{
			var guids = Selection.assetGUIDs;
			if (guids.Length == 1)
			{
				EditorGUIUtility.systemCopyBuffer = AssetDatabase.GUIDToAssetPath(guids[0]);
			}
			else
			{
				var sb = new System.Text.StringBuilder();
				foreach (var assetPath in guids.Select(i => AssetDatabase.GUIDToAssetPath(i)))
				{
					sb.AppendLine(assetPath);
				}
				EditorGUIUtility.systemCopyBuffer = sb.ToString();
			}
		}
	}
}