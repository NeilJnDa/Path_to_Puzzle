using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/CellInfo")]
public class CellInfo : ScriptableObject
{
    public int[,] cellGrid;
    public int width;
}
