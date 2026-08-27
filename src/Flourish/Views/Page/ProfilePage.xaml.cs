using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Localization;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfPage = System.Windows.Controls.Page;

namespace ArkheideSystem.Flourish.Views.Page;

internal partial class ProfilePage : WpfPage
{
    private readonly IProfileService profileService;
    private readonly LocalizationService localizationService;
    private readonly ProfileImageBrushCache profileImageCache = new();
    private string? selectedImagePath;
    private bool isEditingLogin;
    private bool isUpdatingState;
    private bool isSubscribed;

    public ProfilePage(
        IProfileService profileService,
        LocalizationService localizationService
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
        LoginButton.Content = localizationService.Get(LocaleKeys.ProfileSignIn);
        FirstNameLabel.Text = localizationService.Get(LocaleKeys.ProfileFirstName);
        LastNameLabel.Text = localizationService.Get(LocaleKeys.ProfileLastName);
        ProfileImageLabel.Text = localizationService.Get(LocaleKeys.ProfileImage);
        UploadImageText.Text = localizationService.Get(LocaleKeys.ProfileUploadImage);
        PasswordLabel.Text = localizationService.Get(LocaleKeys.ProfilePassword);
        CancelLoginButton.Content = localizationService.Get(LocaleKeys.ProfileCancel);
        SubmitLoginButton.Content = localizationService.Get(LocaleKeys.ProfileSignIn);
        RememberLoginCheckBox.Content = localizationService.Get(
            LocaleKeys.ProfileRememberLogin
        );
        LogoutButton.Content = localizationService.Get(LocaleKeys.ProfileSignOut);
    }

    private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!isSubscribed)
        {
            profileService.Changed += ProfileService_Changed;
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

        profileService.Changed -= ProfileService_Changed;
        localizationService.Changed -= LocalizationService_Changed;
        isSubscribed = false;
    }

    private void ProfileService_Changed(
        object? sender,
        StateChangedEventArgs<ProfileState> e
    )
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(UpdateState);
            return;
        }

        UpdateState();
    }

    private void LocalizationService_Changed(object? sender, LocalizationChangedEventArgs e)
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
        var profile = profileService.Current.Profile;
        isUpdatingState = true;
        try
        {
            isEditingLogin = true;
            FirstNameInput.Text = profile.FirstName;
            LastNameInput.Text = profile.LastName;
            var imageSource = ProfileImageLoader.Load(profile.ImagePath);
            profileImageCache.Set(profile.ImagePath, imageSource);
            selectedImagePath = imageSource is null ? null : profile.ImagePath;
            PasswordInput.Clear();
            ErrorText.Text = string.Empty;
            ApplyNameOrder(profile.NameOrder);
        }
        finally
        {
            isUpdatingState = false;
        }

        UpdateState();
        var firstInput = profile.NameOrder == NameOrder.FirstLast ? FirstNameInput : LastNameInput;
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
            Title = localizationService.Get(LocaleKeys.ProfileChooseImage),
            CheckFileExists = true,
            Multiselect = false,
            Filter =
                $"{localizationService.Get(LocaleKeys.ProfileImageFiles)}"
                + "|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|"
                + $"{localizationService.Get(LocaleKeys.ProfileAllFiles)}|*.*",
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
        {
            return;
        }

        var imageSource = ProfileImageLoader.Load(dialog.FileName);
        if (imageSource is null)
        {
            ErrorText.Text = localizationService.Get(LocaleKeys.ProfileImageLoadFailed);
            return;
        }

        profileImageCache.Set(dialog.FileName, imageSource);
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
                    profileService.Current.NameOrder,
                    selectedImagePath
                )
            );
            if (!result.Succeeded)
            {
                ErrorText.Text =
                    result.ErrorMessage
                    ?? localizationService.Get(LocaleKeys.ProfileSignInFailed);
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
        if (isUpdatingState || profileService.Current.LoginState == ProfileLoginState.SignedOut)
        {
            return;
        }

        SignedInErrorText.Text = string.Empty;
        SetBusy(true);
        try
        {
            await profileService.SetRememberLoginAsync(RememberLoginCheckBox.IsChecked == true);
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
            var state = profileService.Current;
            var profile = state.Profile;
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

            var isSignedIn = state.LoginState != ProfileLoginState.SignedOut;
            LoginStateText.Text = localizationService.Get(
                isSignedIn
                    ? LocaleKeys.ProfileSignedIn
                    : LocaleKeys.ProfileSignedOut
            );
            LoginButton.Visibility =
                !isSignedIn && !isEditingLogin ? Visibility.Visible : Visibility.Collapsed;
            LoginForm.Visibility =
                !isSignedIn && isEditingLogin ? Visibility.Visible : Visibility.Collapsed;
            SignedInPanel.Visibility = isSignedIn ? Visibility.Visible : Visibility.Collapsed;
            RememberLoginCheckBox.IsChecked =
                state.LoginState == ProfileLoginState.SignedInRemembered;
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
            firstName = localizationService.Get(LocaleKeys.ProfileDefaultName);
        }

        SetAvatar(
            new ProfileUser(
                firstName,
                lastName,
                profileService.Current.NameOrder,
                selectedImagePath
            )
        );
    }

    private void SetAvatar(ProfileUser profile)
    {
        var imageBrush = profileImageCache.Get(profile.ImagePath);
        if (!ReferenceEquals(AvatarImage.Fill, imageBrush))
        {
            AvatarImage.Fill = imageBrush;
        }

        AvatarImage.Visibility = imageBrush is null ? Visibility.Collapsed : Visibility.Visible;
        AvatarInitials.Text = profile.Initials;
        AvatarInitials.Visibility = imageBrush is null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetBusy(bool isBusy)
    {
        LoginForm.IsEnabled = !isBusy;
        SignedInPanel.IsEnabled = !isBusy;
    }
}
