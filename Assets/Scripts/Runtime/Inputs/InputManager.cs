using System;
using Runtime.Actions;

namespace Runtime.Inputs {
    public class InputManager : IDisposable
    {
        private readonly InputActions input;

        public event Action<ActionType> OnActionPressed;

        public InputManager()
        {
            input = new InputActions();
            input.Gameplay.Enable();

            input.Gameplay.Direct.performed += _ => OnActionPressed?.Invoke(ActionType.Direct);
            input.Gameplay.Crochet.performed += _ => OnActionPressed?.Invoke(ActionType.Crochet);
            input.Gameplay.BlockLeft.performed += _ => OnActionPressed?.Invoke(ActionType.BlockLeft);
            input.Gameplay.BlockRight.performed += _ => OnActionPressed?.Invoke(ActionType.BlockRight);
        }

        public void Dispose()
        {
            input.Dispose();
        }
    }
}
