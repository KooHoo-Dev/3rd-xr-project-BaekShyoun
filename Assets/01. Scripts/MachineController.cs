using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Whack_A_Mole
{
    public class MachineController : MonoBehaviour
    {
        [SerializeField] private HandSide machineSide;
        [SerializeField] private MoleController[] moles;

        // 동작하는지 확인하기 위한 임시 InputAction들 (Random 로직 완성 후 삭제 예정)
        private InputAction popUpAction;
        private InputAction hideAction;

        private void Awake()
        {
            string popUpKey = machineSide == HandSide.Left ? "<Keyboard>/5" : "<Keyboard>/6";
            popUpAction = new InputAction("PopUp", InputActionType.Button, popUpKey);
            string hideKey = machineSide == HandSide.Left ? "<Keyboard>/7" : "<Keyboard>/8";
            hideAction = new InputAction("Hide", InputActionType.Button, hideKey);

        }

        private void OnEnable()
        {
            popUpAction.Enable();
            hideAction.Enable();
        }

        private void Update()
        {
            if (popUpAction.WasPressedThisFrame())
            {
                for (int i = 0; i < moles.Length; i++)
                {
                    moles[i].PopUp();
                }
            }

            if (hideAction.WasPressedThisFrame())
            {
                for (int i = 0; i < moles.Length; i++)
                {
                    moles[i].HideInTheHole();
                }
            }
        }

        private void OnDisable()
        {
            popUpAction.Disable();
            hideAction.Disable();
        }

        private void OnDestroy()
        {
            popUpAction.Dispose();
            hideAction.Dispose();
        }
    }
}