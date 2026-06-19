/* ============================================================
   EMPS — Global Site JavaScript
   ============================================================ */

document.addEventListener('DOMContentLoaded', function () {

    // ── Mobile sidebar toggle ─────────────────────────────────
    const sidebar       = document.querySelector('.sidebar');
    const overlay       = document.getElementById('sidebar-overlay');
    const btnOpen       = document.getElementById('btn-sidebar-open');
    const btnClose      = document.getElementById('btn-sidebar-close');

    function openSidebar() {
        sidebar?.classList.add('sidebar-open');
        overlay?.classList.add('visible');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        sidebar?.classList.remove('sidebar-open');
        overlay?.classList.remove('visible');
        document.body.style.overflow = '';
    }

    btnOpen?.addEventListener('click', openSidebar);
    btnClose?.addEventListener('click', closeSidebar);
    overlay?.addEventListener('click', closeSidebar);

    // Close on Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeSidebar();
    });

    // ── Auto-dismiss alerts after 4 s ────────────────────────
    const alerts = document.querySelectorAll('.alert.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert?.close();
        }, 4000);
    });

    // ── Confirm destructive actions ──────────────────────────
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            const msg = el.getAttribute('data-confirm') || 'Are you sure?';
            if (!confirm(msg)) e.preventDefault();
        });
    });

    // ── Active nav item highlight (fallback for server-side) ─
    // In case Razor active class missed something
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sidebar-link').forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && href !== '/' && currentPath.startsWith(href.toLowerCase())) {
            link.closest('.sidebar-item')?.classList.add('active');
        }
    });

    // ── Tooltip init (Bootstrap) ──────────────────────────────
    const tooltipTriggers = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggers.forEach(function (el) {
        new bootstrap.Tooltip(el, { trigger: 'hover' });
    });

    // ── Form dirty-state warning ─────────────────────────────
    const forms = document.querySelectorAll('form[data-dirty-warn]');
    forms.forEach(function (form) {
        let dirty = false;
        form.addEventListener('input', function () { dirty = true; });
        form.addEventListener('submit', function () { dirty = false; });
        window.addEventListener('beforeunload', function (e) {
            if (dirty) {
                e.preventDefault();
                e.returnValue = 'You have unsaved changes. Leave anyway?';
            }
        });
    });

});
