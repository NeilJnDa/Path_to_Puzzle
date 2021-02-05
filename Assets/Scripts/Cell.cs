using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int XcellIndex = -1; //此cell在拼图中的位置X
    public int YcellIndex = -1; //此cell在拼图中的位置Y
    [HideInInspector]
    public int[,] cellGrid;    //是否有地板：1有，0无
    public GroundInfo[,] groundInfo;  //地板上有什么东西

    private int length;
    private int width;
    // Start is called before the first frame update
    private void Awake()
    {
        length = WorldGrid.Instance.cellLength;
        width = WorldGrid.Instance.cellWidth;
        cellGrid = new int[length, width];
        groundInfo = new GroundInfo[length, width];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                cellGrid[i, j] = 0;
                groundInfo[i, j] = GroundInfo.NotUsed;
            }
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector3 pos = this.transform.position;
                pos.x += i * WorldGrid.Instance.sideSize;
                pos.z += j * WorldGrid.Instance.sideSize;
                if (cellGrid[i, j] == 0) Gizmos.DrawSphere(pos, 0.2f);
            }
        }
    }
}
