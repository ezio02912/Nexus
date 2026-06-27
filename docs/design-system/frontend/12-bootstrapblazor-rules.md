# BootstrapBlazor Rules
Không tự viết component nếu BootstrapBlazor có sẵn.
Ưu tiên dùng component từ `external/bootstrap-blazor` trước mọi raw HTML/native input trong app Blazor.

Áp dụng mặc định cho: `Table`, `Modal`, `Button`, `Select`, `DateTimePicker`, `BootstrapInput`, `BootstrapInputNumber`, `Checkbox`, `InputFile`, `Card`, `Toast`, `Swal`.

Chỉ dùng raw HTML cho layout semantic hoặc khi BootstrapBlazor không có component phù hợp.
Không inline style.
