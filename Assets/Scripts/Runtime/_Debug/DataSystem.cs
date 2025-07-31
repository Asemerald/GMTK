using Runtime.GameServices;
using Runtime.GameServices.Interfaces;

namespace Runtime._Debug
{
    public class DataSystem : IGameSystem
    {
        private readonly GameSystems _gameServices;
        
        public DataSystem(GameSystems gameServices)
        {
            _gameServices = gameServices;
        }
        
        public void Initialize()    
        {
            
        }

        public void Tick()
        {
            
        }
        
        public void Dispose()
        {
            
        }
    }
}