using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Space(4)]
    [Header("[SETTINGS]")]
    [Range(1, 4000)] [SerializeField] private int _currentLevel = 1;
    [SerializeField] private List<LevelDataBase> _scenesData;

    [Space(10)]
    [Header("[DATA]")]
    [SerializeField] private int _currentBottlesFullFill = 0;
    [SerializeField] private List<GameObject> _bottleList;
    [SerializeField] private int _currentTotalStep = 0;
    [SerializeField] private List<int> _bestTotalStepList;
    [SerializeField] private List<string> _stepGuideList;
    [SerializeField] private string _historyStep;

    [Space(12)]
    [Header("[REFERENCES]")]
    [SerializeField] private SaveData _saveData;
    [SerializeField] private UtilityManager _uManager;

    [Space(12)]
    [Header("[REFERENCES UI]")]
    [SerializeField] private Canvas _objectCanvas;
    [SerializeField] private TextMeshProUGUI _txtCurrentLevel;
    [SerializeField] private TextMeshProUGUI _txtCurrentTotalStep;
    [SerializeField] private TextMeshProUGUI _txtStepGuide;
    [SerializeField] private GameObject _txtPauseNotification;
    [SerializeField] private GameObject _btnPause;
    [SerializeField] private GameObject _btnContinue;

    /// <summary>
    /// Properties
    /// </summary>
    public int CurrentLevel { get => _currentLevel; }
    public List<LevelDataBase> ScenesData { get => _scenesData; }
    public List<string> StepGuideList { get => _stepGuideList; set => _stepGuideList = value; }
    public List<GameObject> BottleList { get => _bottleList; set => _bottleList = value; }
    public List<int> BestTotalStepList { get => _bestTotalStepList; set => _bestTotalStepList = value; }

    // /// <summary>
    // /// SingleTon Parttern
    // /// </summary>
    // private static GameManager inst;
    // public static GameManager Inst { get => inst; }
    // Start is called before the first frame update

    void Start()
    {
        if (_currentLevel > 0)
        {
            IntitialObjectInScene(_currentLevel);
            ResetStep();
        }
        else
            Debug.LogError($"Start Level must be > 0");
    }

    // void ManagerSingleTon()
    // {
    //     if (inst != null)
    //     {
    //         gameObject.SetActive(false);
    //         Destroy(gameObject);
    //     }
    //     else
    //     {
    //         inst = this;
    //         DontDestroyOnLoad(gameObject);
    //     }
    // }

    private void IntitialObjectInScene(int currentLevel)
    {
        //print($"Intial with currentLevel {currentLevel}");
        if (currentLevel > _scenesData.Count)
        {
            Debug.LogError($"Level Over!!");
            return;
        }

        _txtCurrentLevel.text = $"LEVEL {currentLevel}";

        LevelDataBase scene = _scenesData[currentLevel - 1];
        if (scene.BottleList.Count > 0)
        {
            _txtStepGuide.text = _stepGuideList[currentLevel - 1];
            for (int i = 0; i < scene.BottleList.Count; i++)
            {
                GameObject bottle = Instantiate(scene.BottleList[i], _objectCanvas.transform);
                bottle.name = bottle.name.Replace(" Variant(Clone)", "");
                //bottle.GetComponent<RectTransform>().localPosition = _uManager.posDB.posOfBottlesEachLevels[scene.BottleList.Count].posOfBottles[i];
                _bottleList.Add(bottle);
            }
        }
        else
            Debug.LogError($"Scene at {currentLevel} level has bottleList empty!");
    }

    public void CheckConditionToWin()
    {
        _currentBottlesFullFill++;
        if (_currentBottlesFullFill == _scenesData[_currentLevel - 1].ConditionToWin)
        {
            _uManager.SoundManager.PlayWinSFX(Camera.main.transform.position);
            SaveTotalStep();
            StartCoroutine(WaitSecondAndNextLevel());
        }
    }

    IEnumerator WaitSecondAndNextLevel()
    {
        yield return new WaitForSeconds(2.75f);
        NextLevel();
    }

    //For btnNext
    public void NextLevel()
    {
        foreach (GameObject bottle in _bottleList)
            Destroy(bottle);

        ResetStep();
        StopAllCoroutines();
        _bottleList.Clear();
        _currentBottlesFullFill = 0;
        _currentLevel++;
        IntitialObjectInScene(_currentLevel);
        ResetStep();
    }

    //For btnPrevious
    public void PreviousLevel()
    {
        if (_currentLevel == 1)
            return;

        foreach (GameObject bottle in _bottleList)
            Destroy(bottle);

        StopAllCoroutines();
        _bottleList.Clear();
        _currentBottlesFullFill = 0;
        _currentLevel--;
        IntitialObjectInScene(_currentLevel);
        ResetStep();
    }

    //For btnReload
    public void ReloadAtCurrentLevel()
    {
        //print($"Reload");
        //ContinueGame();
        foreach (GameObject bottle in _bottleList)
            Destroy(bottle);

        _bottleList.Clear();
        _currentBottlesFullFill = 0;
        IntitialObjectInScene(_currentLevel);

    }

    //For btnPause
    public void PauseGame()
    {
        Time.timeScale = 0;
        _btnPause.SetActive(false);
        _txtPauseNotification.SetActive(true);
        _btnContinue.SetActive(true);
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
        _btnPause.SetActive(true);
        _txtPauseNotification.SetActive(false);
        _btnContinue.SetActive(false);
    }

    public void StepIncr(string bottle1, string bottle2)
    {
        _currentTotalStep++;
        _txtCurrentTotalStep.text = $"{_currentTotalStep} / {_bestTotalStepList[_currentLevel - 1]}";

        if (_currentTotalStep == 1)
            _historyStep += $"{_currentTotalStep}. {bottle1} -> {bottle2}";
        else
            _historyStep += $"\n{_currentTotalStep}. {bottle1} -> {bottle2}";
    }

    public void ResetStep()
    {
        _currentTotalStep = 0;
        _historyStep = string.Empty;
        _txtCurrentTotalStep.text = $"{_currentTotalStep} / {_bestTotalStepList[_currentLevel - 1]}";
    }


    public void SaveTotalStep()
    {
        print($"currentTotalStep {_currentTotalStep} <? bestTotalStepList {_bestTotalStepList[_currentLevel - 1]}");
        if (_currentTotalStep < _bestTotalStepList[_currentLevel - 1] && _currentTotalStep != 0)
        {
            print($"Add {_currentTotalStep} at level {_currentLevel}");
            _saveData.SaveTotalStep("bestTotalStep", _currentLevel, _currentTotalStep, _historyStep);
        }
    }
}
