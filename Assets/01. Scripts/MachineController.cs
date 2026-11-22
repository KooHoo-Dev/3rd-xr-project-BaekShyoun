using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Whack_A_Mole
{
    public class MachineController : MonoBehaviour
    {
        public HandSide machineSide;
        [SerializeField] private MoleController[] moles;

        // 동작하는지 확인하기 위한 임시 InputAction들 (Random 로직 완성 후 수정 예정)
        private InputAction popUpAction;
        private InputAction hideAction;

        // 현재 Hide 상태인 두더지 인덱스 저장 ( PopUp 시킬 인덱스 목록 )
        private List<int> hiddenIndices = new List<int>();

        # region Unity Methods
        
        private void Awake()
        {
            // 디버그용
            {
                string popUpKey = machineSide == HandSide.Left ? "<Keyboard>/5" : "<Keyboard>/6";
                popUpAction = new InputAction("PopUp", InputActionType.Button, popUpKey);
                string hideKey = machineSide == HandSide.Left ? "<Keyboard>/7" : "<Keyboard>/8";
                hideAction = new InputAction("Hide", InputActionType.Button, hideKey);
            }
            
        }

        private void OnEnable()
        {
            popUpAction.Enable();
            hideAction.Enable();
        }

        private void Update()
        {
            // 디버그용
            {
                if (popUpAction.WasPressedThisFrame())
                {
                    RandomPopUp();
                }

                if (hideAction.WasPressedThisFrame())
                {
                    GetHideAll();
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
        
        # endregion

        # region PopUp & Hide
        
        private void RandomPopUp()
        {
            // 기존에 모아둔 Hide중인 두더지 인덱스를 비움.
            hiddenIndices.Clear();

            // Hide중인 두더지의 인덱스를 저장
            for (int i = 0; i < moles.Length; i++)
            {
                if (moles[i].IsHide)
                {
                    hiddenIndices.Add(i);
                }
            }

            // 모두 나와있다면 더 꺼낼 두더지가 없으므로 return
            if (hiddenIndices.Count == 0) return;
            
            // 숨어있는 두더지 인덱스 중 어떤 인덱스를 뽑을지 고른다
            int randIndex = Random.Range(0, hiddenIndices.Count);
            // 해당 인덱스에 지정된 실제 두더지의 인덱스를 가져옴(moles의 인덱스)
            int index = hiddenIndices[randIndex];
            
            moles[index].PopUp();
            SoundManager.Instance?.PlaySFX(SFXType.MolePopUp);
        }

        // 디버깅용이라 추후 삭제해도 될듯
        private void GetHideAll()
        {
            for (int i = 0; i < moles.Length; i++)
            {
                moles[i].HideInTheHole();
            }
        }
        
        public void StartGame()
        {
            RandomPopUp();
        }
        # endregion
    }
}