//document.addEventListener("DOMContentLoaded", function () {
//    const themeToggleBtn = document.getElementById("themeToggle");
//    const body = document.body;
//    const storedTheme = localStorage.getItem("theme");

//    if (storedTheme === "dark") {
//        body.classList.add("dark-mode");
//    }

//    themeToggleBtn.addEventListener("click", function () {
//        body.classList.toggle("dark-mode");
//        const newTheme = body.classList.contains("dark-mode") ? "dark" : "light";
//        localStorage.setItem("theme", newTheme);
//    });
//});
document.addEventListener("DOMContentLoaded", function () {
    const fullscreenBtn = document.getElementById("fullscreenToggle");
    const fullscreenIcon = document.getElementById("fullscreenIcon");

    function showF11Tip() {
        if (!localStorage.getItem("f11TipShown")) {
            alert("Press F11 on your keyboard for true fullscreen (browser-level).");
            localStorage.setItem("f11TipShown", "true");
        }
    }


    fullscreenBtn.addEventListener("click", function () {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen().then(() => {
                localStorage.setItem("isFullscreen", "true");
                fullscreenIcon.classList.remove("fa-expand");
                fullscreenIcon.classList.add("fa-compress");
                showF11Tip(); // Prompt user to press F11
            }).catch(err => {
                console.log("Error entering fullscreen:", err.message);
            });
        } else {
            document.exitFullscreen().then(() => {
                localStorage.setItem("isFullscreen", "false");
                fullscreenIcon.classList.remove("fa-compress");
                fullscreenIcon.classList.add("fa-expand");
            }).catch(err => {
                console.log("Error exiting fullscreen:", err.message);
            });
        }
    });
});