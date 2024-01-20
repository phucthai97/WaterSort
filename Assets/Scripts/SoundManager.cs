using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Space(4)]
    [Header("[AUDIO ATTACH]")]
    [SerializeField] private AudioClip _winSFX;
    [SerializeField] private AudioClip _pourLiquidSFX;
    [SerializeField] private AudioClip _fullFillSFX;
    [SerializeField] private AudioClip _clappingCrowdedSFX;
    [SerializeField] private AudioClip _yeahChildrenSFX;

    public void PlayFullColorSFX(Vector3 posPlay) =>
        AudioSource.PlayClipAtPoint(_winSFX, posPlay, 1f);

    public void PlayWinSFX(Vector3 posPlay)
    {
        AudioSource.PlayClipAtPoint(_clappingCrowdedSFX, posPlay, 0.7f);
        AudioSource.PlayClipAtPoint(_yeahChildrenSFX, posPlay, 0.5f);
    }

    public void PlayPourLiquidSFX(Vector3 posPlay) =>
        AudioSource.PlayClipAtPoint(_pourLiquidSFX, posPlay, 1f);

    public void PlayFullFilSFX(Vector3 posPlay) =>
        AudioSource.PlayClipAtPoint(_fullFillSFX, posPlay, 1f);
}
