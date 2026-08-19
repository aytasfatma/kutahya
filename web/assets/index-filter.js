(() => {
  if (window.__ngIndexFilterInitialized) return;
  window.__ngIndexFilterInitialized = true;

  if (!document.querySelector('link[href^="assets/index-filter.css"]')) {
    const stylesheet = document.createElement("link");
    stylesheet.rel = "stylesheet";
    stylesheet.href = "assets/index-filter.css";
    document.head.appendChild(stylesheet);
  }

  ["filterToggle", "filterBackdrop", "filterDrawer", "toggle", "backdrop", "drawer"].forEach((id) => {
    document.getElementById(id)?.remove();
  });

  const container = document.createElement("div");
  container.id = "sharedIndexFilter";
  container.innerHTML = `
    <button aria-controls="filterDrawer" aria-expanded="false" class="filter-rail" id="filterToggle" type="button">
      <span>Filtre</span>
      <svg fill="none" height="18" stroke="currentColor" stroke-width="1.4" viewBox="0 0 24 24" width="18" aria-hidden="true"><path d="M4 6h16M7 12h10M10 18h4"></path></svg>
    </button>
    <div class="drawer-backdrop" id="filterBackdrop"></div>
    <aside aria-hidden="true" class="filter-drawer" id="filterDrawer">
      <div class="drawer-head">
        <div><p class="eyebrow">Ürün Keşfi</p><h3>Filtre</h3></div>
        <button aria-label="Filtreyi kapat" id="filterClose" type="button">×</button>
      </div>
      <form class="filter-form">
        <label>Marka<select><option>Tümü</option><option>NG Kütahya Seramik</option><option>NG Slim</option><option>NG Stone</option><option>NG Performa</option></select></label>
        <label>Ürün Adı<input placeholder="Ürün ara" type="search"></label>
        <label>Koleksiyon<select><option>Tümü</option></select></label>
        <label>Ebat<select><option>Tümü</option></select></label>
        <label>Kalınlık<select><option>Tümü</option></select></label>
        <label>Kategori<select><option>Tümü</option></select></label>
        <label>Yüzey<select><option>Tümü</option></select></label>
        <button class="filter-submit" type="button">Ürünleri Göster</button>
      </form>
    </aside>`;
  document.body.appendChild(container);

  const toggle = document.getElementById("filterToggle");
  const drawer = document.getElementById("filterDrawer");
  const backdrop = document.getElementById("filterBackdrop");
  const setOpen = (open) => {
    drawer.classList.toggle("open", open);
    backdrop.classList.toggle("open", open);
    drawer.setAttribute("aria-hidden", String(!open));
    toggle.setAttribute("aria-expanded", String(open));
    document.body.classList.toggle("drawer-open", open);
  };

  toggle.addEventListener("click", () => setOpen(true));
  document.getElementById("filterClose").addEventListener("click", () => setOpen(false));
  backdrop.addEventListener("click", () => setOpen(false));
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") setOpen(false);
  });
})();
