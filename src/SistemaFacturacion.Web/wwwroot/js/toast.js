// toast.js - Sistema de notificaciones
window.showToast = function(message, type = 'error') {
    // Remover toast anterior si existe
    const existingToast = document.getElementById('custom-toast');
    if (existingToast) {
        existingToast.remove();
    }

    // Crear el toast
    const toast = document.createElement('div');
    toast.id = 'custom-toast';
    toast.className = `custom-toast ${type}`;
    
    const icon = type === 'error' ? '⛔' : 
                 type === 'success' ? '✅' : 
                 type === 'warning' ? '⚠️' : 'ℹ️';
    
    toast.innerHTML = `
        <div class="toast-content">
            <span class="toast-icon">${icon}</span>
            <span class="toast-message">${message}</span>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    // Mostrar con animación
    setTimeout(() => toast.classList.add('show'), 10);
    
    // Ocultar después de 3 segundos
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
};
