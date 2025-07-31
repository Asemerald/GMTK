using System;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Runtime {
    public class BeatSyncManager : MonoBehaviour
    {
        public static BeatSyncManager Instance { get; private set; }
        
        [SerializeField] EventReference music;

        public TimelineInfo timelineInfo;
        GCHandle timelineHandle;
        
        EventInstance musicInstance;
        EVENT_CALLBACK beatCallback;

        public static int lastBeat = 0;
        public static string lastMarkerString = null;

        public Action BeatEventAction;
        public Action MarkerEventAction;
        
        [StructLayout(LayoutKind.Sequential)]
        public class TimelineInfo {
            public int currentBeat = 0;
            public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
        }
        
        void Awake() {
            if(Instance == null) Instance = this;
            else Destroy(this);
            
            if (music.IsNull) {
                Debug.LogWarning("FMOD Studio: BeatSyncManager::Awake - No EventReference");
                return;
            }
        
            musicInstance = RuntimeManager.CreateInstance(music);
            musicInstance.start();
        }

        void Start() {
            if(music.IsNull) return;
            
            timelineInfo = new TimelineInfo();
            beatCallback = BeatEventCallback;
            timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
            musicInstance.setUserData(GCHandle.ToIntPtr(timelineHandle));
            musicInstance.setCallback(beatCallback,
                EVENT_CALLBACK_TYPE.TIMELINE_BEAT | EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
        }
        
        void Update() {
            if (lastMarkerString != timelineInfo.lastMarker) {
                lastMarkerString = timelineInfo.lastMarker;

                MarkerEventAction?.Invoke();
            }

            if (lastBeat != timelineInfo.currentBeat) {
                lastBeat = timelineInfo.currentBeat;
                
                BeatEventAction?.Invoke();
            }
        }

        void OnDestroy() {
            musicInstance.setUserData(IntPtr.Zero);
            musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
            timelineHandle.Free();
        }
        
        #if UNITY_EDITOR
        void OnGUI() {
            GUILayout.Box($"Current beat: {timelineInfo.currentBeat}, last marker: {(string)timelineInfo.lastMarker}");
        }
        #endif
        
        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        static RESULT BeatEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr) {
            var instance = new EventInstance(instancePtr);

            var result = instance.getUserData(out var timelineInfoPtr);
            if (result != RESULT.OK) {
                Debug.LogWarning("FMOD Studio: BeatSyncManager::BeatEventCallback - result " + result);
            }
            else if (timelineInfoPtr != IntPtr.Zero) {
                var timeHandler = GCHandle.FromIntPtr(timelineInfoPtr);
                var timelineInfo = (TimelineInfo)timeHandler.Target;

                switch (type) {
                    case EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                        var beatParameter = (TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_BEAT_PROPERTIES));
                        if (timelineInfo != null) timelineInfo.currentBeat = beatParameter.beat;
                        break;
                    case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                        var markerParameter = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
                        if (timelineInfo != null) timelineInfo.lastMarker = markerParameter.name;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            
            return RESULT.OK;
        }
        
        
    }
}
