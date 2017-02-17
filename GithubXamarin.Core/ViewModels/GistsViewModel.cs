﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GithubXamarin.Core.Contracts.Service;
using GithubXamarin.Core.Messages;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Octokit;

namespace GithubXamarin.Core.ViewModels
{
    public class GistsViewModel : BaseViewModel
    {
        #region Properties and Commands

        private readonly IGistDataService _gistDataService;

        private ObservableCollection<Gist> _gists;
        public ObservableCollection<Gist> Gists
        {
            get { return _gists;}
            set
            {
                _gists = value;
                RaisePropertyChanged(() => Gists);
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex;}
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                _refreshCommand = _refreshCommand ?? new MvxAsyncCommand(async () => await Refresh());
                return _refreshCommand;
            }
        }

        private ICommand _gistClickCommand;
        public ICommand GistClickCommand
        {
            get
            {
                _gistClickCommand = _gistClickCommand ?? new MvxCommand(NavigateToGist);
                return _gistClickCommand;
            }
        }

        private string _userName;

        #endregion

        public GistsViewModel(IGistDataService gistDataService, IGithubClientService githubClientService, IMvxMessenger messenger, IDialogService dialogService) : base(githubClientService, messenger, dialogService)
        {
            _gistDataService = gistDataService;
        }

        public async void Init(string userName)
        {
            _userName = userName;
            await Refresh();
        }

        private async Task Refresh()
        {
            Messenger.Publish(new LoadingStatusMessage(this) { IsLoadingIndicatorActive = true });

            if (!IsInternetAvailable())
            {
                await DialogService.ShowDialogASync("", "Internet is not available");
                return;
            }
            if (string.IsNullOrWhiteSpace(_userName))
            {
                Messenger.Publish(new AppBarHeaderChangeMessage(this) { HeaderTitle = $"Your Gists" });
                Gists = await _gistDataService.GetAllGistsForCurrentUser(GithubClientService.GetAuthorizedGithubClient());
            }
            else
            {
                Gists = await _gistDataService.GetAllPublicGistsForUser(_userName, GithubClientService.GetAuthorizedGithubClient());
                Messenger.Publish(new AppBarHeaderChangeMessage(this) { HeaderTitle = $"Gists For {Gists[0].Owner.Name}" });
            }
            Messenger.Publish(new LoadingStatusMessage(this) { IsLoadingIndicatorActive = false });
        }

        private void NavigateToGist()
        {
            if (SelectedIndex != null)
            {
                ShowViewModel<GistViewModel>(new {gistId = Gists[SelectedIndex].Id});
            }
        }
    }
}
