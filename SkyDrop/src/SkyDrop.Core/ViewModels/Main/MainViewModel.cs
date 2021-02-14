using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private IApiService apiService;
        private IStorageService storageService;

        public MainViewModel(IApiService apiService, IStorageService storageService)
        {
            Title = "Upload";

            this.apiService = apiService;
            this.storageService = storageService;

            SelectFileCommand = new MvxCommand(NativeFileSelect);
        }

        public IMvxCommand SelectFileCommand { get; set; }

        public IMvxCommand FileTapCommand { get; set; }

        public Action SelectTheFileNative { get; set; }

        private void NativeFileSelect()
        {
            SelectTheFileNative?.Invoke();
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
            RaisePropertyChanged(() => IsLoading);

            var skyFile = await apiService.UploadFile(filename, file);
            Console.WriteLine("UPLOAD COMPLETE: " + skyFile.Skylink);

            SkyFiles.Add(GetSkyFileDVM(skyFile));

            storageService.SaveSkyFiles(skyFile);

            SkylinksText = GetSkyLinksText();

            IsLoading = false;

            RaiseAllPropertiesChanged();
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

            RaiseAllPropertiesChanged();
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
