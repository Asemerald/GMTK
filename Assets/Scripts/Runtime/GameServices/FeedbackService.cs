using System;
using System.Collections.Generic;
using Runtime.GameServices.Interfaces;
using UnityEngine;

public class FeedbackService : IGameSystem
{
    private readonly List<IFeedbackHandler> _handlers = new();

    public void RegisterHandler(IFeedbackHandler handler)
    {
        if (handler != null && !_handlers.Contains(handler))
            _handlers.Add(handler);
    }

    public void PlayFeedback(string feedbackType)
    {
        foreach (var handler in _handlers)
            handler.PlayFeedback(feedbackType);
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