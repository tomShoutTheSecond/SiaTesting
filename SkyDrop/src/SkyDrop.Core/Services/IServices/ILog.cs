using System;
namespace SkyDrop.Core.Services
{
    public interface ILog
    {
        public void Exception(Exception exception);

        void Error(string errorMessage, System.Exception ex);

        void Error(string errorMessage);
    }
}
