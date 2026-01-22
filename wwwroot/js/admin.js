// Backend Admin Interactions - Tourism Platform
// Handles table operations, confirmations, data loading, and admin-specific interactions

document.addEventListener('DOMContentLoaded', function() {
    // Initialize admin table operations
    initializeAdminTableOperations();
    
    // Initialize admin confirmation dialogs
    initializeAdminConfirmations();
    
    // Initialize admin data loading
    initializeAdminDataLoading();
    
    // Initialize admin form operations
    initializeAdminFormOperations();
    
    // Initialize admin status updates
    initializeAdminStatusUpdates();
});

// ============================================================================
// TABLE OPERATIONS
// ============================================================================

/**
 * Initialize admin table operations (delete, edit, status change)
 */
function initializeAdminTableOperations() {
    // Delete buttons in tables
    const deleteButtons = document.querySelectorAll('button[data-admin-action="delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const itemId = this.getAttribute('data-item-id');
            const itemType = this.getAttribute('data-item-type');
            const url = this.getAttribute('data-url');
            const itemName = this.getAttribute('data-item-name') || '该项目';
            
            adminDeleteItem(itemId, itemType, url, itemName);
        });
    });

    // Edit buttons in tables
    const editButtons = document.querySelectorAll('a[data-admin-action="edit"]');
    editButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            const url = this.getAttribute('href');
            // Allow default link behavior
        });
    });

    // Status change buttons
    const statusButtons = document.querySelectorAll('button[data-admin-action="change-status"]');
    statusButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const itemId = this.getAttribute('data-item-id');
            const newStatus = this.getAttribute('data-new-status');
            const url = this.getAttribute('data-url');
            
            adminChangeStatus(itemId, newStatus, url);
        });
    });

    // Bulk action buttons
    const bulkActionButtons = document.querySelectorAll('button[data-admin-action="bulk-action"]');
    bulkActionButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const action = this.getAttribute('data-bulk-action');
            const selectedItems = getSelectedTableItems();
            
            if (selectedItems.length === 0) {
                showAdminAlert('请先选择要操作的项目', 'warning');
                return;
            }
            
            adminBulkAction(action, selectedItems);
        });
    });

    // Initialize table row selection
    initializeTableRowSelection();
}

/**
 * Initialize table row selection with checkboxes
 */
function initializeTableRowSelection() {
    const selectAllCheckbox = document.querySelector('input[data-admin-action="select-all"]');
    const rowCheckboxes = document.querySelectorAll('input[data-admin-action="select-row"]');

    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function() {
            rowCheckboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
                const row = checkbox.closest('tr');
                if (row) {
                    if (this.checked) {
                        row.classList.add('table-active');
                    } else {
                        row.classList.remove('table-active');
                    }
                }
            });
        });
    }

    rowCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const row = this.closest('tr');
            if (row) {
                if (this.checked) {
                    row.classList.add('table-active');
                } else {
                    row.classList.remove('table-active');
                }
            }

            // Update select-all checkbox state
            const allChecked = Array.from(rowCheckboxes).every(cb => cb.checked);
            const someChecked = Array.from(rowCheckboxes).some(cb => cb.checked);
            
            if (selectAllCheckbox) {
                selectAllCheckbox.checked = allChecked;
                selectAllCheckbox.indeterminate = someChecked && !allChecked;
            }
        });
    });
}

/**
 * Get selected table items
 */
function getSelectedTableItems() {
    const selectedCheckboxes = document.querySelectorAll('input[data-admin-action="select-row"]:checked');
    const selectedItems = [];
    
    selectedCheckboxes.forEach(checkbox => {
        const itemId = checkbox.getAttribute('data-item-id');
        if (itemId) {
            selectedItems.push(itemId);
        }
    });
    
    return selectedItems;
}

/**
 * Delete item via AJAX
 */
