using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class MainViewModel : BaseViewModel
    {
        public SkyFile LastUploadedSkyFile { get; set; }

        public List<SkyFile> SkyFiles { get; set; } = new List<SkyFile>();

        public string FileNameForSkyLink { get; set; }

        public bool IsLoading { get; set; }

        private IApiService apiService;
        private IUserDialogs crossUiService;

        public IMvxCommand SelectFileCommand { get; set; }
        public IMvxCommand CopyLatestSkyLinkCommand { get; set; }

        public MainViewModel(IApiService apiService, IUserDialogs crossUiService)
        {
            Debug.WriteLine("MainViewModel() ctor");

            Title = "Upload";

            CopyLatestSkyLinkCommand = new MvxAsyncCommand(CopyLastSkylink);

            this.apiService = apiService;
            this.crossUiService = crossUiService;
        }

        /// <summary>
        /// Copies the latest uploaded SkyFile's skylink to the clipboard.
        /// </summary>
        /// <returns></returns>
        private async Task CopyLastSkylink()
        {
            string lastSkyLink = LastUploadedSkyFile?.Skylink;

            if (string.IsNullOrEmpty(lastSkyLink))
                return;

            await Xamarin.Essentials.Clipboard.SetTextAsync(lastSkyLink);

            crossUiService.Toast("Copied link to clipboard");
        }

        public async Task UploadFile(string filename, byte[] file)
        {
            IsLoading = true;
            _ = RaisePropertyChanged(() => IsLoading);

            var skyFile = await apiService.UploadFile(filename, file);
            Debug.WriteLine("UPLOAD COMPLETE: " + skyFile.Skylink);

            LastUploadedSkyFile = skyFile;

            SkyFiles.Add(skyFile);

            FileNameForSkyLink = GetFileNameForSkyLink();

            IsLoading = false;

            _ = RaiseAllPropertiesChanged();
        }

        private string GetFileNameForSkyLink()
        {
            var stringBuilder = new StringBuilder();
            foreach (var skyfile in SkyFiles)
                stringBuilder.Append(skyfile.Filename + "\n");

            return stringBuilder.ToString();
        }
    }
}
