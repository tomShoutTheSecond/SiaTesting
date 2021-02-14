using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using Xamarin.Essentials;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class MainView : BaseActivity<MainViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.MainView;

        private const int pickFileRequestCode = 100;

        protected override async void OnCreate(Bundle bundle)
        {
            System.Diagnostics.Debug.WriteLine("MainView OnCreate()");

            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            ViewModel.SelectFileAsyncFunc = SelectFileCommand;
            ViewModel.FileTapCommand = new MvxCommand<SkyFile>(OpenFile);
        }

        private async Task SelectFileCommand()
        {
            if (!await CheckPermissions())
                return;

            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("image/png, image/jpeg");
            StartActivityForResult(intent, pickFileRequestCode);
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
                Console.WriteLine("" + ex);

                return false;
            }
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(requestCode == pickFileRequestCode)
            {
                if (data == null)
                    return;

                //handle the selected file
                var uri = data.Data;
                string mimeType = ContentResolver.GetType(uri);
                Console.WriteLine("mime type: ", mimeType);

                string extension = System.IO.Path.GetExtension(uri.Path);
                Console.WriteLine("extension: ", extension);

                Console.WriteLine("path: ", uri.Path);

                var filename = AndroidUtil.GetFileName(this, uri);

                Toast.MakeText(this, uri.Path, ToastLength.Long).Show();

                var fileBytes = UploadFile(uri);
                await ViewModel.UploadFile(filename, fileBytes);
            }
        }

        public byte[] UploadFile(Android.Net.Uri uri)
        {
            var inputStream = ContentResolver.OpenInputStream(uri);

            byte[] bytes = new byte[inputStream.Length];
            try
            {
                var buffer = new Java.IO.BufferedInputStream(inputStream);
                buffer.Read(bytes, 0, bytes.Length);
                buffer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("File upload failed: " + e);
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
