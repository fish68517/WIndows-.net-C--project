// AJAX interactions for favorites, comments, and other dynamic operations

document.addEventListener('DOMContentLoaded', function() {
    // Initialize favorite buttons
    initializeFavoriteButtons();
    
    // Initialize comment submission
    initializeCommentSubmission();
    
    // Initialize dynamic list operations
    initializeDynamicListOperations();
});

// ============================================================================
// FAVORITE OPERATIONS
// ============================================================================

/**
 * Initialize favorite buttons
 */
function initializeFavoriteButtons() {
    const favoriteButtons = document.querySelectorAll('[data-action="toggle-favorite"]');
    
    favoriteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const attractionId = this.getAttribute('data-attraction-id');
            toggleFavorite(attractionId, this);
        });
    });
}

/**
 * Toggle favorite status
 */
function toggleFavorite(attractionId, button) {
    const isFavorited = button.classList.contains('favorited');
    const url = isFavorited ? '/Favorites/Remove' : '/Favorites/Add';
    
    makeFormRequest(url, 'POST', { attractionId: attractionId })
        .then(data => {
            if (data.success) {
                // Update button state
                if (isFavorited) {
                    button.classList.remove('btn-danger', 'favorited');
                    button.classList.add('btn-outline-secondary');
                    const icon = button.querySelector('i');
                    if (icon) {
                        icon.classList.remove('bi-heart-fill');
                        icon.classList.add('bi-heart');
                    }
                    const text = button.querySelector('span');
                    if (text) text.textContent = '收藏';
                } else {
                    button.classList.remove('btn-outline-secondary');
                    button.classList.add('btn-danger', 'favorited');
                    const icon = button.querySelector('i');
                    if (icon) {
                        icon.classList.remove('bi-heart');
                        icon.classList.add('bi-heart-fill');
                    }
                    const text = button.querySelector('span');
                    if (text) text.textContent = '已收藏';
                }
                showAlert(data.message, 'success');
            } else {
                showAlert(data.message || '操作失败', 'danger');
            }
        })
        .catch(error => {
            console.error('Error toggling favorite:', error);
            showAlert('操作失败，请稍后重试', 'danger');
        });
}

/**
 * Check favorite status
 */
function checkFavoriteStatus(attractionId, callback) {
    fetch(`/Favorites/IsFavorited?attractionId=${attractionId}`)
        .then(response => response.json())
        .then(data => {
            if (callback) callback(data.isFavorited);
        })
        .catch(error => {
            console.error('Error checking favorite status:', error);
        });
}

// ============================================================================
// COMMENT OPERATIONS
// ============================================================================

/**
 * Initialize comment submission
 */
function initializeCommentSubmission() {
    const commentForms = document.querySelectorAll('form[data-action="submit-comment"]');
    
    commentForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            submitCommentViaAjax(form);
        });
    });
}

/**
 * Submit comment via AJAX
 */
function submitCommentViaAjax(form) {
    const url = form.getAttribute('action');
    const contentInput = form.querySelector('textarea[name="Content"]');
    const ratingInput = form.querySelector('input[name="Rating"]');
    
    if (!contentInput || !contentInput.value.trim()) {
        showAlert('请输入评论内容', 'warning');
        return;
    }

    const data = {
        Content: contentInput.value,
        Rating: ratingInput ? ratingInput.value : null,
        AttractionId: form.getAttribute('data-attraction-id'),
        DiaryId: form.getAttribute('data-diary-id')
    };

    const submitBtn = form.querySelector('button[type="submit"]');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>提交中...';
    }

    makeRequest(url, 'POST', data)
        .then(response => {
            if (response.success) {
                showAlert('评论发表成功', 'success');
                form.reset();
                
                // Reload comments section if available
                const commentsContainer = document.querySelector('[data-load-url*="comments"]');
                if (commentsContainer) {
                    reloadDynamicContent(commentsContainer);
                }
            } else {
                showAlert(response.message || '评论发表失败', 'danger');
            }
        })
        .catch(error => {
            console.error('Error submitting comment:', error);
            showAlert('请求失败，请稍后重试', 'danger');
        })
        .finally(() => {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '发表评论';
            }
        });
}

/**
 * Delete comment
 */
function deleteComment(commentId) {
    showConfirmDialog('确定要删除这条评论吗？').then(confirmed => {
        if (!confirmed) return;

        makeRequest(`/Comments/Delete/${commentId}`, 'POST')
            .then(data => {
                if (data.success) {
                    showAlert('评论已删除', 'success');
                    
                    // Remove comment element
                    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
                    if (commentElement) {
                        commentElement.remove();
                    }
                } else {
                    showAlert(data.message || '删除失败', 'danger');
                }
            })
            .catch(error => {
                console.error('Error deleting comment:', error);
                showAlert('请求失败，请稍后重试', 'danger');
            });
    });
}

