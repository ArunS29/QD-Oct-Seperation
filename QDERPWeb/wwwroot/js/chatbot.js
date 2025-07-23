function toggleChat() {
    const box = document.getElementById('chatbot-box');
    const inactive = document.getElementById('chatbot-inactive');
    const chat_text = document.getElementById('chat_text');
    const chat_close = document.getElementById('chat_close');
    box.style.display = box.style.display === 'flex' ? 'none' : 'flex';
    chat_text.style.display = chat_text.style.display === 'flex' ? 'none' : 'flex';
    chat_close.style.display = chat_close.style.display === 'flex' ? 'none' : 'flex';
    inactive.style.animation =  inactive.style.animation != 'none' ? 'ump 1.5s ease-in-out 0.3s infinite' : 'none';
}

async function sendChat() {
    const input = document.getElementById('chatbot-input');
    const userMessage = input.value.trim();
    if (!userMessage) return;

    const chatWindow = document.getElementById('chatbot-messages');
    input.value = "";

    // Display user's message
    appendMessage('You', userMessage, chatWindow);

    // Create bot message container
    const botContainer = document.createElement('div');
    botContainer.className = 'message bot';
    const botLabel = document.createElement('strong');
    botLabel.innerText = "Bot: ";
    const botText = document.createElement('span');
    botText.className = 'streaming-text';
    botContainer.appendChild(botLabel);
    botContainer.appendChild(botText);
    chatWindow.appendChild(botContainer);

    chatWindow.scrollTop = chatWindow.scrollHeight;

    // Call API
    const response = await fetch('/api/chat/stream', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: userMessage })
    });

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let partial = '';

    while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        // Decode the chunk
        partial += decoder.decode(value, { stream: true });

        // Split on whitespace
        const words = partial.split(/\s+/);

        // Keep the last partial word in buffer
        partial = words.pop(); // may be half-complete

        for (const word of words) {
            await appendWord(botText, word);
            chatWindow.scrollTop = chatWindow.scrollHeight;
        }
    }

    // Add any final leftover word
    if (partial.trim()) {
        await appendWord(botText, partial.trim());
    }

    chatWindow.scrollTop = chatWindow.scrollHeight;
}

function appendMessage(sender, text, container) {
    const div = document.createElement('div');
    div.className = 'message ' + (sender === 'You' ? 'user' : 'bot');
    div.innerHTML = `<strong>${sender}:</strong> ${text}`;
    container.appendChild(div);
    container.scrollTop = container.scrollHeight;
}

let linkBuffer = '';
let insideLink = false;

async function appendWord(element, word) {
    // Track word buffer if needed (optional for context)
    const formatted = formatWord(word);
    element.innerHTML += formatted + ' ';
    await new Promise(resolve => setTimeout(resolve, 80));
}

function formatWord(word) {
    // Handle [Text](Link) if full link in one word
    const markdownLinkPattern = /\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/;
    if (markdownLinkPattern.test(word)) {
        const match = word.match(markdownLinkPattern);
        const text = match[1];
        const url = match[2];
        return `<a href="${url}" target="_blank" style="color:#007bff;text-decoration:underline;">${text}</a>`;
    }

    // Fix case: Markdown link is split across two+ chunks (skip formatting)
    if (word.startsWith('[') || word.startsWith('(')) return word;

    // Bold (**text**)
    if (word.startsWith('**') && word.endsWith('**')) {
        return `<strong>${word.slice(2, -2)}</strong>`;
    }

    // List bullets
    if (word === '-' || word === '–' || word === '•') {
        return `<br>&bull;`;
    }

    // Header (e.g., ## Section)
    if (/^#{1,6}/.test(word)) {
        const text = word.replace(/^#{1,6}/, '');
        return `<br><strong>${text.trim()}</strong><br>`;
    }

    // Newline markers
    if (word.trim() === '\\n' || word.trim() === '\n') {
        return '<br>';
    }

    return word;
}

