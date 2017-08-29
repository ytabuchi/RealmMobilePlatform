using System;
using System.Threading.Tasks;

namespace RealmMobilePlatformSample
{
    public interface IDisplayTextAlert
    {
        Task<string> Show(string title, string message);
    }
}
