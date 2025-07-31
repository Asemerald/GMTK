using System;
using Runtime.Actions;
using Runtime.GameServices.Interfaces;

namespace Runtime.Inputs {
    public class InputManager : IGameSystem
    {
        private readonly InputActions input;

        public event Action<ActionType> OnActionPressed;

        public InputManager()
        {
            input = new InputActions();
        }

        public void Initialize()
        {
            input.Gameplay.Enable();

            input.Gameplay.Direct.performed += OnDirect;
            input.Gameplay.Crochet.performed += OnCrochet;
            input.Gameplay.BlockLeft.performed += OnBlockLeft;
            input.Gameplay.BlockRight.performed += OnBlockRight;
        }

        public void Tick()
        {
            // rien à faire ici si event-driven
        }

        public void Dispose()
        {
            input.Gameplay.Direct.performed -= OnDirect;
            input.Gameplay.Crochet.performed -= OnCrochet;
            input.Gameplay.BlockLeft.performed -= OnBlockLeft;
            input.Gameplay.BlockRight.performed -= OnBlockRight;

            input.Dispose();
        }

        private void OnDirect(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(ActionType.Direct);

        private void OnCrochet(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(ActionType.Crochet);

        private void OnBlockLeft(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(ActionType.BlockLeft);

        private void OnBlockRight(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
            => OnActionPressed?.Invoke(ActionType.BlockRight);
    }
}