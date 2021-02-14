using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private List<SkyFileDVM> GetSkyFileDVMs(List<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDVM>();
            foreach (var skyFile in skyFiles)
            {
                dvms.Add(GetSkyFileDVM(skyFile));
            }

            return dvms;
        }

        private SkyFileDVM GetSkyFileDVM(SkyFile skyFile)
        {
            return new SkyFileDVM(skyFile,
                new MvxCommand(() => SetSelectedFile(skyFile)),
                new MvxCommand(() => FileTapCommand.Execute(skyFile)));
        }

        public async Task UploadFile(string filename, byte[] file)
        {
            IsLoading = true;
            _ = RaisePropertyChanged(() => IsLoading);

            var skyFile = await apiService.UploadFile(filename, file);
            System.Diagnostics.Debug.WriteLine("UPLOAD COMPLETE: " + skyFile.Skylink);

            var existingFile = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == skyFile.Skylink);
            if (existingFile != null)
            {
                var message = "FILE ALREADY UPLOADED!";
                System.Diagnostics.Debug.WriteLine(message);
                userDialogs.Toast(message);
                IsLoading = false;
                _ = RaisePropertyChanged(() => IsLoading);
                return;
            }

            SkyFiles.Add(GetSkyFileDVM(skyFile));
            SkyFiles = new List<SkyFileDVM>(SkyFiles);

            LastSkyFile = skyFile;

            storageService.SaveSkyFiles(skyFile);

            SkylinksText = GetSkyLinksText();

            IsLoading = false;

            _ = RaiseAllPropertiesChanged();
        }

        private void SetSelectedFile(SkyFile selectedFile)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);

            foreach(var skyFile in SkyFiles)
            {
                skyFile.IsSelected = false;
            }

            selectedFileDVM.IsSelected = true;

            SkyFiles = new List<SkyFileDVM>(SkyFiles);

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
