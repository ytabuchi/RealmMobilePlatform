using System;
using System.Threading.Tasks;

namespace RealmTutorialSample
{
    public interface IDisplayTextAlert
    {
        Task<string> Show(string title, string message);
    }
}
