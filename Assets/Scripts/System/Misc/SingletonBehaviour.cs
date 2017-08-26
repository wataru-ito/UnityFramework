using UnityEngine;

namespace Framework
{
	public class SingletonBehaviour<T> : MonoBehaviour
		where T : MonoBehaviour
	{
		//------------------------------------------------------
		// static functions
		//------------------------------------------------------

		protected static T s_instance;

		public static T instance
		{
			get
			{
				if (s_instance == null)
					s_instance = FindObjectOfType<T>();
				return s_instance;
			}
		}

		public static bool Exists()
		{
			return s_instance != null;
		}

		//------------------------------------------------------
		// unity system functions
		//------------------------------------------------------

		protected virtual void Awake()
		{
			if (s_instance)
			{
				Debug.LogWarningFormat("{0} already exists.", typeof(T).FullName);
			}

			s_instance = this as T;
		}

		protected virtual void OnDestory()
		{
			if (s_instance == this)
			{
				s_instance = null;
			}
		}
	}
}