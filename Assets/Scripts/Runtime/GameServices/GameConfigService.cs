using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;

public class GameConfigService : IGameSystem
{
    private SO_GameConfig _gameConfig;

    public SO_GameConfig GameConfig => _gameConfig;

    public GameConfigService(SO_GameConfig gameConfig)
    {
        _gameConfig = gameConfig;
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