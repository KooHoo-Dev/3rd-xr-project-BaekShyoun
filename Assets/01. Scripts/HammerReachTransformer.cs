using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Whack_A_Mole
{
    // 잡은 순간을 기준으로 손의 이동량을 확대. 손목 회전도 잡는 지점을 중심으로 확대.
    public class HammerReachTransformer : XRGeneralGrabTransformer
    {
        [SerializeField, Range(1f, 2.5f)] private float movementMultiplier = 2.5f;
        [SerializeField, Range(1f, 1.5f)] private float rotationMultiplier = 1.5f;

        private Transform hand;
        private Transform playerOrigin;
        private Vector3 grabHandPosition;
        private Quaternion grabHandRotation;

        public override void OnGrab(XRGrabInteractable grabInteractable)
        {
            base.OnGrab(grabInteractable);
            hand = null;
            playerOrigin = null;
            if (grabInteractable.interactorsSelecting.Count != 1) return;

            hand = grabInteractable.interactorsSelecting[0].transform;
            XROrigin origin = hand.GetComponentInParent<XROrigin>();
            if (origin == null && Camera.main != null)
                origin = Camera.main.GetComponentInParent<XROrigin>();
            if (origin == null) return;

            playerOrigin = origin.transform;
            grabHandPosition = playerOrigin.InverseTransformPoint(hand.position);
            grabHandRotation = Quaternion.Inverse(playerOrigin.rotation) * hand.rotation;
        }

        public override void Process(XRGrabInteractable grabInteractable,
            XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            // XRI의 원래 잡기 위치를 먼저 계산하여 보정이 계속 누적되지 않도록 함.
            base.Process(grabInteractable, updatePhase, ref targetPose, ref localScale);
            if (!isActiveAndEnabled || hand == null || playerOrigin == null ||
                grabInteractable.interactorsSelecting.Count != 1) return;

            // 플레이어 기준 좌표를 사용하여 스틱 이동과 회전은 확대하지 않음.
            Vector3 handPosition = playerOrigin.InverseTransformPoint(hand.position);
            Vector3 movement = handPosition - grabHandPosition;
            float extraMultiplier = Mathf.Clamp(movementMultiplier, 1f, 2.5f) - 1f;

            // 앞뒤·좌우·위아래 이동에 150%를 더함. 원래 손 위치로 돌아오면 보정도 0.
            targetPose.position += playerOrigin.TransformVector(movement * extraMultiplier);

            // 잡았을 때부터 손목이 돌아간 각도에 50%를 추가. 스틱 회전은 제외.
            Quaternion handRotation = Quaternion.Inverse(playerOrigin.rotation) * hand.rotation;
            Quaternion rotationChange = handRotation * Quaternion.Inverse(grabHandRotation);
            Quaternion extraRotation = Quaternion.SlerpUnclamped(Quaternion.identity, rotationChange,
                Mathf.Clamp(rotationMultiplier, 1f, 1.5f) - 1f);
            extraRotation = playerOrigin.rotation * extraRotation * Quaternion.Inverse(playerOrigin.rotation);

            // 망치 중심이 아닌 손잡이의 AttachPoint를 축으로 회전하여 잡는 위치 유지.
            Transform attach = grabInteractable.GetAttachTransform(grabInteractable.interactorsSelecting[0]);
            Vector3 attachOffset = Quaternion.Inverse(transform.rotation) * (attach.position - transform.position);
            Vector3 gripPosition = targetPose.position + targetPose.rotation * attachOffset;
            targetPose.rotation = extraRotation * targetPose.rotation;
            targetPose.position = gripPosition - targetPose.rotation * attachOffset;
        }
    }
}
