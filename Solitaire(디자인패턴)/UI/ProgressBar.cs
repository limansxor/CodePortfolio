using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Asset;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System.Linq;
using NGle.Solitaire.Support;

namespace NGle.Solitaire.RunGame
{
    public class ProgressBar : MonoBehaviour
    {
        const float hightMin = 205;
        const float hightMax = -205;

        float _challengerprocess;
        float _process;
        int pageMax;

       [SerializeField] RectTransform challengerPin;
        [SerializeField] RectTransform playerPin;

        [SerializeField] RectTransform[] pageFlags;


        [SerializeField] Image[] challengerPlacements;
        [SerializeField] Image[] playerPlacements;



        public void Initialized(int pageMax)
        {
            this.pageMax = pageMax;
            pageFlags[0].gameObject.SetActive( pageMax > 1);
           
            Vector2 pos = pageFlags[0].anchoredPosition;
            pageFlags[0].anchoredPosition = new Vector2(pos.x, Mathf.Lerp(hightMin, hightMax, 1f / pageMax));
            pageFlags[0].transform.GetChild(0).gameObject.SetActive(false);

            pageFlags[1].gameObject.SetActive(pageMax>2);
            pos = pageFlags[1].anchoredPosition;
            pageFlags[1].anchoredPosition = new Vector2(pos.x, Mathf.Lerp(hightMin, hightMax, 2f / pageMax));
            pageFlags[1].transform.GetChild(0).gameObject.SetActive(false);

            pos = challengerPin.anchoredPosition;
            challengerPin.anchoredPosition = new Vector2(pos.x, hightMax);

            pos = playerPin.anchoredPosition;
            playerPin.anchoredPosition = new Vector2(pos.x, hightMax);

            _challengerprocess = 0;
            _process = 0;


            challengerPlacements.ToList().ForEach(item => item.gameObject.SetActive(false));
            playerPlacements.ToList().ForEach(item => item.gameObject.SetActive(false));

            challengerPlacements[1].gameObject.SetActive(true);
            playerPlacements[1].gameObject.SetActive(true);

            Player(0);
            Challenger(0);
        }

        public void Player(float process)
        {
       //     NLog.Log("먼데 먼데 process " + process);
            Vector2 pos = playerPin.anchoredPosition; //  hightMax - (process+1) / hightMax
            playerPin.anchoredPosition = new Vector2(pos.x, Mathf.Lerp(hightMin, hightMax, process/ pageMax) );

          
            if (process >= 2)
            {
                pageFlags[1].transform.GetChild(0).gameObject.SetActive(true);
            }
            else if (process >= 1)
            {
                pageFlags[0].transform.GetChild(0).gameObject.SetActive(true);
            }

            _process = process;
            PlacementUpdate();
        }
        public void Challenger(float process)
        {
            Vector2 pos = challengerPin.anchoredPosition;
            //  challengerPin.anchoredPosition = new Vector2(pos.x, hightMax-process / pageMax * hightMax);
            challengerPin.anchoredPosition = new Vector2(pos.x, Mathf.Lerp(hightMin, hightMax, process / pageMax)
                );

            _challengerprocess = process;
            PlacementUpdate();
        }

        public void PlacementUpdate()
        {

            challengerPlacements.ToList().ForEach(item => item.gameObject.SetActive(false));
            playerPlacements.ToList().ForEach(item => item.gameObject.SetActive(false));

            if (_challengerprocess > _process)
            {
                challengerPlacements[2].gameObject.SetActive(true);
                playerPlacements[0].gameObject.SetActive(true);
            }
            else if(_challengerprocess == _process)
            {
                challengerPlacements[1].gameObject.SetActive(true);
                playerPlacements[1].gameObject.SetActive(true);
            }
            else
            {
                challengerPlacements[0].gameObject.SetActive(true);
                playerPlacements[2].gameObject.SetActive(true);
            }
        }

    }
}