// ============================================================================
// DYNAMIC LIST OPERATIONS
// ============================================================================

/**
 * Initialize dynamic list operations (delete, edit, etc.)
 */
function initializeDynamicListOperations() {
    // Delete buttons
    const deleteButtons = document.querySelectorAll('[data-action="delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const itemId = this.getAttribute('data-item-id');
            const itemType = this.getAttribute('data-item-type');
            const url = this.getAttribute('data-url');
            deleteItem(itemId, itemType, url);
        });
    });

    // Edit buttons
    const editButtons = document.querySelectorAll('[data-action="edit"]');
    editButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const url = this.getAttribute('data-url');
            window.location.href = url;
        });
    });
}

/**
 * Delete item via AJAX
 */
function deleteItem(itemId, itemType, url) {
    const message = `确定要删除这条${itemType}吗？`;
    
    showConfirmDialog(message).then(confirmed => {
        if (!confirmed) return;

        makeRequest(url, 'POST')
            .then(data => {
                if (data.success) {
                    showAlert(`${itemType}已删除`, 'success');
                    
                    // Remove item element
                    const itemElement = document.querySelector(`[data-item-id="${itemId}"]`);
                    if (itemElement) {
                        itemElement.remove();
                    } else {
                        // Reload page if element not found
                        setTimeout(() => {
                            location.reload();
                        }, 1000);
                    }
                } else {
                    showAlert(data.message || '删除失败', 'danger');
                }
            })
            .catch(error => {
                console.error('Error deleting item:', error);
                showAlert('请求失败，请稍后重试', 'danger');
            });
    });
}

// ============================================================================
// QUANTITY ADJUSTMENT
// ============================================================================

/**
 * Initialize quantity adjustment buttons
 */
function initializeQuantityAdjustment() {
    const decreaseButtons = document.querySelectorAll('[data-action="decrease-quantity"]');
    const increaseButtons = document.querySelectorAll('[data-action="increase-quantity"]');

    decreaseButtons.forEach(button => {
        button.addEventListener('click', function() {
            const input = this.parentElement.querySelector('input[type="number"]');
            if (input) {
                let value = parseInt(input.value) || 1;
                if (value > 1) {
                    input.value = value - 1;
                    updateOrderTotal(input);
                }
            }
        });
    });

    increaseButtons.forEach(button => {
        button.addEventListener('click', function() {
            const input = this.parentElement.querySelector('input[type="number"]');
            if (input) {
                let value = parseInt(input.value) || 1;
                const max = parseInt(input.getAttribute('max')) || 100;
                if (value < max) {
                    input.value = value + 1;
                    updateOrderTotal(input);
                }
            }
        });
    });
}

/**
 * Update order total price
 */
function updateOrderTotal(quantityInput) {
    const quantity = parseInt(quantityInput.value) || 1;
    const unitPrice = parseFloat(quantityInput.getAttribute('data-unit-price')) || 0;
    const total = (unitPrice * quantity).toFixed(2);

    const totalElement = document.querySelector('[data-total-price]');
    if (totalElement) {
        totalElement.textContent = total;
    }

    const quantityDisplay = document.querySelector('[data-quantity-display]');
    if (quantityDisplay) {
        quantityDisplay.textContent = quantity;
    }
}

// ============================================================================
// PAGINATION
// ============================================================================

/**
 * Load page via AJAX
 */
function loadPage(url) {
    const container = document.querySelector('[data-page-container]');
    if (!container) return;

    loadDynamicContent(container, url);
}

// ============================================================================
// SEARCH AND FILTER
// ============================================================================

/**
 * Apply search filter
 */
function applySearchFilter(formSelector) {
    const form = document.querySelector(formSelector);
    if (!form) return;

    const formData = new FormData(form);
    const params = new URLSearchParams(formData);
    const url = form.getAttribute('action') + '?' + params.toString();

    const container = document.querySelector('[data-page-container]');
    if (container) {
        loadDynamicContent(container, url);
    } else {
        window.location.href = url;
    }
}

/**
 * Clear search filter
 */
function clearSearchFilter(formSelector) {
    const form = document.querySelector(formSelector);
    if (!form) return;

    form.reset();
    applySearchFilter(formSelector);
}
