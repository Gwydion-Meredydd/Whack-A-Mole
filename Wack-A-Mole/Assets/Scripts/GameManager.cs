using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int CurrentGridSize;
    public List<GameObject> SpawnedTiles;

    private void Awake()
    {
        Instance = this;
    }
}
