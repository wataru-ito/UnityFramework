using UnityEngine;
using UnityEngine.SceneManagement;


namespace Framework.Game
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField, SceneName] string m_backSceneName;

		//------------------------------------------------------
		// event
		//------------------------------------------------------
		
		public void OnBackButtonDown()
		{
			SceneManager.LoadScene(m_backSceneName);
		}
	}
}