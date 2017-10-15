using System.Collections;
using UnityEngine;

namespace Framework.SceneManagement
{
	public interface ISceneManagementHandler
	{
		IEnumerator OnLoadPreprocess(string sceneName);
		void OnLoadBegan(string sceneName);
		void OnLoadEnded(string sceneName);
	}


	public sealed class SceneManager : SingletonBehaviour<SceneManager>
	{
		[SerializeField, SceneName] string m_defaultSceneName;
		[SerializeField] SceneTransition[] m_transitions;

		bool m_isLoading;
		SceneBehaviour m_current;

		ISceneManagementHandler m_handler;


		//------------------------------------------------------
		// settings
		//------------------------------------------------------

		public void SetHandler(ISceneManagementHandler handler)
		{
			m_handler = handler;
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		/// <summary>
		/// transitionIndex に -1 を指定するとトランジションなしにできる
		/// </summary>
		public static void LoadScene(string sceneName, int transitionIndex = 0)
		{
			instance.LoadSceneConcrete(sceneName, transitionIndex);
		}

		void LoadSceneConcrete(string sceneName, int transitionIndex)
		{
			if (m_isLoading)
			{
				Debug.LogWarning("already loading scene.");
				return;
			}

			var transisiton = transitionIndex >= 0 && transitionIndex < m_transitions.Length ? 
				m_transitions[transitionIndex] : 
				null;
			
			StartCoroutine(yLoadScene(sceneName, transisiton));
		}

		IEnumerator yLoadScene(string sceneName, SceneTransition transition)
		{
			m_isLoading = true;
			if (m_handler != null)
			{
				m_handler.OnLoadBegan(sceneName);
			}

			if (transition != null)
			{
				yield return transition.Enter();
			}

			if (m_handler != null)
			{
				var process = m_handler.OnLoadPreprocess(sceneName);
				if (process != null) yield return process;
			}

			yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName,
				UnityEngine.SceneManagement.LoadSceneMode.Single);

			while (!m_current || !m_current.IsSetup)
			{
				if (m_current && m_current.IsError)
				{
					yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(m_defaultSceneName,
						UnityEngine.SceneManagement.LoadSceneMode.Single);
				}

				yield return null;
			}

			if (transition != null)
			{
				yield return transition.Exit();
			}

			m_isLoading = false;

			if (m_handler != null)
			{
				m_handler.OnLoadEnded(sceneName);
			}
		}


		//------------------------------------------------------
		// scene behaviour
		//------------------------------------------------------

		public void OnSceneActivated(SceneBehaviour scene)
		{
			m_current = scene;
		}

		public void OnSceneDeactivated(SceneBehaviour scene)
		{
			if (m_current == scene)
			{
				m_current = null;
			}
		}
	}
}