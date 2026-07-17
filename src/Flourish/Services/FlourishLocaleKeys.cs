namespace ArkheideSystem.Flourish.Services;

internal static class FlourishLocaleKeys
{
    public const string TitleBarBack = "TitleBar.Back";
    public const string TitleBarForward = "TitleBar.Forward";
    public const string TitleBarToggleNavigation = "TitleBar.ToggleNavigation";
    public const string TitleBarTheme = "TitleBar.Theme";
    public const string TitleBarThemeSystem = "TitleBar.ThemeSystem";
    public const string TitleBarThemeCurrent = "TitleBar.ThemeCurrent";
    public const string TitleBarProfile = "TitleBar.Profile";
    public const string TitleBarApplicationInfo = "TitleBar.ApplicationInfo";
    public const string TitleBarProjectMenu = "TitleBar.ProjectMenu";
    public const string TitleBarNewProject = "TitleBar.NewProject";
    public const string TitleBarMinimize = "TitleBar.Minimize";
    public const string TitleBarMaximize = "TitleBar.Maximize";
    public const string TitleBarRestore = "TitleBar.Restore";
    public const string TitleBarClose = "TitleBar.Close";
    public const string ThemeDark = "Theme.Dark";
    public const string ThemeLight = "Theme.Light";
    public const string ProfileDefaultName = "Profile.DefaultName";
    public const string ProfileSignIn = "Profile.SignIn";
    public const string ProfileSignOut = "Profile.SignOut";
    public const string ProfileFirstName = "Profile.FirstName";
    public const string ProfileLastName = "Profile.LastName";
    public const string ProfileImage = "Profile.Image";
    public const string ProfileChooseImage = "Profile.ChooseImage";
    public const string ProfileUploadImage = "Profile.UploadImage";
    public const string ProfilePassword = "Profile.Password";
    public const string ProfileCancel = "Profile.Cancel";
    public const string ProfileRememberLogin = "Profile.RememberLogin";
    public const string ProfileSignedIn = "Profile.SignedIn";
    public const string ProfileSignedOut = "Profile.SignedOut";
    public const string ProfileImageFiles = "Profile.ImageFiles";
    public const string ProfileAllFiles = "Profile.AllFiles";
    public const string ProfileImageLoadFailed = "Profile.ImageLoadFailed";
    public const string ProfileSignInFailed = "Profile.SignInFailed";
    public const string ProfileEnterName = "Profile.EnterName";
    public const string ProfileEnterPassword = "Profile.EnterPassword";
    public const string ProfileRememberLoginRequiresSignIn =
        "Profile.RememberLoginRequiresSignIn";
    public const string BackgroundTaskTitle = "BackgroundTask.Title";
    public const string BackgroundTaskRunning = "BackgroundTask.Running";
    public const string BackgroundTaskQueued = "BackgroundTask.Queued";
    public const string BackgroundTaskCancelling = "BackgroundTask.Cancelling";
    public const string BackgroundTaskCancel = "BackgroundTask.Cancel";
    public const string BackgroundTaskWaitingCount = "BackgroundTask.WaitingCount";
    public const string BackgroundTaskNoActiveTasks = "BackgroundTask.NoActiveTasks";
    public const string SystemStatusTitle = "SystemStatus.Title";
    public const string SystemStatusNetwork = "SystemStatus.Network";
    public const string SystemStatusPower = "SystemStatus.Power";
    public const string SystemStatusAC = "SystemStatus.AC";
    public const string SystemStatusBattery = "SystemStatus.Battery";
    public const string SystemStatusUnknown = "SystemStatus.Unknown";
    public const string MessageBoxOk = "MessageBox.OK";
    public const string MessageBoxCancel = "MessageBox.Cancel";
    public const string MessageBoxYes = "MessageBox.Yes";
    public const string MessageBoxNo = "MessageBox.No";
    public const string WindowCloseTitle = "Window.CloseTitle";
    public const string WindowClosePrompt = "Window.ClosePrompt";
    public const string TrayShow = "Tray.Show";
    public const string TrayExit = "Tray.Exit";
    public const string StatusConnected = "Status.Connected";
    public const string StatusDisconnected = "Status.Disconnected";

    public static IReadOnlyList<string> All { get; } =
    [
        TitleBarBack,
        TitleBarForward,
        TitleBarToggleNavigation,
        TitleBarTheme,
        TitleBarThemeSystem,
        TitleBarThemeCurrent,
        TitleBarProfile,
        TitleBarApplicationInfo,
        TitleBarProjectMenu,
        TitleBarNewProject,
        TitleBarMinimize,
        TitleBarMaximize,
        TitleBarRestore,
        TitleBarClose,
        ThemeDark,
        ThemeLight,
        ProfileDefaultName,
        ProfileSignIn,
        ProfileSignOut,
        ProfileFirstName,
        ProfileLastName,
        ProfileImage,
        ProfileChooseImage,
        ProfileUploadImage,
        ProfilePassword,
        ProfileCancel,
        ProfileRememberLogin,
        ProfileSignedIn,
        ProfileSignedOut,
        ProfileImageFiles,
        ProfileAllFiles,
        ProfileImageLoadFailed,
        ProfileSignInFailed,
        ProfileEnterName,
        ProfileEnterPassword,
        ProfileRememberLoginRequiresSignIn,
        BackgroundTaskTitle,
        BackgroundTaskRunning,
        BackgroundTaskQueued,
        BackgroundTaskCancelling,
        BackgroundTaskCancel,
        BackgroundTaskWaitingCount,
        BackgroundTaskNoActiveTasks,
        SystemStatusTitle,
        SystemStatusNetwork,
        SystemStatusPower,
        SystemStatusAC,
        SystemStatusBattery,
        SystemStatusUnknown,
        MessageBoxOk,
        MessageBoxCancel,
        MessageBoxYes,
        MessageBoxNo,
        WindowCloseTitle,
        WindowClosePrompt,
        TrayShow,
        TrayExit,
        StatusConnected,
        StatusDisconnected,
    ];
}
