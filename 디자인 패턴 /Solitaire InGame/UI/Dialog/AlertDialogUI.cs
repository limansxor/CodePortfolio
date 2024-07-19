using System;
using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Dialog;
using NGle.Solitaire.Support;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AlertDialogUI : DialogBase
{
    public enum Type { Alert, Confirm }
    public static void DoModal(Type type,string massage ,Action onOk, Action onCancel= null)
    //public static void DoModal(string msg, int n, Action<int> onTimeExtension = null)
    {
        DialogManager.CreateDialog<AlertDialogUI>("Dialogs/AlertDialogUI.prefab",
            delegate (AlertDialogUI dialog)
            {
                if (dialog)
                {
                    dialog.InitEvent(type, massage, onOk, onCancel);
                    dialog.Show();
                }
            });
    }
    public Transform alert;
    public Transform confirm;
    
    // public Button[] okBtns;
    // public Button cacelBtn;

    public List<Text> messageTxs;
   

    protected override void OnHide()
    {
      
    }

    protected override void OnRestore()
    {
     
    }

    protected override void OnShow()
    {
        // foreach (var item in okBtns)
        //     item.onClick.RemoveAllListeners();
        //
        // cacelBtn.onClick.RemoveAllListeners();
    }

    private System.Action onOk;
    private System.Action onCancel;
    

    public void InitEvent(Type type, string massage, Action onOk, Action onCancel)
    {
        alert.gameObject.SetActive(false);
        confirm.gameObject.SetActive(false);

        messageTxs.ForEach(item => item.text = massage);

        this.onOk = onOk;
        this.onCancel = onCancel;

        // foreach (var item in okBtns)
        // {
        //     item.AddListenerWithClickEvent("se_select_button", () =>
        //     {
        //         Close();
        //         onOk?.Invoke();
        //     });
        // }
        //
        // cacelBtn.AddListenerWithClickEvent("se_cancel_button", () =>
        // {
        //     Close();
        //     onCancel?.Invoke();
        // });

        if (type == Type.Alert) alert.gameObject.SetActive(true);
        else confirm.gameObject.SetActive(true);
    }

    public void OnClickAlertOk()
    {
        onOk?.Invoke();
        Close();
    }

    public void OnClickConfirmOk()
    {
        onOk?.Invoke();
        Close();
    }
    
    public void OnClickConfirmCancel()
    {
        onCancel?.Invoke();
        Close();
    }
    
}
