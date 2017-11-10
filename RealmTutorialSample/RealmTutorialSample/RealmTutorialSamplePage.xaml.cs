using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Realms;
using Realms.Sync;

namespace RealmTutorialSample
{
    public partial class RealmTutorialSamplePage : ContentPage
    {
        IList<Task> _items = new List<Task>();
        Realm _realm;
        IDisposable _notificationToken;

        public RealmTutorialSamplePage()
        {
            InitializeComponent();

            SetupRealmAsync();


        }

        private async void AddAsync(object sender, EventArgs e)
        {
            var text = await DependencyService.Get<IDisplayTextAlert>().Show("New Task", "Enter Task Name");

            if (!string.IsNullOrEmpty("text")) //text
            {
                // この部分を書き換えています。
                try
                {
                    _realm.Write(() =>
                    {
                        _items.Insert(_realm.All<TaskList>().FirstOrDefault().Items.Where(x => x.Completed).Count(), new Task
                        {
                            Title = text
                        });
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        private void OnDelete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var item = mi.CommandParameter as Task;

            try
            {
                _realm.Write(() =>
                {
                    _items.Remove(item);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void OnSelect(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            var item = e.SelectedItem as Task;

            try
            {
                _realm.Write(() =>
                {
                    if (!item.Completed)
                    {
                        //completedにして一番下に
                        item.Completed = true;
                        _items.Move(item, _items.Count() - 1);
                    }
                    else
                    {
                        //de-completeにしてde-completedの一番下に
                        item.Completed = false;
                        _items.Move(item, _items.Count() - _items.Count(x => x.Completed == true) - 1);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private async void SetupRealmAsync()
        {
            var username = "ytabuchi"; // use default.
            var password = "realm"; // use default.
            var serverIp = "127.0.0.1:9080"; // use default.

            User user = null;
            try
            {
                user = User.Current;  // if still logged in from last session
            }
            catch (Exception) { }

            try
            {
                if (user == null)
                {
                    var credentials = Credentials.UsernamePassword(username, password, createUser: false);
                    user = await User.LoginAsync(credentials, new Uri($"http://{serverIp}"));
                }
                var config = new SyncConfiguration(user, new Uri($"realm://{serverIp}/~/realmtasks"));
                _realm = await Realm.GetInstanceAsync(config);
            }
            catch (Exception)
            {
                return;
            }

            if (user != null)
            {
                UpdateList();

                _notificationToken = _items.SubscribeForNotifications((sender, changes, error) => UpdateList());
            }
        }

        private void UpdateList()
        {
            if (_realm.All<TaskList>().FirstOrDefault() != null)
                _items = _realm.All<TaskList>().FirstOrDefault().Items;

            this.BindingContext = _items;
        }
    }
}
