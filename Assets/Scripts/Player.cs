using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Linq;

public class Player : MonoBehaviour
{
    public bool movable = true;
    public float moveDuration = 0.2f;
    [Tooltip("延时多长时间通知Cell中的其他物体")]
    public float LateWaitingDuration = 0.1f;
    int inputX;
    int inputY;
    bool isMoving = false;
    [HideInInspector]
    public Cell fatherCell;
    CellChild cellChild;

    [HideInInspector]
    public UnityEvent OnPlayerMoveLate = new UnityEvent();
    [HideInInspector]
    public Vector3 targetPos = new Vector3();
    public Direction faceTo;
    public List <KeyValuePair<Monster,float>> sortedMonsters = new List<KeyValuePair<Monster, float>>();
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
        if (isMoving || !movable)
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
            if (CanMoveThis(cellChild, direction)) StartCoroutine(MoveProcess(direction));
            else { 
                //Debug.Log(this.gameObject.name + "无法移动"); 
            }
        }
    }

    public bool CanMoveThis(CellChild child, Direction direction)
    {
        Vector2Int originalPos = fatherCell.PosInCell(child.transform);
        Vector2Int targetPos = originalPos;
        targetPos += Cell.DirectionVector[direction];
        Cell targetCell = fatherCell;
        bool isCrossingCell = false;

        #region 是否跨越cell边缘
        if (targetPos.x == WorldGrid.Instance.cellLength || targetPos.x == -1 ||
            targetPos.y == WorldGrid.Instance.cellWidth || targetPos.y == -1)
        {
            isCrossingCell = true;
            Debug.Log(child.name + "尝试跨越Cell");
        }
        if (targetPos.x == WorldGrid.Instance.cellLength)
        {
            targetCell = fatherCell.GetNextCell(Direction.right);
            targetPos.x = 0;
        }
        else if (targetPos.x == -1)
        {
            targetCell = fatherCell.GetNextCell(Direction.left);
            targetPos.x = WorldGrid.Instance.cellLength - 1;
        }
        else if (targetPos.y == WorldGrid.Instance.cellWidth)
        {
            targetCell = fatherCell.GetNextCell(Direction.up);
            targetPos.y = 0;
        }
        else if (targetPos.y == -1)
        {
            targetCell = fatherCell.GetNextCell(Direction.down);
            targetPos.y = WorldGrid.Instance.cellWidth - 1;
        }
        #endregion

        if (!targetCell)
        {
            Debug.Log(child.name + "跨越Cell时目标Cell不存在，相邻没有Cell");
            return false;
        }
        if (targetCell.cellGrid[targetPos.x, targetPos.y] == 0)
        {
            Debug.Log(targetCell.name + "的" + targetPos + "没有格子");
            return false;   //不存在格子
        }
        else if (targetCell.groundInfo[targetPos.x, targetPos.y] == GridObjectType.None)
        {
            if (isCrossingCell)
            {
                //跨越Cell
                fatherCell.groundInfo[originalPos.x, originalPos.y] = GridObjectType.None;
                fatherCell.cellObjects[originalPos.x, originalPos.y] = null;

                targetCell.groundInfo[targetPos.x, targetPos.y] = child.type;
                targetCell.cellObjects[targetPos.x, targetPos.y] = child.gameObject;

                fatherCell = targetCell;
                child.transform.parent = targetCell.gameObject.transform;
                return true;
            }
            else
            {
                //不跨越Cell
                targetCell.groundInfo[originalPos.x, originalPos.y] = GridObjectType.None;
                targetCell.cellObjects[originalPos.x, originalPos.y] = null;

                targetCell.groundInfo[targetPos.x, targetPos.y] = child.type;
                targetCell.cellObjects[targetPos.x, targetPos.y] = child.gameObject;

                return true;
            }
        }
        Debug.Log(targetCell.name + "的" + targetPos + "格有" + targetCell.groundInfo[targetPos.x, targetPos.y]);
        return false;   //目标格子有东西
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
        LatePlayerMove();
        OnPlayerMoveLate.Invoke();
        yield return new WaitForSeconds(moveDuration - LateWaitingDuration);
        isMoving = false;
    }
    /// <summary>
    /// 在网格中寻找monsters并做互动
    /// </summary>
    public void LatePlayerMove()
    { 

        Dictionary<Monster, float> monsters = new Dictionary<Monster, float>();
        foreach (var child in FindObjectsOfType<Monster>())
        {

            float distance = Vector3.Distance(child.transform.position, this.transform.position);
            //同属一个父Cell的，通知检测
            if (child.fatherCell == this.fatherCell) monsters.Add(child, distance);
            //世界距离3以内的也要通知移动检测，因为有可能跨cell
            else if (distance <= WorldGrid.Instance.sideSize * 3f) monsters.Add(child, distance);
        }

        //从小到大排序，近的先判定
        sortedMonsters = (from pair in monsters orderby pair.Value select pair).ToList();

        //通知所有子Enemy去检测是否需要移动。
        foreach (var child in sortedMonsters)
        {
            //Debug.Log(child.Key.name);
            child.Key.MoveCheck();
        }
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
    public void SetMovable(bool b)
    {
        movable = b;
    }
}
