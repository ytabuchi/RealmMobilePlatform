## Tutorial：Xamarin.Forms アプリをスクラッチからビルドしよう

Swift でやっている Realm Mobile Platform のチュートリアル「[Realm Mobile Platform iOS App Tutorial](https://realm.io/docs/tutorials/realmtasks/)」を Xamarin.Forms でやってみましょう。

Realm Mobile Platform とはなんぞや？という方は、[公式ページ](https://realm.io/jp/products/realm-mobile-platform/) をご覧ください。

Realm Mobile Platform には Developer Edition があり、無料でオフラインファーストで自動同期の機能を備えたモバイルアプリを構築できます。有償の Professional Edition 以上では、無制限の Realm Functions（いわゆるサーバーレスの実行基盤、Azure Functions／AWS Lambda と同じような機能）が使えたりします。

Realm Mobile Platform のインストールは、公式ドキュメントの [Getting Started with the Realm Mobile Platform](https://realm.io/docs/get-started/) を参照してください。

macOS 用には、ライブラリを含む Realm Object Server（Realm Mobile Platform のサーバーアプリの呼称）が用意されていて、圧縮ファイルを回答しコマンドを実行するだけで動作します。非常に簡単に導入できるため、今回は macOS 版を使用してこのチュートリアルを進めたいと思います。

Linux がある方は Linux にインストールしてアクセスしても良いですし、docker で立てても良いかもしれないですね。

### 事前準備

事前準備として、Realm Object Server を起動し、データを登録します。

Realm Object Server の展開が終了したら、ルートディレクトリの `start-object-server.command` を実行します。

![Realm](Realm0-01.png)

ブラウザが起動し、ログイン画面が表示されます。

![Realm](Realm0-02.png)

任意のメールアドレスとパスワードで Admin User を作成します。この時のユーザー名とパスワードを後で使用しますので、メモしておきましょう。ログインすると Dashboard や存在する Realm Database、User などが閲覧できます。

![Realm](Realm0-03.png)

Realm Object Server の起動を確認したら、`＜ルートディレクトリ＞/demo/RealmTasks/RealmTasks.app` を起動します。

![Realm](Realm0-04.png)

先ほど作成した Admin User でログインします。ログイン後、右上の「＋」ボタンからタスクを登録します。

![Realm](Realm0-05.png)

ブラウザの Realm タブから、データが登録されていることが分かります。

![Realm](Realm0-06.png)

![Realm](Realm0-07.png)



### Xamarin.Forms プロジェクトを作成

最初に Xamarin.Forms のプロジェクトを作成します。

#### macOS の場合

プロジェクトを作成します。新規プロジェクトを作成して「Multiplatform＞App＞Black Forms App」です。

![Realm](Realm1-01.png)

今回は名前を「RealmMobilePlatformSample」にしました。また、`ポータブルクラスライブラリ（Portable Class Library）` ではなく、`共有ライブラリ（Shared Library）` を使用しますのでご注意ください。

![Realm](Realm1-02.png)

そのままプロジェクトを作成します。

![Realm](Realm1-03.png)

最初に表示するページとして `RealmMobilePlatformSamplePage.xaml` と `RealmMobilePlatformSamplePage.xaml.cs` が作成されているはずです。今回は macOS を使用して開発して行きますので文中にはこのファイル名が出てきますが、Windows の場合は `MainPage.xaml` と `MainPage.xaml.cs` が作成されますので適宜読み替えてください。

#### Windows

プロジェクトを作成します。「Visual C#＞Cross-Platform＞Cross Platform App (Xamarin)」です。
![Realm](Realm1-04.png)

次の画面で「空のアプリ」「Xamarin.Forms」「共有プロジェクト」を選択した状態で「OK」をクリックしてプロジェクトを作成してください。

![Realm](Realm1-05.png)

### 画面を作成

「2. Remove parts of the Xcode project to start with a simple base」の章です。チュートリアルでは簡略化のため Storyborad を削除していますが、Xamarin.Forms では XAML で画面を作成するため、そのまま画面を作成していきます。

`AppDelegate.swift` を以下のように書き換える部分、

```swift
import UIKit

@UIApplicationMain
class AppDelegate: UIResponder, UIApplicationDelegate {
    var window: UIWindow?
    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions:[UIApplicationLaunchOptionsKey : Any]? = nil) -> Bool {
        window = UIWindow(frame: UIScreen.main.bounds)
        window?.rootViewController = UINavigationController(rootViewController: ViewController(style: .plain))
        window?.makeKeyAndVisible()
        return true
    }
}
```

これは、`ViewController` を呼びだしているコードなので、Xamarin.Forms では `App.xaml.cs` の `MainPage` の指定に該当します。次のように `NavigationPage` で初期ページを呼び出します。

```csharp
MainPage = new NavigationPage(new RealmMobilePlatformSamplePage());
```


### NuGet パッケージをインストール

「3. Import Realm Swift and create models」の章は Framework のインストールなので、Xamarin.Forms では NuGet パッケージをインストールします。

「Realm」で検索して、「Realm 1.6.0」（2017/08/28 時点）をインストールします。

![Realm](Realm2-01.png)

![Realm](Realm2-02.png)

少し時間が掛かるのと、ライブラリの使用許諾のダイアログが表示されるのでのんびり待ちましょう。

ViewController に以下のコードを追加する部分、

```swift
import RealmSwift

// MARK: Model

final class TaskList: Object {
    dynamic var text = ""
    dynamic var id = ""
    let items = List<Task>()

    override static func primaryKey() -> String? {
        return "id"
    }
}

final class Task: Object {
    dynamic var text = ""
    dynamic var completed = false
}
```

これはモデルクラスの作成に該当します。Realm ではデータベーススキーマはモデルクラスで実装します。

Xamarin.Forms の場合も同様にモデルクラスを作成します。`RealmMobilePlatformSamplePage.xaml.cs` に作成しても良いですが、今回は新規に `TaskList.cs` ファイルを作成します。

```csharp
using System.Collections.Generic;
using Realms;
```

`using` を追加した後で、以下のクラスを作成します。`RealmObject` を継承するとモデルクラスになります。C# のアッパーキャメルに対応するよう、プロパティを作成し、サンプルの「realmtasks」の小文字のスキーマに `MapTo` 属性でマッピングします。

```csharp
public class TaskList : RealmObject
{
	[PrimaryKey]
	[Required]
	[MapTo("id")]
	public string Id { get; set; }

	[MapTo("text")]
	[Required]
    public string Title { get; set; } = string.Empty;

    [MapTo("items")]
    public IList<Task> Items { get; }
}

public class Task : RealmObject
{
    [MapTo("text")]
    [Required]
    public string Title { get; set; } = string.Empty;

    [MapTo("completed")]
    public bool Completed { get; set; } = false;
}
```

> Realm モデルの詳細は [Realm Xamarin ドキュメントの「モデル」の章](https://realm.io/jp/docs/xamarin/latest/#section-5) を参考にしてください。

### ListView の作成

「4. Add a title and register a cell class for use with our table view」「5. Use a Realm List to display Tasks in the table view」の章は Ream ではなく、単純に iOS の UITableView を表示し、データを追加する処理です。

```swift
// MARK: UITableView

override func tableView(_ tableView: UITableView?, numberOfRowsInSection section: Int) -> Int {
    return items.count
}

override func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    let cell = tableView.dequeueReusableCell(withIdentifier: "cell", for: indexPath)
    let item = items[indexPath.row]
    cell.textLabel?.text = item.text
    cell.textLabel?.alpha = item.completed ? 0.5 : 1
    return cell
}
```

この部分ですが、`cell.textLabel?.alpha = item.completed ? 0.5 : 1` に注意が必要です。`Completed` プロパティが true なら透明度が 0.5 になる。ということで、Xamarin.Forms で実現するには、IValueConverter が必要です。

プロジェクトにクラス `OpacityConverter.cs` を作成し、次のコードで置き換えます。

```csharp
using System;
using System.Globalization;
using Xamarin.Forms;

namespace RealmMobilePlatformSample
{
    public class OpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 1;

            return (bool)value ? 0.5 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
```

Xamarin.Forms の `ListView` を作成します。`RealmMobilePlatformSamplePage.xaml` の `ContentPage` にタイトルプロパティを追加し、以下の `ListView` を追加します。（Windows の場合はクラス名が `MainPage` なので注意してください。）

```xml
<?xml version="1.0" encoding="utf-8"?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:RealmMobilePlatformSample"
             x:Class="RealmMobilePlatformSample.RealmMobilePlatformSamplePage"
             Title="My Tasks">
    <ContentPage.Resources>
        <ResourceDictionary>
            <!-- Converter参照 -->
            <local:OpacityConverter x:Key="opacityConverter" />
        </ResourceDictionary>
    </ContentPage.Resources>
    <ListView x:Name="listView"
              ItemsSource="{Binding}">
        <ListView.ItemTemplate>
            <DataTemplate>
                <ViewCell>
                    <!-- OpecityプロパティにCompletedをバインドして、Converterでtrueなら1(不透明)、falseなら0.5(半透明)を返すようにします。-->
                    <Label Text="{Binding Title}"
                           Opacity="{Binding Completed, Converter={StaticResource opacityConverter}}"
                           VerticalOptions="Center"
                           Margin="15,0,0,0"/>
                </ViewCell>
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>
</ContentPage>
```

続いてコードビハインド `RealmMobilePlatformSamplePage.xaml.cs` のクラスに `IList` のフィールドを作成し、初期データを追加します。次のようなコードになります。

```csharp
public partial class RealmMobilePlatformSamplePage : ContentPage
{
    IList<Task> _items = new List<Task>();

    public RealmMobilePlatformSamplePage()
    {
        InitializeComponent();

        _items.Add(new Task{ Title = "My First Task" });
        this.BindingContext = _items;
    }
    ...
    ...
```

Swift のサンプルでは、`UITableView` をコードで定義していますが、Xamarin.Forms では XAML で作成した `ListView` の `ItemsSource` プロパティに `{Binding}` を指定し、バインド対象として `BindingContext = _items` を指定すると各列のセルに `Task` データが表示されます。各セルは標準で用意されている `TextCell` などを使用しても良いですし、`ViewCell` で独自にセルを設定しても構いません。今回は、`Opacity` をバインドする必要があったため、`ViewCell` の中に `Label` を使用しました。

### データを作成する機能を追加

「6. Add support for creating new tasks」の章です。Swift での次の部分、

```swift
func add() {
    let alertController = UIAlertController(title: "New Task", message: "Enter Task Name", preferredStyle: .alert)
    var alertTextField: UITextField!
    alertController.addTextField { textField in
        alertTextField = textField
        textField.placeholder = "Task Name"
    }
    alertController.addAction(UIAlertAction(title: "Add", style: .default) { _ in
        guard let text = alertTextField.text , !text.isEmpty else { return }

        self.items.append(Task(value: ["text": text]))
        self.tableView.reloadData()
    })
    present(alertController, animated: true, completion: nil)
}
```

を移植していきますが、`UIAlertController` に `alertController.addTextField` で入力欄を追加しています。Xamarin.Forms では入力欄付きのダイアログは存在しないため、[Dependency Services](https://developer.xamarin.com/guides/xamarin-forms/application-fundamentals/dependency-service/) を使用して、iOS／Android のネイティブのダイアログを呼びだします。


最初に Xamarin.Forms のプロジェクトにインターフェースを作成します。

`IDisplayTextAlert.cs` インターフェースを作成し、次のメソッドを追加します。

```csharp
public interface IDisplayTextAlert
{
    Task<string> Show(string title, string message);
}
```

次に、`RealmMobilePlatformSamplePage.xaml` のコードビハインドに次のメソッドを追加します。`DependencyService.Get<IDisplayTextAlert>().Show()` の部分がインターフェース経由で iOS／Android のコードを呼び出す箇所です。

```csharp
private async void AddAsync(object sender, EventArgs e)
{
    var text = await DependencyService.Get<IDisplayTextAlert>().Show("New Task", "Enter Task Name");
    if (!string.IsNullOrEmpty(text))
    {
        _items.Add(new Task{ Title = text });
        var items = new List<Task>(_items);
        this.BindingContext = items;
    }
}
```

Swift コードでは `UIAlertAction` のアクションにコレクションの追加のロジックを組み込んでいますが、ダイアログ表示は iOS／Android のネイティブコードで実装するため、一度共通コードにダイアログの入力欄の文字を返し、共通コード内でチェックを行ないます。

iOS プロジェクトに移動して、`DisplayTextAlert.cs` クラスを作成します。次のコードで置き換えます。

```csharp
using System;
using Xamarin.Forms;
using RealmMobilePlatformSample.iOS;
using UIKit;
using Foundation;
using System.Threading.Tasks;

[assembly: Dependency(typeof(DisplayTextAlert))]

namespace RealmMobilePlatformSample.iOS
{
    public class DisplayTextAlert : IDisplayTextAlert
    {

        public async Task<string> Show(string title, string message)
        {
            var text = await ShowDialog(title, message);
            return text;
        }

        private Task<string> ShowDialog(string title, string message)
        {
            var comp = new TaskCompletionSource<string>();

            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddTextField((field) =>
            {
                field.Placeholder = "Task name";
            });
            alert.AddAction(UIAlertAction.Create("Add", UIAlertActionStyle.Default, x =>
            {
                comp.SetResult(alert.TextFields[0].Text);
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            return comp.Task;
        }
    }
}
```

AddAsync ボタンが押された時に return を返したかったので、[@amay077](https://twitter.com/amay077) さんの [UIAlertController を async/await 対応させて便利に使う \- Qiita](http://qiita.com/amay077/items/0a3fa3dfac7f29a2807d) を参考にしました。

Android プロジェクトを開き、同様に `DisplayTextAlert.cs` クラスを追加します。
iOS と同様に以下のコードで置き換えます。

```csharp
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using RealmMobilePlatformSample.Droid;
using Android;
using Android.Widget;
using Android.App;

[assembly: Dependency(typeof(DisplayTextAlert))]

namespace RealmMobilePlatformSample.Droid
{
    public class DisplayTextAlert : IDisplayTextAlert
    {
        public Task<string> Show(string title, string message)
        {
            var text = ShowDialog(title, message);
            return text;
        }

        private Task<string> ShowDialog(string title, string message)
        {
            var comp = new TaskCompletionSource<string>();

            var textDialog = new AlertDialog.Builder(Xamarin.Forms.Forms.Context);
            var editText = new EditText(Xamarin.Forms.Forms.Context)
            {
                Hint = "Task name"
            };
            textDialog.SetTitle(title);
            textDialog.SetMessage(message);
            textDialog.SetView(editText);
            textDialog.SetNegativeButton("Add", (sender, e) =>
            {
                comp.SetResult(editText.Text);
            });
            textDialog.Show();

            return comp.Task;
        }
    }
}
```

Xamarin.Forms プロジェクトに戻り、`RealmMobilePlatformSamplePage.xaml` のコードビハインド内の最初に作成した `_items.Add(new Task{ Title = "My First Task" });` を削除します。

ドキュメントで以下の部分

```swift
func setupUI() {
    // ... existing function ...
    navigationItem.rightBarButtonItem = UIBarButtonItem(barButtonSystemItem: .add, target: self, action: #selector(add))
}
```

は `rightBarButtonItem` に標準で用意されている Add スタイル（＋が表示される）のボタンを追加しています。Xamarin.Forms の場合は、[Page](https://developer.xamarin.com/api/type/Xamarin.Forms.Page/) クラスの `ToolbarItems` プロパティを使用します。

`RealmMobilePlatformSamplePage.xaml` の `ContentPage.Resources` の後に以下を追加します。

```xml
<ContentPage.ToolbarItems>
    <ToolbarItem Text="＋" Clicked="AddAsync"/>
</ContentPage.ToolbarItems>
```

`ToolbarItem` のプロパティは[公式ドキュメント](https://developer.xamarin.com/api/type/Xamarin.Forms.ToolbarItem/)を参照してください。

ここで一度ビルドして、AddAsync ボタンとダイアログが正常に動作するか確認しましょう。


### Realm への追加と同期処理の実装

「7. Back items by a Realm and integrate sync」の章です。

using と `_items` フィールドの後に以下を追加します。

```csharp
using Realms;
using Realms.Sync;
```


```csharp
Realm _realm;
IDisposable _notificationToken;
```

`setupUI()` の後、Xamarin.Forms では `InitializeComponent()` の後に `SetupRealmAsync();` を追加しています。ドキュメントでは iOS のみなのでサーバー IP は `127.0.0.1` で良いのですが、Android Emulator からアクセスする際は `10.0.2.2` になります。（実機や別のマシンに立てた Realm Object Server にアクセスする場合はその Realm Object Server の IP アドレスとポートを指定します。）

`Add` メソッドの下に `SetupRealmAsync` メソッドを追加します。サンプルのソースコードでは、`.gitignore` に指定して公開されないようにした `Secrets.cs` に情報を記載しています。

```csharp
private async void SetupRealmAsync()
{
    var username = "xxxxx";
    var password = "xxxxx";
    var serverIp = (Device.RuntimePlatform == Device.Android) ? "10.0.2.2:9080" : "127.0.0.1:9080";
}
```

コンストラクターの `InitializeComponent` の後で `SetupRealmAsync` メソッドの呼びだしを追加します。

```csharp
public RealmMobilePlatformSamplePage()
{
    InitializeComponent();

    SetupRealmAsync();

    this.BindingContext = _items;
}
```


続いて以下の部分、

```swift
func setupRealm() {
    // ... existing function ...
    SyncUser.logIn(with: .usernamePassword(username: username, password: password, register: false), server: URL(string: "http://127.0.0.1:9080")!) { user, error in
        guard let user = user else {
            fatalError(String(describing: error))
        }

        DispatchQueue.main.async {
            // Open Realm
            let configuration = Realm.Configuration(
                syncConfiguration: SyncConfiguration(user: user, realmURL: URL(string: "realm://127.0.0.1:9080/~/realmtasks")!)
            )
            self.realm = try! Realm(configuration: configuration)

            // Show initial tasks
            func updateList() {
                if self.items.realm == nil, let list = self.realm.objects(TaskList.self).first {
                    self.items = list.items
                }
                self.tableView.reloadData()
            }
            updateList()

            // Notify us when Realm changes
            self.notificationToken = self.realm.addNotificationBlock { _ in
                updateList()
            }
        }
    }
}
```

を移植します。`SetupRealmAsync` メソッドを作成し、`SyncUser.logIn`、`Realm.Configuration`、`Realm(configuration: configuration)` の部分は次のコードを追加します。

```csharp
private async void SetupRealmAsync()
{
    ...

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
        _realm = Realm.GetInstance(config);
    }
    catch (Exception)
    {
        return;
    }

    ...
}
```

最初に `Realms.Sync.User` クラスの `Current` メソッドで既にログインしている場合にユーザーを取得する処理がありますが、これは Swift のサンプルにはありません。（[Realtime Databases with the Realm Mobile Platform | Xamarin Blog](https://blog.xamarin.com/shared-drawing-with-the-realm-mobile-platform/) のコードを参考（パクリ）にしています。）

Swift での `SyncUser.logIn(with: .usernamePassword ...` の部分、Xamarin.Forms 用の SDK では、`Realm.Sync.Credentials` クラスの `UsernamePassword` メソッドを使用してクレデンシャルを作成し、`Realm.Sync.User` クラスの `LoginAsync (Credentials credentials, Uri serverUrl)` メソッドの引数で渡してあげます。Swift の `try! Realm(configuration: configuration)` の部分が、`Realm.GetInstance(config)` メソッドです。


その後、Swift では初期タスクとして `func updateList()` 内で　Realm Object Server からデータを取得して取得したデータで UITableView をリロードしていますので、C# でも `UpdateList` メソッドを用意します。

```csharp
private void UpdateList()
{
    if (_realm.All<TaskList>().FirstOrDefault() != null)
        _items = _realm.All<TaskList>().FirstOrDefault().Items;
    this.BindingContext = _items;
}
```


`user` が取得できていれば、`UpdateList` メソッドを呼び出します。その後、Realm Object Server 側で変更があった際に発行される `notificationToken` を受け取った際の処理 `SubscribeForNotifications` 内でも `UpdateList` メソッドを呼び出します。`SetupRealmAsync` メソッドの最後に以下のコードを追加します。

```csharp
if (user != null)
{
    UpdateList();
    _notificationToken = _items.SubscribeForNotifications((sender, changes, error) => UpdateList());
}
```

`Add` メソッドを Realm に追加するように置き換えている部分、

```Swift
try! items.realm?.write {
    items.insert(Task(value: ["text": text]), at: items.filter("completed = false").count)
}
```

増えているのは上記なので、以下のように置き換えます。

```csharp
if (!string.IsNullOrEmpty(text))
{
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
```

`Add` だと Realm 側では最後にデータが入りますが、`ListView` の表示が古い方が下になっているため、見えているデータの最初に表示するために `Insert` を使用します。`Insert` の `Index int` には Swift では `filter` 関数を使っていますが、C# であれば LINQ の出番ですね。`Completed` を `Where` しましょう。

現時点でのコードビハインドは以下のコードになっているはずです。

```csharp
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Realms;
using Realms.Sync;

namespace RealmMobilePlatformSample
{
    public partial class RealmMobilePlatformSamplePage : ContentPage
    {
        IList<Task> _items = new List<Task>();
        Realm _realm;
        IDisposable _notificationToken;

        public RealmMobilePlatformSamplePage()
        {
            InitializeComponent();

            SetupRealmAsync();

            this.BindingContext = _items;
        }

        private async void AddAsync(object sender, EventArgs e)
        {
            var text = await DependencyService.Get<IDisplayTextAlert>().Show("New Task", "Enter Task Name");

            if (!string.IsNullOrEmpty(text))
            {
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

        private async void SetupRealmAsync()
        {
            var username = "xxxxx";
            var password = "xxxxx";
            var serverIp = "xxxxx";

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
                _realm = Realm.GetInstance(config);
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
```


最後に TLS ではないネットワークアクセスをするので、`Info.plist` にその設定をします。

`Info.plist` を右クリックして、任意のテキストエディタで開き、最後の `</dict>` の前に次を追加します。

```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSAllowsArbitraryLoads</key>
    <true/>
</dict>
```

おめでとうございます！これで Realm と同期する最初のアプリが完成しました！

![動作画面](./Realm.gif)

### タスクの移動と削除をサポート

まずは動くものができましたが、引き続き実装を追加して行きます。iOS の UITableView でのエディットモードを使用しています。削除処理を追加してみましょう。「8. Support moving and deleting tasks」の部分です。

Swift で以下の部分、

```swift
func setupUI() {
    // ... existing function ...
    navigationItem.leftBarButtonItem = editButtonItem
}
```

と

```swift
override func tableView(_ tableView: UITableView, moveRowAt sourceIndexPath: IndexPath, to destinationIndexPath: IndexPath) {
    try! items.realm?.write {
        items.move(from: sourceIndexPath.row, to: destinationIndexPath.row)
    }
}

override func tableView(_ tableView: UITableView, commit editingStyle: UITableViewCellEditingStyle, forRowAt indexPath: IndexPath) {
    if editingStyle == .delete {
          try! realm.write {
              let item = items[indexPath.row]
              realm.delete(item)
          }
    }
}
```

です。残念ながら Xamarin.Forms では、ListView のエディットモードがサポートされていないため、Custom Renderer を使うことになりますが、ちょっと大変そうなので、今回このパートはパスします。時間ができたら ListView Renderer を作成したいと思います。

代わりに、Xamarin.Forms の ListView に標準で付属している `Context Actions` を使ってスワイプ（iOS）と長押し（Android）でアイテムの削除を実装したいと思います。

Context Actions は、XAML では Cell 内に定義します。

```xml
<ViewCell.ContextActions>
    <MenuItem Clicked="OnDelete" Text="Delete" CommandParameter="{Binding}" IsDestructive="True" />
</ViewCell.ContextActions>
```

`CommandParameter` に Binding することで、Delete した要素（ここでは Task 型）を取得できます。

コードビハインド `RealmMobilePlatformSamplePage.xaml.cs` を開き、`AddAsync` メソッドの後に以下のメソッドを追加します。

```csharp
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
```

まず MenuItem　を取得し、その `CommandParameter` を取得して Task にキャストしています。後は、`Realm.Write` のアクションで `Remove` メソッドに `Task` を渡してあげています。

### タップすることでタスクの完了にトグルする処理をサポート

アイテムをタップすると、完了（して一番最後に移動）、未完了をトグルする処理を追加します。「9. Support toggling the ‘completed’ state of a task by tapping it」の部分です。

Swift で次の部分、

```swift
override func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    let item = items[indexPath.row]
    try! item.realm?.write {
        item.completed = !item.completed
        let destinationIndexPath: IndexPath
        if item.completed {
            // move cell to bottom
            destinationIndexPath = IndexPath(row: items.count - 1, section: 0)
        } else {
            // move cell just above the first completed item
            let completedCount = items.filter("completed = true").count
            destinationIndexPath = IndexPath(row: items.count - completedCount - 1, section: 0)
        }
        items.move(from: indexPath.row, to: destinationIndexPath.row)
    }
}
```

で `item.completed` の状態により、完了、非完了を書き換え、一番下に配置したり、完了の一つ上に配置したりしています。Xamarin.Forms でも実装して行きましょう。

最初に ListView に `ItemSelected` イベントを追加します。

```xml
<ListView x:Name="listView"
          ItemsSource="{Binding}"
          ItemSelected="OnSelect">
    <ListView.ItemTemplate>
    ...
```

コードビハインドで `OnDelete` メソッドの後に `OnSelect` メソッドを追加します。

```csharp
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
                _items.Move(item, _items.Count() -1);
            }
            else
            {
                //de-completeにしてcompletedの一個上に
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
```

動かしてみましょう！

![Realm](RealmFinish.gif)


お疲れ様でした！これで全てのチュートリアルは終了です。
