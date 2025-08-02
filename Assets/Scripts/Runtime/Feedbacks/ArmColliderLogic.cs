using System;
using Unity.VisualScripting;
using UnityEngine;

public class ArmColliderLogic : MonoBehaviour
{
    private Collider armCollider;
    
    [SerializeField] private bool isLeftArm = true;
    [SerializeField] private bool isEnemy = false;

    public Action<HitType> OnArmHit;
    public Action OnHeadHit;
    
    private void Awake()
    {
        armCollider = GetComponent<Collider>();
        if (armCollider == null)
        {
            Debug.LogError("ArmColliderLogic requires a Collider component.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!isEnemy)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Arms"))
            {
                if (TryGetComponent(out ArmColliderLogic armColliderLogic))
                {
                    if (armColliderLogic.isLeftArm != isLeftArm)
                    {
                        OnArmHit?.Invoke(isLeftArm ? HitType.LeftArm : HitType.RightArm);
                    }
                }
            }
        
            if (other.gameObject.layer == LayerMask.NameToLayer("Head"))
            {
                OnHeadHit?.Invoke();
            }
        }
        
        
    }
}

public enum HitType
{
    LeftArm,
    RightArm,
}

