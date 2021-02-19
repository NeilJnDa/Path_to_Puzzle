using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class CellChild : MonoBehaviour
{
    public bool isGroundTile;
    public GridObjectType type;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Disappear()
    {
        Cell cell = transform.parent.GetComponent<Cell>();
        Vector2Int pos = cell.PosInCell(this.transform);
        cell.groundInfo[pos.x, pos.y] = GridObjectType.None;
        cell.cellObjects[pos.x, pos.y] = null;
        GetComponent<SpriteRenderer>().DOFade(0f, 1f);
        Destroy(this, 1f);
    }
}
