using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class DropViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly IBarcodeService barcodeService;

        public IMvxCommand SendCommand { get; set; }
        public IMvxCommand ReceiveCommand { get; set; }
        public IMvxCommand<SkyFile> OpenFileCommand { get; set; }

        private StagedFile stagedFile { get; set; }
        private string errorMessage;

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

        public DropViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IBarcodeService barcodeService,
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "Drop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.barcodeService = barcodeService;

            SendCommand = new MvxAsyncCommand(SendFile);
            ReceiveCommand = new MvxAsyncCommand(ReceiveFile);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            //show error message after the qr code scanner view has closed to avoid exception
            if (!string.IsNullOrEmpty(errorMessage))
                userDialogs.Toast(errorMessage);
        }

        private async Task SendFile()
        {
        }

        private async Task ReceiveFile()
        {
            try
            {
                //prompt user with instructions
                var message = "Scan the QR code on the sender device to receive the file";
                await userDialogs.AlertAsync(message);

                //open the QR code scan view
                var codeJson = await barcodeService.ScanBarcode();
                var skyFile = JsonConvert.DeserializeObject<SkyFile>(codeJson);

                //open the file in browser
                OpenFileCommand.Execute(skyFile);
            }
            catch(JsonException e)
            {
                Log.Exception(e);

                //show error message after the qr code scanner view has closed to avoid exception
                errorMessage = "Error: Invalid QR code";
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private async Task UploadFile()
        {
            var skyFile = await apiService.UploadFile(stagedFile.Filename, stagedFile.Data);
            var skyFileJson = JsonConvert.SerializeObject(skyFile);

            var barcode = barcodeService.GenerateBarcode(skyFileJson, 200, 200);

        }
    }
}
