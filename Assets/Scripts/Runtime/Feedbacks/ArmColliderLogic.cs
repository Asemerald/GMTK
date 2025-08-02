using Unity.VisualScripting;
using UnityEngine;

public class ArmColliderLogic : MonoBehaviour
{
    private Collider armCollider;
    
    [SerializeField] private bool isLeftArm = true;
    
    private void Awake()
    {
        armCollider = GetComponent<Collider>();
        if (armCollider == null)
        {
            Debug.LogError("ArmColliderLogic requires a Collider component.");
        }
    }
    
    
}
