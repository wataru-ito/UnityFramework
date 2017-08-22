#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	/// <summary>
	/// 他のどのスクリプトよりも早く実行される事
	/// </summary>
	public class DebugApplicationLauncher : MonoBehaviour
	{
		GameObject[] m_gos;

		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void Awake()
		{
			if (ApplicationManager.Exists())
			{
				Destroy(gameObject);
				return;
			}

			// 他のスクリプト起動前に寝かす
			m_gos = FindObjectsOfType<Transform>()
				.Where(i => i.parent == null && i != transform)
				.Select(i => i.gameObject)
				.ToArray();
			foreach (var go in m_gos)
			{
				go.SetActive(false);
			}
		}

		IEnumerator Start()
		{
			yield return SceneManager.LoadSceneAsync("Common", LoadSceneMode.Additive);
			yield return new WaitUntil(ApplicationManager.instance.IsSetup);

			Destroy(gameObject);
		}

		void OnDestroy()
		{
			if (m_gos == null) return;

			foreach (var go in m_gos)
			{
				go.SetActive(true);
			}
		}
	}
}
#endif