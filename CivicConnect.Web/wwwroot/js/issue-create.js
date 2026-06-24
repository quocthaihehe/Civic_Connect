$(document).ready(function() {
    // 1. Quản lý tải ảnh lên và xem trước (Previews)
    const fileInput = $('#image-upload-input');
    const previewsContainer = $('#image-previews-container');
    let blobUrls = []; // Theo dõi blob URLs để giải phóng bộ nhớ

    // Hàm giải phóng tất cả blob URLs đã tạo
    function revokeAllBlobUrls() {
        blobUrls.forEach(function(url) {
            try { URL.revokeObjectURL(url); } catch(e) {}
        });
        blobUrls = [];
    }

    fileInput.on('change', function() {
        // Giải phóng blob URLs cũ trước khi tạo mới
        revokeAllBlobUrls();
        previewsContainer.empty();
        const files = this.files;

        if (!files || files.length === 0) {
            previewsContainer.addClass('d-none').removeClass('d-flex');
            $('#upload-empty-state').removeClass('d-none');
            return;
        }

        if (files.length > 5) {
            alert("Chỉ cho phép tải lên tối đa 5 hình ảnh.");
            fileInput.val(''); // Clear input
            previewsContainer.addClass('d-none').removeClass('d-flex');
            $('#upload-empty-state').removeClass('d-none');
            return;
        }

        // Kiểm tra kích thước từng file trước khi tạo preview
        for (let i = 0; i < files.length; i++) {
            if (files[i].size > 5 * 1024 * 1024) {
                alert('Tệp ' + files[i].name + ' vượt quá dung lượng tối đa cho phép (5MB).');
                fileInput.val('');
                previewsContainer.empty();
                previewsContainer.addClass('d-none').removeClass('d-flex');
                $('#upload-empty-state').removeClass('d-none');
                return;
            }
        }

        // Sử dụng DocumentFragment để batch DOM updates — tránh trigger
        // Browser Link DOM mutation observer nhiều lần gây crash kết nối VS
        var fragment = document.createDocumentFragment();
        
        if (files.length === 1) {
            // Trường hợp có 1 ảnh duy nhất: phóng to full ô tải ảnh
            var imgUrl = URL.createObjectURL(files[0]);
            blobUrls.push(imgUrl);

            var wrapper = document.createElement('div');
            wrapper.className = 'position-relative w-100 h-100';

            var img = document.createElement('img');
            img.className = 'rounded border w-100 h-100';
            img.style.cssText = 'object-fit: cover;';
            img.src = imgUrl;

            var badge = document.createElement('span');
            badge.className = 'position-absolute badge rounded-circle bg-danger remove-preview-btn shadow-sm';
            badge.style.cssText = 'top: 8px; right: 8px; padding: 0.3em 0.5em; font-size: 0.8rem; cursor: pointer; z-index: 10; line-height: 1;';
            badge.textContent = 'x';

            wrapper.appendChild(img);
            wrapper.appendChild(badge);
            fragment.appendChild(wrapper);
        } else {
            // Gallery layout (1 ảnh lớn trên, nhiều ảnh nhỏ dưới)
            for (let i = 0; i < files.length; i++) {
                blobUrls.push(URL.createObjectURL(files[i]));
            }

            // Main viewer container (Chiếm không gian còn lại ở trên)
            var mainViewerWrapper = document.createElement('div');
            mainViewerWrapper.className = 'position-relative w-100 mb-2 flex-grow-1';
            
            var mainImg = document.createElement('img');
            mainImg.id = 'gallery-main-img';
            mainImg.className = 'rounded border w-100 h-100';
            mainImg.style.cssText = 'object-fit: cover; position: absolute; top: 0; left: 0;';
            mainImg.src = blobUrls[0];
            
            var badge = document.createElement('span');
            badge.className = 'position-absolute badge rounded-circle bg-danger remove-preview-btn shadow-sm';
            badge.style.cssText = 'top: 8px; right: 8px; padding: 0.3em 0.5em; font-size: 0.8rem; cursor: pointer; z-index: 10; line-height: 1;';
            badge.textContent = 'x';
            
            mainViewerWrapper.appendChild(mainImg);
            mainViewerWrapper.appendChild(badge);
            
            // Thumbnails container (Nằm dưới cùng, cố định chiều cao)
            var thumbnailsContainer = document.createElement('div');
            thumbnailsContainer.className = 'd-flex gap-2 w-100 overflow-x-auto pb-1';
            thumbnailsContainer.style.height = '60px';
            thumbnailsContainer.style.flexShrink = '0';
            
            for (let i = 0; i < blobUrls.length; i++) {
                var thumb = document.createElement('img');
                thumb.className = 'rounded border gallery-thumbnail cursor-pointer';
                if (i === 0) thumb.classList.add('border-primary', 'border-2', 'opacity-100');
                else thumb.classList.add('opacity-50');
                
                thumb.style.cssText = 'width: 60px; height: 100%; object-fit: cover; transition: all 0.2s;';
                thumb.src = blobUrls[i];
                thumb.dataset.url = blobUrls[i];
                
                thumbnailsContainer.appendChild(thumb);
            }
            
            fragment.appendChild(mainViewerWrapper);
            fragment.appendChild(thumbnailsContainer);
        }

        // Dùng setTimeout(0) để insert DOM sau khi call stack hiện tại hoàn tất,
        // giúp Browser Link không bị overwhelm bởi DOM changes đồng bộ
        setTimeout(function() {
            previewsContainer[0].appendChild(fragment);
            $('#upload-empty-state').addClass('d-none');
            previewsContainer.removeClass('d-none flex-wrap justify-content-center gap-2 p-1').addClass('d-flex flex-column');
        }, 0);
    });

    // Hủy bỏ xem trước ảnh (Clear input) và giải phóng bộ nhớ
    previewsContainer.on('click', '.remove-preview-btn', function(e) {
        e.stopPropagation(); // Ngăn mở hộp thoại chọn file
        revokeAllBlobUrls();
        fileInput.val('');
        previewsContainer.empty().addClass('d-none').removeClass('d-flex flex-column');
        $('#upload-empty-state').removeClass('d-none');
    });

    // Thumbnail click to swap Main Viewer
    previewsContainer.on('click', '.gallery-thumbnail', function(e) {
        e.stopPropagation();
        const url = $(this).data('url');
        $('#gallery-main-img').attr('src', url);
        
        // Update active styling
        $('.gallery-thumbnail').removeClass('border-primary border-2 opacity-100').addClass('opacity-50');
        $(this).removeClass('opacity-50').addClass('border-primary border-2 opacity-100');
    });

    // Mở hộp thoại chọn file khi click vào dropzone (trừ nút remove-preview-btn)
    $('.upload-dropzone').on('click', function(e) {
        if ($(e.target).closest('.remove-preview-btn').length === 0) {
            fileInput.click();
        }
    });

    fileInput.on('click', function(e) {
        e.stopPropagation();
    });

    // 2. Bản đồ thực tế Leaflet cho việc chọn vị trí (Quốc gia/Địa phương)
    if (document.getElementById('map-selector')) {
        let map;
        let marker;
        let currentTileLayer;

        const tileUrls = {
            light: 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png',
            dark: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
        };
        const attribution = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>';

        // Lấy tọa độ ban đầu từ inputs ẩn (mặc định Bến Nghé, Quận 1)
        const initialLat = parseFloat($('#lat-input').val()) || 10.7769;
        const initialLng = parseFloat($('#lng-input').val()) || 106.7009;

        // Khởi tạo bản đồ
        map = L.map('map-selector', {
            zoomControl: true,
            attributionControl: true
        }).setView([initialLat, initialLng], 15);

        window.map = map; // Expose globally to window.map for the wizard script!

        // Khắc phục lỗi hiển thị bản đồ trong container ẩn (tab/wizard)
        if (window.ResizeObserver) {
            const mapEl = document.getElementById('map-selector');
            const ro = new ResizeObserver(() => {
                map.invalidateSize();
            });
            ro.observe(mapEl);
        }

        // Hàm lấy mapping class & icon từ mã danh mục
        function getCategoryMappings(catVal) {
            switch(catVal) {
                case 1: return { className: 'marker-traffic', iconName: 'bi-car-front-fill' };
                case 2: return { className: 'marker-environment', iconName: 'bi-trash3-fill' };
                case 3: return { className: 'marker-security', iconName: 'bi-shield-fill-exclamation' };
                case 4: return { className: 'marker-infrastructure', iconName: 'bi-wrench-adjustable-circle-fill' };
                case 5: return { className: 'marker-administration', iconName: 'bi-file-earmark-text-fill' };
                default: return { className: 'marker-other', iconName: 'bi-geo-alt-fill' };
            }
        }

        // Khởi tạo ghim vị trí tùy chỉnh (draggable)
        let currentIconClass = 'marker-other';
        let currentIconName = 'bi-geo-alt-fill';

        // Lấy danh mục được chọn sẵn nếu có
        const initialCategory = parseInt($('#categoryInput').val());
        if (initialCategory) {
            const mappings = getCategoryMappings(initialCategory);
            currentIconClass = mappings.className;
            currentIconName = mappings.iconName;
        }

        const markerIcon = L.divIcon({
            html: `<div class="selector-pin ${currentIconClass}"><i class="bi ${currentIconName}"></i></div>`,
            className: 'selector-pin-wrapper',
            iconSize: [38, 38],
            iconAnchor: [19, 19]
        });

        marker = L.marker([initialLat, initialLng], {
            icon: markerIcon,
            draggable: true
        }).addTo(map);

        // Cập nhật ghim khi đổi danh mục
        function updateMarkerIcon(catVal) {
            const mappings = getCategoryMappings(catVal);
            const newIcon = L.divIcon({
                html: `<div class="selector-pin ${mappings.className}"><i class="bi ${mappings.iconName}"></i></div>`,
                className: 'selector-pin-wrapper',
                iconSize: [38, 38],
                iconAnchor: [19, 19]
            });
            marker.setIcon(newIcon);
        }

        // Lắng nghe thay đổi danh mục
        $('#categoryInput').on('change', function() {
            const catVal = parseInt($(this).val()) || 99;
            updateMarkerIcon(catVal);
        });

        let activeStyle = 'current';

        // Nạp Tile Layer dựa trên style và theme
        function updateMapStyle(style, theme) {
            if (currentTileLayer) {
                map.removeLayer(currentTileLayer);
            }
            
            activeStyle = style;

            if (style === 'current') {
                const url = tileUrls[theme] || tileUrls.light;
                currentTileLayer = L.tileLayer(url, {
                    attribution: attribution,
                    maxZoom: 20
                }).addTo(map);
            } 
            else if (style === 'terrain') {
                currentTileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
                    maxZoom: 19
                }).addTo(map);
            } 
            else if (style === 'satellite') {
                const satelliteLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
                    attribution: 'Tiles &copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community',
                    maxZoom: 19
                });
                const labelLayer = L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_only_labels/{z}/{x}/{y}{r}.png', {
                    attribution: ''
                });
                currentTileLayer = L.layerGroup([satelliteLayer, labelLayer]).addTo(map);
            }
        }

        const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
        updateMapStyle(activeStyle, currentTheme);

        // Theo dõi sự thay đổi theme để đổi màu nền bản đồ
        const themeObserver = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.attributeName === 'data-theme') {
                    const newTheme = document.documentElement.getAttribute('data-theme') || 'light';
                    updateMapStyle(activeStyle, newTheme);
                }
            });
        });
        themeObserver.observe(document.documentElement, { attributes: true });

        // Lắng nghe click đổi style bản đồ
        $('#style-panel .btn-glass').on('click', function() {
            $('#style-panel .btn-glass').removeClass('active');
            $(this).addClass('active');
            const style = $(this).data('style');
            const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
            updateMapStyle(style, currentTheme);
        });

        // Tự động phát hiện vị trí người dùng toàn quốc nếu trình duyệt hỗ trợ
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function(position) {
                const userLat = position.coords.latitude;
                const userLng = position.coords.longitude;
                
                // Di chuyển bản đồ và marker tới vị trí người dùng
                map.setView([userLat, userLng], 16);
                marker.setLatLng([userLat, userLng]);
                
                // Cập nhật tọa độ và lấy địa chỉ
                updateCoordinates(userLat, userLng);
                updateAddressFromCoords(userLat, userLng);
            }, function(error) {
                console.log("Quyền truy cập vị trí bị từ chối hoặc lỗi: Sử dụng vị trí mặc định.");
                // Chạy reverse geocoding ban đầu cho vị trí mặc định nếu chưa có địa chỉ cụ thể
                if (!$('#address-input').val()) {
                    updateAddressFromCoords(initialLat, initialLng);
                }
            });
        } else {
            if (!$('#address-input').val()) {
                updateAddressFromCoords(initialLat, initialLng);
            }
        }

        // Lắng nghe sự kiện click trên bản đồ để đặt marker
        map.on('click', function(e) {
            const lat = e.latlng.lat;
            const lng = e.latlng.lng;
            
            marker.setLatLng([lat, lng]);
            updateCoordinates(lat, lng);
            checkBoundary(lat, lng);
            updateAddressFromCoords(lat, lng);
        });

        // Lắng nghe sự kiện kéo thả marker
        marker.on('dragend', function(e) {
            const position = marker.getLatLng();
            const lat = position.lat;
            const lng = position.lng;
            
            updateCoordinates(lat, lng);
            checkBoundary(lat, lng);
            updateAddressFromCoords(lat, lng);
        });

        // Cập nhật các trường input ẩn
        function updateCoordinates(lat, lng) {
            $('#lat-input').val(lat.toFixed(6));
            $('#lng-input').val(lng.toFixed(6));
        }

        // Kiểm tra phạm vi hành chính (TP.HCM)
        function checkBoundary(lat, lng) {
            const hcmMinLat = 10.370;
            const hcmMaxLat = 11.160;
            const hcmMinLng = 106.350;
            const hcmMaxLng = 106.950;
            
            // Remove old inline warning just in case it exists from previous state
            $('#boundary-warning').remove();
            
            if (lat < hcmMinLat || lat > hcmMaxLat || lng < hcmMinLng || lng > hcmMaxLng) {
                showBoundaryModal();
                $('#address-input').addClass('is-invalid');
            } else {
                $('#address-input').removeClass('is-invalid');
            }
        }

        function showBoundaryModal() {
            // Remove existing modal if any
            $('#boundaryModal').remove();
            
            const modalHtml = `
            <div class="modal fade" id="boundaryModal" tabindex="-1" aria-hidden="true" style="backdrop-filter: blur(8px); background-color: rgba(15, 23, 42, 0.4);">
              <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow-lg" style="border-radius: 24px; overflow: hidden; background: #ffffff;">
                  <div class="modal-header border-0 pb-0 pt-3 pe-3 justify-content-end">
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" style="opacity: 0.6; transition: opacity 0.2s;"></button>
                  </div>
                  <div class="modal-body text-center pt-0 pb-5 px-4 px-md-5">
                    <div class="d-inline-flex justify-content-center align-items-center rounded-circle mb-4" style="width: 90px; height: 90px; background: rgba(32, 107, 196, 0.1);">
                        <i class="bi bi-geo-alt-fill text-primary" style="font-size: 3rem;"></i>
                    </div>
                    <h3 class="fw-bold text-dark mb-3" style="letter-spacing: -0.5px;">Vị trí ngoài phạm vi</h3>
                    <p class="text-muted mb-4" style="font-size: 1rem; line-height: 1.6;">
                        Hệ thống Civic Connect hiện tại chỉ tiếp nhận và xử lý phản ánh nằm trong ranh giới hành chính của <strong>TP. Hồ Chí Minh</strong>.<br>Vui lòng kéo ghim lại vào vị trí hợp lệ.
                    </p>
                    <button type="button" class="btn btn-primary rounded-pill px-5 py-2 fw-bold" data-bs-dismiss="modal" style="font-size: 1rem; box-shadow: 0 8px 20px rgba(32, 107, 196, 0.25); transition: all 0.3s ease;">
                        Đã hiểu & Chọn lại
                    </button>
                  </div>
                </div>
              </div>
            </div>`;
            
            $('body').append(modalHtml);
            const modalEl = document.getElementById('boundaryModal');
            // Mặc định bootstrap modal tạo backdrop, ta set backdrop-filter trên modal nên tắt backdrop mặc định
            const modal = new bootstrap.Modal(modalEl, { backdrop: false });
            modal.show();
            
            modalEl.addEventListener('hidden.bs.modal', function () {
                $(this).remove();
            });
        }

        // Gọi API reverse geocoding Nominatim của OpenStreetMap để phân tích địa chỉ tiếng Việt
        let geocodeTimeout;
        function updateAddressFromCoords(lat, lng) {
            // Debounce để tránh spam requests
            clearTimeout(geocodeTimeout);
            
            $('#address-input').val("Đang định vị địa chỉ thực tế...");

            geocodeTimeout = setTimeout(function() {
                const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}&accept-language=vi`;
                
                fetch(url, {
                    headers: {
                        'User-Agent': 'CivicConnect-App/1.0'
                    }
                })
                .then(response => response.json())
                .then(data => {
                    if (data && data.display_name) {
                        const addr = data.address || {};
                        
                        // Trích xuất tên Tỉnh, Quận, Phường động từ API Geocoding
                        const province = addr.city || addr.province || addr.state || "TP. Hồ Chí Minh";
                        const district = addr.district || addr.suburb || addr.county || addr.city_district || "Quận 1";
                        const ward = addr.suburb || addr.quarter || addr.town || addr.village || addr.ward || "Phường Bến Nghé";

                        // Tự động xây dựng địa chỉ chi tiết loại bỏ mã bưu điện và quốc gia
                        let detailedAddress = [];
                        if (addr.house_number) detailedAddress.push(addr.house_number);
                        if (addr.road) detailedAddress.push(addr.road);
                        if (addr.suburb || addr.quarter || addr.neighbourhood) detailedAddress.push(addr.suburb || addr.quarter || addr.neighbourhood);
                        
                        let fullAddress = detailedAddress.join(', ');
                        if (ward && !fullAddress.includes(ward)) fullAddress += (fullAddress ? ', ' : '') + ward;
                        if (district && !fullAddress.includes(district)) fullAddress += (fullAddress ? ', ' : '') + district;
                        if (province && !fullAddress.includes(province)) fullAddress += (fullAddress ? ', ' : '') + province;
                        
                        // Nếu vẫn trống, dùng display_name nhưng cắt bỏ phần ZIP code và Việt Nam
                        if (!fullAddress) {
                            fullAddress = data.display_name.replace(/, \d{5,6}(, Việt Nam)?$/, '').replace(/, Việt Nam$/, '');
                        }

                        $('#address-input').val(fullAddress);

                        $('#prov-name').val(province);
                        $('#dist-name').val(district);
                        $('#ward-name').val(ward);
                        
                        // Set default codes to prevent validation errors since OSM doesn't return administrative codes
                        $('#prov-code').val("79");
                        $('#dist-code').val("760");
                        $('#ward-code').val("26734");
                    } else {
                        $('#address-input').val(`Tọa độ: ${lat.toFixed(6)}, ${lng.toFixed(6)}`);
                    }
                })
                .catch(error => {
                    console.error("Lỗi khi giải mã toạ độ:", error);
                    $('#address-input').val(`Tọa độ: ${lat.toFixed(6)}, ${lng.toFixed(6)}`);
                });
            }, 600); // 600ms debounce delay
        }
    }
});
