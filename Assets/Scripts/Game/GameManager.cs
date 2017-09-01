using UnityEngine;
using UnityEngine.SceneManagement;
using Framework.UI;

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
			if (MessageBox.Exists())
				return;

			MessageBox.YesNo("MENU", "タイトルに戻る",
				yes: () => SceneManager.LoadScene(m_backSceneName));
		}
	}
}