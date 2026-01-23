// Password toggle with Blazor compatibility
function togglePasswordVisibility(inputId, buttonId) {
    const input = document.getElementById(inputId);
    const button = document.getElementById(buttonId);

    if (!input || !button) return;

    if (input.type === 'password') {
        input.type = 'text';
        button.innerHTML = '<span>🙈</span>';
    } else {
        input.type = 'password';
        button.innerHTML = '<span>👁️</span>';
    }

    // Trigger Blazor's onchange event to update the model
    input.dispatchEvent(new Event('change', { bubbles: true }));
}
