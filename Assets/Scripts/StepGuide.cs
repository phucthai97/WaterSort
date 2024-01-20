using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StepGuide : MonoBehaviour
{
    [Space(4)]
    [Header("[REFERENCES]")]
    [SerializeField] private GameObject _canvasUI;
    [SerializeField] private GameObject _pnStepGuide;
    [SerializeField] private UtilityManager _uti;

    [Space(12)]
    [Header("[MOVE UP && DOWN]")]
    [SerializeField] private float _speed = 120f;
    [SerializeField] private float _height = 20f;
    [SerializeField] private int _repeatTimes = 2;

    [Space(12)]
    [Header("[MOVE TO SECOND BOTTLE]")]
    [SerializeField] private GameObject _indicator;
    [SerializeField] private bool _isStepGuide = false;
    [SerializeField] private int _indexStepGuide = 0;
    [SerializeField] private GameObject _firstBottle;
    [SerializeField] private GameObject _secondBottle;
    [SerializeField] private List<string> _stepGuideListStr;
    private Coroutine _stepGuideCoroutine;

    /// <summary>
    /// Properties
    /// </summary>
    public bool IsStepGuide { get => _isStepGuide;}

    public void StartStepGuide()
    {
        _uti.LvManager.ReloadAtCurrentLevel();
        StartCoroutine(StartStepGuideCoroutine());
    }

    IEnumerator StartStepGuideCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        _isStepGuide = true;
        _pnStepGuide.SetActive(true);
        _indexStepGuide = 0;
        string stepGuideStr = _uti.LvManager.StepGuideList[_uti.LvManager.CurrentLevel - 1];
        _stepGuideListStr = stepGuideStr.Split('\n').ToList();
        DoStepGuide();
    }

    public void NextStepGuide()
    {
        _indexStepGuide++;
        if (_indexStepGuide < _stepGuideListStr.Count)
            DoStepGuide();
    }

    private void DoStepGuide()
    {
        List<string> currentStepGuideStr = _stepGuideListStr[_indexStepGuide].Split(' ').ToList();

        _firstBottle = GameObject.Find(currentStepGuideStr[1]);
        _secondBottle = GameObject.Find(currentStepGuideStr.Last());
        _firstBottle.GetComponent<BottleController>().IsOnStepGuide = true;

        //Set blur bottles
        foreach (GameObject child in _uti.LvManager.BottleList)
        {
            BottleController bottleController = child.GetComponent<BottleController>();
            SetAlphaColorAndColliderBottle(bottleController, 0.37f, false);
        }

        //And hightlight bottles
        SetAlphaColorAndColliderBottle(_firstBottle.GetComponent<BottleController>(), 1, true);

        Vector3 startPos = _firstBottle.GetComponent<RectTransform>().localPosition;
        startPos.y += 70f;
        Vector3 endPos = _secondBottle.GetComponent<RectTransform>().localPosition;
        endPos.y += 70f;

        _stepGuideCoroutine = StartCoroutine(AnimationIndicator(startPos, endPos, 0.05f));
    }

    IEnumerator AnimationIndicator(Vector3 startPos, Vector3 endPos, float startDelay)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.005f);
        _indicator = Instantiate(_uti.PrefabDB.fingerUpPrefab, _canvasUI.transform);
        _indicator.transform.localPosition = startPos;

        //At bottle1
        for (int i = 0; i < _repeatTimes; i++)
        {
            // Di chuyển lên
            while (_indicator.transform.localPosition.y < startPos.y + _height)
            {
                _indicator.transform.localPosition += Vector3.up * _speed * Time.deltaTime;
                yield return null;
            }

            // Di chuyển xuống
            while (_indicator.transform.localPosition.y > startPos.y)
            {
                _indicator.transform.localPosition -= Vector3.up * _speed * Time.deltaTime;
                yield return null;
            }
        }

        for (int i = 0; i < 30; i++)
        {
            UnityEngine.UI.Image imageChild = _indicator.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
            imageChild.color -= new Color(0, 0, 0, 0.025f);
            yield return waitForSeconds;
        }
        Destroy(_indicator);

        yield return new WaitForSeconds(0.2f);

        _indicator = Instantiate(_uti.PrefabDB.fingerUpPrefab, _canvasUI.transform);
        _indicator.transform.localPosition = endPos;
        //At bottle1
        for (int i = 0; i < _repeatTimes; i++)
        {
            // Di chuyển lên
            while (_indicator.transform.localPosition.y < endPos.y + _height)
            {
                _indicator.transform.localPosition += Vector3.up * _speed * Time.deltaTime;
                yield return null;
            }

            // Di chuyển xuống
            while (_indicator.transform.localPosition.y > endPos.y)
            {
                _indicator.transform.localPosition -= Vector3.up * _speed * Time.deltaTime;
                yield return null;
            }
        }

        //Scale out
        for (int i = 0; i < 30; i++)
        {
            UnityEngine.UI.Image imageChild = _indicator.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
            imageChild.color -= new Color(0, 0, 0, 0.025f);
            yield return waitForSeconds;
        }
        Destroy(_indicator);
    }

    public void StopStepGuide()
    {
        _pnStepGuide.SetActive(false);
        _isStepGuide = false;
        if (_stepGuideCoroutine != null)
        {
            if (_indicator != null)
                Destroy(_indicator);
            StopCoroutine(_stepGuideCoroutine);
            _stepGuideCoroutine = null;
        }

        //Set blur bottles
        foreach (GameObject child in _uti.LvManager.BottleList)
        {
            BottleController bottleController = child.GetComponent<BottleController>();
            SetAlphaColorAndColliderBottle(bottleController, 1, true);
        }
    }


    private void SetAlphaColorAndColliderBottle(BottleController bottleController, float alphaValue, bool onCollider)
    {
        //Set active collider
        bottleController.GetComponent<BoxCollider2D>().enabled = onCollider;

        //Set alpha for emptybottle
        Color colorImgBody = bottleController.ImgEmptyBody.color;
        colorImgBody.a = alphaValue + 0.3f;
        bottleController.ImgEmptyBody.color = colorImgBody;

        //Set alpha for liquids in bottle
        foreach (Transform child in bottleController.Liquids.transform)
        {
            LiquidController liquidController = child.gameObject.GetComponent<LiquidController>();
            Color colorImgLiquid = liquidController.ImgLiquid.color;
            colorImgLiquid.a = alphaValue;
            liquidController.ImgLiquid.color = colorImgLiquid;
        }
    }

    public void SetActiveSecondBottle(bool val)
    {
        if (val)
            SetAlphaColorAndColliderBottle(_secondBottle.GetComponent<BottleController>(), 1, val);
        else
            SetAlphaColorAndColliderBottle(_secondBottle.GetComponent<BottleController>(), 0.37f, !val);
    }
}