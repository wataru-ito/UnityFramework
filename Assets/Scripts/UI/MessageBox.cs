using UnityEngine;
using UnityEngine.UI;
using Callback = System.Action;

namespace Framework.UI
{
	/// <summary>
	/// トランジション再生用にAnimator
	/// トランジションα実装にcanvsgroup
	/// </summary>
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Canvas))]
	[RequireComponent(typeof(CanvasGroup))]
	public class MessageBox : MonoBehaviour
	{
		readonly int kOpenHash = Animator.StringToHash("Open");

		[SerializeField] Text m_title;
		[SerializeField] Text m_message;

		Animator m_animator;
		bool m_playing;

		Callback m_ok;
		Callback m_close;

		static MessageBox s_instance;


		//--------------------------------------------------
		// static function
		//--------------------------------------------------

		public static MessageBox OK(string title, string message, Callback ok)
		{
			var mb = Create("UI/MessageBox/MessageBoxOK");
			mb.Init(title, message, ok, ok);
			return mb;
		}

		public static MessageBox YesNo(string title, string message, Callback yes, Callback no = null)
		{
			var mb = Create("UI/MessageBox/MessageBoxYesNo");
			mb.Init(title, message, yes, no);
			return mb;
		}

		static MessageBox Create(string resourcesPath)
		{
			var prefab = Resources.Load<GameObject>(resourcesPath);
			var go = Instantiate(prefab);
			return go.GetComponent<MessageBox>();
		}

		void Init(string title, string message, Callback ok, Callback close)
		{
			m_title.text = title;
			m_message.text = message;

			m_ok = ok;
			m_close = close;
		}

		public static bool Exists()
		{
			return s_instance;
		}


		//--------------------------------------------------
		// unity system function
		//--------------------------------------------------

		void Awake()
		{
			// もし仮に複数作ってしまった場合前のインスタンスの手前に描画する
			// > なるべく Exists() でチェックして複数生成しないように
			if (s_instance)
			{
				GetComponent<Canvas>().sortingOrder = s_instance.GetComponent<Canvas>().sortingOrder + 1;
			}	

			s_instance = this;
			m_animator = GetComponent<Animator>();
			m_animator.Update(0f);
		}

		void Start()
		{
			m_animator.SetBool(kOpenHash, true);
		}

		void OnDestroy()
		{
			if (s_instance == this)
			{
				s_instance = null;
			}

			if (m_ok != null)
			{
				m_ok();
				m_ok = null;
			}

			if (m_close != null)
			{
				m_close();
				m_close = null;
			}
		}


		//--------------------------------------------------
		// animator event
		//--------------------------------------------------

		public void OnOpenEnded()
		{
			
		}

		public void OnCloseEnded()
		{
			Destroy(gameObject);
		}


		//--------------------------------------------------
		// accessor
		//--------------------------------------------------

		public void ClearCallbacks()
		{
			m_ok = 
			m_close = null;
		}

		public void OK()
		{
			if (IsPlaying()) return;

			m_close = null;
			m_animator.SetBool(kOpenHash, false);
		}

		public void Close()
		{
			if (IsPlaying()) return;

			m_ok = null;
			m_animator.SetBool(kOpenHash, false);
		}

		public bool IsPlaying()
		{
			return m_animator.GetCurrentAnimatorStateInfo(0).IsTag("Playing");
		}


		//--------------------------------------------------
		// editor
		//--------------------------------------------------
#if UNITY_EDITOR

		[ContextMenu("セットアップ")]
		void Setup()
		{
			var GUIDs = UnityEditor.AssetDatabase.FindAssets("MessageBox t:runtimeAnimatorController");
			if (GUIDs.Length == 0)
			{
				Debug.LogError("MessageBox AnimationController not found.");
				return;
			}

			if (GUIDs.Length > 1)
			{
				Debug.LogWarning("MessageBoxAnimationController exists multiply");
			}

			var _animator = GetComponent<Animator>();
			_animator.runtimeAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
				UnityEditor.AssetDatabase.GUIDToAssetPath(GUIDs[0]));
			UnityEditor.EditorUtility.SetDirty(_animator);
		}

#endif
	}
}