using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace NGle.Solitaire.RunGame
{
    public class TouchBlockView : BlockView
    {
        [SerializeField] protected FlipAttachment flip;
       // private bool isPreSelect;
        private Vector3 currentPos;

        public override void Initialized()
        {
            base.Initialized();
          //  isPreSelect = false;
            currentPos = transform.position;
        }
        public virtual void Select(bool isTop)
        {
            GameSetting setting = ClientData.Instance.gameSetting;

         //   isPreSelect = true;

            Vector3 expansionPos = new Vector3(currentPos.x - 0.1f, currentPos.y + (isTop ? 0 : 0.75f), currentPos.z); //0.7 | 0.5로 하면 밑에 것도 비슷한 위치로 오름 

            Vector3 expansionScale = new Vector3(setting.expansionScale, setting.expansionScale, setting.expansionScale);

            transform.position = expansionPos;
            transform.parent.GetComponent<SortingGroup>().sortingOrder = 5;
            transform.localScale = expansionScale;
        }

        public virtual async void Selected(bool isCombo)
        {

            // 부드러운 움직임 구현
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;


            transform.position = Vector3.Lerp(pos, currentPos, 0.5f);
            transform.parent.GetComponent<SortingGroup>().sortingOrder = 5;
            transform.localScale = Vector3.Lerp(scale, new Vector3(1, 1, 1), 0.5f);

            await Task.Yield();

            transform.position = currentPos;
           transform.parent.GetComponent<SortingGroup>().sortingOrder = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }

        public virtual void Release()
        {
//            isPreSelect = false;
        }

        public  void FlipForward() => flip.Execute((int)Flip.Forward);

        public  void FlipReverse() => flip.Execute((int)Flip.Reverse);

        public  void FlipRelease() => flip.Release();

        public void Hint()
        {
            // 여기서는 아무것도 안한다 
        }

        
    }
}
