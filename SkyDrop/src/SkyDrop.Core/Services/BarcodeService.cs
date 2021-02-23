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

        public async Task<string> ScanBarcode()
        {
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan();

            var message = result == null ? "Scanned NULL Barcode" : "Scanned Barcode: " + result.Text;
            log.Trace(message);
            return message;
        }
    }

    public interface IBarcodeService
    {
        Task<string> ScanBarcode();
    }
}
