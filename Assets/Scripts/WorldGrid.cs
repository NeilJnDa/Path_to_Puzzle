using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using DG.Tweening;
using System;

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

    //JigsawMode
    public bool jigsawMode;
    public List<CanvasGroup> UIJigsawMapCanvasGroups = new List<CanvasGroup>();

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
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (jigsawMode)
            {
                //退出jigsawMode
                FindObjectOfType<Player>().movable = true;
                foreach (var c in UIJigsawMapCanvasGroups)
                {
                    c.blocksRaycasts = false;
                    DOTween.To(() => c.alpha, x => c.alpha = x, 0f, 1f);
                }
                jigsawMode = false;
                CellPosUpdate();
            }
            else
            {
                //进入jigsawMode
                FindObjectOfType<Player>().movable = false;
                foreach (var c in UIJigsawMapCanvasGroups)
                {
                    c.blocksRaycasts = true;
                    DOTween.To(() => c.alpha, x => c.alpha = x, 1f, 1f);
                }
                jigsawMode = true;
            }

        }
    }

    private void CellPosUpdate()
    {
        //对UIJigsawMap中的信息复制
        for (int i = 0; i < horizontalCells; i++)
        {
            for (int j = 0; j < verticalCells; j++)
            {
                if (UIJigsaw.Instance.UIJigsawMap[i, j] == null) jigsawMap[i, j] = null;
                else
                {
                    jigsawMap[i, j] = UIJigsaw.Instance.UIJigsawMap[i, j].relatingCell;
                    jigsawMap[i, j].cellPosInGrid = new Vector2Int(i, j);
                }
            }
        }
        //UI等候区的信息
        foreach (var uiCell in UIJigsaw.Instance.waitingArea.gameObject.GetComponentsInChildren<UICell>())
        {
            Vector2Int newPosInJigsaw = UIJigsaw.Instance.PosInJigsaw(uiCell);  //身处等候区必定返回（-1 -1）
            uiCell.relatingCell.cellPosInGrid = new Vector2Int(-1, -1);
            uiCell.relatingCell.SmoothMoveTo(PosInWorld(newPosInJigsaw));
        }
        foreach (var cell in jigsawMap)
        {
            if(cell) cell.SmoothMoveTo(PosInWorld(cell.cellPosInGrid));
        }

        //foreach (var uiCell in UIJigsaw.Instance.GetComponentsInChildren<UICell>())
        //{
        //    Vector2Int newPosInJigsaw = UIJigsaw.Instance.PosInJigsaw(uiCell);
        //    SetCellPos(uiCell.relatingCell, newPosInJigsaw);
        //    uiCell.relatingCell.SmoothMoveTo(PosInWorld(newPosInJigsaw));
        //}
        //foreach(var uiCell in UIJigsaw.Instance.waitingArea.gameObject.GetComponentsInChildren<UICell>())
        //{
        //    Vector2Int newPosInJigsaw = UIJigsaw.Instance.PosInJigsaw(uiCell);
        //    SetCellPos(uiCell.relatingCell, newPosInJigsaw);
        //    uiCell.relatingCell.SmoothMoveTo(PosInWorld(newPosInJigsaw));
        //}
    }

    public void SetCellPos(Cell cell, Vector2Int targetPos)
    {
        #region Abandoned Cell交换的判定
        //if (targetPos.x >= 0 && targetPos.x < horizontalCells && targetPos.y >= 0 && targetPos.y < verticalCells)
        //{
        //    if (jigsawMap[targetPos.x, targetPos.y] == null)    //目标为空
        //    {
        //        jigsawMap[cell.cellPosInGrid.x, cell.cellPosInGrid.y] = null;
        //        jigsawMap[targetPos.x, targetPos.y] = cell;
        //        cell.cellPosInGrid = targetPos;
        //    }
        //    else if (jigsawMap[targetPos.x, targetPos.y] == cell) return;   //目标就是自己，原地拖动
        //    else    //和目标换位置
        //    {
        //        if (jigsawMap[targetPos.x, targetPos.y].draggable == false)
        //        {
        //            cell.SmoothMoveTo(PosInWorld(cell.cellPosInGrid));
        //            return; //目标cell含有Player，不可交换
        //        }
        //        Cell targetCell = jigsawMap[targetPos.x, targetPos.y];
        //        Vector2Int originalPos = cell.cellPosInGrid;

        //        //换Cell内的坐标
        //        cell.cellPosInGrid = targetPos;
        //        targetCell.cellPosInGrid = originalPos;
        //        //换WorldGrid内的坐标

        //        jigsawMap[targetPos.x, targetPos.y] = cell;
        //        jigsawMap[originalPos.x, originalPos.y] = targetCell;

        //        targetCell.SmoothMoveTo(PosInWorld(targetCell.cellPosInGrid));

        //    }
        //}
        #endregion 
        if (targetPos.x >= 0 && targetPos.x < horizontalCells && targetPos.y >= 0 && targetPos.y < verticalCells)
        {
            jigsawMap[targetPos.x, targetPos.y] = cell;
            cell.cellPosInGrid = targetPos;
        }
        else if (targetPos.x == -1 && targetPos.y == -1)
        {
            jigsawMap[cell.cellPosInGrid.x, cell.cellPosInGrid.y] = null;
            cell.cellPosInGrid = targetPos;
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
    public Vector2Int PosInJigsaw(Cell cell)
    {
        Vector2Int cellPos = new Vector2Int();
        cellPos.x = (int)((cell.transform.position.x - origin.position.x) / sideSize);
        cellPos.y = (int)((cell.transform.position.y - origin.position.y) / sideSize);
        if (cellPos.x >= 0 && cellPos.x < WorldGrid.Instance.horizontalCells &&
            cellPos.y >= 0 && cellPos.y < WorldGrid.Instance.verticalCells) return cellPos;
        else return new Vector2Int(-1, -1); //表示不在范围内，cell在等候区
    }
}
