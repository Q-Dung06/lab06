LAB 6 - DATAGRIDVIEW, LISTVIEW, TREEVIEW VÀ MÔ HÌNH 3 LỚP
Học phần: Lập trình .NET (IT3212)

MỞ VÀ CHẠY
- Mở Lab06_ThreeLayerMemory.sln bằng Visual Studio 2022 và nhấn F5.
- Hoặc chạy: dotnet run --project Lab06.GUI\Lab06.GUI.csproj
- Tệp CSV sinh viên được tạo và lưu tại Lab06.GUI\bin\Debug\net10.0-windows\Data\SinhVien.csv.

CHỨC NĂNG
1. BindingList<SinhVien> liên kết DataGridView, có tiêu đề cột và chọn dòng.
2. Thêm, sửa, xóa sinh viên; kiểm tra mã trùng, thông tin rỗng và điểm 0-10.
3. ListView Details hiển thị mã, họ tên, khoa, điểm, xếp loại; ImageList phân biệt học lực.
4. TreeView nhóm sinh viên theo khoa; chọn sinh viên để xem thông tin.
5. Tách ba project Lab06.GUI -> Lab06.BUS -> Lab06.DAL; DAL đọc/ghi CSV UTF-8.
6. Quản lý sản phẩm: thêm, sửa, xóa, tìm kiếm, sắp xếp và tính tổng giá trị tồn kho.
7. TreeView danh mục - sản phẩm; menu chuột phải thêm, đổi tên, xóa danh mục.
8. Có 9 sinh viên mẫu thuộc 3 khoa, mỗi khoa 3 sinh viên; 6 sản phẩm mẫu thuộc 3 danh mục.
9. DAL tự đọc CSV khi khởi động và ghi lại sau khi thêm/sửa/xóa sinh viên.

BUILD VÀ ẢNH MINH CHỨNG
- Chạy dotnet build Lab06_ThreeLayerMemory.sln.
- Ảnh giao diện nằm trong Lab06.GUI\Screenshots.
