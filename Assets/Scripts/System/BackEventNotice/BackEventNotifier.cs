using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.BackEventNotice
{
	public class BackEventNotifier : MonoBehaviour
	{
		[SerializeField] UnityEvent m_onBackDefault;
		List<BackEventReceiver> m_observers = new List<BackEventReceiver>();


		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		static BackEventNotifier s_instance;

		public static BackEventNotifier Instance
		{
			get
			{
				if (s_instance == null)
					s_instance = FindObjectOfType<BackEventNotifier>();
				return s_instance;
			}
		}


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void Awake()
		{
			s_instance = this;
		}

		void OnDestroy()
		{
			if (s_instance == this)
				s_instance = null;
		}

		#if UNITY_EDITOR || !UNITY_IOS // iOSでは戻るボタン禁止
		void LateUpdate()
		{
			// Androidの戻るボタンはEscapeに割り当てられているらしいらしい
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Invoke();
			}	
		}
		#endif


		//------------------------------------------------------
		// observer
		//------------------------------------------------------

		internal void AddObserver(BackEventReceiver receiver)
		{
			m_observers.Add(receiver);
		}

		internal void RemoveObserver(BackEventReceiver receiver)
		{
			m_observers.Remove(receiver);
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public void Invoke()
		{
			if (!enabled) return;

			if (m_observers.Count == 0)
			{
				m_onBackDefault.Invoke();
				return;
			}

			var receiver = m_observers[m_observers.Count - 1];
			receiver.Invoke();
		}
	}
}