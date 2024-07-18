using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NGle.Solitaire.Asset;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace NGle.Solitaire.RunGame
{
    public class GameItemFactory : MonoBehaviour
    {

        public AssetReference reference;

        [System.Serializable]
        public class ItemGraphic
        {
            public GameItemDefaultUI.GameItemType type;
            public AssetReference reference;

        }
        public ItemGraphic[] itemGraphics;
        private Dictionary<GameItemDefaultUI.GameItemType, AssetReference> dicItemGraphics;

        private GameItem prefab;


        public void Awake()
        {
            prefab = AssetManager.Instance.LoadAsset(reference).GetComponent<GameItem>();
        }

        public GameItem Create(Transform target, GameItemDefaultUI.GameItemType type, int count ,
            UnityAction onUseItem,UnityAction<GameItemDefaultUI.GameItemType> onShowShop)
        {

            dicItemGraphics = new Dictionary<GameItemDefaultUI.GameItemType, AssetReference>();
            foreach (var item in itemGraphics) dicItemGraphics.Add(item.type, item.reference);

            Button btn = AssetManager.Instance.LoadAsset(dicItemGraphics[type]).GetComponent<Button>();

            GameItem gitem = Instantiate(prefab);
            gitem = Instantiate(prefab, target);
            gitem.Initialized(type, count, btn, onUseItem, onShowShop);

            //switch (type)
            //{
            //    case GameItem.Type.Magnifier:
                 

            //        break;
            //    case GameItem.Type.shuffiling:
            //        break;
            //    case GameItem.Type.hammer:
            //        break;
            //}

            return gitem;
        }
    }
}
