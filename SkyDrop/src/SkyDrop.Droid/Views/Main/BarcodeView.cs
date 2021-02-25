using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using Xamarin.Essentials;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class BarcodeView : BaseActivity<BarcodeViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.BarcodeView;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("BarcodeView OnCreate()");
        }

        private void ShowBarcode()
        {
            var imageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
            var bitmap = EncodeBarcode("panchos", imageView.Width, imageView.Height);
            imageView.SetImageBitmap(bitmap);
        }

        private Bitmap EncodeBarcode(String text, int width, int height)
        {
            QRCodeWriter writer = new QRCodeWriter();
            BitMatrix matrix = null;

            try
            {
                matrix = writer.encode(text, BarcodeFormat.QR_CODE, width, height);
            }
            catch (WriterException ex)
            {
                //
            }

            Bitmap bmp = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmp.SetPixel(x, y, GetBit(matrix, x, y) ? Color.Black : Color.White);
                }
            }

            return bmp;
        }

        private bool GetBit(BitMatrix matrix, int x, int y)
        {
            var row = matrix.getRow(y, null);
            return row[x];
        }
    }
}
