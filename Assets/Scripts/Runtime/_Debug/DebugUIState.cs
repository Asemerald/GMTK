using System.Collections.Generic;

public class DebugUIState
{
    private readonly Dictionary<string, bool> _state = new();

    public void SetVisible(string key, bool visible)
    {
        _state[key] = visible;
    }

    public void Toggle(string key)
    {
        if (!_state.ContainsKey(key))
            _state[key] = true;
        else
            _state[key] = !_state[key];
    }

    public bool IsVisible(string key)
    {
        return _state.TryGetValue(key, out var visible) && visible;
    }
}