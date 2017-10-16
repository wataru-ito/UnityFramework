using UnityEngine;
using UnityEngine.Assertions;
using ReferenceClearCallback = System.Action;

/// <summary>
/// 内部に参照カウンタを持ち、参照が０になるまで待ち続ける
/// </summary>
public class WaitCounter : CustomYieldInstruction
{
	int m_refCount;
	ReferenceClearCallback m_callback;

	//------------------------------------------------------
	// lifetime
	//------------------------------------------------------

	public WaitCounter(ReferenceClearCallback callback = null)
	{
		m_callback = callback;
	}

	//------------------------------------------------------
	// accessor
	//------------------------------------------------------

	public void AddRef()
	{
		++m_refCount;
	}

	public void RemoveRef()
	{
		Assert.IsTrue(m_refCount > 0, "RefCount remove over.");
		if (--m_refCount == 0 && m_callback != null)
		{
			m_callback();
			m_callback = null;
		}
	}

	public override bool keepWaiting
	{
		get { return m_refCount > 0; }
	}
}