function adminDeleteItem(itemId, itemType, url, itemName) {
    const message = `确定要删除${itemName}吗？此操作无法撤销。`;
    
    adminShowConfirmDialog(message, '删除', 'danger').then(confirmed => {
        if (!confirmed) return;

        const submitBtn = document.querySelector(`button[data-item-id="${itemId}"][data-admin-action="delete"]`);
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>删除中...';
        }

        makeRequest(url, 'POST')
            .then(data => {
                if (data.success) {
                    showAdminAlert(`${itemName}已删除`, 'success');
                    
                    // Remove table row
                    const row = document.querySelector(`tr[data-item-id="${itemId}"]`);
                    if (row) {
                        row.style.opacity = '0.5';
                        setTimeout(() => {
                            row.remove();
                            
                            // Check if table is empty
                            const tableBody = row.closest('tbody');
                            if (tableBody && tableBody.querySelectorAll('tr').length === 0) {
                                location.reload();
                            }
                        }, 300);
                    } else {
                        // Reload page if row not found
                        setTimeout(() => {
                            location.reload();
                        }, 1000);
                    }
                } else {
                    showAdminAlert(data.message || '删除失败', 'danger');
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = '<i class="bi bi-trash"></i> 删除';
                    }
                }
            })
            .catch(error => {
                console.error('Error deleting item:', error);
                showAdminAlert('请求失败，请稍后重试', 'danger');
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="bi bi-trash"></i> 删除';
                }
            });
    });
}

/**
 * Change item status via AJAX
 */
function adminChangeStatus(itemId, newStatus, url) {
    const data = { status: newStatus };

    makeRequest(url, 'POST', data)
        .then(response => {
            if (response.success) {
                showAdminAlert('状态已更新', 'success');
                
                // Update status badge in table
                const statusBadge = document.querySelector(`tr[data-item-id="${itemId}"] [data-status-badge]`);
                if (statusBadge) {
                    statusBadge.textContent = getStatusText(newStatus);
                    statusBadge.className = 'badge ' + getStatusBadgeClass(newStatus);
                }
            } else {
                showAdminAlert(response.message || '状态更新失败', 'danger');
            }
        })
        .catch(error => {
            console.error('Error changing status:', error);
            showAdminAlert('请求失败，请稍后重试', 'danger');
        });
}

/**
 * Bulk action on multiple items
 */
function adminBulkAction(action, itemIds) {
    const message = `确定要对选中的 ${itemIds.length} 项执行此操作吗？`;
    
    adminShowConfirmDialog(message, '确认', 'warning').then(confirmed => {
        if (!confirmed) return;

        const data = {
            action: action,
            itemIds: itemIds
        };

        makeRequest(window.location.pathname + '/BulkAction', 'POST', data)
            .then(response => {
                if (response.success) {
                    showAdminAlert(`已成功处理 ${itemIds.length} 项`, 'success');
                    setTimeout(() => {
                        location.reload();
                    }, 1000);
                } else {
                    showAdminAlert(response.message || '操作失败', 'danger');
                }
            })
            .catch(error => {
                console.error('Error performing bulk action:', error);
                showAdminAlert('请求失败，请稍后重试', 'danger');
            });
    });
}

// ============================================================================
// CONFIRMATION DIALOGS
// ============================================================================

/**
 * Initialize admin confirmation dialogs
 */
function initializeAdminConfirmations() {
    // Form submissions with confirmation
    const confirmForms = document.querySelectorAll('form[data-admin-confirm]');
    confirmForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const confirmMessage = this.getAttribute('data-admin-confirm');
            if (confirmMessage) {
                e.preventDefault();
                
                adminShowConfirmDialog(confirmMessage, '确认', 'warning').then(confirmed => {
                    if (confirmed) {
                        form.submit();
                    }
                });
            }
        });
    });

    // Link confirmations
    const confirmLinks = document.querySelectorAll('a[data-admin-confirm]');
    confirmLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            const confirmMessage = this.getAttribute('data-admin-confirm');
            if (confirmMessage) {
                e.preventDefault();
                const href = this.getAttribute('href');
                
                adminShowConfirmDialog(confirmMessage, '确认', 'warning').then(confirmed => {
                    if (confirmed) {
                        window.location.href = href;
                    }
                });
            }
        });
    });
}

/**
 * Show admin confirmation dialog
 */
