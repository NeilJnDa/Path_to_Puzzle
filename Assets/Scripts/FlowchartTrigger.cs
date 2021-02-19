using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Fungus;

public class FlowchartTrigger : MonoBehaviour
{
    UnityAction action;
    Player player;
    public bool interactable = true;
    public bool isNear = false;
    public bool repeat = false;
    public SpriteRenderer bubble;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        action = new UnityAction(CheckPlayerNear);
        player.OnPlayerMoveLate.AddListener(action);
        CheckPlayerNear();
    }
    public IEnumerator ShakePosition(float duration, float strength)
    {
        transform.DOShakePosition(duration, strength);
        yield return new WaitForSeconds(duration);
    }
    // Update is called once per frame
    void Update()
    {
        if(isNear && Input.GetKeyDown(KeyCode.E))
        {
            try
            {
                GetComponent<Flowchart>().ExecuteBlock("Start");
                SetBubble(false);
            }
            catch
            {
                Debug.LogError(this.gameObject.name + " has no block named Start");
            }

        }
    }
    public void SetInteractable(bool b)
    {
        interactable = b;
    }
    private void CheckPlayerNear()
    {
        if (interactable)
        {
            Vector2Int playerPos = player.TargetPosInCell();
            Vector2Int pos = transform.parent.GetComponent<Cell>().PosInCell(transform);
            if (Vector2Int.Distance(playerPos, pos) <= 1f)
            {
                isNear = true;
                try
                {
                    GetComponent<Flowchart>().ExecuteBlock("Start");
                    SetBubble(false);
                }
                catch
                {
                    Debug.LogError(this.gameObject.name + " has no block named Start");
                }
                if(!repeat)interactable = false;

            }
            else isNear = false;
        }

    }
    public void SetBubble(bool b)
    {
        if (b)
        {
            bubble.DOFade(1f, 1f);
        }
        else
        {
            bubble.DOFade(0f, 1f);
        }
    }
}
