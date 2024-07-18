using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace NGle.Solitaire.RunGame
{
 
    public class MiniPopupScore : MonoBehaviour
    {
    

        [SerializeField] TAndH stageScore;
        [SerializeField] TAndH itemScore;

        public void Initialized(int score, int itmeScore)
        {

            stageScore.score.gameObject.SetActive(score > 0);
            stageScore.score.text = score.ToString("###,###");
            stageScore.hyphen.gameObject.SetActive(score == 0);

            itemScore.score.gameObject.SetActive(itmeScore > 0);
            itemScore.score.text = itmeScore.ToString("###,###");
            itemScore.hyphen.gameObject.SetActive(itmeScore == 0);

        }

    }
}
