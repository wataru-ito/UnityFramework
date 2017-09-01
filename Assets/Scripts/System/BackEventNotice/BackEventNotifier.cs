using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Framework.BackEventNotice
{
	public class BackEventNotifier : MonoBehaviour
	{
		[SerializeField] UnityEvent m_onBackDefault;
		List<BackEventListener> m_listeners = new List<BackEventListener>();


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
			Assert.IsTrue(s_instance == null || s_instance == this, "BackEventNotifier alread exists.");
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

		internal void AddListener(BackEventListener listener)
		{
			m_listeners.Add(listener);
		}

		internal void RemoveListener(BackEventListener listener)
		{
			m_listeners.Remove(listener);
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public void Invoke()
		{
			if (!enabled) return;

			if (m_listeners.Count == 0)
			{
				m_onBackDefault.Invoke();
				return;
			}

			var receiver = m_listeners[m_listeners.Count - 1];
			receiver.Invoke();
		}


		//------------------------------------------------------
		// editor
		//------------------------------------------------------
#if UNITY_EDITOR

		[UnityEditor.CustomEditor(typeof(BackEventNotifier))]
		class BackEventNotifierInspector : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				var notifier = target as BackEventNotifier;
				using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
				{
					UnityEditor.EditorGUILayout.LabelField("Listeners", UnityEditor.EditorStyles.boldLabel);
					++UnityEditor.EditorGUI.indentLevel;
					if (notifier.m_listeners.Count == 0)
					{
						UnityEditor.EditorGUILayout.LabelField("listener no exists");
					}
					else
					{
						foreach (var listener in notifier.m_listeners)
						{
							UnityEditor.EditorGUILayout.ObjectField(listener, typeof(BackEventListener), true);
						}
					}
					--UnityEditor.EditorGUI.indentLevel;
				}
			}
		}

#endif
	}
}