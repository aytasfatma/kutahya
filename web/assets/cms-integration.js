(function () {
  "use strict";

  const API_ROOT = (window.NG_CMS_API_BASE || "/api/public").replace(/\/$/, "");
  const API_ORIGIN = new URL(API_ROOT, location.href).origin;
  const params = new URLSearchParams(location.search);
  const language = params.get("lang") || document.documentElement.lang || "tr";
  const page = location.pathname.split("/").pop() || "index.html";

  const esc = (value) => String(value == null ? "" : value).replace(/[&<>"']/g, (char) => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
  })[char]);
  const asset = (url) => !url ? "" : (/^(https?:|data:|blob:)/i.test(url) ? url : `${API_ORIGIN}/${url.replace(/^\//, "")}`);
  const link = (url) => {
    const value = String(url || "").trim();
    if (!value) return "";
    if (/^(https?:\/\/|mailto:|tel:|#)/i.test(value)) return value;
    if (/^(www\.|[a-z0-9-]+\.[a-z]{2,})(\/|$)/i.test(value)) return "https://" + value;
    return value.startsWith("/") ? value : "/" + value.replace(/^\.\//, "");
  };
  const categorySlug = (name) => String(name || "").toLocaleLowerCase("tr-TR")
    .replace(/ı/g, "i").replace(/ğ/g, "g").replace(/ü/g, "u").replace(/ş/g, "s").replace(/ö/g, "o").replace(/ç/g, "c")
    .replace(/[^a-z0-9]+/g, "-").replace(/^-+|-+$/g, "");
  const SURFACE_IMAGE_PATH = "assets/AgeçiciGörsel/deneme.webp";
  const COLLECTION_IMAGE_PATH = "assets/AgeçiciGörsel/deneme.webp";
  const PROJECT_IMAGE_PATH = "assets/AgeçiciGörsel/deneme.webp";
  const BLOG_FALLBACK_IMAGE_PATH = "assets/AgeçiciGörsel/images.jpg";
  const localWebAsset = (path) => new URL(path, location.href).href;
  const productCardImage = (item, fallback) => item.primaryImageUrl ? asset(item.primaryImageUrl) : fallback;
  const collectionCardImage = (item) => item.imageUrl ? asset(item.imageUrl) : COLLECTION_IMAGE_PATH;
  const productDetailHref = (item, fallback) => {
    const detailParams = new URLSearchParams({ slug: item.seoUrl, image: productCardImage(item, fallback) });
    return `urun-detay.html?${detailParams.toString()}`;
  };
  const surfaceImageMarkup = (item) => `<img src="${esc(item.imageUrl ? asset(item.imageUrl) : SURFACE_IMAGE_PATH)}" alt="${esc(item.name)} yüzeyi" loading="lazy">`;
  const surfaceImageStyle = document.createElement("style");
  surfaceImageStyle.textContent = ".surface-card__image{overflow:hidden!important}.surface-card__image img{display:block!important;width:100%!important;height:100%!important;object-fit:cover!important;object-position:center!important;background:#f5f5f3!important}.all-collections-empty:not([data-cms-ready=\"true\"]){display:none!important}.cms-search-result-count{display:block;margin-top:7px;color:#77736f;font-size:11px;line-height:1.4;text-align:right}.cms-product-results{display:grid!important;grid-template-columns:repeat(4,minmax(0,1fr))!important;gap:42px 20px!important}.cms-product-results>.cms-product-result-card{display:block!important;min-width:0!important;max-width:none!important;width:100%!important;flex:none!important;color:inherit!important;text-decoration:none!important}.cms-product-results>.cms-product-result-card>.carousel-item__img{width:100%!important;aspect-ratio:1/1!important;margin-bottom:14px!important;overflow:hidden!important}.cms-product-results>.cms-product-result-card>.carousel-item__img img{display:block!important;width:100%!important;height:100%!important;object-fit:cover!important;transition:transform .8s cubic-bezier(.25,.46,.45,.94)!important}.cms-product-results>.cms-product-result-card:hover>.carousel-item__img img{transform:scale(1.05)!important}.cms-product-results>.cms-product-result-card>h3{margin:0 0 5px!important;font-family:\"Noto Sans\",Arial,sans-serif!important;font-size:15px!important;font-weight:400!important;line-height:1.4!important}.cms-product-results>.cms-product-result-card>p{display:block!important;margin:0!important;color:#77736f!important;font-family:\"Noto Sans\",Arial,sans-serif!important;font-size:12px!important;font-weight:300!important;line-height:1.5!important}.cms-home-carousel{display:flex!important;gap:20px!important;overflow-x:auto!important;scroll-behavior:smooth!important;scroll-snap-type:x mandatory!important;scrollbar-width:none!important;transform:none!important}.cms-home-carousel::-webkit-scrollbar{display:none!important}.cms-home-carousel>.carousel-item,.cms-home-carousel>.surface-card{flex:0 0 calc((100% - 80px)/5)!important;max-width:calc((100% - 80px)/5)!important;min-width:0!important;scroll-snap-align:start!important}.cms-home-carousel .carousel-item__img{overflow:hidden!important}.cms-home-carousel .carousel-item__img img,#allCollectionsGrid .carousel-item__img img,#slimCollectionsGrid .carousel-item__img img,#stoneCollections .carousel-item__img img,#performaCollectionsGrid .carousel-item__img img,#stoneListingGrid .carousel-item__img img{display:block!important;width:100%!important;height:100%!important;object-fit:cover!important;object-position:center!important}.cms-home-carousel h3{margin-bottom:5px!important}.cms-home-carousel p,.all-collections-grid>.carousel-item>p,.slim-collections-grid>.carousel-item>p,.performa-collections-grid>.carousel-item>p,.stone-listing-grid>.carousel-item>p,main .listing .grid>.carousel-item>p{display:block!important}@media(max-width:900px){.cms-product-results{grid-template-columns:repeat(3,minmax(0,1fr))!important}.cms-home-carousel>.carousel-item,.cms-home-carousel>.surface-card{flex-basis:calc((100% - 40px)/3)!important;max-width:calc((100% - 40px)/3)!important}}@media(max-width:650px){.cms-product-results{grid-template-columns:repeat(2,minmax(0,1fr))!important}.cms-home-carousel>.carousel-item,.cms-home-carousel>.surface-card{flex-basis:calc((100% - 20px)/2)!important;max-width:calc((100% - 20px)/2)!important}}";
  surfaceImageStyle.textContent += "#kutahyaSurfaces h3,#kutahyaCollections h3,#slimSurfaces h3,#slimCollections h3,#stoneSurfaces h3,#stoneCollections h3,#performaCollections h3,#slimSurfacesGrid>.surface-card>h3,#slimCollectionsGrid>.carousel-item>h3,#stoneListingGrid>.surface-card>h3,#stoneListingGrid>.carousel-item>h3,#performaCollectionsGrid>.carousel-item>h3,#listingGrid>.surface-card>h3,main .listing .grid>.carousel-item>h3{margin:0 0 5px!important;color:#2d2926!important;font-family:'Noto Sans',Arial,sans-serif!important;font-size:15px!important;font-style:normal!important;font-weight:400!important;line-height:1.2!important;letter-spacing:normal!important;text-transform:none!important}#slimSurfacesGrid>.surface-card>p,#slimCollectionsGrid>.carousel-item>p,#stoneListingGrid>.surface-card>p,#stoneListingGrid>.carousel-item>p,#performaCollectionsGrid>.carousel-item>p,#listingGrid>.surface-card>p,main .listing .grid>.carousel-item>p{margin:0!important;color:#77736f!important;font-family:'Noto Sans',Arial,sans-serif!important;font-size:12px!important;font-style:normal!important;font-weight:300!important;line-height:1.5!important;letter-spacing:normal!important;text-transform:none!important}";
  surfaceImageStyle.textContent += "#kutahyaSurfaces .surface-card__image,#slimSurfaces .surface-card__image,#stoneSurfaces .surface-card__image,#listingGrid .surface-card__image,#slimSurfacesGrid .surface-card__image,#stoneListingGrid .surface-card__image{display:block!important;width:100%!important;height:auto!important;aspect-ratio:1/1!important;overflow:hidden!important;flex:none!important}#kutahyaSurfaces .surface-card__image img,#slimSurfaces .surface-card__image img,#stoneSurfaces .surface-card__image img,#listingGrid .surface-card__image img,#slimSurfacesGrid .surface-card__image img,#stoneListingGrid .surface-card__image img{display:block!important;width:100%!important;height:100%!important;min-width:100%!important;min-height:100%!important;aspect-ratio:1/1!important;object-fit:cover!important;object-position:center!important}";
  surfaceImageStyle.textContent += ".cms-home-carousel .surface-card__image,.cms-home-carousel .carousel-item__img,#allCollectionsGrid .carousel-item__img,#slimCollectionsGrid .carousel-item__img,#performaCollectionsGrid .carousel-item__img,#stoneListingGrid .carousel-item__img,#stoneListingGrid .surface-card__image,#listingGrid .surface-card__image,#slimSurfacesGrid .surface-card__image{display:block!important;width:100%!important;height:auto!important;aspect-ratio:1/1!important;overflow:hidden!important;flex:none!important}.cms-home-carousel .surface-card__image img,.cms-home-carousel .carousel-item__img img,#allCollectionsGrid .carousel-item__img img,#slimCollectionsGrid .carousel-item__img img,#performaCollectionsGrid .carousel-item__img img,#stoneListingGrid .carousel-item__img img,#stoneListingGrid .surface-card__image img,#listingGrid .surface-card__image img,#slimSurfacesGrid .surface-card__image img{display:block!important;width:100%!important;height:100%!important;min-width:100%!important;min-height:100%!important;aspect-ratio:1/1!important;object-fit:cover!important;object-position:center!important}";
  surfaceImageStyle.textContent += "main .listing .grid .carousel-item__img{display:block!important;width:100%!important;height:auto!important;aspect-ratio:1/1!important;overflow:hidden!important;flex:none!important}main .listing .grid .carousel-item__img img{display:block!important;width:100%!important;height:100%!important;min-width:100%!important;min-height:100%!important;aspect-ratio:1/1!important;object-fit:cover!important;object-position:center!important}";
  surfaceImageStyle.textContent += ".cms-card-skeleton{flex:0 0 calc((100% - 80px)/5);aspect-ratio:1/1;background:linear-gradient(90deg,#ebe7e1 25%,#f7f4ef 50%,#ebe7e1 75%);background-size:200% 100%;animation:cms-skeleton 1.1s infinite linear}.cms-list-skeleton{display:grid!important;grid-template-columns:repeat(4,minmax(0,1fr));gap:42px 20px}.cms-list-skeleton .cms-card-skeleton{width:100%;flex:none}@keyframes cms-skeleton{to{background-position:-200% 0}}@media(max-width:900px){.cms-card-skeleton{flex-basis:calc((100% - 40px)/3)}.cms-list-skeleton{grid-template-columns:repeat(3,minmax(0,1fr))}}@media(max-width:650px){.cms-card-skeleton{flex-basis:calc((100% - 20px)/2)}.cms-list-skeleton{grid-template-columns:repeat(2,minmax(0,1fr))}}";
  document.head.appendChild(surfaceImageStyle);
  const noPaginationStyle = document.createElement("style");
  noPaginationStyle.textContent = ".cms-api-pagination[hidden]{display:none!important}.cms-api-pagination{display:flex!important;align-items:center;justify-content:center;gap:10px;margin-top:44px}.cms-api-pagination [data-page]{min-width:38px;height:38px;border:1px solid #d8d3cd;background:#fff;color:#2d2926;cursor:pointer}.cms-api-pagination [data-page].active{background:#2d2926;color:#fff;border-color:#2d2926}.cms-api-pagination>button{min-width:42px;height:38px;border:1px solid #d8d3cd;background:#fff;cursor:pointer}.cms-api-pagination>button:disabled{opacity:.35;cursor:default}.cms-surface-search-wrap{flex:0 0 350px;width:350px}.cms-surface-search-wrap .surface-inline-search{width:100%!important;margin-bottom:0!important}@media(max-width:820px){.cms-surface-search-wrap{width:100%;flex:auto}}";
  noPaginationStyle.textContent += ".cms-product-results .cms-product-name{display:block!important;margin:0 0 5px!important;color:#2d2926!important;font-family:'Noto Sans',Arial,sans-serif!important;font-size:15px!important;font-style:normal!important;font-weight:400!important;line-height:1.4!important;text-transform:none!important;letter-spacing:normal!important}.cms-product-results .cms-product-code{display:block!important;margin:0!important;color:#77736f!important;font-family:'Noto Sans',Arial,sans-serif!important;font-size:12px!important;font-style:normal!important;font-weight:300!important;line-height:1.5!important;text-transform:none!important;letter-spacing:normal!important}";
  document.head.appendChild(noPaginationStyle);
  const slug = () => params.get("slug") || params.get("id");
  const date = (value) => value ? new Intl.DateTimeFormat(language, { day: "numeric", month: "long", year: "numeric" }).format(new Date(value)) : "";
  const query = (values) => {
    const result = new URLSearchParams();
    Object.entries(values || {}).forEach(([key, value]) => value !== "" && value != null && result.set(key, value));
    return result.toString();
  };
  async function get(path, values) {
    const suffix = query({ lang: language, ...values });
    const response = await fetch(`${API_ROOT}/${path}${suffix ? `?${suffix}` : ""}`, { headers: { Accept: "application/json" } });
    if (!response.ok) throw new Error(`${response.status} ${response.statusText}`);
    return response.json();
  }
  async function post(path, body) {
    const response = await fetch(`${API_ROOT}/${path}`, { method: "POST", headers: { Accept: "application/json", "Content-Type": "application/json" }, body: JSON.stringify(body) });
    const payload = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(payload.detail || payload.title || "Form gönderilemedi.");
    return payload;
  }
  const items = (data) => Array.isArray(data) ? data : (data && Array.isArray(data.items) ? data.items : []);
  const setText = (selector, value) => { const node = document.querySelector(selector); if (node && value != null && value !== "") node.textContent = value; };
  const showCardSkeletons = (container, count = 5, list = false) => {
    if (!container || container.children.length) return;
    if (list) container.classList.add("cms-list-skeleton");
    container.innerHTML = Array.from({ length: count }, () => '<span class="cms-card-skeleton" aria-hidden="true"></span>').join("");
    container.setAttribute("aria-busy", "true");
  };
  const clearCardSkeletons = (container) => { container?.classList.remove("cms-list-skeleton"); container?.removeAttribute("aria-busy"); };
  const PRODUCT_PAGE_SIZE = 20;
  const productCardMarkup = (x, fallback, index = 0) => `<a class="carousel-item cms-product-result-card" href="${esc(productDetailHref(x, fallback))}" data-name="${esc(x.name)}"><div class="carousel-item__img"><img loading="${index < 8 ? "eager" : "lazy"}"${index < 8 ? ' fetchpriority="high"' : ""} src="${esc(productCardImage(x, fallback))}" alt="${esc(x.name)}"></div><h3 class="cms-product-name">${esc(x.name)}</h3><p class="cms-product-code">${esc(x.productCode)}</p></a>`;
  const productPaginationParts = (grid) => {
    const names = grid.id === "allCollectionsGrid"
      ? ["allCollectionsPagination", "allCollectionsPageNumbers", "allCollectionsPrev", "allCollectionsNext"]
      : grid.id === "slimCollectionsGrid"
        ? ["slimCollectionsPagination", "slimCollectionsPageNumbers", "slimCollectionsPrev", "slimCollectionsNext"]
        : grid.id === "performaCollectionsGrid"
          ? ["performaCollectionsPagination", "performaCollectionsPageNumbers", "performaCollectionsPrev", "performaCollectionsNext"]
          : grid.id === "slimSurfacesGrid"
            ? ["slimSurfacesPagination", "slimSurfacesPageNumbers", "slimSurfacesPrev", "slimSurfacesNext"]
            : grid.id === "stoneListingGrid"
              ? ["stoneListingPagination", "stoneListingPageNumbers", "stoneListingPrev", "stoneListingNext"]
              : ["surfacePagination", "surfacePageNumbers", "surfacePrev", "surfaceNext"];
    let root = document.getElementById(names[0]);
    if (!root) {
      root = document.createElement("nav");
      root.id = names[0]; root.setAttribute("aria-label", "Sayfalama");
      root.innerHTML = `<button type="button" id="${names[2]}" aria-label="Önceki sayfa">←</button><div id="${names[1]}"></div><button type="button" id="${names[3]}" aria-label="Sonraki sayfa">→</button>`;
      grid.insertAdjacentElement("afterend", root);
    }
    root.classList.add("cms-api-pagination");
    return { root, numbers: document.getElementById(names[1]), prev: document.getElementById(names[2]), next: document.getElementById(names[3]) };
  };
  async function setupProductPagination({ grid, filters, fallback, searchSelector, empty, count }) {
    showCardSkeletons(grid, 8, true);
    const pager = productPaginationParts(grid);
    const search = document.querySelector(searchSelector);
    let currentPage = 1;
    let searchTimer;
    const renderPager = (data) => {
      const totalPages = Number(data.totalPages || 0);
      pager.root.hidden = totalPages <= 1;
      pager.prev.disabled = currentPage <= 1;
      pager.next.disabled = currentPage >= totalPages;
      if (pager.numbers) {
        const start = Math.max(1, Math.min(currentPage - 2, totalPages - 4));
        const end = Math.min(totalPages, start + 4);
        pager.numbers.innerHTML = Array.from({ length: Math.max(0, end - start + 1) }, (_, i) => start + i).map((pageNumber) => `<button type="button" data-page="${pageNumber}" class="${pageNumber === currentPage ? "active" : ""}">${pageNumber}</button>`).join("");
      }
    };
    const load = async (scroll = false) => {
      grid.setAttribute("aria-busy", "true");
      const response = await get("products", { ...filters, q: search?.value.trim(), page: currentPage, pageSize: PRODUCT_PAGE_SIZE });
      const products = items(response);
      clearCardSkeletons(grid);
      grid.classList.add("cms-product-results");
      grid.innerHTML = products.map((x, index) => productCardMarkup(x, fallback, index)).join("");
      grid.removeAttribute("aria-busy");
      if (empty) { empty.textContent = "Bu seçime ait aktif ürün bulunamadı."; empty.hidden = products.length !== 0; empty.classList.toggle("is-visible", products.length === 0); empty.classList.toggle("show", products.length === 0); }
      if (count) count.textContent = `${response.totalCount || 0} ürün`;
      renderPager(response);
      if (scroll) grid.scrollIntoView({ behavior: "smooth", block: "start" });
    };
    pager.numbers?.addEventListener("click", (event) => { const button = event.target.closest("[data-page]"); if (!button) return; currentPage = Number(button.dataset.page); load(true); });
    pager.prev?.addEventListener("click", () => { if (currentPage > 1) { currentPage--; load(true); } });
    pager.next?.addEventListener("click", () => { currentPage++; load(true); });
    search?.addEventListener("input", () => { clearTimeout(searchTimer); searchTimer = setTimeout(() => { currentPage = 1; load(); }, 300); });
    await load();
  }

  function setupHomeCarousel(track, kind) {
    if (!track) return;
    track.classList.add("cms-home-carousel");
    track.style.removeProperty("display");
    track.style.removeProperty("grid-template-columns");
    track.style.removeProperty("gap");
    track.style.transform = "none";
    const root = track.parentElement;
    const previous = root?.querySelector(kind === "surface" ? ".surfaces-arrow--prev" : ".collections-arrow--prev, .collections-mini-arrow--prev, .carousel-mini-arrow--prev");
    const next = root?.querySelector(kind === "surface" ? ".surfaces-arrow--next" : ".collections-arrow--next, .collections-mini-arrow--next, .carousel-mini-arrow--next");
    const step = () => Math.round((track.firstElementChild?.getBoundingClientRect().width || 0) + 20);
    const update = () => {
      if (!previous || !next) return;
      previous.disabled = track.scrollLeft <= 1;
      next.disabled = track.scrollLeft + track.clientWidth >= track.scrollWidth - 1;
    };
    const bind = (button, direction) => {
      if (!button || button.dataset.cmsCarouselBound === "true") return;
      button.dataset.cmsCarouselBound = "true";
      button.disabled = false;
      button.addEventListener("click", (event) => {
        event.preventDefault();
        event.stopImmediatePropagation();
        track.scrollBy({ left: direction * step(), behavior: "smooth" });
        window.setTimeout(update, 350);
      }, true);
    };
    bind(previous, -1);
    bind(next, 1);
    track.addEventListener("scroll", update, { passive: true });
    window.addEventListener("resize", update);
    requestAnimationFrame(update);
  }

  function setupNameSearch(grid, selector) {
    const search = document.querySelector(selector);
    if (!grid || !search || search.dataset.cmsNameSearchBound === "true") return;
    search.dataset.cmsNameSearchBound = "true";
    const normalize = (value) => String(value || "").trim().toLocaleLowerCase("tr-TR");
    const productMode = Boolean(params.get("collectionId") || params.get("categoryId") || params.get("surface"));
    const itemLabel = productMode ? "ürün" : "koleksiyon";
    search.placeholder = productMode ? "Ürün ara" : "Koleksiyon ara";
    search.setAttribute("aria-label", productMode ? "Ürün ara" : "Koleksiyon ara");
    if (search.matches(".surface-inline-search") && !search.parentElement?.classList.contains("cms-surface-search-wrap")) {
      const wrapper = document.createElement("div");
      wrapper.className = "cms-surface-search-wrap";
      search.insertAdjacentElement("beforebegin", wrapper);
      wrapper.appendChild(search);
    }
    let resultCount = search.parentElement?.querySelector(".cms-search-result-count");
    if (!resultCount) {
      resultCount = document.createElement("span");
      resultCount.className = "cms-search-result-count";
      resultCount.setAttribute("aria-live", "polite");
      search.insertAdjacentElement("afterend", resultCount);
    }
    const filter = () => {
      const term = normalize(search.value);
      let visible = 0;
      [...grid.children].forEach((card) => {
        const show = !term || normalize(card.dataset.name || card.querySelector("h2,h3")?.textContent).includes(term);
        card.hidden = !show;
        if (show) card.style.removeProperty("display");
        else card.style.setProperty("display", "none", "important");
        if (show) visible++;
      });
      const count = document.querySelector("#resultCount, .result-count, .count");
      if (count) count.textContent = `${visible} sonuç gösteriliyor`;
      resultCount.textContent = `${visible} ${itemLabel}`;
      if (!productMode) {
        document.querySelectorAll(".all-collections-filters [data-collection-id], .unified-collections-filters [data-collection-id], .slim-collections-filters [data-collection-id], .stone-listing-filters [data-collection-id], .performa-collections-filters [data-collection-id]").forEach((button) => {
          const show = !term || normalize(button.textContent).includes(term);
          button.hidden = !show;
          if (show) button.style.removeProperty("display");
          else button.style.setProperty("display", "none", "important");
        });
      }
      const empty = document.getElementById("allCollectionsEmpty");
      if (empty) {
        empty.dataset.cmsReady = "true";
        empty.hidden = visible !== 0;
        empty.textContent = productMode ? "Aramanızla eşleşen ürün bulunamadı." : "Aramanızla eşleşen koleksiyon bulunamadı.";
      }
      const brandEmpty = document.getElementById("empty");
      if (brandEmpty) { brandEmpty.textContent = productMode ? "Aramanızla eşleşen ürün bulunamadı." : "Aramanızla eşleşen koleksiyon bulunamadı."; brandEmpty.classList.toggle("show", visible === 0); }
      const surfaceEmpty = document.getElementById("emptyState");
      if (surfaceEmpty) { surfaceEmpty.textContent = productMode ? "Aramanızla eşleşen ürün bulunamadı." : "Aramanızla eşleşen yüzey bulunamadı."; surfaceEmpty.classList.toggle("is-visible", visible === 0); }
    };
    search.addEventListener("input", (event) => {
      event.stopImmediatePropagation();
      filter();
    }, true);
    search.addEventListener("search", filter);
    filter();
  }

  function populateCollectionFilters(data, selectedId) {
    const filterRoot = document.querySelector(".all-collections-filters");
    const sample = filterRoot?.querySelector("[data-filter]");
    if (!filterRoot || !sample) return;
    const scrollStorageKey = "ng-index-collection-filter-scroll";
    const selectionStorageKey = "ng-index-collection-filter-selection";
    const filterClass = sample.className.replace(/\s*active\b/g, "");
    const allSelected = !selectedId;
    filterRoot.innerHTML = `<button class="${esc(filterClass)}${allSelected ? " active" : ""}" type="button" data-filter="all" aria-pressed="${allSelected}">Tümü</button>` + data.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((x) => {
      const selected = String(x.id) === String(selectedId || "");
      return `<button class="${esc(filterClass)}${selected ? " active" : ""}" type="button" data-filter="${esc(x.name)}" data-collection-id="${x.id}" aria-pressed="${selected}">${esc(x.name)}</button>`;
    }).join("");
    requestAnimationFrame(() => {
      const savedPosition = Number(sessionStorage.getItem(scrollStorageKey));
      const savedSelection = sessionStorage.getItem(selectionStorageKey);
      const selectedButton = selectedId ? filterRoot.querySelector(`[data-collection-id="${CSS.escape(String(selectedId))}"]`) : null;
      if (selectedId && savedSelection !== String(selectedId) && selectedButton) {
        selectedButton.scrollIntoView({ behavior: "auto", block: "nearest", inline: "center" });
        sessionStorage.setItem(scrollStorageKey, String(filterRoot.scrollLeft));
      } else if (Number.isFinite(savedPosition)) {
        filterRoot.scrollLeft = savedPosition;
      }
    });
    filterRoot.querySelector('[data-filter="all"]')?.addEventListener("click", () => {
      sessionStorage.setItem(scrollStorageKey, String(filterRoot.scrollLeft));
      sessionStorage.setItem(selectionStorageKey, "");
      location.href = "index-koleksiyonlar.html";
    });
    filterRoot.querySelectorAll("[data-collection-id]").forEach((button) => button.addEventListener("click", () => {
      sessionStorage.setItem(scrollStorageKey, String(filterRoot.scrollLeft));
      sessionStorage.setItem(selectionStorageKey, String(button.dataset.collectionId));
      location.href = `index-koleksiyonlar.html?collectionId=${button.dataset.collectionId}`;
    }));
  }
  const setMeta = (data) => {
    if (data.metaTitle) document.title = data.metaTitle;
    const description = document.querySelector('meta[name="description"]');
    if (description && data.metaDescription) description.content = data.metaDescription;
    const canonical = document.querySelector('link[rel="canonical"]');
    if (canonical && data.seoUrl) canonical.href = `${location.origin}/${language}/${data.seoUrl.replace(/^\//, "")}`;
    const values = { 'meta[property="og:title"]': data.metaTitle || data.title || data.name, 'meta[property="og:description"]': data.metaDescription, 'meta[property="og:image"]': asset(data.featuredImageUrl || data.primaryImageUrl) };
    Object.entries(values).forEach(([selector, value]) => { const node = document.querySelector(selector); if (node && value) node.content = value; });
  };

  function applyTypesOnlyLayout() {
    const isSurfacePage = /-yuzeyler\.html$/i.test(page);
    const isCollectionPage = /-koleksiyonlar\.html$/i.test(page) || page === "index-koleksiyonlar.html";
    if (!isSurfacePage && !isCollectionPage) return;
    if (isSurfacePage) return;
    if (isCollectionPage) return;
    if (isCollectionPage && (params.get("collectionId") || params.get("categoryId") || params.get("surface"))) return;

    const selectors = isSurfacePage
      ? ["#listingGrid", "#slimSurfacesGrid", "#stoneListingGrid"]
      : ["#allCollectionsGrid", "#slimCollectionsGrid", "#performaCollectionsGrid", "#stoneListingGrid", "main .listing .grid"];
    selectors.forEach((selector) => document.querySelectorAll(selector).forEach((node) => {
      node.style.removeProperty("display");
      node.classList.add("cms-type-name-grid");
    }));
    if (!document.getElementById("cms-type-name-grid-style")) {
      const style = document.createElement("style");
      style.id = "cms-type-name-grid-style";
      style.textContent = ".cms-type-name-grid{display:grid!important;grid-template-columns:repeat(5,minmax(0,1fr))!important;gap:28px 24px!important}.cms-type-name-card{display:block!important;min-width:0!important;color:inherit!important;text-decoration:none!important}.cms-type-name-card h3{margin:0!important;font-size:17px!important;font-weight:400!important;line-height:1.4!important}.cms-type-name-card p,.cms-type-name-card img,.cms-type-name-card .carousel-item__img{display:none!important}@media(max-width:900px){.cms-type-name-grid{grid-template-columns:repeat(3,minmax(0,1fr))!important}}@media(max-width:560px){.cms-type-name-grid{grid-template-columns:repeat(2,minmax(0,1fr))!important}}";
      document.head.appendChild(style);
    }
    document.querySelectorAll(".surface-pagination, .slim-surfaces-pagination, .stone-listing-pagination, .all-collections-pagination, .slim-collections-pagination, .performa-collections-pagination, .unified-listing-pagination, .empty, .all-collections-empty, .slim-surfaces-empty, .stone-surfaces-empty, .slim-collections-empty, .performa-collections-empty, .unified-collections-empty, .result-count, .count").forEach((node) => { node.style.display = "none"; });
  }

  async function collections() {
    const grid = document.getElementById("allCollectionsGrid") || document.getElementById("carousel");
    if (!grid) return;
    const collectionLoading = document.getElementById("allCollectionsLoading");
    if (collectionLoading) collectionLoading.hidden = false;
    const collectionEmpty = document.getElementById("allCollectionsEmpty");
    if (collectionEmpty) { collectionEmpty.dataset.cmsReady = "false"; collectionEmpty.hidden = true; }
    if (page === "index.html") {
      const homeData = await get("collections");
      grid.innerHTML = homeData.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((x) => `<a class="carousel-item" href="index-koleksiyonlar.html?collectionId=${x.id}"><div class="carousel-item__img"><img src="${esc(collectionCardImage(x))}" alt="${esc(x.name)} koleksiyonu"></div><h3>${esc(x.name)}</h3></a>`).join("");
      setupHomeCarousel(grid, "collection");
      if (collectionLoading) collectionLoading.hidden = true;
      return;
    }
    const collectionId = params.get("collectionId"); const categoryId = params.get("categoryId");
    const selectedSurface = params.get("surface"); const selectedBrand = params.get("brand");
    if (collectionId || categoryId || selectedSurface) {
      const collectionData = await get("collections");
      populateCollectionFilters(collectionData, collectionId);
      if (collectionLoading) collectionLoading.hidden = true;
      grid.style.removeProperty("display");
      const products = [true]; // Eski boş-durum kontrolünü API sayfalayıcısı yönetir.
      if (!products.length) { grid.innerHTML = ""; const empty = document.getElementById("allCollectionsEmpty"); if (empty) { empty.dataset.cmsReady = "true"; empty.hidden = false; empty.textContent = "Bu seçime ait aktif ürün bulunamadı."; } return; }
      const empty = document.getElementById("allCollectionsEmpty");
      if (empty) empty.dataset.cmsReady = "true";
      await setupProductPagination({ grid, filters: { collectionId, categoryId, surface: selectedSurface, brand: selectedBrand }, fallback: COLLECTION_IMAGE_PATH, searchSelector: "#collectionSearch", empty });
      return;
    }
    const data = await get("collections");
    if (collectionLoading) collectionLoading.hidden = true;
    if (!data.length) { if (collectionEmpty) { collectionEmpty.dataset.cmsReady = "true"; collectionEmpty.hidden = false; } return; }
    if (page === "index-koleksiyonlar.html") {
      populateCollectionFilters(data, null);
      grid.innerHTML = data.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((x) => `<a class="carousel-item all-collections-card" href="index-koleksiyonlar.html?collectionId=${x.id}" data-name="${esc(x.name)}"><div class="carousel-item__img"><img src="${esc(collectionCardImage(x))}" alt="${esc(x.name)} koleksiyonu"></div><h3>${esc(x.name)}</h3><p>${x.productCount} ürün</p></a>`).join("");
      setupNameSearch(grid, "#collectionSearch");
      return;
    }
    grid.innerHTML = data.map((x) => `<a class="carousel-item${grid.id === "allCollectionsGrid" ? " all-collections-card" : ""}" href="index-koleksiyonlar.html?collectionId=${x.id}" data-name="${esc(x.name)}" data-description="${esc(x.description)}"><div class="carousel-item__img"><img alt="${esc(x.name)} koleksiyon" src="${esc(collectionCardImage(x))}"></div><h3>${esc(x.name)}</h3><p>${x.productCount} ürün</p></a>`).join("");
    document.getElementById("collectionSearch")?.dispatchEvent(new Event("input"));
  }

  async function home() {
    const bannerJob = get("banners").then((banners) => {
      const banner = banners[0];
      if (!banner) return;

      const hero = document.querySelector(".hero");
      const slide = hero?.querySelector(".hero__slide--image");
      if (!hero || !slide) return;

      const image = slide.querySelector("img");
      if (image && banner.imageUrl) {
        image.src = asset(banner.imageUrl);
        image.alt = banner.title || "NG Kütahya Seramik";
      }

      setText(".hero__slide--image h1", banner.title);
      setText(".hero__slide--image .hero__desc", banner.subtitle);

      const button = slide.querySelector("a");
      if (button) {
        button.hidden = !banner.buttonUrl;
        if (banner.buttonUrl) button.href = link(banner.buttonUrl);
        if (banner.buttonText) button.textContent = banner.buttonText;
      }

      // İlk video/statik hero korunur; CMS bannerı ikinci slaytı dinamik besler.
    });

    await Promise.allSettled([bannerJob, collections()]);
  }

  async function projects() {
    const grid = document.querySelector("[data-projects-grid]");
    if (!grid || page === "proje-detay.html") return;
    const [managedProjects, filterOptions] = await Promise.all([
      get("projects").catch(() => []),
      get("project-filter-options").catch(() => ({ regions: [], brands: [], projectTypes: [] }))
    ]);
    const data = managedProjects.slice().sort((a, b) => {
      const yearDifference = (b.year ?? Number.NEGATIVE_INFINITY) - (a.year ?? Number.NEGATIVE_INFINITY);
      return yearDifference || (a.displayOrder ?? 0) - (b.displayOrder ?? 0);
    });
    if (!data.length) {
      grid.innerHTML = '<p class="cms-empty-message">Henüz yayınlanmış referans proje bulunmuyor.</p>';
      return;
    }
    const normalize = (value) => String(value || "").trim().toLocaleLowerCase("tr-TR");
    const fillSelect = (selector, placeholder, options) => {
      const select = document.querySelector(selector); if (!select) return;
      select.innerHTML = `<option value="">${esc(placeholder)}</option>` + options.map((option) => `<option value="${esc(normalize(option.value))}">${esc(option.label)}</option>`).join("");
    };
    fillSelect("#projectRegion", "Bölge seçin", filterOptions.regions || []);
    fillSelect("#projectBrand", "Marka seçin", filterOptions.brands || []);
    fillSelect("#projectReferenceType", "Referans tipi seçin", filterOptions.projectTypes || []);
    grid.innerHTML = data.map((x) => `<a class="project-card" data-region="${esc(normalize(x.region))}" data-brand="${esc(normalize(x.brand))}" data-reference-type="${esc(normalize(x.projectType))}" href="proje-detay.html?slug=${encodeURIComponent(x.seoUrl || x.id)}"><div class="project-card__img"><img alt="${esc(x.name)}" src="${esc(x.featuredImageUrl ? asset(x.featuredImageUrl) : PROJECT_IMAGE_PATH)}"></div><h3>${esc(x.name)}</h3><p>${esc([x.projectTypeLabel, x.location, x.year].filter(Boolean).join(" · "))}</p></a>`).join("");
    const selects = [...document.querySelectorAll("[data-project-filter-select]")];
    const resetButton = document.getElementById("projectFilterReset");
    const filter = () => {
      const region = normalize(document.querySelector("#projectRegion")?.value);
      const brand = normalize(document.querySelector("#projectBrand")?.value);
      const referenceType = normalize(document.querySelector("#projectReferenceType")?.value);
      grid.querySelectorAll(".project-card").forEach((card) => {
        card.hidden = Boolean((region && card.dataset.region !== region) || (brand && card.dataset.brand !== brand) || (referenceType && card.dataset.referenceType !== referenceType));
      });
      if (resetButton) resetButton.disabled = !region && !brand && !referenceType;
    };
    selects.forEach((select) => select.addEventListener("change", filter));
    resetButton?.addEventListener("click", () => {
      selects.forEach((select) => { select.value = ""; });
      filter();
    });
    filter();
  }

  async function projectDetail() {
    if (page !== "proje-detay.html") return;
    const main = document.querySelector("main");
    try {
    if (!slug()) return;
    const x = await get(`projects/${encodeURIComponent(slug())}`);
    setMeta(x); setText(".page-header h1", x.name); setText(".breadcrumb__current [itemprop=name]", x.name);
    const body = document.querySelector(".article-body");
    if (body) body.innerHTML = x.description ? `<p>${esc(x.description).replace(/\r?\n/g, "<br>")}</p>` : "";
    const projectMeta = document.querySelector(".project-meta");
    if (projectMeta) projectMeta.innerHTML = [
      ["Proje Tipi", x.projectTypeLabel], ["Lokasyon", x.location], ["Yıl", x.year], ["Uygulama Alanı", x.architect]
    ].map(([label, value]) => `<div class="project-meta__item"><h3>${esc(label)}</h3><p>${esc(value || "-")}</p></div>`).join("");
    const gallery = document.querySelector(".project-gallery");
    if (gallery) {
      const imageUrls = (x.images || []).map((item) => item.url)
        .filter(Boolean)
        .map(asset)
        .filter((url, index, all) => all.indexOf(url) === index)
        .slice(0, 3);
      if (!imageUrls.length) imageUrls.push(PROJECT_IMAGE_PATH);
      gallery.innerHTML = imageUrls.map((url, index) => `<img src="${esc(url)}" alt="${esc(x.name)} referans görseli ${index + 1}" loading="${index ? "lazy" : "eager"}">`).join("");
    }
    } finally { if (main) main.hidden = false; }
  }

  function blogCard(x) {
    const categories = [categorySlug(x.categoryName || "Genel"), x.isTrend ? "trendler" : ""].filter(Boolean).join(" ");
    return `<article class="blog-card" data-category="${esc(categories)}"><a class="blog-card__link" href="blog-detay.html?slug=${encodeURIComponent(x.seoUrl || x.id)}"><div class="blog-card__media"><img loading="lazy" src="${esc(asset(x.featuredImageUrl))}" alt="${esc(x.title)}"></div><div class="blog-card__body"><span class="blog-category">${esc(x.categoryName || "Genel")}</span><h3>${esc(x.title)}</h3><p class="blog-card__excerpt">${esc(x.excerpt)}</p><div class="blog-meta"><time datetime="${esc(x.publishDate)}">${esc(date(x.publishDate))}</time><span>${esc(x.author)}</span></div></div></a></article>`;
  }
  async function blogs() {
    const grid = document.querySelector(".blog-grid");
    if (!grid || page === "blog-detay.html") return;
    const empty = document.querySelector(".blog-empty");
    const showEmpty = () => { grid.replaceChildren(); if (empty) { empty.textContent = "Henüz yayınlanmış blog yazısı bulunmuyor"; empty.hidden = false; } };
    let data;
    try { data = items(await get("blogs", { page: 1, pageSize: 100 })); } catch { showEmpty(); return; }
    if (data.length) { grid.innerHTML = data.map(blogCard).join(""); if (empty) empty.hidden = true; } else showEmpty();
  }
  async function blogDetail() {
    if (page !== "blog-detay.html") return;
    const main = document.querySelector("main");
    try {
    if (!slug()) throw new Error("Blog adresi bulunamadı.");
    const x = await get(`blogs/${encodeURIComponent(slug())}`);
    const canonicalUrl = `${location.origin}/blog-detay.html?slug=${encodeURIComponent(x.seoUrl || x.id)}`;
    const coverUrl = x.featuredImageUrl ? asset(x.featuredImageUrl) : "";
    const secondaryImageUrl = x.secondaryImageUrl ? asset(x.secondaryImageUrl) : coverUrl;
    const metaTitle = x.metaTitle || x.title;
    const metaDescription = x.metaDescription || x.excerpt || "";
    document.title = `${metaTitle} | NG Kütahya Seramik`;
    const metaValues = {
      'meta[name="description"]': metaDescription,
      'meta[property="og:title"]': metaTitle,
      'meta[property="og:description"]': metaDescription,
      'meta[property="og:image"]': coverUrl || secondaryImageUrl,
      'meta[property="article:published_time"]': x.publishDate || "",
      'meta[property="article:author"]': x.author || ""
    };
    Object.entries(metaValues).forEach(([selector, value]) => { const node = document.querySelector(selector); if (node) node.content = value; });
    const canonical = document.querySelector('link[rel="canonical"]'); if (canonical) canonical.href = canonicalUrl;

    setText(".article-header h1", x.title);
    setText(".article-header__spot", x.excerpt || "");
    setText(".article-header .blog-category", [x.isTrend ? "Trend" : "", x.categoryName].filter(Boolean).join(" · "));
    setText("#blogBreadcrumbTitle", x.title);
    setText("[data-blog-author]", x.author || "NG Kütahya Seramik");
    const time = document.querySelector(".article-header time"); if (time) { time.dateTime = x.publishDate || ""; time.textContent = date(x.publishDate); }
    const coverFigure = document.querySelector(".article-cover");
    const cover = coverFigure?.querySelector("img");
    if (coverFigure && cover) { coverFigure.hidden = !coverUrl; if (coverUrl) { cover.src = coverUrl; cover.alt = x.title; } }
    const secondaryFigure = document.querySelector(".article-secondary-media");
    const secondaryImage = secondaryFigure?.querySelector("img");
    if (secondaryFigure && secondaryImage) { secondaryFigure.hidden = !secondaryImageUrl; if (secondaryImageUrl) { secondaryImage.src = secondaryImageUrl; secondaryImage.alt = `${x.title} ikinci görseli`; } }
    const body = document.querySelector(".article-content"); if (body) body.innerHTML = x.content || "<p>Bu blog yazısının içeriği henüz eklenmedi.</p>";
    const words = (body?.textContent || "").trim().split(/\s+/).filter(Boolean).length;
    setText("[data-blog-reading-time]", `${Math.max(1, Math.ceil(words / 200))} dakika okuma`);

    const tags = document.querySelector(".article-tags");
    if (tags) { tags.innerHTML = (x.tags || []).map((tag) => `<a class="article-tag" href="blog.html?tag=${encodeURIComponent(tag)}">${esc(tag)}</a>`).join(""); tags.hidden = !x.tags?.length; }
    const relatedSection = document.getElementById("relatedBlogSection");
    const relatedGrid = document.getElementById("relatedBlogGrid");
    if (relatedSection && relatedGrid) {
      relatedGrid.innerHTML = (x.relatedPosts || []).map((item) => `<article class="blog-card"><a class="blog-card__link" href="blog-detay.html?slug=${encodeURIComponent(item.seoUrl || item.id)}"><div class="blog-card__media"><img loading="lazy" src="${esc(item.featuredImageUrl ? asset(item.featuredImageUrl) : localWebAsset(BLOG_FALLBACK_IMAGE_PATH))}" alt="${esc(item.title)}"></div><div class="blog-card__body"><h3>${esc(item.title)}</h3></div></a></article>`).join("");
      relatedSection.hidden = !x.relatedPosts?.length;
    }

    const breadcrumbSchema = document.getElementById("blog-breadcrumb-schema");
    if (breadcrumbSchema) breadcrumbSchema.textContent = JSON.stringify({ "@context": "https://schema.org", "@type": "BreadcrumbList", itemListElement: [
      { "@type": "ListItem", position: 1, name: "Ana Sayfa", item: `${location.origin}/index.html` },
      { "@type": "ListItem", position: 2, name: "Blog Yazıları", item: `${location.origin}/blog.html` },
      { "@type": "ListItem", position: 3, name: x.title, item: canonicalUrl }
    ] });
    const articleSchema = document.getElementById("blog-article-schema");
    if (articleSchema) articleSchema.textContent = JSON.stringify({ "@context": "https://schema.org", "@type": "Article", headline: x.title, description: metaDescription, image: [coverUrl || secondaryImageUrl], author: { "@type": "Organization", name: x.author || "NG Kütahya Seramik" }, publisher: { "@type": "Organization", name: "NG Kütahya Seramik", logo: { "@type": "ImageObject", url: `${location.origin}/assets/ng-kutahya-logo.png` } }, datePublished: x.publishDate, mainEntityOfPage: { "@type": "WebPage", "@id": canonicalUrl } });
    document.querySelectorAll("[data-share]").forEach((shareLink) => { const url = encodeURIComponent(canonicalUrl); const title = encodeURIComponent(x.title); const type = shareLink.dataset.share; shareLink.href = type === "linkedin" ? `https://www.linkedin.com/sharing/share-offsite/?url=${url}` : type === "facebook" ? `https://www.facebook.com/sharer/sharer.php?u=${url}` : type === "x" ? `https://x.com/intent/post?url=${url}&text=${title}` : type === "instagram" ? "https://www.instagram.com/" : "#"; });
    } catch (error) {
      setText(".article-header .blog-category", "Blog");
      setText(".article-header h1", "Blog yazısı bulunamadı");
      setText(".article-header__spot", "İstediğiniz blog yazısı yayında değil veya adresi değişmiş olabilir.");
      document.querySelector(".article-cover")?.setAttribute("hidden", "");
      document.querySelector(".article-secondary-media")?.setAttribute("hidden", "");
      document.querySelector(".article-layout")?.setAttribute("hidden", "");
      document.getElementById("relatedBlogSection")?.setAttribute("hidden", "");
    } finally { if (main) main.hidden = false; }
  }

  async function news() {
    const grid = document.getElementById("newsGrid"); if (!grid) return;
    const empty = document.getElementById("newsEmpty");
    let data;
    try { data = items(await get("news", { page: 1, pageSize: 100 })); } catch { return; }
    if (!data.length) return;
    data.sort((a, b) => {
      const dateDifference = new Date(b.publishDate || 0).getTime() - new Date(a.publishDate || 0).getTime();
      return dateDifference || Number(b.id || 0) - Number(a.id || 0);
    });
    if (empty) empty.hidden = true;
    grid.innerHTML = data.map((x) => `<a class="news-card news-card--compact" data-category="${esc(categorySlug(x.categoryName))}" href="haber-detay.html?slug=${encodeURIComponent(x.seoUrl || x.id)}"><div class="news-card__img"><img alt="${esc(x.title)}" src="${esc(asset(x.featuredImageUrl))}"></div><span class="news-card__date">${esc(date(x.publishDate))}</span><h3>${esc(x.title)}</h3></a>`).join("");
  }
  async function newsDetail() {
    if (page !== "haber-detay.html") return;
    const main = document.querySelector("main");
    try {
    if (!slug()) return;
    const x = await get(`news/${encodeURIComponent(slug())}`); setMeta(x);
    setText(".page-header h1", x.title); setText(".breadcrumb__current [itemprop=name]", x.title); setText(".news-card__tag", x.categoryName); setText(".page-header__desc", x.excerpt);
    const meta = document.querySelectorAll(".article-header__meta span"); if (meta.length) meta[meta.length - 1].textContent = date(x.publishDate);
    const cover = document.querySelector(".article-header__cover img"); if (cover && x.featuredImageUrl) { cover.src = asset(x.featuredImageUrl); cover.alt = x.title; }
    const body = document.querySelector(".article-body"); if (body && x.content) body.innerHTML = x.content;
    const relatedGrid = document.getElementById("relatedNewsGrid");
    const relatedSection = document.getElementById("relatedNewsSection");
    if (relatedGrid) {
      if (x.relatedPosts?.length) {
        relatedGrid.innerHTML = x.relatedPosts.map((rp) => `<a class="news-card" href="haber-detay.html?slug=${encodeURIComponent(rp.seoUrl || rp.id)}"><div class="news-card__img"><img alt="${esc(rp.title)}" src="${esc(asset(rp.featuredImageUrl))}"></div><h3>${esc(rp.title)}</h3><span class="news-card__more">Devamını Oku<svg class="arrow-icon" fill="none" stroke="currentColor" stroke-linecap="round" stroke-width="1.3" viewbox="0 0 24 24"><path d="M5 12h14M13 6l6 6-6 6"></path></svg></span></a>`).join("");
      } else if (relatedSection) {
        relatedSection.hidden = true;
      }
    }
    } finally { if (main) main.hidden = false; }
  }

  async function documents() {
    const list = document.querySelector(".docs-list"); if (!list) return;
    const empty = document.querySelector(".docs-empty");
    const controls = document.querySelector(".docs-controls");
    const searchField = document.getElementById("docsSearch")?.closest(".docs-field");
    if (controls && searchField && !document.getElementById("docsBrand")) {
      searchField.insertAdjacentHTML("beforebegin", '<label class="docs-field"><span>Marka</span><select id="docsBrand"><option value="">Tüm Markalar</option></select></label><label class="docs-field"><span>Koleksiyon</span><select id="docsCollection"><option value="">Tüm Koleksiyonlar</option></select></label>');
    }
    const showEmpty = () => { list.replaceChildren(); setText(".docs-count", "0 doküman"); if (empty) { empty.textContent = "Henüz yayınlanmış teknik doküman bulunmuyor."; empty.hidden = false; } };
    let data;
    try { data = await get("documents"); } catch { showEmpty(); return; }
    if (!data.length) { showEmpty(); return; }
    const replaceOptions = (id, label, options) => { const select = document.getElementById(id); if (!select) return; select.replaceChildren(new Option(label, "")); options.forEach((option) => select.add(new Option(option.label, option.value))); };
    const uniqueOptions = (items) => [...new Map(items.map((item) => [String(item.value), item])).values()].sort((a, b) => String(a.label).localeCompare(String(b.label), "tr"));
    replaceOptions("docsBrand", "Tüm Markalar", [
      { value: "NgSeramik", label: "NG SERAMİK" },
      { value: "NgStone", label: "NG STONE" },
      { value: "NgSlim", label: "NG SLIM" },
      { value: "NgPerforma", label: "NG PERFORMA" }
    ]);
    const refreshCollections = () => {
      const brand = document.getElementById("docsBrand")?.value;
      const available = data.filter((document) => !brand || (document.brands || []).some((item) => item.value === brand));
      replaceOptions("docsCollection", "Tüm Koleksiyonlar", uniqueOptions(available.flatMap((document) => document.collections || [])));
    };
    refreshCollections();
    list.innerHTML = data.map((x) => `<article class="docs-card" data-type="${esc(x.documentType)}" data-language="${esc(x.languageCode)}" data-brands="${esc((x.brands || []).map((item) => item.value).join("|"))}" data-collections="${esc((x.collections || []).map((item) => item.value).join("|"))}"><div class="docs-card__mark" aria-hidden="true">PDF</div><div><div class="docs-card__meta"><span>${esc(x.documentTypeLabel)}</span></div><h3>${esc(x.title)}</h3><p>${esc(x.description || x.originalFileName)}</p></div><div class="docs-card__actions"><a class="docs-action" href="${esc(asset(x.fileUrl))}" target="_blank" rel="noopener">Görüntüle</a><a class="docs-action" href="${API_ROOT}/documents/${x.id}/download">İndir</a></div></article>`).join("");
    if (empty) empty.hidden = true;
    const filter = () => { const q = (document.getElementById("docsSearch")?.value || "").toLocaleLowerCase(language); const type = document.getElementById("docsType")?.value; const brand = document.getElementById("docsBrand")?.value; const collection = document.getElementById("docsCollection")?.value; let count = 0; list.querySelectorAll(".docs-card").forEach((card) => { const brands = (card.dataset.brands || "").split("|"); const collections = (card.dataset.collections || "").split("|"); const show = (!q || card.textContent.toLocaleLowerCase(language).includes(q)) && (!type || card.dataset.type === type) && (!brand || brands.includes(brand)) && (!collection || collections.includes(collection)); card.hidden = !show; if (show) count++; }); setText(".docs-count", `${count} doküman`); if (empty) { empty.textContent = "Seçtiğiniz ölçütlere uygun doküman bulunamadı."; empty.hidden = count !== 0; } };
    ["docsSearch", "docsType", "docsCollection"].forEach((id) => document.getElementById(id)?.addEventListener(id === "docsSearch" ? "input" : "change", filter));
    document.getElementById("docsBrand")?.addEventListener("change", () => { refreshCollections(); filter(); });
    filter();
  }

  async function dealers() {
    const list = document.querySelector(".sales-list"); if (!list) return;
    setText(".sales-hero__intro", "Koleksiyonlarımızı yakından inceleyebileceğiniz yetkili bayileri, premium deneyimle keşfedin.");
    document.getElementById("salesSearch")?.closest(".sales-field")?.remove();
    const showEmpty = (message) => {
      list.innerHTML = `<p class="sales-empty">${esc(message)}</p>`;
      setText(".sales-count", "0 satış noktası");
      [["salesCity", "Tüm Ülkeler"], ["salesDistrict", "Tüm Şehirler"]]
        .forEach(([id, label]) => document.getElementById(id)?.replaceChildren(new Option(label, "")));
    };
    let data;
    try { data = await get("dealers"); } catch { showEmpty("Satış noktaları şu anda yüklenemiyor."); return; }
    if (!data.length) { showEmpty("Henüz satış noktası eklenmedi."); return; }
    data = data.map((item) => ({ ...item, category: item.category === "SalesPoint" ? "dealer" : "showroom" }));
    const cityCoordinates = {
      "Adana": [37, 35.3213], "Ankara": [39.9334, 32.8597], "Antalya": [36.8969, 30.7133],
      "Bursa": [40.195, 29.06], "Erzurum": [39.9043, 41.2679], "Eskişehir": [39.7767, 30.5206], "İstanbul": [41.0082, 28.9784],
      "İzmir": [38.4237, 27.1428], "Konya": [37.8746, 32.4932], "Kütahya": [39.4192, 29.9857], "Samsun": [41.2867, 36.33]
    };
    // Eski kayıtlar Şehir/İlçe düzeninde tutuluyordu. Yeni Ülke/Şehir formuna
    // geçildikten sonra iki veri biçimini de aynı dinamik filtrede destekle.
    data = data.map((item) => cityCoordinates[item.city]
      ? { ...item, filterCountry: "Türkiye", filterCity: item.city }
      : { ...item, filterCountry: item.city, filterCity: item.district });
    data = data.slice().sort((a, b) => a.name.localeCompare(b.name, "tr"));
    let liveMap;
    const mapMarkers = new Map();
    const loadLeaflet = () => new Promise((resolve, reject) => {
      if (window.L) { resolve(window.L); return; }
      if (!document.querySelector('link[data-sales-map="leaflet"]')) {
        const style = document.createElement("link"); style.rel = "stylesheet"; style.href = "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"; style.dataset.salesMap = "leaflet"; document.head.appendChild(style);
      }
      const existing = document.querySelector('script[data-sales-map="leaflet"]');
      if (existing) { existing.addEventListener("load", () => resolve(window.L), { once: true }); return; }
      const script = document.createElement("script"); script.src = "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"; script.dataset.salesMap = "leaflet"; script.onload = () => resolve(window.L); script.onerror = reject; document.head.appendChild(script);
    });
    const syncMapMarkers = () => {
      if (!liveMap) return;
      mapMarkers.forEach(({ marker, card }) => card.hidden ? marker.remove() : marker.addTo(liveMap));
    };
    list.innerHTML = data.map((x) => `<article class="sales-card" data-id="${x.id}" data-type="${x.category.toLowerCase()}" data-name="${esc(x.name)}" data-country="${esc(x.filterCountry)}" data-city="${esc(x.filterCity)}" data-address="${esc(x.address)}" data-brands="${esc((x.brands || []).join(","))}"><div class="sales-card__top"><h3>${esc(x.name)}</h3><span class="sales-badge">${esc(x.categoryLabel)}</span></div><address>${esc(x.address || [x.district, x.city].filter(Boolean).join(", "))}</address><div class="sales-card__contact">${x.phone ? `<a href="tel:${esc(x.phone)}">${esc(x.phone)}</a>` : ""}${x.email ? `<a href="mailto:${esc(x.email)}">${esc(x.email)}</a>` : ""}</div><div class="sales-card__actions"><a href="https://maps.google.com/?q=${encodeURIComponent(x.latitude && x.longitude ? `${x.latitude},${x.longitude}` : `${x.name} ${x.filterCity} ${x.filterCountry}`)}" target="_blank" rel="noopener">Yol Tarifi</a></div></article>`).join("");
    const initializeMap = async () => {
      const mapElement = document.querySelector(".sales-map"); if (!mapElement) return;
      try {
        const L = await loadLeaflet();
        mapElement.replaceChildren(); mapElement.classList.add("is-live-map");
        liveMap = L.map(mapElement, {
          scrollWheelZoom: true,
          touchZoom: true,
          doubleClickZoom: true,
          zoomControl: true
        }).setView([39, 35], 6);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { maxZoom: 19, attribution: "&copy; OpenStreetMap katkıda bulunanlar" }).addTo(liveMap);
        const bounds = [];
        data.forEach((item, index) => {
          const base = item.latitude && item.longitude ? [Number(item.latitude), Number(item.longitude)] : (cityCoordinates[item.filterCity] || [39, 35]);
          const offset = item.latitude && item.longitude ? 0 : ((index % 5) - 2) * .012;
          const position = [base[0] + offset, base[1] - offset];
          const address = item.address || [item.district, item.city].filter(Boolean).join(", ");
          const mapsUrl = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address || `${item.name} ${item.city}`)}`;
          const marker = L.marker(position).bindTooltip(`<strong>${esc(item.name)}</strong><br>${esc(address)}`, { direction: "top" });
          marker.on("click", () => window.open(mapsUrl, "_blank", "noopener")); marker.addTo(liveMap);
          const card = list.querySelector(`[data-id="${item.id}"]`); mapMarkers.set(String(item.id), { marker, card }); bounds.push(position);
        });
        if (bounds.length) liveMap.fitBounds(bounds, { padding: [45, 45], maxZoom: 12 });
        setTimeout(() => liveMap.invalidateSize(), 0);
      } catch { mapElement.innerHTML = '<p class="sales-map-note">Harita şu anda yüklenemiyor.</p>'; }
    };
    initializeMap();
    const replace = (id, label, values) => { const select = document.getElementById(id); if (!select) return; select.replaceChildren(new Option(label, "")); [...new Set(values.filter(Boolean))].sort((a, b) => a.localeCompare(b, "tr")).forEach((x) => select.add(new Option(x, x))); };
    const fieldLabels = document.querySelectorAll(".sales-controls .sales-field > span:first-child");
    if (fieldLabels[0]) fieldLabels[0].textContent = "Ülke";
    if (fieldLabels[1]) fieldLabels[1].textContent = "Şehir";
    replace("salesCity", "Tüm Ülkeler", data.map((x) => x.filterCountry));
    const refreshDistricts = () => {
      const city = document.getElementById("salesCity")?.value;
      replace("salesDistrict", "Tüm Şehirler", data.filter((x) => !city || x.filterCountry === city).map((x) => x.filterCity));
    };
    refreshDistricts();
    const filter = () => { const country = document.getElementById("salesCity")?.value; const city = document.getElementById("salesDistrict")?.value; const brand = document.getElementById("salesBrand")?.value; const type = document.getElementById("salesType")?.value; const q = (document.getElementById("salesSearch")?.value || "").toLocaleLowerCase(language); let count = 0; list.querySelectorAll(".sales-card").forEach((card) => { const cardBrands = (card.dataset.brands || "").split(",").filter(Boolean); const show = (!type || card.dataset.type === type) && (!country || card.dataset.country === country) && (!city || card.dataset.city === city) && (!brand || cardBrands.includes(brand)) && (!q || card.textContent.toLocaleLowerCase(language).includes(q)); card.hidden = !show; if (show) count++; }); setText(".sales-count", `${count} satış noktası`); syncMapMarkers(); };
    document.getElementById("salesCity")?.addEventListener("change", () => { refreshDistricts(); filter(); });
    ["salesDistrict", "salesBrand", "salesType", "salesSearch"].forEach((id) => document.getElementById(id)?.addEventListener(id === "salesSearch" ? "input" : "change", filter));
    document.getElementById("salesReset")?.addEventListener("click", () => {
      const city = document.getElementById("salesCity"); const search = document.getElementById("salesSearch"); const brand = document.getElementById("salesBrand"); const type = document.getElementById("salesType");
      if (city) city.value = ""; if (search) search.value = ""; if (brand) brand.value = ""; if (type) type.value = ""; refreshDistricts(); filter();
    });
    filter();
  }
  async function dealerDetail() {
    if (page !== "bayi-detay.html") return;
    const main = document.querySelector("main");
    try {
    if (!slug()) return;
    const x = await get(`dealers/${encodeURIComponent(slug())}`);
    setText("[data-dealer-name]", x.name); setText("[data-dealer-type]", x.categoryLabel); setText("[data-dealer-address]", x.address || [x.district, x.city].filter(Boolean).join(", "));
    const rows = document.querySelectorAll(".dealer-data dd a");
    if (rows[0] && x.phone) { rows[0].href = `tel:${x.phone}`; rows[0].textContent = x.phone; }
    if (rows[1] && x.email) { rows[1].href = `mailto:${x.email}`; rows[1].textContent = x.email; }
    const workingHours = document.querySelector(".dealer-data div:nth-child(4) dd"); if (workingHours) workingHours.textContent = x.workingHours || "Bilgi için satış noktasıyla iletişime geçin.";
    const region = document.querySelector(".dealer-data div:last-child dd"); if (region && x.regionName) region.textContent = x.regionName;
    const route = document.querySelector(".dealer-actions a:first-child"); if (route) route.href = `https://maps.google.com/?q=${encodeURIComponent(x.latitude && x.longitude ? `${x.latitude},${x.longitude}` : `${x.name} ${x.city}`)}`;
    const visual = document.querySelector(".dealer-visual"); if (visual && x.images?.length) { visual.classList.remove("dealer-visual--map"); const featured = x.images.find((item) => item.isFeatured) || x.images[0]; visual.innerHTML = `<img class="dealer-visual__main" src="${esc(asset(featured.url))}" alt="${esc(x.name)}"><div class="dealer-visual__thumbs">${x.images.map((item) => `<button type="button" data-dealer-image="${esc(asset(item.url))}"><img src="${esc(asset(item.url))}" alt=""></button>`).join("")}</div>`; visual.querySelectorAll("[data-dealer-image]").forEach((button) => button.addEventListener("click", () => { visual.querySelector(".dealer-visual__main").src = button.dataset.dealerImage; })); }
    } finally { if (main) main.hidden = false; }
  }

  async function productDetail() {
    if (page !== "urun-detay.html") return;
    const pageMain = document.querySelector("main");
    try {
    const backLink = document.querySelector("[data-product-back]");
    if (backLink) backLink.addEventListener("click", (event) => {
      if (window.history.length <= 1 || !document.referrer) return;
      event.preventDefault();
      window.history.back();
    });
    const currentSlug = slug();
    const title = document.querySelector(".pd-title");
    const eyebrow = document.querySelector(".pd-eyebrow");
    const initialMainImage = document.getElementById("pdMainFace");
    const clickedImage = params.get("image");
    if (initialMainImage && clickedImage) {
      initialMainImage.src = /^(https?:|data:|blob:)/i.test(clickedImage) ? clickedImage : clickedImage.replace(/^\//, "");
      initialMainImage.loading = "eager";
      initialMainImage.fetchPriority = "high";
    }
    if (!currentSlug) {
      if (eyebrow) eyebrow.hidden = true;
      if (title) title.textContent = "ÃœrÃ¼n seÃ§ilmedi";
      return;
    }
    if (eyebrow) eyebrow.hidden = true;
    if (title) title.textContent = "";
    const x = await get(`products/${encodeURIComponent(currentSlug)}`); setMeta(x);
    if (title) title.textContent = x.collectionName || x.name || x.productCode;
    const descriptionTitle = String(x.shortDescription || "").trim();
    const descriptionText = String(x.longDescription || "").trim();
    const descriptionHead = document.querySelector(".pd-section-head");
    setText("[data-product-description-title]", descriptionTitle);
    setText("[data-product-description]", descriptionText);
    if (descriptionHead) descriptionHead.hidden = !(descriptionTitle && descriptionText);
    setText("[data-product-contact-title]", x.collectionName ? `${x.collectionName} hakkında bilgi alın` : "");
    if (eyebrow) eyebrow.hidden = true;
    const breadcrumbProduct = document.getElementById("pdBreadcrumbProduct");
    if (breadcrumbProduct) breadcrumbProduct.textContent = x.name || x.productCode;
    setText("#pdProductCode", x.productCode || "-");
    const setVariantGroup = (key, value, suffix = "") => {
      const group = document.querySelector(`[data-variant-group="${key}"]`);
      const container = group?.closest(".pd-variant-group");
      const displayValue = value == null || String(value).trim() === "" ? "-" : `${value}${suffix}`;
      if (container) container.hidden = false;
      if (group) group.innerHTML = `<button class="pd-variant-chip is-active" data-variant-value="${esc(displayValue)}" aria-pressed="true" type="button">${esc(displayValue)}</button>`;
    };
    setVariantGroup("size", x.size);
    setVariantGroup("thickness", x.thickness, x.thickness == null ? "" : " mm");
    setVariantGroup("surface", x.surface);
    setVariantGroup("color", x.color);
    const values = { code: x.productCode, size: x.size, thickness: x.thickness == null ? "" : `${x.thickness} mm`, surface: x.surface, pei: x.pei, vValue: x.vValue, boxArea: x.boxM2, palletArea: x.palletM2 };
    Object.entries(values).forEach(([key, value]) => {
      document.querySelectorAll(`[data-variant-output="${key}"]`).forEach((row) => {
        const empty = value == null || String(value).trim() === "";
        row.hidden = false;
        const output = row.querySelector("dd");
        if (output) output.textContent = empty ? "-" : value;
      });
    });
    const yesNo = (value, trueLabel = "Evet", falseLabel = "Hayır") => value == null ? "" : (value ? trueLabel : falseLabel);
    const detailValues = new Map([
      ["ürün adı", x.name], ["ürün kodu", x.productCode], ["durum", x.statusLabel],
      ["ürün grubu", x.productGroup], ["kategori", x.categoryName],
      ["renk", x.color], ["ebat", x.size], ["birim", x.unit],
      ["kalınlık", x.thickness == null ? "" : `${x.thickness} mm`], ["yüzey", x.surface],
      ["özel yüzey", x.specialSurface], ["rölyef", x.relief],
      ["face", yesNo(x.hasFace, "Var", "Yok")], ["mekân", yesNo(x.hasVenue, "Var", "Yok")],
      ["face sayısı", x.faceCount], ["kategori / doku / görünüm", x.categoryName],
      ["bünye", x.bodyType], ["bünye / ürün tipi", x.bodyType], ["bitiş", x.finish],
      ["kenar işlem", x.finish], ["pei", x.pei], ["peı", x.pei], ["v değeri", x.vValue],
      ["r değeri", x.rValue], ["derin aşınma", x.deepAbrasion],
      ["ısıya dayanıklılık", yesNo(x.heatResistance)], ["kaymaz", yesNo(x.antiSlip)],
      ["sırlı granite", yesNo(x.glazedGranite)], ["renk malzeme grubu", x.colorMaterial],
      ["uygulama alanı", x.applicationArea], ["kullanım alanı", x.usageArea],
      ["kutu m²", x.boxM2 == null ? "" : `${x.boxM2} m²`],
      ["palet m²", x.palletM2 == null ? "" : `${x.palletM2} m²`]
    ]);
    const groups = document.querySelectorAll(".pd-tech-group");
    const renderRows = (rows) => rows.map((label) => `<div class="pd-tech-row"><dt>${esc(label)}</dt><dd></dd></div>`).join("");
    if (groups[0]) groups[0].innerHTML = `<h3>Ürün ve yüzey bilgileri</h3><dl class="pd-tech-table">${renderRows(["Ürün Adı", "Ürün Kodu", "Durum", "Ürün Grubu", "Kategori", "Ebat", "Birim", "Yüzey", "Özel Yüzey", "Rölyef", "Face", "Mekân", "Face Sayısı", "Renk", "Renk Malzeme Grubu", "Kalınlık", "Bünye", "Bitiş", "PEI", "V Değeri", "R Değeri", "Derin Aşınma", "Isıya Dayanıklılık", "Kaymaz", "Sırlı Granite"])}</dl>`;
    if (groups[1]) groups[1].innerHTML = `<h3>Uygulama bilgileri</h3><dl class="pd-tech-table">${renderRows(["Uygulama Alanı", "Kullanım Alanı"])}</dl>`;
    if (groups[2]) groups[2].innerHTML = `<h3>Paketleme ve satış</h3><dl class="pd-tech-table">${renderRows(["Kutu m²", "Palet m²"])}</dl>`;
    document.querySelectorAll(".pd-tech-row").forEach((row) => {
      const label = row.querySelector("dt")?.textContent?.trim().toLocaleLowerCase("tr-TR");
      if (!label) return;
      const value = detailValues.has(label) ? detailValues.get(label) : null;
      const empty = value == null || String(value).trim() === "";
      row.hidden = false;
      const output = row.querySelector("dd");
      if (output) output.textContent = empty ? "-" : value;
    });
    const clickedCardImage = params.get("image");
    const resolvedClickedCardImage = clickedCardImage
      ? (/^(https?:|data:|blob:)/i.test(clickedCardImage) ? clickedCardImage : clickedCardImage.replace(/^\//, ""))
      : SURFACE_IMAGE_PATH;
    const images = x.images?.length
      ? x.images.slice().sort((a, b) => Number(b.isPrimary) - Number(a.isPrimary) || Number(a.displayOrder) - Number(b.displayOrder)).slice(0, 6).map((image) => ({ ...image, resolvedUrl: asset(image.url) }))
      : (x.primaryImageUrl
        ? [{ url: x.primaryImageUrl, resolvedUrl: asset(x.primaryImageUrl) }]
        : [{ url: resolvedClickedCardImage, resolvedUrl: resolvedClickedCardImage }]);
    const main = document.getElementById("pdMainFace"); const thumbs = document.querySelector(".pd-thumbs");
    if (main && images.length) { main.src = images[0].resolvedUrl; main.alt = x.name; }
    if (thumbs && images.length) {
      thumbs.innerHTML = images.map((image, index) => `<button class="pd-thumb${index ? "" : " is-active"}" data-media-type="${esc(String(image.imageType || "").toLowerCase())}" data-pd-src="${esc(image.resolvedUrl)}"><img alt="${esc(x.name)} ${index + 1}" src="${esc(image.resolvedUrl)}"></button>`).join("");
      thumbs.querySelectorAll("[data-pd-src]").forEach((button) => button.addEventListener("click", () => {
        thumbs.querySelectorAll(".pd-thumb").forEach((item) => item.classList.remove("is-active"));
        button.classList.add("is-active");
        if (main) main.src = button.dataset.pdSrc;
      }));
    }
    } finally { if (pageMain) pageMain.hidden = false; }
  }

  async function brandProducts() {
    const brandMap = { "ng-kutahya-seramik": "NgSeramik", "ng-stone": "NgStone", "ng-slim": "NgSlim", "ng-performa": "NgPerforma" };
    const brandKey = Object.keys(brandMap).find((key) => page.startsWith(`${key}-`));
    if (!brandKey) return genericPage();
    const data = await get("collections", { brand: brandMap[brandKey] }); if (!data.length) return;
    const selectedCollectionId = params.get("collectionId");
    const keepResultsOnPage = true;
    let filterRoot = document.querySelector(".unified-collections-filters, .slim-collections-filters, .performa-collections-filters");
    let sample = filterRoot?.querySelector("[data-filter]");
    if (!filterRoot || !sample) {
      filterRoot = document.createElement("div");
      filterRoot.hidden = true;
      sample = document.createElement("button");
      sample.dataset.filter = "all";
      filterRoot.appendChild(sample);
      document.body.appendChild(filterRoot);
    }
    const buttonClass = sample.className.replace(/\s*active\b/g, "");
    filterRoot.innerHTML = `<button class="${esc(buttonClass)}${selectedCollectionId ? "" : " active"}" type="button" data-filter="all" aria-pressed="${!selectedCollectionId}">Tümü</button>` + data.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((x) => {
      const selected = String(x.id) === String(selectedCollectionId || "");
      return `<button class="${esc(buttonClass)}${selected ? " active" : ""}" type="button" data-filter="${esc(x.name)}" data-collection-id="${x.id}" aria-pressed="${selected}">${esc(x.name)}</button>`;
    }).join("");
    const scrollKey = `ng-brand-collection-filter-scroll:${page}`;
    const selectionKey = `ng-brand-collection-filter-selection:${page}`;
    requestAnimationFrame(() => {
      const savedPosition = Number(sessionStorage.getItem(scrollKey));
      const savedSelection = sessionStorage.getItem(selectionKey);
      const selectedButton = filterRoot.querySelector('[aria-pressed="true"]');
      if (selectedCollectionId && selectedButton && savedSelection !== String(selectedCollectionId)) {
        filterRoot.scrollLeft = Math.max(0, selectedButton.offsetLeft - ((filterRoot.clientWidth - selectedButton.offsetWidth) / 2));
        sessionStorage.setItem(scrollKey, String(filterRoot.scrollLeft));
      } else if (Number.isFinite(savedPosition)) {
        filterRoot.scrollLeft = savedPosition;
      }
    });
    filterRoot.querySelector('[data-filter="all"]')?.addEventListener("click", () => {
      sessionStorage.setItem(scrollKey, String(filterRoot.scrollLeft));
      sessionStorage.setItem(selectionKey, "");
      location.href = keepResultsOnPage ? page : "index-koleksiyonlar.html";
    });
    filterRoot.querySelectorAll("[data-collection-id]").forEach((button) => button.addEventListener("click", () => {
      sessionStorage.setItem(scrollKey, String(filterRoot.scrollLeft));
      sessionStorage.setItem(selectionKey, String(button.dataset.collectionId));
      location.href = keepResultsOnPage ? `${page}?collectionId=${button.dataset.collectionId}` : `index-koleksiyonlar.html?collectionId=${button.dataset.collectionId}`;
    }));
    const grid = document.getElementById("slimCollectionsGrid") || document.getElementById("performaCollectionsGrid") || document.getElementById("stoneListingGrid") || document.querySelector("main .listing .grid");
    if (grid) {
      if (keepResultsOnPage && selectedCollectionId) {
        const empty = document.getElementById("empty");
        const products = [true]; // Eski boş-durum kontrolünü API sayfalayıcısı yönetir.
        await setupProductPagination({ grid, filters: { collectionId: selectedCollectionId, brand: brandMap[brandKey] }, fallback: COLLECTION_IMAGE_PATH, searchSelector: ".unified-collections-search, .slim-collections-search, .stone-listing-search, .performa-collections-search, #search", empty });
        if (empty) { empty.textContent = "Bu seriye ait aktif ürün bulunamadı."; empty.classList.toggle("show", products.length === 0); }
      } else {
        grid.classList.remove("cms-product-results");
        grid.innerHTML = data.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((x) => `<a class="carousel-item" href="${keepResultsOnPage ? `${page}?collectionId=${x.id}` : `index-koleksiyonlar.html?collectionId=${x.id}`}" data-name="${esc(x.name)}"><div class="carousel-item__img"><img src="${esc(collectionCardImage(x))}" alt="${esc(x.name)} koleksiyonu"></div><h3>${esc(x.name)}</h3><p>${x.productCount} ürün</p></a>`).join("");
        document.getElementById("empty")?.classList.remove("show");
      }
      if (!(keepResultsOnPage && selectedCollectionId)) setupNameSearch(grid, ".unified-collections-search, .slim-collections-search, .stone-listing-search, .performa-collections-search, #search");
    }
  }

  async function categories() {
    const grid = document.getElementById("listingGrid") || document.getElementById("slimSurfacesGrid") || document.getElementById("stoneListingGrid"); if (!grid) return genericPage();
    showCardSkeletons(grid, 8, true);
    const brandMap = { "ng-kutahya-seramik-yuzeyler.html": "NgSeramik", "ng-stone-yuzeyler.html": "NgStone", "ng-slim-yuzeyler.html": "NgSlim", "ng-performa-yuzeyler.html": "NgPerforma" };
    const currentBrand = brandMap[page];
    if (currentBrand) {
      const categoryData = await get("categories", { brand: currentBrand });
      const selectedCategoryId = params.get("categoryId");
      const filterRoot = document.querySelector(page === "ng-slim-yuzeyler.html"
        ? ".slim-surfaces-filters"
        : (page === "ng-stone-yuzeyler.html" ? ".stone-surfaces-filters" : ".surface-inline-filters"));
      const sampleFilter = filterRoot?.querySelector("[data-filter]");
      const filterClass = sampleFilter?.className.replace(/\s*active\b/g, "") || "";
      const sortedCategories = categoryData.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" }));

      if (filterRoot) {
        filterRoot.innerHTML = `<button class="${esc(filterClass)}${selectedCategoryId ? "" : " active"}" type="button" data-category-id="" aria-pressed="${selectedCategoryId ? "false" : "true"}">Tümü</button>` + sortedCategories.map((item) => {
          const selected = String(item.id) === String(selectedCategoryId || "");
          return `<button class="${esc(filterClass)}${selected ? " active" : ""}" type="button" data-category-id="${esc(item.id)}" aria-pressed="${selected}">${esc(item.name)}</button>`;
        }).join("");
      }

      const search = document.querySelector(".surface-inline-search, .slim-surfaces-search, .stone-surfaces-search");
      const drawerSearch = document.getElementById("searchInput");
      const empty = document.querySelector("#emptyState, .slim-surfaces-empty, .stone-surfaces-empty");
      const count = document.getElementById("resultCount");
      const normalize = (value) => String(value || "").trim().toLocaleLowerCase("tr-TR");

      if (selectedCategoryId) {
        clearCardSkeletons(grid);
        if (search) { search.placeholder = "Ürün ara"; search.setAttribute("aria-label", "Ürün ara"); }
        await setupProductPagination({ grid, filters: { brand: currentBrand, categoryId: selectedCategoryId }, fallback: SURFACE_IMAGE_PATH, searchSelector: page === "ng-slim-yuzeyler.html" ? ".slim-surfaces-search" : (page === "ng-stone-yuzeyler.html" ? ".stone-surfaces-search" : ".surface-inline-search"), empty, count });
        filterRoot?.querySelectorAll("[data-category-id]").forEach((button) => button.addEventListener("click", () => { location.href = button.dataset.categoryId ? `${page}?categoryId=${encodeURIComponent(button.dataset.categoryId)}` : page; }));
        return;
        const products = items(await get("products", { brand: currentBrand, categoryId: selectedCategoryId, pageSize: 2000 }));
        grid.classList.add("cms-product-results");
        grid.innerHTML = products.map((x) => `<a class="carousel-item cms-product-result-card" href="${esc(productDetailHref(x, SURFACE_IMAGE_PATH))}" data-name="${esc(x.name)}"><div class="carousel-item__img"><img src="${esc(productCardImage(x, SURFACE_IMAGE_PATH))}" alt="${esc(x.name)}"></div><h3 class="cms-product-name">${esc(x.name)}</h3><p class="cms-product-code">${esc(x.productCode)}</p></a>`).join("");
        if (search) { search.placeholder = "Ürün ara"; search.setAttribute("aria-label", "Ürün ara"); }
        setupNameSearch(grid, page === "ng-slim-yuzeyler.html" ? ".slim-surfaces-search" : (page === "ng-stone-yuzeyler.html" ? ".stone-surfaces-search" : ".surface-inline-search"));
        if (count) count.textContent = `${products.length} ürün gösteriliyor`;
        if (empty) empty.classList.toggle("is-visible", products.length === 0);
      } else {
        clearCardSkeletons(grid);
        grid.classList.remove("cms-product-results");
        grid.innerHTML = sortedCategories.map((x) => `<a class="surface-card" href="${page}?categoryId=${encodeURIComponent(x.id)}" data-category-id="${esc(x.id)}" data-name="${esc(x.name)}"><div class="surface-card__image"><img src="${esc(x.imageUrl ? asset(x.imageUrl) : SURFACE_IMAGE_PATH)}" alt="${esc(x.name)} kategorisi" loading="lazy"></div><h3>${esc(x.name)}</h3><p>${x.productCount} ürün</p></a>`).join("");
        if (search) { search.placeholder = "Kategori ara"; search.setAttribute("aria-label", "Kategori ara"); }
        const cards = [...grid.querySelectorAll(".surface-card")];
        const renderCategories = () => {
          const term = normalize(search?.value || drawerSearch?.value);
          const activeId = filterRoot?.querySelector('[aria-pressed="true"]')?.dataset.categoryId || "";
          let visible = 0;
          cards.forEach((card) => {
            const show = (!activeId || card.dataset.categoryId === activeId) && (!term || normalize(card.dataset.name).includes(term));
            card.hidden = !show;
            if (show) { card.style.removeProperty("display"); visible++; } else card.style.setProperty("display", "none", "important");
          });
          if (count) count.textContent = `${visible} kategori gösteriliyor`;
          if (empty) empty.classList.toggle("is-visible", visible === 0);
        };
        filterRoot?.querySelectorAll("[data-category-id]").forEach((button) => button.addEventListener("click", () => {
          filterRoot.querySelectorAll("[data-category-id]").forEach((item) => { item.classList.toggle("active", item === button); item.setAttribute("aria-pressed", String(item === button)); });
          renderCategories();
        }));
        [search, drawerSearch].filter(Boolean).forEach((input) => input.addEventListener("input", () => { if (search && drawerSearch) (input === search ? drawerSearch : search).value = input.value; renderCategories(); }));
        renderCategories();
      }

      filterRoot?.querySelectorAll("[data-category-id]").forEach((button) => {
        if (!selectedCategoryId) return;
        button.addEventListener("click", () => { location.href = button.dataset.categoryId ? `${page}?categoryId=${encodeURIComponent(button.dataset.categoryId)}` : page; });
      });
      document.querySelectorAll("#surfacePagination, #slimSurfacesPagination, #stoneListingPagination").forEach((node) => { node.hidden = true; });
      return;
    }
    const data = await get("surfaces", { brand: currentBrand });
    const normalize = (value) => String(value || "").trim().toLocaleLowerCase("tr-TR");
    const selectedSurface = params.get("surface");
    const keepsSurfaceResultsOnPage = page === "ng-kutahya-seramik-yuzeyler.html" || page === "ng-slim-yuzeyler.html" || page === "ng-stone-yuzeyler.html";
    if (keepsSurfaceResultsOnPage && selectedSurface) {
      const filterRoot = document.querySelector(page === "ng-slim-yuzeyler.html"
        ? ".slim-surfaces-filters"
        : (page === "ng-stone-yuzeyler.html" ? ".stone-surfaces-filters" : ".surface-inline-filters"));
      const sample = filterRoot?.querySelector("[data-filter]");
      if (filterRoot && sample) {
        const buttonClass = sample.className.replace(/\s*active\b/g, "");
        filterRoot.innerHTML = `<button class="${esc(buttonClass)}" type="button" data-filter="all" aria-pressed="false">Tümü</button>` + data.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((item) => {
          const selected = normalize(item.name) === normalize(selectedSurface);
          return `<button class="${esc(buttonClass)}${selected ? " active" : ""}" type="button" data-surface="${esc(item.name)}" aria-pressed="${selected}">${esc(item.name)}</button>`;
        }).join("");
        const surfaceStoragePrefix = page === "ng-slim-yuzeyler.html" ? "ng-slim" : (page === "ng-stone-yuzeyler.html" ? "ng-stone" : "ng-kutahya");
        const scrollKey = `${surfaceStoragePrefix}-surface-filter-scroll`;
        const selectionKey = `${surfaceStoragePrefix}-surface-filter-selection`;
        requestAnimationFrame(() => {
          const saved = Number(sessionStorage.getItem(scrollKey));
          const savedSelection = sessionStorage.getItem(selectionKey);
          const selectedButton = filterRoot.querySelector('[aria-pressed="true"]');
          if (selectedButton && savedSelection !== selectedSurface) {
            filterRoot.scrollLeft = Math.max(0, selectedButton.offsetLeft - ((filterRoot.clientWidth - selectedButton.offsetWidth) / 2));
            sessionStorage.setItem(scrollKey, String(filterRoot.scrollLeft));
          } else if (Number.isFinite(saved)) {
            filterRoot.scrollLeft = saved;
          }
        });
        filterRoot.querySelector('[data-filter="all"]')?.addEventListener("click", () => {
          sessionStorage.setItem(scrollKey, String(filterRoot.scrollLeft));
          sessionStorage.setItem(selectionKey, "");
          location.href = page;
        });
        filterRoot.querySelectorAll("[data-surface]").forEach((button) => button.addEventListener("click", () => {
          sessionStorage.setItem(scrollKey, String(filterRoot.scrollLeft));
          sessionStorage.setItem(selectionKey, String(button.dataset.surface));
          location.href = `${page}?surface=${encodeURIComponent(button.dataset.surface)}`;
        }));
      }
      const productSearchSelector = page === "ng-slim-yuzeyler.html" ? ".slim-surfaces-search" : (page === "ng-stone-yuzeyler.html" ? ".stone-surfaces-search" : ".surface-inline-search");
      const productEmpty = document.querySelector("#emptyState, .slim-surfaces-empty, .stone-surfaces-empty");
      await setupProductPagination({ grid, filters: { brand: currentBrand, surface: selectedSurface }, fallback: SURFACE_IMAGE_PATH, searchSelector: productSearchSelector, empty: productEmpty, count: document.getElementById("resultCount") });
      return;
      const products = items(await get("products", { brand: currentBrand, surface: selectedSurface, pageSize: 2000 }));
      grid.classList.add("cms-product-results");
      grid.innerHTML = products.map((x) => `<a class="carousel-item cms-product-result-card" href="${esc(productDetailHref(x, SURFACE_IMAGE_PATH))}" data-name="${esc(x.name)}"><div class="carousel-item__img"><img src="${esc(productCardImage(x, SURFACE_IMAGE_PATH))}" alt="${esc(x.name)}"></div><h3 class="cms-product-name">${esc(x.name)}</h3><p class="cms-product-code">${esc(x.productCode)}</p></a>`).join("");
      const empty = document.querySelector("#emptyState, .slim-surfaces-empty, .stone-surfaces-empty");
      if (empty) empty.classList.toggle("is-visible", products.length === 0);
      setupNameSearch(grid, page === "ng-slim-yuzeyler.html"
        ? ".slim-surfaces-search"
        : (page === "ng-stone-yuzeyler.html" ? ".stone-surfaces-search" : ".surface-inline-search"));
      return;
    }
    grid.classList.remove("cms-product-results");
    const surfaceResultPage = keepsSurfaceResultsOnPage ? page : "index-koleksiyonlar.html";
    grid.innerHTML = data.map((x) => `<a class="surface-card" href="${surfaceResultPage}${surfaceResultPage === page ? `?surface=${encodeURIComponent(x.name)}` : `?brand=${encodeURIComponent(currentBrand)}&surface=${encodeURIComponent(x.name)}` }" data-category="${esc(x.name)}" data-name="${esc(x.name)}"><div class="surface-card__image">${surfaceImageMarkup(x)}</div><h3>${esc(x.name)}</h3><p>${x.productCount} ürün</p></a>`).join("");

    const cards = [...grid.children].filter((card) => card.dataset.category);
    const trSort = (a, b) => String(a).localeCompare(String(b), "tr", { sensitivity: "base" });
    cards.sort((a, b) => trSort(a.dataset.category, b.dataset.category)).forEach((card) => grid.appendChild(card));
    const categoryNames = [...new Set([...cards.map((card) => card.dataset.category), ...data.map((x) => x.name)].filter(Boolean))].sort(trSort);
    const filterRoot = document.querySelector(".surface-inline-filters, .slim-surfaces-filters, .stone-surfaces-filters");
    const oldFilter = filterRoot?.querySelector("[data-filter]");
    const allValue = oldFilter?.dataset.filter === "" ? "" : "all";
    if (filterRoot && oldFilter) {
      const filterClass = oldFilter.className;
      filterRoot.innerHTML = [`<button class="${esc(filterClass)}" type="button" data-filter="${allValue}" aria-pressed="true">Tümü</button>`, ...categoryNames.map((name) => `<button class="${esc(filterClass)}" type="button" data-filter="${esc(name)}" aria-pressed="false">${esc(name)}</button>`)].join("");
    }
    const categorySelect = document.getElementById("categorySelect");
    if (categorySelect) categorySelect.innerHTML = `<option value="">Tümü</option>${categoryNames.map((name) => `<option value="${esc(name)}">${esc(name)}</option>`).join("")}`;

    const filters = [...(filterRoot?.querySelectorAll("[data-filter]") || [])];
    const search = document.querySelector(".surface-inline-search, .slim-surfaces-search, .stone-surfaces-search");
    let surfaceSearchCount = search?.parentElement?.querySelector(".cms-search-result-count");
    if ((page === "ng-kutahya-seramik-yuzeyler.html" || page === "ng-slim-yuzeyler.html" || page === "ng-stone-yuzeyler.html") && search && !surfaceSearchCount) {
      const wrapper = document.createElement("div");
      wrapper.className = "cms-surface-search-wrap";
      search.insertAdjacentElement("beforebegin", wrapper);
      wrapper.appendChild(search);
      surfaceSearchCount = document.createElement("span");
      surfaceSearchCount.className = "cms-search-result-count";
      surfaceSearchCount.setAttribute("aria-live", "polite");
      wrapper.appendChild(surfaceSearchCount);
    }
    const drawerSearch = document.getElementById("searchInput");
    const empty = document.querySelector("#emptyState, .slim-surfaces-empty, .stone-surfaces-empty");
    const count = document.getElementById("resultCount");
    const pagination = document.querySelector("#surfacePagination, #slimSurfacesPagination, #stoneListingPagination");
    const numbers = document.querySelector("#surfacePageNumbers, #slimSurfacesPageNumbers, #stoneListingPageNumbers");
    const prev = document.querySelector("#surfacePrev, #slimSurfacesPrev, #stoneListingPrev");
    const next = document.querySelector("#surfaceNext, #slimSurfacesNext, #stoneListingNext");
    const pageSize = Number.MAX_SAFE_INTEGER;
    let activeFilter = allValue;
    let currentPage = 1;
    const render = () => {
      const term = normalize(search?.value || drawerSearch?.value);
      const matched = cards.filter((card) => (activeFilter === allValue || card.dataset.category === activeFilter) && (!term || normalize(`${card.dataset.name} ${card.textContent}`).includes(term)));
      const totalPages = Math.max(1, Math.ceil(matched.length / pageSize));
      currentPage = Math.min(currentPage, totalPages);
      cards.forEach((card) => { card.hidden = true; });
      matched.slice((currentPage - 1) * pageSize, currentPage * pageSize).forEach((card) => { card.hidden = false; });
      if (count) count.textContent = `${matched.length} yüzey gösteriliyor`;
      if (surfaceSearchCount) surfaceSearchCount.textContent = `${matched.length} yüzey`;
      if (empty) empty.classList.toggle("is-visible", matched.length === 0);
      if (pagination) pagination.hidden = matched.length <= pageSize;
      if (prev) prev.disabled = currentPage === 1;
      if (next) next.disabled = currentPage === totalPages;
      if (numbers) numbers.innerHTML = Array.from({ length: totalPages }, (_, index) => `<button type="button" class="${index + 1 === currentPage ? "active" : ""}" data-page="${index + 1}">${index + 1}</button>`).join("");
    };
    filters.forEach((button) => button.addEventListener("click", () => { activeFilter = button.dataset.filter; currentPage = 1; filters.forEach((item) => item.setAttribute("aria-pressed", String(item === button))); if (categorySelect) categorySelect.value = activeFilter === allValue ? "" : activeFilter; render(); }));
    [search, drawerSearch].filter(Boolean).forEach((input) => input.addEventListener("input", () => { if (search && drawerSearch) (input === search ? drawerSearch : search).value = input.value; currentPage = 1; render(); }));
    categorySelect?.addEventListener("change", () => { activeFilter = categorySelect.value || allValue; currentPage = 1; filters.forEach((button) => button.setAttribute("aria-pressed", String(button.dataset.filter === activeFilter))); render(); });
    numbers?.addEventListener("click", (event) => { const button = event.target.closest("[data-page]"); if (!button) return; currentPage = Number(button.dataset.page); render(); });
    prev?.addEventListener("click", () => { if (currentPage > 1) { currentPage--; render(); } });
    next?.addEventListener("click", () => { currentPage++; render(); });
    render();
  }

  async function brandHomeSurfaces() {
    const brands = {
      "ng-kutahya-seramik.html": { value: "NgSeramik", id: "kutahyaSurfaces", collectionId: "kutahyaCollections" },
      "ng-slim.html": { value: "NgSlim", id: "slimSurfaces", collectionId: "slimCollections" },
      "ng-stone.html": { value: "NgStone", id: "stoneSurfaces", collectionId: "stoneCollections" },
      "ng-performa.html": { value: "NgPerforma", id: "performaSurfaces", collectionId: "performaCollections" }
    };
    const config = brands[page]; if (!config) return genericPage();
    const initialSurfaceTrack = document.getElementById(config.id);
    const initialCollectionTrack = document.getElementById(config.collectionId);
    showCardSkeletons(initialSurfaceTrack);
    showCardSkeletons(initialCollectionTrack);
    const [data, collectionData] = await Promise.all([
      get("categories", { brand: config.value }),
      get("collections", { brand: config.value })
    ]);
    const collectionTrack = document.getElementById(config.collectionId);
    if (collectionTrack) {
      clearCardSkeletons(collectionTrack);
      const collectionPage = page.replace(/\.html$/i, "-koleksiyonlar.html");
      collectionTrack.innerHTML = collectionData.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((x, index) => `<a class="carousel-item" href="${collectionPage}?collectionId=${x.id}"><div class="carousel-item__img"><img src="${esc(collectionCardImage(x))}" alt="${esc(x.name)} koleksiyonu" loading="${index < 5 ? "eager" : "lazy"}"${index < 5 ? ' fetchpriority="high"' : ""}></div><h3>${esc(x.name)}</h3></a>`).join("");
      setupHomeCarousel(collectionTrack, "collection");
    }
    if (page === "ng-performa.html") {
      document.getElementById(config.id)?.closest(".surfaces-section")?.remove();
      return;
    }
    let track = document.getElementById(config.id);
    let section = track?.closest(".surfaces-section");
    if (!data.length) { if (section) section.hidden = true; return; }
    if (!track && page === "ng-performa.html") {
      section = document.createElement("section");
      section.className = "section surfaces-section";
      section.innerHTML = `<div class="wrap"><div class="section-heading"><p class="eyebrow">YÜZEYLER</p></div><div class="surfaces-carousel-wrap"><div class="carousel surfaces-carousel-scroll" id="${config.id}"></div></div></div>`;
      document.querySelector("main .feature-section")?.before(section);
      track = document.getElementById(config.id);
    }
    if (!track) return;
    clearCardSkeletons(track);
    const categoryPage = page === "ng-kutahya-seramik.html"
      ? "ng-kutahya-seramik-yuzeyler.html"
      : (page === "ng-slim.html" ? "ng-slim-yuzeyler.html" : (page === "ng-stone.html" ? "ng-stone-yuzeyler.html" : "index-koleksiyonlar.html"));
    track.innerHTML = data.slice().sort((a, b) => String(a.name).localeCompare(String(b.name), "tr", { sensitivity: "base" })).map((x, index) => `<a class="surface-card" href="${categoryPage}?categoryId=${encodeURIComponent(x.id)}"><div class="surface-card__image"><img src="${esc(x.imageUrl ? asset(x.imageUrl) : SURFACE_IMAGE_PATH)}" alt="${esc(x.name)} kategorisi" loading="${index < 5 ? "eager" : "lazy"}"${index < 5 ? ' fetchpriority="high"' : ""}></div><h3>${esc(x.name)}</h3></a>`).join("");
    setupHomeCarousel(track, "surface");
  }

  async function aboutCertificates() {
    if (page !== "hakkimizda.html") return;
    const grid = document.getElementById("certificateGrid");
    const empty = document.getElementById("certificateEmpty");
    if (!grid) return;
    const showEmpty = () => { grid.replaceChildren(); if (empty) empty.hidden = false; };
    let data;
    try { data = (await get("documents")).filter((item) => item.documentType === "Certificate"); }
    catch { showEmpty(); return; }
    if (!data.length) { showEmpty(); return; }
    data.sort((a, b) => Number(a.displayOrder || 0) - Number(b.displayOrder || 0) || String(a.title || "").localeCompare(String(b.title || ""), language));
    grid.innerHTML = data.map((item) => `<article class="certificate-card">
      <div class="certificate-card__head">
        <svg aria-hidden="true" class="certificate-card__icon" fill="none" stroke="currentColor" stroke-linejoin="round" stroke-width="1.25" viewBox="0 0 24 24"><path d="M6.5 2.75h7l4 4v14.5h-11z"></path><path d="M13.5 2.75v4h4M9 12h6M9 15h6M9 18h4"></path></svg>
        <span class="certificate-card__type">${esc((item.contentType || "").includes("pdf") ? "PDF Belgesi" : (item.documentTypeLabel || "Belge"))}${item.fileSizeLabel ? ` · ${esc(item.fileSizeLabel)}` : ""}</span>
      </div>
      <h3>${esc(item.title)}</h3>
      <p class="certificate-card__description">${esc(item.description || "Sertifika belgesi")}</p>
      <div class="certificate-card__footer">
        <span class="certificate-card__filename" title="${esc(item.originalFileName || "")}">${esc(item.originalFileName || "Belge")}</span>
        <a class="certificate-download" href="${API_ROOT}/documents/${encodeURIComponent(item.id)}/download">
          <svg aria-hidden="true" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.4" viewBox="0 0 24 24"><path d="M12 3v12m-4-4 4 4 4-4M5 20h14"></path></svg>
          İndir
        </a>
      </div>
    </article>`).join("");
    if (empty) empty.hidden = true;
  }

  async function aboutManagedContent() {
    if (page !== "hakkimizda.html") return;
    let managed;
    try { const response = await get("pages/hakkimizda-yonetimi"); managed = JSON.parse(response.blocks?.[0]?.content || "{}"); }
    catch { managed = {}; }
    managed = Object.fromEntries(Object.entries(managed).map(([key, value]) => [key.charAt(0).toLowerCase() + key.slice(1), value]));
    const text = (selector, value) => { if (value != null && value !== "") setText(selector, value); };
    const lines = (value, size) => String(value || "").split(/\r?\n/).map(line => line.split("|").map(part => part.trim())).filter(parts => parts.length >= size && parts[0]);
    text(".page-header .section-label", managed.headerEyebrow); text(".page-header__title", managed.headerTitle);
    const aboutDescription = document.querySelector(".page-header__desc");
    const aboutFullText = String(managed.headerDescription || aboutDescription?.textContent || "").trim();
    if (aboutDescription && aboutFullText) {
      const visibleEndMarker = "devam ediyor.";
      const markerIndex = aboutFullText.indexOf(visibleEndMarker);
      const visibleEnd = markerIndex >= 0 ? markerIndex + visibleEndMarker.length : aboutFullText.length;
      const visibleText = aboutFullText.slice(0, visibleEnd).trim();
      const remainingText = aboutFullText.slice(visibleEnd).trim();
      aboutDescription.textContent = visibleText;
      document.querySelector(".page-header__desc-more")?.remove();
      document.querySelector(".page-header__more-button")?.remove();
      if (remainingText) {
        const moreText = document.createElement("p");
        moreText.className = "page-header__desc page-header__desc-more";
        moreText.hidden = true;
        moreText.textContent = remainingText;
        const moreButton = document.createElement("button");
        moreButton.type = "button";
        moreButton.className = "page-header__more-button";
        moreButton.setAttribute("aria-expanded", "false");
        moreButton.textContent = "Devamını Oku";
        moreButton.addEventListener("click", () => {
          const expanded = moreButton.getAttribute("aria-expanded") === "true";
          moreButton.setAttribute("aria-expanded", String(!expanded));
          moreButton.textContent = expanded ? "Devamını Oku" : "Daha Az Göster";
          moreText.hidden = expanded;
        });
        aboutDescription.after(moreText, moreButton);
      }
    }
    const visionPanel = document.getElementById("about-vision-container");
    if (visionPanel && !visionPanel.querySelector(".about-vision-mission")) visionPanel.insertAdjacentHTML("beforeend", `<div class="about-vision-mission"><article class="about-vision-card"><h3></h3><h4></h4><p></p></article><article class="about-vision-card"><h3></h3><h4></h4><p></p></article></div>`);
    const visionCards = visionPanel?.querySelectorAll(".about-vision-card");
    if (visionCards?.[0]) { visionCards[0].querySelector("h3").textContent = managed.visionTitle || "Vizyonumuz"; visionCards[0].querySelector("h4").textContent = managed.visionSubtitle || "Geleceğe Yön Veren Değerlerimiz"; visionCards[0].querySelector("p").textContent = managed.visionText || ""; }
    if (visionCards?.[1]) { visionCards[1].querySelector("h3").textContent = managed.missionTitle || "Misyonumuz"; visionCards[1].querySelector("h4").textContent = managed.missionSubtitle || "Sürdürülebilir Değer Üretme Yaklaşımımız"; visionCards[1].querySelector("p").textContent = managed.missionText || ""; }
    const statisticItems = (Array.isArray(managed.statisticItems) ? managed.statisticItems : []).filter((item) => !item.Hidden && String(item.Value || "").trim() !== "");
    if (statisticItems.length) {
      const strip = document.querySelector(".stat-strip");
      strip.style.setProperty("--stat-count", String(statisticItems.length));
      strip.innerHTML = statisticItems.map((item) => {
        const label = item.Label || "";
        const figure = item.IconPath
          ? `<img class="stat-strip__icon" src="${esc(asset(item.IconPath))}" alt="${esc(label)}" loading="lazy">`
          : "";
        return `<div class="stat-strip__item"><div class="stat-strip__num">${esc(item.Value)}</div>${figure}</div>`;
      }).join("");
    }
    text("#panel-tarihce h2", managed.historyTitle); text("#panel-tarihce .corp-panel__head>p:last-child", managed.historyDescription);
    const history = lines(managed.historyItems, 3); if (history.length) document.querySelector("#panel-tarihce .timeline").innerHTML = history.map(([era,title,description]) => `<div class="timeline-item"><div class="timeline-item__era">${esc(era)}</div><div class="timeline-item__text"><h3>${esc(title)}</h3><p>${esc(description)}</p></div></div>`).join("");
    text("#panel-degerlerimiz h2", managed.valuesTitle); text("#panel-degerlerimiz .corp-panel__head>p:last-child", managed.valuesDescription);
    const values = lines(managed.values, 2); if (values.length) document.querySelector("#panel-degerlerimiz .values-grid").innerHTML = values.map(([title,description], index) => `<div class="value-card"><div class="value-card__num">${String(index + 1).padStart(2,"0")}</div><h3>${esc(title)}</h3><p>${esc(description)}</p></div>`).join("");
    text("#panel-uretim .section-title", managed.productionTitle); const productionText = document.querySelector("#panel-uretim .split__text>p:not(.section-label)"); if (managed.productionText && productionText) productionText.textContent = managed.productionText;
    const production = lines(managed.productionItems, 2); if (production.length) document.querySelector("#panel-uretim .values-grid").innerHTML = production.map(([title,description]) => `<div class="value-card"><h3>${esc(title)}</h3><p>${esc(description)}</p></div>`).join("");
    text("#panel-odullar h2", managed.awardsTitle); text("#panel-odullar .corp-panel__head>p:last-child", managed.awardsDescription); const awards = lines(managed.awards, 2); if (awards.length) document.querySelector("#panel-odullar .award-list").innerHTML = awards.map(([title,status]) => `<div class="award-item"><h3>${esc(title)}</h3><span>${esc(status)}</span></div>`).join("");
    text("#panel-sertifikalar h2", managed.certificatesTitle); text("#panel-sertifikalar .corp-panel__head>p:last-child", managed.certificatesDescription);
    text("#panel-isbirlikleri h2", managed.partnershipsTitle); text("#panel-isbirlikleri .corp-panel__head>p:last-child", managed.partnershipsDescription); const partners = lines(managed.partnerships, 2); if (partners.length) document.querySelector("#panel-isbirlikleri .partner-grid").innerHTML = partners.map(([title,description]) => `<div class="partner-card"><h3>${esc(title)}</h3><p>${esc(description)}</p></div>`).join("");
    text("#panel-bilgitoplumu h2", managed.informationTitle); text("#panel-bilgitoplumu .corp-panel__head>p:last-child", managed.informationDescription);
  }

  async function ngSlimManagedHero() {
    const description = document.querySelector("[data-managed-slim-hero-description]");
    if (!description) return;
    try {
      const managedPage = await get("pages/ng-slim");
      const block = managedPage.blocks?.find((item) => item.blockType === "TextImage" && item.content?.trim());
      if (!block) return;
      description.textContent = block.content.trim();
    } catch {
      /* CMS kaydı veya API erişimi yoksa HTML içindeki güvenli varsayılan metni koru. */
    }
  }

  async function genericPage() {
    const fileSlug = page.replace(/\.html$/i, "");
    const available = await get("pages");
    const match = available.find((item) => { const seo = String(item.seoUrl || "").replace(/^\/+|\/+$/g, ""); return seo === fileSlug || seo.endsWith(`/${fileSlug}`); });
    if (!match) return;
    const route = String(match.seoUrl).replace(/^\/+|\/+$/g, "").split("/").map(encodeURIComponent).join("/");
    const x = await get(`pages/${route}`);
    setMeta(x); setText("main h1", x.title);
    const sections = [...document.querySelectorAll("main section")].filter((section) => !section.querySelector("nav") && !section.classList.contains("page-header"));
    x.blocks.forEach((block, index) => {
      const section = sections[index]; if (!section) return;
      const heading = section.querySelector("h2, h3"); if (heading && block.title) heading.textContent = block.title;
      const content = section.querySelector("p:not(.eyebrow):not(.section-label)"); if (content && block.content) content.innerHTML = block.content;
      const image = section.querySelector("img"); if (image && block.imageUrl) { image.src = asset(block.imageUrl); image.alt = block.title || x.title; }
      const iframe = section.querySelector("iframe"); if (iframe && block.videoEmbedUrl) iframe.src = block.videoEmbedUrl;
    });
  }

  function bindForms() {
    const form = document.querySelector(".pd-form"); if (!form || form.dataset.cmsBound) return; form.dataset.cmsBound = "true";
    const controls = form.querySelectorAll("input, select, textarea"); const button = form.querySelector("button"); if (!button) return;
    button.type = "submit";
    if (!form.querySelector('input[type="checkbox"]')) { const label = document.createElement("label"); label.className = "form-check"; label.innerHTML = '<input type="checkbox" required><span>KVKK Aydınlatma Metni’ni okudum ve kabul ediyorum.</span>'; form.insertBefore(label, button); }
    form.addEventListener("submit", async (event) => { event.preventDefault(); if (!form.reportValidity()) return; button.disabled = true; const previous = button.textContent;
      try { const result = await post("forms", { formType: "RequestInformation", fullName: controls[0]?.value || "", email: controls[1]?.value || "", phone: controls[2]?.value || "", subject: controls[3]?.value || "Ürün bilgi talebi", message: form.querySelector("textarea")?.value || "Ürün hakkında bilgi talebi", consentAccepted: form.querySelector('input[type="checkbox"]')?.checked === true, productCode: document.getElementById("pdProductCode")?.textContent, productName: document.querySelector(".pd-title")?.textContent }); button.textContent = result.message; form.reset(); }
      catch (error) { button.textContent = error.message; } finally { setTimeout(() => { button.textContent = previous; button.disabled = false; }, 4000); }
    });
    if (page !== "kariyer.html") return;
  }

  function bindCareerForm() {
    if (page !== "kariyer.html") return;
    const form = [...document.querySelectorAll("form.filter-form")].find((item) => item.querySelector('input[type="email"]')); if (!form || form.dataset.cmsBound) return;
    form.dataset.cmsBound = "true"; const button = form.querySelector("button"); if (!button) return; button.type = "submit"; const cvInput = form.querySelector('input[type="file"]'); if (cvInput) cvInput.required = true;
    form.addEventListener("submit", async (event) => { event.preventDefault(); const consent = form.querySelector('input[type="checkbox"]'); if (!consent?.checked) { consent?.setCustomValidity("Başvuruyu göndermek için KVKK onayını vermelisiniz."); consent?.reportValidity(); return; } consent.setCustomValidity(""); if (!form.reportValidity()) return; const inputs = form.querySelectorAll("input"); const select = form.querySelector("select"); const textarea = form.querySelector("textarea"); const previous = button.textContent; button.disabled = true;
      try { const data = new FormData(); data.set("fullName", inputs[0]?.value || ""); data.set("email", form.querySelector('input[type="email"]')?.value || ""); data.set("phone", form.querySelector('input[type="tel"]')?.value || ""); data.set("department", select?.value || "Genel başvuru"); data.set("message", textarea?.value || "Genel kariyer başvurusu"); data.set("consentAccepted", String(form.querySelector('input[type="checkbox"]')?.checked === true)); const cv = form.querySelector('input[type="file"]')?.files?.[0]; if (cv) data.set("cv", cv); const response = await fetch(`${API_ROOT}/forms/career`, { method: "POST", headers: { Accept: "application/json" }, body: data }); const result = await response.json(); if (!response.ok) throw new Error(result.message || "Başvuru gönderilemedi."); button.textContent = result.message; form.reset(); }
      catch (error) { button.textContent = error.message; } finally { setTimeout(() => { button.textContent = previous; button.disabled = false; }, 4000); }
    });
  }

  async function syncLanguages() {
    const available = await get("languages", {}); if (!available.length) return;
    const codes = new Set(available.map((item) => item.code.toLowerCase()));
    document.querySelectorAll(".lang__dd button").forEach((button) => { const code = (button.dataset.lang || button.textContent).trim().toLowerCase(); button.hidden = !codes.has(code); });
  }

  async function headquarters() {
    const records = await get("dealers");
    const item = records.find((entry) => entry.category === "GeneralHeadquarters");
    if (!item) return;
    setText("#hq-location-title", item.name);
    const location = item.address || [item.district, item.city].filter(Boolean).join(", ");
    setText(".hq-address", location);
    const contact = document.querySelector(".hq-contact");
    if (contact) {
      contact.replaceChildren();
      if (item.phone) { const phone = document.createElement("a"); phone.href = `tel:${item.phone.replace(/\s+/g, "")}`; phone.textContent = item.phone; contact.appendChild(phone); }
      if (item.email) { const email = document.createElement("a"); email.href = `mailto:${item.email}`; email.textContent = item.email; contact.appendChild(email); }
    }
    const routeButton = document.querySelector("[data-headquarters-route]");
    if (routeButton) routeButton.dataset.destination = location;
    const frame = document.querySelector(".hq-map iframe");
    if (frame) frame.src = `https://www.google.com/maps?q=${encodeURIComponent(location)}&output=embed`;
  }

  async function factoriesCopy() {
    const factoryList = document.querySelector(".factories-list");
    try {
      const managedFactories = (await get("dealers")).filter((item) => item.category === "Factory");
      if (factoryList && managedFactories.length) {
        factoryList.innerHTML = managedFactories.map((item, index) => {
          const location = [item.district, item.city].filter(Boolean).join(" / ");
          const destination = item.address || location;
          const areaLabel = item.name.toLocaleLowerCase("tr").includes("umut 2") ? "Çalca üretim bölgesi" : "Kütahya üretim kampüsü";
          return `<article class="factory-card"${destination ? ` data-managed-destination="${esc(destination)}"` : ""}><span class="factory-card__number">${String(index + 1).padStart(2, "0")}</span><h3>${esc(item.name)}</h3><address>${item.address ? esc(item.address) : `${esc(areaLabel)}<br>${esc(location)}`}</address>${item.address ? `<a class="factory-card__map" href="#">Haritada Aç →</a>` : '<p class="factories-note">Açık adres henüz eklenmemiştir.</p>'}</article>`;
        }).join("");
      }
    } catch { /* API erişilemezse mevcut statik fabrika içeriğini koru. */ }
    const routeDestinations = {
      "01": "Kütahya Merkez/Kütahya",
      "02": "Kütahya Merkez/Kütahya",
      "03": "Kütahya Merkez/Kütahya",
      "04": "Kütahya Merkez/Kütahya",
      "05": "Çalca, 43100 Kütahya Merkez/Kütahya",
      "06": "4. Cadde, Çalca OSB, 43100 Kütahya Merkez/Kütahya",
      "07": "Çalca, 43100 Kütahya Merkez/Kütahya",
      "08": "Çalca, 43100 Kütahya Merkez/Kütahya",
      "09": "Çalca, 43100 Kütahya Merkez/Kütahya"
    };
    document.querySelectorAll(".factories-note").forEach((note) => {
      if (note.textContent.includes("rota bağlantısı eklenmemiştir")) note.textContent = "Açık adres henüz eklenmemiştir.";
      if (note.textContent.includes("Kesin adres bilgisi resmî içerik yönetimi sırasında eklenecektir")) note.textContent = "Açık adres henüz eklenmemiştir.";
    });
    document.querySelectorAll(".factory-card").forEach((card) => {
      const address = card.querySelector("address"); if (!address) return;
      const factoryNumber = card.querySelector(".factory-card__number")?.textContent.trim();
      let mapLink = card.querySelector(".factory-card__map");
      const addressLines = address.innerText.split(/\n+/).map((line) => line.trim()).filter(Boolean);
      const note = card.querySelector(".factories-note");
      const fallbackLocation = addressLines.at(-1) || "Merkez / Kütahya";
      if (!card.dataset.managedDestination && ["01", "02", "03", "04"].includes(factoryNumber)) {
        address.replaceChildren(
          document.createTextNode("Kütahya üretim kampüsü"),
          document.createElement("br"),
          document.createTextNode("Açık adres henüz eklenmemiştir, Merkez / Kütahya")
        );
      }
      if (!mapLink && note) {
        address.replaceChildren(
          document.createTextNode(addressLines[0] || "Kütahya üretim kampüsü"),
          document.createElement("br"),
          document.createTextNode(`Açık adres henüz eklenmemiştir, ${fallbackLocation}`)
        );
        note.remove();
      }
      const destination = card.dataset.managedDestination || routeDestinations[factoryNumber] || "Çalca, 43100 Kütahya Merkez/Kütahya";
      if (!mapLink) {
        mapLink = document.createElement("a");
        mapLink.className = "factory-card__map";
        mapLink.target = "_blank";
        mapLink.rel = "noopener";
        address.insertAdjacentElement("afterend", mapLink);
      }
      mapLink.href = `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(destination)}&travelmode=driving&dir_action=navigate`;
      mapLink.textContent = "Haritada Aç →";
    });
  }

  function managedVideoSource(url) {
    try {
      const parsed = new URL(url, location.href);
      if (!/^https?:$/.test(parsed.protocol)) return null;
      const host = parsed.hostname.replace(/^www\./, "");
      if (host === "youtu.be") return `https://www.youtube.com/embed/${encodeURIComponent(parsed.pathname.slice(1))}`;
      if (host.endsWith("youtube.com")) {
        const videoId = parsed.searchParams.get("v") || parsed.pathname.split("/").filter(Boolean).pop();
        return videoId ? `https://www.youtube.com/embed/${encodeURIComponent(videoId)}` : null;
      }
      if (host.endsWith("vimeo.com")) {
        const videoId = parsed.pathname.split("/").filter(Boolean).pop();
        return videoId ? `https://player.vimeo.com/video/${encodeURIComponent(videoId)}` : null;
      }
      return parsed.href;
    } catch { return null; }
  }

  async function kutahyaManagedVideo() {
    const section = document.querySelector("[data-managed-video-section]");
    const documentsSection = document.querySelector("[data-managed-video-documents]");
    if (!section) return;
    section.hidden = true;
    documentsSection?.classList.remove("has-managed-video");
    try {
      const managedPage = await get("pages/ng-kutahya-seramik-video");
      const block = managedPage.blocks?.find((item) => item.blockType === "VideoEmbed" && item.videoEmbedUrl);
      const source = block ? managedVideoSource(block.videoEmbedUrl) : null;
      if (!block || !source) return;

      const media = section.querySelector("[data-managed-video]");
      if (!media) return;
      media.replaceChildren();
      if (/\.(mp4|webm|ogg)(?:$|[?#])/i.test(source)) {
        const video = document.createElement("video");
        video.controls = true;
        video.preload = "metadata";
        video.src = source;
        video.setAttribute("playsinline", "");
        media.appendChild(video);
      } else {
        const frame = document.createElement("iframe");
        frame.src = source;
        frame.title = block.title || "NG Kütahya Seramik videosu";
        frame.loading = "lazy";
        frame.allow = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share";
        frame.allowFullscreen = true;
        media.appendChild(frame);
      }
      setText("[data-managed-video-eyebrow]", managedPage.metaTitle || "");
      setText("[data-managed-video-title]", block.title || "");
      setText("[data-managed-video-description]", block.content || "");
      section.hidden = false;
      documentsSection?.classList.add("has-managed-video");
    } catch {
      section.hidden = true;
      documentsSection?.classList.remove("has-managed-video");
    }
  }

  function pruneFooterLinks() {
    document.querySelectorAll(".footer__col a").forEach((a) => {
      const text = a.textContent.trim();
      if (text === "İş Birlikleri" || text === "Çevresel Sorumluluk") a.remove();
    });
  }

  const jobs = { "index.html": home, "index-koleksiyonlar.html": collections, "urun-detay.html": productDetail, "ng-kutahya-seramik.html": () => Promise.all([brandHomeSurfaces(), kutahyaManagedVideo()]), "ng-slim.html": () => Promise.all([brandHomeSurfaces(), ngSlimManagedHero()]), "ng-stone.html": brandHomeSurfaces, "ng-performa.html": brandHomeSurfaces, "projeler.html": projects, "proje-detay.html": projectDetail, "blog.html": blogs, "blog-detay.html": blogDetail, "haberler.html": news, "haber-detay.html": newsDetail, "teknik-dokumanlar.html": documents, "hakkimizda.html": () => Promise.all([aboutManagedContent(), aboutCertificates()]), "satis-noktalari.html": dealers, "bayi-detay.html": dealerDetail, "genel-merkez.html": headquarters, "fabrikalar.html": factoriesCopy };
  document.addEventListener("DOMContentLoaded", () => { pruneFooterLinks(); bindForms(); bindCareerForm(); applyTypesOnlyLayout(); syncLanguages().catch(() => {}); Promise.resolve(jobs[page] ? jobs[page]() : (/-koleksiyonlar\.html$/.test(page) ? brandProducts() : (/-yuzeyler\.html$/.test(page) ? categories() : genericPage()))).then(applyTypesOnlyLayout).catch((error) => {
    const loading = document.getElementById("allCollectionsLoading");
    const empty = document.getElementById("allCollectionsEmpty");
    if (loading) loading.hidden = true;
    if (empty) { empty.dataset.cmsReady = "true"; empty.hidden = false; empty.textContent = "Koleksiyonlar yüklenemedi. Lütfen tekrar deneyin."; }
    console.error("CMS bağlantısı kurulamadı:", error);
  }); });
  window.NGCms = { get, post, apiRoot: API_ROOT, language };
})();
