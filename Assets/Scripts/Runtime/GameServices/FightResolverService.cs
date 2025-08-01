using Runtime.GameServices.Interfaces;
using UnityEngine;

namespace Runtime.GameServices {
    public class FightResolverService : IGameSystem {
        
        private GameSystems _gameSystems;
        
        SO_ActionData aiAction;
        SO_ActionData playerAction;
        
        float timer = 0;
        
        public FightResolverService(GameSystems gameSystems) {
            _gameSystems = gameSystems;
        }
        
        public void Dispose() {
            
        }

        public void Initialize() {
            
        }

        public void Tick() {
            /*if(aiAction && playerAction) //Modifier pour ajouter un délai d'attente pour vérifier si il va avoir une action de l'IA et du joueur ou juste d'un des deux
                CompareAction();*/
        }

        void StartBuffer() {
            if(playerAction && aiAction)
                CompareAction();
            else if (aiAction || playerAction) {
                timer += Time.deltaTime;
                if (timer >= 0.15f) {
                    CompareAction();
                }
            }
        }

        public void GetAIAction(SO_ActionData action) {
            aiAction = action;   
        }
        
        public void GetPlayerAction(SO_ActionData action) {
            playerAction = action;
        }

        void CompareAction() {
            //Stocker en local peut-être ?
            if (timer > 0)
                timer = 0;
            
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