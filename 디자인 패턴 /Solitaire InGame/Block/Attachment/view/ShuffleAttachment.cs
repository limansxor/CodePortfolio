using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class ShuffleAttachment : Attachment
    {
        public SpriteRenderer icon;

        private List<Sprite> _sprites;

        private int _current;

        public override void Initialized()
        {
            base.Initialized();

            icon.gameObject.SetActive(false);
        }

        public void SetRes(List<Sprite> sprites, int current)
        {
            _sprites = sprites;
            _current = current;
        }
        public override void Execute(int index = -1, bool isLoop = false)
        {
            StartCoroutine(RunCoroutine());
        }

        public IEnumerator RunCoroutine()
        {
            float t = 0;
            float gap = 0.04f;
            WaitForSeconds waitForSeconds = new WaitForSeconds(gap);

            int idx = _current;
            icon.gameObject.SetActive(true);

            while (ClientData.Instance.gameSetting.shuffleTime > t)
            {
                icon.sprite = _sprites[idx];
                yield return waitForSeconds;
                t += gap;
                idx = (idx + 1) >= _sprites.Count ? 0 : idx + 1;

            }
            icon.gameObject.SetActive(false);

        }

        public override void Remove()
        {
            base.Remove();
        }


        public override void Release()
        {

        }
    }
}
