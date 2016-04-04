using UnityEngine;
using System;
using Zenject;

namespace MistRidge
{
    public class TutorialManager : IInitializable
    {
        private readonly Settings settings;
        private readonly TutorialSignal tutorialSignal;
        private readonly DisplayManager displayManager;
        private readonly DeathManager deathManager;
        private readonly CinematicManager cinematicManager;
        private readonly CameraManager cameraManager;
        private readonly CameraRigManager cameraRigManager;
        private readonly InputManager inputManager;
        private readonly PlayerManager playerManager;

        public TutorialManager(
                Settings settings,
                TutorialSignal tutorialSignal,
                DisplayManager displayManager,
                DeathManager deathManager,
                CameraManager cameraManager,
                CameraRigManager cameraRigManager,
                CinematicManager cinematicManager,
                InputManager inputManager,
                PlayerManager playerManager)
        {
            this.settings = settings;
            this.tutorialSignal = tutorialSignal;
            this.displayManager = displayManager;
            this.deathManager = deathManager;
            this.cameraManager = cameraManager;
            this.cameraRigManager = cameraRigManager;
            this.cinematicManager = cinematicManager;
            this.inputManager = inputManager;
            this.playerManager = playerManager;
        }

        public void Initialize()
        {
            tutorialSignal.Event += OnTutorialEvent;
        }

        private void OnTutorialEvent(TutorialType tutorialType)
        {
            switch(tutorialType)
            {
                case TutorialType.Start:
                    StartTutorial();
                    break;

                case TutorialType.End:
                    EndTutorial();
                    break;
            }
        }

        private void StartTutorial()
        {
            displayManager.UpdateCinematic(true);
            deathManager.IsActive = false;
            cameraManager.ZoomOverride = settings.zoomOverride;
            cameraManager.ZoomOverrideEnabled = true;
            cameraRigManager.RigPosition = settings.rigPosition;
            cinematicManager.CinematicType = CinematicType.StartingZone;

            TurnipView turnipView = cinematicManager.StartingZoneView.TurnipView;
            turnipView.Alert();
        }

        private void EndTutorial()
        {
            displayManager.UpdateCinematic(false);
            deathManager.IsTutorial = false;
            deathManager.IsActive = true;
            cameraManager.ZoomOverrideEnabled = false;
            cameraRigManager.ResetRig();
            cinematicManager.CinematicType = CinematicType.None;

            foreach (Input input in inputManager.Inputs)
            {
                if (!playerManager.HasPlayerFacade(input))
                {
                    continue;
                }

                PlayerFacade playerFacade = playerManager.PlayerFacade(input);

                displayManager.Display(input.DeviceNum, playerFacade.CharacterType);
            }
        }

        [Serializable]
        public class Settings
        {
            public float zoomOverride;
            public Vector3 rigPosition;
        }
    }
}
