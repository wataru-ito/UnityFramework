using UnityEngine;


namespace Framework.Title
{
	public class TitleManager : SceneManagement.SceneBehaviour
	{
		[SerializeField, SceneName] string m_nextSceneName;

		//------------------------------------------------------
		// event
		//------------------------------------------------------

		public void OnStartButtonDown()
		{
			SceneManagement.SceneManager.LoadScene(m_nextSceneName);
		}
	}
}
