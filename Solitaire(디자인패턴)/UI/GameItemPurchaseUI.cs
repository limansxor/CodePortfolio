using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NGle.Solitaire.RunGame
{
    public class GameItemPurchaseUI : MonoBehaviour
    {
        public Button _btnClose;

        [System.Serializable]
        public class ViewInfoSet
        {
            public GameObject parent;

            public TMPro.TextMeshProUGUI itemCounter;
            public GameObject exceedObj;

            public TMPro.TextMeshProUGUI[] purchaseSlot1Counters;
            public TMPro.TextMeshProUGUI purchaseSlot1PriceGold;
            public Button btnSlot1Purchase;

            public TMPro.TextMeshProUGUI[] purchaseSlot2Counters;
            public TMPro.TextMeshProUGUI purchaseSlot2PriceGold;
            public Button btnSlot2Purchase;

            public TMPro.TextMeshProUGUI[] purchaseSlot3Counters;
            public TMPro.TextMeshProUGUI purchaseSlot3PriceGold;
            public Button btnSlot3Purchase;

        }

        public ViewInfoSet[] _viewInfoSets;

        public void Show(GameItemDefaultUI.GameItemType type)
        {
            switch (type)
            {
                case GameItemDefaultUI.GameItemType.ITEM_GLASSES:
                    break;
                case GameItemDefaultUI.GameItemType.ITEM_SHUFFLE:
                    break;
                case GameItemDefaultUI.GameItemType.ITEM_HAMMER:
                    break;
            }
        }
    }
}
