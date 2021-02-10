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
    //更合理的方案应该是，i,j-格子物体索引-groundInfo；这样可以方便通过i，j找到对应的物体

    private int length;
    private int width;

    //Jigsaw
    public LayerMask anchorLayerMask;
    private Vector3 originalPos;
    public bool draggable = true;
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
    public void SmoothMoveTo(Vector3 target)
    {
        transform.DOMove(target, 0.5f);
    }
    #region 大地图拖动
    public void JigsawMode(bool b)
    {
        if (b)
        {
            if (transform.GetComponentInChildren<Player>()) draggable = false;
            else draggable = true;
        }
        else draggable = false;
    }
    private void OnMouseDown()
    {
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
    #endregion

    private IEnumerator CaptureByRect(Rect mRect, string mFileName)
    {
        //等待渲染线程结束  
        yield return new WaitForEndOfFrame();
        //初始化Texture2D  
        Texture2D mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGB24, false);
        //读取屏幕像素信息并存储为纹理数据  
        mTexture.ReadPixels(mRect, 0, 0);
        mTexture.Apply();
        //将图片信息编码为字节信息  
        byte[] bytes = mTexture.EncodeToPNG();
        //保存  
        System.IO.File.WriteAllBytes(mFileName, bytes);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Rect rect = new Rect(200f, 200f, 200f, 200f);
            StartCoroutine(CaptureByRect(rect, Application.dataPath + " Time.time" + ".png"));
            ScreenCapture.CaptureScreenshot(Application.dataPath + Time.time + "Shot.png");
            Debug.Log("Shot");
        }
    }
}
