using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class MainViewModel : BaseViewModel
    {
        public List<SkyFile> SkyFiles { get; set; } = new List<SkyFile>();

        public string SkylinksText { get; set; }

        public bool IsLoading { get; set; }

        private IApiService apiService;

        public MainViewModel(IApiService apiService)
        {
            Title = "Upload";

            SelectFileCommand = new MvxCommand(DoSomething);

            this.apiService = apiService;
        }

        public IMvxCommand SelectFileCommand { get; set; }

        public Action SelectTheFileNative { get; set; }

        private void DoSomething()
        {
            SelectTheFileNative.Invoke();
        }

        public async Task UploadFile(string filename, byte[] file)
        {
            IsLoading = true;

            var skyFile = await apiService.UploadFile(filename, file);
            Console.WriteLine("UPLOAD COMPLETE: " + skyFile.Skylink);

            SkyFiles.Add(skyFile);

            SkylinksText = GetSkyLinksText();

            IsLoading = false;

            RaiseAllPropertiesChanged();
        }

        private string GetSkyLinksText()
        {
            var stringBuilder = new StringBuilder();
            foreach (var skyfile in SkyFiles)
                stringBuilder.Append(skyfile.Filename + "\n");

            return stringBuilder.ToString();
        }
    }
}
