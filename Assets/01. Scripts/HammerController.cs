using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Whack_A_Mole
{
    public class HammerController : MonoBehaviour
    {
        public HandSide hammerSide;

        private XRBaseInteractor holdingHand;
        private bool previousKeepSelected;

        private void OnEnable()
        {
            GetComponent<XRGrabInteractable>().selectEntered.AddListener(OnGrab);
        }

        private void OnGrab(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
        {
            SoundManager.Instance?.PlaySFX(SFXType.HammerGrab);
        }

        public void LockGrab()
        {
            if (holdingHand != null) return;
            XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
            if (grab.isSelected == false) return;

            holdingHand = grab.interactorsSelecting[0] as XRBaseInteractor;
            if (holdingHand == null) return;

            // 그립을 놓거나 망치가 손의 탐색 범위를 벗어나도 잡은 상태 유지.
            previousKeepSelected = holdingHand.keepSelectedTargetValid;
            holdingHand.keepSelectedTargetValid = true;
            holdingHand.StartManualInteraction((IXRSelectInteractable)grab);
        }

        public void UnlockGrab()
        {
            if (holdingHand == null) return;

            // 게임 종료 시 유지 기능을 해제하고 기존 잡기 설정 복구.
            if (holdingHand.isPerformingManualInteraction)
                holdingHand.EndManualInteraction();
            holdingHand.keepSelectedTargetValid = previousKeepSelected;
            holdingHand = null;
        }

        private void OnDisable()
        {
            GetComponent<XRGrabInteractable>().selectEntered.RemoveListener(OnGrab);
            UnlockGrab();
        }
    }
}
