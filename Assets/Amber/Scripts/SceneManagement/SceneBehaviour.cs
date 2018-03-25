using UnityEngine;

namespace Amber.SceneManagement
{
	/// <summary>
	/// 各シーンに一つ置いておく
	/// </summary>
	public class SceneBehaviour : MonoBehaviour
	{
		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		protected virtual void Awake()
		{
			var manager = SceneManager.instance;
			manager.OnSceneActivated(this);
		}

		protected virtual void OnDestroy()
		{
			if (SceneManager.Exists())
			{
				SceneManager.instance.OnSceneActivated(this);
			}
		}
		
		/// <summary>
		/// IsSetup が true になるまでトランジションが開始されない
		/// </summary>
		public virtual bool IsSetup
		{
			get { return true; }
		}

		/// <summary>
		/// Setupが失敗したらtrue. トランジションが明けずにデフォルトシーンに戻る
		/// </summary>
		public virtual bool IsError
		{
			get { return false; }
		}
	}
}