function adminShowConfirmDialog(message, buttonText = '确认', buttonClass = 'primary') {
    return new Promise((resolve) => {
        // Create modal
        const modalId = 'adminConfirmModal_' + Date.now();
        const modalHtml = `
            <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">确认操作</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            ${message}
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                            <button type="button" class="btn btn-${buttonClass}" id="confirmBtn">${buttonText}</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Add modal to DOM
        const modalContainer = document.createElement('div');
        modalContainer.innerHTML = modalHtml;
        document.body.appendChild(modalContainer);

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById(modalId));
        modal.show();

        // Handle confirm button
        const confirmBtn = document.getElementById('confirmBtn');
        confirmBtn.addEventListener('click', function() {
            modal.hide();
            resolve(true);
            
            // Clean up modal
            setTimeout(() => {
                modalContainer.remove();
            }, 500);
        });

        // Handle cancel
        const cancelBtn = modalContainer.querySelector('[data-bs-dismiss="modal"]');
        cancelBtn.addEventListener('click', function() {
            resolve(false);
            
            // Clean up modal
            setTimeout(() => {
                modalContainer.remove();
            }, 500);
        });

        // Handle backdrop click
        const backdrop = modalContainer.querySelector('.modal-backdrop');
        if (backdrop) {
            backdrop.addEventListener('click', function() {
                resolve(false);
                
                // Clean up modal
                setTimeout(() => {
                    modalContainer.remove();
                }, 500);
            });
        }
    });
}

// ============================================================================
// DATA LOADING
// ============================================================================

/**
 * Initialize admin data loading
 */
function initializeAdminDataLoading() {
    // Load data on tab change
    const navTabs = document.querySelectorAll('[role="tablist"] a[role="tab"]');
    navTabs.forEach(tab => {
        tab.addEventListener('shown.bs.tab', function(e) {
            const tabPane = document.querySelector(this.getAttribute('data-bs-target'));
            if (tabPane && tabPane.getAttribute('data-load-url')) {
                loadAdminData(tabPane);
            }
        });
    });

    // Load data on pagination
    const paginationLinks = document.querySelectorAll('[data-admin-pagination] a');
    paginationLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const url = this.getAttribute('href');
            loadAdminDataByUrl(url);
        });
    });

    // Load data on filter change
    const filterForms = document.querySelectorAll('form[data-admin-filter]');
    filterForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            applyAdminFilter(form);
        });
    });
}

/**
 * Load admin data
 */
function loadAdminData(container) {
    const url = container.getAttribute('data-load-url');
    if (!url) return;

    loadAdminDataByUrl(url, container);
}

/**
 * Load admin data by URL
 */
function loadAdminDataByUrl(url, container = null) {
    if (!container) {
        container = document.querySelector('[data-admin-content]') || document.querySelector('main');
    }

    if (!container) return;

    // Show loading spinner
    container.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">加载中...</span>
            </div>
            <p class="mt-3">加载中...</p>
        </div>
    `;

    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.text();
        })
        .then(html => {
            container.innerHTML = html;
            
            // Re-initialize admin operations for newly loaded content
            initializeAdminTableOperations();
            initializeAdminConfirmations();
            initializeAdminDataLoading();
            
            // Re-initialize Bootstrap components
            reinitializeBootstrapComponents();
        })
        .catch(error => {
            console.error('Error loading admin data:', error);
            container.innerHTML = `
                <div class="alert alert-danger" role="alert">
                    <strong>加载失败</strong>
                    <p>无法加载数据，请稍后重试。</p>
                </div>
            `;
        });
}

/**
 * Apply admin filter
 */
function applyAdminFilter(form) {
    const formData = new FormData(form);
    const params = new URLSearchParams(formData);
    const url = form.getAttribute('action') || window.location.pathname;
    const fullUrl = url + '?' + params.toString();

    loadAdminDataByUrl(fullUrl);
}

// ============================================================================
// FORM OPERATIONS
// ============================================================================

/**
 * Initialize admin form operations
 */
function initializeAdminFormOperations() {
    // Form submission with loading state
    const adminForms = document.querySelectorAll('form[data-admin-form]');
    adminForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                const originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>处理中...';
                
                // Re-enable button after 5 seconds (in case of error)
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 5000);
            }
        });
    });

    // File input preview
    const fileInputs = document.querySelectorAll('input[type="file"][data-admin-preview]');
    fileInputs.forEach(input => {
        input.addEventListener('change', function() {
            const previewContainer = document.querySelector(this.getAttribute('data-admin-preview'));
            if (!previewContainer) return;

            previewContainer.innerHTML = '';

            const files = this.files;
            for (let i = 0; i < files.length; i++) {
                const file = files[i];

                if (file.type.startsWith('image/')) {
                    const reader = new FileReader();

                    reader.onload = function(event) {
                        const previewItem = document.createElement('div');
                        previewItem.className = 'admin-image-preview';
                        previewItem.innerHTML = `
                            <img src="${event.target.result}" alt="Preview" />
                            <small>${file.name}</small>
                        `;
                        previewContainer.appendChild(previewItem);
                    };

                    reader.readAsDataURL(file);
                }
            }
        });
    });
}

