using System.Collections.Generic;
using UnityEngine;

namespace Runtime._Debug
{
    public class TimelineDebugService : IDebugSystem, IDebugGUI
    {
        private readonly DebugUIState _debugUIState;
        private readonly List<float> _eventTimestamps = new();

        private const float TimelineDuration = 10f; // Seconds visible on screen
        private const float SpikeWidth = 2f;

        private Rect _graphRect = new(20, 20, 400, 60);

        public TimelineDebugService(DebugUIState debugUIState)
        {
            _debugUIState = debugUIState;
        }

        public void Initialize() { }

        public void Tick()
        {
            // Example: simulate one event every second
            if (Time.frameCount % 60 == 0)
            {
                AddEvent(Time.time);
            }

            // Clean up old events
            float cutoff = Time.time - TimelineDuration;
            _eventTimestamps.RemoveAll(t => t < cutoff);
        }

        public void AddEvent(float timestamp)
        {
            _eventTimestamps.Add(timestamp);
        }

        public void DrawDebugGUI()
        {
            if (!_debugUIState.IsVisible("Timeline")) return;

            float now = Time.time;

            GUI.BeginGroup(_graphRect, GUI.skin.box);

            // Draw center red line
            float centerX = _graphRect.width / 2f;
            DrawLine(new Vector2(centerX, 0), new Vector2(centerX, _graphRect.height), Color.red);

            // Draw events (as vertical black lines)
            foreach (var timestamp in _eventTimestamps)
            {
                float dt = now - timestamp;
                if (dt < 0 || dt > TimelineDuration) continue;

                float posX = _graphRect.width * (1f - dt / TimelineDuration);
                DrawLine(new Vector2(posX, 0), new Vector2(posX, _graphRect.height), Color.black, SpikeWidth);
            }

            GUI.EndGroup();
        }

        public void Dispose() { }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1f)
        {
            Color oldColor = GUI.color;
            Matrix4x4 oldMatrix = GUI.matrix;

            GUI.color = color;
            Vector2 d = end - start;
            float angle = Mathf.Rad2Deg * Mathf.Atan2(d.y, d.x);
            float length = d.magnitude;

            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, length, width), Texture2D.whiteTexture);
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
        }
    }
}
