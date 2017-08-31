using UnityEngine;
using UnityEngine.Assertions;

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
			Assert.IsTrue(s_instance == null || s_instance == this, typeof(T).FullName + " already exists.");
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