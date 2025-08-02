using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.GameServices;
using UnityEngine;

namespace Runtime._Debug
{
    public class TimelineDebugService : IDebugSystem, IDebugGUI
    {
        private readonly DebugUIState _debugUIState;
        private readonly List<TimelineEvent> _events = new();
        private readonly float _duration = 10f; // seconds visible in timeline
        private readonly Rect _graphRect = new(20, 100, 500, 60); // Position on screen

        private BeatSyncService _beatSync;

        private float _bpm;
        private readonly List<float> _beatTimestamps = new();
        private const int MinBeatsForBPM = 4; // plus = plus stable


        public TimelineDebugService(DebugUIState debugUIState, BeatSyncService beatSync)
        {
            _debugUIState = debugUIState;
            _beatSync = beatSync;
        }

        public void Initialize()
        {
            if (_beatSync == null)
            {
                Debug.LogError(
                    "BeatSyncService is not set. Please initialize TimelineDebugService with a BeatSyncService instance.");
                return;
            }

            SetBeatSyncService(_beatSync);
        }


        private void SetBeatSyncService(BeatSyncService beatSync)
        {
            _beatSync = beatSync;
            _beatSync.OnBeat += () => AddEvent(TimelineEventType.Beat);
            _beatSync.OnHalfBeat += () => AddEvent(TimelineEventType.HalfBeat);
            _beatSync.OnBar += () => AddEvent(TimelineEventType.Bar);
            _beatSync.OnMarker += () => { AddEvent(TimelineEventType.Marker); };
        }


        public void Tick()
        {
            var now = Time.time;
            _events.RemoveAll(e => now - e.timestamp > _duration);

            if (_beatTimestamps.Count >= MinBeatsForBPM)
            {
                var count = _beatTimestamps.Count;
                var totalInterval = 0f;

                for (var i = 1; i < count; i++) totalInterval += _beatTimestamps[i] - _beatTimestamps[i - 1];

                var averageInterval = totalInterval / (count - 1);
                _bpm = 60f / averageInterval;
            }
            else
            {
                _bpm = 0f;
            }
        }


