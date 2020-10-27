using System.Collections;
using System;
using UnityEngine;

public class Utility
{
    public static void PlayParticles(GameObject particleSystem, Color color)
    {
        particleSystem.GetComponent<ParticleSystemRenderer>().material.color = color;
        particleSystem.GetComponent<ParticleSystem>().Play();
    }

    public static IEnumerator Delay(float delayTime, Action callBack)
    {
        float counter = 0;

        while (counter < delayTime)
        {
            counter += Time.deltaTime;
            yield return null;
        }
        
        callBack?.Invoke();
    }
}
