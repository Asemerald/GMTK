using System;
using System.Collections;
using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runtime.GameServices
{
    public class StructureService : IGameSystem
    {
        private FeedbackService _feedbackService;
        
        public int PlayerHP { get; private set; }
        public int EnemyHP { get; private set; }

        public event Action OnPlayerDeath;
        public event Action OnEnemyDeath;

        private SO_GameConfig _gameConfig;
        private readonly GameSystems _gameSystems;

        float timer = 0;
        bool timerStart = false;

        public StructureService(GameSystems gameSystems)
        {
            _gameSystems = gameSystems ?? 
                           throw new ArgumentNullException(nameof(gameSystems), "GameSystems cannot be null");
        }
        
        public void Initialize()
        {
            _gameConfig = _gameSystems.Get<GameConfigService>()?.GameConfig ??
                          throw new NullReferenceException("GameConfigService is not registered in GameSystems");
            _feedbackService = _gameSystems.Get<FeedbackService>();

            PlayerHP = _gameConfig.maxHealth;
            EnemyHP = _gameConfig.maxHealth;

            OnEnemyDeath += EnemyDied;
            OnPlayerDeath += PlayerDied;
        }

        /// <summary>
        /// Inflige des dégâts à la cible.
        /// </summary>
        public void ApplyDamage(FeedbackTarget target, int damage)
        {
            if (damage <= 0) return;

            switch (target)
            {
                case FeedbackTarget.Player:
                    PlayerHP = Math.Max(PlayerHP - damage, 0);
                    if (PlayerHP <= 0)
                        OnPlayerDeath?.Invoke();
                    break;

                case FeedbackTarget.Enemy:
                    EnemyHP = Math.Max(EnemyHP - damage, 0);
                    if (EnemyHP <= 0)
                        OnEnemyDeath?.Invoke();
                    break;
            }
        }

        void EnemyDied() {
            _feedbackService.FeedbackDeath(false);
            _feedbackService.EndScreen(true);
            StartTimer();
        }

        void PlayerDied() {
            _feedbackService.FeedbackDeath(true);
            _feedbackService.EndScreen(false);
            StartTimer();
            
        }

        void StartTimer() {
            timerStart = true;
            timer = 0;
            Time.timeScale = .5f;
        }
        
        
        void ReloadScene() {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        public void Dispose()
        {
            // Nettoyage si nécessaire
        }

        

        public void Tick()
        {
            // Update du service si nécessaire
            if(timerStart)
                timer += Time.unscaledDeltaTime;
            
            if(timer >= 4f)
                ReloadScene();
        }
    }
}