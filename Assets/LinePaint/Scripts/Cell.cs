using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private MeshRenderer cellCenterPaint;

    private Vector2Int cellCoords;

    public Vector2Int CellCoords { get => cellCoords; set => cellCoords = value; }
    public MeshRenderer CellCenterPaint { get => cellCenterPaint; }
}
