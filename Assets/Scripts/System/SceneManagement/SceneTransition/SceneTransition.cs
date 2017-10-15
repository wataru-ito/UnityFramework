using System.Collections;
using UnityEngine;

namespace Framework.SceneManagement
{
	public abstract class SceneTransition : MonoBehaviour
	{
		//------------------------------------------------------
		// overridable function
		//------------------------------------------------------

		public abstract IEnumerator Enter();
		public abstract IEnumerator Exit();
	}
}