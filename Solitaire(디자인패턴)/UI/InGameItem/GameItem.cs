using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.AddressableAssets;
using Spine.Unity;

using NGle.Solitaire.Asset;
using NGle.Solitaire.Data;
using NGle.Solitaire.Support;
using NGle.Solitaire.Sound;

namespace NGle.Solitaire.RunGame
{
    public class GameItem : MonoBehaviour
    {

        public enum State { Disable, Enable }
        //[SerializeField] public List<PlayableDirector> timeLines;

        [SerializeField] Transform _btnTarget;
        Button runBtn;

        [SerializeField] protected Button purchaseBtn;

        public GameObject RedBg;
        public Text txCount;

        public UnityAction onClick;

        public Image cover;

        private GameItemDefaultUI.GameItemType _type;
        private int _count;

        private const string Idle = "01_idle";
        private const string Run = "02_ani";

        private const float redWide = 45.42f;
        private const float redNoraml = 35f;

        private bool isItemHammerMode = false;

        public void Awake()
        {
            for (int i = _btnTarget.childCount-1; i >=0 ; i--)
            {
                GameObject.Destroy(_btnTarget.GetChild(i).gameObject);
            }
        }

        public void Initialized(GameItemDefaultUI.GameItemType type,int count,Button button, UnityAction onUseItem, UnityAction<GameItemDefaultUI.GameItemType> onPurchaseClick)
        {
            _type = type;
            txCount.text = count.ToString("00");

            _count = count;
          
            runBtn = Instantiate(button, _btnTarget);
            runBtn.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, Idle, true);

            runBtn.onClick.RemoveAllListeners();
            runBtn.onClick.AddListener(() => onUseItem?.Invoke());
            purchaseBtn.onClick.RemoveAllListeners();
            purchaseBtn.onClick.AddListener(()=>onPurchaseClick?.Invoke(_type));

            runBtn.onClick.AddListener(() =>
            {


                if (type == GameItemDefaultUI.GameItemType.ITEM_GLASSES)
                {
                    ClientData.Instance.UserData.ItemDataGroup.UseItem(4, null);
                    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_hint").Load());
                }
                else if (type == GameItemDefaultUI.GameItemType.ITEM_SHUFFLE)
                {
                    ClientData.Instance.UserData.ItemDataGroup.UseItem(5, null);
                  //  SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_shuffle").Load());
                }
                else if(type == GameItemDefaultUI.GameItemType.ITEM_HAMMER)
                {
                    ClientData.Instance.UserData.ItemDataGroup.UseItem(6, null);
                }
             

                if (type == GameItemDefaultUI.GameItemType.ITEM_HAMMER)
                {
                    Reward();
                }
                else
                {
                    Play();
                }
            });

            cover.gameObject.SetActive(false);

            Counter(_count);
        }

        public void Reward()
        {
            if (_count > 0)
            {
                cover.gameObject.SetActive(true);
                SkeletonGraphic ani = runBtn.GetComponent<SkeletonGraphic>();

                ani.AnimationState.SetAnimation(0, Run, false);

                ani.AnimationState.GetCurrent(0).TrackTime = 0;
                ani.AnimationState.GetCurrent(0).TimeScale = 0;

                isItemHammerMode = true;
            }

        }
        public async void Play()
        {
            if (_count > 0)
            {
                if (isItemHammerMode == false && _type == GameItemDefaultUI.GameItemType.ITEM_HAMMER)
                {
                    return;
                }
                isItemHammerMode = false;

                    SkeletonGraphic ani = runBtn.GetComponent<SkeletonGraphic>();
                    cover.gameObject.SetActive(true);
                    ani.AnimationState.GetCurrent(0).TimeScale = 1;
                    ani.AnimationState.SetAnimation(0, Run, false);
                    _count--;
                    Counter(_count);

                    Spine.AnimationStateData stateData = ani.SkeletonDataAsset.GetAnimationStateData();
                    Spine.Animation animation = stateData.SkeletonData.FindAnimation(Run);

                    await System.Threading.Tasks.Task.Delay((int)(animation.Duration * 1000));
                    cover.gameObject.SetActive(false);
                
            }
        }
        public void ItemCountUpdate(int n)
        {
            _count = n;
            Counter(n);
        }
        private void Counter(int n)
        {
            purchaseBtn.gameObject.SetActive(false);
            runBtn.enabled = true;
            RedBg.SetActive(true);
            if (0 < n && n < 100)
            {
                RedBg.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 24);
                
                txCount.text = n.ToString();
            }
            else if (n >= 100)
            {
                RedBg.GetComponent<RectTransform>().sizeDelta = new Vector2(45.5f, 24);
             
                txCount.text = "99+";
            }
            else if (n == 0)
            {
                RedBg.SetActive(false);
                purchaseBtn.gameObject.SetActive(true);
                runBtn.enabled = false;
            }
        }



      


    }
}
