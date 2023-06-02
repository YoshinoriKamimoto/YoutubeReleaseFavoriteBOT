using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        // Googleのプロフィールの保存先パスを入力から取得
        Console.Write("プロフィールの保存先ディレクトリを入力 => ");
        string profilePath = Console.ReadLine();

        // プロフィール名を入力から取得
        Console.Write("プロフィール名を入力 => ");
        string profileName = Console.ReadLine();

        // 高評価取り消し動画数を入力から取得
        Console.Write("取り消し動画数を入力 => ");
        string releaseCntStr = Console.ReadLine();


        Console.Write("プログラムを開始します...");
        Thread.Sleep(1000);


        // 高評価取り消し動画数の入力値チェック
        int releaseCnt = 0;
        if (int.TryParse(releaseCntStr, out releaseCnt) == false)
        {
            Console.WriteLine("WARNING : 取り消し本数の入力形式不正");
            Console.WriteLine("WARNING : 取り消し本数は数字で入力してください。");
            ExitPrint();
            return;
        }

        

        // アカウントを指定して起動する場合、すでにchromeが起動中だとエラーになるため、chromeを起動していないかチェック
        Process[] chromeProcess = Process.GetProcessesByName("chrome");
        if (chromeProcess.Length > 0)
        {
            // 起動中のchromeがある場合、処理終了
            Console.WriteLine("WARNING : chromeプロセスが実行中です。すべてのchromeを終了してから再実行してください。");
            ExitPrint();
            return;
        }



        // chrome操作
        ChromeDriver driver = null;
        try
        {
            // chrome操作インスタンスのオプションを作成
            ChromeOptions options = new ChromeOptions();

            // --user-data-dirにプロフィールの保存先パスを設定
            options.AddArgument($"--user-data-dir={profilePath}");

            // --profile-directoryにプロフィール名を設定
            options.AddArgument($"--profile-directory={profileName}");


            // chrome操作インスタンス生成
            driver = new ChromeDriver(options);

            // 暗黙的な待機を設定
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);



            // yotubeトップページにアクセス
            driver.Navigate().GoToUrl("https://www.youtube.com/");

            // youtube高評価トップページにアクセス
            driver.Navigate().GoToUrl("https://www.youtube.com/playlist?list=LL");


            // 指定された回数、高評価取り消しメソッドを実行
            for (int i = 0; i < releaseCnt; i++)
            {
                if (ReleaseFavorite(driver) == false)
                {
                    break;
                }
            }

            // youtube高評価トップページにアクセス
            driver.Navigate().GoToUrl("https://www.youtube.com/playlist?list=LL");


            // 終了
            ExitPrint();
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR : ブラウザ操作エラー\n{ex}");
            Debug.WriteLine($"ERROR : ブラウザ操作エラー\n{ex}");


            ExitPrint();
            return;
        }
        finally
        {
            // リソース解放
            if (driver != null)
            {
                driver.Quit();
            }
        }
    }



    // 高評価取り消しメソッド
    // 高評価取り消しに失敗した場合、falseを返却
    public static bool ReleaseFavorite(ChromeDriver driver)
    {
        try
        {
            // 画面再読み込み
            driver.Navigate().Refresh();

            // 高評価動画へアクセス
            driver.FindElement(By.Id("video-title")).Click();


            // 高評価ボタン要素を取得
            IWebElement targetElement = GetTargetElement(driver, 4);
            if (targetElement == null)
            {
                Console.WriteLine("WARNING : 高評価ボタン要素が見つかりませんでした。");
                return false;
            }
            Debug.WriteLine($"ターゲット要素クラス名 : {targetElement.GetAttribute("class")}");


            // 高評価を取り消し(=高評価ボタンをクリック)
            targetElement.Click();

            // 対象のURLを取得して表示
            string path = driver.Url.Replace("&list=LL&index=1", "");
            Debug.WriteLine($"INFO : 高評価を取り消しました。 > {path}");
            Console.WriteLine($"INFO : 高評価を取り消しました。 > {path}");

            // ひとつ前のページへ戻る
            driver.Navigate().Back();

            return true;
        }
        catch
        {
            throw;
        }
    }



    // 高評価ボタン要素取得メソッド
    // n:再帰呼び出し回数
    public static IWebElement GetTargetElement(ChromeDriver driver, int n)
    {
        try
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Debug.WriteLine($"INFO : {methodName}メソッド呼び出し");
            Console.WriteLine("INFO : 高評価ボタン要素取得中...");

            // 要素表示待機
            Thread.Sleep(500);

            // 高評価ボタン要素取得
            IEnumerable<IWebElement> elements = driver.FindElements(By.XPath("//button"));
            IWebElement targetElement = null;
            foreach (IWebElement element in elements)
            {
                // aria-label属性の属性値で高評価ボタンかチェック
                string value = element.GetAttribute("aria-label");
                if (value == null)
                {
                    // 高評価ボタン以外のボタン要素であればスキップ
                    continue;
                }

                if ((value.Contains("高評価") && value.Contains("件")) || value.Contains("高く評価"))
                {
                    // 高評価ボタンに該当
                    targetElement = element;
                    break;
                }
            }

            // 要素を取得できたなかった場合、再帰的にメソッドを呼び出す
            if (targetElement == null)
            {
                // 再帰呼び出し回数をチェック
                if (n > 1)
                {
                    // 再帰呼び出し
                    targetElement = GetTargetElement(driver, n - 1);
                }

                return targetElement;
            }
            else
            {
                return targetElement;
            }
        }
        catch
        {
            throw;
        }
        
    }



    // 終了前処理
    public static void ExitPrint()
    {
        Thread.Sleep(5000);
        Console.Write("INFO : プログラムを終了します...");
        Thread.Sleep(2000);
    }
}