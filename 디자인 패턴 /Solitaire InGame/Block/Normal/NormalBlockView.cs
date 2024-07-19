using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace NGle.Solitaire.RunGame
{
    public class NormalBlockView : TouchBlockView
    {
        [SerializeField] protected SpriteRenderer icon;
        //[SerializeField] InteractionAttachment interaction;
        [SerializeField] protected ShuffleAttachment shuffle;
      

        public override void Initialized()
        {
            base.Initialized();

            attachments.Add(flip);
            attachments.Add(shuffle);

        }

        public void SetRes( List<Sprite> sprites, int kind)
        {
            icon.sprite = sprites[kind];

            shuffle.SetRes(sprites, kind);
        }

        public void SkipShuffle( Sprite sprite)
        {
            //foreach (var item in interaction.interactions)
            //{
            //    item.kind = kind;
            //}

            icon.sprite = sprite;

            shuffle.body.gameObject.SetActive(false);
        }

        public void Shuffle( Sprite sprite)
        {
            if (shuffle.gameObject.activeSelf == false) return;
            StartCoroutine(ShuffleCoroutine( sprite));
        }

        public IEnumerator ShuffleCoroutine( Sprite sprite)
        {
            shuffle.body.gameObject.SetActive(true);

            icon.sprite = sprite; // 바뀐 값 넣기 
  
            shuffle.Execute();

            yield return new WaitForSeconds(ClientData.Instance.gameSetting.shuffleTime);

            //interaction.gameObject.SetActive(true);
            shuffle.body.gameObject.SetActive(false);
        }
    }
}
