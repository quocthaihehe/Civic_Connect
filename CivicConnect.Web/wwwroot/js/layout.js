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

  // 9.2 Toggle Avatar Menu (Click ra ngoài để đóng)
  const avatarMenu = document.getElementById('avatarMenu');
  window.toggleAvatarMenu = () => {
    if (avatarMenu) {
      const isVisible = avatarMenu.style.display === 'block';
      closeAllDropdowns();
      avatarMenu.style.display = isVisible ? 'none' : 'block';
    }
  };

  // 9.3 Toggle Notification Panel
  const notifPanel = document.getElementById('notifPanel');
  window.toggleNotifPanel = () => {
    if (notifPanel) {
      const isVisible = notifPanel.style.display === 'block';
      closeAllDropdowns();
      notifPanel.style.display = isVisible ? 'none' : 'block';
    }
  };

  // Đóng dropdown khi click ra ngoài
  document.addEventListener('click', (e) => {
    if (!e.target.closest('#avatarWrap') && !e.target.closest('#notifWrap')) {
      closeAllDropdowns();
    }
  });

  function closeAllDropdowns() {
    if (avatarMenu) avatarMenu.style.display = 'none';
    if (notifPanel) notifPanel.style.display = 'none';
  }

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
