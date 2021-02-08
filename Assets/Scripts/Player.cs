using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Player : MonoBehaviour
{
    public float moveDuration = 0.2f;
    int inputX;
    int inputY;
    bool isMoving = false;
    Cell fatherCell;
    CellChild cellChild;
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
        else if(inputY == -1) Move(Direction.down);
    }
    private void Move(Direction direction)
    {
        if (isMoving)
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
            if (fatherCell.CanMoveThis(cellChild, direction)) StartCoroutine(MoveAnim(direction));
            else { Debug.Log(this.gameObject.name + "无法移动"); }
        }
    }
    IEnumerator MoveAnim(Direction direction)
    {
        isMoving = true;
        Vector3 pos = transform.position;
        switch (direction)
        {
            case Direction.right: pos.x += WorldGrid.Instance.sideSize; break;
            case Direction.left: pos.x -= WorldGrid.Instance.sideSize; break;
            case Direction.up: pos.z += WorldGrid.Instance.sideSize; break;
            case Direction.down: pos.z -= WorldGrid.Instance.sideSize; break;
        }
        transform.DOMove(pos, moveDuration);
        yield return new WaitForSeconds(moveDuration);
        isMoving = false;
    }
}
