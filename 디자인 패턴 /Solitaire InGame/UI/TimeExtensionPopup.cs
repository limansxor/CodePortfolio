using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace NGle.Solitaire.RunGame
{
    public class TimeExtensionPopup : MonoBehaviour
    {
        [SerializeField] PlayableDirector playableDirector;
        [SerializeField] Button closeBtn;
        const float failManuTime = 12.5f;

        private void Start()
        {
            //closeBtn.onClick

            // 타임라인 트랙 가져오기
            TimelineAsset timeline = (TimelineAsset)playableDirector.playableAsset;

            //if (timeline != null)
            //{
            //    // 시그널 트랙 찾기
            //    SignalTrack signalTrack = null;
            //    foreach (TrackAsset track in timeline.GetOutputTracks())
            //    {
            //        if (track is SignalTrack)
            //        {
            //            signalTrack = (SignalTrack)track;
            //            break;
            //        }
            //    }

            //    if (signalTrack != null)
            //    {
            //        // 시그널 타임라인에서 가져오기
            //        var markers = scriptPlayable.GetMarkers();
            //        foreach (var marker in markers)
            //        {
            //            if (marker is SignalMarker)
            //            {
            //                SignalMarker signalMarker = (SignalMarker)marker;
            //                Debug.Log("Signal received at time: " + signalMarker.time);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Debug.LogWarning("No SignalTrack found in the Timeline.");
            //    }
            //}
        }
    }
}
