(function(){
  const html = document.documentElement;
  const themeBtn = document.getElementById('btn-theme');
  const sidebar = document.getElementById('app-sidebar');
  const toggleBtn = document.getElementById('btn-toggle');

  function setTheme(mode){
    if(mode === 'light') html.classList.add('light'); else html.classList.remove('light');
    localStorage.setItem('theme-mode', mode);
  }
  const saved = localStorage.getItem('theme-mode') || 'dark';
  setTheme(saved);

  if(themeBtn){
    themeBtn.addEventListener('click', function(){
      const next = html.classList.contains('light') ? 'dark' : 'light';
      setTheme(next);
      this.querySelector('i').classList.toggle('bi-moon');
      this.querySelector('i').classList.toggle('bi-sun');
    });
  }

  // Sidebar collapsed state persistence (desktop)
  function setSidebarCollapsed(collapsed){
    if(!sidebar) return;
    if(window.innerWidth < 992){
      // On mobile use overlay menu, not collapsed mode
      sidebar.classList.remove('collapsed');
      return;
    }
    if(collapsed) sidebar.classList.add('collapsed');
    else sidebar.classList.remove('collapsed');
    localStorage.setItem('sidebar-collapsed', collapsed ? '1' : '0');
  }

  // Restore state on load (desktop only)
  const savedCollapsed = localStorage.getItem('sidebar-collapsed') === '1';
  if(window.innerWidth >= 992){
    setSidebarCollapsed(savedCollapsed);
  }

  if(toggleBtn && sidebar){
    toggleBtn.addEventListener('click', function(){
      if(window.innerWidth < 992){
        sidebar.classList.toggle('show');
      }else{
        const willCollapse = !sidebar.classList.contains('collapsed');
        setSidebarCollapsed(willCollapse);
      }
    });
    // Adjust on resize between desktop and mobile
    window.addEventListener('resize', function(){
      if(window.innerWidth < 992){
        sidebar.classList.remove('collapsed');
      }else{
        const saved = localStorage.getItem('sidebar-collapsed') === '1';
        setSidebarCollapsed(saved);
      }
    });
  }
})();
