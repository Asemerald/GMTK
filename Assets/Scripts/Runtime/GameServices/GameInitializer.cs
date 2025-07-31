using Runtime._Debug;
using Runtime.Inputs;
using UnityEngine;

namespace Runtime.GameServices
{
    public class GameInitializer : MonoBehaviour
    {
        private GameSystems _gameSystems;

        [SerializeField] private FMODUnity.EventReference musicEvent;

        private InputManager _inputManager;
        private BeatSyncService _beatSyncService;
        private HitHandlerService _hitHandlerService;
        private ActionDatabase _actionDatabase;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private DebugSystemInitializer debugSystemInitializer;
#endif

        private void Awake()
        {
            _gameSystems = new GameSystems();

            // Instancie et enregistre les systèmes
            _inputManager = new InputManager();
            _beatSyncService = new BeatSyncService(musicEvent);
            _hitHandlerService = new HitHandlerService(_gameSystems);

            _gameSystems.Register(_inputManager);
            _gameSystems.Register(_beatSyncService);
            _gameSystems.Register(_hitHandlerService);
            _gameSystems.Register(_actionDatabase);

            _gameSystems.Initialize();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            RegisterDebugSystems();
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void RegisterDebugSystems()
        {
            var debugUIState = new DebugUIState();
            var debugSystem = new DebugSystem();

            var debugInputService = new DebugInputService(debugUIState);
            debugSystem.Register(debugInputService);

            var beatDebugService = new BeatSyncDebugService(_beatSyncService, debugUIState);
            debugSystem.Register(beatDebugService);

            var timelineDebugService = new TimelineDebugService(debugUIState, _beatSyncService);
            debugSystem.Register(timelineDebugService);

            var fpsDebugService = new FPSDebugService(debugUIState);
            debugSystem.Register(fpsDebugService);

            debugSystemInitializer.DebugSystem = debugSystem;
        }

#endif


        private void Update()
        {
            _gameSystems.Tick();
        }

        private void OnDestroy()
        {
            _gameSystems.Dispose();
        }
    }
}