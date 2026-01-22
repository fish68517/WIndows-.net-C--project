// Utility functions and helpers for the tourism platform

// ============================================================================
// LOCAL STORAGE UTILITIES
// ============================================================================

/**
 * Save data to local storage
 */
function saveToLocalStorage(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
        return true;
    } catch (error) {
        console.error('Error saving to local storage:', error);
        return false;
    }
}

/**
 * Get data from local storage
 */
function getFromLocalStorage(key) {
    try {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : null;
    } catch (error) {
        console.error('Error reading from local storage:', error);
        return null;
    }
}

/**
 * Remove data from local storage
 */
function removeFromLocalStorage(key) {
    try {
        localStorage.removeItem(key);
        return true;
    } catch (error) {
        console.error('Error removing from local storage:', error);
        return false;
    }
}

/**
 * Clear all local storage
 */
function clearLocalStorage() {
    try {
        localStorage.clear();
        return true;
    } catch (error) {
        console.error('Error clearing local storage:', error);
        return false;
    }
}

// ============================================================================
// SESSION STORAGE UTILITIES
// ============================================================================

/**
 * Save data to session storage
 */
function saveToSessionStorage(key, value) {
    try {
        sessionStorage.setItem(key, JSON.stringify(value));
        return true;
    } catch (error) {
        console.error('Error saving to session storage:', error);
        return false;
    }
}

/**
 * Get data from session storage
 */
function getFromSessionStorage(key) {
    try {
        const item = sessionStorage.getItem(key);
        return item ? JSON.parse(item) : null;
    } catch (error) {
        console.error('Error reading from session storage:', error);
        return null;
    }
}

// ============================================================================
// COOKIE UTILITIES
// ============================================================================

/**
 * Set cookie
 */
function setCookie(name, value, days = 7) {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
    const expires = 'expires=' + date.toUTCString();
    document.cookie = name + '=' + value + ';' + expires + ';path=/';
}

/**
 * Get cookie
 */
function getCookie(name) {
    const nameEQ = name + '=';
    const cookies = document.cookie.split(';');
    
    for (let i = 0; i < cookies.length; i++) {
        let cookie = cookies[i].trim();
        if (cookie.indexOf(nameEQ) === 0) {
            return cookie.substring(nameEQ.length);
        }
    }
    return null;
}

/**
 * Delete cookie
 */
function deleteCookie(name) {
    setCookie(name, '', -1);
}

// ============================================================================
// URL UTILITIES
// ============================================================================

/**
 * Get URL parameter
 */
function getUrlParameter(name) {
    const url = new URL(window.location.href);
    return url.searchParams.get(name);
}

/**
 * Get all URL parameters
 */
function getAllUrlParameters() {
    const url = new URL(window.location.href);
    const params = {};
    
    url.searchParams.forEach((value, key) => {
        params[key] = value;
    });
    
    return params;
}

/**
 * Build query string
 */
function buildQueryString(params) {
    const queryParts = [];
    
    for (const key in params) {
        if (params.hasOwnProperty(key) && params[key] !== null && params[key] !== '') {
            queryParts.push(encodeURIComponent(key) + '=' + encodeURIComponent(params[key]));
        }
    }
    
    return queryParts.length > 0 ? '?' + queryParts.join('&') : '';
}

/**
 * Update URL without reload
 */
function updateUrl(url) {
    window.history.pushState({}, '', url);
}

// ============================================================================
// STRING UTILITIES
// ============================================================================

/**
 * Truncate string
 */
function truncateString(str, length = 100, suffix = '...') {
    if (str.length <= length) return str;
    return str.substring(0, length) + suffix;
}

/**
 * Capitalize first letter
 */
