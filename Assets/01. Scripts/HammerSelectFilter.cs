using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Whack_A_Mole
{
    // 컨트롤러의 방향과 망치의 방향이 동일한지 확인
    public class HammerSelectFilter : MonoBehaviour, IXRSelectFilter
    {
        public bool canProcess => isActiveAndEnabled;
        private HammerController hammer;

        private void Awake()
        {
            hammer = GetComponent<HammerController>();
        }
        
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            if (hammer == null) return false;
            // isCorrectSide
            // 망치의 방향과 컨트롤러의 방향이 일치하는가?
            bool isCorrectSide = 
                hammer.hammerSide == HandSide.Left ? interactor.handedness == InteractorHandedness.Left
                : interactor.handedness == InteractorHandedness.Right;
            
            // 망치의 방향이 컨트롤러의 방향과 일치하지 않고,
            // interactor로 선택 입력을 받고 있을 때
            if (isCorrectSide == false && interactor.isSelectActive)
            {
                // 잘못된 방향이라는 안내 UI 출력 (3초간)
                UIController.Instance.ShowWrongSideAnnounce().Forget();
            }
            return isCorrectSide;
        }
    }
}