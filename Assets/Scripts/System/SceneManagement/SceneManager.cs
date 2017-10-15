using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PreloadProcess = System.Func<string, System.Collections.IEnumerator>;

namespace Framework.SceneManagement
{
	public interface ISceneManagementHandler
	{
		void OnLoadBegan(string sceneName);
		void OnLoadEnded(string sceneName);
	}


	public class SceneManager : SingletonBehaviour<SceneManager>
	{
		[SerializeField, SceneName] string m_defaultSceneName;
		[SerializeField] SceneTransition[] m_transitions;

		bool m_isLoading;
		SceneBehaviour m_current;

		PreloadProcess m_preloadProcess;
		List<ISceneManagementHandler> m_listeners = new List<ISceneManagementHandler>();


		//------------------------------------------------------
		// event listener
		//------------------------------------------------------

		public static void Subscribe(ISceneManagementHandler listener)
		{
			instance.m_listeners.Add(listener);
		}

		public static void Unsubscribe(ISceneManagementHandler listener)
		{
			if (s_instance)
			{
				s_instance.m_listeners.Remove(listener);
			}
		}


		//------------------------------------------------------
		// settings
		//------------------------------------------------------

		public void SetPreloadProcess(PreloadProcess preloadProcess)
		{
			m_preloadProcess = preloadProcess;
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
			m_listeners.ForEach(i => i.OnLoadBegan(sceneName));

			if (transition != null)
			{
				transition.Enter();
				yield return new WaitWhile(() => transition.IsTransiting);
			}

			if (m_preloadProcess != null)
			{
				var process = m_preloadProcess(sceneName);
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
				transition.Exit();
				yield return new WaitWhile(() => transition.IsTransiting);
			}

			m_isLoading = false;
			m_listeners.ForEach(i => i.OnLoadEnded(sceneName));
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