using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NGle.Solitaire.RunGame
{
    public class MiniPopupReward : MonoBehaviour
    {


        [SerializeField] TAndH goldView;
        [SerializeField] TAndH evtGoldView;

        public void Initialized(int gold, int evtGold)
        {
            goldView.score.gameObject.SetActive(gold > 0);
            goldView.score.text = "x"+ gold.ToString("###,###");
            goldView.hyphen.gameObject.SetActive(gold == 0);

            evtGoldView.score.gameObject.SetActive(evtGold > 0);
            evtGoldView.score.text = "x" + evtGold.ToString("###,###");
            evtGoldView.hyphen.gameObject.SetActive(evtGold == 0);
        }
    }
}
