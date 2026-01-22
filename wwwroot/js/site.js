/* wwwroot/js/site.js */
document.addEventListener("DOMContentLoaded", function () {
    // 侧边栏切换逻辑
    var sidebarCollapse = document.getElementById('sidebarCollapse');
    var sidebar = document.getElementById('sidebar');

    if (sidebarCollapse && sidebar) {
        sidebarCollapse.addEventListener('click', function () {
            sidebar.classList.toggle('active');
        });
    }

    // 自动高亮当前菜单项 (简单的 URL 匹配)
    var currentPath = window.location.pathname.toLowerCase();
    var menuLinks = document.querySelectorAll('#sidebar ul li a');
    
    menuLinks.forEach(function(link) {
        var href = link.getAttribute('href').toLowerCase();
        // 首页特殊处理
        if (href === '/' && currentPath === '/') {
            link.parentElement.classList.add('active');
            return;
        }
        // 其他页面匹配
        if (href !== '/' && currentPath.includes(href)) {
            link.parentElement.classList.add('active');
        }
    });
});