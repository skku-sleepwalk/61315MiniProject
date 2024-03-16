using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMusic : MonoBehaviour
{
    [SerializeField] private AudioClip normalBGM;
    [SerializeField] private AudioClip burningBGM;
    private bool isBurning;
    static AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = normalBGM;
        audioSource.Play();
        isBurning = false;
    }

    private void LateUpdate()
    {
        if (BurningGauge.IsBurning && !isBurning)
        {
            isBurning = true;
            audioSource.clip = burningBGM;
            audioSource.Play();
        }
        else if (!BurningGauge.IsBurning && isBurning)
        {
            isBurning = false;
            audioSource.clip = normalBGM;
            audioSource.Play();
        }
    }

    public static void Pause()
    {
        audioSource.Pause();
    }

    public static void Resume()
    {
        audioSource.UnPause();
    }

}
