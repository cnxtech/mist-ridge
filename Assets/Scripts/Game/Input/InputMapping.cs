using UnityEngine;

namespace MistRidge
{
    public struct InputMapping
    {
        public InControl.TwoAxisInputControl Direction;
        public bool MenuWasPressed;
        public InControl.InputControl Submit;
        public InControl.InputControl Cancel;
        public InControl.InputControl Jump;
    }
}
