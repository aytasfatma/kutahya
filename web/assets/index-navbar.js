(() => {
  if (window.__ngSlimNavbarInitialized) return;
  window.__ngSlimNavbarInitialized = true;

  if (!document.querySelector('link[href^="assets/slim-navbar.css"]')) {
    const stylesheet = document.createElement("link");
    stylesheet.rel = "stylesheet";
    stylesheet.href = "assets/slim-navbar.css";
    document.head.appendChild(stylesheet);
  }

  const currentHeader = document.querySelector("header.header");
  if (!currentHeader) return;

  currentHeader.outerHTML = `
    <header class="header" data-shared-navbar="ng-slim">
      <div class="header__inner">
        <a class="header__brand" href="index.html">
          <img alt="NG Kütahya Seramik" class="logo-img" src="https://www.mostidea.com.tr/ngkutahya/Alt1/assets/ng-kutahya-logo.png">
        </a>
        <nav aria-label="Ana navigasyon" class="nav nav--mega">
          <div class="nav-group">
            <button class="nav__trigger" type="button">Markalarımız</button>
            <div class="mega-menu">
              <div class="mega-menu__inner">
                <div class="mega-menu__links">
                  <a href="ng-kutahya-seramik.html">NG Kütahya Seramik</a>
                  <a href="ng-stone.html">NG Stone</a>
                  <a href="ng-slim.html">NG Slim</a>
                  <a href="ng-performa.html">NG Performa</a>
                </div>
              </div>
            </div>
          </div>
          <div class="nav-group">
            <button class="nav__trigger" type="button">Ürünler</button>
            <div class="mega-menu">
              <div class="mega-menu__inner">
                <div class="mega-menu__columns">
                  <div>
                    <h4>Yüzey Desenleri</h4>
                    <a>Mermer Görünümlü</a><a>Doğal Taş Görünümlü</a><a>Ahşap Görünümlü</a><a>Cement Görünümlü</a><a>Solid Görünümlü</a>
                  </div>
                  <div>
                    <h4>Uygulama Alanları</h4>
                    <a>Mutfak</a><a>Banyo</a><a>Dış Mekân</a><a>İç Mekân</a><a>Mobilya</a><a>Havuz</a>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="nav-group">
            <button class="nav__trigger" type="button">Profesyoneller</button>
            <div class="mega-menu">
              <div class="mega-menu__inner">
                <div class="mega-menu__links">
                  <a>3D Viewer / Mekân Görselleştirici</a><a href="projeler.html">Referans Projeler</a><a>Teknik Uygulamalar</a><a href="teknik-dokumanlar.html">Teknik Dokümanlar</a>
                </div>
              </div>
            </div>
          </div>
          <div class="nav-group">
            <button class="nav__trigger" type="button">İletişim</button>
            <div class="mega-menu">
              <div class="mega-menu__inner">
                <div class="mega-menu__links">
                  <a href="genel-merkez.html">Genel Merkez</a><a href="fabrikalar.html">Fabrika</a><a aria-disabled="true">Showroomlar</a><a href="satis-noktalari.html">Satış Noktaları</a>
                </div>
              </div>
            </div>
          </div>
        </nav>
        <div class="header__right">
          <button aria-label="Ara" type="button">
            <svg fill="none" height="19" stroke="currentColor" stroke-linecap="round" stroke-width="1.3" viewBox="0 0 24 24" width="19"><circle cx="11" cy="11" r="7"></circle><path d="M21 21l-4.35-4.35"></path></svg>
          </button>
          <div class="lang">
            <button class="lang__btn" type="button">
              <img alt="" class="lang__flag" height="15" src="assets/flags/tr.svg" width="22"><span class="lang__code">TR</span>
              <svg fill="none" height="11" stroke="currentColor" stroke-width="1.6" viewBox="0 0 24 24" width="11"><path d="M6 9l6 6 6-6"></path></svg>
            </button>
            <div class="lang__dd" id="langDd">
              <button class="active" data-lang="tr" type="button"><img alt="" class="lang__flag" src="assets/flags/tr.svg">TR</button>
              <button data-lang="en" type="button"><img alt="" class="lang__flag" src="assets/flags/en.svg">EN</button>
              <button data-lang="de" type="button"><img alt="" class="lang__flag" src="assets/flags/de.svg">DE</button>
              <button data-lang="fr" type="button"><img alt="" class="lang__flag" src="assets/flags/fr.svg">FR</button>
              <button data-lang="ru" type="button"><img alt="" class="lang__flag" src="assets/flags/ru.svg">RU</button>
              <button data-lang="es" type="button"><img alt="" class="lang__flag" src="assets/flags/es.svg">ES</button>
              <button data-lang="ar" type="button"><img alt="" class="lang__flag" src="assets/flags/ar.svg">AR</button>
            </div>
          </div>
          <a class="dealer-btn">İş Ortağı Portalı</a>
          <button aria-label="Menü" class="mobile-menu-button" type="button">
            <svg fill="none" height="21" stroke="currentColor" stroke-width="1.3" viewBox="0 0 24 24" width="21"><path d="M3 7h18M3 12h18M3 17h18"></path></svg>
          </button>
        </div>
      </div>
    </header>`;

  const header = document.querySelector("header.header");
  const groups = [...header.querySelectorAll(".nav-group")];
  const desktopHover = () => window.matchMedia("(hover: hover) and (pointer: fine)").matches;
  const closeMenus = () => groups.forEach((group) => {
    group.classList.remove("open");
    group.querySelector(".mega-menu")?.classList.remove("open");
    group.querySelector(".nav__trigger")?.classList.remove("active");
  });
  const openMenu = (group) => {
    closeMenus();
    group.classList.add("open");
    group.querySelector(".mega-menu")?.classList.add("open");
    group.querySelector(".nav__trigger")?.classList.add("active");
  };

  groups.forEach((group) => {
    const trigger = group.querySelector(".nav__trigger");
    group.addEventListener("mouseenter", () => {
      if (desktopHover()) openMenu(group);
    });
    trigger?.addEventListener("click", (event) => {
      event.stopPropagation();
      const shouldOpen = !group.classList.contains("open");
      closeMenus();
      if (shouldOpen) openMenu(group);
    });
  });
  header.addEventListener("mouseleave", () => {
    if (desktopHover()) closeMenus();
  });

  const langButton = header.querySelector(".lang__btn");
  const langMenu = header.querySelector(".lang__dd");
  const selectLanguage = (selected) => {
    langMenu?.querySelectorAll("[data-lang]").forEach((item) => {
      const active = item.dataset.lang === selected;
      item.classList.toggle("active", active);
      item.hidden = active;
    });
    const code = header.querySelector(".lang__code");
    const flag = langButton?.querySelector(".lang__flag");
    if (code) code.textContent = selected.toUpperCase();
    if (flag) flag.src = `assets/flags/${selected}.svg`;
  };
  selectLanguage("tr");
  langButton?.addEventListener("click", (event) => {
    event.stopPropagation();
    langMenu?.classList.toggle("open");
  });
  langMenu?.querySelectorAll("[data-lang]").forEach((button) => button.addEventListener("click", (event) => {
    event.stopPropagation();
    selectLanguage(button.dataset.lang);
    langMenu.classList.remove("open");
  }));

  document.addEventListener("click", (event) => {
    if (!event.target.closest(".nav-group")) closeMenus();
    if (!event.target.closest(".lang")) langMenu?.classList.remove("open");
  });
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      closeMenus();
      langMenu?.classList.remove("open");
    }
  });

  // Mevcut bir HTML karşılığı bulunan ortak navigasyon/footer başlıklarını bağla.
  const pageLinks = {
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
    const target = pageLinks[link.textContent.trim()];
    if (target) link.href = target;
  });

  const prepareMobileLinkLabels = () => {
    document.querySelectorAll(".mobile-menu nav a, .side-menu__nav a").forEach((link) => {
      if (link.querySelector(".nav-link__label")) return;
      const textNode = [...link.childNodes].find((node) => node.nodeType === Node.TEXT_NODE && node.textContent.trim());
      if (!textNode) return;
      const label = document.createElement("span");
      label.className = "nav-link__label";
      label.textContent = textNode.textContent.trim();
      link.replaceChild(label, textNode);
    });
  };
  prepareMobileLinkLabels();
  document.querySelectorAll(".footer__col").forEach((column) => {
    if (column.querySelector("h4")?.textContent.trim() !== "Medya") return;
    if ([...column.querySelectorAll("a")].some((link) => link.textContent.trim() === "Blog Yazıları")) return;
    const blogLink = document.createElement("a");
    blogLink.href = "blog.html";
    blogLink.textContent = "Blog Yazıları";
    column.appendChild(blogLink);
  });
  const corporateFooterTargets = {
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
      const target = corporateFooterTargets[link.textContent.trim()];
      if (target) link.href = target;
    });
  });
  document.querySelectorAll(".footer__col").forEach((column) => {
    if (column.querySelector("h4")?.textContent.trim() !== "Katalog ve Belgeler") return;
    column.querySelectorAll("a").forEach((link) => {
      if (link.textContent.trim() === "Sertifikalar") link.href = "hakkimizda.html#sertifikalar";
    });
  });

  const filterExcludedPages = new Set([
    "blog.html",
    "blog-detay.html",
    "haberler.html",
    "haber-detay.html",
    "hakkimizda.html",
    "kariyer.html",
    "projeler.html",
    "proje-detay.html",
    "satis-noktalari.html",
    "bayi-detay.html",
    "teknik-dokumanlar.html",
    "cerez-politikasi.html",
    "kvkk-gizlilik.html"
  ]);
  const currentPage = window.location.pathname.split("/").pop().toLocaleLowerCase("tr-TR");
  if (filterExcludedPages.has(currentPage) && !document.querySelector('link[href="assets/index-typography.css"]')) {
    const typographyStylesheet = document.createElement("link");
    typographyStylesheet.rel = "stylesheet";
    typographyStylesheet.href = "assets/index-typography.css";
    document.head.appendChild(typographyStylesheet);
  }
  if (!filterExcludedPages.has(currentPage) && !document.querySelector('script[src^="assets/index-filter.js"]')) {
    const filterScript = document.createElement("script");
    filterScript.src = "assets/index-filter.js";
    document.body.appendChild(filterScript);
  }
})();
