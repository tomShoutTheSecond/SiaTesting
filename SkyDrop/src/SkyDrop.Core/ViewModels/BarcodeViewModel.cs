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
    public class BarcodeViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IBarcodeService barcodeService;

        public IMvxCommand GenerateBarcodeCommand { get; set; }

        public BarcodeViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             IBarcodeService barcodeService,
                             ILog log) : base(singletonService)
        {
            Title = "Scan";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.barcodeService = barcodeService;

            GenerateBarcodeCommand = new MvxCommand(GenerateBarcode);
        }

        private void GenerateBarcode()
        {

        }

    }
}
