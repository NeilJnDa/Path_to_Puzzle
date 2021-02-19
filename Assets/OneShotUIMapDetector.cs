using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OneShotUIMapDetector : MonoBehaviour
{
    public UICell uICell;
    public Vector2Int posInUIJigsaw;
    public UnityEvent OnDetectedTrue = new UnityEvent();
    // Start is called before the first frame update

    public void StartDetecting()
    {
        StartCoroutine(Detector());
    }
    public void StopDetecting()
    {
        StopCoroutine(Detector());
    }
    IEnumerator Detector()
    {
        while (true)
        {
            if (UIJigsaw.Instance.UIJigsawMap[posInUIJigsaw.x, posInUIJigsaw.y] == uICell)
            {
                OnDetectedTrue.Invoke();
                break;
            }
            else yield return new WaitForSeconds(1f);
            Debug.Log("Detecting");
        }
    }
}
