using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GridController : MonoBehaviour
{
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

    public List<GameObject> SpawnedTiles;

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

    public void StartButtonPressed(GameObject StartButton) 
    {
        StartButton.SetActive(false);
        buildGrid = true;
    }
    public void QuitButtonPressed() 
    {
        Application.Quit();
    }

    private void SpawnGrid() 
    {

        GridSize currentSavedGridSize = FetchSizeJson(CreateNewGridSaveFileOnBuild);

        GameObject refClickableTile = null;
        switch (currentSavedGridSize.GridSizeCollum) 
        {
            case 2:
                refClickableTile = (GameObject)Instantiate(Resources.Load("Clickable Square Big"));
                break;
            case 3:
                refClickableTile = (GameObject)Instantiate(Resources.Load("Clickable Square Meidum"));
                break;
            case 4:
                refClickableTile = (GameObject)Instantiate(Resources.Load("Clickable Square Small"));
                break;
        }

        for (int row = 0; row < currentSavedGridSize.GridSizeRows; row++)
        {
            for (int collum = 0; collum < currentSavedGridSize.GridSizeCollum; collum++)
            {
                GameObject square =  (GameObject) Instantiate(refClickableTile, transform);
                SpawnedTiles.Add(square);
                float posX = collum * spacing;
                float posY = row * -spacing;

                square.transform.position = new Vector2(posX, posY);

            }
        }
        Destroy(refClickableTile);

        float gridW = currentSavedGridSize.GridSizeCollum * spacing;
        float gridH = currentSavedGridSize.GridSizeRows * spacing;

        transform.position = new Vector2(-gridW / 2 + spacing / 2, gridH / 2 - spacing / 2);

        gridHasBeenBuilt = true;
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
