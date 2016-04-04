using UnityEngine;
using System;

namespace MistRidge
{
    public class TutorialTriggerView : MonoView
    {
        [SerializeField]
        private TutorialView tutorialView;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            tutorialView.OnTutorialTrigger();
        }
    }
}
