using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BottleController : MonoBehaviour
{
    [Space(4)]
    [Header("[SETTINGS BEFORE RUN]")]
    [SerializeField] public List<UtilityManager.LiquidColor> layersOfLiquid;

    [Space(12)]
    [Header("[INFOMATION]")]
    [SerializeField] private UtilityManager.StateType _stateBottle = UtilityManager.StateType.Idlle;
    [SerializeField] private bool _isOnStepGuide = false;
    [SerializeField] private List<GameObject> _liquidObjects;
    private UtilityManager.LiquidColor _surfaceLiquidColor;
    [SerializeField] private RectTransform _rtSurfaceLiquid;
    private UtilityManager.LiquidColor _lastLiquidColor;

    [Space(12)]
    [Header("[FOR MOVING UP & DOWN]")]
    [SerializeField] private Vector3 _moveUpPos;
    [SerializeField] private Vector3 _startLocalPosBottle;
    private AnimationCurve _moveUpCurve;
    [SerializeField] private float _elapsedTimeChoose = 0;

    [Space(12)]
    [Header("[MOVE AND POUR STEP 1]")]
    //Move down mid-LiquidLayer when first-Bottle pour
    [SerializeField] private List<RectTransform> _rtMidLayerList;
    [SerializeField] private List<Vector3> _startPosMidLayerList;
    [SerializeField] private List<Vector3> _endPosMidLayerList;
    [SerializeField] private List<AnimationCurve> _aniMidLayerMoveDownList;
    [SerializeField] private List<AnimationCurve> _aniMidLayerMoveUpList;

    private bool _pouredLiquidMoveUpDone = false;
    private GameObject _pouredLiquidAtSecondBottle;
    private Vector3 _rawPosForPour;
    private Vector3 _startPosLiquidsParent;
    private Vector3 _targetPosLiquidsParent;
    private Vector3 _startPosLiquid;
    private Vector3 _targetPosLiquid;
    private Vector3Int _targetLocalPosPouredLqInt;
    private Quaternion _startRotation;
    private Quaternion _targetDegrees;
    private Quaternion _threshholdToPouredLiquidMoveUp;
    private AnimationCurve _aniMoveAndPourCurve;
    private AnimationCurve _aniRotateCurveStep1;
    private AnimationCurve _aniLiquidMoveStep1;
    private AnimationCurve _aniLiquidsParentMoveUpStep1;
    private AnimationCurve _aniPouredLiquidMoveUp;

    [Space(12)]
    [Header("[MOVE AND POUR STEP 2]")]
    private AnimationCurve _rotateCurveStep2;
    private AnimationCurve _aniLiquidsParentMoveDownStep2;

    [Space(12)]
    [Header("[REFERENCES]")]
    [SerializeField] RectTransform _rtBottomCollider;
    [SerializeField] private Image _imgEmptyBody;
    [SerializeField] private GameObject _maskFlow;
    [SerializeField] private GameObject _maskLiquids;
    [SerializeField] private GameObject _liquids;
    [SerializeField] private RectTransform _rtTopPointLq;
    [SerializeField] private RectTransform _rtBottomPointLq;
    [SerializeField] private GameObject _childBigWaveSecondBottle;
    //[SerializeField] private GameObject _dropLets;
    [SerializeField] public FlowController flowController;

    //To SetParent for Liquid
    [SerializeField] private UtilityManager _uti;
    [SerializeField] private RectTransform _rect;
    public Coroutine _MoveUpAndDownCorouVar;

    [Space(12)]
    [Header("[REFERENCE PREFABS]")]
    [SerializeField] public ParticleSystem parBottomAirBubbles;

    /// <summary>
    /// Properties
    /// </summary>
    #region 
    public UtilityManager.StateType StateBottle { get => _stateBottle; }
    public bool IsOnStepGuide { set => _isOnStepGuide = value; }
    public List<GameObject> LiquidObjects { get => _liquidObjects; }
    public UtilityManager.LiquidColor SurfaceLiquidColor { get => _surfaceLiquidColor; set => _surfaceLiquidColor = value; }
    public RectTransform RtSurfaceLiquid { get => _rtSurfaceLiquid; set => _rtSurfaceLiquid = value; }
    public UtilityManager.LiquidColor LastLiquidColor { get => _lastLiquidColor; }
    public float ElapsedTimeChoose { get => _elapsedTimeChoose; set => _elapsedTimeChoose = value; }
    public GameObject PouredLiquidAtSecondBottle { get => _pouredLiquidAtSecondBottle; set => _pouredLiquidAtSecondBottle = value; }
    public Vector3 RawPosForPour { get => _rawPosForPour; }
    public Vector3Int TargetLocalPosPouredLqInt { get => _targetLocalPosPouredLqInt; }
    public RectTransform Rect { get => _rect; }
    public Image ImgEmptyBody { get => _imgEmptyBody; set => _imgEmptyBody = value; }
    public GameObject MaskFlow { get => _maskFlow; }
    public GameObject MaskLiquids { get => _maskLiquids; }
    public GameObject Liquids { get => _liquids; }
    public RectTransform RtTopPointLq { get => _rtTopPointLq; }
    public RectTransform RtBottomPointLq { get => _rtBottomPointLq; }
    public GameObject ChildBigWaveSecondBottle { get => _childBigWaveSecondBottle; set => _childBigWaveSecondBottle = value; }
    #endregion

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != "Editor")
        {
            _uti = FindObjectOfType<UtilityManager>();
            _uti.InitialLayerColorInBottle(layersOfLiquid, this);
        }
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Editor")
            InitialSetPara();
    }

    void Update()
    {
        if (_stateBottle == UtilityManager.StateType.Selected)
            _elapsedTimeChoose += Time.deltaTime;
    }

    void OnMouseDown()
    {
        if (SceneManager.GetActiveScene().name != "Editor")
        {
            if (_uti.FirstBottleClicked == null && _liquidObjects.Count > 0 && _stateBottle == UtilityManager.StateType.Idlle)
            {
                _uti.FirstBottleClicked = this;
                MoveUp();
                //print($"Moveup");
            }

            //Next, choosing second-bottle
            else if (_uti.FirstBottleClicked != null)
            {
                if (_uti.FirstBottleClicked != this && _stateBottle != UtilityManager.StateType.Pouring)
                {
                    BottleController firstBottle = _uti.FirstBottleClicked;
                    BottleController secondBottle = this;

                    //Check conditions: (second-Bottle has same color with first-Bottl || second-Bottle is empty bottle) 
                    //&& (number layer emty of second-Bottle > number layer will be same color on surface of first-Bottle)
                    bool validityPour = (_surfaceLiquidColor == firstBottle._surfaceLiquidColor || _liquidObjects.Count == 0)
                                        && (4 - _liquidObjects.Count) >= _uti.CountLayerSameTypeOnSurface(firstBottle._liquidObjects);
                    if (validityPour)
                    {
                        //Set active collier all liquid for detect flow touch in surface
                        _uti.SetActiveLiquidCollider(_rtSurfaceLiquid.gameObject, true, 0.01f);

                        //Handles moving and dumping of water
                        firstBottle.MoveAndPouringTo(secondBottle);

                        //Incr step
                        _uti.LvManager.StepIncr(firstBottle.name, secondBottle.name);

                        //De-selected first bottle

                        //Clear bottle list for already other bottles
                        _uti.FirstBottleClicked = null;
                    }
                }
            }
        }
    }

    //For initialization and pouring water at the end
    public void InitialSetPara()
    {
        //print($"Init bottle name {this.name} with _rect.rect.height {_rect.rect.height}");
        //print($"_rect.localPosition {_rect.localPosition}");
        _startLocalPosBottle = _rect.localPosition;
        _moveUpPos = new Vector3(_startLocalPosBottle.x
                                , _startLocalPosBottle.y + (_rect.rect.height * _rect.localScale.y * 0.15f)
                                , _startLocalPosBottle.z);

        _rawPosForPour = new Vector3(_startLocalPosBottle.x
                                , _startLocalPosBottle.y + (_rect.rect.height * _rect.localScale.y * 0.25f)
                                , _startLocalPosBottle.z);

        _startRotation = this._rect.localRotation;

        //Get from objectData
        if (_uti != null)
        {
            _moveUpCurve = _uti.animaDB.moveUpCurve;
            _aniMoveAndPourCurve = _uti.animaDB.moveAndPourCurve;
            _aniRotateCurveStep1 = _uti.animaDB.rotateCurveStep1;
            _aniPouredLiquidMoveUp = _uti.animaDB.pouredLiquidMoveUp;
            _rotateCurveStep2 = _uti.animaDB.rotateCurveStep2;
        }
    }

    public void SetParaForPouring(Vector3 argStartPosLiquidsParent, Vector3 argTargetPosLiquidsParent, Vector3 startPosLq, Vector3 targetPosLq
                                , Quaternion targetDeg, Quaternion thresholdDegress
                                , AnimationCurve argliquidMoveStep1
                                , AnimationCurve argLiquidsParentMoveUpStep1, AnimationCurve argLiquidsParentMoveDownStep2)
    {
        ///Get from objectData by index
        //Vector3
        _startPosLiquidsParent = argStartPosLiquidsParent;
        _targetPosLiquidsParent = argTargetPosLiquidsParent;
        _startPosLiquid = startPosLq;
        _targetPosLiquid = targetPosLq;

        //Quaternion
        _targetDegrees = targetDeg;
        _threshholdToPouredLiquidMoveUp = thresholdDegress;

        //Animation Curve
        _aniLiquidMoveStep1 = argliquidMoveStep1;
        _aniLiquidsParentMoveUpStep1 = argLiquidsParentMoveUpStep1;
        _aniLiquidsParentMoveDownStep2 = argLiquidsParentMoveDownStep2;
    }

    public void MoveAndPouringTo(BottleController secondBottle)
    {
        if (_MoveUpAndDownCorouVar != null)
        {
            StopCoroutine(_MoveUpAndDownCorouVar);
            _MoveUpAndDownCorouVar = null;
        }
        _MoveUpAndDownCorouVar = StartCoroutine(MoveAndPouringCoroutine(secondBottle));
    }

    public void MoveUp()
    {
        if (_isOnStepGuide)
            _uti.StepGuide.SetActiveSecondBottle(true);

        if (_MoveUpAndDownCorouVar != null)
            StopCoroutine(_MoveUpAndDownCorouVar);

        _MoveUpAndDownCorouVar = StartCoroutine(MoveUpAndDownCoroutine(transform.localPosition, _moveUpPos));

        _stateBottle = UtilityManager.StateType.Selected;
    }

    public void MoveDown()
    {
        if (_isOnStepGuide)
            _uti.StepGuide.SetActiveSecondBottle(false);

        if (_MoveUpAndDownCorouVar != null)
            StopCoroutine(_MoveUpAndDownCorouVar);

        _MoveUpAndDownCorouVar = StartCoroutine(MoveUpAndDownCoroutine(transform.localPosition, _startLocalPosBottle));
        _stateBottle = UtilityManager.StateType.Idlle;
    }

    IEnumerator MoveUpAndDownCoroutine(Vector3 start, Vector3 end)
    {
        //print($"start {start}, end {end}");
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.005f);

        //Move up bottle when it's selected
        float elapsedTime = 0;
        while (elapsedTime < _uti.animaDB.moveTimeUpAndDown)
        {
            float progress = elapsedTime / _uti.animaDB.moveTimeUpAndDown;
            float curvedProgress = _moveUpCurve.Evaluate(progress);
            _rect.localPosition = Vector3.Lerp(start, end, curvedProgress);
            elapsedTime += Time.deltaTime;
            yield return waitForSeconds;
        }
        _rect.localPosition = end;
        _MoveUpAndDownCorouVar = null;
    }

    IEnumerator MoveAndPouringCoroutine(BottleController secondBottle)
    {
        //First, set state of fist-Bottle
        _stateBottle = UtilityManager.StateType.Pouring;
        //Determine the direction and location to pour
        Vector3 posForPour = _uti.CalculateToPassParaToPouring(this, secondBottle);

        //Set boolen for flow exits or not
        bool flowExist = false;
        _pouredLiquidMoveUpDone = false;

        ////Create flow of liquid
        //StartCoroutine(uManager.CreateFlowLiquid(this, secondBottle));
        Vector3 startPosBottle = transform.localPosition;
        WaitForSeconds waitForSeconds1 = new WaitForSeconds(0.005f);

        //Move firstBottle to the last in Hierachy to its sprite overlap all other sprites
        transform.SetAsLastSibling();

        //Determine surface small-wave at first-Bottle & SetActive
        GameObject surfaceSmallWave = _rtSurfaceLiquid.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        surfaceSmallWave.SetActive(true);

        //Create poured Liquid in second-Bottle
        _pouredLiquidAtSecondBottle = Instantiate(_uti.PrefabDB.liquidPrefab, secondBottle.transform);
        //pouredLiquidList = pouredLiquid ;

        _targetLocalPosPouredLqInt = _uti.CreatePouredLiquidAndReturnTargetLocalPos(this, secondBottle, _pouredLiquidAtSecondBottle);
        //print($"Out Method: targetLocalPosPouredLq {targetLocalPosPouredLqInt.ToString("F2")}");

        int preferIndex = _uti.FindIndexInScriptableObject(_liquidObjects);
        GetMidLiquidLayer(preferIndex);

        GameObject lastLiquid = null;
        //Adding pouredLiquid in liquid-Objects
        if (secondBottle._liquidObjects.Count > 0)
        {
            lastLiquid = secondBottle._liquidObjects[secondBottle._liquidObjects.Count - 1];
            if (lastLiquid.GetComponent<LiquidController>().TypeLiquidColor
            == _pouredLiquidAtSecondBottle.GetComponent<LiquidController>().TypeLiquidColor)
            {
                secondBottle._liquidObjects[secondBottle._liquidObjects.Count - 1] = null;
                StartCoroutine(DestroyLastLiquid(lastLiquid));
            }
        }

        //Number layer will be pour from first-Bottle to second-Bottle
        int numberLayerWillBePour = _uti.CountLayerSameTypeOnSurface(_liquidObjects);
        for (int i = 0; i < numberLayerWillBePour; i++)
        {
            //Add layers will be pour from liquidObjects in second-Bottle
            if (i == numberLayerWillBePour - 1)
                secondBottle._liquidObjects.Add(_pouredLiquidAtSecondBottle);
            else
                secondBottle._liquidObjects.Add(null);
        }
        //print($"targetPosLiquidsParent {_targetPosLiquidsParent.ToString("F2")}");

        float elapsedTime = 0;
        while (elapsedTime < _uti.animaDB.moveAndPourTimeStep1)
        {
            secondBottle._stateBottle = UtilityManager.StateType.Selected;
            float progressMove = elapsedTime / _uti.animaDB.moveAndPourTimeStep1;

            ////-----HANDLE FIRST-BOTTLE----
            //For move first-bottle -> Move the first pitcher of water to the second pitcher to pour
            float curvedProgressMoveBottle = _aniMoveAndPourCurve.Evaluate(progressMove);
            _rect.localPosition = Vector3.Lerp(startPosBottle, posForPour, curvedProgressMoveBottle);

            //For rotate first-bottle -> When approaching the second jar
            //, you will start to tilt the first jar of water to pour
            float curvedProgressRotate = _aniRotateCurveStep1.Evaluate(progressMove);
            _rect.rotation = Quaternion.Lerp(_startRotation, _targetDegrees, curvedProgressRotate);

            //For rotate liquid -> Keep globalRotation of liquid when the first-bottle rotate
            _liquids.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, -0.00001f, 1.00000f);

            float curvedProgressMoveUpLiquidsParent = _aniLiquidsParentMoveUpStep1.Evaluate(progressMove);
            _liquids.GetComponent<RectTransform>().localPosition = Vector3.Lerp(_startPosLiquidsParent, _targetPosLiquidsParent, curvedProgressMoveUpLiquidsParent);

            //For move down liquiq -> The liquid will gradually decrease as the jar is steeped
            float curvedProgressMoveDownLiquid = _aniLiquidMoveStep1.Evaluate(progressMove);
            _rtSurfaceLiquid.localPosition = Vector3.Lerp(_startPosLiquid, _targetPosLiquid, curvedProgressMoveDownLiquid);

            //For move down liquid layer when bottle pour
            MoveDownLiquidLayer(progressMove);

            ////-----HANDLE SECOND-BOTTLE----
            if (_targetDegrees.eulerAngles.z >= 269f && _rect.rotation.eulerAngles.z >= 269)
            {
                float currentRotationZ = 361 - _rect.rotation.eulerAngles.z;
                float thresholdZ = 360 - _threshholdToPouredLiquidMoveUp.eulerAngles.z;
                if (currentRotationZ >= thresholdZ && !flowExist)
                {
                    flowExist = true;
                    //Create flow of liquid
                    StartCoroutine(_uti.CreateFlowLiquid(this, secondBottle));
                }
            }
            else if (_targetDegrees.eulerAngles.z <= 91f)
            {
                float currentRotationZ = 90 - _rect.rotation.eulerAngles.z;
                float thresholdZ = 90 - _threshholdToPouredLiquidMoveUp.eulerAngles.z;
                if (currentRotationZ <= thresholdZ && !flowExist)
                {
                    flowExist = true;
                    StartCoroutine(_uti.CreateFlowLiquid(this, secondBottle));
                }
            }

            elapsedTime += Time.deltaTime;
            yield return waitForSeconds1;
        }
        ////----HANDLE FIRST-BOTTLE----
        //Stop waving
        surfaceSmallWave.SetActive(false);

        flowController.DisableImageFlow();
        Destroy(flowController.gameObject, 3.5f);
        // if (_dropLets != null)
        //     Destroy(_dropLets);

        //Remove surfaceliquid just pouring in first-Bottle
        _liquidObjects.RemoveAt(_liquidObjects.Count - 1);
        Destroy(_rtSurfaceLiquid.gameObject);

        //Set new surface-Liquid
        _rtSurfaceLiquid = _uti.FindRTSurfaceLiquid(_liquidObjects);
        //first-Bottle is empty after pour
        if (_rtSurfaceLiquid == null)
        {
            _uti.SoundManager.PlayFullFilSFX(Camera.main.transform.position);
            _rtSurfaceLiquid = _rtBottomCollider;
        }

        if (_liquidObjects.Count > 0)
            _surfaceLiquidColor = _liquidObjects.Last().GetComponent<LiquidController>().TypeLiquidColor;

        //Check if there are any liquids left in the list or if they are all null element
        _uti.CheckAllNullOrNotInList(_liquidObjects);

        ////-----HANDLE SECOND-BOTTLE----
        Destroy(_childBigWaveSecondBottle, 2);
        secondBottle._stateBottle = UtilityManager.StateType.Idlle;

        //Check if second-Bottle is full or not?
        bool secondBottleIsFullFill = _uti.CheckBottleFullFill(secondBottle._liquidObjects, _targetLocalPosPouredLqInt.y);
        if (secondBottleIsFullFill)
        {
            _uti.SoundManager.PlayFullColorSFX(Camera.main.transform.position);
            secondBottle._stateBottle = UtilityManager.StateType.Selected;
            secondBottle.transform.SetParent(_uti.CanvasFullBottle.transform);

            _uti.SpawnParticleSystem((int)secondBottle._surfaceLiquidColor, secondBottle);
            _uti.LvManager.CheckConditionToWin();
        }

        ////STEP 2: TURN BACK
        elapsedTime = 0;
        while (elapsedTime < _uti.animaDB.moveAndPourTimeStep2)
        {
            float progressMove = elapsedTime / _uti.animaDB.moveAndPourTimeStep2;

            //For moving
            float curvedProgressMoveBottle = _aniMoveAndPourCurve.Evaluate(progressMove);
            _rect.localPosition = Vector3.Lerp(posForPour, _startLocalPosBottle, curvedProgressMoveBottle);

            //For rotate
            float curvedProgressRotate = _rotateCurveStep2.Evaluate(progressMove);
            _rect.rotation = Quaternion.Lerp(_targetDegrees, _startRotation, curvedProgressRotate);

            //For rotate liquid -> Keep globalRotation of liquid when the bottle rotate
            _liquids.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 1.00000f);

            float curvedProgressMoveUpLiquidsParent = _aniLiquidsParentMoveDownStep2.Evaluate(progressMove);
            _liquids.GetComponent<RectTransform>().localPosition = Vector3.Lerp(_targetPosLiquidsParent, _startPosLiquidsParent, curvedProgressMoveUpLiquidsParent);

            //For move up liquid layer when bottle pour
            MoveUpLiquidLayer(progressMove);

            elapsedTime += Time.deltaTime;
            yield return waitForSeconds1;
        }
        _rect.localPosition = _startLocalPosBottle;
        _rect.rotation = _startRotation;
        _liquids.GetComponent<RectTransform>().localPosition = _startPosLiquidsParent;

        ////----HANDLE FIRST-BOTTLE---- 
        _stateBottle = UtilityManager.StateType.Idlle;
        _elapsedTimeChoose = 0;
        ResetMidLayerList();
        //Reset state of bottle
        _MoveUpAndDownCorouVar = null;

        if (_uti.StepGuide.IsStepGuide)
            _uti.StepGuide.NextStepGuide();
    }

    private void GetMidLiquidLayer(int preferIndex)
    {
        if (_liquidObjects.Count > 2)
        {
            for (int i = 1; i < _liquidObjects.Count - 1; i++)
            {
                if (_liquidObjects[i] != null)
                {
                    int multiFactorLocalPosY = _liquidObjects[i].GetComponent<LiquidController>().CurrentIndex;
                    Vector3 endLocalPos = _liquidObjects[i].GetComponent<RectTransform>().localPosition
                                    - new Vector3(0, 28 * multiFactorLocalPosY, 0);

                    int currentIndexLq = _liquidObjects[i].GetComponent<LiquidController>().CurrentIndex;

                    _rtMidLayerList.Add(_liquidObjects[i].GetComponent<RectTransform>());
                    _startPosMidLayerList.Add(_liquidObjects[i].GetComponent<RectTransform>().localPosition);
                    _endPosMidLayerList.Add(endLocalPos);
                    _aniMidLayerMoveDownList.Add(_uti.animaDB.PourAnimations[preferIndex].midLiquidLayerMoveDownStep1[currentIndexLq]);
                    _aniMidLayerMoveUpList.Add(_uti.animaDB.PourAnimations[preferIndex].midLiquidLayerMoveUpStep2[currentIndexLq]);
                }
            }
        }
    }

    private void MoveDownLiquidLayer(float progressMove)
    {
        if (_rtMidLayerList.Count > 0)
        {
            for (int i = 0; i < _rtMidLayerList.Count; i++)
            {
                float curvedProgress = _aniMidLayerMoveDownList[i].Evaluate(progressMove);
                _rtMidLayerList[i].localPosition = Vector3.Lerp(_startPosMidLayerList[i], _endPosMidLayerList[i], curvedProgress);
            }
        }
    }

    private void MoveUpLiquidLayer(float progressMove)
    {
        if (_rtMidLayerList.Count > 0)
        {
            for (int i = 0; i < _rtMidLayerList.Count; i++)
            {
                float curvedProgress = _aniMidLayerMoveUpList[i].Evaluate(progressMove);
                _rtMidLayerList[i].localPosition = Vector3.Lerp(_endPosMidLayerList[i], _startPosMidLayerList[i], curvedProgress);
            }
        }
    }

    private void ResetMidLayerList()
    {
        _rtMidLayerList.Clear();
        _startPosMidLayerList.Clear();
        _endPosMidLayerList.Clear();
        _aniMidLayerMoveUpList.Clear();
    }

    public IEnumerator PouredLiquidMoveUp(RectTransform rtPouredLiquid, Vector3 startPos, Vector3Int targetLocalPosPouredLqInt)
    {
        //_dropLets = dropsWaterObj;
        float elapsedTime = 0;
        //print($"MoveUp with startPos: {startPos}");
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.005f);
        float pourLiquidMoveUpTime = _uti.animaDB.moveAndPourTimeStep1 * 0.6f;
        while (elapsedTime < pourLiquidMoveUpTime)
        {
            float progressMove = elapsedTime / pourLiquidMoveUpTime;
            float curvedProgressMovePouredLiquid = _aniPouredLiquidMoveUp.Evaluate(progressMove);
            rtPouredLiquid.localPosition = Vector3.Lerp(startPos
                                                        , targetLocalPosPouredLqInt
                                                        , curvedProgressMovePouredLiquid);
            elapsedTime += Time.deltaTime;
            yield return waitForSeconds;
        }
        _pouredLiquidMoveUpDone = true;
    }

    private IEnumerator DestroyLastLiquid(GameObject lastLiquid)
    {
        yield return new WaitWhile(() => !_pouredLiquidMoveUpDone);
        if (lastLiquid != null)
            Destroy(lastLiquid);
    }

    public void InsertLiquidObjects(GameObject liquid, UtilityManager.LiquidColor liquidColor)
    {
        _liquidObjects.Insert(0, liquid);
        _lastLiquidColor = liquidColor;
    }

}
