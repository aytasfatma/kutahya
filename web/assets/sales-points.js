document.addEventListener("DOMContentLoaded", () => {
  const dealerTypeButton = document.querySelector('[data-sales-type="dealer"]');
  if (dealerTypeButton) dealerTypeButton.textContent = "Satış Noktaları";

  const countryCities = {
    "Türkiye": ["İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Kütahya", "Adana", "Konya", "Gaziantep", "Samsun"],
    "Almanya": ["Berlin", "Hamburg", "Münih", "Köln", "Frankfurt", "Stuttgart", "Düsseldorf", "Leipzig", "Dortmund", "Bremen"],
    "Fransa": ["Paris", "Marsilya", "Lyon", "Toulouse", "Nice", "Nantes", "Strazburg", "Montpellier", "Bordeaux", "Lille"],
    "Birleşik Krallık": ["Londra", "Birmingham", "Manchester", "Glasgow", "Liverpool", "Leeds", "Edinburgh", "Bristol", "Sheffield", "Cardiff"],
    "İtalya": ["Roma", "Milano", "Napoli", "Torino", "Palermo", "Cenova", "Bologna", "Floransa", "Bari", "Venedik"],
    "İspanya": ["Madrid", "Barselona", "Valensiya", "Sevilla", "Zaragoza", "Malaga", "Murcia", "Palma", "Bilbao", "Alicante"],
    "Hollanda": ["Amsterdam", "Rotterdam", "Lahey", "Utrecht", "Eindhoven", "Tilburg", "Groningen", "Almere", "Breda", "Nijmegen"],
    "Belçika": ["Brüksel", "Anvers", "Gent", "Charleroi", "Liège", "Brugge", "Namur", "Leuven", "Mons", "Mechelen"],
    "Amerika Birleşik Devletleri": ["New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "Miami"],
    "Birleşik Arap Emirlikleri": ["Dubai", "Abu Dabi", "Şarika", "Acman", "Resü'l-Hayme", "Füceyre", "Ummül-Kayveyn", "Al Ain", "Hatta", "Dibba"]
  };

  // Dosya adları CMS bağlantısı kurulana kadar showroom kayıtlarının tek kaynağıdır.
  const showroomImages = {
    "Afyon Birinci": ["assets/showroomlar/afyon (1) (1).jpeg"],
    "Afyon İkinci": ["assets/showroomlar/afyon (2) (1).jpeg"],
    "Antalya Birinci": ["assets/showroomlar/Antalya showroom(1) (1).jpeg"],
    "Antalya İkinci": ["assets/showroomlar/Antalya showroom(2) (1).jpeg"],
    "Ege Birinci": ["assets/showroomlar/ege (1) (1).jpeg"],
    "Ege İkinci": ["assets/showroomlar/ege (2) (1).jpeg"],
    "Etiler": ["assets/showroomlar/istanbul etiler showroom(1) (1).jpg"],
    "Beşiktaş": ["assets/showroomlar/istanbul etiler showroom(2) (1).jpg"],
    "Sultangazi": ["assets/showroomlar/istanbul etiler showroom(3) (1).jpeg"]
  };

  const showrooms = [
    { id: "1", name: "Afyon Birinci", city: "Afyonkarahisar", district: "Merkez" },
    { id: "2", name: "Afyon İkinci", city: "Afyonkarahisar", district: "Merkez" },
    { id: "3", name: "Antalya Birinci", city: "Antalya", district: "Merkez" },
    { id: "4", name: "Antalya İkinci", city: "Antalya", district: "Merkez" },
    { id: "5", name: "Ege Birinci", city: "İzmir", district: "Ege" },
    { id: "6", name: "Ege İkinci", city: "İzmir", district: "Ege" },
    { id: "7", name: "Etiler", city: "İstanbul", district: "Etiler" },
    { id: "8", name: "Beşiktaş", city: "İstanbul", district: "Beşiktaş" },
    { id: "9", name: "Sultangazi", city: "İstanbul", district: "Sultangazi" }
  ];

  const listRoot = document.querySelector(".sales-list");
  // Listeleme sayfası tamamen CMS API tarafından yönetilir. Bu dosyadaki eski
  // prototip ülke/şehir ve showroom kayıtları yalnızca eski tasarım yedeğidir.
  if (listRoot) return;
  if (listRoot) {
    listRoot.replaceChildren();
    showrooms.forEach((item) => {
      const card = document.createElement("article");
      card.className = "sales-card";
      Object.assign(card.dataset, {
        id: item.id,
        type: "dealer",
        name: item.name,
        country: "Türkiye",
        city: item.city,
        district: item.district,
        address: `${item.district}, ${item.city}`
      });
      card.innerHTML = `<div class="sales-card__top"><h3>${item.name}</h3><span class="sales-badge">Yetkili Bayi</span></div><address>${item.district}, ${item.city}</address><div class="sales-card__actions"><a href="bayi-detay.html" data-dealer-link>Detayları Gör</a><a href="https://maps.google.com/?q=${encodeURIComponent(`${item.name} ${item.district} ${item.city}`)}" target="_blank" rel="noopener">Yol Tarifi</a></div>`;
      listRoot.append(card);
    });

    const replaceOptions = (select, label, values) => {
      if (!select) return;
      select.replaceChildren(new Option(label, ""));
      values.forEach((value) => select.add(new Option(value, value)));
    };
    const countrySelect = document.getElementById("salesCity");
    const citySelect = document.getElementById("salesDistrict");
    const fieldLabels = document.querySelectorAll(".sales-controls .sales-field > span:first-child");
    if (fieldLabels[0]) fieldLabels[0].textContent = "Ülke";
    if (fieldLabels[1]) fieldLabels[1].textContent = "Şehir";
    replaceOptions(countrySelect, "Tüm Ülkeler", Object.keys(countryCities));
    const refreshCities = () => {
      const selectedCountry = countrySelect?.value || "";
      const cities = selectedCountry ? countryCities[selectedCountry] : [...new Set(Object.values(countryCities).flat())];
      replaceOptions(citySelect, "Tüm Şehirler", cities);
    };
    countrySelect?.addEventListener("change", refreshCities);
    refreshCities();
  }

  const page = document.querySelector(".sales-list");
  const city = document.getElementById("salesCity");
  const district = document.getElementById("salesDistrict");
  const search = document.getElementById("salesSearch");
  const types = [...document.querySelectorAll("[data-sales-type]")];
  const reset = document.getElementById("salesReset");
  const count = document.querySelector(".sales-count");
  const empty = document.querySelector(".sales-empty");

  if (page) {
    const cards = [...page.querySelectorAll(".sales-card")];
    const pins = [...document.querySelectorAll(".map-pin")];
    let type = "all";
    const render = () => {
      const query = (search?.value || "").toLocaleLowerCase("tr-TR").trim();
      const selectedCountry = city?.value || "";
      const selectedCity = district?.value || "";
      let visible = 0;
      cards.forEach((card) => {
        const matches = (type === "all" || card.dataset.type === type)
          && (!selectedCountry || card.dataset.country === selectedCountry)
          && (!selectedCity || card.dataset.city === selectedCity)
          && (!query || `${card.dataset.name} ${card.dataset.city} ${card.dataset.district}`.toLocaleLowerCase("tr-TR").includes(query));
        card.hidden = !matches;
        if (matches) visible += 1;
      });
      pins.forEach((pin) => {
        pin.hidden = cards.find((card) => card.dataset.id === pin.dataset.id)?.hidden ?? true;
      });
      if (count) count.textContent = `${visible} satış noktası`;
      if (empty) empty.hidden = visible !== 0;
    };
    types.forEach((button) => button.addEventListener("click", () => {
      type = button.dataset.salesType;
      types.forEach((item) => item.setAttribute("aria-pressed", String(item === button)));
      render();
    }));
    [city, district, search].forEach((input) => input?.addEventListener(input === search ? "input" : "change", render));
    reset?.addEventListener("click", () => {
      type = "all";
      if (city) {
        city.value = "";
        city.dispatchEvent(new Event("change"));
      }
      if (district) district.value = "";
      if (search) search.value = "";
      types.forEach((item, index) => item.setAttribute("aria-pressed", String(index === 0)));
      render();
    });
    pins.forEach((pin) => pin.addEventListener("click", () => {
      pins.forEach((item) => item.classList.remove("active"));
      pin.classList.add("active");
      cards.find((item) => item.dataset.id === pin.dataset.id)?.scrollIntoView({ behavior: "smooth", block: "center" });
    }));
    render();
  }

  document.querySelectorAll("[data-dealer-link]").forEach((link) => {
    link.href = "bayi-detay.html";
    link.addEventListener("click", () => {
      const card = link.closest(".sales-card");
      if (!card) return;
      try {
        sessionStorage.setItem("ngSelectedDealer", JSON.stringify({
          ...card.dataset,
          images: showroomImages[card.dataset.name] || []
        }));
      } catch {}
    });
  });

  if (document.querySelector(".dealer-detail-page")) {
    try {
      const data = JSON.parse(sessionStorage.getItem("ngSelectedDealer") || "null");
      if (data) {
        const name = document.querySelector("[data-dealer-name]");
        const typeLabel = document.querySelector("[data-dealer-type]");
        const address = document.querySelector("[data-dealer-address]");
        const visual = document.querySelector(".dealer-visual");
        if (name) name.textContent = data.name;
        if (typeLabel) typeLabel.textContent = data.type === "showroom" ? "Showroom" : "Yetkili Bayi";
        if (address) address.textContent = data.address || "Adres bilgisi admin panelinden yönetilecektir.";
        const images = data.images?.length ? data.images : (showroomImages[data.name] || []);
        if (visual && images.length) {
          visual.classList.remove("dealer-visual--map");
          const main = document.createElement("img");
          const thumbs = document.createElement("div");
          main.className = "dealer-visual__main";
          main.src = images[0];
          main.alt = `${data.name} showroom görünümü`;
          thumbs.className = "dealer-visual__thumbs";
          images.forEach((src, index) => {
            const button = document.createElement("button");
            const image = document.createElement("img");
            button.type = "button";
            button.setAttribute("aria-label", `${data.name} görsel ${index + 1}`);
            button.classList.toggle("active", index === 0);
            image.src = src;
            image.alt = "";
            button.append(image);
            button.addEventListener("click", () => {
              main.src = src;
              thumbs.querySelectorAll("button").forEach((item) => item.classList.toggle("active", item === button));
            });
            thumbs.append(button);
          });
          visual.replaceChildren(main, thumbs);
        }
      }
    } catch {}
  }
});
