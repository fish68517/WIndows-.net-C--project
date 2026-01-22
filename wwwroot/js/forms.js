// Form-specific interactions and validations

document.addEventListener('DOMContentLoaded', function() {
    // Initialize registration form validation
    initializeRegistrationForm();
    
    // Initialize login form
    initializeLoginForm();
    
    // Initialize order forms
    initializeOrderForms();
    
    // Initialize diary form
    initializeDiaryForm();
    
    // Initialize comment form
    initializeCommentForm();
    
    // Initialize search and filter forms
    initializeSearchForms();
});

// ============================================================================
// REGISTRATION FORM
// ============================================================================

function initializeRegistrationForm() {
    const form = document.querySelector('form[asp-action="Register"]');
    if (!form) return;

    const emailInput = form.querySelector('input[asp-for="Email"]');
    const nicknameInput = form.querySelector('input[asp-for="Nickname"]');
    const passwordInput = form.querySelector('input[asp-for="Password"]');
    const confirmPasswordInput = form.querySelector('input[asp-for="ConfirmPassword"]');

    // Email validation
    if (emailInput) {
        emailInput.addEventListener('blur', function() {
            if (this.value && !validateEmail(this.value)) {
                showFieldError(this, '请输入有效的邮箱地址');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Nickname validation
    if (nicknameInput) {
        nicknameInput.addEventListener('blur', function() {
            const length = this.value.trim().length;
            if (length < 2 || length > 50) {
                showFieldError(this, '昵称长度必须在2-50个字符之间');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Password validation
    if (passwordInput) {
        passwordInput.addEventListener('blur', function() {
            if (this.value && !validatePasswordStrength(this.value)) {
                showFieldError(this, '密码必须至少8个字符，包含大小写字母和数字');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Confirm password validation
    if (confirmPasswordInput && passwordInput) {
        confirmPasswordInput.addEventListener('blur', function() {
            if (this.value !== passwordInput.value) {
                showFieldError(this, '两次输入的密码不一致');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Form submission
    form.addEventListener('submit', function(e) {
        let isValid = true;

        if (emailInput && !validateEmail(emailInput.value)) {
            showFieldError(emailInput, '请输入有效的邮箱地址');
            isValid = false;
        }

        if (nicknameInput) {
            const length = nicknameInput.value.trim().length;
            if (length < 2 || length > 50) {
                showFieldError(nicknameInput, '昵称长度必须在2-50个字符之间');
                isValid = false;
            }
        }

        if (passwordInput && !validatePasswordStrength(passwordInput.value)) {
            showFieldError(passwordInput, '密码必须至少8个字符，包含大小写字母和数字');
            isValid = false;
        }

        if (confirmPasswordInput && confirmPasswordInput.value !== passwordInput.value) {
            showFieldError(confirmPasswordInput, '两次输入的密码不一致');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
        }
    });
}

// ============================================================================
// LOGIN FORM
// ============================================================================

function initializeLoginForm() {
    const form = document.querySelector('form[asp-action="Login"]');
    if (!form) return;

    const emailInput = form.querySelector('input[type="email"]');
    const passwordInput = form.querySelector('input[type="password"]');

    // Email validation
    if (emailInput) {
        emailInput.addEventListener('blur', function() {
            if (this.value && !validateEmail(this.value)) {
                showFieldError(this, '请输入有效的邮箱地址');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Password validation
    if (passwordInput) {
        passwordInput.addEventListener('blur', function() {
            if (this.value.length < 1) {
                showFieldError(this, '请输入密码');
            } else {
                clearFieldError(this);
            }
        });
    }
}

// ============================================================================
// ORDER FORMS
// ============================================================================

function initializeOrderForms() {
    // Ticket order form
    initializeTicketOrderForm();
    
    // Hotel order form
    initializeHotelOrderForm();
}

function initializeTicketOrderForm() {
    const form = document.querySelector('form[asp-action="CreateTicket"]');
    if (!form) return;

    const quantityInput = form.querySelector('input[name="Quantity"]');
    const visitDateInput = form.querySelector('input[name="VisitDate"]');
    const decreaseBtn = document.getElementById('decreaseBtn');
    const increaseBtn = document.getElementById('increaseBtn');
    const unitPriceElement = document.getElementById('unitPrice');

    if (!quantityInput || !unitPriceElement) return;

    const unitPrice = parseFloat(unitPriceElement.textContent);

    function updateTotal() {
        const quantity = parseInt(quantityInput.value) || 1;
        const total = (unitPrice * quantity).toFixed(2);
        
        const quantityDisplay = document.getElementById('quantityDisplay');
        const totalPriceDisplay = document.getElementById('totalPrice');
        
        if (quantityDisplay) quantityDisplay.textContent = quantity;
        if (totalPriceDisplay) totalPriceDisplay.textContent = total;
    }

    // Decrease quantity
    if (decreaseBtn) {
        decreaseBtn.addEventListener('click', function() {
            let quantity = parseInt(quantityInput.value) || 1;
            if (quantity > 1) {
                quantityInput.value = quantity - 1;
                updateTotal();
            }
        });
    }

    // Increase quantity
    if (increaseBtn) {
        increaseBtn.addEventListener('click', function() {
            let quantity = parseInt(quantityInput.value) || 1;
            if (quantity < 100) {
                quantityInput.value = quantity + 1;
                updateTotal();
            }
        });
    }

    // Update on input change
    quantityInput.addEventListener('change', updateTotal);

    // Validate visit date
    if (visitDateInput) {
        visitDateInput.addEventListener('change', function() {
            const selectedDate = new Date(this.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                showFieldError(this, '请选择今天或之后的日期');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Form submission validation
    form.addEventListener('submit', function(e) {
        let isValid = true;

        if (!visitDateInput.value) {
            showFieldError(visitDateInput, '请选择访问日期');
            isValid = false;
        }

        const quantity = parseInt(quantityInput.value) || 0;
        if (quantity < 1 || quantity > 100) {
            showFieldError(quantityInput, '购票数量必须在1-100之间');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
        }
    });
}

function initializeHotelOrderForm() {
    const form = document.querySelector('form[asp-action="CreateHotel"]');
    if (!form) return;

    const checkInInput = form.querySelector('input[name="CheckInDate"]');
    const checkOutInput = form.querySelector('input[name="CheckOutDate"]');

    // Validate check-in date
    if (checkInInput) {
        checkInInput.addEventListener('change', function() {
            const selectedDate = new Date(this.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                showFieldError(this, '请选择今天或之后的日期');
            } else {
                clearFieldError(this);
                // Update check-out minimum date
                if (checkOutInput) {
                    checkOutInput.min = this.value;
                }
            }
        });
    }

    // Validate check-out date
    if (checkOutInput && checkInInput) {
        checkOutInput.addEventListener('change', function() {
            const checkInDate = new Date(checkInInput.value);
            const checkOutDate = new Date(this.value);

            if (checkOutDate <= checkInDate) {
                showFieldError(this, '离店日期必须晚于入住日期');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Form submission validation
    form.addEventListener('submit', function(e) {
        let isValid = true;

        if (!checkInInput.value) {
            showFieldError(checkInInput, '请选择入住日期');
            isValid = false;
        }

        if (!checkOutInput.value) {
            showFieldError(checkOutInput, '请选择离店日期');
            isValid = false;
        }

        if (checkInInput.value && checkOutInput.value) {
            const checkInDate = new Date(checkInInput.value);
            const checkOutDate = new Date(checkOutInput.value);

            if (checkOutDate <= checkInDate) {
                showFieldError(checkOutInput, '离店日期必须晚于入住日期');
                isValid = false;
            }
        }

        if (!isValid) {
            e.preventDefault();
        }
    });
}

// ============================================================================
// DIARY FORM
// ============================================================================

function initializeDiaryForm() {
    const form = document.querySelector('form[asp-action="Create"][enctype="multipart/form-data"]');
    if (!form) return;

    const titleInput = form.querySelector('input[name="Title"]');
    const contentInput = form.querySelector('textarea[name="Content"]');
    const imageInput = form.querySelector('input[type="file"][name="Images"]');
    const imagePreview = document.getElementById('imagePreview');

    // Title validation
    if (titleInput) {
        titleInput.addEventListener('blur', function() {
            const length = this.value.trim().length;
            if (length < 1) {
                showFieldError(this, '请输入标题');
            } else if (length > 200) {
                showFieldError(this, '标题不能超过200个字符');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Content validation
    if (contentInput) {
        contentInput.addEventListener('blur', function() {
            const length = this.value.trim().length;
            if (length < 1) {
                showFieldError(this, '请输入内容');
            } else if (length > 5000) {
                showFieldError(this, '内容不能超过5000个字符');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Image preview
    if (imageInput && imagePreview) {
        imageInput.addEventListener('change', function(e) {
            imagePreview.innerHTML = '';

            const files = e.target.files;
            if (files.length > 10) {
                showAlert('最多只能上传10张图片', 'warning');
                return;
            }

            for (let i = 0; i < files.length; i++) {
                const file = files[i];
                
                // Validate file type
                if (!file.type.startsWith('image/')) {
                    showAlert(`文件 ${file.name} 不是有效的图片格式`, 'warning');
                    continue;
                }

                // Validate file size (max 5MB)
                if (file.size > 5 * 1024 * 1024) {
                    showAlert(`文件 ${file.name} 超过5MB限制`, 'warning');
                    continue;
                }

                const reader = new FileReader();

                reader.onload = function(event) {
                    const previewItem = document.createElement('div');
                    previewItem.className = 'image-preview-item';
                    previewItem.innerHTML = `<img src="${event.target.result}" alt="Preview" />`;
                    imagePreview.appendChild(previewItem);
                };

                reader.readAsDataURL(file);
            }
        });
    }

    // Form submission validation
    form.addEventListener('submit', function(e) {
        let isValid = true;

        if (titleInput && titleInput.value.trim().length < 1) {
            showFieldError(titleInput, '请输入标题');
            isValid = false;
        }

        if (contentInput && contentInput.value.trim().length < 1) {
            showFieldError(contentInput, '请输入内容');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
        }
    });
}

// ============================================================================
// COMMENT FORM
// ============================================================================

function initializeCommentForm() {
    const form = document.querySelector('form[asp-action="Create"][asp-controller="Comments"]');
    if (!form) return;

    const contentInput = form.querySelector('textarea[name="Content"]');
    const ratingInput = form.querySelector('input[name="Rating"]');

    // Content validation
    if (contentInput) {
        contentInput.addEventListener('blur', function() {
            const length = this.value.trim().length;
            if (length < 1) {
                showFieldError(this, '请输入评论内容');
            } else if (length > 1000) {
                showFieldError(this, '评论不能超过1000个字符');
            } else {
                clearFieldError(this);
            }
        });
    }

    // Form submission validation
    form.addEventListener('submit', function(e) {
        let isValid = true;

        if (contentInput && contentInput.value.trim().length < 1) {
            showFieldError(contentInput, '请输入评论内容');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
        }
    });
}

// ============================================================================
// SEARCH AND FILTER FORMS
// ============================================================================

function initializeSearchForms() {
    // Attraction search form
    const searchForm = document.querySelector('form[asp-action="Index"][asp-controller="Attractions"]');
    if (searchForm) {
        const searchInput = searchForm.querySelector('input[name="keyword"]');
        const districtSelect = searchForm.querySelector('select[name="districtId"]');
        const categorySelect = searchForm.querySelector('select[name="categoryId"]');

        // Debounce search input
        if (searchInput) {
            searchInput.addEventListener('input', debounce(function() {
                // Auto-submit form on search input change
                // searchForm.submit();
            }, 500));
        }

        // Auto-submit on filter change
        if (districtSelect) {
            districtSelect.addEventListener('change', function() {
                searchForm.submit();
            });
        }

        if (categorySelect) {
            categorySelect.addEventListener('change', function() {
                searchForm.submit();
            });
        }
    }
}
