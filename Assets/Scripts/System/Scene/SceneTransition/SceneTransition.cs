﻿using UnityEngine;

namespace Framework.SceneManagement
{
	public abstract class SceneTransition : MonoBehaviour
	{
		//------------------------------------------------------
		// overridable function
		//------------------------------------------------------

		public abstract string transitionName { get; }
		public abstract void Enter();
		public abstract void Exit();
		public abstract bool IsTransiting { get; }
	}
}