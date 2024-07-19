using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using NGle.Solitaire.Asset;

using UnityEngine.EventSystems;
using Spine.Unity;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Data;

public class ButtonClickEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]private bool _onlySound = false;
    [SerializeField]private AudioClip _clickSound;
    private AudioClip ClickSound
    {
        get
        {
            if(_clickSound == null)
            {
                _clickSound = ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_select_button").Load();
            }

            return _clickSound;
        }
        set
        {
            _clickSound = value;
        }
    }

    private Animation _clickAni;

    public void Init(string clickSound)
    {
        ClickSound = ClientData.Instance.AssetDataFileLink.GetAudioClipData(clickSound).Load();
    }

    public void Init(string clickSound, string animationStr)
    {
        ClickSound = ClientData.Instance.AssetDataFileLink.GetAudioClipData(clickSound).Load();
        _clickAni = AssetManager.Instance.LoadAsset<Animation>(animationStr);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_onlySound == false)
        {
            if (_clickAni == null)
            {
                transform.DOScale(0.92f, 0.15f);
            }
            else
            {
                _clickAni.Play();
            }    
        }
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_clickAni == null && _onlySound == false)
            transform.DOScale(1f, 0.15f);
        
        SoundManager.Instance.PlaySfx(ClickSound);
    }
}