        public void DrawDebugGUI()
        {
            if (!_debugUIState.IsVisible("Timeline")) return;

            GUILayout.BeginVertical("box");
            GUILayout.Label($"[BeatSync] BPM : {_bpm:F0}");

            var graphRect = GUILayoutUtility.GetRect(500, 60, GUILayout.ExpandWidth(true));
            GUI.BeginGroup(graphRect);

            var now = Time.time;
            var width = graphRect.width;
            var height = graphRect.height;

            var beatHeight = height * 0.5f;
            var measureHeight = height;

            var centerX = width / 2f;
            DrawLine(new Vector2(centerX, 0), new Vector2(centerX, height), Color.red);

            var avgBeatInterval = 0f;
            if (_beatTimestamps.Count >= 2)
                avgBeatInterval = (_beatTimestamps[^1] - _beatTimestamps[0]) / (_beatTimestamps.Count - 1);

            // --- PARTIE PASSÉE ---
            foreach (var ev in _events)
            {
                var dt = now - ev.timestamp;
                var normalizedX = 0.5f - dt / _duration;

                if (normalizedX < 0 || normalizedX > 1)
                    continue;

                var x = normalizedX * width;
                var color = GetColorForEvent(ev.type);

                switch (ev.type)
                {
                    case TimelineEventType.HalfBeat:
                        DrawLine(new Vector2(x, 0), new Vector2(x, beatHeight), color, 1.5f);
                        break;
                    case TimelineEventType.Bar:
                        DrawLine(new Vector2(x, 0), new Vector2(x, measureHeight), color, 2f);
                        break;
                    case TimelineEventType.Beat:
                    default:
                        DrawLine(new Vector2(x, 0), new Vector2(x, beatHeight), color, 2f);
                        break;
                }
            }

            // --- PARTIE PRÉDICTION ---
            if (avgBeatInterval > 0)
            {
                var lastBeatTime = _beatTimestamps.Count > 0 ? _beatTimestamps[^1] : now;

                // Beats prédits (temps forts)
                var nextBeatTime = lastBeatTime + avgBeatInterval;
                List<float> predictedBeats = new();

                while (nextBeatTime - now < _duration / 2f)
                {
                    var dt = nextBeatTime - now;
                    if (dt >= 0)
                    {
                        var normalizedX = 0.5f + dt / _duration;
                        if (normalizedX >= 0.5f && normalizedX <= 1f)
                        {
                            var x = normalizedX * width;
                            DrawLine(new Vector2(x, 0), new Vector2(x, beatHeight), Color.black, 2f);
                            predictedBeats.Add(nextBeatTime);
                        }
                    }

                    nextBeatTime += avgBeatInterval;
                }

                // Demi-beats prédits (gris), sauf s'ils tombent pile sur un beat
                var halfBeatInterval = avgBeatInterval / 2f;
                var nextHalfBeatTime = lastBeatTime + halfBeatInterval;

                while (nextHalfBeatTime - now < _duration / 2f)
                {
                    bool isNearAnyPredictedBeat = predictedBeats.Any(bt => Mathf.Abs(bt - nextHalfBeatTime) < 0.01f);
                    var alreadyExists = _events.Exists(e =>
                        e.type == TimelineEventType.HalfBeat &&
                        Mathf.Abs(e.timestamp - nextHalfBeatTime) < 0.01f);

                    if (!alreadyExists && !isNearAnyPredictedBeat)
                    {
                        var dt = nextHalfBeatTime - now;
                        var normalizedX = 0.5f + dt / _duration;
                        if (normalizedX >= 0.5f && normalizedX <= 1f)
                        {
                            var x = normalizedX * width;
                            DrawLine(new Vector2(x, 0), new Vector2(x, beatHeight),
                                GetColorForEvent(TimelineEventType.HalfBeat), 1.5f);
                        }
                    }

                    nextHalfBeatTime += halfBeatInterval;
                }

                // Mesures prédits
                int beatsPerBar = 4;
                float barInterval = beatsPerBar * avgBeatInterval;

// On rend le retour nullable avec `TimelineEvent?`
                TimelineEvent? lastBarEvent = _events.FindLast(e => e.type == TimelineEventType.Bar);
                float lastBarTime = lastBarEvent.HasValue ? lastBarEvent.Value.timestamp : now;

                float nextBarTime = lastBarTime + barInterval;
                while (nextBarTime - now < _duration / 2f)
                {
                    float dt = nextBarTime - now;
                    float normalizedX = 0.5f + dt / _duration;
                    if (normalizedX >= 0.5f && normalizedX <= 1f)
                    {
                        float x = normalizedX * width;
                        DrawLine(new Vector2(x, 0), new Vector2(x, measureHeight), GetColorForEvent(TimelineEventType.Bar), 2f);
                    }
                    nextBarTime += barInterval;
                }


            }

            GUI.Box(new Rect(0, 0, width, height), GUIContent.none);
            GUI.EndGroup();
            GUILayout.EndVertical();
        }

        private Color GetColorForEvent(TimelineEventType type)
        {
            return type switch
            {
                TimelineEventType.Beat => Color.black,
                TimelineEventType.HalfBeat => Color.gray,
                TimelineEventType.Bar => Color.white,
                TimelineEventType.Marker => Color.blue,
                _ => Color.magenta
            };
        }


        private void AddEvent(TimelineEventType type)
        {
            var now = Time.time;

            _events.Add(new TimelineEvent
            {
                timestamp = now,
                type = type
            });

            if (type == TimelineEventType.Beat)
            {
                _beatTimestamps.Add(now);
                // Garde uniquement les N derniers
                if (_beatTimestamps.Count > 16)
                    _beatTimestamps.RemoveAt(0);
            }
        }


        private void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1f)
        {
            var oldColor = GUI.color;
            var oldMatrix = GUI.matrix;

            GUI.color = color;
            var d = end - start;
            var angle = Mathf.Rad2Deg * Mathf.Atan2(d.y, d.x);
            var length = d.magnitude;

            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, length, width), Texture2D.whiteTexture);
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
        }

        public void Dispose()
        {
            // Optionally unsubscribe
        }

        private struct TimelineEvent
        {
            public float timestamp;
            public TimelineEventType type;
        }

        private enum TimelineEventType
        {
            Beat,
            HalfBeat,
            Bar,
            Marker
        }
    }
}