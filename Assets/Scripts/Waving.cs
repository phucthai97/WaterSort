using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Waving : MonoBehaviour
{
    [Space(4)]
    [Header("[GENERAL SETTING]")]
    //For apply color of wave
    [SerializeField] private List<UnityEngine.UI.Image> imgWaveList;
    [SerializeField] private RectTransform _rtWaves;

    [Space(12)]
    [Header("[MOVE BIG WAVE]")]
    //For moving smooth
    [SerializeField] private bool _isBigWave = false;
    [SerializeField] private Vector3 _moveBy;
    [Range(0.2f, 5f)][SerializeField] private float _duration = 1f;
    [SerializeField] private AnimationCurve _curve;

    [Space(12)]
    [Header("[MOVE SMALL WAVE]")]
    [SerializeField] private float _speedSmallWaves = 1f;
    [SerializeField] private float _threshold = -21.5f;

    // Start is called before the first frame update
    void Start()
    {
        //Use Dotween for move bigWave
        if (isActiveAndEnabled && _isBigWave)
            _rtWaves.transform.DOBlendableLocalMoveBy(_moveBy, _duration).SetEase(_curve);
    }

    void FixedUpdate()
    {
        if (!_isBigWave) { MoveSmallWave(); }
    }

    private void MoveSmallWave()
    {
        Vector3 newPos = _rtWaves.transform.localPosition;
        newPos.x -= _speedSmallWaves * Time.deltaTime;
        _rtWaves.transform.localPosition = newPos;
        if (_rtWaves.transform.localPosition.x <= _threshold)
        {
            newPos.x = 0;
            _rtWaves.transform.localPosition = newPos;
        }
    }

    //Aplly a common color to all waves
    public void SetColorWaves(Color32 argColor) =>
                                    imgWaveList.ForEach(x => x.color = argColor);
}
