using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public float moveDuration = 0.2f;
    [Tooltip("延时多长时间通知Cell中的其他物体")]
    public float LateWaitingDuration = 0.1f;
    int inputX;
    int inputY;
    bool isMoving = false;
    public Cell fatherCell;
    CellChild cellChild;

    [HideInInspector]
    public Vector3 targetPos = new Vector3();
    public Direction faceTo;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            //Cell内的物体必须属于Cell的子物体，方便随意拖动
            fatherCell = transform.parent.gameObject.GetComponent<Cell>();
        }
        catch
        {
            Debug.LogError(this.gameObject.name + "has no father Cell");
        }
        cellChild = GetComponent<CellChild>();

    }

    // Update is called once per frame
    void Update()
    {
        inputX = (int)Input.GetAxisRaw("Horizontal");
        inputY = (int)Input.GetAxisRaw("Vertical");
        if (inputX == 1) Move(Direction.right);
        else if (inputX == -1) Move(Direction.left);
        else if (inputY == 1) Move(Direction.up);
        else if (inputY == -1) Move(Direction.down);
    }
    private void Move(Direction direction)
    {
        if (isMoving)
        {
            //TODO:平滑连续移动
            return;
        }
        else
        {
            try
            {
                fatherCell = transform.parent.gameObject.GetComponent<Cell>();
            }
            catch
            {
                Debug.LogError(this.gameObject.name + "has no father Cell");
                return;
            }
            if (fatherCell.CanMoveThis(cellChild, direction)) StartCoroutine(MoveProcess(direction));
            else { 
                //Debug.Log(this.gameObject.name + "无法移动"); 
            }
        }
    }


    
    IEnumerator MoveProcess(Direction direction)
    {
        isMoving = true;
        targetPos = transform.position;
        faceTo = direction;
        switch (direction)
        {
            case Direction.right: targetPos.x += WorldGrid.Instance.sideSize; break;
            case Direction.left: targetPos.x -= WorldGrid.Instance.sideSize; break;
            case Direction.up: targetPos.z += WorldGrid.Instance.sideSize; break;
            case Direction.down: targetPos.z -= WorldGrid.Instance.sideSize; break;
        }
        transform.DOMove(targetPos, moveDuration);
        yield return new WaitForSeconds(LateWaitingDuration);
        fatherCell.LatePlayerMove();
        yield return new WaitForSeconds(moveDuration - LateWaitingDuration);
        isMoving = false;
    }
    public Vector2Int PosInCell()
    {
        Vector2Int pos = new Vector2Int();
        pos.x = (int)((transform.position.x - fatherCell.origin.position.x) / WorldGrid.Instance.sideSize);
        pos.y = (int)((transform.position.z - fatherCell.origin.position.z) / WorldGrid.Instance.sideSize);
        return pos;
    }
    public Vector2Int TargetPosInCell()
    {
        Vector2Int pos = new Vector2Int();
        pos.x = (int)((targetPos.x - fatherCell.origin.position.x) / WorldGrid.Instance.sideSize);
        pos.y = (int)((targetPos.z - fatherCell.origin.position.z) / WorldGrid.Instance.sideSize);
        return pos;
    }
}
