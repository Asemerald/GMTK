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

        public void Initialize()
        {
            input.Gameplay.Enable();

            input.Gameplay.Direct.performed += OnRightPerformed;
            input.Gameplay.Crochet.performed += OnLeftPerformed;
            input.Gameplay.BlockLeft.performed += OnBlockLeftPerformed;
            input.Gameplay.BlockRight.performed += OnBlockRightPerformed;
            
            input.Gameplay.Direct.canceled += OnRightReleased;
            input.Gameplay.Crochet.canceled += OnLeftReleased;
        }
        
        public void Tick()
        {
            // rien à faire ici si event-driven
        }

        public void Dispose()
        {
            input.Gameplay.Direct.performed -= OnRightPerformed;
            input.Gameplay.Crochet.performed -= OnLeftPerformed;
            input.Gameplay.BlockLeft.performed -= OnBlockLeftPerformed;
            input.Gameplay.BlockRight.performed -= OnBlockRightPerformed;
            
            input.Gameplay.Direct.canceled -= OnRightReleased;
            input.Gameplay.Crochet.canceled -= OnLeftReleased;

            input.Dispose();
        }

        private void OnRightPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.Right);

        private void OnLeftPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.Left);

        private void OnBlockLeftPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.BlockLeft);

        private void OnBlockRightPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(InputType.BlockRight);
        
        private void OnRightReleased(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        => OnActionReleased?.Invoke(InputType.Right);
        
        private void OnLeftReleased(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionReleased?.Invoke(InputType.Left);
    }
}