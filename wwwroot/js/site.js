/**
 * El Shaib - Core Client-Side Utilities & Animations
 */

(function () {
    'use strict';

    /**
     * Fly to Cart & Product Minimize Animation
     * @param {HTMLElement} sourceElement - The button, card, or image clicked.
     * @param {Object} [options] - Optional configurations (e.g. imgSrc, onComplete).
     */
    window.flyToCart = function (sourceElement, options = {}) {
        if (!sourceElement) return;

        // Prevent duplicate trigger within 400ms on same element
        const now = Date.now();
        if (sourceElement._lastFlyTime && now - sourceElement._lastFlyTime < 400) {
            return;
        }
        sourceElement._lastFlyTime = now;

        // 1. Locate the Cart icon in navbar
        const cartTarget = document.getElementById('navbarCartBtn') ||
            document.querySelector('.navbar-actions a[href*="Cart"]') ||
            document.querySelector('.navbar-actions .fa-shopping-cart')?.closest('a') ||
            document.querySelector('.cart-badge')?.closest('a');

        // 2. Find product image to fly
        let imgEl = null;
        if (sourceElement.tagName === 'IMG') {
            imgEl = sourceElement;
        } else {
            const parentCard = sourceElement.closest('.pcard, .product-card, .pd-gallery, .pd-main-image-box, .col-lg-6, .card, .product-item') || document;
            imgEl = parentCard.querySelector('img#pdMainImage, img#pdMainImg, .pcard-image img, .product-image-wrapper img, .card-img-top, img') ||
                document.getElementById('pdMainImage');
        }

        // 3. Card minimize/press micro-animation
        const cardEl = sourceElement.closest('.pcard, .product-card, .card, .pd-main-image-box');
        if (cardEl) {
            cardEl.classList.remove('product-card-press');
            void cardEl.offsetWidth;
            cardEl.classList.add('product-card-press');
            setTimeout(() => cardEl.classList.remove('product-card-press'), 300);
        }

        // 4. Source starting dimensions & coordinates
        const startEl = (imgEl && imgEl.offsetParent !== null && imgEl.getBoundingClientRect().width > 0) ? imgEl : sourceElement;
        const startRect = startEl.getBoundingClientRect();
        if (startRect.width === 0 || startRect.height === 0) return;

        const targetRect = cartTarget ? cartTarget.getBoundingClientRect() : null;

        // Fallback target coordinates (e.g. mobile collapsed menu)
        let targetCenterX = window.innerWidth - 40;
        let targetCenterY = 30;
        if (targetRect && targetRect.width > 0 && targetRect.top > -50 && targetRect.left > 0) {
            targetCenterX = targetRect.left + targetRect.width / 2;
            targetCenterY = targetRect.top + targetRect.height / 2;
        }

        const startCenterX = startRect.left + startRect.width / 2;
        const startCenterY = startRect.top + startRect.height / 2;
        const dx = targetCenterX - startCenterX;
        const dy = targetCenterY - startCenterY;

        const imgSrc = options.imgSrc || (imgEl ? (imgEl.currentSrc || imgEl.src) : '/images/HeroImage.jpg');

        // 5. Create flyer element
        const flyer = document.createElement('div');
        flyer.className = 'fly-to-cart-flying-item';

        const flyerImg = document.createElement('img');
        flyerImg.src = imgSrc;
        flyerImg.alt = 'Flying product';
        flyer.appendChild(flyerImg);

        flyer.style.cssText = `
            position: fixed;
            left: ${startRect.left}px;
            top: ${startRect.top}px;
            width: ${startRect.width}px;
            height: ${startRect.height}px;
            z-index: 999999;
            pointer-events: none;
            overflow: hidden;
            border-radius: 16px;
            box-shadow: 0 12px 35px rgba(0,0,0,0.28), 0 0 0 2px rgba(255,255,255,0.85);
            background: #fff;
            transform-origin: center center;
            will-change: transform, opacity, border-radius;
        `;

        document.body.appendChild(flyer);

        // 6. Arc trajectory: starts full size -> scales down / minimizes -> curves into cart
        const anim = flyer.animate([
            {
                transform: 'translate3d(0, 0, 0) scale(1) rotate(0deg)',
                opacity: 1,
                borderRadius: '16px'
            },
            {
                offset: 0.18,
                transform: `translate3d(${dx * 0.12}px, ${dy * 0.12 - 35}px, 0) scale(0.72) rotate(-6deg)`,
                opacity: 1,
                borderRadius: '24px'
            },
            {
                offset: 0.65,
                transform: `translate3d(${dx * 0.68}px, ${dy * 0.68 - 22}px, 0) scale(0.32) rotate(-18deg)`,
                opacity: 0.95,
                borderRadius: '50%'
            },
            {
                offset: 1,
                transform: `translate3d(${dx}px, ${dy}px, 0) scale(0.08) rotate(-35deg)`,
                opacity: 0.1,
                borderRadius: '50%'
            }
        ], {
            duration: 700,
            easing: 'cubic-bezier(0.2, 0.9, 0.3, 1)',
            fill: 'forwards'
        });

        anim.onfinish = function () {
            flyer.remove();

            // Bounce cart icon
            if (cartTarget) {
                cartTarget.classList.remove('cart-bounce');
                void cartTarget.offsetWidth;
                cartTarget.classList.add('cart-bounce');
                setTimeout(() => cartTarget.classList.remove('cart-bounce'), 600);
            }

            // Pop badge
            const badges = document.querySelectorAll('.cart-badge');
            badges.forEach(b => {
                b.classList.remove('badge-pop');
                void b.offsetWidth;
                b.classList.add('badge-pop');
            });
            setTimeout(() => badges.forEach(b => b.classList.remove('badge-pop')), 400);

            if (typeof options.onComplete === 'function') {
                options.onComplete();
            }
        };
    };

    // Global listener for dynamic add-to-cart clicks as safety fallback
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-add-cart-ajax, .btn-home-add-cart, .pd-add-cart-btn, .btn-add-cart-from-wishlist, [data-fly-to-cart]');
        if (btn) {
            window.flyToCart(btn);
        }
    });
})();
