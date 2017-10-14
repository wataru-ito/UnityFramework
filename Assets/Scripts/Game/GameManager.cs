using UnityEngine;
using Framework.UI;

namespace Framework.Game
{
	public class GameManager : SceneManagement.SceneBehaviour
	{
		[SerializeField, SceneName] string m_backSceneName;

		//------------------------------------------------------
		// event
		//------------------------------------------------------
		
		public void OnBackButtonDown()
		{
			if (MessageBox.Exists())
				return;

			MessageBox.YesNo("MENU", "タイトルに戻る",
				yes: () => SceneManagement.SceneManager.LoadScene(m_backSceneName));
		}
	}
}