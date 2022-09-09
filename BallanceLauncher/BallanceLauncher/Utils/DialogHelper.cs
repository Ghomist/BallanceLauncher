using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BallanceLauncher.Utils
{
    public class DialogHelper
    {
        private static readonly SemaphoreSlim s_semaphore = new(1, 1);

        public static async Task<ContentDialogResult> ShowConfirmAsync(XamlRoot root, string title, string content,
            bool secondary = false, bool close = false)
        {
            string secondaryText = secondary ? "不要" : null;
            string closeText = close ? "我再想想" : null;
            return await ShowDialogAsync(root, title, content, "好的", secondaryText, closeText, ContentDialogButton.Primary);
        }

        public static Task ShowErrorMessageAsync(XamlRoot root, string content, bool canCopy = false)
        {
            return ShowDialogAsync(root, "出错啦！QAQ", content: content, close: "好的 T^T",
               defaultButton: ContentDialogButton.Close, canCopy: canCopy);
        }

        public static async Task<ContentDialogResult> ShowDialogAsync(XamlRoot root,
            string title = null, object content = null,
            string primary = null, string secondary = null, string close = null,
            ContentDialogButton defaultButton = ContentDialogButton.None, bool canCopy = false)
        {
            await s_semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                ContentDialog dialog = new()
                {
                    XamlRoot = root,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = title,
                    PrimaryButtonText = primary,
                    SecondaryButtonText = secondary,
                    CloseButtonText = close,
                    DefaultButton = defaultButton,
                    Content = content is string text
                    ? new TextBox()
                    {
                        Text = text,
                        IsReadOnly = canCopy
                    }
                    : content,
                };

                return await dialog.ShowAsync();
            }
            catch (Exception) { }
            finally
            {
                s_semaphore.Release();
            }
            return ContentDialogResult.None;
        }
    }
}
