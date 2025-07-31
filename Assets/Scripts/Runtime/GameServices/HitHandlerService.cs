using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using UnityEngine;

public class HitHandlerService : IGameSystem
{
    public void Dispose() {
        
    }

    public void Initialize() {
        //S'abonner a OnActionPressed
        //S'abonner a OnActionPressed
    }

    public void Tick() {
        //Se désabonner a OnActionCanceled
        //Se désabonner a OnActionCanceled
    }

    #region ActionPerformed
    
    void HandleInputPerformed(InputType inputType) {
        switch (inputType) {
            case InputType.Right:
                HandleRightInputPerformed();
                break;
            case InputType.Left:
                HandleLeftInputPerformed();
                break;
            default:
                Debug.LogError($"Unhandled Input Type: {inputType}");
                break;
        }
    }
    
    void HandleRightInputPerformed() {
        //Fonction pour savoir sur quelle mesure je suis actuellement
        //Fonction de tri pour savoir qu'elle action va être lancé
    }
    
    void HandleLeftInputPerformed() {
        //Fonction pour savoir sur quelle mesure je suis actuellement
        //Fonction de tri pour savoir qu'elle action va être lancé
    }
    
    #endregion
    
    #region ActionCanceled

    void HandleInputCanceled(InputType inputType) {
        switch (inputType) {
            case InputType.Right:
                HandleRightInputCanceled();
                break;
            case InputType.Left:
                HandleLeftInputCanceled();
                break;
            default:
                Debug.LogError($"Unhandled Input Type: {inputType}");
                break;
        }
    }

    void HandleRightInputCanceled() {
        
    }

    void HandleLeftInputCanceled() {
        
    }

    #endregion
    
    
    //Fonction pour aller chercher la mesure
    
}
