using UnityEngine;
using UnityEngine.SceneManagement;


namespace Framework.Title
{
	public class TitleManager : MonoBehaviour
	{
		[SerializeField, SceneName] string m_nextSceneName;

		//------------------------------------------------------
		// event
		//------------------------------------------------------

		public void OnStartButtonDown()
		{
			SceneManager.LoadScene(m_nextSceneName);
		}
	}
}
