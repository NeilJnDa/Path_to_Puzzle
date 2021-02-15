using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Monster : MonoBehaviour, IChildLoader
{

    CellChild cellChild;
    public bool movable = true;
    bool isMoving = false;
    public float moveDuration = 0.2f;
    [HideInInspector]
    public Cell fatherCell;
    [HideInInspector]
    public Vector3 targetPos = new Vector3();
    public Direction faceTo;

    public UnityEvent AfterDeath = new UnityEvent();
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
        
    }
    public void MoveCheck()
    {
        //TODO:隔壁Cell的也要判断
        Player player = FindObjectOfType<Player>();
        if (!player) { Debug.LogError("No Player in" + this.name + "'s Cell"); return; }
        Vector2Int playerPos =  player.TargetPosInCell();
        Vector2Int pos = this.PosInCell();
        int distance = int.MaxValue;
        //Debug.Log(playerPos + " " + pos);
        if(playerPos.x == pos.x)
        {
            distance = playerPos.y - pos.y;
            if (distance > 0 && player.faceTo == Direction.down) Move(Direction.down);
            else if (distance == 1) Move(Direction.down);
            else if (distance >= 3) Move(Direction.up);
            //距离为2时不动
            else if (distance < 0 && player.faceTo == Direction.up) Move(Direction.up);
            else if (distance == -1) Move(Direction.up);
            else if (distance <= -3) Move(Direction.down);
        }
        else if(playerPos.y == pos.y)
        {
            distance = playerPos.x - pos.x;
            if (distance > 0 && player.faceTo == Direction.left) Move(Direction.left);
            else if (distance == 1) Move(Direction.left);
            else if (distance >= 3) Move(Direction.right);
            //距离为2时不动
            else if (distance < 0 && player.faceTo == Direction.right) Move(Direction.right);
            else if (distance == -1) Move(Direction.right);
            else if (distance <= -3) Move(Direction.left);
        } 
    }
    public Vector2Int PosInCell()
    {
        Vector2Int pos = new Vector2Int();
        pos.x = (int)((transform.position.x - fatherCell.origin.position.x) / WorldGrid.Instance.sideSize);
        pos.y = (int)((transform.position.z - fatherCell.origin.position.z) / WorldGrid.Instance.sideSize);
        return pos;
    }

    private void Move(Direction direction)
    {
        if (isMoving || !movable)
        {
            //TODO:连续移动
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
            else
            {
                //Debug.Log(this.gameObject.name + "无法移动或进入死亡过程"); 
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
                targetCell.groundInfo[targetPos.x, targetPos.y] = child.type;
                fatherCell = targetCell;
                child.transform.parent = targetCell.gameObject.transform;
                return true;
            }
            else
            {
                //不跨越Cell
                targetCell.groundInfo[originalPos.x, originalPos.y] = GridObjectType.None;
                targetCell.groundInfo[targetPos.x, targetPos.y] = child.type;
                return true;
            }
        }
        //怪物的特殊情况，如果是怪物，可以跳坑
        else if (child.type == GridObjectType.Enemy && targetCell.groundInfo[targetPos.x, targetPos.y] == GridObjectType.Abyss)
        {
            targetCell.groundInfo[originalPos.x, originalPos.y] = GridObjectType.None;
            child.GetComponent<Monster>().Death(direction);
            return false;
        }
        Debug.Log(targetCell.name + "的" + targetPos + "格有" + targetCell.groundInfo[targetPos.x, targetPos.y]);
        return false;   //目标格子有东西
    }
    IEnumerator MoveProcess(Direction direction)
    {
        Debug.Log("Monster Move");
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
        yield return new WaitForSeconds(moveDuration);
        isMoving = false;
    }
    public void Death(Direction direction)
    {
        StartCoroutine(DeathProcess(direction));
    }
    IEnumerator DeathProcess(Direction direction)
    {
        Debug.Log(this.name + "Death");
        isMoving = true;
        movable = false;
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
        yield return new WaitForSeconds(0.1f);
        this.transform.gameObject.GetComponent<Animator>().Play("Death");
        //Debug.Log("Anim");
        yield return new WaitForSeconds(0.1f);
        AfterDeath.Invoke();
        isMoving = false;
    }
    public void DestroyThis()
    {
        Destroy(this);
    }
}
