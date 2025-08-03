using Runtime._Debug;
using Runtime.Inputs;
using Runtime.ScriptableObject;
using UnityEngine;

namespace Runtime.GameServices
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Game Config")] [Tooltip("Le SO de la config du jeu si c vide bah ça marchera paslol")] [SerializeField]
        private SO_GameConfig _gameConfig;
        [SerializeField] private SO_AIConfig _aiConfig;

        [Header("Feedback Player")]
        [Tooltip("Le prefab du feedback player si c vide bah ça marchera paslol")]
        [SerializeField]
        private FeedbackPlayer _feedbackPlayer;

        private GameSystems _gameSystems;

        private GameConfigService _gameConfigService;
        private InputManager _inputManager;
        private BeatSyncService _beatSyncService;
        private HitHandlerService _hitHandlerService;
        private ActionDatabase _actionDatabase;
        private ComboManagerService _comboManagerService;
        private ActionHandlerService _actionHandlerService;
        private AIService _aiService;
        private FeedbackService _feedbackService;
        private FightResolverService _fightResolverService;
        private StructureService _structureService;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private DebugSystemInitializer debugSystemInitializer;
#endif

        [Header("Debug Script")] 
        [SerializeField] private bool debugFeedbackService;
        [SerializeField] private bool debugFightResolverService;

        private void Awake()
        {
            InitializeGameSystems();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            RegisterDebugSystems();
#endif
        }

        private void InitializeGameSystems()
        {
            _gameSystems = new GameSystems();


            // Instancie et enregistre les systèmes
            _gameConfigService = new GameConfigService(_gameConfig);
            _inputManager = new InputManager();
            _beatSyncService = new BeatSyncService(_gameConfigService.GameConfig.gameMusic);
            _hitHandlerService = new HitHandlerService(_gameSystems);
            _actionDatabase = new ActionDatabase(_gameSystems);
            _comboManagerService = new ComboManagerService(_gameSystems);
            _actionHandlerService = new ActionHandlerService(_gameSystems);
            _feedbackService = new FeedbackService(_gameSystems, _feedbackPlayer);
            _aiService = new AIService(_gameSystems, _aiConfig);
            _fightResolverService = new FightResolverService(_gameSystems);
            _structureService = new StructureService(_gameSystems);
            
            _feedbackService.SetDebug(debugFeedbackService); //pas la meilleure façon pour faire ça mais pas le temps
            _fightResolverService.SetDebug(debugFightResolverService);

            _gameSystems.Register(_gameConfigService);
            _gameSystems.Register(_inputManager);
            _gameSystems.Register(_beatSyncService);
            _gameSystems.Register(_hitHandlerService);
            _gameSystems.Register(_actionDatabase);
            _gameSystems.Register(_comboManagerService);
            _gameSystems.Register(_actionHandlerService);
            _gameSystems.Register(_aiService);
            _gameSystems.Register(_feedbackService);
            _gameSystems.Register(_fightResolverService);
            _gameSystems.Register(_structureService);

            _gameSystems.Initialize();
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
            
            var actionDebugService = new ActionDebugService(debugUIState,_actionHandlerService);
            debugSystem.Register(actionDebugService);

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