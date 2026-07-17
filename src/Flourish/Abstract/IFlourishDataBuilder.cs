namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures localization used by Flourish.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureData(data =>
/// {
///     data.SetLocale("EN")
///         .AddLocale("Locales/lang_EN.json");
/// });
/// ]]></code>
/// </example>
public interface IFlourishDataBuilder
{
    /// <summary>
    /// Sets the locale used by Flourish built-in interface text.
    /// </summary>
    /// <param name="locale">The locale identifier.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Flourish uses the built-in English locale when this method and
    /// <see cref="IFlourishBuilder.ConfigureData(Action{IFlourishDataBuilder})" /> are omitted.
    /// Built-in locale identifiers are <c>CN</c> and <c>EN</c>. Identifiers are
    /// case-insensitive and are normalized when the application is built.
    /// </remarks>
    IFlourishDataBuilder SetLocale(string locale);

    /// <summary>
    /// Adds a custom locale file that can extend or override built-in translations.
    /// </summary>
    /// <param name="path">The path to the custom locale file.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// <para>
    /// The file is read while <see cref="IFlourishBuilder.Build" /> applies configuration. It
    /// must be a UTF-8, non-empty, flat JSON object named <c>lang_&lt;locale&gt;.json</c>.
    /// Keys and values must be non-empty strings, and keys cannot be repeated. The locale segment
    /// may contain letters, digits, hyphens, and underscores.
    /// </para>
    /// <para>
    /// Files registered for the same locale are merged in registration order; a later file
    /// replaces earlier values for matching keys. Lookup priority is: custom selected locale,
    /// built-in selected locale, custom <c>EN</c>, built-in <c>EN</c>, then the key itself.
    /// A missing file throws <see cref="System.IO.FileNotFoundException" />. Invalid file names throw
    /// <see cref="ArgumentException" />. Unreadable files and invalid locale JSON throw
    /// <see cref="System.IO.InvalidDataException" />.
    /// </para>
    /// <para>A custom locale file can contain only the values it overrides:</para>
    /// <code><![CDATA[
    /// {
    ///   "TitleBar.Back": "Previous",
    ///   "Tray.Show": "Open"
    /// }
    /// ]]></code>
    /// <para>The canonical built-in translation keys and values are:</para>
    /// <list type="table">
    /// <listheader><term>Key</term><description>English / Simplified Chinese</description></listheader>
    /// <item><term><c>TitleBar.Back</c></term><description>Back / 返回</description></item>
    /// <item><term><c>TitleBar.Forward</c></term><description>Forward / 前进</description></item>
    /// <item><term><c>TitleBar.ToggleNavigation</c></term><description>Toggle navigation / 切换导航</description></item>
    /// <item><term><c>TitleBar.Theme</c></term><description>Theme / 主题</description></item>
    /// <item><term><c>TitleBar.ThemeSystem</c></term><description>Theme: System ({0}) / 主题：跟随系统（{0}）</description></item>
    /// <item><term><c>TitleBar.ThemeCurrent</c></term><description>Theme: {0} / 主题：{0}</description></item>
    /// <item><term><c>TitleBar.Profile</c></term><description>Profile / 个人资料</description></item>
    /// <item><term><c>TitleBar.ApplicationInfo</c></term><description>Application information / 应用信息</description></item>
    /// <item><term><c>TitleBar.ProjectMenu</c></term><description>Projects / 项目</description></item>
    /// <item><term><c>TitleBar.NewProject</c></term><description>New project / 新建项目</description></item>
    /// <item><term><c>TitleBar.Minimize</c></term><description>Minimize / 最小化</description></item>
    /// <item><term><c>TitleBar.Maximize</c></term><description>Maximize / 最大化</description></item>
    /// <item><term><c>TitleBar.Restore</c></term><description>Restore / 还原</description></item>
    /// <item><term><c>TitleBar.Close</c></term><description>Close / 关闭</description></item>
    /// <item><term><c>Theme.Dark</c></term><description>Dark / 深色</description></item>
    /// <item><term><c>Theme.Light</c></term><description>Light / 浅色</description></item>
    /// <item><term><c>Profile.DefaultName</c></term><description>User / 用户</description></item>
    /// <item><term><c>Profile.SignIn</c></term><description>Sign in / 登录</description></item>
    /// <item><term><c>Profile.SignOut</c></term><description>Sign out / 退出登录</description></item>
    /// <item><term><c>Profile.FirstName</c></term><description>First Name / 名</description></item>
    /// <item><term><c>Profile.LastName</c></term><description>Last Name / 姓</description></item>
    /// <item><term><c>Profile.Image</c></term><description>Profile image / 个人资料图片</description></item>
    /// <item><term><c>Profile.ChooseImage</c></term><description>Choose profile image / 选择个人资料图片</description></item>
    /// <item><term><c>Profile.UploadImage</c></term><description>Upload image / 上传图片</description></item>
    /// <item><term><c>Profile.Password</c></term><description>Password / 密码</description></item>
    /// <item><term><c>Profile.Cancel</c></term><description>Cancel / 取消</description></item>
    /// <item><term><c>Profile.RememberLogin</c></term><description>Remember login / 记住登录状态</description></item>
    /// <item><term><c>Profile.SignedIn</c></term><description>Signed in / 已登录</description></item>
    /// <item><term><c>Profile.SignedOut</c></term><description>Signed out / 未登录</description></item>
    /// <item><term><c>Profile.ImageFiles</c></term><description>Image files / 图片文件</description></item>
    /// <item><term><c>Profile.AllFiles</c></term><description>All files / 所有文件</description></item>
    /// <item><term><c>Profile.ImageLoadFailed</c></term><description>The selected image could not be loaded. / 无法加载所选图片。</description></item>
    /// <item><term><c>Profile.SignInFailed</c></term><description>Sign in failed. / 登录失败。</description></item>
    /// <item><term><c>Profile.EnterName</c></term><description>Enter a first or last name. / 请输入名字或姓氏。</description></item>
    /// <item><term><c>Profile.EnterPassword</c></term><description>Enter a password. / 请输入密码。</description></item>
    /// <item><term><c>Profile.RememberLoginRequiresSignIn</c></term><description>Remember login can only be changed while a profile is signed in. / 仅可在个人资料已登录时更改记住登录状态。</description></item>
    /// <item><term><c>BackgroundTask.Title</c></term><description>Background tasks / 后台任务</description></item>
    /// <item><term><c>BackgroundTask.Running</c></term><description>Running / 运行中</description></item>
    /// <item><term><c>BackgroundTask.Queued</c></term><description>Waiting / 等待中</description></item>
    /// <item><term><c>BackgroundTask.Cancelling</c></term><description>Cancelling / 正在取消</description></item>
    /// <item><term><c>BackgroundTask.Cancel</c></term><description>Cancel / 取消</description></item>
    /// <item><term><c>BackgroundTask.WaitingCount</c></term><description>{0} task(s) waiting / {0} 个任务等待中</description></item>
    /// <item><term><c>BackgroundTask.NoActiveTasks</c></term><description>No active background tasks / 没有活动的后台任务</description></item>
    /// <item><term><c>SystemStatus.Title</c></term><description>System status / 系统状态</description></item>
    /// <item><term><c>SystemStatus.Network</c></term><description>Network / 网络</description></item>
    /// <item><term><c>SystemStatus.Power</c></term><description>Power / 电源</description></item>
    /// <item><term><c>SystemStatus.AC</c></term><description>AC power / 外接电源</description></item>
    /// <item><term><c>SystemStatus.Battery</c></term><description>Battery / 电池供电</description></item>
    /// <item><term><c>SystemStatus.Unknown</c></term><description>Unknown / 未知</description></item>
    /// <item><term><c>MessageBox.OK</c></term><description>OK / 确定</description></item>
    /// <item><term><c>MessageBox.Cancel</c></term><description>Cancel / 取消</description></item>
    /// <item><term><c>MessageBox.Yes</c></term><description>Yes / 是</description></item>
    /// <item><term><c>MessageBox.No</c></term><description>No / 否</description></item>
    /// <item><term><c>Window.CloseTitle</c></term><description>Close / 关闭</description></item>
    /// <item><term><c>Window.ClosePrompt</c></term><description>Are you sure you want to close this window? / 确定要关闭此窗口吗？</description></item>
    /// <item><term><c>Tray.Show</c></term><description>Show / 显示</description></item>
    /// <item><term><c>Tray.Exit</c></term><description>Exit / 退出</description></item>
    /// <item><term><c>Status.Connected</c></term><description>Connected / 已连接</description></item>
    /// <item><term><c>Status.Disconnected</c></term><description>Disconnected / 未连接</description></item>
    /// </list>
    /// </remarks>
    IFlourishDataBuilder AddLocale(string path);

}
