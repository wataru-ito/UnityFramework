using UnityEngine;
using UnityEngine.Events;

namespace Framework.BackEventNotice
{
	public class BackEventListener : MonoBehaviour
	{
		[SerializeField] UnityEvent m_onBack;


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void OnEnable()
		{
			BackEventNotifier.Instance.AddListener(this);
		}

		void OnDisable()
		{
			if (BackEventNotifier.Instance)
			{
				BackEventNotifier.Instance.RemoveListener(this);
			}
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public void Invoke()
		{
			m_onBack.Invoke();
		}
	}
}