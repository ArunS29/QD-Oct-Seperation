function openModal(url, title, height = "90%", width = "100%") {
    const separator = url.includes('?') ? '&' : '?';
    const modalUrl = url + separator + 'modal=true';

    document.getElementById("modalIframe").src = modalUrl; 
    document.getElementById("modalTitle").innerText = title; // Set modal title
    document.getElementById("customModal").style.display = "block";
    document.getElementById("modalContent").style.height = height;
    document.getElementById("modalContent").style.width = width;
}

function closeModal() {
    document.getElementById("customModal").style.display = "none";
    document.getElementById("modalIframe").src = ""; // Clear iframe on close
}

document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
        closeModal();
    }
});

function showToastModal(type, title, message) {
    const iconMap = {
        success: "✔️",
        info: "ℹ️",
        warning: "⚠️",
        error: "❌"
    };

    const toast = document.getElementById("toastModal");
    const toastContent = document.getElementById("toastModalContent");
    const toastTitle = document.getElementById("toastTitle");
    const toastMessage = document.getElementById("toastMessage");
    const toastIcon = document.getElementById("toastIcon");

    toastContent.style.animation = "none";
    void toastContent.offsetWidth;
    toastContent.style.animation = "";

    toastTitle.innerText = title;
    toastMessage.innerText = message;
    toastIcon.innerText = iconMap[type] || "ℹ️";

    toastContent.className = `toast toast-${type}`;

    toast.style.display = "block";
    toastContent.style.display = "flex";

    setTimeout(() => {
        hideToastModal();
    }, 5000); // You had 500000ms (~8 mins), adjusted to 5 sec
}


function hideToastModal() {
    document.getElementById("toastModal").style.display = "none";
}
const originalNotify = DevExpress.ui.notify;
DevExpress.ui.notify = function (options, type, displayTime) {
    if (typeof options === "string") {
        options = {
            message: options,
            type: type || "info",
            displayTime: displayTime || 3000,
        };
    }
    options.width = "20vw";
    options.position = {
        my: "top right",
        at: "top right",
        of: "#toastModal"
    };
    const result = originalNotify(options);
    return result;
};
function getDefaultCompany(id = "ddGetCompany") {
    $.ajax({
        type: "GET",
        url: "/api/AddNewClientRequest/GetDefaultCompany",
        success: function (data) {
            if (data && data.CompanyId) {
                console.log("Default Company:", data.CompanyId, data.CompanyName);

                const CompanyDropDown = $(`#${id}`).dxDropDownBox("instance");
                if (CompanyDropDown) {
                    // ✅ Bind CompanyId as value
                    CompanyDropDown.option("value", data.CompanyId);
                }
            }
        },
        error: function (xhr) {
            console.error("Company fetch failed:", xhr.responseText);
        }
    });
}