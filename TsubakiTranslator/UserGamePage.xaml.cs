﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using TsubakiTranslator.BasicLibrary;


namespace TsubakiTranslator
{
    /// <summary>
    /// UserConfig.xaml 的交互逻辑
    /// </summary>
    public partial class UserGamePage : UserControl
    {

        public ObservableCollection<GameData> GameItems { get; }
        private ObservableCollection<string> ProcessStrings { get; }

        public UserGamePage()
        {
            InitializeComponent();

            GameItems = FileHandler.DeserializeObject<ObservableCollection<GameData>>(System.AppDomain.CurrentDomain.BaseDirectory + @"config/GameData.json", new ObservableCollection<GameData>());
            ProcessStrings = new ObservableCollection<string>();

            GameList.ItemsSource = GameItems;
            GameProcessList.ItemsSource = ProcessStrings;
            HistoryGameProcessList.ItemsSource = ProcessStrings;

        }


        private void DeleteGame_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GameData item = (GameData)GameList.SelectedItem;
            GameItems.Remove(item);
        }

        private void OpenHistoryGame_Button_Click(object sender, RoutedEventArgs e)
        {
            SetProcessItems();

            GameData item = (GameData)GameList.SelectedItem;

            HistoryGameName.Text = item.GameName;
            HistoryGameHookCode.Text = item.HookCode;
            HistoryGameDuplicateTimes.Text = item.DuplicateTimes.ToString();

        }

        private async void AcceptGame_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            GameData item = (GameData)GameList.SelectedItem;

            int.TryParse(HistoryGameDuplicateTimes.Text, out int times);

            item.GameName = HistoryGameName.Text;
            item.HookCode = HistoryGameHookCode.Text;
            item.DuplicateTimes = times;

            string processInfo = (string)HistoryGameProcessList.SelectedItem;
            if (processInfo == null)
                return;


            Process gameProcess = GetGameProcessByProcessStrings(processInfo);

            TextHookHandler textHookHandler = new TextHookHandler(gameProcess, times);

            Window mainWindow = Window.GetWindow(this);
            mainWindow.Hide();
            TranslateWindow translateWindow = new TranslateWindow(mainWindow, textHookHandler);
            translateWindow.Show();


            if (HistoryGameHookCode.Text != null && HistoryGameHookCode.Text.Trim().Length != 0)
                await textHookHandler.AttachProcessByHookCode(HistoryGameHookCode.Text);

        }

        private void OpenGameByPid_Button_Click(object sender, RoutedEventArgs e)
        {
            SetProcessItems();
        }

        private async void AcceptProcess_Button_Click(object sender, RoutedEventArgs e)
        {
            string processInfo = (string)GameProcessList.SelectedItem;
            if (processInfo == null)
                return;

            Process gameProcess = GetGameProcessByProcessStrings(processInfo);

            int.TryParse(GameProcessDuplicateTimes.Text, out int times);

            GameData item = new GameData
            {
                HookCode = GameProcessHookCode.Text,
                DuplicateTimes = times,
                GameName = gameProcess.ProcessName
            };

            GameItems.Add(item);

            TextHookHandler textHookHandler = new TextHookHandler(gameProcess, times);

            Window mainWindow = Window.GetWindow(this);
            mainWindow.Hide();
            TranslateWindow translateWindow = new TranslateWindow(mainWindow, textHookHandler);
            translateWindow.Show();


            if (GameProcessHookCode.Text != null && GameProcessHookCode.Text.Trim().Length != 0)
                await textHookHandler.AttachProcessByHookCode(GameProcessHookCode.Text);
        }

        private void GamePage_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            ProcessStrings.Clear();
        }

        private void SetProcessItems()
        {
            Process[] ps = Process.GetProcesses();

            foreach (Process p in ps)
                ProcessStrings.Add($"{p.Id} - {p.ProcessName}");
        }

        private Process GetGameProcessByProcessStrings(string processString)
        {
            Regex reg = new Regex(@"(\d+?)\s-\s");
            Match match = reg.Match(processString);

            int.TryParse(match.Groups[1].Value, out int pid);

            Process gameProcess = Process.GetProcessById(pid);

            return gameProcess;
        }
    }
  

}
