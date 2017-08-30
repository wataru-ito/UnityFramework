using UnityEngine;
using UnityEngine.Events;

namespace Framework.BackEventNotice
{
	public class BackEventReceiver : MonoBehaviour
	{
		[SerializeField] UnityEvent m_onBack;


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void OnEnable()
		{
			BackEventNotifier.Instance.AddObserver(this);
		}

		void OnDisable()
		{
			BackEventNotifier.Instance.RemoveObserver(this);
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