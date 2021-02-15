using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UICell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Cell relatingCell;
    public bool draggable = true;
    private CanvasGroup canvasGroup;
    Vector2Int lastPosInJigsaw = new Vector2Int();
    private void Awake()
    {
        canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
    }
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable)
        {
            transform.DOShakePosition(1f,3f);
            return;
        }
        canvasGroup.blocksRaycasts = false;
        lastPosInJigsaw = UIJigsaw.Instance.PosInJigsaw(this);
        if (lastPosInJigsaw.x == -1 && lastPosInJigsaw.y == -1 && WorldGrid.Instance.jigsawMode) //从等候区出发且拼图UI界面
            this.transform.SetParent(UIJigsaw.Instance.transform);    //暂时让父物体变为UIJigsaw，使其位于最上方

        transform.SetAsLastSibling();
        //Debug.Log(lastPosInJigsaw);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        SmoothMoveTo(eventData.position);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        //做一次UGUI的raycast，去寻找是否在waiting area内；
        bool targetInWaitingArea = false;
        List<RaycastResult> list = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, list);
        foreach(var i in list)
        {
            if (i.gameObject == UIJigsaw.Instance.waitingArea.gameObject)
                targetInWaitingArea = true;
        }


        var target = eventData.pointerCurrentRaycast;
        if (lastPosInJigsaw.x == -1 && lastPosInJigsaw.y == -1) //从等候区出发
        {
            if (!target.gameObject)
            {
                SmoothMoveTo(UIJigsaw.Instance.waitingArea.position);
                this.transform.SetParent(UIJigsaw.Instance.waitingArea);    //父物体重设，这样关闭地图后图块也不会消失
            }
            else if (targetInWaitingArea|| target.gameObject.tag == "WaitingArea")
            {
                this.transform.SetParent(UIJigsaw.Instance.waitingArea);
            }
            else if (target.gameObject.tag == "CellAnchor")
            {
                SmoothMoveTo(target.gameObject.transform.position);
                var anchor = target.gameObject.GetComponent<CellAnchor>();
                UIJigsaw.Instance.UIJigsawMap[anchor.cellPosInWorld.x, anchor.cellPosInWorld.y] = this;
                this.transform.SetParent(UIJigsaw.Instance.transform);
            }
            else if (target.gameObject.tag == "UICell" )
            {
                if (target.gameObject.GetComponent<UICell>().draggable)
                {
                    //目标索引
                    Vector2Int targetPosInJigsaw = UIJigsaw.Instance.PosInJigsaw(target.gameObject.GetComponent<UICell>());
                    //移动
                    SmoothMoveTo(target.gameObject.transform.position);
                    target.gameObject.GetComponent<UICell>().SmoothMoveTo(UIJigsaw.Instance.waitingArea.position);
                    //更新UIJigsawMap
                    UIJigsaw.Instance.UIJigsawMap[targetPosInJigsaw.x, targetPosInJigsaw.y] = this;
                    //更新父物体
                    this.transform.SetParent(UIJigsaw.Instance.transform);
                    target.gameObject.transform.SetParent(UIJigsaw.Instance.waitingArea);
                }
                else
                {
                    target.gameObject.transform.DOShakePosition(1f, 3f);
                    SmoothMoveTo(UIJigsaw.Instance.waitingArea.position);
                    this.transform.SetParent(UIJigsaw.Instance.waitingArea);
                }

            }
            else
            {
                SmoothMoveTo(UIJigsaw.Instance.waitingArea.position);
                this.transform.SetParent(UIJigsaw.Instance.waitingArea);
            }
        }
        else
        {
            //从Jigsaw内出发的cell
            if (!target.gameObject)
            {
                SmoothMoveTo(UIJigsaw.Instance.PosInWorld(lastPosInJigsaw));
            }
            else if ((targetInWaitingArea || target.gameObject.tag == "WaitingArea"))
            {
                UIJigsaw.Instance.UIJigsawMap[lastPosInJigsaw.x, lastPosInJigsaw.y] = null;
                this.transform.SetParent(UIJigsaw.Instance.waitingArea);
            }
            else if (target.gameObject.tag == "CellAnchor")
            {
                SmoothMoveTo(target.gameObject.transform.position);
                var anchor = target.gameObject.GetComponent<CellAnchor>();
                UIJigsaw.Instance.UIJigsawMap[anchor.cellPosInWorld.x, anchor.cellPosInWorld.y] = this;
            }
            else if (target.gameObject.tag == "UICell")
            {
                if (target.gameObject.GetComponent<UICell>().draggable)
                {
                    //目标索引
                    Vector2Int targetPosInJigsaw = UIJigsaw.Instance.PosInJigsaw(target.gameObject.GetComponent<UICell>());
                    //移动
                    SmoothMoveTo(target.gameObject.transform.position);
                    target.gameObject.GetComponent<UICell>().SmoothMoveTo(UIJigsaw.Instance.PosInWorld(lastPosInJigsaw));
                    //更新UIJigsawMap
                    UIJigsaw.Instance.UIJigsawMap[targetPosInJigsaw.x, targetPosInJigsaw.y] = this;
                    UIJigsaw.Instance.UIJigsawMap[lastPosInJigsaw.x, lastPosInJigsaw.y] = target.gameObject.GetComponent<UICell>();
                }
                else
                {
                    target.gameObject.transform.DOShakePosition(1f, 3f);
                    SmoothMoveTo(UIJigsaw.Instance.PosInWorld(lastPosInJigsaw));
                }

            }

            else
            {
                SmoothMoveTo(UIJigsaw.Instance.PosInWorld(lastPosInJigsaw));
            }
        }
        canvasGroup.blocksRaycasts = true;
        lastPosInJigsaw = Vector2Int.zero;

    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SmoothMoveTo(Vector3 target)
    {
        transform.DOMove(target, 0.5f);
    }
}
