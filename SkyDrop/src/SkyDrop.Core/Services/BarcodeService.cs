using System;
using System.Threading.Tasks;
using ZXing.Mobile;

namespace SkyDrop.Core.Services
{
    public class BarcodeService : IBarcodeService
    {
        private readonly ILog log;

        public BarcodeService(ILog log)
        {
            this.log = log;
        }

        public async Task ScanBarcode()
        {
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan();

            if (result != null)
                log.Trace("Scanned Barcode: " + result.Text);
            else
                log.Trace("Scanned NULL Barcode");
        }
    }

    public interface IBarcodeService
    {
        Task ScanBarcode();
    }
}
