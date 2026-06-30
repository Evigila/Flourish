namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishFontService
{
    string FontFamily { get; }

    string IconFontFamily { get; }

    double FontSize { get; }

    void SetFont(string fontFamily, double fontSize);

    void SetFontFamily(string fontFamily);

    void SetFontSize(double fontSize);

    void SetIconFontFamily(string fontFamily);
}
