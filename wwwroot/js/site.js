// Shared client-side behaviour.
//
// Progressive enhancement is the rule: every screen must work with JavaScript disabled.
// AJAX is used to remove full page reloads where the legacy forms gave instant feedback,
// never as a prerequisite for the screen functioning at all.

(function () {
    'use strict';

    // Auto-dismiss success alerts, which are informational. Errors stay until dismissed.
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.alert-success').forEach(function (alert) {
            window.setTimeout(function () {
                alert.classList.remove('show');
            }, 5000);
        });
    });
})();
