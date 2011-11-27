﻿namespace IntergalacticControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using IntergalacticControls.Classes;

    /// <summary>
    /// Provides a way to make automated
    /// </summary>
    public class TabBarController : HorizontalStackController
    {
        /// <summary>
        /// List of open tabs
        /// </summary>
        private List<TabButton> tabs;

        /// <summary>
        /// Initializes a new instance of the TabBarController class
        /// </summary>
        public TabBarController() : base()
        {
            this.tabs = new List<TabButton>();

            Manager.Instance.OnNewTabAdded += this.AddTab;
            Manager.Instance.OnTabChanged += this.SwitchTab;
            Manager.Instance.OnTabClosed += this.RemoveTab;
        }

        /// <summary>
        /// Adds a tab to the UI
        /// </summary>
        /// <param name="mng">The manager</param>
        /// <param name="tab">The tab</param>
        private void AddTab(Manager mng, Tab tab)
        {
            TabButton newTab = new TabButton(tab);
            newTab.MouseDown += new MouseButtonEventHandler(this.NewTab_MouseDown);
            this.tabs.Add(newTab);
            this.MainStackPanel.Children.Add(newTab);

            if (this.tabs.Count == 1)
            {
                this.tabs[0].IsSelected = true;
            }
        }

        /// <summary>
        /// Removes tab form the UI
        /// </summary>
        /// <param name="mng">The manager</param>
        /// <param name="tab">The tab</param>
        private void RemoveTab(Manager mng, Tab tab)
        {
            int i;
            for (i = 0; i < this.tabs.Count; i++)
            {
                if (this.tabs[i].Tab.Name == tab.Name)
                {
                    this.MainStackPanel.Children.Remove(this.tabs[i]);
                    this.tabs.RemoveAt(i);
                    return;
                }
            }

            this.DeselectAllTabs();
            if (i == 0)
            {
                this.tabs[0].IsSelected = true;
            }
            else
            {
                this.tabs[i - 1].IsSelected = true;
            }
        }

        /// <summary>
        /// Deselects all tabs
        /// </summary>
        private void DeselectAllTabs()
        {
            for (int i = 0; i < this.tabs.Count; i++)
            {
                this.tabs[i].IsSelected = false;
            }
        }

        /// <summary>
        /// Switches from one tab to another
        /// </summary>
        /// <param name="mng">The manager</param>
        /// <param name="tab">THe tab</param>
        private void SwitchTab(Manager mng, Tab tab)
        {
            this.DeselectAllTabs();
            for (int i = 0; i < this.tabs.Count; i++)
            {
                if (this.tabs[i].Tab.Name == tab.Name)
                {
                    this.tabs[i].IsSelected = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Mouse donw function to switch between tabs
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event arguments</param>
        private void NewTab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Manager.Instance.SwitchImage(((TabButton)sender).Tab.Name);
        }
    }
}