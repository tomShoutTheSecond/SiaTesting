using System;
using System.Collections.Generic;
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
        public List<StagedFileDVM> StagedFiles { get; set; } = new List<StagedFileDVM>();

        public string SkylinksText { get; set; }

        public bool IsLoading { get; set; }

        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;

        public IMvxAsyncCommand SelectFileCommand { get; set; }
        public IMvxCommand SelectImageCommand { get; set; }
        public IMvxCommand FileTapCommand { get; set; }
        public IMvxCommand UploadCommand { get; set; }

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
            UploadCommand = new MvxAsyncCommand(UploadStagedFiles);
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

        private SkyFileDVM GetSkyFileDVM(SkyFile skyFile, bool isNew = false)
        {
            return new SkyFileDVM(skyFile,
                new MvxCommand(() => SetSelectedFile(skyFile)),
                new MvxCommand(() => FileTapCommand.Execute(skyFile)),
                new MvxAsyncCommand(() => CopyFileLinkToClipboard(skyFile)),
                new MvxCommand(() => DeleteSkyFileFromList(skyFile)))
            { IsNew = isNew };
        }

        public void StageFile(StagedFile stagedFile)
        {
            StagedFiles.Add(new StagedFileDVM(stagedFile));
            StagedFiles = new List<StagedFileDVM>(StagedFiles);
            _ = RaisePropertyChanged(() => StagedFiles);
        }

        private void DeleteSkyFileFromList(SkyFile file)
        {
            SkyFiles = new List<SkyFileDVM>(SkyFiles.Where(f => f.SkyFile.Skylink != file.Skylink));
            _ = RaisePropertyChanged(() => SkyFiles);
            storageService.DeleteSkyFile(file);
        }

        private async Task UploadStagedFiles()
        {
            IsLoading = true;
            _ = RaisePropertyChanged(() => IsLoading);

            foreach (var stagedFile in StagedFiles)
            {
                stagedFile.IsLoading = true;
                StagedFiles = new List<StagedFileDVM>(StagedFiles);
                _ = RaisePropertyChanged(() => StagedFiles);

                await UploadFile(stagedFile.StagedFile);

                stagedFile.IsLoading = false;
                StagedFiles = new List<StagedFileDVM>(StagedFiles.Where(f => f.StagedFile.Filename != stagedFile.StagedFile.Filename));
                _ = RaisePropertyChanged(() => StagedFiles);
            }

            IsLoading = false;
            _ = RaisePropertyChanged(() => IsLoading);
        }

        private async Task UploadFile(StagedFile stagedFile)
        {
            try
            {
                var skyFile = await apiService.UploadFile(stagedFile.Filename, stagedFile.Data);
                Log.Trace("UPLOAD COMPLETE: " + skyFile.Skylink);

                var existingFile = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == skyFile.Skylink);
                if (existingFile != null)
                {
                    var message = "File already uploaded";
                    Log.Trace(message);
                    userDialogs.Toast(message);

                    //bring existing file to the top of the list and highlight it green
                    existingFile.IsNew = true;
                    SkyFiles.Remove(existingFile);
                    SkyFiles.Insert(0, existingFile);
                    SkyFiles = new List<SkyFileDVM>(SkyFiles);
                    _ = RaisePropertyChanged(() => SkyFiles);

                    return;
                }

                SkyFiles.Insert(0, GetSkyFileDVM(skyFile, isNew: true));
                SkyFiles = new List<SkyFileDVM>(SkyFiles);
                _ = RaisePropertyChanged(() => SkyFiles);

                storageService.SaveSkyFiles(skyFile);

                SkylinksText = GetSkyLinksText();
            }
            catch (Exception e)
            {
                Log.Exception(e);
                userDialogs.Toast("File upload failed");
            }
        }

        private void SetSelectedFile(SkyFile selectedFile)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);

            foreach (var skyFile in SkyFiles)
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
