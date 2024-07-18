using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Dialog;
using NGle.Solitaire.Sound;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Data;
using NGle.Solitaire.Support;

public class PauseDialogUI : DialogBase

{
    public static void DoModal(Action onContinue)
    {
                                                // Dialogs/PauseDialogUI.prefab
        DialogManager.CreateDialog<PauseDialogUI>("Dialogs/PauseDialogUI.prefab",
            delegate (PauseDialogUI dialog)
            {
                if (dialog)
                {
                    dialog.InitPauseMenu(onContinue);
                    dialog.Show();
                 
                }
            });
    }

    public Button btnClose;
    public Button btnContinue;
    public Button btnReTry;
    public Button btnLobby;

    public Button bgmSoundBtn;
    public Button sfxBtn;

    public DOTweenAnimation bgmSoundOn;
    public DOTweenAnimation bgmSoundOff;
    public DOTweenAnimation sfxOn;
    public DOTweenAnimation sfxOff;

  
    public bool isBgmSoundVolume;
    public bool isSfxVolume;

    protected override void OnShow()
    {
      //  InitPauseMenu();
    }


    private void InitPauseMenu( Action onContinue)
    {  
        bgmSoundOn.gameObject.SetActive(false);
        bgmSoundOff.gameObject.SetActive(false);

        sfxOn.gameObject.SetActive(false);
        sfxOff.gameObject.SetActive(false);

        isBgmSoundVolume = SoundManager.Instance.MuteBGM  == false;
        isSfxVolume = SoundManager.Instance.MuteSFX == false; // PlayerPrefs.GetFloat("sound_sfx_volume") > 0 ? true : false;

        if (isBgmSoundVolume) bgmSoundOff.gameObject.SetActive(true);
        else bgmSoundOn.gameObject.SetActive(true);
        if (isSfxVolume) sfxOff.gameObject.SetActive(true);
        else sfxOn.gameObject.SetActive(true);

        bgmSoundOn.DORewind();
        bgmSoundOff.DORewind();
        sfxOn.DORewind();
        sfxOff.DORewind();

        bgmSoundBtn.onClick.RemoveAllListeners();
        bgmSoundBtn.onClick.AddListener(OnChangeBgmVolume);
        bgmSoundBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();

        sfxBtn.onClick.RemoveAllListeners();
        sfxBtn.onClick.AddListener(OnChangeSfxVolume);
        sfxBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(() => {
            OnClickClose();
            onContinue.Invoke();
            });
      

        btnContinue.onClick.RemoveAllListeners();
        btnContinue.onClick.AddListener(() => {
            OnClickClose();
            onContinue.Invoke();
        });
        btnContinue.gameObject.GetOrAddComponent<ButtonClickEvent>();

        btnReTry.onClick.RemoveAllListeners();
        btnReTry.onClick.AddListener(() => {
            Close();
            StageStartDialogUI.DoModal(ClientData.Instance.UserData.StageDataGroup.selectedStageId, () => SceneManager.Instance.PushScene(SceneType.World) );
        });
        btnReTry.gameObject.GetOrAddComponent<ButtonClickEvent>();

        btnLobby.onClick.RemoveAllListeners();
        btnLobby.onClick.AddListener(() => {
            Close();
            SceneManager.Instance.PushScene(SceneType.World);
        });
        btnLobby.gameObject.GetOrAddComponent<ButtonClickEvent>();

    }

    private void OnChangeBgmVolume()
    {
        bgmSoundOn.gameObject.SetActive(false);
        bgmSoundOff.gameObject.SetActive(false);

        bgmSoundOn.DORewind();
        bgmSoundOff.DORewind();


        isBgmSoundVolume = !isBgmSoundVolume; // 전 상태 반대로 
        if (isBgmSoundVolume) // 지금 상태를 반영 
        {
            bgmSoundOn.gameObject.SetActive(true);
            bgmSoundOn.DOPlay();
            // 뮤트 가 false 인 상황 
           // SoundManager.Instance.BgmVolume = 1;

            SoundManager.Instance.MuteBGM = false;


        }
        else 
        {
            bgmSoundOff.gameObject.SetActive(true);
            bgmSoundOff.DOPlay();
            //SoundManager.Instance.BgmVolume = 0;

            SoundManager.Instance.MuteBGM = true;
        
        }
    }

    private void OnChangeSfxVolume()
    {
        sfxOn.gameObject.SetActive(false);
        sfxOff.gameObject.SetActive(false);
        sfxOn.DORewind();
        sfxOff.DORewind();

        isSfxVolume = !isSfxVolume;
        if (isSfxVolume) 
        {
            sfxOn.gameObject.SetActive(true);
            sfxOn.DOPlay();
            //  SoundManager.Instance.SfxVolume = 1; // set
            SoundManager.Instance.MuteSFX = false;
        }
        else
        {
            sfxOff.gameObject.SetActive(true);
            sfxOff.DOPlay();
            // SoundManager.Instance.SfxVolume = 0;
            SoundManager.Instance.MuteSFX = true;
        }

       



    }

    protected override void OnHide()
    {
    }

    protected override void OnRestore()
    {
    }

    public void OnClickClose()
    {
       
        Close();
        
    }
}
