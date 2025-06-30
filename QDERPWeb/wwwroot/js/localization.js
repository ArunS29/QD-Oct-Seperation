//DevExpress.localization.loadMessages(dictionary);
var formatMessage = DevExpress.localization.formatMessage;

var locales = [];

function fetchLocales() {
    return fetch('/api/language/GetLanguages')
        .then(response => response.json())
        .then(data => {
            if (!Array.isArray(data)) {
                throw new Error("Unexpected response format");
            }

            locales = data.map(locale => ({
                name: locale.Name,
                value: locale.Value,
                flag: locale.Flag
            }));

            const selectBox = $("#languageSelectBox").dxSelectBox("instance");
            if (selectBox) {
                selectBox.option("dataSource", locales);
            }

            updateFlagIcon(getLocale()); // ✅ Moved here
        })
        .catch(error => console.error("Error fetching locales:", error));
}
$(document).ready(function () {
    const locale = getLocale(); // Get saved or default locale

    loadLocaleMessages(locale)
        .then(() => {
            DevExpress.localization.locale(locale); // Set locale globally
            return fetchLocales(); // Now fetch available languages
        })
        .catch(error => {
            console.error("Localization setup failed:", error);
        });
});
function getCurrentTenantId() {
    return localStorage.getItem("tenantId") || "defaultTenant"; // Modify based on your logic
}
var locale = getLocale();
//DevExpress.localization.locale(locale);
loadLocaleMessages(locale).then(() => {
    DevExpress.localization.locale(locale);
    // Continue initializing UI or refresh if needed
});
function changeLocale(dropdown) {
    console.log("dropdowndropdown", dropdown)
    var selectedLocale = dropdown.value;
    setLocale(selectedLocale);
    document.location.reload(); // Reload page to apply changes
    updateFlagIcon(selectedLocale);
}
function getLocale() {
    var locale = sessionStorage.getItem("locale");
    return locale !== null ? locale : "en"; // Default to English
}
function setLocale(locale) {
    sessionStorage.setItem("locale", locale);
}
function updateFlagIcon(selectedValue) {
    var selectedItem = locales.find(item => item.value === selectedValue);
    if (!selectedItem) return;

    $("#selectInput").css({
        backgroundImage: `url(${selectedItem.flag})`,
        backgroundRepeat: "no-repeat",
        backgroundSize: "20px 20px",
        backgroundPosition: "5px center",
        paddingLeft: "30px"
    });
}
$(document).ready(() => updateFlagIcon(locale));
function customItemTemplate(data) {
    if (!data) return $("<div>");
    return $("<div>").css({ display: "flex", alignItems: "center" })
        .append(
            $("<img>").attr("src", data.flag).attr("alt", data.name)
                .css({ width: "20px", height: "20px", marginRight: "8px", borderRadius: "50%" }),
            $("<span>").text(data.name)
        );
}
function loadLocaleMessages(locale) {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = `/localization/${locale}.js`;
        script.onload = resolve;
        script.onerror = reject;
        document.head.appendChild(script);
    });
}