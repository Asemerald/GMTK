using System;
using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;

namespace Runtime.GameServices
{
    public class StructureService : IGameSystem
    {
        public int PlayerHP { get; private set; }
        public int EnemyHP { get; private set; }

        public event Action OnPlayerDeath;
        public event Action OnEnemyDeath;

        private SO_GameConfig _gameConfig;
        private readonly GameSystems _gameSystems;

        public StructureService(GameSystems gameSystems)
        {
            _gameSystems = gameSystems ?? 
                           throw new ArgumentNullException(nameof(gameSystems), "GameSystems cannot be null");
        }
        
        public void Initialize()
        {
            _gameConfig = _gameSystems.Get<GameConfigService>()?.GameConfig ??
                          throw new NullReferenceException("GameConfigService is not registered in GameSystems");

            PlayerHP = _gameConfig.maxHealth;
            EnemyHP = _gameConfig.maxHealth;
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

        public void Dispose()
        {
            // Nettoyage si nécessaire
        }

        

        public void Tick()
        {
            // Update du service si nécessaire
        }
    }
}