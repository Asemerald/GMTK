using Runtime.GameServices.Interfaces;

namespace Runtime.GameServices {
    public class FightResolverService : IGameSystem {
        
        private GameSystems _gameSystems;
        
        SO_ActionData aiAction;
        SO_ActionData playerAction;
        
        public FightResolverService(GameSystems gameSystems) {
            _gameSystems = gameSystems;
        }
        
        public void Dispose() {
            
        }

        public void Initialize() {
            
        }

        public void Tick() {
            if(aiAction && playerAction) 
                CompareAction();
        }

        public void GetAIAction(SO_ActionData action) {
            aiAction = action;   
        }
        
        public void GetPlayerAction(SO_ActionData action) {
            playerAction = action;
        }

        void CompareAction() {
            //Stocker en local peut-être ?
            
            var ai = aiAction;
            var player = playerAction;
            ClearActions();
            
            //Faire la comparaison et qu'est-ce qui se passe
        }

        void ClearActions() {
            aiAction = null;
            playerAction = null;
        }
    }
}