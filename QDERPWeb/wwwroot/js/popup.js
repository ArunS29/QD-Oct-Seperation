// Function to open modal with URL and title
function openModal(url, title) {
    document.getElementById("modalIframe").src = url + "?modal=true"; // Append ?modal=true
    document.getElementById("modalTitle").innerText = title; // Set modal title
    document.getElementById("customModal").style.display = "block";
}

// Function to close modal
function closeModal() {
    document.getElementById("customModal").style.display = "none";
    document.getElementById("modalIframe").src = ""; // Clear iframe on close
}