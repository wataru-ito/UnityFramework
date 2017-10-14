using System.Collections;
using UnityEngine;
using EndCallback = System.Action;

namespace Framework.VFX
{
	/// <summary>
	/// Faderが使うカメラはenabled制御されるので何も映さないフェーダー用のカメラ
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public class Fader : SingletonBehaviour<Fader>
	{
		[SerializeField] Material m_material;
		[SerializeField] Color m_color;
		[SerializeField, Range(0,1)] float m_thickness;
		[SerializeField] bool m_sleepWithCamera;

		Camera m_camera;
		Material m_materialInstance;
		int m_colorId;
		int m_thicknessId;

		Coroutine m_coroutine;


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		protected override void Awake()
		{
			base.Awake();

			m_camera = GetComponent<Camera>();
			m_materialInstance = Instantiate<Material>(m_material);
			m_colorId = Shader.PropertyToID("_Color");
			m_thicknessId = Shader.PropertyToID("_Thickness");
			SetColor(m_color);
			SetThickness(m_thickness);
		}

		void OnDisable()
		{
			StopIntepolate();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (m_materialInstance)
			{
				Destroy(m_materialInstance);
					m_materialInstance = null;
			}
		}

		void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			Graphics.Blit(src, dest, m_materialInstance);
		}


		//------------------------------------------------------
		// 色
		//------------------------------------------------------

		public Color color
		{
			get { return m_color; }
			set
			{
				if (m_color != value)
				{
					SetColor(value);
					SetThickness(m_thickness);
				}
			}
		}

		void SetColor(Color color)
		{
			m_color = color;
			m_materialInstance.SetColor(m_colorId, m_color);
		}


		//------------------------------------------------------
		// 強さ
		//------------------------------------------------------

		public float thickness
		{
			get { return m_thickness; }
			set
			{
				var _value = Mathf.Clamp01(value);
				if (!Mathf.Approximately(m_thickness, _value))
				{
					StopIntepolate();
					SetThickness(_value);
				}
			}
		}

		public void SetThickness(float from, float to, float duration, EndCallback onEnded = null)
		{
			StopIntepolate();
			m_coroutine = StartCoroutine(yInterpolate(from, to, duration, onEnded));
		}

		public void SetThickness(float to, float duration, EndCallback onEnded = null)
		{
			StopIntepolate();
			m_coroutine = StartCoroutine(yInterpolate(m_thickness, to, duration, onEnded));
		}

		void SetThickness(float value)
		{
			m_thickness = value;
			m_materialInstance.SetFloat(m_thicknessId, m_thickness);

			// 透明は無駄なので寝とく
			var sleep = Mathf.Approximately(value, 0);
			enabled = m_sleepWithCamera || !sleep; 
			m_camera.enabled = !m_sleepWithCamera || !sleep;
		}

		void StopIntepolate()
		{
			if (m_coroutine != null)
			{
				StopCoroutine(m_coroutine);
				m_coroutine = null;
			}
		}
		
		IEnumerator yInterpolate(float from, float to, float duration, EndCallback onEnded)
		{
			float t = 0;
			while (t < duration)
			{
				t += Time.deltaTime;
				SetThickness(Mathf.Lerp(from, to, t / duration));
				yield return null;
			}

			SetThickness(to);
			m_coroutine = null;

			if (onEnded != null)
			{
				onEnded();
			}
		}

		public bool IsInterpolating
		{
			get{ return m_coroutine != null; }
		}


		//------------------------------------------------------
		// camera
		// Runtimeでカメラの深度変えたい時のために一応
		//------------------------------------------------------

		public float depth
		{
			get { return m_camera.depth; }
			set
			{
				m_camera.depth = value;
			}
		}


		//------------------------------------------------------
		// editor
		//------------------------------------------------------
#if UNITY_EDITOR

		void Reset()
		{
			var GUIDs = UnityEditor.AssetDatabase.FindAssets("Fader t:Material");
			if (GUIDs.Length > 0)
			{
				m_material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
					UnityEditor.AssetDatabase.GUIDToAssetPath(GUIDs[0]));
			}
		}

		void OnValidate()
		{
			if (m_materialInstance)
			{
				SetColor(m_color);
				SetThickness(m_thickness);
			}			
		}

#endif
	}
}