using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIJigsaw : MonoBehaviour
{
    public static UIJigsaw Instance;
    public float sideSizeUI = 200f;
    public UICell[,] UIJigsawMap;
    public Transform waitingArea;
    public Transform originPos;
    // Start is called before the first frame update
    private void Awake()
    {
        if (FindObjectOfType<UIJigsaw>() != this) Destroy(this);
        else Instance = this;
    }
    void Start()
    {
        UIJigsawMap = new UICell[WorldGrid.Instance.horizontalCells, WorldGrid.Instance.verticalCells];
        foreach (var cell in GetComponentsInChildren<UICell>())
        {
            Vector2Int cellPos = PosInJigsaw(cell);
            if (cellPos.x >= 0 && cellPos.x <= WorldGrid.Instance.horizontalCells && cellPos.y >= 0 && cellPos.y <= WorldGrid.Instance.verticalCells)
                UIJigsawMap[cellPos.x, cellPos.y] = cell;
            Debug.Log(cell.name + " is at " + cellPos);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            foreach (var cell in GetComponentsInChildren<UICell>())
            {
                Vector2Int cellPos = PosInJigsaw(cell);
                Debug.Log(cell.name + " is at " + cellPos);
            }
        }

    }
    public Vector2Int PosInJigsaw(UICell cell)
    {
        Vector2Int cellPos = new Vector2Int();
        cellPos.x = (int)((cell.transform.position.x - originPos.position.x) / sideSizeUI);
        cellPos.y = (int)((cell.transform.position.y - originPos.position.y) / sideSizeUI);
        if (cellPos.x >= 0 && cellPos.x < WorldGrid.Instance.horizontalCells &&
            cellPos.y >= 0 && cellPos.y < WorldGrid.Instance.verticalCells) return cellPos;
        else return new Vector2Int(-1, -1); //表示不在范围内，cell在等候区
    }
    public Vector3 PosInWorld(Vector2Int cellPos)
    {
        Vector3 worldPos = new Vector3();
        worldPos.x = originPos.position.x + sideSizeUI * cellPos.x;
        worldPos.y = originPos.position.y + sideSizeUI * cellPos.y;
        worldPos.z = 0;
        return worldPos;
    }
}
