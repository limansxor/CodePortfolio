using System;
using UnityEngine;
using UnityEngine.Playables;

using NGle.Solitaire.Asset;

namespace NGle.Solitaire.RunGame
{
    public class BlinkerAttachment : Attachment
    {
       // [SerializeField] PlayableDirector _timeLine;
        public Transform _blinker;

        public SpriteRenderer cover;

        public void Awake()
        {
            _blinker.gameObject.SetActive(false);
            cover.gameObject.SetActive(false);
        }

        public override void Initialized()
        {
            base.Initialized();

            _blinker.gameObject.SetActive(false);
            cover.gameObject.SetActive(true);
        }



        public override void Execute(int index = -1, bool isLoop = false)
        {
            if (gameObject != null)
            {
                cover.gameObject.SetActive(false);
                _blinker.gameObject.SetActive(true);
        
            //_timeLine.playableAsset =AssetManager.Instance.LoadAsset<PlayableAsset>("Assets/AssetData/Timeline/Block/blinker.playable");

            _blinker.GetComponent<PlayableDirector>().Play();
            }
        }

        public override void Release()
        {
          
        }
        public override void Remove()
        {
            base.Remove();
        }


    }
}
