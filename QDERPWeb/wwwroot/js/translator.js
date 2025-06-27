// translator.js

async function translateToArabic(text) {
    debugger;
    const apiKey = "AIzaSyBVBXVgNFAdgHslaqXIG36StBsDRKcGwUs";
    const url = `https://translation.googleapis.com/language/translate/v2` +
        `?q=${encodeURIComponent(text)}` +
        `&source=en&target=ar&format=text&key=${apiKey}`;

    try {
        const response = await fetch(url);
        const data = await response.json();
        return data.data.translations[0].translatedText;
    } catch (error) {
        console.error("Translation failed:", error);
        return "Translation Error";
    }
}
