#if DEBUG
using UnityEngine;
using UnityEngine.Assertions;
using DebugMenuSystem;

namespace Amber.DebugSystem
{
	public class DebugManager : MonoBehaviour
	{
		const float kDebugWindowDuration = 3f;
		DebugMenuWindow m_debugMenu;
		float m_presstime;


		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		public static DebugManager Create(Transform parent = null)
		{
			var prefab = Resources.Load<GameObject>("Debug/DebugManager");
			Assert.IsNotNull(prefab);

			var go = Instantiate(prefab, parent);
			go.hideFlags |= HideFlags.DontSave;

			return go.GetComponent<DebugManager>();
		}


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void Awake()
		{
			new DebugMenu("System/Application", DrawApplicationInfo);
		}

		void Update()
		{
			OpenDebugMenu();
		}


		//------------------------------------------------------
		// debug menu
		//------------------------------------------------------

		void OpenDebugMenu()
		{
			if (m_debugMenu)
				return;

			#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.D))
			{
				m_debugMenu = DebugMenuWindow.Create();
				return;
			}
			#endif

			if (Input.touchCount >= 3)
			{
				m_presstime += Time.deltaTime;
				if (m_presstime >= kDebugWindowDuration)
				{
					m_debugMenu = DebugMenuWindow.Create();
					return;
				}
			}
			else
			{
				m_presstime = 0f;
			}
		}

		void DrawApplicationInfo()
		{
			GUILayout.Label("FPS : " + Application.targetFrameRate.ToString());
		}
	}
}
#endif