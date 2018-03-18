#if UNITY_EDITOR || UNITY_DEBUG
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Amber
{
	public class DevelopTop : MonoBehaviour
	{
		[SerializeField] GameObject m_settingsRoot;
		[SerializeField] Button m_clearCache;

		[Space]
		[SerializeField] GameObject m_sceneSelectRoot;
		[SerializeField] GameObject m_button;
		[SerializeField] float m_buttonInterval;
		[SerializeField, SceneName] string[] m_scenes;

		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void Awake()
		{
			InitSceneSelect();

			m_settingsRoot.SetActive(true);
			m_sceneSelectRoot.SetActive(false);
		}


		//------------------------------------------------------
		// Settings
		//------------------------------------------------------

		public void OnClearCacheDown()
		{
			Caching.ClearCache();

			m_clearCache.interactable = false;
		}


		//------------------------------------------------------
		// SceneSelect
		//------------------------------------------------------

		void InitSceneSelect()
		{
			var parent = m_button.transform.parent;
			var localPositionX = m_button.GetComponent<RectTransform>().localPosition.x;
			var localPositionY = 0f;

			foreach (var sceneName in m_scenes)
			{
				var go = Instantiate(m_button, parent) as GameObject;

				var goTrans = go.GetComponent<RectTransform>();
				goTrans.anchoredPosition = new Vector2(localPositionX, localPositionY);

				var text = go.GetComponentInChildren<Text>();
				text.text = sceneName;

				var button = go.GetComponent<Button>();
				var tmp = sceneName;
				button.onClick.AddListener(() => SceneManager.LoadScene(tmp));

				localPositionY += m_buttonInterval;
			}

			m_button.SetActive(false);
		}

		public void OnSceneSelectDown()
		{
			m_settingsRoot.SetActive(false);
			m_sceneSelectRoot.SetActive(true);
		}
	}
}
#endif