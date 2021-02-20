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
    public bool repeat = false;
    public SpriteRenderer bubble;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        action = new UnityAction(CheckPlayerNear);
        player.OnPlayerMoveLate.AddListener(action);
        //CheckPlayerNear();
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if (player.fatherCell != this.transform.parent.GetComponent<Cell>()) return;
            Vector2Int playerPos = player.fatherCell.PosInCell(player.transform);
            Vector2Int pos = transform.parent.GetComponent<Cell>().PosInCell(this.transform);
            Debug.Log(this.gameObject + " " + Vector2Int.Distance(playerPos, pos).ToString() + " " + playerPos + " " + pos);
        }
    }
    public void SetInteractable(bool b)
    {
        interactable = b;
        SetBubble(b);
    }
    private void CheckPlayerNear()
    {
        if (interactable)
        {
            if (player.fatherCell != this.transform.parent.GetComponent<Cell>()) return;
            Vector2Int playerPos = player.fatherCell.PosInCell(player.transform);
            Vector2Int pos = transform.parent.GetComponent<Cell>().PosInCell(this.transform);
            if (Vector2Int.Distance(playerPos, pos) <= 1f)
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
                if(!repeat)interactable = false;

            }
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
