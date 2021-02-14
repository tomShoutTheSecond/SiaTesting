using System.Collections.Generic;
using System.Linq;
using Realms;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services
{
    public class StorageService : IStorageService
    {
        public List<SkyFile> LoadSkyFiles()
        {
            var realm = Realm.GetInstance();

            return realm.All<SkyFile>().ToList();
        }

        public void SaveSkyFiles(params SkyFile[] skyFiles)
        {
            var realm = Realm.GetInstance();

            realm.Write(() =>
            {
                realm.Add(skyFiles);
            });
        }
    }

    public interface IStorageService
    {
        List<SkyFile> LoadSkyFiles();

        void SaveSkyFiles(params SkyFile[] skyFile);
    }
}
