using UnityEngine;
using System;
using Zenject;

namespace MistRidge
{
    public class DisplayInstaller : MonoInstaller
    {
        [SerializeField]
        private Settings settings;

        public override void InstallBindings()
        {
            InstallDisplay();
            InstallSettings();
        }

        private void InstallDisplay()
        {
            Container.Bind<DisplayManager>().ToSingle();
            Container.BindAllInterfacesToSingle<DisplayManager>();

            Container.Bind<GameDisplayView>().ToSinglePrefab(settings.gameDisplayPrefab);
            Container.Bind<CharacterSelectDisplayView>().ToSinglePrefab(settings.characterSelectDisplayPrefab);
        }

        private void InstallSettings()
        {
            Container.Bind<DisplayManager.Settings>().ToSingleInstance(settings.displayManagerSettings);
        }

        [Serializable]
        public class Settings
        {
            public GameObject gameDisplayPrefab;
            public GameObject characterSelectDisplayPrefab;
            public DisplayManager.Settings displayManagerSettings;
        }
    }
}
