// Front-end JavaScript - Tourism Platform

document.addEventListener('DOMContentLoaded', function() {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize Bootstrap popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Initialize form validation
    initializeFormValidation();

    // Initialize dynamic content loaders
    initializeDynamicLoaders();

    // Initialize AJAX form submissions
    initializeAjaxForms();
});

// ============================================================================
// FORM VALIDATION
// ============================================================================

/**
 * Initialize Bootstrap form validation
 */
function initializeFormValidation() {
    const forms = document.querySelectorAll('form[novalidate]');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
}

/**
 * Validate email format
 */
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Validate password strength
 * Requirements: at least 8 characters, contains uppercase, lowercase, and number
 */
function validatePasswordStrength(password) {
    if (password.length < 8) return false;
    if (!/[A-Z]/.test(password)) return false;
    if (!/[a-z]/.test(password)) return false;
    if (!/[0-9]/.test(password)) return false;
    return true;
}

/**
 * Validate form field
 */
function validateField(field) {
    const value = field.value.trim();
    const fieldType = field.getAttribute('data-validate-type');
    
    if (!fieldType) return true;

    switch (fieldType) {
        case 'email':
            return validateEmail(value);
        case 'password':
            return validatePasswordStrength(value);
        case 'required':
            return value.length > 0;
        case 'minlength':
            const minLength = field.getAttribute('data-validate-minlength');
            return value.length >= parseInt(minLength);
        case 'maxlength':
            const maxLength = field.getAttribute('data-validate-maxlength');
            return value.length <= parseInt(maxLength);
        default:
            return true;
    }
}

/**
 * Show field validation error
 */
function showFieldError(field, message) {
    field.classList.add('is-invalid');
    let errorElement = field.nextElementSibling;
    
    if (!errorElement || !errorElement.classList.contains('invalid-feedback')) {
        errorElement = document.createElement('div');
        errorElement.className = 'invalid-feedback d-block';
        field.parentNode.insertBefore(errorElement, field.nextSibling);
    }
    
    errorElement.textContent = message;
}

/**
 * Clear field validation error
 */
function clearFieldError(field) {
    field.classList.remove('is-invalid');
    const errorElement = field.nextElementSibling;
    
    if (errorElement && errorElement.classList.contains('invalid-feedback')) {
        errorElement.remove();
    }
}

// ============================================================================
// DYNAMIC CONTENT LOADING
// ============================================================================

/**
 * Initialize dynamic content loaders
 */
function initializeDynamicLoaders() {
    // Load content for elements with data-load-url attribute
    const loaders = document.querySelectorAll('[data-load-url]');
    
    loaders.forEach(loader => {
        const url = loader.getAttribute('data-load-url');
        if (url) {
            loadDynamicContent(loader, url);
        }
    });
}

/**
 * Load dynamic content via AJAX
 */
function loadDynamicContent(container, url, showSpinner = true) {
    if (showSpinner) {
        container.innerHTML = `
            <div class="text-center">
                <div class="spinner-border spinner-border-sm" role="status">
                    <span class="visually-hidden">加载中...</span>
                </div>
            </div>
        `;
    }

    return fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.text();
        })
        .then(html => {
            container.innerHTML = html;
            // Re-initialize tooltips and popovers for newly loaded content
            reinitializeBootstrapComponents();
        })
        .catch(error => {
            console.error('Error loading dynamic content:', error);
            container.innerHTML = `
                <div class="alert alert-danger" role="alert">
                    <strong>加载失败</strong>
                    <p>无法加载内容，请稍后重试。</p>
                </div>
            `;
        });
}

/**
 * Reload dynamic content
 */
function reloadDynamicContent(container) {
    const url = container.getAttribute('data-load-url');
    if (url) {
        loadDynamicContent(container, url);
    }
}

/**
 * Re-initialize Bootstrap components for dynamically loaded content
 */
function reinitializeBootstrapComponents() {
    // Re-initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(tooltipTriggerEl => {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Re-initialize popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(popoverTriggerEl => {
        new bootstrap.Popover(popoverTriggerEl);
    });
}

// ============================================================================
// AJAX REQUESTS
// ============================================================================

/**
 * Utility function for AJAX requests
 */
function makeRequest(url, method = 'GET', data = null, contentType = 'application/json') {
    const options = {
        method: method,
        headers: {
            'Content-Type': contentType
        }
    };

    // Add CSRF token if available
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (csrfToken) {
        options.headers['X-CSRF-TOKEN'] = csrfToken;
    }

    if (data && (method === 'POST' || method === 'PUT' || method === 'DELETE')) {
        if (contentType === 'application/json') {
            options.body = JSON.stringify(data);
        } else if (contentType === 'application/x-www-form-urlencoded') {
            options.body = new URLSearchParams(data).toString();
        }
    }

    return fetch(url, options)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .catch(error => {
            console.error('AJAX Error:', error);
            throw error;
        });
}

/**
 * Make form-encoded AJAX request
 */
function makeFormRequest(url, method = 'POST', formData = {}) {
    return makeRequest(url, method, formData, 'application/x-www-form-urlencoded');
}

/**
 * Initialize AJAX form submissions
 */
function initializeAjaxForms() {
    const ajaxForms = document.querySelectorAll('form[data-ajax="true"]');
    
    ajaxForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            submitFormViaAjax(form);
        });
    });
}

/**
 * Submit form via AJAX
 */
function submitFormViaAjax(form) {
    const url = form.getAttribute('action');
    const method = form.getAttribute('method') || 'POST';
    const formData = new FormData(form);
    
    // Convert FormData to object for JSON submission
    const data = {};
    formData.forEach((value, key) => {
        data[key] = value;
    });

    const submitBtn = form.querySelector('button[type="submit"]');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>处理中...';
    }

    makeRequest(url, method, data)
        .then(response => {
            if (response.success) {
                showAlert(response.message, 'success');
                if (response.redirectUrl) {
                    setTimeout(() => {
                        window.location.href = response.redirectUrl;
                    }, 1000);
                } else {
                    form.reset();
                }
            } else {
                showAlert(response.message || '操作失败', 'danger');
            }
        })
        .catch(error => {
            showAlert('请求失败，请稍后重试', 'danger');
        })
        .finally(() => {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = submitBtn.getAttribute('data-original-text') || '提交';
            }
        });
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

/**
 * Show alert message
 */
function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.setAttribute('role', 'alert');
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    // Insert at the top of main content area
    const mainContent = document.querySelector('main') || document.querySelector('.container');
    if (mainContent) {
        mainContent.insertBefore(alertDiv, mainContent.firstChild);
        
        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    }
}

/**
 * Show confirmation dialog
 */
function showConfirmDialog(message) {
    return new Promise((resolve) => {
        const confirmed = confirm(message);
        resolve(confirmed);
    });
}

/**
 * Format currency
 */
function formatCurrency(value) {
    return parseFloat(value).toFixed(2);
}

/**
 * Format date
 */
function formatDate(date, format = 'YYYY-MM-DD') {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    
    return format
        .replace('YYYY', year)
        .replace('MM', month)
        .replace('DD', day);
}

/**
 * Debounce function for search and filter operations
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Throttle function for scroll and resize events
 */
function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}
