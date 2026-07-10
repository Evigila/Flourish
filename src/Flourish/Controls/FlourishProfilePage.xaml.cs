using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ArkheideSystem.Flourish.Controls;

internal partial class FlourishProfilePage : Page
{
    private readonly IProfileService profileService;
    private bool isEditingLogin;
    private bool isUpdatingState;
    private bool isSubscribed;

    public FlourishProfilePage(IProfileService profileService)
    {
        this.profileService = profileService;
        InitializeComponent();
        Loaded += ProfilePage_Loaded;
        Unloaded += ProfilePage_Unloaded;
        UpdateState();
    }

    private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!isSubscribed)
        {
            profileService.ProfileChanged += ProfileService_ProfileChanged;
            isSubscribed = true;
        }

        UpdateState();
    }

    private void ProfilePage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (!isSubscribed)
        {
            return;
        }

        profileService.ProfileChanged -= ProfileService_ProfileChanged;
        isSubscribed = false;
    }

    private void ProfileService_ProfileChanged(object? sender, ProfileChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(UpdateState);
            return;
        }

        UpdateState();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        isEditingLogin = true;
        UserNameInput.Text = profileService.CurrentProfile.UserName;
        ImagePathInput.Text = profileService.CurrentProfile.ImagePath ?? string.Empty;
        PasswordInput.Clear();
        ErrorText.Text = string.Empty;
        UpdateState();
        UserNameInput.Focus();
        UserNameInput.SelectAll();
    }

    private void CancelLoginButton_Click(object sender, RoutedEventArgs e)
    {
        isEditingLogin = false;
        PasswordInput.Clear();
        ErrorText.Text = string.Empty;
        UpdateState();
    }

    private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose profile image",
            CheckFileExists = true,
            Multiselect = false,
            Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|All files|*.*",
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
        {
            return;
        }

        ImagePathInput.Text = dialog.FileName;
        UpdateAvatarPreview();
    }

    private async void SubmitLoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        SetBusy(true);
        try
        {
            var result = await profileService.SignInAsync(
                new ProfileSignInRequest(
                    UserNameInput.Text,
                    PasswordInput.Password,
                    ImagePathInput.Text
                )
            );
            if (!result.Succeeded)
            {
                ErrorText.Text = result.ErrorMessage ?? "Sign in failed.";
                return;
            }

            isEditingLogin = false;
            PasswordInput.Clear();
            UpdateState();
        }
        catch (Exception error)
        {
            ErrorText.Text = error.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void RememberLoginCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (isUpdatingState || profileService.LoginState == ProfileLoginState.SignedOut)
        {
            return;
        }

        SignedInErrorText.Text = string.Empty;
        SetBusy(true);
        try
        {
            await profileService.SetRememberLoginAsync(
                RememberLoginCheckBox.IsChecked == true
            );
        }
        catch (Exception error)
        {
            SignedInErrorText.Text = error.Message;
            UpdateState();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        SignedInErrorText.Text = string.Empty;
        SetBusy(true);
        try
        {
            await profileService.SignOutAsync();
            isEditingLogin = false;
            UpdateState();
        }
        catch (Exception error)
        {
            SignedInErrorText.Text = error.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateState()
    {
        isUpdatingState = true;
        try
        {
            var profile = profileService.CurrentProfile;
            UserNameText.Text = profile.UserName;
            SetAvatar(profile);

            var isSignedIn = profileService.LoginState != ProfileLoginState.SignedOut;
            LoginStateText.Text = isSignedIn ? "Signed in" : "Signed out";
            LoginButton.Visibility = !isSignedIn && !isEditingLogin
                ? Visibility.Visible
                : Visibility.Collapsed;
            LoginForm.Visibility = !isSignedIn && isEditingLogin
                ? Visibility.Visible
                : Visibility.Collapsed;
            SignedInPanel.Visibility = isSignedIn
                ? Visibility.Visible
                : Visibility.Collapsed;
            RememberLoginCheckBox.IsChecked =
                profileService.LoginState == ProfileLoginState.SignedInRemembered;
        }
        finally
        {
            isUpdatingState = false;
        }
    }

    private void UpdateAvatarPreview()
    {
        var userName = string.IsNullOrWhiteSpace(UserNameInput.Text)
            ? profileService.CurrentProfile.UserName
            : UserNameInput.Text;
        SetAvatar(new ProfileUser(userName, ImagePathInput.Text));
    }

    private void SetAvatar(ProfileUser profile)
    {
        var imageSource = ProfileImageLoader.Load(profile.ImagePath);
        AvatarImage.Fill = imageSource is null
            ? null
            : new ImageBrush(imageSource) { Stretch = Stretch.UniformToFill };
        AvatarImage.Visibility = imageSource is null
            ? Visibility.Collapsed
            : Visibility.Visible;
        AvatarInitials.Text = profile.Initials;
        AvatarInitials.Visibility = imageSource is null
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void SetBusy(bool isBusy)
    {
        LoginForm.IsEnabled = !isBusy;
        SignedInPanel.IsEnabled = !isBusy;
    }
}
