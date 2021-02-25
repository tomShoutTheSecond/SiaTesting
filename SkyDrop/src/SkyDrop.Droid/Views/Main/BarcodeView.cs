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

            ViewModel.GenerateBarcodeAsyncFunc = ShowBarcode;
        }

        private async Task ShowBarcode()
        {
            var imageView = FindViewById<ImageView>(Resource.Id.BarcodeImage);
            var bitmap = await EncodeBarcode("panchos", imageView.Width, imageView.Height);
            imageView.SetImageBitmap(bitmap);
        }

        private Task<Bitmap> EncodeBarcode(string text, int width, int height)
        {
            return Task.Run(() =>
            {
                try
                {
                    var matrix = ViewModel.GenerateBarcode(text, width, height);
                    var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            bitmap.SetPixel(x, y, GetBit(matrix, x, y) ? Color.Black : Color.White);
                        }
                    }

                    return bitmap;
                }
                catch (WriterException ex)
                {
                    ViewModel.Log.Exception(ex);
                    return null;
                }
            });
        }

        private bool GetBit(BitMatrix matrix, int x, int y)
        {
            var row = matrix.getRow(y, null);
            return row[x];
        }
    }
}
