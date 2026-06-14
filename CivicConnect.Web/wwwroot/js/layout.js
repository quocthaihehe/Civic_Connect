/* wwwroot/js/layout.js */
document.addEventListener('DOMContentLoaded', () => {
  // 9.1 Toggle Sidebar Mobile
  const sidebar = document.getElementById('sidebar');
  const overlay = document.getElementById('sidebarOverlay');
  
  window.toggleSidebar = () => {
    if (sidebar && overlay) {
      sidebar.classList.toggle('open');
      overlay.classList.toggle('show');
    }
  };

  // Legacy JS dropdown logic removed in favor of Bootstrap 5 native dropdowns

  // --- THEME SWITCHER LOGIC (VANILLA JS - 100% RELIABLE) ---
  const themeToggleBtn = document.getElementById('theme-toggle');
  if (themeToggleBtn) {
    const themeIcon = document.getElementById('theme-toggle-icon');

    function applyThemeIcon(theme) {
      if (theme === 'dark') {
        if (themeIcon) themeIcon.className = 'bi bi-sun-fill text-warning';
      } else {
        if (themeIcon) themeIcon.className = 'bi bi-moon';
      }
    }

    // Initialize icon based on current document theme
    const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
    applyThemeIcon(currentTheme);

    // Toggle click handler
    themeToggleBtn.addEventListener('click', () => {
      const activeTheme = document.documentElement.getAttribute('data-theme') || 'light';
      const newTheme = activeTheme === 'dark' ? 'light' : 'dark';
      
      document.documentElement.setAttribute('data-theme', newTheme);
      localStorage.setItem('theme', newTheme);
      applyThemeIcon(newTheme);
    });
  }
});
