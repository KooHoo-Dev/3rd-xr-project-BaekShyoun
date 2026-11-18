using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Whack_A_Mole
{
    public class MachineController : MonoBehaviour
    {
        public HandSide machineSide;
        [SerializeField] private MoleController[] moles;

        // 동작하는지 확인하기 위한 임시 InputAction들 (Random 로직 완성 후 수정 예정)
        private InputAction popUpAction;
        private InputAction hideAction;
        private bool isPopUp;
        private int index = -1; // 두더지 인덱스(선택된 두더지가 없음)
        
        private CancellationToken destroyToken;
        [SerializeField] private float popUpDuration = 3f;


        # region Unity Methods
        
        private void Awake()
        {
            destroyToken = this.GetCancellationTokenOnDestroy();
            
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

        // 추가해야 할 로직
        // 1. Left면 return시키고서 Right만 실행시키기
        // 2. Right면 return 시키고서 Right만 실행시키기
        // 3. 1번과 2번이 반복하게 하기
        
        private void RandomPopUp()
        {
            if (isPopUp) return;
            index = Random.Range(0, moles.Length);
            moles[index].PopUp();
            isPopUp = true;
        }

        // 디버깅용이라 추후 삭제해도 될듯
        private void GetHideAll()
        {
            for (int i = 0; i < moles.Length; i++)
            {
                moles[i].HideInTheHole();
            }
            DoneHide();
        }

        private void GetHideOne()
        {
            moles[index].HideInTheHole();
            // 시간내로 두더지 못잡았을 때 나오는 로직이므로, 하트 개수를 깎는다.
            --GameManager.Instance.maxHearts;
            Debug.Log($"현재 하트 개수 : {GameManager.Instance.maxHearts}");
            if (GameManager.Instance.maxHearts == 0)
                GameManager.Instance.EndGame();
        }
        
        // 두더지 상태 초기화
        public void DoneHide()
        {
            isPopUp = false;
            index = -1;
        }
        
        public async UniTask StartGame()
        {
            if (destroyToken.IsCancellationRequested) return;
            RandomPopUp();
            await UniTask.WaitForSeconds(popUpDuration, cancellationToken: destroyToken);
            if (index != -1) GetHideOne();
        }
        # endregion
    }
}