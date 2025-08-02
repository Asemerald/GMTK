using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using UnityEngine;

/*
 * Ce script se charge de récupérer les inputs de l'IA et du joeur sur un temps
 * et de les comparer pour par la suite executé / mettre a jour l'état du joueur / de l'IA
 */

namespace Runtime.GameServices {
    public class FightResolverService : IGameSystem {
        
        private GameSystems _gameSystems;
        private FeedbackService _feedbackService;
        
        SO_ActionData aiAction;
        SO_ActionData playerAction;
        
        float timer = 0;
        
        public FightResolverService(GameSystems gameSystems) {
            _gameSystems = gameSystems;
        }
        
        public void Dispose() {
            
        }

        public void Initialize() 
        {
            _feedbackService = _gameSystems.Get<FeedbackService>();
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
            
            var playerActionType = playerAction.actionType;
            var aiActionType = aiAction.actionType;

            switch (playerActionType, aiActionType)
            {
                case (ActionType.Attack, ActionType.Attack):                                            // Les deux joueurs s'entre-attaquent
                    if (ActionCounters(playerAction, aiAction) || ActionCounters(aiAction, playerAction)) // Si l'un des joueurs réussit à attaquer miroir
                    {
                        _feedbackService.PlayActionFeedback(playerAction.feedbackDataSuccess,true);
                        _feedbackService.PlayActionFeedback(aiAction.feedbackDataSuccess,false);
                        ResolveAction(playerAction,false,aiAction,false);            // Résultat : les deux coups s'annulent
                    }
                    else
                    {
                        ResolveAction(playerAction,true,aiAction,true);             // Résultat : les deux se prennent un coup
                    }
                    
                    break;
                case (ActionType.Attack, ActionType.Parry):                                             // Le joueur attaque et l'IA pare
                    
                    ResolveAction(playerAction,true,aiAction,true);                 // Résultat : joueur attaque et l'IA perd de l'endurance/hp
                    
                    break;
                case (ActionType.Attack, ActionType.Dodge):
                    
                    ResolveAction(playerAction,false,aiAction,true);
                    
                    break;
                case (ActionType.Attack, ActionType.Combo):
                    break;
                case (ActionType.Attack, ActionType.Empty):
                    break;

                case (ActionType.Parry, ActionType.Attack):
                    break;
                case (ActionType.Parry, ActionType.Parry):
                    break;
                case (ActionType.Parry, ActionType.Dodge):
                    break;
                case (ActionType.Parry, ActionType.Combo):
                    break;
                case (ActionType.Parry, ActionType.Empty):
                    break;

                case (ActionType.Dodge, ActionType.Attack):
                    break;
                case (ActionType.Dodge, ActionType.Parry):
                    break;
                case (ActionType.Dodge, ActionType.Dodge):
                    break;
                case (ActionType.Dodge, ActionType.Combo):
                    break;
                case (ActionType.Dodge, ActionType.Empty):
                    break;

                case (ActionType.Combo, ActionType.Attack):
                    
                    break;
                case (ActionType.Combo, ActionType.Parry):
                    break;
                case (ActionType.Combo, ActionType.Dodge):
                    break;
                case (ActionType.Combo, ActionType.Combo):
                    break;
                case (ActionType.Combo, ActionType.Empty):
                    break;

                case (ActionType.Empty, ActionType.Attack):
                    break;
                case (ActionType.Empty, ActionType.Parry):
                    break;
                case (ActionType.Empty, ActionType.Dodge):
                    break;
                case (ActionType.Empty, ActionType.Combo):
                    break;
                case (ActionType.Empty, ActionType.Empty):
                    break;

                default:
                    break;
            }
            
            ClearActions();
            
            //Faire la comparaison et qu'est-ce qui se passe
        }
        
        private bool ActionCounters(SO_ActionData source, SO_ActionData target)
        {
            return source != null && target != null && source.counterActions != null && source.counterActions.Contains(target);
        }

        private bool ComboInputSuccess()
        {
            Debug.LogError("Ajouter ici la logique de savoir si l'attaquant du combo a fait le bon input, pour l'instant réussite auto");
            return true;
        }

        void ResolveAction(SO_ActionData playerFinalAction,bool playerSuccess, SO_ActionData iaFinalAction,bool iaSuccess )
        {
            Debug.LogError("Ajouter ici le déclenchement des 1/2 actions simultanées, feedback associés et enregsitrer dans un historique");

            ApplyAction(playerFinalAction, playerSuccess, true, iaFinalAction);
            ApplyAction(iaFinalAction, iaSuccess, false, playerFinalAction);
        }
        
        void ApplyAction(SO_ActionData action, bool success, bool isPlayer, SO_ActionData opponentAction)
        {
            if (success)
            {
                switch (action.actionType)
                {
                    case ActionType.Attack:
                        ApplyDamages(action.holdDuration, isPlayer, opponentAction.actionType == ActionType.Parry);
                        break;
                    
                    case ActionType.Combo:
                        ApplyDamages(action.holdDuration, isPlayer, opponentAction.actionType == ActionType.Parry);
                        break;

                    case ActionType.Parry:
                    case ActionType.Dodge:
                    case ActionType.Empty:
                        break;
                }
            }
            else
            {
                switch (action.actionType)
                {
                    case ActionType.Attack:
                        // pas d'effet juste un feedback de coup raté
                        break;
                    case ActionType.Combo:
                        // pas d'effet juste un feedback de coup raté
                        break;

                    case ActionType.Parry:
                    case ActionType.Dodge:
                    case ActionType.Empty:
                        break;
                }
            }
        }

        void ApplyDamages(AttackHoldDuration holdDuration, bool toPlayer, bool opponentParry)
        {
            switch (holdDuration)
            {
                case AttackHoldDuration.None:
                    // que pour les esquives donc osef ?
                    break;
                case AttackHoldDuration.Half:
                    //retirer un peu d'endurance
                    break;
                case AttackHoldDuration.Full:
                    //retirer bcp d'endurance
                    break;
            }
            //réduire selon opponentParry
        }

        void ClearActions() {
            aiAction = null;
            playerAction = null;
        }
    }
}