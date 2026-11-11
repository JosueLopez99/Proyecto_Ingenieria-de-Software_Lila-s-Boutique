// GESTIÓN DE TEMAS (MODO CLARO/OSCURO) PARA BOOTSTRAP 5
window.setModoOscuro = (isDark) => {
    const theme = isDark ? 'dark' : 'light';
    document.documentElement.setAttribute('data-bs-theme', theme);
    localStorage.setItem('theme', theme);
};

// Autoejecutable: revisa el LocalStorage antes de renderizar la UI
(function () {
    const theme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-bs-theme', theme);
})();

// EVENTOS DE TECLADO NATIVOS (DELEGACIÓN DE EVENTOS)
document.addEventListener('keydown', function (e) {
    // Si el elemento donde se teclea tiene la clase 'solo-enteros'
    if (e.target && e.target.classList.contains('solo-enteros')) {
        // Bloquea punto, coma, signos matemáticos y la letra 'e'
        if (['e', 'E', ','].includes(e.key)) {
            e.preventDefault();
        }
    }
});