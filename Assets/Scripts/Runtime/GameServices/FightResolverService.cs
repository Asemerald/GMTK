using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;
using UnityEngine;

/*
 * Ce script se charge de récupérer les inputs de l'IA et du joeur sur un temps
 * et de les comparer pour par la suite executé / mettre a jour l'état du joueur / de l'IA
 */

namespace Runtime.GameServices {
    public class FightResolverService : IGameSystem {
        
        private GameSystems _gameSystems;
        private FeedbackService _feedbackService;
        private BeatSyncService _beatSyncService;
        
        SO_ActionData aiAction;
        SO_ActionData playerAction;
        
        float timer = 0;
        
        bool compareCalled = false;
        
        public FightResolverService(GameSystems gameSystems) {
            _gameSystems = gameSystems;
        }
        
        public void Dispose() {
            
        }

        public void Initialize() 
        {
            _feedbackService = _gameSystems.Get<FeedbackService>();
            _beatSyncService = _gameSystems.Get<BeatSyncService>();

            _beatSyncService.OnBeat += CallCompareEvent;
            _beatSyncService.OnHalfBeat += CallCompareEvent;
            _beatSyncService.OnQuarterBeat += CallCompareEvent;

        }

        public void Tick() {
            if(compareCalled)
                StartBuffer();
        }

        void CallCompareEvent() {
            compareCalled = true;
        }
        
