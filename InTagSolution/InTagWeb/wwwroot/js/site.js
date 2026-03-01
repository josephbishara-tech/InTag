/* ═══════════════════════════════════════════
   InTag — Core UI JavaScript
   ═══════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {
    initThemeEngine();
    initSidebarCollapse();
    initAlertAutoDismiss();
});

/* ── Theme Engine ──────────────────────── */
function initThemeEngine() {
    const btn = document.getElementById('btn-theme-toggle');
    const icon = document.getElementById('theme-icon');
    if (!btn || !icon) return;

    // Load saved preference
    const saved = localStorage.getItem('intag-theme') || 'light';
    applyTheme(saved);

    btn.addEventListener('click', () => {
        const current = document.documentElement.getAttribute('data-bs-theme');
        const next = current === 'dark' ? 'light' : 'dark';
        applyTheme(next);
        localStorage.setItem('intag-theme', next);
    });

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        icon.className = theme === 'dark'
            ? 'bi bi-sun-fill'
            : 'bi bi-moon-stars';
    }
}

/* ── Sidebar Collapse ──────────────────── */
function initSidebarCollapse() {
    const btn = document.getElementById('btn-collapse-sidebar');
    const sidebar = document.getElementById('sidebar');
    const icon = document.getElementById('collapse-icon');
    if (!btn || !sidebar) return;

    // Load saved state
    const collapsed = localStorage.getItem('intag-sidebar-collapsed') === 'true';
    if (collapsed) {
        sidebar.classList.add('collapsed');
        if (icon) icon.className = 'bi bi-chevron-bar-right';
    }

    btn.addEventListener('click', (e) => {
        e.preventDefault();
        sidebar.classList.toggle('collapsed');
        const isCollapsed = sidebar.classList.contains('collapsed');

        if (icon) {
            icon.className = isCollapsed
                ? 'bi bi-chevron-bar-right'
                : 'bi bi-chevron-bar-left';
        }

        localStorage.setItem('intag-sidebar-collapsed', isCollapsed);
    });
}

/* ── Auto-dismiss alerts after 5s ──────── */
function initAlertAutoDismiss() {
    document.querySelectorAll('.alert-dismissible').forEach(alert => {
        setTimeout(() => {
            const closeBtn = alert.querySelector('.btn-close');
            if (closeBtn) closeBtn.click();
        }, 5000);
    });
}