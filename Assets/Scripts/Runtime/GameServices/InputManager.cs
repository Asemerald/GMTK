using System;
using Runtime.Enums;
using Runtime.GameServices.Interfaces;

namespace Runtime.Inputs {
    public class InputManager : IGameSystem
    {
        private readonly InputActions input;

        public event Action<InputType> OnActionPressed;
        public event Action<InputType> OnActionReleased;

        public InputManager()
        {
            input = new InputActions();
        }

        public void Initialize() {
            input.Gameplay.Enable();

            input.Gameplay.Right.performed += OnRightPerformed;
            input.Gameplay.Left.performed += OnLeftPerformed;
            input.Gameplay.Up.performed += OnUpPerformed;
            input.Gameplay.Down.performed += OnDownPerformed;

            input.Gameplay.Right.canceled += OnRightReleased;
            input.Gameplay.Left.canceled += OnLeftReleased;
            input.Gameplay.Up.canceled += OnUpReleased;
            input.Gameplay.Down.canceled += OnDownReleased;

            input.Gameplay.Dogde.performed += OnDodgePerformed;
        }

        public void Tick()
        {
            // rien à faire ici si event-driven
        }

        public void Dispose()
        {
            input.Gameplay.Right.performed -= OnRightPerformed;
            input.Gameplay.Left.performed -= OnLeftPerformed;
            input.Gameplay.Up.performed -= OnUpPerformed;
            input.Gameplay.Down.performed -= OnDownPerformed;
            
            input.Gameplay.Right.canceled -= OnRightReleased;
            input.Gameplay.Left.canceled -= OnLeftReleased;
            input.Gameplay.Up.canceled -= OnUpReleased;
            input.Gameplay.Down.canceled -= OnDownReleased;
            
            input.Gameplay.Dogde.performed -= OnDodgePerformed;

            input.Dispose();
        }

        //Performed Input
        private void OnRightPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.Right);

        private void OnLeftPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.Left);

        private void OnUpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.Up);

        private void OnDownPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.Down);
        
        
        //Released Input
        private void OnRightReleased(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionReleased?.Invoke(InputType.Right);
        
        private void OnLeftReleased(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionReleased?.Invoke(InputType.Left);
        
        private void OnUpReleased(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionReleased?.Invoke(InputType.Up);

        private void OnDownReleased(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionReleased?.Invoke(InputType.Down);
        
        
        //Dodge Input
        private void OnDodgePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) {
            OnActionPressed?.Invoke(ctx.ReadValue<float>() > 0 ? InputType.DodgeRight : InputType.DodgeLeft);
        }
    }
}