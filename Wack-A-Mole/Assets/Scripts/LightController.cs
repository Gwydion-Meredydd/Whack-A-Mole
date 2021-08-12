using System.Collections.Generic;
using System.IO;
using System.Collections;
using System;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public bool startLighting;

    public static string directory = "/JsonData";
    public static string fileName1 = "LightSequenceData";
    public static string fileName2 = "LightTimingData";

    public string CurrentOrderString;
    public List  <int> CurrentOrder;
    public float CurrentDelayTime;
    public float CurrentActiveTime;
    public int LightsTurnedOn;
    public bool LightIsOn;
    public Collider2D CurrentCollider;
    public bool clickedLight;
    public bool failed;
    public GameObject[] inGameUI;
    public GameObject MainMenu;

    private void Start()
    {
        inGameUI[0].SetActive(false);
        inGameUI[1].SetActive(false);
        inGameUI[2].SetActive(false);
    }
    public void LateUpdate()
    {
        if (startLighting) 
        {
            //late update is used to ensure that grid controller is activated before light controller 
            startSequence();
            startLighting = false;
        }
    }
    public void FixedUpdate()
    {
        if (LightIsOn) 
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (CurrentCollider.OverlapPoint(wp))
                {
                    SpriteRenderer spriteRenderer = CurrentCollider.gameObject.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = Color.blue ;
                    clickedLight = true;
                }
                else 
                {
                    clickedLight = false;
                }
            }
        }
        if (failed && !LightIsOn) 
        {

            inGameUI[1].SetActive(true);
            StartCoroutine(FailTimer());
            failed = false;
        }
    }
    public void StartButtonPressed() 
    {
        MainMenu.SetActive(false);
        startLighting = true;
        
    }
    public void startSequence() 
    {
        inGameUI[2].SetActive(true);
        LightSequence currentLightSequence = FetchLightJson();
        LightTiming currentLightTiming = FetchLightTimeJson();
        switch (GameManager.Instance.CurrentGridSize) 
        {
            case 4:
                CurrentOrderString = currentLightSequence.OrderSize4;
                    break;
            case 9:
                CurrentOrderString = currentLightSequence.OrderSize9;
                break;
            case 16:
                CurrentOrderString = currentLightSequence.OrderSize16;
                break;
        }

        //turns the string into a usable list to allow for the light sequancing to be played the same each time 
        string[] splitString = CurrentOrderString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string item in splitString)
        {
            try
            {
                CurrentOrder.Add(Convert.ToInt32(item));
            }
            catch (System.Exception e)
            {
                Debug.Log("Value in string was not an int.");
                Debug.LogException(e);
            }
        }
        CurrentDelayTime = currentLightTiming.LightDelayTime;
        CurrentActiveTime = currentLightTiming.LightingActiveTime;
        LightsTurnedOn = 0;
        StartCoroutine(LightTimer());
    }
    IEnumerator LightTimer()
    {
        //starts processing which light to turn on and when to turn it off and uses the is clicked bool to see if the player has pressed the correct light
        for (int i = 0; i <= GameManager.Instance.CurrentGridSize; i++)
        {
            Debug.Log(i  + "  "  + LightsTurnedOn);
            if (LightsTurnedOn < GameManager.Instance.CurrentGridSize)
            {
                if (CurrentOrder[LightsTurnedOn] == i)
                {
                    clickedLight = false;
                    SpriteRenderer spriteRenderer = GameManager.Instance.SpawnedTiles[i - 1].GetComponent<SpriteRenderer>();
                    spriteRenderer.color = Color.green;
                    LightIsOn = true;
                    CurrentCollider = GameManager.Instance.SpawnedTiles[i - 1].GetComponent<Collider2D>();
                    yield return new WaitForSeconds(CurrentActiveTime);
                    LightIsOn = false;
                    LightsTurnedOn = LightsTurnedOn + 1;
                    spriteRenderer.color = Color.red;
                    if (!clickedLight)
                    {
                        failed = true;
                        StopAllCoroutines();
                    }
                    yield return new WaitForSeconds(CurrentDelayTime);
                }
            }
            yield return null;
        }
        if (LightsTurnedOn < GameManager.Instance.CurrentGridSize)
        {
            StartCoroutine(LightTimer());
        }
        else 
        {
            inGameUI[0].SetActive(true);
            yield return new WaitForSeconds(CurrentDelayTime * 2);
            ResetPuzzle();
        }
    }
    public void ResetPuzzle() 
    {
        foreach (GameObject tiles in GameManager.Instance.SpawnedTiles)
        {
            Destroy(tiles);
        }
        GameManager.Instance.SpawnedTiles.Clear();
        foreach (GameObject GameText in inGameUI)
        {
            GameText.SetActive(false);
        }
        MainMenu.SetActive(true);
        CurrentOrder.Clear();
        CurrentOrderString = "";
        startLighting = false;
        CurrentDelayTime = 0;
        CurrentActiveTime = 0;
        LightsTurnedOn = 0;
        CurrentCollider = null;
        clickedLight = false;
        failed = false;
        GridController.Instance.ResetGrid();
        GameManager.Instance.CurrentGridSize = 0;

    }
    IEnumerator FailTimer()
    {
        yield return new WaitForSeconds(CurrentDelayTime * 2);
        ResetPuzzle();
    }
    public static LightSequence FetchLightJson()
    {
        string fullpath = Application.persistentDataPath + directory + fileName1;
        LightSequence lS = new LightSequence();

        if (File.Exists(fullpath))
        {
            string json = File.ReadAllText(fullpath);
            lS = JsonUtility.FromJson<LightSequence>(json);
            Debug.Log("Light sequence File is found... Location:  " + Application.persistentDataPath);
            return lS;

        }
        else
        {
            //incase file is delted, game will automaticly make a new json file
            Debug.Log("Light sequence  File is Missing... Creating New File in " + Application.persistentDataPath);
            LightSequence newLightSequence = new LightSequence();
            newLightSequence.OrderSize4 = "02 01 04 03";
            newLightSequence.OrderSize9 = "09 04 03 05 01 02 05 06 08 07";
            newLightSequence.OrderSize16 = "11 09 12 04 16 14 03 05 13 01 02 05 15 06 08 07";
            SaveLightJson(newLightSequence);
            return newLightSequence;
        }
    }

    public static void SaveLightJson(LightSequence lS)
    {
        string dir = Application.persistentDataPath + directory;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonUtility.ToJson(lS);
        File.WriteAllText(dir + fileName1, json);
    }
    public static LightTiming FetchLightTimeJson()
    {
        string fullpath = Application.persistentDataPath + directory + fileName2;
        LightTiming lT = new LightTiming();

        if (File.Exists(fullpath))
        {
            string json = File.ReadAllText(fullpath);
            lT = JsonUtility.FromJson<LightTiming>(json);
            Debug.Log("Light Timing File is found... Location:  " + Application.persistentDataPath);
            return lT;

        }
        else
        {
            //incase file is delted, game will automaticly make a new json file
            Debug.Log("Light Timing File is Missing... Creating New File in " + Application.persistentDataPath);
            LightTiming newLightTiming = new LightTiming();
            newLightTiming.LightingActiveTime = 1;
            newLightTiming.LightDelayTime = 1;
            SaveLightTimingJson(newLightTiming);
            return newLightTiming;
        }
    }

    public static void SaveLightTimingJson(LightTiming lT)
    {
        string dir = Application.persistentDataPath + directory;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonUtility.ToJson(lT);
        File.WriteAllText(dir + fileName2, json);
    }
}

[System.Serializable]
public class LightSequence
{
    //number value goes from left to right , each number is sperated by a space
    public string OrderSize4;
    public string OrderSize9;
    public string OrderSize16;
}
[System.Serializable]
public class LightTiming
{
    //default is 1
    public float LightingActiveTime;
    public float LightDelayTime;
}