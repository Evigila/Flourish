function getAlternateLanguageUrl() {
  const path = window.location.pathname;
  const search = window.location.search;
  const hash = window.location.hash;

  if (path.includes("/zh-cn/")) {
    return {
      label: "EN",
      title: "Switch to English",
      href: path.replace("/zh-cn/", "/en-us/") + search + hash,
      disabled: false,
    };
  }

  if (path.includes("/en-us/")) {
    return {
      label: "\u4e2d\u6587",
      title: "\u5207\u6362\u5230\u4e2d\u6587",
      href: path.replace("/en-us/", "/zh-cn/") + search + hash,
      disabled: false,
    };
  }

  return {
    label: "\u4e2d\u6587 / EN",
    title: "Language switching is ready for /zh-cn/ and /en-us/ builds.",
    href: "#",
    disabled: true,
  };
}

function addLanguageSwitch() {
  const target =
    document.querySelector(".navbar .navbar-nav") ||
    document.querySelector(".navbar .container-xxl") ||
    document.querySelector(".navbar");

  if (!target || document.querySelector(".flourish-language-switch")) {
    return;
  }

  const language = getAlternateLanguageUrl();
  const link = document.createElement("a");
  link.className = "flourish-language-switch";
  link.href = language.href;
  link.textContent = language.label;
  link.title = language.title;
  link.setAttribute("aria-label", language.title);

  if (language.disabled) {
    link.dataset.disabled = "true";
    link.addEventListener("click", (event) => event.preventDefault());
  }

  target.appendChild(link);
}

function isChinesePage() {
  const lang = document.documentElement.getAttribute("lang") || "";

  return lang.toLowerCase().startsWith("zh") || window.location.pathname.includes("/zh-cn/");
}

function replaceExactText(selector, translations) {
  document.querySelectorAll(selector).forEach((element) => {
    const text = element.textContent.trim();
    const translated = translations[text];

    if (translated) {
      element.textContent = translated;
    }
  });
}

function replaceAttributes(selector, attributes, translations) {
  document.querySelectorAll(selector).forEach((element) => {
    attributes.forEach((attribute) => {
      const value = element.getAttribute(attribute);
      const translated = value ? translations[value] : null;

      if (translated) {
        element.setAttribute(attribute, translated);
      }
    });
  });
}

function localizeTypePrefixText(text) {
  return text
    .replace(/^Class /, "\u7c7b ")
    .replace(/^Interface /, "\u63a5\u53e3 ")
    .replace(/^Enum /, "\u679a\u4e3e ")
    .replace(/^Struct /, "\u7ed3\u6784 ")
    .replace(/^Delegate /, "\u59d4\u6258 ")
    .replace(/^Namespace /, "\u547d\u540d\u7a7a\u95f4 ");
}

function localizeTypePrefixElement(element) {
  const textNode = Array.from(element.childNodes).find((node) => node.nodeType === Node.TEXT_NODE);

  if (!textNode) {
    return;
  }

  textNode.textContent = textNode.textContent.replace(
    /^(\s*)(Class|Interface|Enum|Struct|Delegate|Namespace)\s+/,
    (_, leading, kind) => {
      const translations = {
        Class: "\u7c7b",
        Interface: "\u63a5\u53e3",
        Enum: "\u679a\u4e3e",
        Struct: "\u7ed3\u6784",
        Delegate: "\u59d4\u6258",
        Namespace: "\u547d\u540d\u7a7a\u95f4",
      };

      return `${leading}${translations[kind]} `;
    },
  );
}

function localizeChineseChrome() {
  if (!isChinesePage()) {
    return;
  }

  const sectionTranslations = {
    Constructors: "\u6784\u9020\u51fd\u6570",
    Events: "\u4e8b\u4ef6",
    Examples: "\u793a\u4f8b",
    Exceptions: "\u5f02\u5e38",
    Fields: "\u5b57\u6bb5",
    Methods: "\u65b9\u6cd5",
    Parameters: "\u53c2\u6570",
    Properties: "\u5c5e\u6027",
    Remarks: "\u5907\u6ce8",
    Returns: "\u8fd4\u56de\u503c",
    "See Also": "\u53c2\u89c1",
    "Type Parameters": "\u7c7b\u578b\u53c2\u6570",
  };

  const factTranslations = {
    Assembly: "\u7a0b\u5e8f\u96c6",
    Derived: "\u6d3e\u751f",
    "Derived Classes": "\u6d3e\u751f\u7c7b",
    "Extension Methods": "\u6269\u5c55\u65b9\u6cd5",
    Implements: "\u5b9e\u73b0",
    Inheritance: "\u7ee7\u627f",
    "Inherited Members": "\u7ee7\u627f\u6210\u5458",
    Namespace: "\u547d\u540d\u7a7a\u95f4",
  };

  const uiTranslations = {
    Close: "\u5173\u95ed",
    Search: "\u641c\u7d22",
    "Show table of contents": "\u663e\u793a\u76ee\u5f55",
    "Switch to dark mode": "\u5207\u6362\u5230\u6df1\u8272\u6a21\u5f0f",
    "Switch to light mode": "\u5207\u6362\u5230\u6d45\u8272\u6a21\u5f0f",
    "Table of Contents": "\u76ee\u5f55",
    "View source": "\u67e5\u770b\u6e90\u7801",
  };

  const locMetaTranslations = {
    "Filter by title": "\u6309\u6807\u9898\u8fc7\u6ee4",
    "In this article": "\u672c\u6587\u5185\u5bb9",
    Next: "\u4e0b\u4e00\u7bc7",
    Previous: "\u4e0a\u4e00\u7bc7",
    "{count} results for \"{query}\"": "\u627e\u5230 {count} \u6761\u4e0e \"{query}\" \u76f8\u5173\u7684\u7ed3\u679c",
    "No results for \"{query}\"": "\u672a\u627e\u5230\u4e0e \"{query}\" \u76f8\u5173\u7684\u7ed3\u679c",
  };

  replaceExactText("h2.section, h2, h3.section, h4.section, h5.offcanvas-title", {
    ...sectionTranslations,
    ...uiTranslations,
  });
  replaceExactText("dt", factTranslations);
  replaceAttributes("[title], [aria-label], input[placeholder]", ["title", "aria-label", "placeholder"], uiTranslations);
  replaceAttributes("meta[name^='loc:']", ["content"], locMetaTranslations);

  document.querySelectorAll("h1").forEach(localizeTypePrefixElement);
  document.title = localizeTypePrefixText(document.title);
  replaceAttributes("meta[name='title']", ["content"], {
    [document.querySelector("meta[name='title']")?.getAttribute("content") || ""]: localizeTypePrefixText(
      document.querySelector("meta[name='title']")?.getAttribute("content") || "",
    ),
  });
}

function start() {
  addLanguageSwitch();
  localizeChineseChrome();
}

export default {
  start,
};
