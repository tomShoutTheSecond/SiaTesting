using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class FilesViewModel : BaseViewModel
    {
        public MvxObservableCollection<SkyFileDVM> SkyFiles { get; } = new MvxObservableCollection<SkyFileDVM>();
        public MvxObservableCollection<StagedFileDVM> StagedFiles { get; } = new MvxObservableCollection<StagedFileDVM>();

        public string SkylinksText { get; set; }

        public bool IsLoading { get; set; }

        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly ILog _log;

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

        public FilesViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "Upload";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            _log = log;
            SelectFileCommand = new MvxAsyncCommand(async () => await SelectFileAsyncFunc());
            SelectImageCommand = new MvxAsyncCommand(async () => await SelectImageAsyncFunc());
            UploadCommand = new MvxAsyncCommand(UploadStagedFiles);
        }

        public override Task Initialize()
        {
            LoadSkyFiles();

            return base.Initialize();
        }

        private void LoadSkyFiles()
        {
            var newSkyFiles = GetSkyFileDVMs(storageService.LoadSkyFiles());
            SkyFiles.SwitchTo(newSkyFiles);

            // TODO remove if possible
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
        }

        private void DeleteSkyFileFromList(SkyFile file)
        {
            var newSkyFiles = new List<SkyFileDVM>(SkyFiles.Where(f => f.SkyFile.Skylink != file.Skylink));
            SkyFiles.SwitchTo(newSkyFiles);

            storageService.DeleteSkyFile(file);
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

        private async Task UploadStagedFiles()
        {
            IsLoading = true;

            // make local copy of StagedFiles for safe enumeration in foreach
            var currentlyStagedFiles = new List<StagedFileDVM>(StagedFiles);

            foreach (var stagedFile in currentlyStagedFiles)
            {
                // todo make StagedFileDVM method called UploadFile() which does this logic
                stagedFile.IsLoading = true;
                var newSkyFiles = new List<StagedFileDVM>(StagedFiles);
                StagedFiles.SwitchTo(newSkyFiles);

                await UploadFile(stagedFile.StagedFile);

                stagedFile.IsLoading = false;

                StagedFiles.Remove(stagedFile);
            }

            IsLoading = false;
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

                    existingFile.IsNew = true;
                    int indexOfExistingFile = SkyFiles.IndexOf(existingFile);
                    SkyFiles.Move(indexOfExistingFile, 0);

                    return;
                }

                SkyFiles.Insert(0, GetSkyFileDVM(skyFile, isNew: true));

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
