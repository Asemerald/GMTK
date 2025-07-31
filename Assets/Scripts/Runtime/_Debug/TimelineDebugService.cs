using System;
using System.Collections.Generic;
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
                //TODO add custom logger
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

            // Ligne rouge = présent au centre
            var centerX = width / 2f;
            DrawLine(new Vector2(centerX, 0), new Vector2(centerX, height), Color.red, 2f);

            // Dessiner les événements passés (comme avant)
            foreach (var ev in _events)
            {
                var dt = now - ev.timestamp; // temps passé depuis événement (positif si passé)

                // Normaliser dans [0, 1] (0 = à droite de la ligne rouge, 1 = à gauche)
                var normalizedX = 0.5f - dt / _duration;

                if (normalizedX < 0 || normalizedX > 1) continue;

                var x = normalizedX * width;
                var color = GetColorForEvent(ev.type);
                DrawLine(new Vector2(x, 0), new Vector2(x, height), color, 2f);
            }

            // --- PRÉDICTION des beats futurs ---
            if (_beatTimestamps.Count > 1)
            {
                // Moyenne intervalle entre beats (en secondes)
                var count = _beatTimestamps.Count;
                var totalInterval = 0f;
                for (var i = 1; i < count; i++)
                    totalInterval += _beatTimestamps[i] - _beatTimestamps[i - 1];

                var avgInterval = totalInterval / (count - 1);

                // Dernier beat connu
                var lastBeatTime = _beatTimestamps[count - 1];

                // On prédit les beats dans la fenêtre visible vers le futur (durée / 2)
                // La ligne rouge = présent = now, la timeline montre +/- duration/2 autour de now
                var maxFutureTime = now + _duration / 2f;

                for (var predictedTime = lastBeatTime + avgInterval;
                     predictedTime <= maxFutureTime;
                     predictedTime += avgInterval)
                {
                    var dt = now - predictedTime; // négatif car futur
                    // Normaliser position : dt négatif donc 0.5 - dt/_duration > 0.5 (à gauche de la ligne rouge)
                    var normalizedX = 0.5f - dt / _duration;

                    if (normalizedX < 0 || normalizedX > 1) continue;

                    var x = normalizedX * width;
                    DrawLine(new Vector2(x, 0), new Vector2(x, height), Color.green, 2f);
                }
            }

            // Fond sombre sous la timeline
            GUI.Box(new Rect(0, 0, width, height), GUIContent.none);

            GUI.EndGroup();
            GUILayout.EndVertical();
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