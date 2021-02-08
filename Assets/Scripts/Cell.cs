using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public int XcellIndex = -1; //此cell在拼图中的位置X
    public int YcellIndex = -1; //此cell在拼图中的位置Y
    [HideInInspector]
    public int[,] cellGrid;    //是否有地板：1有，0无
    public GridObjectType[,] groundInfo;  //地板上有什么东西
    public List<GameObject> childrenList;   //子物体列表

    private int length;
    private int width;
    // Start is called before the first frame update
    private void Awake()
    {
        length = WorldGrid.Instance.cellLength;
        width = WorldGrid.Instance.cellWidth;
        cellGrid = new int[length, width];
        groundInfo = new GridObjectType[length, width];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                cellGrid[i, j] = 0;
                groundInfo[i, j] = GridObjectType.NotUsed;
            }
        }
    }
    void Start()
    {
        WorldGrid.Instance.SetCellPos(this, XcellIndex, YcellIndex);    //在世界中登记cell位置
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

    Vector2Int PosInCell(Transform target)
    {
        Vector2Int pos = new Vector2Int();
        pos.x = (int)((target.position.x - origin.position.x) / WorldGrid.Instance.sideSize);
        pos.y = (int)((target.position.z - origin.position.z) / WorldGrid.Instance.sideSize);
        return pos;
    }

    public bool CanMoveThis(CellChild child, Direction direction)
    {
        Vector2Int originalPos = PosInCell(child.transform);
        Vector2Int targetPos = originalPos;
        targetPos += DirectionVector[direction];
        if (cellGrid[targetPos.x, targetPos.y] == 0) return false;
        else if(groundInfo[targetPos.x, targetPos.y] == GridObjectType.None)
        {
            //TODO: 但是动物能移动到深渊
            groundInfo[originalPos.x, originalPos.y] = GridObjectType.None;
            groundInfo[targetPos.x, targetPos.y] = child.type;
            return true;
        }
        return false;

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
                if (cellGrid[i, j] == 1) Gizmos.DrawWireSphere(pos, ((float)groundInfo[i, j] + 1) / 5f);
            }
        }
    }

    private void OnMouseDrag()
    {
        //TODO:拖动
    }
}
