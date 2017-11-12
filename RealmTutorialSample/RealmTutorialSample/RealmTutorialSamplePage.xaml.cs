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
            // Dependency Servicesを使用してiOS／Androidネイティブのダイアログを表示するコードを呼び出します。
            var text = await DependencyService.Get<IDisplayTextAlert>().Show("New Task", "Enter Task Name");

            if (!string.IsNullOrEmpty(text))
            {
                // この部分を書き換えています。
                try
                {
                    _realm.Write(() =>
                    {
                        // Realmから最初のTaskListのItemsのうち、Completedの数の位置にTaskを追加します。
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
            // ListViewCellのContextActionsが実行されると、sender引数で渡されるMenuItemのCommandParameterに
            // Taskがバインドされて来るため、そのまま_item.Remove()に渡します。
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

            // ListViewのItemがタップされると引数SelectedItemChangedEventArgsにオブジェクトが入っています。
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
            var username = ""; // 作成したユーザーを指定します。
            var password = ""; // パスワード
            var serverIp = "127.0.0.1:9080";

            User user = null;
            try
            {
                user = User.Current;  // if still logged in from last session
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            try
            {
                if (user == null)
                {
                    // クレデンシャルを作成してユーザーを取得します。
                    var credentials = Credentials.UsernamePassword(username, password, createUser: false);
                    user = await User.LoginAsync(credentials, new Uri($"http://{serverIp}"));
                }
                // 同期対象のRealm(ここではログインユーザーが所有している（~でアクセス可能）realmtasks)を指定します。
                var config = new SyncConfiguration(user, new Uri($"realm://{serverIp}/~/realmtasks"));
                // 接続
                _realm = await Realm.GetInstanceAsync(config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return;
            }

            if (user != null)
            {
                // 最初に一度同期しておきます。
                UpdateList();
                // その後はRealm Object Serverのアップデートを通知で受け取り、受け取るたびに同期メソッドを実行します。
                _notificationToken = _items.SubscribeForNotifications((sender, changes, error) => UpdateList());
            }
        }

        private void UpdateList()
        {
            // RealmがあればローカルのコレクションをRealmコレクションで置き換えて、再バインドします。
            if (_realm.All<TaskList>().FirstOrDefault() != null)
                _items = _realm.All<TaskList>().FirstOrDefault().Items;

            this.BindingContext = _items;
        }
    }
}
