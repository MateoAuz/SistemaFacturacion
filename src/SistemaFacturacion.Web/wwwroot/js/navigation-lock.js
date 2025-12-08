(function() {
    let isInternalNavigation = false;
    let currentPath = window.location.pathname;
    
    const publicRoutes = ['/login', '/acceso-denegado'];
    
    // Función para verificar si el usuario está autenticado
    function isAuthenticated() {
        const token = localStorage.getItem('authToken');
        return token !== null && token !== '';
    }
    
    // Función para verificar si la ruta es pública
    function isPublicRoute(path) {
        return publicRoutes.some(route => path.startsWith(route));
    }
    
    (function verificacionInicial() {
        const path = window.location.pathname;
        if (!isPublicRoute(path) && !isAuthenticated()) {
            window.location.replace('/login');
            return;
        }
        currentPath = path;
    })();
    
    // Función para verificar navegación
    function checkNavigation(targetPath) {
        // Si la ruta es pública, permitir
        if (isPublicRoute(targetPath)) {
            return true;
        }
        
        // Si no está autenticado, redirigir al login
        if (!isAuthenticated()) {
            window.location.replace('/login');
            return false;
        }
        
        // Si está autenticado y es navegación manual, bloquear
        if (!isInternalNavigation) {
            if (window.showToast) {
                window.showToast('⚠️ Usa el menú lateral para navegar', 'warning');
            }
            return false;
        }
        
        return true;
    }

    // Función para marcar navegación interna (desde la app)
    window.allowNavigation = function() {
        isInternalNavigation = true;
        setTimeout(() => {
            isInternalNavigation = false;
        }, 100);
    };

    // Interceptar clicks en links
    document.addEventListener('click', function(e) {
        const link = e.target.closest('a');
        if (link && link.href && link.href.startsWith(window.location.origin)) {
            const targetPath = new URL(link.href).pathname;
            
            // Si no es ruta pública y no está autenticado
            if (!isPublicRoute(targetPath) && !isAuthenticated()) {
                e.preventDefault();
                window.location.replace('/login');
                return;
            }
            
            isInternalNavigation = true;
            currentPath = targetPath;
            
            setTimeout(() => {
                isInternalNavigation = false;
            }, 500);
        }
    }, true);

    // Interceptar cambios de URL (botón atrás/adelante o URL manual)
    window.addEventListener('popstate', function(e) {
        const targetPath = window.location.pathname;
        
        if (!checkNavigation(targetPath)) {
            e.preventDefault();
            history.pushState(null, '', currentPath);
            return false;
        }
        
        currentPath = targetPath;
        isInternalNavigation = false;
    });

    // Monitorear cambios directos en la URL
    let lastUrl = window.location.href;
    setInterval(function() {
        const currentUrl = window.location.href;
        
        if (currentUrl !== lastUrl) {
            const newPath = new URL(currentUrl).pathname;
            
            // Ignorar si es la misma ruta
            if (newPath === currentPath) {
                lastUrl = currentUrl;
                return;
            }
            
            if (!checkNavigation(newPath)) {
                // Restaurar URL anterior
                history.replaceState(null, '', lastUrl);
                return;
            }
            
            currentPath = newPath;
        }
        
        lastUrl = window.location.href;
    }, 100);

    // Para Blazor Enhanced Navigation
    if (typeof Blazor !== 'undefined') {
        Blazor.addEventListener('enhancedload', function() {
            currentPath = window.location.pathname;
            isInternalNavigation = false;
        });
    }
})();
