using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SaveData : MonoBehaviour
{
    [SerializeField] private string _savePath;

    public void SaveTotalStep(string loadName, int currentLevel, int bestTotalStep, string historyStep)
    {
        //Set the data save path
        _savePath = Application.persistentDataPath + $"/{loadName}.json";

        //Create a string named jsonData to store values
        string jsonData = "";

        SaveTotalStep newSaveTotalStep = new SaveTotalStep();
        newSaveTotalStep.level = currentLevel;
        newSaveTotalStep.bestTotalStep = bestTotalStep;
        newSaveTotalStep.stepGuide = historyStep;

        //Covert string to Json data
        jsonData += JsonUtility.ToJson(newSaveTotalStep) + "\n";

        //Write the jsonData to path savePath
        if (File.Exists(_savePath))
            File.AppendAllText(_savePath, jsonData);
        //Write the jsonData to path savePath
        else
            File.WriteAllText(_savePath, jsonData);
    }
}

[Serializable]
public class SaveTotalStep
{
    public int level;
    public int bestTotalStep;
    public string stepGuide;
}