// ============================================================================
// STATUS UPDATES
// ============================================================================

/**
 * Initialize admin status updates
 */
function initializeAdminStatusUpdates() {
    // Status update buttons
    const statusUpdateButtons = document.querySelectorAll('button[data-admin-status-update]');
    statusUpdateButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const itemId = this.getAttribute('data-item-id');
            const newStatus = this.getAttribute('data-new-status');
            const url = this.getAttribute('data-url');
            
            adminUpdateStatus(itemId, newStatus, url, this);
        });
    });
}

/**
 * Update item status
 */
function adminUpdateStatus(itemId, newStatus, url, button) {
    const data = { status: newStatus };

    button.disabled = true;
    const originalText = button.innerHTML;
    button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>更新中...';

    makeRequest(url, 'POST', data)
        .then(response => {
            if (response.success) {
                showAdminAlert('状态已更新', 'success');
                
                // Update UI
                const statusDisplay = document.querySelector(`[data-status-display="${itemId}"]`);
                if (statusDisplay) {
                    statusDisplay.textContent = getStatusText(newStatus);
                    statusDisplay.className = 'badge ' + getStatusBadgeClass(newStatus);
                }
                
                // Reload page after 1 second
                setTimeout(() => {
                    location.reload();
                }, 1000);
            } else {
                showAdminAlert(response.message || '状态更新失败', 'danger');
                button.disabled = false;
                button.innerHTML = originalText;
            }
        })
        .catch(error => {
            console.error('Error updating status:', error);
            showAdminAlert('请求失败，请稍后重试', 'danger');
            button.disabled = false;
            button.innerHTML = originalText;
        });
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

/**
 * Show admin alert message
 */
function showAdminAlert(message, type = 'info') {
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
 * Get status text in Chinese
 */
function getStatusText(status) {
    const statusMap = {
        'PendingPay': '待支付',
        'Paid': '已支付',
        'ToUse': '待使用',
        'Used': '已使用',
        'CheckedIn': '已入住',
        'Cancelled': '已取消',
        'Active': '正常',
        'Inactive': '已禁用',
        'Approved': '已批准',
        'Rejected': '已拒绝',
        'Pending': '待审核'
    };
    
    return statusMap[status] || status;
}

/**
 * Get status badge CSS class
 */
function getStatusBadgeClass(status) {
    const classMap = {
        'PendingPay': 'bg-warning',
        'Paid': 'bg-info',
        'ToUse': 'bg-primary',
        'Used': 'bg-success',
        'CheckedIn': 'bg-success',
        'Cancelled': 'bg-danger',
        'Active': 'bg-success',
        'Inactive': 'bg-danger',
        'Approved': 'bg-success',
        'Rejected': 'bg-danger',
        'Pending': 'bg-warning'
    };
    
    return classMap[status] || 'bg-secondary';
}

/**
 * Format admin date
 */
function formatAdminDate(date, format = 'YYYY-MM-DD HH:mm') {
    if (typeof date === 'string') {
        date = new Date(date);
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return format
        .replace('YYYY', year)
        .replace('MM', month)
        .replace('DD', day)
        .replace('HH', hours)
        .replace('mm', minutes);
}

/**
 * Export table to CSV
 */
function exportTableToCsv(tableSelector, filename = 'export.csv') {
    const table = document.querySelector(tableSelector);
    if (!table) return;

    let csv = [];
    const rows = table.querySelectorAll('tr');

    rows.forEach(row => {
        const cols = row.querySelectorAll('td, th');
        const csvRow = [];

        cols.forEach(col => {
            csvRow.push('"' + col.textContent.trim().replace(/"/g, '""') + '"');
        });

        csv.push(csvRow.join(','));
    });

    downloadCsv(csv.join('\n'), filename);
}

/**
 * Download CSV file
 */
function downloadCsv(csv, filename) {
    const link = document.createElement('a');
    link.href = 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv);
    link.download = filename;
    link.click();
}

/**
 * Print admin page
 */
function printAdminPage(selector = 'main') {
    const printContent = document.querySelector(selector);
    if (!printContent) return;

    const printWindow = window.open('', '', 'height=600,width=800');
    printWindow.document.write('<html><head><title>打印</title>');
    printWindow.document.write('<link rel="stylesheet" href="/css/bootstrap.min.css">');
    printWindow.document.write('<link rel="stylesheet" href="/css/admin.css">');
    printWindow.document.write('</head><body>');
    printWindow.document.write(printContent.innerHTML);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.print();
}

