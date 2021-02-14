using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class MainViewModel : BaseViewModel
    {
        public List<SkyFileDVM> SkyFiles { get; set; } = new List<SkyFileDVM>();

        public SkyFile LastSkyFile { get; set; }

        public string SkylinksText { get; set; }

        public bool IsLoading { get; set; }

        private IApiService apiService;
        private IStorageService storageService;
        private IUserDialogs userDialogs;

        public IMvxAsyncCommand SelectFileCommand { get; set; }
        private Func<Task> _selectFileAsyncFunc;
        public Func<Task> SelectFileAsyncFunc
        {
            get => _selectFileAsyncFunc ?? throw new ArgumentNullException(nameof(SelectFileAsyncFunc));
            set => _selectFileAsyncFunc = value;
        }
        public IMvxAsyncCommand CopyLatestSkyLinkCommand { get; set; }
        public IMvxCommand FileTapCommand { get; set; }


        public MainViewModel(IApiService apiService, IStorageService storageService, IUserDialogs userDialogs)
        {
            Title = "Upload";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;

            SelectFileCommand = new MvxAsyncCommand(async () => await SelectFileAsyncFunc());
            CopyLatestSkyLinkCommand = new MvxAsyncCommand(async () => await CopyLastFileLinkToClipboard());
        }

        private async Task CopyLastFileLinkToClipboard()
        {
            string lastUploadedSkyLink = LastSkyFile?.Skylink;
            if (string.IsNullOrEmpty(lastUploadedSkyLink))
                return;

            await Xamarin.Essentials.Clipboard.SetTextAsync(lastUploadedSkyLink);

            System.Diagnostics.Debug.WriteLine("Set clipboard text to " + lastUploadedSkyLink);
            userDialogs.Toast("Copied SkyLink to clipboard");
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            LoadSkyFiles();
        }

        private void LoadSkyFiles()
        {
            SkyFiles = GetSkyFileDVMs(storageService.LoadSkyFiles());

            RaiseAllPropertiesChanged();
        }

        public List<SkyFileDVM> GetSkyFileDVMs(List<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDVM>();
            foreach (var skyFile in skyFiles)
            {
                dvms.Add(new SkyFileDVM(skyFile, new MvxCommand(() => FileTapCommand.Execute(skyFile))));
            }

            return dvms;
        }

        public async Task UploadFile(string filename, byte[] file)
        {
            IsLoading = true;
            _ = RaisePropertyChanged(() => IsLoading);

            var skyFile = await apiService.UploadFile(filename, file);
            Console.WriteLine("UPLOAD COMPLETE: " + skyFile.Skylink);

            SkyFiles.Add(new SkyFileDVM(skyFile, new MvxCommand(() => FileTapCommand.Execute(skyFile))));

            LastSkyFile = skyFile;

            storageService.SaveSkyFiles(skyFile);

            SkylinksText = GetSkyLinksText();

            IsLoading = false;

            _ = RaiseAllPropertiesChanged();
        }

        private string GetSkyLinksText()
        {
            var stringBuilder = new StringBuilder();
            foreach (var skyfile in SkyFiles)
                stringBuilder.Append(skyfile.SkyFile.Filename + "\n");

            return stringBuilder.ToString();
        }
    }
}
