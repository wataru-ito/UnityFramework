using UnityEngine;

public class WaitForDestroy : CustomYieldInstruction
{
	Object m_obj;

	public WaitForDestroy(Object obj)
	{
		m_obj = obj;
	}

	public override bool keepWaiting
	{
		get { return m_obj; }
	}
}
