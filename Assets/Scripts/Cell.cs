using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public enum GridObjectType
{
    NotUsed,
    None,
    Enemy,
    Player,
    NPC,
    Stone,
    Abyss
}

public class Cell : MonoBehaviour
{
    //
    public static Dictionary<Direction, Vector2Int> DirectionVector = new Dictionary<Direction, Vector2Int>()
    {
    {Direction.left, Vector2Int.left },
    {Direction.right, Vector2Int.right },
    {Direction.up, Vector2Int.up},
    {Direction.down , Vector2Int.down}
    };


    public Transform origin;
    public Vector2Int cellPosInGrid = new Vector2Int(-1, -1);
    [HideInInspector]
    public int[,] cellGrid;    //是否有地板：1有，0无
    public GridObjectType[,] groundInfo;  //地板上有什么东西

    private int length;
    private int width;

    //Jigsaw
    public LayerMask anchorLayerMask;
    private Vector3 originalPos;
    public bool draggable = true;
    // Start is called before the first frame update
    private void Awake()
    {
        length = 11;
        width = 11;
        cellGrid = new int[length, width];
        groundInfo = new GridObjectType[length, width];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                cellGrid[i, j] = 0;
                groundInfo[i, j] = GridObjectType.NotUsed;
            }
        }
    }
    void Start()
    {
        WorldGrid.Instance.SetCellPos(this, cellPosInGrid);    //在世界中登记cell位置
        foreach (var child in transform.GetComponentsInChildren<CellChild>())
        {
            Vector2Int pos = PosInCell(child.transform);
            if (child.isGroundTile)
            {
                cellGrid[pos.x, pos.y] = 1;
                groundInfo[pos.x, pos.y] = GridObjectType.None;
                continue;
            }
            else groundInfo[pos.x, pos.y] = child.type;
        }
    }

    public Vector2Int PosInCell(Transform target)
    {
        Vector2Int pos = new Vector2Int();
        pos.x = (int)((target.position.x - origin.position.x) / WorldGrid.Instance.sideSize);
        pos.y = (int)((target.position.z - origin.position.z) / WorldGrid.Instance.sideSize);
        return pos;
    }

    public bool CanMoveThis(CellChild child, Direction direction)
    {
        Cell fatherCell = this;
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
            Debug.Log(targetPos);
            Debug.Log(targetCell.name);
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
                child.GetComponent<Player>().fatherCell = targetCell;
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
        else if (child.type == GridObjectType.Enemy && targetCell.groundInfo[targetPos.x, targetPos.y] == GridObjectType.Abyss)
        {
            //如果是怪物，可以跳坑
            targetCell.groundInfo[originalPos.x, originalPos.y] = GridObjectType.None;
            child.GetComponent<Monster>().Death(direction);
            return false;
        }
        Debug.Log(targetCell.name + "的" + targetPos + "格有" + targetCell.groundInfo[targetPos.x, targetPos.y]);
        return false;   //目标格子有东西
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector3 pos = origin.position;
                pos.x += i * WorldGrid.Instance.sideSize;
                pos.z += j * WorldGrid.Instance.sideSize;
                if (groundInfo[i, j] == GridObjectType.None)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(pos, 0.1f);
                }
                else if (groundInfo[i, j] == GridObjectType.Stone)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(pos, 0.1f);
                }
                else if (groundInfo[i, j] == GridObjectType.Abyss)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(pos, 0.1f);
                }
                else if (groundInfo[i, j] == GridObjectType.Enemy)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(pos, 0.1f);
                }
            }
        }
    }

    public Cell GetNextCell(Direction direction)
    {
        return WorldGrid.Instance.GetCell(cellPosInGrid + DirectionVector[direction]);
    }
    public void LatePlayerMove()
    {
        Dictionary<Monster, float> monsters = new Dictionary<Monster, float>();
        Player player = transform.GetComponentInChildren<Player>();
        foreach (var child in transform.GetComponentsInChildren<Monster>())
        {
            Debug.Log(child.PosInCell() + " " + player.TargetPosInCell());
            float distance = Vector2Int.Distance(child.PosInCell(), player.TargetPosInCell());
            monsters.Add(child, distance);
        }

        //从小到大排序
        var sortedMonsters = (from pair in monsters orderby pair.Value select pair).ToList();

        //通知所有子Enemy去检测是否需要移动。
        foreach(var child in sortedMonsters)
        {
            child.Key.MoveCheck();
        }
    }
    private void OnMouseDown()
    {
        if (transform.GetComponentInChildren<Player>()) draggable = false;
        else draggable = true;

        if (draggable)
        {

            originalPos = WorldGrid.Instance.PosInWorld(cellPosInGrid);
            WorldGrid.Instance.SetCellAnchors(true);
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));
            SmoothMoveTo(new Vector3(mousePos.x, transform.position.y, mousePos.z));
        }
    }
    private void OnMouseDrag()
    {
        if (draggable)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));
            SmoothMoveTo(new Vector3(mousePos.x, transform.position.y, mousePos.z));
        }

    }
    private void OnMouseUp()
    {
        if (draggable)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));
            Vector3 origin = Camera.main.transform.position;
            Vector3 direction = mousePos - origin;
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, 100f, anchorLayerMask))
            {
                SmoothMoveTo(hit.transform.position);
                Debug.DrawLine(origin, origin + direction.normalized * 50f, Color.red, 1f);
                Debug.Log(hit.transform.gameObject.name);
                WorldGrid.Instance.SetCellPos(this, hit.transform.GetComponent<CellAnchor>().cellPosInWorld);
            }
            else
            {
                SmoothMoveTo(originalPos);
            }
            WorldGrid.Instance.SetCellAnchors(false);
        }
    }
    public void SmoothMoveTo(Vector3 target)
    {
        transform.DOMove(target, 0.5f);
    }
}
