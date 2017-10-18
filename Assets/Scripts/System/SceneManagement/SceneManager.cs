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


	/// <summary>
	/// このクラスは sealed. 機能拡張はHandlerで。(Handlerって違うよね。なんていうの？
	/// </summary>
	public sealed class SceneManager : SingletonBehaviour<SceneManager>
	{
		enum State
		{
			None,
			Loading,
			Transiting,
		}

		[SerializeField, SceneName] string m_defaultSceneName;
		[SerializeField] SceneTransition[] m_transitions;

		State m_state;
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
			var transisiton = transitionIndex >= 0 && transitionIndex < m_transitions.Length ? 
				m_transitions[transitionIndex] : 
				null;

			StopAllCoroutines();
			StartCoroutine(yLoadScene(sceneName, transisiton));
		}

		IEnumerator yLoadScene(string sceneName, SceneTransition transition)
		{
			m_state = State.Loading;
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

			m_current = null;

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

			// フェードが明けるのはロード完了合図してから
			m_state = State.Transiting;
			if (m_handler != null)
			{
				m_handler.OnLoadEnded(sceneName);
			}

			if (transition != null)
			{
				yield return transition.Exit();
			}

			m_state = State.None;
		}

		/// <summary>
		/// トランジションが明ける時は既に false になっている
		/// </summary>
		public static bool IsLoading
		{
			get { return s_instance && s_instance.m_state == State.Loading; ; }
		}

		/// <summary>
		/// ロード終了後のトランジション中もtrueを返す
		/// </summary>
		public static bool IsTransiting
		{
			get { return s_instance && s_instance.m_state != State.None; }
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