using System;
using UnityEngine;

public class ArmColliderLogic : MonoBehaviour
{
    private Collider armCollider;

    public Action<Vector3> OnArmHit;
    public Action<Vector3> OnHeadHit;
    
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
        // Point de contact entre notre collider et l'autre
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        if (other.gameObject.layer == LayerMask.NameToLayer("Arms"))
        {
            OnArmHit?.Invoke(hitPoint);
        }
    
        if (other.gameObject.layer == LayerMask.NameToLayer("Head"))
        {
            OnHeadHit?.Invoke(hitPoint);
        }
    }
}