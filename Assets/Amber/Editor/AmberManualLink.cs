using UnityEditor;
using UnityEngine;

namespace Amber
{
	class AmberManualLink
	{
		[MenuItem("Help/Amber/マニュアル", false, -1)]
        static void OpenManual()
		{
			Application.OpenURL(Application.dataPath + "/../Manual/AmberManual.html");
		}	
	}
}