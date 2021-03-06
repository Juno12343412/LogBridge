﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public Image guideImage;
    
    void Start()
    {
        CameraResolution.SetCamera();

        guideImage.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator CR_GameStart(float _Time)
    {
        yield return new WaitForSeconds(_Time);
        //씬 전환
        GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
    }

    public void GameStart(float _Time)
    {
        StartCoroutine(CR_GameStart(_Time));
        StartCoroutine(CR_ImageAlpha());
    }

    public IEnumerator CR_ImageAlpha()
    {
        while (guideImage.color.a < 255)
        {
            yield return new WaitForSeconds(0.01f);
            guideImage.color = new Color(0, 0, 0, guideImage.color.a + 0.01f);
        }
    }
}
