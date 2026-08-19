(function () {
  if (!document.querySelector('script[src="assets/index-navbar.js"]')) {
    const navbarScript = document.createElement("script");
    navbarScript.src = "assets/index-navbar.js";
    document.head.appendChild(navbarScript);
  }

  const flagPath = (lang) => `assets/flags/${lang}.svg`;
  const normalize = (lang) => String(lang || "tr").trim().toLowerCase();
  const codeOf = (lang) => normalize(lang).toUpperCase();
  const urlLanguage = () => normalize(new URLSearchParams(location.search).get("lang"));

  const closeAll = () => {
    document.querySelectorAll(".lang__dd.open").forEach((dropdown) => {
      dropdown.classList.remove("open");
    });
  };

  const ensureHiddenStyle = () => {
    if (document.getElementById("common-language-selector-style")) return;
    const style = document.createElement("style");
    style.id = "common-language-selector-style";
    style.textContent = ".lang__dd button[hidden]{display:none!important}";
    document.head.appendChild(style);
  };

  const optionLang = (option) => normalize(option.dataset.lang || option.textContent);

  const setSelected = (lang) => {
    const selected = normalize(lang);

    document.querySelectorAll(".lang").forEach((root) => {
      if (root.closest('header[data-shared-navbar="ng-slim"]')) return;
      const trigger = root.querySelector(".lang__btn");
      const code = root.querySelector(".lang__code");
      const triggerFlag = trigger?.querySelector(".lang__flag");
      const dropdown = root.querySelector(".lang__dd");

      if (!trigger || !code || !triggerFlag || !dropdown) return;

      root.dataset.lang = selected;
      code.textContent = codeOf(selected);
      triggerFlag.src = flagPath(selected);

      dropdown.querySelectorAll("button").forEach((option) => {
        const langCode = optionLang(option);
        option.dataset.lang = langCode;
        option.hidden = langCode === selected;
        option.classList.toggle("active", langCode === selected);
      });
    });
  };

  window.toggleLang = function () {
    const dropdown = document.querySelector(".lang__dd");
    if (!dropdown) return;
    dropdown.classList.toggle("open");
  };

  window.selLang = function (lang, code, button) {
    setSelected(lang || button?.dataset.lang || code);
    closeAll();
  };

  const init = () => {
    ensureHiddenStyle();

    document.querySelectorAll(".lang").forEach((root) => {
      if (root.closest('header[data-shared-navbar="ng-slim"]')) return;
      const trigger = root.querySelector(".lang__btn");
      const dropdown = root.querySelector(".lang__dd");
      const code = root.querySelector(".lang__code");

      if (!trigger || !dropdown || !code) return;

      trigger.removeAttribute("onclick");
      trigger.type = "button";

      dropdown.querySelectorAll("button").forEach((option) => {
        option.removeAttribute("onclick");
        option.type = "button";
        option.dataset.lang = optionLang(option);
        option.addEventListener("click", (event) => {
          event.stopPropagation();
          const selected = option.dataset.lang;
          setSelected(selected);
          closeAll();
          const url = new URL(location.href);
          url.searchParams.set("lang", selected);
          location.assign(url.href);
        });
      });

      trigger.addEventListener("click", (event) => {
        event.stopPropagation();
        const shouldOpen = !dropdown.classList.contains("open");
        closeAll();
        dropdown.classList.toggle("open", shouldOpen);
      });

      setSelected(
        urlLanguage() || root.dataset.lang ||
          dropdown.querySelector("button.active")?.dataset.lang ||
          code.textContent ||
          "tr",
      );
    });
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }

  document.addEventListener("click", (event) => {
    if (!event.target.closest(".lang")) closeAll();
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") closeAll();
  });

  // Footer yasal bağlantıları bütün sayfalarda aynı hedefleri kullanır.
  const syncFooterLegalLinks = () => {
    const targets = {
      Gizlilik: "kvkk-gizlilik.html#gizlilik",
      Çerezler: "cerez-politikasi.html",
      KVKK: "kvkk-gizlilik.html#kvkk",
    };
    document.querySelectorAll(".footer__legal a").forEach((link) => {
      const target = targets[link.textContent.trim()];
      if (target) link.href = target;
    });
  };
  const syncSiteNavigation = () => {
    const targets = {
      "NG Kütahya Seramik": "ng-kutahya-seramik.html",
      "NG Stone": "ng-stone.html",
      "NG Slim": "ng-slim.html",
      "NG Performa": "ng-performa.html",
      "Koleksiyonlar": "index-koleksiyonlar.html",
      "Tüm Koleksiyonlar": "index-koleksiyonlar.html",
      "Referans Projeler": "projeler.html",
      "Projeler": "projeler.html",
      "Teknik Dokümanlar": "teknik-dokumanlar.html",
      "Genel Merkez": "genel-merkez.html",
      "Fabrika": "fabrikalar.html",
      "Satış Noktaları": "satis-noktalari.html",
      "Blog": "blog.html",
      "Blog Yazıları": "blog.html",
      "Haberler": "haberler.html",
      "Bültenler": "haberler.html",
      "Hakkımızda": "hakkimizda.html",
      "Kariyer": "kariyer.html",
      "Gizlilik": "kvkk-gizlilik.html#gizlilik",
      "Çerezler": "cerez-politikasi.html",
      "KVKK": "kvkk-gizlilik.html#kvkk"
    };
    document.querySelectorAll("header a, footer a, .mobile-menu a, .side-menu a").forEach((link) => {
      const target = targets[link.textContent.trim()];
      if (target) link.href = target;
    });
    document.querySelectorAll('a[href="showroom-detay.html"]').forEach((link) => { link.href = "bayi-detay.html"; });
    document.querySelectorAll("header .mega-menu__columns a").forEach((link) => {
      if (link.textContent.trim() === "Zemin ve Duvar") link.textContent = "Havuz";
    });
    document.querySelectorAll(".mobile-menu nav a, .side-menu__nav a").forEach((link) => {
      if (link.querySelector(".nav-link__label")) return;
      const textNode = [...link.childNodes].find((node) => node.nodeType === Node.TEXT_NODE && node.textContent.trim());
      if (!textNode) return;
      const label = document.createElement("span");
      label.className = "nav-link__label";
      label.textContent = textNode.textContent.trim();
      link.replaceChild(label, textNode);
    });
    document.querySelectorAll(".footer__col").forEach((column) => {
      if (column.querySelector("h4")?.textContent.trim() !== "Medya") return;
      if ([...column.querySelectorAll("a")].some((link) => link.textContent.trim() === "Blog Yazıları")) return;
      const blogLink = document.createElement("a");
      blogLink.href = "blog.html";
      blogLink.textContent = "Blog Yazıları";
      column.appendChild(blogLink);
    });
    const corporateTargets = {
      "Hakkımızda": "hakkimizda.html#hakkimizda",
      "Tarihçe": "hakkimizda.html#tarihce",
      "Üretim": "hakkimizda.html#uretim",
      "Temel Değerlerimiz": "hakkimizda.html#degerlerimiz",
      "Başarılar / Ödüller": "hakkimizda.html#odullar",
      "Sertifikalar": "hakkimizda.html#sertifikalar",
      "İş Birlikleri": "hakkimizda.html#isbirlikleri",
      "Bilgi Toplumu Hizmetleri": "hakkimizda.html#bilgitoplumu"
    };
    document.querySelectorAll(".footer__col").forEach((column) => {
      if (column.querySelector("h4")?.textContent.trim() !== "Kurumsal") return;
      const heading = column.querySelector("h4");
      if (heading && !heading.querySelector(".footer__heading-link")) {
        const headingLink = document.createElement("a");
        headingLink.className = "footer__heading-link";
        headingLink.href = "hakkimizda.html";
        headingLink.textContent = heading.textContent.trim();
        heading.replaceChildren(headingLink);
      }
      column.querySelectorAll("a").forEach((link) => {
        const target = corporateTargets[link.textContent.trim()];
        if (target) link.href = target;
      });
    });
    document.querySelectorAll(".footer__col").forEach((column) => {
      if (column.querySelector("h4")?.textContent.trim() !== "Katalog ve Belgeler") return;
      column.querySelectorAll("a").forEach((link) => {
        if (link.textContent.trim() === "Sertifikalar") link.href = "hakkimizda.html#sertifikalar";
      });
    });
  };
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => { syncFooterLegalLinks(); syncSiteNavigation(); });
  } else {
    syncFooterLegalLinks();
    syncSiteNavigation();
  }
  document.addEventListener("click", (event) => {
    const link = event.target.closest(".footer__legal a");
    if (link?.textContent.trim() === "Yasal Uyarı") event.preventDefault();
    const showroomPlaceholder = event.target.closest("header a, .mobile-menu a, .side-menu a");
    if (showroomPlaceholder?.textContent.trim() === "Showroomlar") event.preventDefault();
  });
})();
