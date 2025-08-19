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

  if(toggleBtn && sidebar){
    toggleBtn.addEventListener('click', function(){
      if(window.innerWidth < 992){
        sidebar.classList.toggle('show');
      }else{
        sidebar.classList.toggle('collapsed');
      }
    });
  }
})();
