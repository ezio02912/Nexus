# Web Tenant

Blazor tenant portal cho người dùng thuộc từng công ty thuê hệ thống. Tenant portal luôn đi qua API Gateway/BFF.

## CRM / Sales UX Phase

Các cập nhật đang ưu tiên ổn định luồng nhập liệu cho CRM, mua hàng và bán hàng:

- Reset form sau khi tạo/cập nhật thành công để input, select, ngày và file đính kèm không giữ dữ liệu cũ.
- Đồng bộ nút upload giữa file đã lưu và file chờ lưu bằng cùng lớp `file-attachment-upload`.
- Bổ sung bộ lọc nhanh cho `/sales/orders`: tìm kiếm, trạng thái đơn, giữ hàng và giao hàng.
- Chuẩn hóa badge trạng thái bán hàng theo helper `SalesLabels` để tránh pill quá lớn, xuống dòng xấu.
- Chuẩn hóa DateTimePicker sang `dd/MM/yyyy`, thêm locale tiếng Việt cho placeholder, tháng, ngày, Clear/Now/Ok.
- CRM Activities chuyển sang calendar, danh mục kho có cấu hình tồn âm theo kho; Invoice/Accounting để các phase cuối sau khi vận hành CRM/Sales/Inventory/Purchase ổn định.

## Roadmap Tiếp Theo

- Phase 1: Hoàn tất audit reset form trên các màn còn lại của CRM, Sales, Purchase và Inventory.
- Phase 2: Chuẩn hóa bộ lọc table dùng chung cho các page nghiệp vụ để không mỗi page một kiểu.
- Phase 3: Tách component trạng thái bán hàng/mua hàng dùng chung, có màu nhất quán theo semantic status.
- Phase 4: Kiểm tra responsive modal, table action và upload preview trên mobile/tablet.
- Phase 5: Bổ sung test UI cho các luồng tạo mới, cập nhật, upload file và lọc danh sách.
