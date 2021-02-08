using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    left,
    right,
    up,
    down
}
public class WorldGrid : MonoBehaviour
{

    public static WorldGrid Instance { get; set; }  //单例
    public Transform origin;
    [Header("网格信息")]
    public int horizontalCells = 3;
    public int verticalCells = 3;
    public int cellLength = 11;
    public int cellWidth = 11;
    public float sideSize = 1f;

    [HideInInspector]
    public Cell[,] jigsawMap;

    private List<GameObject> cellAnchors = new List<GameObject>();
    private void Awake()
    {
        //单例
        if (FindObjectOfType<WorldGrid>() != this) Destroy(this);
        else Instance = this;
        jigsawMap = new Cell[horizontalCells, verticalCells];

        foreach (Transform child in transform)
        {
            cellAnchors.Add(child.gameObject);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            for (int i = 0; i < horizontalCells; i++)
            {
                for (int j = 0; j < verticalCells; j++)
                {
                    Debug.Log(i + " " + j + " " + jigsawMap[i, j]);
                }
            }
        }
    }

    public void SetCellPos(Cell cell, Vector2Int targetPos)
    {
        if (targetPos.x >= 0 && targetPos.x < horizontalCells && targetPos.y >= 0 && targetPos.y < verticalCells)
        {
            if (jigsawMap[targetPos.x, targetPos.y] == null)    //目标为空
            {
                jigsawMap[cell.cellPosInGrid.x, cell.cellPosInGrid.y] = null;
                jigsawMap[targetPos.x, targetPos.y] = cell;
                cell.cellPosInGrid = targetPos;
            }
            else if (jigsawMap[targetPos.x, targetPos.y] == cell) return;   //目标就是自己，原地拖动
            else    //和目标换位置
            {
                if (jigsawMap[targetPos.x, targetPos.y].draggable == false)
                {
                    cell.SmoothMoveTo(PosInWorld(cell.cellPosInGrid));
                    return; //目标cell含有Player，不可交换
                }
                Cell targetCell = jigsawMap[targetPos.x, targetPos.y];
                Vector2Int originalPos = cell.cellPosInGrid;

                //换Cell内的坐标
                cell.cellPosInGrid = targetPos;
                targetCell.cellPosInGrid = originalPos;
                //换WorldGrid内的坐标
                
                jigsawMap[targetPos.x, targetPos.y] = cell;
                jigsawMap[originalPos.x, originalPos.y] = targetCell;

                targetCell.SmoothMoveTo(PosInWorld(targetCell.cellPosInGrid));

            }
        }
    }
    public Cell GetCell(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= horizontalCells || pos.y < 0 || pos.y >= verticalCells) return null;
        return jigsawMap[pos.x, pos.y];
    }
    public void SetCellAnchors(bool b)
    {
        foreach (var anchor in cellAnchors)
        {
            anchor.SetActive(b);
        }
    }
    public Vector3 PosInWorld(Vector2Int posInGrid)
    {
        Vector3 pos = new Vector3();
        pos.x = origin.position.x + (float)posInGrid.x * sideSize * (float)cellLength;
        pos.y = origin.position.y;
        pos.z = origin.position.z + (float)posInGrid.y * sideSize * (float)cellWidth;
        return pos;
    }
}
