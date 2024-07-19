using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Dialog;
using UnityEngine;
using UnityEngine.UI;
using NGle.Solitaire.Support;
using System;
using Spine.Unity;
using NGle.Solitaire.RunGame;
using NGle.Solitaire.Data;
using static NGle.Solitaire.Data.LocalUserGoodsDataGroup;

public class TimeExtensionDialogUI : DialogBase
{
    private static TimeExtensionDialogUI currentDialog;

    public static void DoModal(Action onSkip, Action<int> onAlert)
    {
        DialogManager.CreateDialog<TimeExtensionDialogUI>("Dialogs/TimeExtensionDialogUI.prefab",
            delegate (TimeExtensionDialogUI dialog)
            {
                if (dialog)
                {
                    dialog.InitEvent(onSkip, onAlert);
                    dialog.Show();

                    currentDialog = dialog;
                }
            });
    }



    public Button closeBtn;
    public Button[] extensionTimeBtns;

    public SkeletonGraphic skeletonGraphic;

    public Text time10secPrice;
    public Text time20secPrice;

    protected override void OnHide()
    {

    }

    protected override void OnRestore()
    {

    }

    protected override void OnShow()
    {
        StartCoroutine(AutoCloseCoroutine());
    }

    private IEnumerator AutoCloseCoroutine()
    {
        skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
        yield return new WaitForSeconds(SkeletonCommon.Duration(skeletonGraphic, "animation"));

    }

    public void InitEvent(Action onFail, Action<int> onAlert)
    {
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            onFail?.Invoke();
            Close();
        });



        //topUIPresenter //  탑 UI 뜨우기

        for (int i = 0; i < 2; i++)
        {
            int n = i;
            extensionTimeBtns[i].onClick.RemoveAllListeners();
            extensionTimeBtns[i].onClick.AddListener(() => TimeExtensionSet(n,  onAlert));
        }

        ProductData productData = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_PRODUCT).GetTableData<ProductData>(productIDs[0]);

        time10secPrice.text = productData.pay_price.ToString();

         productData = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_PRODUCT).GetTableData<ProductData>(productIDs[1]);

        time20secPrice.text = productData.pay_price.ToString();

    }

    private int[] productIDs = { 151, 152 };
    public void TimeExtensionSet(int n, Action<int> onAlert)
    {
        ProductData productData = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_PRODUCT).GetTableData<ProductData>(productIDs[n]);

        NLog.Log("시간 연장을 구매시도 ");

       


        LocalUserGoodsData localUserGoods = ClientData.Instance.UserData.GoodsDataGroup.GetGoodsData(productData.pay_type);

        if (localUserGoods.EnableUseGoods(productData.pay_price))
        {
            NLog.Log("잼 있다고 가정  ");
            //


            onAlert.Invoke(n); // 시간 연장 실행 
            ClientData.Instance.UserData.GoodsDataGroup.UseGoods(ItemData.Type.ITEM_JEWEL, productData.pay_price);

            ClientData.Instance.UserData.StageDataGroup.ExtendTime(productIDs[n], (res) =>
            {
                // 여기서 무엇을 해야 할지 ? 
            });

        }
        else
        {
            NLog.Log("Gem 부족");

            AlertDialogUI.DoModal(AlertDialogUI.Type.Alert,
                ClientData.Instance.TableData.TextDataImplement.GetText("TEXT.common.Insufficient_gem")
                , null, null);
          
        }

    }

    public static void OnClose()
    {
        currentDialog.Close();
     
    }
}
