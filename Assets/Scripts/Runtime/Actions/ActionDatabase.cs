using System.Collections.Generic;
using UnityEngine;

public class ActionDatabase : MonoBehaviour
{
    public static ActionDatabase Instance { get; private set; }

    [SerializeField] private List<SO_ActionData> actionDatas;
    public IReadOnlyList<SO_ActionData> ActionDatas => actionDatas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
