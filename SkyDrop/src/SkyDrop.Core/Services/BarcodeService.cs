using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;
using ZXing.QrCode;

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

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            var writer = new MultiFormatWriter();
            var matrix = writer.encode(text, BarcodeFormat.QR_CODE, width, height);
            return matrix;
        }
    }

    public interface IBarcodeService
    {
        Task<string> ScanBarcode();

        BitMatrix GenerateBarcode(string text, int width, int height);
    }
}
