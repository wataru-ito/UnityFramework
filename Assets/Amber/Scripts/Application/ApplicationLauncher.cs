using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Amber
{
	public class ApplicationLauncher : MonoBehaviour
	{
		[SerializeField, SceneName] string m_bootScene;

		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		IEnumerator Start()
		{
			yield return SceneManager.LoadSceneAsync("Common", LoadSceneMode.Additive);
			yield return new WaitUntil(ApplicationManager.instance.IsSetup);
			SceneManager.LoadScene(m_bootScene);
		}
	}
}