        void StartBuffer() {
            if(playerAction && aiAction)
                CompareAction();
            else if (aiAction || playerAction) {
                timer += Time.deltaTime;
                if (timer >= 0.1f) {
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
            compareCalled = false;
            if (timer > 0)
                timer = 0;
            
            Debug.Log("FightResolverService::CompareAction");
            var playerActionType = ActionType.Empty;
            var aiActionType = ActionType.Empty;

            SO_FeedbackData playerFeedback = null; // par défaut anim réussie
            SO_FeedbackData aiFeedback = null;     // par defaut anim réussie
            
            if (playerAction != null) {
                playerActionType = playerAction.actionType;
                playerFeedback = playerAction.feedbackDataSuccess;
            }

            if (aiAction != null) {
                aiActionType = aiAction.actionType;
                aiFeedback = aiAction.feedbackDataSuccess;
            }
            
            

            switch (playerActionType, aiActionType)
            {
                case (ActionType.Attack, ActionType.Attack):                                            // Les deux joueurs s'entre-attaquent
                    if (ActionCounters(playerAction, aiAction) || ActionCounters(aiAction, playerAction)) // Si l'un des joueurs réussit à attaquer miroir
                    {
                        ResolveAction(playerAction,false,aiAction,false);            // Résultat : les deux coups s'annulent
                    }
                    else
                    {
                        ResolveAction(playerAction,true,aiAction,true);             // Résultat : les deux se prennent un coup
                    }
                    
                    break;
                case (ActionType.Attack, ActionType.Parry):                                             // Le joueur attaque et l'IA pare
                    ResolveAction(playerAction,true,aiAction,true);                 // Résultat : joueur attaque et l'IA pare le coup
                    
                    break;
                case (ActionType.Attack, ActionType.Dodge):                                             // Le joueur attaque et l'IA pare
                    if (ActionCounters(aiAction, playerAction))                                                     // Si c'est la bonne esquive
                    {
                        ResolveAction(playerAction,false,aiAction,true);            // Résultat : joueur attaque et l'IA esquive
                    }
                    else
                    {
                        aiFeedback = aiAction.feedbackDataFail;
                        ResolveAction(playerAction,true,aiAction,false);            // Résultat : joueur attaque et l'IA prend le coup
                    }
                    
                    break;
                case (ActionType.Attack, ActionType.Combo):                                             // Le joueur se défend de L'IA qui effectue un combo  
                    if (ComboInputSuccess())                                                                                    //L'IA réussit son combo
                    {
                        if (ActionCounters(playerAction, aiAction) )                                                        // SI le joueur effectue l'attaque miroir                 
                        {
                            if (ComboTimingSuccess())                                                                                   // Si il a un meilleur timing
                            {
                                ResolveAction(playerAction,false,aiAction,false);           
                            }
                            else                                                                                                       // Si il a un moins bon timing
                            {
                                ResolveAction(playerAction,false,aiAction,true);    
                            }
                        }
                        else                                                                                                            // le joueur fait la mauvaise attaque
                        {
                            playerFeedback = playerAction.feedbackDataFail;
                            ResolveAction(playerAction,false,aiAction,true);                // le joueur rate et se prend le coup de l'IA
                        }
                    }
                    else                                                                                                        //L'IA rate son combo
                    {
                        aiFeedback = aiAction.feedbackDataFail;
                        if (ActionCounters(playerAction, aiAction) )                                                        // SI le joueur effectue l'attaque miroir                 
                        {
                            ResolveAction(playerAction,true,aiAction,false);                // Le joueur STUN l'IA et la sort de son combo
                        }
                        else                                                                                                            // le joueur fait la mauvaise attaque
                        {
                            playerFeedback = playerAction.feedbackDataFail;
                            ResolveAction(playerAction,false,aiAction,true);                // le joueur rate et l'IA rate aussi
                        }
                    }
                    
                    break;
                case (ActionType.Attack, ActionType.Empty):                                             // Le joueur attaque et l'IA fait rien
                    aiFeedback = null;
                    ResolveAction(playerAction,true,aiAction,false);   
                    break;

                case (ActionType.Parry, ActionType.Attack):                                             // Le joueur pare une attaque de l'IA
                    ResolveAction(playerAction,true,aiAction,true);    
                    break;
                case (ActionType.Parry, ActionType.Parry):                                              // Le joueur pare et l'IA pare
                    ResolveAction(playerAction,true,aiAction,true);
                    break;
                case (ActionType.Parry, ActionType.Dodge):                                              // Le joueur pare et l'IA esquive
                    ResolveAction(playerAction,true,aiAction,true);
                    break;                                           
                case (ActionType.Parry, ActionType.Combo):
                    aiFeedback = aiAction.feedbackDataFail;
                    break;
                case (ActionType.Parry, ActionType.Empty):                                              //Le joueur pare et l'IA fait rien
                    ResolveAction(playerAction,true,aiAction,true);
                    break;

                case (ActionType.Dodge, ActionType.Attack):                                             // Le joueur essaye de dodge et l'IA attaque
                    if (ActionCounters(playerAction,aiAction))                                                     // Si c'est la bonne esquive
                    {
                        ResolveAction(playerAction,true,aiAction,false);            // Résultat : l'IA attaque et le joueur esquive
                    }
                    else
                    {
                        playerFeedback = playerAction.feedbackDataFail;
                        ResolveAction(playerAction,false,aiAction,true);            // Résultat : l'IA attaque et joueur prend le coup
                    }
                    break;
                case (ActionType.Dodge, ActionType.Parry):                                              // Le joueur esquive et l'IA pare
                    ResolveAction(playerAction,true,aiAction,true);
                    break;
                case (ActionType.Dodge, ActionType.Dodge):
                    ResolveAction(playerAction,true,aiAction,true);                 // Le joueur esquive et l'IA esquive
                    break;
                case (ActionType.Dodge, ActionType.Combo):
                    aiFeedback = aiAction.feedbackDataFail;
                    break;
                case (ActionType.Dodge, ActionType.Empty):                                              //Le joueur esquive et l'IA fait rien
                    ResolveAction(playerAction,true,aiAction,true);                     
                    break;

                case (ActionType.Combo, ActionType.Attack):
                    if (ComboInputSuccess())                                                                                    //Le joueur réussit son combo
                    {
                        if (ActionCounters(aiAction, playerAction))                                                             // SI l'IA effectue l'attaque miroir                  
                        {
                            if (ComboTimingSuccess())                                                                           // Si l'IA a un meilleur timing
                            {
                                ResolveAction(playerAction, false, aiAction, false);           
                            }
                            else                                                                                                 // Si l'IA a un moins bon timing
                            {
                                ResolveAction(playerAction, true, aiAction, false);    
                            }
                        }
                        else                                                                                                    // l'IA fait la mauvaise attaque
                        {
                            aiFeedback = aiAction.feedbackDataFail;
                            ResolveAction(playerAction, true, aiAction, false);                // l'IA rate et se prend le coup du joueur
                        }
                    }
                    else                                                                                                        //Le joueur rate son combo
                    {
                        playerFeedback = playerAction.feedbackDataFail;
                        if (ActionCounters(aiAction, playerAction))                                                             // SI l'IA effectue l'attaque miroir                  
                        {
                            ResolveAction(playerAction, false, aiAction, true);                // L'IA STUN le joueur et le sort de son combo
                        }
                        else                                                                                                    // l'IA fait la mauvaise attaque
                        {
                            aiFeedback = aiAction.feedbackDataFail;
                            ResolveAction(playerAction, false, aiAction, false);                // l'IA rate et le joueur rate aussi
                        }
                    }
                    break;
                case (ActionType.Combo, ActionType.Parry):
                    playerFeedback = playerAction.feedbackDataFail;
                    break;
                case (ActionType.Combo, ActionType.Dodge):
                    playerFeedback = playerAction.feedbackDataFail;
                    break;
                case (ActionType.Combo, ActionType.Combo):                                              //Situation Impossible
                    Debug.LogError("Les deux joueur ont lancé une attaque como, c'est impossible. Il doit y avoir un attaquant et un défenseur");
                    break;                                            
                case (ActionType.Combo, ActionType.Empty):
                    playerFeedback = playerAction.feedbackDataFail;
                    break;

                case (ActionType.Empty, ActionType.Attack):                                             // Le joueur ne fait rien et l'IA attaque
                    playerFeedback = null;
                    ResolveAction(playerAction,true,aiAction,true);  
                    break;
                case (ActionType.Empty, ActionType.Parry):                                              //Le joueur fait rien et l'IA pare
                    ResolveAction(playerAction,true,aiAction,true);
                    break;
                case (ActionType.Empty, ActionType.Dodge):                                              //Le joueur fait rien et l'IA esquive
                    ResolveAction(playerAction,true,aiAction,true);
                    break;
                case (ActionType.Empty, ActionType.Combo):
                    aiFeedback = aiAction.feedbackDataFail;
                    break;
                case (ActionType.Empty, ActionType.Empty):                                              // Le joueur ne fait rien et l'IA ne fait rien
                    break;

                default:
                    break;
            }
            
            if(playerActionType==ActionType.Parry)
                playerFeedback = null;  //gestion du feedback de parry ne se fait pas ici

            if (aiActionType==ActionType.Parry)
            {
                aiFeedback = null;      //gestion du feedback de parry ne se fait pas ici
            }
            
            _feedbackService.PlayActionFeedback(playerFeedback,true);
            _feedbackService.PlayActionFeedback(aiFeedback,false);
            
            ClearActions();
            
            //Faire la comparaison et qu'est-ce qui se passe
        }
        
        private bool ActionCounters(SO_ActionData source, SO_ActionData target)
        {
            return source != null && target != null && target.counterActions != null && target.counterActions.Contains(source);
        }

        private bool ComboInputSuccess()
        {
            Debug.LogError("Ajouter ici la logique de si le défenseur a réussi son action de combo, pour l'insatnt réussite auto");
            return true;
        }
        
        private bool ComboTimingSuccess()
        {
            Debug.LogError("Ajouter ici la logique de si l'attaquant a eu un meilleur timing que le defenseur, pour l'instant réussite automatique ");
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
                        ApplyDamages(action.holdDuration, !isPlayer, opponentAction.actionType == ActionType.Parry,false);
                        if (opponentAction.actionType == ActionType.Combo)
                        {
                            ApplyDamages(action.holdDuration, !isPlayer, false,true); // stun son adversaire, ne doit pas s'activer si l'advenrsaire n'a pas raté son combo
                        }
                        break;
                    
                    case ActionType.Combo:
                        ApplyDamages(action.holdDuration, !isPlayer, opponentAction.actionType == ActionType.Parry,false);
                        break;

                    case ActionType.Parry:
                        ApplyDamages(AttackHoldDuration.None, isPlayer, false,false);
                        break;
                    case ActionType.Dodge:
                        // réduit cooldown 
                        break;
                    case ActionType.Empty:
                        //ne fait rien 
                        break;
                }
            }
        }

        void ApplyDamages(AttackHoldDuration holdDuration, bool toPlayer, bool opponentParry,bool shouldStun)
        {
            if (shouldStun)
            {
                //stun
                return;
            }

            switch (holdDuration)
            {
                case AttackHoldDuration.None:
                    // pour laconsommation q'endurance quand tu parry
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