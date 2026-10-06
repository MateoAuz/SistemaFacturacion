function verificarMenuConfig() {
    const rol = localStorage.getItem('userRole');
    const menuConfig = document.getElementById('menu-config');
    
    if (menuConfig) {
        if (rol === 'Admin') {
            menuConfig.style.display = 'flex';
        } else {
            menuConfig.style.display = 'none';
        }
    }
}

// Ejecutar inmediatamente
verificarMenuConfig();

// Ejecutar cada vez que cambia la página
document.addEventListener('DOMContentLoaded', verificarMenuConfig);

// Para Blazor Enhanced Navigation
if (typeof Blazor !== 'undefined') {
    Blazor.addEventListener('enhancedload', verificarMenuConfig);
}

// Verificar periódicamente (backup)
setInterval(verificarMenuConfig, 500);
