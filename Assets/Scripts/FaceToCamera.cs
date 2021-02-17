using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class FaceToCamera : MonoBehaviour
{
    public static Dictionary<Direction, Vector3> DirectionVector = new Dictionary<Direction, Vector3>()
    {
    {Direction.left, Vector3.left },
    {Direction.right, Vector3.right},
    {Direction.up, Vector3.forward},
    {Direction.down , Vector3.back}
    };
    Vector3 basePos = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        basePos = transform.position;
        basePos.y = 0;
        transform.RotateAround(basePos, Vector3.right, Camera.main.transform.rotation.eulerAngles.x - transform.rotation.eulerAngles.x);
    }

}
