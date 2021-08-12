using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    [SerializeField]
    [Range(1.1f, 2f)]
    private float spacing;

    [SerializeField]
    [Space]
    public bool CreateNewGridSaveFileOnBuild;
    public bool buildGrid;
    public bool gridHasBeenBuilt;

    public static string directory = "/JsonData";
    public static string fileName = "GridSizeData";

    public Text GridMenuText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!gridHasBeenBuilt)
        {
            if (buildGrid)
            {
                buildGrid = false;
                SpawnGrid();
            }
        }
    }

    #region MainMenu
    public void StartButtonPressed() 
    {
        buildGrid = true;
    }
    public void QuitButtonPressed() 
    {
        Application.Quit();
    }
    public void ChangedGridScale(int ButtonInput)
    {
        GridSize gs = new GridSize();
        switch (ButtonInput)
        {
            case 1:
                GridMenuText.text = "Current Scale 2x2";
                gs.GridSizeCollum = 2;
                gs.GridSizeRows = 2;
                break;
            case 2:
                GridMenuText.text = "Current Scale 3x3";
                gs.GridSizeCollum = 3;
                gs.GridSizeRows = 3;
                break;
            case 3:
                GridMenuText.text = "Current Scale 4x4";
                gs.GridSizeCollum = 4;
                gs.GridSizeRows = 4;
                break;
        }
        SaveSizeJson(gs);
    }
    #endregion

    private void SpawnGrid() 
    {

        GridSize currentSavedGridSize = FetchSizeJson(CreateNewGridSaveFileOnBuild);

        GameObject refClickableTile = null;
        //difrent sizes of prefab to make the overall look of the tiles better
        switch (currentSavedGridSize.GridSizeCollum) 
        {
            case 2:
                refClickableTile = (GameObject)Instantiate(Resources.Load("Clickable Square Big"));
                break;
            case 3:
                refClickableTile = (GameObject)Instantiate(Resources.Load("Clickable Square Medium"));
                break;
            case 4:
                refClickableTile = (GameObject)Instantiate(Resources.Load("Clickable Square Small"));
                break;
        }
        GameManager.Instance.CurrentGridSize = currentSavedGridSize.GridSizeCollum * currentSavedGridSize.GridSizeRows;


        //spawns the tiles
        for (int row = 0; row < currentSavedGridSize.GridSizeRows; row++)
        {
            for (int collum = 0; collum < currentSavedGridSize.GridSizeCollum; collum++)
            {
                GameObject square =  (GameObject) Instantiate(refClickableTile, transform);
                GameManager.Instance.SpawnedTiles.Add(square);
                float posX = collum * spacing;
                float posY = row * -spacing;

                square.transform.position = new Vector2(posX, posY);

            }
        }
        Destroy(refClickableTile);

        //centers the tiles
        float gridW = currentSavedGridSize.GridSizeCollum * spacing;
        float gridH = currentSavedGridSize.GridSizeRows * spacing;

        transform.position = new Vector2((-gridW / 2 + spacing / 2)+  2, gridH / 2 - spacing / 2);

        gridHasBeenBuilt = true;
    }
    //called from light controller reset method
    public void ResetGrid() 
    {
        CreateNewGridSaveFileOnBuild = false;
        buildGrid = false;
        gridHasBeenBuilt = false;
        transform.position = new Vector3(0, 0, 0);
    }
    public static GridSize FetchSizeJson(bool NewSave)
    {
        string fullpath = Application.persistentDataPath + directory + fileName;
        GridSize gS = new GridSize();

        if (File.Exists(fullpath) && !NewSave)
        {
            string json = File.ReadAllText(fullpath);
            gS = JsonUtility.FromJson<GridSize>(json);
            Debug.Log("Grid Size File is found... Location:  " + Application.persistentDataPath);
            return gS;

        }
        else
        {
            //incase file is delted, game will automaticly make a new json file
            Debug.Log("Grid Size File is Missing... Creating New File in " + Application.persistentDataPath);
            GridSize newGridSize = new GridSize();
            newGridSize.GridSizeCollum = 2;
            newGridSize.GridSizeRows = 2;
            SaveSizeJson(newGridSize);
            return newGridSize;
        }
    }

    public static void SaveSizeJson(GridSize gS)
    {
        string dir = Application.persistentDataPath + directory;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonUtility.ToJson(gS);
        File.WriteAllText(dir + fileName, json);
    }
}

[System.Serializable]
public class GridSize 
{
    public int GridSizeCollum;
    public int GridSizeRows;
}
