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
    public class MainView : BaseActivity<MainViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.MainView;

        private const int pickFileRequestCode = 100;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            Log.Trace("MainView OnCreate()");

            ViewModel.SelectFileAsyncFunc = SelectFile;
            ViewModel.SelectImageAsyncFunc = SelectImage;
            ViewModel.FileTapCommand = new MvxCommand<SkyFile>(OpenFile);

            var progressBar = FindViewById<ProgressBar>(Resource.Id.ProgressBar);
            if(progressBar != null)
                progressBar.IndeterminateDrawable.SetColorFilter(Color.White, PorterDuff.Mode.SrcIn);
        }

        private async Task SelectImage()
        {
            if (!await CheckPermissions())
                return;

            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("image/*");
            StartActivityForResult(intent, pickFileRequestCode);
        }

        private async Task SelectFile()
        {
            try
            {
                if (!await CheckPermissions())
                    return;

                var intent = new Intent(Intent.ActionGetContent);
                intent.SetType("file/*");
                intent.AddCategory(Intent.CategoryOpenable);

                // special intent for Samsung file manager
                Intent sIntent = new Intent("com.sec.android.app.myfiles.PICK_DATA");
                sIntent.AddCategory(Intent.CategoryDefault);

                Intent chooserIntent;
                if (PackageManager.ResolveActivity(sIntent, 0) != null)
                {
                    // it is device with Samsung file manager
                    chooserIntent = Intent.CreateChooser(sIntent, "Open file");
                    chooserIntent.PutExtra(Intent.ExtraInitialIntents, new Intent[] { intent });
                }
                else
                {
                    chooserIntent = Intent.CreateChooser(intent, "Open file");
                }

                try
                {
                    StartActivityForResult(chooserIntent, pickFileRequestCode);
                }
                catch (Android.Content.ActivityNotFoundException ex)
                {
                    Log.Exception(ex);
                    Toast.MakeText(this, "No suitable File Manager was found", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private async Task<bool> CheckPermissions()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                Log.Error("Permission not granted");
                Log.Exception(ex);

                return false;
            }
        }

        protected override async void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            try
            {
                if (requestCode == pickFileRequestCode)
                {
                    if (data == null)
                        return;

                    await HandlePickedFile(data);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async Task HandlePickedFile(Intent data)
        {
            //handle the selected file
            var uri = data.Data;
            string mimeType = ContentResolver.GetType(uri);
            Log.Trace("mime type: " + mimeType);

            string extension = System.IO.Path.GetExtension(uri.Path);
            Log.Trace("extension: " + extension);

            Log.Trace("path: " + uri.Path);

            var filename = AndroidUtil.GetFileName(this, uri);

            Toast.MakeText(this, uri.Path, ToastLength.Long).Show();

            var fileBytes = await ReadFile(uri);
            ViewModel.StageFile(new StagedFile { Filename = filename, Data = fileBytes });
        }

        public async Task<byte[]> ReadFile(Android.Net.Uri uri)
        {
            var inputStream = ContentResolver.OpenInputStream(uri);

            byte[] bytes = new byte[inputStream.Length];
            try
            {
                var buffer = new Java.IO.BufferedInputStream(inputStream);
                await buffer.ReadAsync(bytes);
                buffer.Close();
            }
            catch (Exception e)
            {
                Log.Trace("Failed to read file");
                Log.Exception(e);
            }

            return bytes;
        }

        private void OpenFile(SkyFile file)
        {
            Toast.MakeText(this, $"Opening file {file.Filename}", ToastLength.Long).Show();

            var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(file.Skylink));
            StartActivity(browserIntent);
        }
    }
}
