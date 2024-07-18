using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NGle.Solitaire.Asset;
using UnityEngine.UI;
using Spine.Unity;
using UnityEngine.AddressableAssets;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class TimeGaugeUI : MonoBehaviour
    {
        StarIcon starPrefab;
        Image linePrefab;
        [SerializeField] Text timeText;
        public Image timeWarning;

        public float gaugeHight = 241;

        [SerializeField] Image gauge;

        private StarIcon[] _stars;
        private Image[] _lines;
        private float[] _lineValues;

        private bool[] _oneRuns;
        private const int starMax = 3;

        public bool isRun = false;

        public SkeletonGraphic comboCat;
        public SkeletonGraphic sirenCat;
        private bool isOneWarning;

        public AssetReference voice_hurryup;
        private AudioClip voice_hurryup_clip;

        StageRunGameMgr mgr;

        public void Initialized(int addTime)
        {
            mgr = StageRunGameMgr.Instance;

            starPrefab = AssetManager.Instance.LoadAsset("Prefabs/InGameUI/starIcon.prefab").GetComponent<StarIcon>();

            linePrefab = AssetManager.Instance.LoadAsset("Prefabs/InGameUI/line.prefab").GetComponent<Image>();

            _stars = new StarIcon[starMax];
            _lines = new Image[starMax];
            _lineValues = new float[starMax];

            _oneRuns = new bool[starMax];
            for (int i = 0; i < starMax; i++)
            {
               
                _lines[i] = Instantiate(linePrefab, gameObject.GetComponent<RectTransform>());

                _oneRuns[i] = true;
            }

            GameTime(mgr.runGameData.starRewardTimes);

            isOneWarning = true;

            voice_hurryup_clip = AssetManager.Instance.LoadAsset<AudioClip>(voice_hurryup);

           
        }

        public void GameTime(int[] times) // 90 70 50 
        {
           // float[]  lineValues = new float[3];

            timeText.text = times[0].ToString();  // 초기 값 
                                                  // 별 3개 (60초 위치)
            RectTransform rect = _stars[0].GetComponent<RectTransform>();
            float yPos = gaugeHight * ((float)(times[1] + 1 * (times[0]/ 90f)) / (float)times[0]);
            rect.anchoredPosition = new Vector2(-19.0f, yPos);

            rect = _lines[0].GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, yPos);
            _lineValues[0] = rect.anchoredPosition.y;

            // 별 2개 달성 위치  (40초 위치) ==============
            rect = _stars[1].GetComponent<RectTransform>();
            yPos = gaugeHight * ((float) (times[2]+1 * (times[0] / 90f)) / (float)times[0]);
            rect.anchoredPosition = new Vector2(-19.0f, yPos);

            rect = _lines[1].GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, yPos);
            _lineValues[1] = rect.anchoredPosition.y;


            // 별 1개 달성위치  (0초 위치 ) ========================
            rect = _stars[2].GetComponent<RectTransform>();
            yPos = 0;//* (1.0f - (0.0f / times[0]));
            rect.anchoredPosition = new Vector2(-19.0f, yPos);

            // 라인3은 0이라 안보임 
            rect = _lines[2].GetComponent<RectTransform>();
            rect.gameObject.SetActive(false); //anchoredPosition = new Vector2(0, yPos);
            _lineValues[2] = yPos;

            // 보정된 값을 시간으로 치환
            // = (게이지의 높이 + "linesYValues" 리스트의 각 값) / 게이지의 높이 * 게임에서 사용하는 최대 시간 값
            //for (int i = 0; i < 3; i++)
            //{
            //    float timeValue = (gaugeHight + lineValues[i]) / gaugeHight * times[0];
            //    timeValues[i] = timeValue;
            //}
        }

        public void Update()
        {
            if (isRun)
            {
                if (mgr.runGameData.remainingTime > 0)
                {
                    mgr.runGameData.AddRemainingTime(-Time.deltaTime);

                    timeText.text = ((int)mgr.runGameData.remainingTime).ToString();
                    gauge.fillAmount = (mgr.runGameData.remainingTime) / mgr.runGameData.starRewardTimes[0]; //       1/  90
                    // UIParticle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, gaugeHight * gauge.fillAmount);


                    if ((int)mgr.runGameData.remainingTime < mgr.runGameData.starRewardTimes[1] && _oneRuns[0])
                    {
                        _stars[0].Broken();
                        _oneRuns[0] = false;
                    }
                    else if ((int)mgr.runGameData.remainingTime < mgr.runGameData.starRewardTimes[2] && _oneRuns[1])
                    {
                        _stars[1].Broken();
                        _oneRuns[1] = false;
                    }

                    if((int)mgr.runGameData.remainingTime <= 10)
                    {
                        if(isOneWarning)
                        {
                            comboCat.gameObject.SetActive(false);
                            sirenCat.gameObject.SetActive(true);
                            timeWarning.gameObject.SetActive(true);

                            isOneWarning = false;

                            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_hurryup").Load(), true);
                            SoundManager.Instance.PlaySfx(voice_hurryup_clip);
                        }
                       
                    }
                    else
                    {
                        if (sirenCat.gameObject.activeSelf)
                        {
                            sirenCat.gameObject.SetActive(false);
                            comboCat.gameObject.SetActive(true);
                            isOneWarning = true;
                        }
                    }

                  
                }
                else
                {
                    SoundManager.Instance.StopSFX(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_hurryup").Load());
                    gauge.fillAmount = 0;
                    _stars[2].Broken();
                    mgr.OnTimeOver();
                     isRun = false;

                    if (sirenCat.gameObject.activeSelf)
                    {
                        sirenCat.gameObject.SetActive(false);
                        comboCat.gameObject.SetActive(true);
                        isOneWarning = true;
                    }
                }
            }
     
            
        }

        public void TimeExtensionReady(float addTime)
        {
            mgr.runGameData.SetRemainingTime(addTime+0.5f);
            timeText.text = ((int)mgr.runGameData.remainingTime).ToString();
            gauge.fillAmount = (mgr.runGameData.remainingTime) / mgr.runGameData.starRewardTimes[0];
        }
      

    }
}
