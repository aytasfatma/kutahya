// NG Kütahya Seramik Admin Panel - küçük vanilla JS eklentileri.
// Tabler'ın kendi JS'i sidebar collapse/offcanvas davranışını yönetiyor.
(function () {
  "use strict";

  var sidebarMenu = document.getElementById("sidebar-menu");

  if (sidebarMenu) {
    sidebarMenu.querySelectorAll(".nav-link").forEach(function (link) {
      link.addEventListener("click", function () {
        if (window.innerWidth >= 992) {
          return;
        }

        if (sidebarMenu.classList.contains("show") && window.bootstrap) {
          var collapse = window.bootstrap.Collapse.getOrCreateInstance(sidebarMenu);
          collapse.hide();
        }
      });
    });
  }

  var searchInput = document.getElementById("navbar-search-input");
  if (searchInput) {
    document.addEventListener("keydown", function (event) {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        searchInput.focus();
      }
    });
  }

  function resizeTextarea(textarea) {
    if (!textarea || textarea.offsetParent === null) {
      return;
    }

    textarea.style.height = "auto";
    var maxHeight = textarea.hasAttribute("data-auto-resize-unbounded") ? Number.POSITIVE_INFINITY : 160;
    var nextHeight = Math.min(textarea.scrollHeight, maxHeight);
    textarea.style.height = nextHeight + "px";
    textarea.classList.toggle("is-scrollable", textarea.scrollHeight > maxHeight);
  }

  function resizeTextareas(root) {
    (root || document).querySelectorAll("textarea[data-auto-resize]").forEach(resizeTextarea);
  }

  document.addEventListener("input", function (event) {
    if (event.target && event.target.matches("textarea[data-auto-resize]")) {
      resizeTextarea(event.target);
    }
  });

  function initPageEnhancements() {
    resizeTextareas(document);
    initProductPreview(document);
    initDocumentRelationModal(document);
    initReferenceProductPicker(document);
    initProductImageUploads(document);
    initSubmitLocks(document);
    initProductCreatableComboboxes(document);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initPageEnhancements);
  } else {
    initPageEnhancements();
  }

  window.addEventListener("admin:autosize", function () {
    requestAnimationFrame(function () {
      resizeTextareas(document);
    });
  });

  document.addEventListener("htmx:load", function (event) {
    initProductPreview(event.target || document);
    initDocumentRelationModal(event.target || document);
    initReferenceProductPicker(event.target || document);
    initProductImageUploads(event.target || document);
    initSubmitLocks(event.target || document);
    initProductCreatableComboboxes(event.target || document);
  });

  function initProductPreview(root) {
    var scope = root || document;
    var triggers = scope.querySelectorAll("[data-preview-src]");
    if (!triggers.length) {
      return;
    }

    var modalElement = document.querySelector("[data-product-preview-modal]");
    if (!modalElement) {
      return;
    }

    var bootstrapModal = window.bootstrap && window.bootstrap.Modal
      ? window.bootstrap.Modal.getOrCreateInstance(modalElement)
      : null;
    var imageElement = modalElement.querySelector("[data-product-preview-image]");
    var titleElement = modalElement.querySelector("[data-product-preview-title]");
    var codeElement = modalElement.querySelector("[data-product-preview-code]");
    var errorElement = modalElement.querySelector("[data-product-preview-error]");

    if (!imageElement || !titleElement || !codeElement || !errorElement) {
      return;
    }

    var lastFocusedElement = null;
    var fallbackBackdrop = null;

    function resetPreview() {
      imageElement.classList.remove("d-none");
      errorElement.classList.add("d-none");
      imageElement.removeAttribute("src");
      imageElement.alt = "";
      titleElement.textContent = "Ürün Görseli";
      codeElement.textContent = "";
    }

    function isFallbackOpen() {
      return modalElement.classList.contains("show") && !bootstrapModal;
    }

    function showModal() {
      if (bootstrapModal) {
        bootstrapModal.show();
        return;
      }

      lastFocusedElement = document.activeElement;
      modalElement.style.display = "block";
      modalElement.removeAttribute("aria-hidden");
      modalElement.setAttribute("aria-modal", "true");
      modalElement.setAttribute("role", "dialog");
      modalElement.classList.add("show");
      document.body.classList.add("modal-open");

      fallbackBackdrop = document.createElement("div");
      fallbackBackdrop.className = "modal-backdrop fade show";
      document.body.appendChild(fallbackBackdrop);

      var closeButton = modalElement.querySelector("[data-bs-dismiss='modal']");
      if (closeButton) {
        closeButton.focus();
      }
    }

    function hideModal() {
      if (bootstrapModal) {
        bootstrapModal.hide();
        return;
      }

      modalElement.classList.remove("show");
      modalElement.style.display = "none";
      modalElement.setAttribute("aria-hidden", "true");
      modalElement.removeAttribute("aria-modal");
      modalElement.removeAttribute("role");
      document.body.classList.remove("modal-open");

      if (fallbackBackdrop) {
        fallbackBackdrop.remove();
        fallbackBackdrop = null;
      }

      resetPreview();

      if (lastFocusedElement && typeof lastFocusedElement.focus === "function") {
        lastFocusedElement.focus();
      }
    }

    function showLoadError() {
      imageElement.classList.add("d-none");
      errorElement.classList.remove("d-none");
    }

    function openPreview(src, title, code) {
      if (!src || !src.trim()) {
        return;
      }

      resetPreview();
      titleElement.textContent = title || "Ürün Görseli";
      codeElement.textContent = code ? "Ürün Kodu: " + code : "";
      imageElement.alt = title || code || "Ürün görseli";
      imageElement.src = src;
      showModal();
    }

    imageElement.onerror = showLoadError;

    triggers.forEach(function (trigger) {
      if (trigger.dataset.previewBound === "true") {
        return;
      }

      trigger.dataset.previewBound = "true";
      trigger.addEventListener("click", function () {
        openPreview(
          trigger.getAttribute("data-preview-src") || "",
          trigger.getAttribute("data-preview-title") || "",
          trigger.getAttribute("data-preview-code") || ""
        );
      });
    });

    if (modalElement.dataset.previewInitialized !== "true") {
      modalElement.dataset.previewInitialized = "true";
      modalElement.addEventListener("hidden.bs.modal", resetPreview);
      modalElement.addEventListener("click", function (event) {
        if (event.target === modalElement || event.target.closest("[data-bs-dismiss='modal']")) {
          hideModal();
        }
      });
      document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && isFallbackOpen()) {
          hideModal();
        }
      });
    }
  }

  function initDocumentRelationModal(root) {
    var scope = root || document;
    var triggers = scope.querySelectorAll("[data-document-relation-items]");
    if (!triggers.length) {
      return;
    }

    var modalElement = document.querySelector("[data-document-relation-modal]");
    if (!modalElement) {
      return;
    }

    var bootstrapModal = window.bootstrap && window.bootstrap.Modal
      ? window.bootstrap.Modal.getOrCreateInstance(modalElement)
      : null;
    var titleElement = modalElement.querySelector("[data-document-relation-modal-title]");
    var subtitleElement = modalElement.querySelector("[data-document-relation-modal-subtitle]");
    var listElement = modalElement.querySelector("[data-document-relation-modal-list]");

    if (!titleElement || !subtitleElement || !listElement) {
      return;
    }

    var lastFocusedElement = null;
    var fallbackBackdrop = null;

    function clearList() {
      listElement.replaceChildren();
      titleElement.textContent = "İlişkiler";
      subtitleElement.textContent = "";
    }

    function isFallbackOpen() {
      return modalElement.classList.contains("show") && !bootstrapModal;
    }

    function showModal() {
      if (bootstrapModal) {
        bootstrapModal.show();
        return;
      }

      lastFocusedElement = document.activeElement;
      modalElement.style.display = "block";
      modalElement.removeAttribute("aria-hidden");
      modalElement.setAttribute("aria-modal", "true");
      modalElement.setAttribute("role", "dialog");
      modalElement.classList.add("show");
      document.body.classList.add("modal-open");

      fallbackBackdrop = document.createElement("div");
      fallbackBackdrop.className = "modal-backdrop fade show";
      document.body.appendChild(fallbackBackdrop);

      var closeButton = modalElement.querySelector("[data-bs-dismiss='modal']");
      if (closeButton) {
        closeButton.focus();
      }
    }

    function hideModal() {
      if (bootstrapModal) {
        bootstrapModal.hide();
        return;
      }

      modalElement.classList.remove("show");
      modalElement.style.display = "none";
      modalElement.setAttribute("aria-hidden", "true");
      modalElement.removeAttribute("aria-modal");
      modalElement.removeAttribute("role");
      document.body.classList.remove("modal-open");

      if (fallbackBackdrop) {
        fallbackBackdrop.remove();
        fallbackBackdrop = null;
      }

      clearList();

      if (lastFocusedElement && typeof lastFocusedElement.focus === "function") {
        lastFocusedElement.focus();
      }
    }

    function appendItem(item) {
      var row = document.createElement("div");
      row.className = "document-relation-modal-item";

      var link = document.createElement("a");
      link.className = "document-relation-modal-link";
      link.textContent = item.label || "";
      link.href = item.href || "#";

      var icon = document.createElement("i");
      icon.className = "ti ti-external-link document-relation-modal-link-icon";
      icon.setAttribute("aria-hidden", "true");

      row.append(link, icon);
      listElement.appendChild(row);
    }

    function openRelationModal(trigger) {
      var items = [];
      try {
        items = JSON.parse(trigger.getAttribute("data-document-relation-items") || "[]") || [];
      } catch (error) {
        items = [];
      }

      if (!items.length) {
        return;
      }

      clearList();
      titleElement.textContent = trigger.getAttribute("data-document-relation-kind") || "İlişkiler";
      subtitleElement.textContent = trigger.getAttribute("data-document-relation-title") || "";
      items.forEach(appendItem);
      showModal();
    }

    triggers.forEach(function (trigger) {
      if (trigger.dataset.documentRelationBound === "true") {
        return;
      }

      trigger.dataset.documentRelationBound = "true";
      trigger.addEventListener("click", function () {
        openRelationModal(trigger);
      });
    });

    if (modalElement.dataset.documentRelationInitialized !== "true") {
      modalElement.dataset.documentRelationInitialized = "true";
      modalElement.addEventListener("hidden.bs.modal", clearList);
      modalElement.addEventListener("click", function (event) {
        if (event.target === modalElement || event.target.closest("[data-bs-dismiss='modal']")) {
          hideModal();
        }
      });
      document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && isFallbackOpen()) {
          hideModal();
        }
      });
    }
  }

  function initReferenceProductPicker(root) {
    var scope = root || document;
    var pickers = scope.querySelectorAll("[data-reference-product-picker]");
    if (!pickers.length) {
      return;
    }

    pickers.forEach(function (picker) {
      if (picker.dataset.referenceProductPickerBound === "true") {
        return;
      }

      picker.dataset.referenceProductPickerBound = "true";

      var searchInput = picker.querySelector("[data-reference-product-search]");
      var selectAllButton = picker.querySelector("[data-reference-product-select-all]");
      var clearButton = picker.querySelector("[data-reference-product-clear]");
      var countElement = picker.querySelector("[data-reference-product-count]");
      var emptyElement = picker.querySelector("[data-reference-product-empty]");
      var options = Array.prototype.slice.call(picker.querySelectorAll("[data-reference-product-option]"));

      function getCheckboxes(visibleOnly) {
        return options
          .filter(function (option) {
            return !visibleOnly || !option.classList.contains("is-filtered");
          })
          .map(function (option) {
            return option.querySelector("input[type='checkbox']");
          })
          .filter(function (checkbox) {
            return checkbox && !checkbox.disabled;
          });
      }

      function updateCount() {
        var selectedCount = 0;

        options.forEach(function (option) {
          var checkbox = option.querySelector("input[type='checkbox']");
          var isSelected = !!(checkbox && checkbox.checked);
          option.classList.toggle("is-selected", isSelected);
          if (isSelected) {
            selectedCount += 1;
          }
        });

        if (countElement) {
          countElement.textContent = selectedCount + " seçili";
        }
      }

      function applyFilter() {
        var query = (searchInput && searchInput.value ? searchInput.value : "").trim().toLocaleLowerCase("tr-TR");
        var visibleCount = 0;

        options.forEach(function (option) {
          var label = (option.getAttribute("data-reference-product-label") || option.textContent || "").toLocaleLowerCase("tr-TR");
          var visible = !query || label.indexOf(query) !== -1;
          option.classList.toggle("is-filtered", !visible);
          if (visible) {
            visibleCount += 1;
          }
        });

        if (emptyElement) {
          emptyElement.classList.toggle("d-none", visibleCount !== 0);
        }
      }

      if (searchInput) {
        searchInput.addEventListener("input", applyFilter);
      }

      if (selectAllButton) {
        selectAllButton.addEventListener("click", function () {
          getCheckboxes(false).forEach(function (checkbox) {
            checkbox.checked = true;
          });
          updateCount();
        });
      }

      if (clearButton) {
        clearButton.addEventListener("click", function () {
          getCheckboxes(false).forEach(function (checkbox) {
            checkbox.checked = false;
          });
          updateCount();
        });
      }

      getCheckboxes(false).forEach(function (checkbox) {
        checkbox.addEventListener("change", updateCount);
      });

      applyFilter();
      updateCount();
    });
  }

  function initSubmitLocks(root) {
    var scope = root || document;

    scope.querySelectorAll("[data-submit-lock]").forEach(function (form) {
      if (form.dataset.submitLockBound === "true") {
        return;
      }

      form.dataset.submitLockBound = "true";
      form.addEventListener("submit", function () {
        var button = form.querySelector("[data-submit-lock-button]");
        var label = form.querySelector("[data-submit-lock-label]");
        if (button) {
          button.disabled = true;
          button.classList.add("disabled");
        }
        if (label) {
          label.textContent = "Kaydediliyor...";
        }
      });
    });
  }

  function initProductImageUploads(root) {
    var scope = root || document;
    var maxBytes = 5 * 1024 * 1024;
    var allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    scope.querySelectorAll("[data-product-image-upload]").forEach(function (component) {
      if (component.dataset.productImageUploadBound === "true") {
        return;
      }

      component.dataset.productImageUploadBound = "true";

      var input = component.querySelector("[data-product-image-input]");
      var dropzone = component.querySelector("[data-product-image-dropzone]");
      var list = component.querySelector("[data-product-image-preview-list]");
      var empty = component.querySelector("[data-product-image-empty]");
      var files = [];
      var maxFiles = Number(input && input.dataset.maxFiles) || 6;

      if (!input || !dropzone || !list) {
        return;
      }

      function extensionOf(fileName) {
        var dot = fileName.lastIndexOf(".");
        return dot >= 0 ? fileName.slice(dot).toLowerCase() : "";
      }

      function formatBytes(bytes) {
        if (bytes >= 1024 * 1024) {
          return (bytes / (1024 * 1024)).toFixed(1) + " MB";
        }
        return Math.max(1, Math.round(bytes / 1024)) + " KB";
      }

      function setInputFiles() {
        var transfer = new DataTransfer();
        files.forEach(function (file) {
          transfer.items.add(file);
        });
        input.files = transfer.files;
      }

      function render() {
        Array.prototype.slice.call(list.querySelectorAll("[data-product-image-preview-card]")).forEach(function (card) {
          var url = card.getAttribute("data-object-url");
          if (url) {
            URL.revokeObjectURL(url);
          }
          card.remove();
        });

        if (empty) {
          empty.hidden = files.length > 0;
        }

        files.forEach(function (file, index) {
          var objectUrl = URL.createObjectURL(file);
          var card = document.createElement("article");
          card.className = "product-image-preview-card";
          card.setAttribute("data-product-image-preview-card", "");
          card.setAttribute("data-object-url", objectUrl);

          var image = document.createElement("img");
          image.className = "product-image-preview-thumb";
          image.src = objectUrl;
          image.alt = file.name + " onizleme";
          card.appendChild(image);

          var body = document.createElement("div");
          body.className = "product-image-preview-body";

          var info = document.createElement("div");
          info.className = "min-w-0";

          var name = document.createElement("div");
          name.className = "product-image-preview-name";
          name.textContent = file.name;
          info.appendChild(name);

          var meta = document.createElement("div");
          meta.className = "product-image-preview-meta";
          meta.textContent = (index === 0 ? "Ana gorsel · " : "Sira " + (index + 1) + " · ") + formatBytes(file.size);
          info.appendChild(meta);
          body.appendChild(info);

          var remove = document.createElement("button");
          remove.type = "button";
          remove.className = "btn btn-sm btn-ghost-danger";
          remove.setAttribute("aria-label", file.name + " gorselini kaldir");
          remove.textContent = "Kaldir";
          remove.addEventListener("click", function () {
            files.splice(index, 1);
            setInputFiles();
            render();
          });
          body.appendChild(remove);

          card.appendChild(body);
          list.appendChild(card);
        });
      }

      function addFiles(fileList) {
        Array.prototype.slice.call(fileList || []).forEach(function (file) {
          var extension = extensionOf(file.name);
          if (allowedExtensions.indexOf(extension) === -1 || file.size > maxBytes) {
            return;
          }
          var duplicate = files.some(function (existing) {
            return existing.name === file.name && existing.size === file.size && existing.lastModified === file.lastModified;
          });
          if (!duplicate) {
            if (files.length < maxFiles) {
              files.push(file);
            }
          }
        });

        setInputFiles();
        render();
      }

      input.addEventListener("change", function () {
        addFiles(input.files);
      });

      dropzone.addEventListener("keydown", function (event) {
        if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          input.click();
        }
      });

      ["dragenter", "dragover"].forEach(function (eventName) {
        dropzone.addEventListener(eventName, function (event) {
          event.preventDefault();
          dropzone.classList.add("is-dragover");
        });
      });

      ["dragleave", "drop"].forEach(function (eventName) {
        dropzone.addEventListener(eventName, function (event) {
          event.preventDefault();
          dropzone.classList.remove("is-dragover");
        });
      });

      dropzone.addEventListener("drop", function (event) {
        addFiles(event.dataTransfer ? event.dataTransfer.files : []);
      });

      render();
    });
  }

  function initProductCreatableComboboxes(root) {
    var scope = root || document;

    scope.querySelectorAll("[data-product-combobox]").forEach(function (combobox) {
      if (combobox.dataset.productComboboxBound === "true") {
        return;
      }

      combobox.dataset.productComboboxBound = "true";

      var input = combobox.querySelector("[data-product-combobox-input]");
      var menu = combobox.querySelector("[data-product-combobox-menu]");
      var toggle = combobox.querySelector("[data-product-combobox-toggle]");
      var options = Array.prototype.slice.call(combobox.querySelectorAll("[data-product-combobox-option]"));
      var createOption = combobox.querySelector("[data-product-combobox-create]");
      var empty = combobox.querySelector("[data-product-combobox-empty]");
      var activeIndex = -1;

      if (!input || !menu) {
        return;
      }

      function normalize(value) {
        return (value || "").trim().replace(/\s+/g, " ");
      }

      function isCreatableValue(value) {
        var normalized = normalize(value);
        return !!normalized && normalized !== "-" && normalized.indexOf("<") === -1;
      }

      function getOptionValue(option) {
        return option.hasAttribute("data-value") ? option.getAttribute("data-value") : option.textContent;
      }

      function openMenu() {
        combobox.classList.add("is-open");
        menu.hidden = false;
        input.setAttribute("aria-expanded", "true");
        filterOptions();
      }

      function closeMenu() {
        combobox.classList.remove("is-open");
        menu.hidden = true;
        input.setAttribute("aria-expanded", "false");
        setActiveOption(-1);
      }

      function getVisibleOptions() {
        return options
          .concat(createOption ? [createOption] : [])
          .filter(function (option) {
            return option && !option.classList.contains("d-none");
          });
      }

      function setActiveOption(index) {
        var visibleOptions = getVisibleOptions();
        visibleOptions.forEach(function (option) {
          option.classList.remove("is-active");
        });

        activeIndex = visibleOptions.length ? Math.max(0, Math.min(index, visibleOptions.length - 1)) : -1;
        if (activeIndex >= 0) {
          visibleOptions[activeIndex].classList.add("is-active");
          visibleOptions[activeIndex].scrollIntoView({ block: "nearest" });
        }
      }

      function filterOptions() {
        var query = normalize(input.value).toLocaleLowerCase("tr-TR");
        var visibleCount = 0;
        var exactMatch = false;

        options.forEach(function (option) {
          var value = normalize(getOptionValue(option));
          var matches = !query || value.toLocaleLowerCase("tr-TR").indexOf(query) !== -1;
          option.classList.toggle("d-none", !matches);
          if (matches) {
            visibleCount += 1;
          }
          if (query && value.toLocaleLowerCase("tr-TR") === query) {
            exactMatch = true;
          }
        });

        var canCreate = isCreatableValue(input.value) && !exactMatch;
        if (createOption) {
          createOption.classList.toggle("d-none", !canCreate);
          createOption.textContent = canCreate ? '"' + normalize(input.value) + '" olarak kullan' : "";
        }
        if (empty) {
          empty.classList.toggle("d-none", visibleCount !== 0 || canCreate);
        }
        setActiveOption(getVisibleOptions().length ? 0 : -1);
      }

      function choose(value) {
        var normalized = normalize(value);
        if (!normalized || normalized === "-" || normalized.indexOf("<") !== -1) {
          input.value = "";
          input.dispatchEvent(new Event("change", { bubbles: true }));
          closeMenu();
          return;
        }

        input.value = normalized;
        input.dispatchEvent(new Event("change", { bubbles: true }));
        closeMenu();
      }

      input.addEventListener("focus", openMenu);
      input.addEventListener("input", function () {
        openMenu();
        filterOptions();
      });
      input.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
          closeMenu();
        }
        if (event.key === "ArrowDown") {
          event.preventDefault();
          openMenu();
          setActiveOption(activeIndex + 1);
        }
        if (event.key === "ArrowUp") {
          event.preventDefault();
          openMenu();
          setActiveOption(activeIndex <= 0 ? getVisibleOptions().length - 1 : activeIndex - 1);
        }
        if (event.key === "Enter" && combobox.classList.contains("is-open")) {
          var visibleOptions = getVisibleOptions();
          var selectedOption = visibleOptions[activeIndex] || visibleOptions[0];
          if (selectedOption) {
            event.preventDefault();
            choose(selectedOption === createOption ? input.value : getOptionValue(selectedOption));
          }
        }
      });

      if (toggle) {
        toggle.addEventListener("click", function () {
          if (combobox.classList.contains("is-open")) {
            closeMenu();
          } else {
            input.focus();
            openMenu();
          }
        });
      }

      options.forEach(function (option) {
        option.addEventListener("mousedown", function (event) {
          event.preventDefault();
          choose(getOptionValue(option));
        });
        option.addEventListener("click", function () {
          choose(getOptionValue(option));
        });
      });

      if (createOption) {
        createOption.addEventListener("mousedown", function (event) {
          event.preventDefault();
          choose(input.value);
        });
        createOption.addEventListener("click", function () {
          choose(input.value);
        });
      }
    });

    scope.querySelectorAll("[data-product-multi-combobox]").forEach(function (combobox) {
      if (combobox.dataset.productMultiComboboxBound === "true") {
        return;
      }

      combobox.dataset.productMultiComboboxBound = "true";

      var hidden = combobox.querySelector("[data-product-multi-combobox-value]");
      var input = combobox.querySelector("[data-product-multi-combobox-input]");
      var menu = combobox.querySelector("[data-product-multi-combobox-menu]");
      var toggle = combobox.querySelector("[data-product-multi-combobox-toggle]");
      var chips = combobox.querySelector("[data-product-multi-combobox-chips]");
      var options = Array.prototype.slice.call(combobox.querySelectorAll("[data-product-multi-combobox-option]"));
      var createOption = combobox.querySelector("[data-product-multi-combobox-create]");
      var empty = combobox.querySelector("[data-product-multi-combobox-empty]");
      var activeIndex = -1;

      if (!hidden || !input || !menu || !chips) {
        return;
      }

      function normalize(value) {
        return (value || "").trim().replace(/\s+/g, " ");
      }

      function isCreatableValue(value) {
        var normalized = normalize(value);
        return !!normalized && normalized !== "-" && normalized.indexOf("<") === -1;
      }

      function selectedValues() {
        return Array.prototype.slice.call(chips.querySelectorAll(".product-combobox-chip"))
          .map(function (chip) { return normalize(chip.getAttribute("data-value")); })
          .filter(Boolean);
      }

      function syncHidden() {
        hidden.value = selectedValues().join(", ");
      }

      function hasSelected(value) {
        var normalized = normalize(value).toLocaleLowerCase("tr-TR");
        return selectedValues().some(function (selected) {
          return selected.toLocaleLowerCase("tr-TR") === normalized;
        });
      }

      function renderChip(value) {
        var normalized = normalize(value);
        if (!normalized || hasSelected(normalized)) {
          return;
        }

        var chip = document.createElement("span");
        chip.className = "product-combobox-chip";
        chip.setAttribute("data-value", normalized);

        var label = document.createElement("span");
        label.textContent = normalized;
        chip.appendChild(label);

        var remove = document.createElement("button");
        remove.type = "button";
        remove.setAttribute("aria-label", normalized + " kaldır");
        remove.innerHTML = '<i class="ti ti-x" aria-hidden="true"></i>';
        remove.addEventListener("click", function () {
          chip.remove();
          syncHidden();
          filterOptions();
        });
        chip.appendChild(remove);

        chips.appendChild(chip);
        syncHidden();
      }

      function bindExistingRemovers() {
        chips.querySelectorAll("[data-product-multi-combobox-remove]").forEach(function (button) {
          button.addEventListener("click", function () {
            var chip = button.closest(".product-combobox-chip");
            if (chip) {
              chip.remove();
              syncHidden();
              filterOptions();
            }
          });
        });
      }

      function openMenu() {
        combobox.classList.add("is-open");
        menu.hidden = false;
        input.setAttribute("aria-expanded", "true");
        filterOptions();
      }

      function closeMenu() {
        combobox.classList.remove("is-open");
        menu.hidden = true;
        input.setAttribute("aria-expanded", "false");
        setActiveOption(-1);
      }

      function getVisibleOptions() {
        return options
          .concat(createOption ? [createOption] : [])
          .filter(function (option) {
            return option && !option.classList.contains("d-none");
          });
      }

      function setActiveOption(index) {
        var visibleOptions = getVisibleOptions();
        visibleOptions.forEach(function (option) {
          option.classList.remove("is-active");
        });

        activeIndex = visibleOptions.length ? Math.max(0, Math.min(index, visibleOptions.length - 1)) : -1;
        if (activeIndex >= 0) {
          visibleOptions[activeIndex].classList.add("is-active");
          visibleOptions[activeIndex].scrollIntoView({ block: "nearest" });
        }
      }

      function filterOptions() {
        var query = normalize(input.value).toLocaleLowerCase("tr-TR");
        var visibleCount = 0;
        var exactMatch = false;

        options.forEach(function (option) {
          var value = normalize(option.getAttribute("data-value") || option.textContent);
          var matches = !query || value.toLocaleLowerCase("tr-TR").indexOf(query) !== -1;
          var hiddenBySelected = hasSelected(value);
          option.classList.toggle("d-none", !matches || hiddenBySelected);
          if (matches && !hiddenBySelected) {
            visibleCount += 1;
          }
          if (query && value.toLocaleLowerCase("tr-TR") === query) {
            exactMatch = true;
          }
        });

        var canCreate = isCreatableValue(input.value) && !exactMatch && !hasSelected(input.value);
        if (createOption) {
          createOption.classList.toggle("d-none", !canCreate);
          createOption.textContent = canCreate ? '"' + normalize(input.value) + '" ekle' : "";
        }
        if (empty) {
          empty.classList.toggle("d-none", visibleCount !== 0 || canCreate);
        }
        setActiveOption(getVisibleOptions().length ? 0 : -1);
      }

      function choose(value) {
        if (!isCreatableValue(value)) {
          input.value = "";
          filterOptions();
          input.focus();
          return;
        }

        renderChip(value);
        input.value = "";
        filterOptions();
        input.focus();
      }

      bindExistingRemovers();
      syncHidden();

      input.addEventListener("focus", openMenu);
      input.addEventListener("input", function () {
        openMenu();
        filterOptions();
      });
      input.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
          closeMenu();
        }
        if (event.key === "ArrowDown") {
          event.preventDefault();
          openMenu();
          setActiveOption(activeIndex + 1);
        }
        if (event.key === "ArrowUp") {
          event.preventDefault();
          openMenu();
          setActiveOption(activeIndex <= 0 ? getVisibleOptions().length - 1 : activeIndex - 1);
        }
        if (event.key === "Enter") {
          event.preventDefault();
          var visibleOptions = getVisibleOptions();
          var selectedOption = visibleOptions[activeIndex] || visibleOptions[0];
          choose(selectedOption && selectedOption !== createOption ? selectedOption.getAttribute("data-value") || selectedOption.textContent : input.value);
        }
        if (event.key === "Backspace" && !input.value) {
          var current = selectedValues();
          var last = current[current.length - 1];
          if (last) {
            var chip = chips.querySelector('[data-value="' + CSS.escape(last) + '"]');
            if (chip) {
              chip.remove();
              syncHidden();
              filterOptions();
            }
          }
        }
      });

      if (toggle) {
        toggle.addEventListener("click", function () {
          if (combobox.classList.contains("is-open")) {
            closeMenu();
          } else {
            input.focus();
            openMenu();
          }
        });
      }

      options.forEach(function (option) {
        option.addEventListener("mousedown", function (event) {
          event.preventDefault();
          choose(option.getAttribute("data-value") || option.textContent);
        });
        option.addEventListener("click", function () {
          choose(option.getAttribute("data-value") || option.textContent);
        });
      });

      if (createOption) {
        createOption.addEventListener("mousedown", function (event) {
          event.preventDefault();
          choose(input.value);
        });
        createOption.addEventListener("click", function () {
          choose(input.value);
        });
      }
    });

    if (document.documentElement.dataset.productComboboxOutsideBound !== "true") {
      document.documentElement.dataset.productComboboxOutsideBound = "true";
      document.addEventListener("mousedown", function (event) {
        document.querySelectorAll("[data-product-combobox].is-open, [data-product-multi-combobox].is-open").forEach(function (combobox) {
          if (!combobox.contains(event.target)) {
            combobox.classList.remove("is-open");
            var menu = combobox.querySelector(".product-combobox-menu");
            if (menu) {
              menu.hidden = true;
            }
            var input = combobox.querySelector("[role='combobox']");
            if (input) {
              input.setAttribute("aria-expanded", "false");
            }
          }
        });
      });
    }
  }
})();
