using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SkyDrop.Core.ViewModels.Main;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Commands;
using System.IO;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using Android.Graphics;
using Android.Provider;

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

            Xamarin.Essentials.Platform.Init(this, bundle);

            //ViewModel.SelectFileCommand = new MvxCommand(SelectFileCommand);

            ViewModel.SelectTheFileNative = async () => await SelectFileCommand();
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

                if (status != Xamarin.Essentials.PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                return status == Xamarin.Essentials.PermissionStatus.Granted;
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

            var uri = data.Data;

            Toast.MakeText(this, uri.Path, ToastLength.Long).Show();

            var fileBytes = UploadFile(uri);
            await ViewModel.UploadFile("cool file.jpg", fileBytes);
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

            }

            return bytes;
        }
        /*
        public byte[] UploadFile(Android.Net.Uri uri)
        {
            string path = GetRealPathFromUri(this, uri);//Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, uri.Path);
            var inputStream = new FileStream(path, FileMode.Open);

            byte[] bytes = new byte[inputStream.Length];
            try
            {
                var buffer = new Java.IO.BufferedInputStream(inputStream);
                buffer.Read(bytes, 0, bytes.Length);
                buffer.Close();
            }
            catch (Exception e)
            {

            }

            return bytes;
        }
        */
        private string GetRealPathFromUri(Context context, Android.Net.Uri contentUri)
        {
            Android.Database.ICursor cursor = null;
            try
            {
                string[] proj = { MediaStore.Images.Media.ContentType };
                cursor = context.ContentResolver.Query(contentUri, proj, null, null, null);
                int column_index = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.ContentType);
                cursor.MoveToFirst();
                return cursor.GetString(column_index);
            }
            finally
            {
                if (cursor != null)
                {
                    cursor.Close();
                }
            }
        }
    }
}
