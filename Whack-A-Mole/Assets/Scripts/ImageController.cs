using System.IO;
using System.Collections.Generic;

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ImageController : MonoBehaviour
{
    public static ImageController Instance { get; private set; }

    public static string directory = "/JsonData";
    public static string fileName = "ImageRefrenceData";

    public string CurrentImageRefrenceDirectory;

    public RawImage ImageTemplate;
    public List<Texture2D> LoadedTextures;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //fetches file path from json save    
        RefImage newRefImage = FetchRefImageJson();
        LoadedTextures.Add(Resources.Load(newRefImage.PathName1) as Texture2D);
        LoadedTextures.Add(Resources.Load(newRefImage.PathName2) as Texture2D);
        LoadedTextures.Add(Resources.Load(newRefImage.PathName3) as Texture2D);
    }

    public void SetRefrenceImage()
    {
        //sets one of three images as image display
        ImageTemplate.texture = LoadedTextures[UnityEngine.Random.Range(0 ,LoadedTextures.Count)];
        ImageTemplate.gameObject.SetActive(true);
    }
    public void DisableRefrenceImage() 
    {
        ImageTemplate.gameObject.SetActive(false);
    }
    public static RefImage FetchRefImageJson()
    {
        string fullpath = Application.persistentDataPath + directory + fileName;
        RefImage rI = new RefImage();

        if (File.Exists(fullpath))
        {
            string json = File.ReadAllText(fullpath);
            rI = JsonUtility.FromJson<RefImage>(json);
            Debug.Log("Image Refrence path File is found... Location:  " + Application.persistentDataPath);
            return rI;

        }
        else
        {
            //incase file is delted, game will automaticly make a new json file
            Debug.Log("Grid Size File is Missing... Creating New File in " + Application.persistentDataPath);
            RefImage RefImageSave = new RefImage();
            RefImageSave.PathName1 = "RefrenceImages/RefrenceImage1";
            RefImageSave.PathName2 = "RefrenceImages/RefrenceImage2";
            RefImageSave.PathName3 = "RefrenceImages/RefrenceImage3";
            SaveRefImage(RefImageSave);
            return RefImageSave;
        }
    }

    public static void SaveRefImage(RefImage rI)
    {
        string dir = Application.persistentDataPath + directory;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonUtility.ToJson(rI);
        File.WriteAllText(dir + fileName, json);
    }
}
[System.Serializable]
public class RefImage
{
    //path names to fetch images from resources folder
    public string PathName1;
    public string PathName2;
    public string PathName3;
}
