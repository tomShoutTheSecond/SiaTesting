using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.IoC;
using SkyDrop.Core.ViewModels.Main;
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

            ViewModel.SelectFileCommand = new MvxAsyncCommand(SelectFileCommand);

            await ViewModel.InitializeTask.Task;

            Platform.Init(this, bundle);
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

            //handle the selected file
            var uri = data.Data;
            string mimeType = ContentResolver.GetType(uri);
            Console.WriteLine("mime type: ", mimeType);

            string extension = System.IO.Path.GetExtension(uri.Path);
            Console.WriteLine("extension: ", extension);

            Console.WriteLine("path: ", uri.Path);

            var filename = GetFileName(uri);

            Toast.MakeText(this, uri.Path, ToastLength.Long).Show();

            var fileBytes = UploadFile(uri);
            await ViewModel.UploadFile(filename, fileBytes);
        }

        private string GetFileName(Android.Net.Uri uri)
        {

            // The query, because it only applies to a single document, returns only
            // one row. There's no need to filter, sort, or select fields,
            // because we want all fields for one document.
            Android.Database.ICursor cursor = ContentResolver.Query(uri, null, null, null, null, null);
            var displayName = "";
            try
            {
                // moveToFirst() returns false if the cursor has 0 rows. Very handy for
                // "if there's anything to look at, look at it" conditionals.
                if (cursor != null && cursor.MoveToFirst())
                {

                    // Note it's called "Display Name". This is
                    // provider-specific, and might not necessarily be the file name.
                    displayName = cursor.GetString(
                            cursor.GetColumnIndex(OpenableColumns.DisplayName));
                    Console.WriteLine("Display Name: " + displayName);


                    int sizeIndex = cursor.GetColumnIndex(OpenableColumns.Size);
                    // If the size is unknown, the value stored is null. But because an
                    // int can't be null, the behavior is implementation-specific,
                    // and unpredictable. So as
                    // a rule, check if it's null before assigning to an int. This will
                    // happen often: The storage API allows for remote files, whose
                    // size might not be locally known.
                    String size = null;
                    if (!cursor.IsNull(sizeIndex))
                    {
                        // Technically the column stores an int, but cursor.getString()
                        // will do the conversion automatically.
                        size = cursor.GetString(sizeIndex);
                    }
                    else
                    {
                        size = "Unknown";
                    }
                    Console.WriteLine("Size: " + size);
                }
            }
            catch(Exception e)
            {
                return "error.jpg";
            }
            finally
            {
                cursor.Close();

            }

            return displayName;

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
    }
}
