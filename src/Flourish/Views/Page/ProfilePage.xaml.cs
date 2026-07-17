using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Imaging;
using ArkheideSystem.Flourish.Services;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfPage = System.Windows.Controls.Page;

namespace ArkheideSystem.Flourish.Views.Page;

internal partial class FlourishProfilePage : WpfPage
{
    private readonly IProfileService profileService;
    private readonly FlourishLocalizationService localizationService;
    private string? selectedImagePath;
    private bool isEditingLogin;
    private bool isUpdatingState;
    private bool isSubscribed;

    public FlourishProfilePage(
        IProfileService profileService,
        FlourishLocalizationService localizationService
    )
    {
        this.profileService = profileService;
        this.localizationService = localizationService;
        InitializeComponent();
        ApplyLocale();
        Loaded += ProfilePage_Loaded;
        Unloaded += ProfilePage_Unloaded;
        UpdateState();
    }

    private void ApplyLocale()
    {
        LoginButton.Content = localizationService.Get(FlourishLocaleKeys.ProfileSignIn);
        FirstNameLabel.Text = localizationService.Get(FlourishLocaleKeys.ProfileFirstName);
        LastNameLabel.Text = localizationService.Get(FlourishLocaleKeys.ProfileLastName);
        ProfileImageLabel.Text = localizationService.Get(FlourishLocaleKeys.ProfileImage);
        UploadImageText.Text = localizationService.Get(FlourishLocaleKeys.ProfileUploadImage);
        PasswordLabel.Text = localizationService.Get(FlourishLocaleKeys.ProfilePassword);
        CancelLoginButton.Content = localizationService.Get(FlourishLocaleKeys.ProfileCancel);
        SubmitLoginButton.Content = localizationService.Get(FlourishLocaleKeys.ProfileSignIn);
        RememberLoginCheckBox.Content = localizationService.Get(
            FlourishLocaleKeys.ProfileRememberLogin
        );
        LogoutButton.Content = localizationService.Get(FlourishLocaleKeys.ProfileSignOut);
    }

    private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!isSubscribed)
        {
            profileService.ProfileChanged += ProfileService_ProfileChanged;
            localizationService.Changed += LocalizationService_Changed;
            isSubscribed = true;
        }

        ApplyLocale();
        UpdateState();
    }

    private void ProfilePage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (!isSubscribed)
        {
            return;
        }

        profileService.ProfileChanged -= ProfileService_ProfileChanged;
        localizationService.Changed -= LocalizationService_Changed;
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

    private void LocalizationService_Changed(
        object? sender,
        FlourishLocalizationChangedEventArgs e
    )
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() =>
            {
                ApplyLocale();
                UpdateState();
            });
            return;
        }

        ApplyLocale();
        UpdateState();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var profile = profileService.CurrentProfile;
        isUpdatingState = true;
        try
        {
            isEditingLogin = true;
            FirstNameInput.Text = profile.FirstName;
            LastNameInput.Text = profile.LastName;
            selectedImagePath = ProfileImageLoader.Load(profile.ImagePath) is null
                ? null
                : profile.ImagePath;
            PasswordInput.Clear();
            ErrorText.Text = string.Empty;
            ApplyNameOrder(profile.NameOrder);
        }
        finally
        {
            isUpdatingState = false;
        }

        UpdateState();
        var firstInput = profile.NameOrder == NameOrder.FirstLast
            ? FirstNameInput
            : LastNameInput;
        firstInput.Focus();
        firstInput.SelectAll();
    }

    private void CancelLoginButton_Click(object sender, RoutedEventArgs e)
    {
        isEditingLogin = false;
        selectedImagePath = null;
        PasswordInput.Clear();
        ErrorText.Text = string.Empty;
        UpdateState();
    }

    private void UploadImageButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = localizationService.Get(FlourishLocaleKeys.ProfileChooseImage),
            CheckFileExists = true,
            Multiselect = false,
            Filter = $"{localizationService.Get(FlourishLocaleKeys.ProfileImageFiles)}"
                + "|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|"
                + $"{localizationService.Get(FlourishLocaleKeys.ProfileAllFiles)}|*.*",
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
        {
            return;
        }

        if (ProfileImageLoader.Load(dialog.FileName) is null)
        {
            ErrorText.Text = localizationService.Get(
                FlourishLocaleKeys.ProfileImageLoadFailed
            );
            return;
        }

        selectedImagePath = dialog.FileName;
        ErrorText.Text = string.Empty;
        UpdateAvatarPreview();
    }

    private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!isEditingLogin || isUpdatingState)
        {
            return;
        }

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
                    FirstNameInput.Text,
                    LastNameInput.Text,
                    PasswordInput.Password,
                    profileService.CurrentProfile.NameOrder,
                    selectedImagePath
                )
            );
            if (!result.Succeeded)
            {
                ErrorText.Text = result.ErrorMessage
                    ?? localizationService.Get(FlourishLocaleKeys.ProfileSignInFailed);
                return;
            }

            isEditingLogin = false;
            selectedImagePath = null;
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
            selectedImagePath = null;
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
            DisplayNameText.Text = profile.DisplayName;
            ApplyNameOrder(profile.NameOrder);
            if (isEditingLogin)
            {
                UpdateAvatarPreview();
            }
            else
            {
                SetAvatar(profile);
            }

            var isSignedIn = profileService.LoginState != ProfileLoginState.SignedOut;
            LoginStateText.Text = localizationService.Get(
                isSignedIn
                    ? FlourishLocaleKeys.ProfileSignedIn
                    : FlourishLocaleKeys.ProfileSignedOut
            );
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

    private void ApplyNameOrder(NameOrder nameOrder)
    {
        Grid.SetColumn(FirstNameField, nameOrder == NameOrder.FirstLast ? 0 : 2);
        Grid.SetColumn(LastNameField, nameOrder == NameOrder.FirstLast ? 2 : 0);
    }

    private void UpdateAvatarPreview()
    {
        var firstName = FirstNameInput.Text.Trim();
        var lastName = LastNameInput.Text.Trim();
        if (firstName.Length == 0 && lastName.Length == 0)
        {
            firstName = localizationService.Get(FlourishLocaleKeys.ProfileDefaultName);
        }

        SetAvatar(
            new ProfileUser(
                firstName,
                lastName,
                profileService.CurrentProfile.NameOrder,
                selectedImagePath
            )
        );
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
