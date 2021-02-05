using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundInfo
{
    NotUsed,
    None,
    Enemy,
    Player,
    NPC,
    Stone,
    Abyss
}

public class WorldGrid : MonoBehaviour
{
    
    public static WorldGrid Instance { get; set; }  //单例
    [Header("网格信息")]
    public int horizontalCells = 3;
    public int verticalCells = 3;
    public int cellLength = 11;
    public int cellWidth = 11;
    public float sideSize = 1f;

    private void Awake()
    {
        //单例
        if (FindObjectOfType<WorldGrid>() != this) Destroy(this);
        else Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
