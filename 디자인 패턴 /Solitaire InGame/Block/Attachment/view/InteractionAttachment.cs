using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class InteractionAttachment : Attachment
    {
        public List<BlockInteraction> interactions;

        const float defaultSize = 1.2f;
        private Vector2 _sumPos = Vector2.zero;
        private Vector2 _sumScales = new Vector2(defaultSize, defaultSize);



        public override void Initialized()
        {
            base.Initialized();
        }

        public void Initialized(BlockType type, int kind, Vector2Int idx)
        {
            foreach (var item in interactions)
            {
                item.type = type;
                item.kind = kind;
                item.idx = idx;
            }

        }

        const float scaleVal = 0.05f; // Left Right Up, Down
        Vector2[] extPosArray = { new Vector2(-scaleVal, 0f), new Vector2(scaleVal, 0f), new Vector2(0.0f, scaleVal), new Vector2(0.0f, -scaleVal), };
        Vector2[] extScales = { new Vector2(scaleVal * 2, 0), new Vector2(scaleVal * 2, 0f), new Vector2(0, scaleVal * 2), new Vector2(0, scaleVal * 2) };


        public void Extension(Direction dic)
        {
            Vector2 sumPos = _sumPos;
            Vector2 sumScales = _sumScales;

            switch (dic) // 추가 증가 이기 때문에 다른게 적용 
            {
                case Direction.LEFT:
                    sumPos += extPosArray[(int)Direction.RIGHT]; // 오른 쪽을 증가 해야 한다 
                    sumScales += extScales[(int)Direction.RIGHT];
                    break;
                case Direction.RIGHT:
                    sumPos += extPosArray[(int)Direction.LEFT];
                    sumScales += extScales[(int)Direction.LEFT];
                    break;
                case Direction.TOP:
                    sumPos += extPosArray[(int)Direction.BOTTOM];
                    sumScales += extScales[(int)Direction.BOTTOM];
                    break;
                case Direction.BOTTOM:
                    sumPos += extPosArray[(int)Direction.TOP];
                    sumScales += extScales[(int)Direction.TOP];
                    break;
            }

            interactions[0].transform.localPosition = new Vector2(0, sumPos.y);
            interactions[0].transform.localScale = new Vector2(1.2f, sumScales.y);

            interactions[1].transform.localPosition = new Vector2(sumPos.x, 0);
            interactions[1].transform.localScale = new Vector2(sumScales.x, 1.2f);
        }

        public void Extension(bool isLeftBlank, bool isRightBlank, bool isUpBlank, bool isDownBlank) // 이거 햇갈리니까 Rect 로 바꾼다 
        {
            Vector2 sumPos = Vector2.zero;
            Vector2 sumScales = new Vector2(1.2f, 1.2f);
            if (isLeftBlank) // 내 주변에 빈칸이 있는지 확인 
            {
                sumPos += extPosArray[(int)Direction.LEFT];
                sumScales += extScales[(int)Direction.LEFT];
            }
            if (isRightBlank)
            {
                sumPos += extPosArray[(int)Direction.RIGHT];
                sumScales += extScales[(int)Direction.RIGHT];
            }
            if (isUpBlank)
            {
                sumPos += extPosArray[(int)Direction.TOP];
                sumScales += extScales[(int)Direction.TOP];
            }
            if (isDownBlank)
            {
                sumPos += extPosArray[(int)Direction.BOTTOM];
                sumScales += extScales[(int)Direction.BOTTOM];
            }

            interactions[0].transform.localPosition = new Vector2(0, sumPos.y);
            interactions[0].transform.localScale = new Vector2(1.2f, sumScales.y);

            interactions[1].transform.localPosition = new Vector2(sumPos.x, 0);
            interactions[1].transform.localScale = new Vector2(sumScales.x, 1.2f);

            _sumPos = sumPos;
            _sumScales = sumScales;
        }

        public void SetActive(bool value)
        {
            foreach (var item in interactions) item.gameObject.SetActive(value);
        }

        public override void Remove()
        {
            gameObject.SetActive(false);
            // GameObject.Destroy(gameObject);

        }

        public override void Execute(int index = -1, bool isLoop = false)
        {

        }

        public override void Release()
        {

        }
    }
}
