using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using UnityEngine;
using UnityEngine.Events;

namespace NGle.Solitaire.RunGame
{
    public class ItemBoard : MonoBehaviour
    {
        public Transform target;

        public GameItemFactory gameItemFactory;

        private GameItem[] gameItems;



        // 아이템 실행 하려면 
        private void Awake()
        {
            for (int i = target.childCount - 1; i >= 0; i--) GameObject.Destroy(target.GetChild(i).gameObject);
        }
        public void SetItemCount()
        {
            var itemData = ClientData.Instance.UserData.ItemDataGroup.GetOwnUserItem(item =>
            {
                if (item.data.type == ItemData.Type.ITEM_GLASSES)
                    return true;

                return false;
            });

            gameItems[0].ItemCountUpdate(itemData?[0].count ?? 0);

            itemData = ClientData.Instance.UserData.ItemDataGroup.GetOwnUserItem(item =>
            {
                if (item.data.type == ItemData.Type.ITEM_SHUFFLE)
                    return true;

                return false;
            });

            gameItems[1].ItemCountUpdate(itemData?[0].count ?? 0);

            itemData = ClientData.Instance.UserData.ItemDataGroup.GetOwnUserItem(item =>
            {
                if (item.data.type == ItemData.Type.ITEM_HAMMER)
                    return true;

                return false;
            });
            gameItems[2].ItemCountUpdate(itemData?[0].count ?? 0);
        }

        public UnityAction onGamePause;
        public UnityAction onGameContinue;

        public void Initialized()
        {
            gameItems = new GameItem[3];

            var itemData = ClientData.Instance.UserData.ItemDataGroup.GetOwnUserItem(item =>
            {
                if (item.data.type == ItemData.Type.ITEM_GLASSES)
                    return true;

                return false;
            });


            gameItems[0] = gameItemFactory.Create(target, GameItemDefaultUI.GameItemType.ITEM_GLASSES,
                itemData?[0].count ?? 0,
                StageRunGameMgr.Instance.OnHint, ShowShop);

            itemData = ClientData.Instance.UserData.ItemDataGroup.GetOwnUserItem(item =>
            {
                if (item.data.type == ItemData.Type.ITEM_SHUFFLE)
                    return true;

                return false;
            });

            gameItems[1] = gameItemFactory.Create(target, GameItemDefaultUI.GameItemType.ITEM_SHUFFLE,
                  itemData?[0].count ?? 0,
                  StageRunGameMgr.Instance.OnShuffle, ShowShop);

            itemData = ClientData.Instance.UserData.ItemDataGroup.GetOwnUserItem(item =>
            {
                if (item.data.type == ItemData.Type.ITEM_HAMMER)
                    return true;

                return false;
            });

            gameItems[2] = gameItemFactory.Create(target, GameItemDefaultUI.GameItemType.ITEM_HAMMER,
             itemData?[0].count ?? 0,
                StageRunGameMgr.Instance.OnHammerMode, ShowShop);
        }


        private void ShowShop(GameItemDefaultUI.GameItemType type)
        {
            onGamePause.Invoke();
            ItemBuyDialog.DoModal(type, SetItemCount, onGameContinue);
        }

        public void HammerRelease()
        {
            gameItems[2].Play();
        }
    }
}
