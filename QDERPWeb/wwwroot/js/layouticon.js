document.addEventListener("DOMContentLoaded", function () {
    const themeToggleBtn = document.getElementById("themeToggle");
    const body = document.body;
    const storedTheme = localStorage.getItem("theme");

    if (storedTheme === "dark") {
        body.classList.add("dark-mode");
    }

    themeToggleBtn.addEventListener("click", function () {
        body.classList.toggle("dark-mode");
        const newTheme = body.classList.contains("dark-mode") ? "dark" : "light";
        localStorage.setItem("theme", newTheme);
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const fullscreenBtn = document.getElementById("fullscreenToggle");
    const fullscreenIcon = document.getElementById("fullscreenIcon");

    fullscreenBtn.addEventListener("click", function () {
        if (!document.fullscreenElement) {
            // Enter fullscreen
            document.documentElement.requestFullscreen().catch(err => {
                console.log(`Error attempting to enable fullscreen: ${err.message}`);
            });
            fullscreenIcon.classList.remove("fa-expand");
            fullscreenIcon.classList.add("fa-compress");
        } else {
            // Exit fullscreen
            document.exitFullscreen().catch(err => {
                console.log(`Error attempting to exit fullscreen: ${err.message}`);
            });
            fullscreenIcon.classList.remove("fa-compress");
            fullscreenIcon.classList.add("fa-expand");
        }
    });

    // Listen for fullscreen change
    document.addEventListener("fullscreenchange", function () {
        if (document.fullscreenElement) {
            fullscreenIcon.classList.remove("fa-expand");
            fullscreenIcon.classList.add("fa-compress");
        } else {
            fullscreenIcon.classList.remove("fa-compress");
            fullscreenIcon.classList.add("fa-expand");
        }
    });
});
