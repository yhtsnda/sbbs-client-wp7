﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace sbbs_client_wp7
{
    using Sbbs;
    using System.Collections.ObjectModel;

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Add Tilt effect for Tile
            TiltEffect.TiltableItems.Add(typeof(Tile));

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            // 登录后刷新收藏夹
            App.ViewModel.LoginChanged += delegate(object sender, bool isLogin)
            {
                LoadFavorates();
            };
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                LoadTopten();
                LoadFavorates();
            }
        }

        // 登录
        private void Login_Click(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        // 注销
        private void Logout_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("真的要注销吗？", "注销", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
                App.ViewModel.IsLogin = false;
        }

        // 刷新十大
        private void RefreshTopten_Click(object sender, MouseButtonEventArgs e)
        {
            App.ViewModel.IsToptenLoaded = false;
            LoadTopten();
        }

        // 刷新收藏夹
        private void RefreshFavorates_Click(object sender, MouseButtonEventArgs e)
        {
            App.ViewModel.IsFavoratesLoaded = false;
            LoadFavorates();
        }

        // 点击收藏夹
        
        private void Favorates_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                ListBox list = sender as ListBox;
                BoardViewModel board = e.AddedItems[0] as BoardViewModel;

                // 收藏夹目录
                if (board.Leaf != true && board.EnglishName != "..")
                {
                    App.ViewModel.FavoratesDirectory.AddLast(App.ViewModel.FavoratesItems);
                    App.ViewModel.FavoratesItems = board.Boards;
                    return;
                }
                else if (board.EnglishName == "..")
                {
                    App.ViewModel.FavoratesItems = App.ViewModel.FavoratesDirectory.Last.Value;
                    App.ViewModel.FavoratesDirectory.RemoveLast();
                    return;
                }

                this.NavigationService.Navigate(new Uri("/BoardPage.xaml?board=" + board.EnglishName + "&description=" + board.Description, UriKind.Relative));

                // 清除选择，否则同样的项目无法点击第二次
                (sender as ListBox).SelectedIndex = -1;
            }
        }

        // 点击十大
        private void Topten_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                TopicViewModel topic = e.AddedItems[0] as TopicViewModel;

                this.NavigationService.Navigate(
                    new Uri("/TopicPage.xaml?board=" + topic.Board + "&id=" + topic.Id + "&title=" + HttpUtility.UrlEncode(topic.Title), UriKind.Relative));

                // 清除选择，否则同样的项目无法点击第二次
                (sender as ListBox).SelectedIndex = -1;
            }
        }

        // 载入收藏夹
        private void LoadFavorates()
        {
            // 登录时载入收藏夹，未登陆时清空
            if (App.ViewModel.IsLogin)
            {
                App.Service.Favorates(delegate(ObservableCollection<BoardViewModel> boards, bool success, string error)
                {
                    App.ViewModel.IsFavoratesLoaded = true;
                    if (error != null)
                        return;

                    App.ViewModel.FavoratesDirectory.Clear();
                    App.ViewModel.FavoratesItems = boards;
                });
            }
            else
            {
                App.ViewModel.IsFavoratesLoaded = true;
                App.ViewModel.FavoratesItems = null;
            }
        }

        // 载入十大
        public void LoadTopten()
        {
            App.Service.Topten(delegate(ObservableCollection<TopicViewModel> topics, bool success, string error)
            {
                App.ViewModel.IsToptenLoaded = true;
                if (error != null)
                    return;

                // 刷新十大
                App.ViewModel.ToptenItems = topics;
            });
        }
    }
}