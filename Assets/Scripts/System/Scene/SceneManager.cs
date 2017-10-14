using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.SceneManagement
{
	public interface ISceneManagementHandler
	{
		void OnLoadBegan();
		void OnLoadEnded();
	}


	public class SceneManager : SingletonBehaviour<SceneManager>
	{
		[SerializeField, SceneName] string m_defaultSceneName;
		[SerializeField] SceneTransition m_defaultTransition;
		[SerializeField] SceneTransition[] m_transitions;

		bool m_isLoading;
		SceneBehaviour m_current;

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
		// accessor
		//------------------------------------------------------

		/// <summary>
		/// transitionName に "" を指定するとトランジションなしにできる
		/// </summary>
		public static void LoadScene(string sceneName, string transitionName = null)
		{
			instance.LoadSceneConcrete(sceneName, transitionName);
		}

		void LoadSceneConcrete(string sceneName, string transitionName)
		{
			if (m_isLoading)
			{
				Debug.LogWarning("already loading scene.");
				return;
			}

			var transisiton = transitionName == null ? 
				m_defaultTransition : 
				System.Array.Find(m_transitions, i => i.transitionName.Equals(transitionName));
			
			StartCoroutine(yLoadScene(sceneName, transisiton));
		}

		IEnumerator yLoadScene(string sceneName, SceneTransition transition)
		{
			m_isLoading = true;
			m_listeners.ForEach(i => i.OnLoadBegan());

			if (transition != null)
			{
				transition.Enter();
				yield return new WaitWhile(() => transition.IsTransiting);
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
			m_listeners.ForEach(i => i.OnLoadEnded());
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


		//------------------------------------------------------
		// editor
		//------------------------------------------------------
#if UNITY_EDITOR

		void OnValidate()
		{
			if (m_defaultTransition == null && m_transitions != null && m_transitions.Length > 0)
			{
				m_defaultTransition = m_transitions[0];
			}
		}

#endif

	}
}