using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class LoadData : MonoBehaviour
{
    private string _loadPath;
    [SerializeField] private LevelManager _lvManger;

    // Start is called before the first frame update
    void Start()
    {
        _lvManger.BestTotalStepList = Enumerable.Repeat(999, _lvManger.ScenesData.Count).ToList();
        _lvManger.StepGuideList = Enumerable.Repeat(string.Empty, _lvManger.ScenesData.Count + 10).ToList();
        LoadBestTotalStep();    
    }

    public void LoadBestTotalStep()
    {
        //Set the data load path
        _loadPath = Application.persistentDataPath + $"/bestTotalStep.json";

        // Check JSON file exits or not?
        if (File.Exists(_loadPath))
        {
            //Read jsonData
            string[] jsonDataArray = File.ReadAllLines(_loadPath);
            foreach (string line in jsonDataArray)
            {
                if (line.Contains("bestTotalStep"))
                {
                    SaveTotalStep saveTotalStep = JsonUtility.FromJson<SaveTotalStep>(line);
                    ReplaceBestTotalStep(saveTotalStep);
                }
            }
        }
    }

    private void ReplaceBestTotalStep(SaveTotalStep argSaveTotalStep)
    {
        if(argSaveTotalStep.level <= _lvManger.BestTotalStepList.Count)
        {
            _lvManger.BestTotalStepList[argSaveTotalStep.level - 1] = argSaveTotalStep.bestTotalStep;
            _lvManger.StepGuideList[argSaveTotalStep.level - 1] = argSaveTotalStep.stepGuide;
        }
    }
}
