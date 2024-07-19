using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using NGle.Solitaire.Support;

namespace NGle.Solitaire.RunGame
{
    public class MiniPopupGp : MonoBehaviour
    {

        [Serializable]
        public class GPChangeView
        {
            public RectTransform[] states;

            public Text currentTxs;
            public Text[] addTxs;
            public Text[] addGpTxs;

  

            public void Show(int cnt, int add, int mGp)
            {
                NLog.Log(" cnt " + cnt + " add " + add + " mGp " + mGp);

                foreach (var item in states) item.gameObject.SetActive(false);

                if (add > 0) states[0].gameObject.SetActive(true);
                else if (add == 0) states[1].gameObject.SetActive(true);
                else states[2].gameObject.SetActive(true);

                currentTxs.gameObject.SetActive(cnt > -1);
                currentTxs.text = cnt.ToString();
                foreach (var item in addTxs) item.text = Mathf.Abs(add).ToString();
                foreach (var item in addGpTxs) item.text = (add * mGp).ToString();
            }
      


        }

        [SerializeField] GPChangeView starView;
        [SerializeField] GPChangeView crownView;
        public void Initialized(int star, int starAdd, int starCVP, int crown, int crownAdd, int crownCVP)
        {
            starAdd = Mathf.Clamp(starAdd,0, 3);
            starView.Show(star ==0 ?-1 : star, starAdd, starCVP);
            crownView.Show(crown, crownAdd, crownCVP);
            // crownView = new GPChangeView(crown, crownAdd, crownCVP);
            //foreach (var item in starView.states)
            //{
            //    NLog.Log(item.name);
            //}
        }

      


    }
}
