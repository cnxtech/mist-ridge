using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Zenject;

namespace MistRidge
{
    public class StartMenuManager : IInitializable, IDisposable
    {
        private readonly Settings settings;
        private readonly MenuSignal menuSignal;
        private readonly SceneLoader sceneLoader;

        private int selectionIndex;

        public StartMenuManager(
            Settings settings,
            MenuSignal menuSignal,
            SceneLoader sceneLoader)
        {
            this.settings = settings;
            this.menuSignal = menuSignal;
            this.sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            menuSignal.Event += OnMenuActionReceived;

            ResetMenuItems();

            for (int i = 0; i < settings.menuItems.Length; ++i)
            {
                MenuItem menuItem = settings.menuItems[i];

                if (menuItem.item == settings.startMenuItem)
                {
                    selectionIndex = i;
                    break;
                }
            }

            MoveSelection(0);
        }

        public void Dispose()
        {
            menuSignal.Event -= OnMenuActionReceived;
        }

        private void OnMenuActionReceived(MenuSignalType menuSignalType)
        {
            switch (menuSignalType)
            {
                case MenuSignalType.Submit:
                    Select();
                    break;
                case MenuSignalType.Up:
                    MoveSelection(1);
                    break;
                case MenuSignalType.Down:
                    MoveSelection(-1);
                    break;
            }
        }

        private void Select()
        {
            StartMenuItem item = settings.menuItems[selectionIndex].item;

            switch(item)
            {
                case(StartMenuItem.NewGame):
                    NewGame();
                    break;
                case(StartMenuItem.Quit):
                    Quit();
                    break;
            }
        }

        private void MoveSelection(int indexDiff)
        {
            MenuItem selectedMenuItem = settings.menuItems[selectionIndex];
            selectedMenuItem.textItem.text = selectedMenuItem.textContent;

            selectionIndex = (selectionIndex + indexDiff + settings.menuItems.Length) % settings.menuItems.Length;
            selectedMenuItem = settings.menuItems[selectionIndex];
            selectedMenuItem.textItem.text = "#" + selectedMenuItem.textContent;
        }

        private void ResetMenuItems()
        {
            foreach(MenuItem menuItem in settings.menuItems)
            {
                menuItem.textItem.text = menuItem.textContent;
            }
        }

        private void NewGame()
        {
            sceneLoader.Load(settings.newGameSceneName);
        }

        private void Quit()
        {
            UnityApplication.Quit();
        }

        [Serializable]
        public class Settings
        {
            public string newGameSceneName;
            public StartMenuItem startMenuItem;
            public MenuItem[] menuItems;
        }

        [Serializable]
        public struct MenuItem
        {
            public Text textItem;
            public string textContent;
            public StartMenuItem item;
        }

        public enum StartMenuItem
        {
            NewGame,
            Quit,
        }
    }
}
