(function () {
    'use strict';

    function togglePassword(button) {
        var container = button.closest('.form-floating');
        var input = container && (container.querySelector('input[type="password"]') || container.querySelector('input[type="text"]'));
        if (!input) {
            return;
        }

        var show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        var label = show ? button.getAttribute('data-hide-label') : button.getAttribute('data-show-label');
        button.setAttribute('aria-pressed', show ? 'true' : 'false');
        button.setAttribute('aria-label', label);
        button.setAttribute('title', label);
        input.focus();
    }

    document.addEventListener('click', function (event) {
        var button = event.target.closest('.password-toggle');
        if (button) {
            event.preventDefault();
            togglePassword(button);
        }
    });
})();