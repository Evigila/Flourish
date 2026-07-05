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
    title: "Language switching is available in /zh-cn/ and /en-us/ builds.",
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
  link.className = "btn btn-sm btn-outline-secondary ms-md-2 mt-2 mt-md-0 flourish-language-switch";
  link.href = language.href;
  link.textContent = language.label;
  link.title = language.title;
  link.setAttribute("aria-label", language.title);

  if (language.disabled) {
    link.setAttribute("aria-disabled", "true");
    link.addEventListener("click", (event) => event.preventDefault());
  }

  target.appendChild(link);
}

function start() {
  addLanguageSwitch();
}

export default {
  start,
};
