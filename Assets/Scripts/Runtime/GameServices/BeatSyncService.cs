using System;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Runtime.GameServices.Interfaces;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Runtime.GameServices
{
    public class BeatSyncService : IGameSystem
    {
        private readonly EventReference music;

        private EventInstance musicInstance;
        private EVENT_CALLBACK beatCallback;
        private GCHandle timelineHandle;

        internal TimelineInfo timelineInfo;
        private int lastBeat = -1;
        private int lastHalfBeat = -1;
        private int lastBar = -1;
        private string lastMarker = "";

        
        public event Action OnBeat;
        public event Action OnHalfBeat;
        public event Action OnBar;
        public event Action OnMarker;

        [StructLayout(LayoutKind.Sequential)]
        public class TimelineInfo
        {
            public int currentBeat = 0;
            public int currentBar = 0;
            public float currentTempo = 0f;
            public int currentHalfBeat = 0;
            public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
        }

        public BeatSyncService(EventReference musicReference)
        {
            if (musicReference.IsNull)
                throw new ArgumentException("FMOD EventReference is null");

            music = musicReference;
        }

        public void Initialize()
        {
            musicInstance = RuntimeManager.CreateInstance(music);
            musicInstance.start();

            timelineInfo = new TimelineInfo();
            timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);

            beatCallback = BeatEventCallback;
            musicInstance.setUserData(GCHandle.ToIntPtr(timelineHandle));
            musicInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT | EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
        }

        public void Tick()
        {
            if (timelineInfo == null) return;

            // Check Marker
            string currentMarker = timelineInfo.lastMarker;
            if (currentMarker != lastMarker) {
                lastMarker = currentMarker;
                OnMarker?.Invoke();
            }

            // Check Beat
            var currentBeat = timelineInfo.currentBeat;
            if (currentBeat != lastBeat) {
                lastBeat = currentBeat;
                OnBeat?.Invoke();
                
                //Check Bar
                var currentBar = timelineInfo.currentBar;
                if (currentBar != lastBar) {
                    lastBar = currentBar;
                    OnBar?.Invoke();
                }
            }
            
            //Check HalfBeat
            musicInstance.getTimelinePosition(out var position);
            var halfBeatCount = Mathf.FloorToInt(position / (60000f / timelineInfo.currentTempo / 2f));

            if (halfBeatCount != lastHalfBeat) {
                lastHalfBeat = halfBeatCount;
                timelineInfo.currentHalfBeat = halfBeatCount % 2;
                
                if(halfBeatCount % 2 == 1) OnHalfBeat?.Invoke();
            }
        }

        public void Dispose()
        {
            musicInstance.setUserData(IntPtr.Zero);
            musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();

            if (timelineHandle.IsAllocated)
                timelineHandle.Free();
        }

        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private static RESULT BeatEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            var instance = new EventInstance(instancePtr);
            var result = instance.getUserData(out var userDataPtr);

            if (result != RESULT.OK || userDataPtr == IntPtr.Zero)
                return RESULT.OK;

            var handle = GCHandle.FromIntPtr(userDataPtr);
            var info = handle.Target as TimelineInfo;

            switch (type)
            {
                case EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    var beatProps = Marshal.PtrToStructure<TIMELINE_BEAT_PROPERTIES>(parameterPtr);
                    if (info != null) {
                        info.currentBeat = beatProps.beat;
                        info.currentBar = beatProps.bar;
                        info.currentTempo = beatProps.tempo;
                    }
                    break;

                case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    var markerProps = Marshal.PtrToStructure<TIMELINE_MARKER_PROPERTIES>(parameterPtr);
                    if (info != null) info.lastMarker = markerProps.name;
                    break;
            }

            return RESULT.OK;
        }
    }
}
