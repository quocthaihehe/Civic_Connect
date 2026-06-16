$(document).ready(function () {
    const badge = $('#notif-counter');
    const container = $('#notif-container');

    // Chỉ kết nối nếu có phần tử chuông thông báo (đã đăng nhập)
    if ($('#notif-dropdown-trigger').length > 0) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notification")
            .withAutomaticReconnect()
            .build();

        connection.on("IssueStatusChanged", function (n) {
            addNewNotification(n);
        });

        connection.on("IssueAssigned", function (n) {
            addNewNotification(n);
        });

        connection.on("UpdateStats", function (totalUsers, totalIssues, resolvedIssues, satisfactionRate) {
            animateValue("kpi-total-users", totalUsers);
            animateValue("kpi-total-issues", totalIssues);
            animateValue("kpi-resolved-issues", resolvedIssues);
            const satEl = document.getElementById("kpi-satisfaction-rate");
            if (satEl) satEl.innerText = satisfactionRate + "%";
        });

        connection.start().then(function() {
            // Load existing unread notifications from API
            $.get('/api/notifications/unread', function(data) {
                renderNotifications(data);
            });
        }).catch(err => console.error(err.toString()));

        // Sự kiện click Đọc tất cả thông báo
        $('#mark-all-read').on('click', function(e) {
            e.stopPropagation();
            $.post('/api/notifications/mark-all-read', function() {
                badge.addClass('d-none').text('0');
                container.html(`
                    <div class="text-center text-muted py-4" id="notif-empty-state">
                        <i class="bi bi-bell-slash fs-3 d-block mb-1"></i>
                        <span style="font-size: 0.85rem;">Không có thông báo mới</span>
                    </div>
                `);
            });
        });
    }

    function renderNotifications(notifications) {
        if (!notifications || notifications.length === 0) {
            badge.addClass('d-none');
            return;
        }

        badge.removeClass('d-none').text(notifications.length);
        container.empty();

        notifications.forEach(n => {
            container.append(createNotifHtml(n));
        });
    }

    function addNewNotification(n) {
        let count = parseInt(badge.text()) || 0;
        count++;
        badge.removeClass('d-none').text(count);

        // Animation rung lắc nhẹ chuông thông báo
        const bell = $('#notif-dropdown-trigger');
        bell.addClass('animate-shake');
        setTimeout(() => bell.removeClass('animate-shake'), 1000);

        const notifData = {
            id: n.id || 0,
            title: n.title,
            message: n.message,
            relatedIssueId: n.issueId || n.relatedIssueId
        };
        
        $('#notif-empty-state').remove();
        container.prepend(createNotifHtml(notifData));
    }

    function createNotifHtml(n) {
        const link = n.relatedIssueId ? `/Issues/${n.relatedIssueId}` : '#';
        return `
            <a href="${link}" class="dropdown-item p-3 border-bottom d-flex align-items-start gap-2 notif-item" data-id="${n.id}" style="white-space: normal; transition: background 0.2s;">
                <div class="bg-primary-light text-primary rounded-circle p-2 d-flex align-items-center justify-content-center" style="width: 32px; height: 32px; flex-shrink: 0; background: rgba(37,99,235,0.08);">
                    <i class="bi bi-chat-left-text-fill"></i>
                </div>
                <div class="w-100">
                    <div class="fw-semibold text-dark" style="font-size: 0.85rem; line-height: 1.2;">${n.title}</div>
                    <div class="text-secondary mt-1" style="font-size: 0.75rem; line-height: 1.3;">${n.message}</div>
                </div>
            </a>
        `;
    }

    // Đánh dấu đã đọc khi click vào thông báo
    container.on('click', '.notif-item', function(e) {
        e.preventDefault();
        const id = $(this).data('id');
        const href = $(this).attr('href');
        
        if (id && id > 0) {
            $.post(`/api/notifications/${id}/mark-read`).always(function() {
                if (href && href !== '#') {
                    window.location.href = href;
                }
            });
        } else {
            if (href && href !== '#') {
                window.location.href = href;
            }
        }
    });
});

function animateValue(id, value) {
    const el = document.getElementById(id);
    if (el) {
        // Chỉ đơn giản là cập nhật số, có thể thêm hiệu ứng countup nếu thích
        el.innerText = value.toLocaleString('vi-VN');
        el.classList.add('animate-shake');
        setTimeout(() => el.classList.remove('animate-shake'), 1000);
    }
}
