using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{

    [CreateAssetMenu(fileName = "GameSetting", menuName = "NGLE/ScriptableData/BlockDataProperty", order = int.MaxValue)]
    public class GameSetting : ScriptableObject
    {

        [System.Serializable]
        public class CamViewSize
        {
            public CamViewSize(int type, float camViewSize)
            {
                _type = type;
                _camViewSize = camViewSize;
            }

            [SerializeField] int _type;
            public int type { get { return _type; } }

            [SerializeField] float _camViewSize;
            public float camViewSize { get { return _camViewSize; } }
        }

        [SerializeField]
        private CamViewSize[] _camViewSize = new CamViewSize[]
        {
        new CamViewSize(1,3.55f),
        new CamViewSize(2,3.94f),
        new CamViewSize(3,3.6f),
        new CamViewSize(4,4.0f),

        };
        public CamViewSize[] camViewSize { get { return _camViewSize; } }

        [SerializeField] int _comboCountMAX = 10;
        public int comboCountMAX { get { return _comboCountMAX; } }

        [SerializeField] float _scaleX = 0.58f; // 한번 클릭 됬는지 두번 클릭 됬는 
        public float scaleX { get { return _scaleX; } }

        [SerializeField]
        private float _scaleY = 0.71f; // 한번 클릭 됬는지 두번 클릭 됬는 
        public float scaleY { get { return _scaleY; } }

        [SerializeField] float _shuffleTime = 0.5f;// 플립 X2
        public float shuffleTime { get { return _shuffleTime; } }

        [SerializeField] float _filpTime = 1f;
        public float filpTime { get { return _filpTime; } }

        [SerializeField] float _flipAniTime = 0.3f;
        public float flipAniTime { get { return _flipAniTime; } }

        [SerializeField]
        Vector2Int[] _maxBlockByType = new Vector2Int[]
        {
        new Vector2Int(18,9),
        new Vector2Int(20,10),
        new Vector2Int(9,8),
        new Vector2Int(10,9),
        };
        public Vector2Int[] maxBlockByType { get { return _maxBlockByType; } }

        [SerializeField] float _comboTimeLimit = 5;
        public float comboTimeLimit { get { return _comboTimeLimit; } }


        [SerializeField] float _warningTime = 10;
        public float warningTime { get { return _warningTime; } }


        [SerializeField] float _waitCounter = 30;
        public float waitCounter { get { return _waitCounter; } }


        [SerializeField] float _expansionScale = 1.5f;
        public float expansionScale { get { return _expansionScale; } }

        [SerializeField] int groupCount = 11;
        public int GroupCount { get { return groupCount; } }



        [SerializeField] float spinnerViewTime = 10;
        public float SpinnerViewTime { get { return spinnerViewTime; } }

        [SerializeField] float networkWarningPopTime = 30;
        public float NetworkWarningPopTime { get { return networkWarningPopTime; } }

        [SerializeField] List<int> octopusInkTimes = new List<int> { 15, 30, 45 };
        public List<int> OctopusInkTimes { get { return octopusInkTimes; } }

        [SerializeField] float[] orthographicSize = { 3.58f, 3.97f, 4f, 4f };
        public float[] OrthographicSize { get { return orthographicSize; } }

        [SerializeField] float[] padAddRat = { 2.36f, 2.527f, 2.36f, 2.36f };
        public float[] PadAddRat { get { return padAddRat; } }

        [SerializeField] float comboPowerGainTime = 0.56f;
        public float ComboPowerGainTime { get { return comboPowerGainTime; } }

        [SerializeField] int attackTargetCount = 2;
        public int AttackTagetCount { get { return attackTargetCount; } }

        [SerializeField] Vector3 attackProjectileAddPos = new Vector3(-0.063f, 0.066f, 0);
        public Vector3 AttackProjectileAddPos { get { return attackProjectileAddPos; } }

        [SerializeField] float attackProjectileDuration = 0.5f;
        public float AttackProjectileDuration { get { return attackProjectileDuration; } }

        public int KeyKind = 3;
        public int NumKind = 9;
        public int OctKind = 3;

        public int characterOrderLayer = 10;
        public int attackEffectOrderLayer = 9;
        public int projectileOrderLayer = 8;
        public int gainEffectOrderLayer = 7;
        public int gainProjectileOrderLayer = 6;
        public int lockOrderLayer = 5;


        public float removeTime = 0.5f;

        public float baseResolutionWidth  { get { return 720;  } }
        public float baseResolutionHeight { get { return 1280; } }

        //ClientData.Instance.gameSetting.gainEffectOrderLayer
    }
}