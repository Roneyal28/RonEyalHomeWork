#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;

// Runs verification inside actual game frames, where Time and input are valid.
public class HomeworkSimulationRunner : MonoBehaviour
{
    public IEnumerator steps;
    public Action<bool, string> finish;

    private IEnumerator Start()
    {
        while (true)
        {
            bool more;
            object wait = null;
            try
            {
                more = steps.MoveNext();
                if (more) wait = steps.Current;
            }
            catch (Exception error)
            {
                finish(false, error.ToString());
                yield break;
            }
            if (!more) { finish(true, "All checks passed"); yield break; }
            yield return wait is float delay ? new WaitForSeconds(delay) : wait;
        }
    }
}
#endif
