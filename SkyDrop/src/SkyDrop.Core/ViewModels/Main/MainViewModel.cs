using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Commands;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class MainViewModel : BaseViewModel
    {
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
            var uploadResponse = await apiService.UploadFile(filename, file);
            Console.WriteLine(uploadResponse);
        }
    }
}
