// Emineo theme toggle – persists choice in localStorage.
window.emineoTheme = {
    get() {
        return localStorage.getItem('emineo-theme')
            || (window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark');
    },
    set(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('emineo-theme', theme);
    },
    toggle() {
        const next = this.get() === 'dark' ? 'light' : 'dark';
        this.set(next);
        return next;
    }
};