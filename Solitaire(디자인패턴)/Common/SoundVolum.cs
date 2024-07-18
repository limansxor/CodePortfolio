using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Sound;
using UnityEngine;
using NGle.Solitaire.Support;

namespace NGle.Solitaire.RunGame
{
    public class SoundVolum : MonoBehaviour
    {
        private AudioSource audioSource;

     
        private IEnumerator Start()
        {
            audioSource = gameObject.GetComponent<AudioSource>();

            if(SoundManager.Instance.MuteSFX)
            {
                audioSource.volume = 0;
                audioSource.mute = true;

                NLog.Log("음소거 " + audioSource.name);
            }
            else
            {
                audioSource.mute = false;
                audioSource.volume = SoundManager.Instance.SfxVolume;
            }

            yield return new WaitUntil(() => SoundManager.Instance.onVolumeChange != null);


            SoundManager.Instance.onVolumeChange.AddListener((val) =>
            {

                if (val == 0)
                {
                    audioSource.volume = 0;
                    audioSource.mute = true;

                    NLog.Log("음소거 " + audioSource.name);
                }
                else
                {
                    audioSource.mute = false;
                    audioSource.volume = val;
                }

            });

            
        }

    }
}
