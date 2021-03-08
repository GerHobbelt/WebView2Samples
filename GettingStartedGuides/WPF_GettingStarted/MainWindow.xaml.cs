using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;


namespace WPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            webViewA.NavigationStarting += EnsureHttps;
            webViewB.NavigationStarting += EnsureHttps;
            InitializeAsync();

        }

        async void InitializeAsync()
        {
            await webViewA.EnsureCoreWebView2Async(null);
            await webViewB.EnsureCoreWebView2Async(null);
            webViewA.CoreWebView2.WebMessageReceived += UpdateAddressBar;
            webViewB.CoreWebView2.WebMessageReceived += UpdateAddressBar;

            await webViewA.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
            await webViewA.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");

            await webViewB.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
            await webViewB.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");

            A_or_B = false;
            ButtonGo.Content = (A_or_B ? "Go [B]" : "Go [A]");
        }

        private bool A_or_B = false;

        void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            Microsoft.Web.WebView2.Core.CoreWebView2 webView = sender as Microsoft.Web.WebView2.Core.CoreWebView2;
            String uri = args.TryGetWebMessageAsString();
            addressBar.Text = uri;
            //Microsoft.Web.WebView2.Wpf.WebView2 webView = (A_or_B ? webViewB : webViewA);
            webView.PostWebMessageAsString(uri);
            //A_or_B = !A_or_B;
            //ButtonGo.Content = (A_or_B ? "Go [B]" : "Go [A]");
        }

        void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            Microsoft.Web.WebView2.Wpf.WebView2 webView = sender as Microsoft.Web.WebView2.Wpf.WebView2;
            String uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Web.WebView2.Wpf.WebView2 webView = (A_or_B ? webViewB : webViewA);
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(addressBar.Text);
                A_or_B = !A_or_B;
                ButtonGo.Content = (A_or_B ? "Go [B]" : "Go [A]");
            }
        }
    }
}

