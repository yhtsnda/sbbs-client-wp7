﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;

namespace sbbs_client_wp7
{
    using Sbbs;

    public partial class TopicPage : PhoneApplicationPage
    {
        // 每页显示多少话题
        const int pageSize = 10;
        // 当前页数
        int currentPage = 0;

        public TopicPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel.CurrentTopic;
        }

        private void LoadMore_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.CurrentTopic.IsLoaded = false;
            LoadMore.IsEnabled = false;
            App.Service.Topic(App.ViewModel.CurrentTopic.Board, App.ViewModel.CurrentTopic.Id, (currentPage + 1) * pageSize, pageSize, delegate(ObservableCollection<TopicViewModel> topics, bool success, string error)
            {
                // 判断后面是否还有内容
                if (error == null && topics.Count < pageSize)
                {
                    LoadMore.Visibility = Visibility.Collapsed;
                    LoadMore.IsEnabled = false;
                }
                else
                {
                    LoadMore.Visibility = Visibility.Visible;
                    LoadMore.IsEnabled = true;
                }

                App.ViewModel.CurrentTopic.IsLoaded = true;
                if (error == null)
                {
                    currentPage++;

                    foreach (TopicViewModel topic in topics)
                        App.ViewModel.CurrentTopic.Topics.Add(topic);
                }
                else
                {
                    MessageBox.Show("网络错误");
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.NavigationContext.QueryString.ContainsKey("board"))
            {
                string board = this.NavigationContext.QueryString["board"];
                int id = int.Parse(this.NavigationContext.QueryString["id"]);

                // 跳转到其他话题时清空并重载
                if (board != App.ViewModel.CurrentTopic.Board || id != App.ViewModel.CurrentTopic.Id)
                {
                    // 重置标题
                    App.ViewModel.CurrentTopic.Id = id;
                    App.ViewModel.CurrentTopic.Board = board;
                    App.ViewModel.CurrentTopic.Title = this.NavigationContext.QueryString["title"];

                    // 清空已有内容
                    if (App.ViewModel.CurrentTopic.Topics != null)
                        App.ViewModel.CurrentTopic.Topics.Clear();

                    currentPage = 0;
                    LoadTopics();
                }
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            currentPage = 0;
            LoadTopics();
        }

        private void Reply_Click(object sender, EventArgs e)
        {
            TopicViewModel topic = TopicsList.SelectedItem as TopicViewModel;
            if (topic == null)
                NavigationService.Navigate(new Uri("/PostPage.xaml?title=" + HttpUtility.UrlEncode(App.ViewModel.CurrentTopic.Title) + "&board=" + App.ViewModel.CurrentTopic.Board
                        + "&reid=" + App.ViewModel.CurrentTopic.Id, UriKind.Relative));
            else
                NavigationService.Navigate(new Uri("/PostPage.xaml?title=" + HttpUtility.UrlEncode(topic.Title) + "&board=" + topic.Board + "&reid=" + topic.Id, UriKind.Relative));
        }

        private void MenuReply_Click(object sender, EventArgs e)
        {
            TopicViewModel topic = (sender as MenuItem).DataContext as TopicViewModel;
            NavigationService.Navigate(new Uri("/PostPage.xaml?title=" + HttpUtility.UrlEncode(topic.Title) + "&board=" + topic.Board + "&reid=" + topic.Id, UriKind.Relative));
        }

        private void LoadTopics()
        {
            App.ViewModel.CurrentTopic.IsLoaded = false;
            LoadMore.IsEnabled = false;

            // 重新加载
            App.Service.Topic(App.ViewModel.CurrentTopic.Board, App.ViewModel.CurrentTopic.Id, currentPage * pageSize, pageSize, delegate(ObservableCollection<TopicViewModel> topics, bool success, string error)
            {
                if (error == null && topics.Count < pageSize)
                {
                    LoadMore.Visibility = Visibility.Collapsed;
                    LoadMore.IsEnabled = false;
                }
                else
                {
                    LoadMore.Visibility = Visibility.Visible;
                    LoadMore.IsEnabled = true;
                }

                App.ViewModel.CurrentTopic.IsLoaded = true;
                if (error == null)
                    App.ViewModel.CurrentTopic.Topics = topics;
            });
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox).SelectedIndex == 0)
                (sender as ListBox).SelectedIndex = -1;
        }
    }
}