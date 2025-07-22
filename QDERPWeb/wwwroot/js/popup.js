function openModal(url, title, height = "90%", width = "100%") {
    debugger;
    const separator = url.includes('?') ? '&' : '?';
    const modalUrl = url + separator + 'modal=true';

    document.getElementById("modalIframe").src = modalUrl; 
    document.getElementById("modalTitle").innerText = title; // Set modal title
    document.getElementById("customModal").style.display = "block";
    document.getElementById("modalContent").style.height = height;
    document.getElementById("modalContent").style.width = width;
}

// Function to close modal
function closeModal() {
    document.getElementById("customModal").style.display = "none";
    document.getElementById("modalIframe").src = ""; // Clear iframe on close
}


function showToastModal(type, title, message) {
    const iconMap = {
        success: "✔️",
        info: "ℹ️",
        warning: "⚠️",
        error: "❌"
    };

    // Get elements
    const toast = document.getElementById("toastModal");
    const toastContent = document.getElementById("toastModalContent");
    const toastTitle = document.getElementById("toastTitle");
    const toastMessage = document.getElementById("toastMessage");
    const toastIcon = document.getElementById("toastIcon");

    // Update content
    toastTitle.innerText = title;
    toastMessage.innerText = message;
    toastIcon.innerText = iconMap[type] || "ℹ️";

    // Change background class
    toastContent.className = `toast toast-${type}`;

    // Show and auto-hide
    toast.style.display = "block";
    toastContent.style.display = "flex";
    setTimeout(() => {
        hideToastModal();
    }, 500000);
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
