document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('loginForm');
    const togglePass = document.getElementById('togglePass');
    const passInput = document.getElementById('Password');

    // 1. Ẩn/Hiện mật khẩu
    togglePass.addEventListener('click', () => {
        const isPass = passInput.type === 'password';
        passInput.type = isPass ? 'text' : 'password';
        togglePass.classList.toggle('fa-eye');
        togglePass.classList.toggle('fa-eye-slash');
    });

    // 2. Loading khi submit
    loginForm.addEventListener('submit', () => {
        if ($(loginForm).valid()) {
            const btn = document.getElementById('submitBtn');
            const txt = document.getElementById('btnTxt');
            btn.disabled = true;
            btn.style.opacity = '0.7';
            txt.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xác thực...';
        }
    });
});

/**
 * Tự động điền tài khoản mẫu
 */
function autoFill(u, p) {
    document.getElementById('Username').value = u;
    document.getElementById('Password').value = p;
}