function capitalizeFirstLetter(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

/**
 * Escape HTML special characters
 */
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

/**
 * Unescape HTML special characters
 */
function unescapeHtml(text) {
    const map = {
        '&amp;': '&',
        '&lt;': '<',
        '&gt;': '>',
        '&quot;': '"',
        '&#039;': "'"
    };
    return text.replace(/&amp;|&lt;|&gt;|&quot;|&#039;/g, m => map[m]);
}

// ============================================================================
// NUMBER UTILITIES
// ============================================================================

/**
 * Format number with thousand separators
 */
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}

/**
 * Format currency
 */
function formatCurrency(value, currency = '¥') {
    return currency + parseFloat(value).toFixed(2);
}

/**
 * Parse currency string
 */
function parseCurrency(str) {
    return parseFloat(str.replace(/[^\d.-]/g, ''));
}

/**
 * Round number to decimal places
 */
function roundNumber(num, decimals = 2) {
    return Math.round(num * Math.pow(10, decimals)) / Math.pow(10, decimals);
}

// ============================================================================
// DATE UTILITIES
// ============================================================================

/**
 * Format date
 */
function formatDate(date, format = 'YYYY-MM-DD') {
    if (typeof date === 'string') {
        date = new Date(date);
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    return format
        .replace('YYYY', year)
        .replace('MM', month)
        .replace('DD', day)
        .replace('HH', hours)
        .replace('mm', minutes)
        .replace('ss', seconds);
}

/**
 * Get relative time (e.g., "2 hours ago")
 */
function getRelativeTime(date) {
    if (typeof date === 'string') {
        date = new Date(date);
    }

    const now = new Date();
    const seconds = Math.floor((now - date) / 1000);

    if (seconds < 60) return '刚刚';
    if (seconds < 3600) return Math.floor(seconds / 60) + '分钟前';
    if (seconds < 86400) return Math.floor(seconds / 3600) + '小时前';
    if (seconds < 604800) return Math.floor(seconds / 86400) + '天前';

    return formatDate(date, 'YYYY-MM-DD');
}

/**
 * Add days to date
 */
function addDays(date, days) {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}

/**
 * Get date difference in days
 */
function getDateDifference(date1, date2) {
    const oneDay = 24 * 60 * 60 * 1000;
    return Math.round(Math.abs((date1 - date2) / oneDay));
}

// ============================================================================
// ARRAY UTILITIES
// ============================================================================

/**
 * Remove duplicates from array
 */
function removeDuplicates(arr) {
    return [...new Set(arr)];
}

/**
 * Group array by property
 */
function groupBy(arr, property) {
    return arr.reduce((groups, item) => {
        const key = item[property];
        if (!groups[key]) {
            groups[key] = [];
        }
        groups[key].push(item);
        return groups;
    }, {});
}

/**
 * Sort array by property
 */
function sortBy(arr, property, ascending = true) {
    return arr.sort((a, b) => {
        if (ascending) {
            return a[property] > b[property] ? 1 : -1;
        } else {
            return a[property] < b[property] ? 1 : -1;
        }
    });
}

/**
 * Filter array by property value
 */
function filterBy(arr, property, value) {
    return arr.filter(item => item[property] === value);
}

// ============================================================================
// OBJECT UTILITIES
// ============================================================================

/**
 * Deep clone object
 */
function deepClone(obj) {
    return JSON.parse(JSON.stringify(obj));
}

/**
 * Merge objects
 */
function mergeObjects(obj1, obj2) {
    return { ...obj1, ...obj2 };
}

/**
 * Check if object is empty
 */
function isEmptyObject(obj) {
    return Object.keys(obj).length === 0;
}

// ============================================================================
// VALIDATION UTILITIES
// ============================================================================

/**
 * Validate email
 */
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Validate phone number
 */
function validatePhoneNumber(phone) {
    const phoneRegex = /^1[3-9]\d{9}$/;
    return phoneRegex.test(phone);
}

/**
 * Validate URL
 */
function validateUrl(url) {
    try {
        new URL(url);
        return true;
    } catch (error) {
        return false;
    }
}

/**
 * Validate password strength
 */
function validatePasswordStrength(password) {
    if (password.length < 8) return false;
    if (!/[A-Z]/.test(password)) return false;
    if (!/[a-z]/.test(password)) return false;
    if (!/[0-9]/.test(password)) return false;
    return true;
}

/**
 * Validate Chinese ID number
 */
function validateIdNumber(id) {
    const idRegex = /^[1-9]\d{5}(18|19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$/;
    return idRegex.test(id);
}

// ============================================================================
// DOM UTILITIES
// ============================================================================

/**
 * Check if element is in viewport
 */
function isElementInViewport(el) {
    const rect = el.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}

/**
 * Scroll to element
 */
function scrollToElement(element, smooth = true) {
    element.scrollIntoView({ behavior: smooth ? 'smooth' : 'auto' });
}

/**
 * Add class to element
 */
function addClass(element, className) {
    element.classList.add(className);
}

/**
 * Remove class from element
 */
function removeClass(element, className) {
    element.classList.remove(className);
}

/**
 * Toggle class on element
 */
function toggleClass(element, className) {
    element.classList.toggle(className);
}

/**
 * Has class
 */
function hasClass(element, className) {
    return element.classList.contains(className);
}

// ============================================================================
// PERFORMANCE UTILITIES
// ============================================================================

/**
 * Measure function execution time
 */
function measureTime(func, label = 'Execution') {
    const start = performance.now();
    const result = func();
    const end = performance.now();
    console.log(`${label}: ${(end - start).toFixed(2)}ms`);
    return result;
}

/**
 * Debounce function
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
 * Throttle function
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

/**
 * Request animation frame wrapper
 */
function requestAnimationFrameWrapper(callback) {
    return window.requestAnimationFrame(callback);
}

/**
 * Cancel animation frame
 */
function cancelAnimationFrameWrapper(id) {
    return window.cancelAnimationFrame(id);
}
