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

        public string SkylinksText { get; set; }

        public bool IsLoading { get; set; }

        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;

        public IMvxAsyncCommand SelectFileCommand { get; set; }
        public IMvxCommand SelectImageCommand { get; set; }
        public IMvxCommand FileTapCommand { get; set; }

        private Func<Task> _selectFileAsyncFunc;
        public Func<Task> SelectFileAsyncFunc
        {
            get => _selectFileAsyncFunc ?? throw new ArgumentNullException(nameof(SelectFileAsyncFunc));
            set => _selectFileAsyncFunc = value;
        }

        private Func<Task> _selectImageAsyncFunc;
        public Func<Task> SelectImageAsyncFunc
        {
            get => _selectImageAsyncFunc ?? throw new ArgumentNullException(nameof(SelectImageAsyncFunc));
            set => _selectImageAsyncFunc = value;
        }

        public MainViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             ILog log) : base(singletonService)
        {
            Title = "Upload";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;

            SelectFileCommand = new MvxAsyncCommand(async () => await SelectFileAsyncFunc());
            SelectImageCommand = new MvxAsyncCommand(async () => await SelectImageAsyncFunc());
        }

        private async Task CopyFileLinkToClipboard(SkyFile skyFile)
        {
            string skyLink = skyFile.Skylink;
            if (string.IsNullOrEmpty(skyLink))
                return;

            await Xamarin.Essentials.Clipboard.SetTextAsync(skyLink);

            Log.Trace("Set clipboard text to " + skyLink);
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
                new MvxCommand(() => FileTapCommand.Execute(skyFile)),
                new MvxAsyncCommand(() => CopyFileLinkToClipboard(skyFile)));
        }

        public async Task UploadFile(string filename, byte[] file)
        {
            IsLoading = true;
            _ = RaisePropertyChanged(() => IsLoading);

            try
            {
                var skyFile = await apiService.UploadFile(filename, file);
                Log.Trace("UPLOAD COMPLETE: " + skyFile.Skylink);

                var existingFile = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == skyFile.Skylink);
                if (existingFile != null)
                {
                    var message = "FILE ALREADY UPLOADED!";
                    Log.Trace(message);
                    userDialogs.Toast(message);
                    IsLoading = false;
                    _ = RaisePropertyChanged(() => IsLoading);
                    return;
                }

                SkyFiles.Add(GetSkyFileDVM(skyFile));
                SkyFiles = new List<SkyFileDVM>(SkyFiles);

                storageService.SaveSkyFiles(skyFile);

                SkylinksText = GetSkyLinksText();
            }
            catch(Exception e)
            {
                Log.Exception(e);
                userDialogs.Toast("File upload failed");
            }
            finally
            {
                IsLoading = false;

                _ = RaiseAllPropertiesChanged();
            }
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
