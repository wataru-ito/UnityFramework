using System.Collections;
using UnityEngine;

namespace Amber.SceneManagement
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