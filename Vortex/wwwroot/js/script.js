document.addEventListener("DOMContentLoaded", function () {

    // --- Chức năng đóng/mở Sidebar ---
    const sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
    const sidebar = document.getElementById('sidebar');
    const contentWrapper = document.querySelector('.content-wrapper');

    if (sidebarToggleBtn && sidebar) {
        sidebarToggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('open');
            // Logic để đẩy content ra khi sidebar mở trên desktop
            // Trên mobile, sidebar sẽ đè lên content
            if (window.innerWidth >= 992) {
                if (sidebar.classList.contains('open')) {
                    if (contentWrapper) contentWrapper.style.marginLeft = '260px';
                } else {
                    if (contentWrapper) contentWrapper.style.marginLeft = '0';
                }
            }
        });
    }

    
    
    // --- Chức năng Gallery ảnh sản phẩm ---
    const mainImage = document.getElementById('main-product-image');
    const thumbnails = document.querySelectorAll('.thumbnail-container .img-thumbnail');

    if (mainImage && thumbnails.length > 0) {
        thumbnails.forEach(thumb => {
            thumb.addEventListener('click', function() {
                // Đổi ảnh chính
                mainImage.src = this.src.replace('100x100', '600x600'); // Giả định tên file ảnh lớn
                
                // Cập nhật class active
                thumbnails.forEach(t => t.classList.remove('active'));
                this.classList.add('active');
            });
        });
    }

    // --- Chức năng đánh giá sao (form) ---
    const starRatingForm = document.querySelector('.star-rating-form');
    if(starRatingForm){
        const stars = starRatingForm.querySelectorAll('i');
        stars.forEach((star, index) => {
            star.addEventListener('click', () => {
                // Tô màu các sao từ đầu đến sao được click
                for(let i=0; i<= index; i++) {
                    stars[i].classList.remove('far');
                    stars[i].classList.add('fas');
                }
                // Bỏ màu các sao phía sau
                 for(let i=index+1; i< stars.length; i++) {
                    stars[i].classList.remove('fas');
                    stars[i].classList.add('far');
                }
            });
        });
    }

});
