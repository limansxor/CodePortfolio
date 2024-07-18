using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

namespace NGle.Solitaire.RunGame
{
    public class StarIcon : MonoBehaviour
    {
        public Image star;
        public Image offStar;
        public SkeletonGraphic starAni;
        public void Init(Vector2 pos)
        {
            GetComponent<RectTransform>().anchoredPosition = pos;
            star.gameObject.SetActive(true);
            offStar.gameObject.SetActive(false);
        }

        public void Broken()
        {
           // star.gameObject.SetActive(false);
            offStar.gameObject.SetActive(true);
        }
    }
}
