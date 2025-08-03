using Runtime.GameServices;

namespace Runtime._Debug
{
    public class ActionDebugService : IDebugSystem, IDebugGUI
    {
        private readonly DebugUIState _debugUIState;
        
        private ActionHandlerService _actionHandlerService;
        
        private readonly System.Collections.Generic.LinkedList<(SO_ActionData, int)> _actionHistory =
            new System.Collections.Generic.LinkedList<(SO_ActionData, int)>();

        private readonly System.Collections.Generic.HashSet<int> _executedActionIds = new();
        private readonly int _maxHistory = 24;
        private int _actionIdCounter = 0;
        
        private System.Collections.Generic.Queue<(SO_ActionData, bool)> _currentQueue;

        public ActionDebugService(DebugUIState debugUIState, ActionHandlerService actionHandlerService)
        {
            _debugUIState = debugUIState;
            _actionHandlerService = actionHandlerService;
        }

        public void Initialize()
        {
            _actionHandlerService._actionDebugService = this;
        }

        public void Tick(){}
        
        public void SetQueue(System.Collections.Generic.Queue<(SO_ActionData, bool)> queue)
        {
            _currentQueue = queue;
        }

        public void RegisterAction(SO_ActionData action)
        {
            var entry = (action, _actionIdCounter++);
            _actionHistory.AddLast(entry);

            if (_actionHistory.Count > _maxHistory)
                _actionHistory.RemoveFirst();
        }

        public void MarkActionExecuted(SO_ActionData action)
        {
            var node = _actionHistory.First;
            while (node != null)
            {
                var (a, id) = node.Value;

                if (a == action && !_executedActionIds.Contains(id))
                {
                    _executedActionIds.Add(id);
                    break;
                }

                node = node.Next;
            }
        }

        public void DrawDebugGUI()
        {
            if (!_debugUIState.IsVisible("Action")) return;
            
            UnityEngine.GUIStyle fontStyle = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label);
            fontStyle.fontSize = 10; 
            fontStyle.normal.textColor = UnityEngine.Color.white;

            UnityEngine.GUILayout.BeginVertical("box");
            UnityEngine.GUILayout.Label("[ACTIONS] Player Queue:", fontStyle);
            UnityEngine.GUILayout.BeginHorizontal();

            var node = _actionHistory.Last;
            while (node != null)
            {
                var (action, id) = node.Value;

                if (action == null)
                {
                    UnityEngine.GUI.contentColor = UnityEngine.Color.gray;
                    UnityEngine.GUILayout.Label("|-|", fontStyle);
                }
                else
                {
                    UnityEngine.GUI.contentColor = _executedActionIds.Contains(id)
                        ? UnityEngine.Color.green
                        : UnityEngine.Color.red;

                    UnityEngine.GUILayout.Label("|" + action.name+"|", fontStyle);
                }

                node = node.Previous;
            }

            UnityEngine.GUI.contentColor = UnityEngine.Color.white;
            UnityEngine.GUILayout.EndHorizontal();
            UnityEngine.GUILayout.EndVertical();
        }

        public void Dispose()
        {
        }
    }
    
}