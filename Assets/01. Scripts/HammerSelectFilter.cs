using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Whack_A_Mole
{
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
            bool isCorrectSide = 
                hammer.hammerSide == HandSide.Left ? interactor.handedness == InteractorHandedness.Left
                : interactor.handedness == InteractorHandedness.Right;
            if (isCorrectSide == false && interactor.isSelectActive)
            {
                UIController.Instance.ShowWrongSideAnnounce().Forget();
            }
            return isCorrectSide;
        }
    }
}