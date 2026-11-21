using System.Globalization;
using Microsoft.Extensions.Localization;
using MoveWell.Common.Cultures;

namespace MoveWell.Web.Localization;

/// <summary>
/// View helper over the JSON localizer: <c>@{ var t = Ui.For("public/account/signin"); } @t["title"]</c>
/// → <c>Resources/public/account/signin.{fa|en}.json</c>. Also exposes the page language and direction.
/// </summary>
public sealed class UiText
{
    private readonly IStringLocalizerFactory _factory;

    public UiText(IStringLocalizerFactory factory)
    {
        _factory = factory;
    }

    public IStringLocalizer For(string page) => _factory.Create(page, string.Empty);

    public IStringLocalizer Common => For("public/shared/common");

    public string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == Cultures.En ? Cultures.En : Cultures.Fa;

    public string Direction => Cultures.IsRightToLeft(Language) ? "rtl" : "ltr";

    public bool IsEnglish => Language == Cultures.En;
}
