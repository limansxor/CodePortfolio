using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using NGle.Solitaire.Sound;
using UnityEngine;

namespace NGle.Solitaire.RunGame.Support
{
    public class ComboAudio : MonoBehaviour
    {
        private List<AudioClip> combo;
        private List<AudioClip> voice;

        RunGameMgr mgr;

        public ComboAudio(RunGameMgr mgr)
        {
            this.mgr = mgr;

            combo = ClientData.Instance.AssetDataFileLink.GetComboAudioClipDatas();
            voice = ClientData.Instance.AssetDataFileLink.GetVoiceComboAudioClipDatas();
        }

        public void Increase()
        {
            SoundManager.Instance.PlaySfx(combo[mgr.runGameData.comboCount - 1]);

            switch (mgr.runGameData.comboCount)
            {
                case 3:
                    SoundManager.Instance.PlaySfx(voice[0]);
                    break;
                case 6:
                    SoundManager.Instance.PlaySfx(voice[1]);
                    break;
                case 9:
                    SoundManager.Instance.PlaySfx(voice[2]);
                    break;
            }
        }

    }
}
