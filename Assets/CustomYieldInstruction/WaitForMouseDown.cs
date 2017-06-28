using UnityEngine;

public class WaitForMouseDown : CustomYieldInstruction
{
	int m_button;

	public WaitForMouseDown(int button = 0)
	{
		m_button = button;
	}

	public override bool keepWaiting
    {
        get
        {
            return !Input.GetMouseButtonDown(m_button);
        }
    }
}
