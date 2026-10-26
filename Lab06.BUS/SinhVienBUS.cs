using System.ComponentModel;
using Lab06.DAL;
using Lab06.DAL.Models;

namespace Lab06.BUS;

public sealed class SinhVienBUS
{
    private readonly SinhVienDAL _dal;
    public BindingList<SinhVien> DanhSach { get; }
    public SinhVienBUS(SinhVienDAL? dal = null) { _dal=dal??new SinhVienDAL(); DanhSach=new BindingList<SinhVien>(_dal.GetAll()); }
    public void Them(SinhVien sv)
    {
        sv.MaSV=sv.MaSV.Trim(); sv.HoTen=sv.HoTen.Trim(); sv.Khoa=sv.Khoa.Trim();
        if(string.IsNullOrWhiteSpace(sv.MaSV)||string.IsNullOrWhiteSpace(sv.HoTen)||string.IsNullOrWhiteSpace(sv.Khoa)) throw new ArgumentException("Mã, họ tên và khoa không được để trống.");
        if(sv.DiemTB<0||sv.DiemTB>10) throw new ArgumentException("Điểm phải từ 0 đến 10.");
        if(DanhSach.Any(x=>x.MaSV.Equals(sv.MaSV,StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Mã sinh viên đã tồn tại.");
        DanhSach.Add(sv); _dal.Save();
    }
    public void CapNhat(SinhVien sv, string maCu)
    {
        sv.MaSV=sv.MaSV.Trim();sv.HoTen=sv.HoTen.Trim();sv.Khoa=sv.Khoa.Trim();
        if(string.IsNullOrWhiteSpace(sv.MaSV)||string.IsNullOrWhiteSpace(sv.HoTen)||string.IsNullOrWhiteSpace(sv.Khoa)||sv.DiemTB<0||sv.DiemTB>10) throw new ArgumentException("Vui lòng nhập đủ dữ liệu hợp lệ; điểm từ 0 đến 10.");
        if(!sv.MaSV.Equals(maCu,StringComparison.OrdinalIgnoreCase)&&DanhSach.Any(x=>x.MaSV.Equals(sv.MaSV,StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException("Mã sinh viên đã tồn tại.");
        var current = DanhSach.FirstOrDefault(x => x.MaSV.Equals(maCu, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Không tìm thấy sinh viên cần cập nhật.");
        current.MaSV=sv.MaSV; current.HoTen=sv.HoTen; current.NgaySinh=sv.NgaySinh;
        current.Khoa=sv.Khoa; current.DiemTB=sv.DiemTB; current.GioiTinh=sv.GioiTinh;
        _dal.Save();
    }
    public void Xoa(SinhVien sv){DanhSach.Remove(sv);_dal.Save();}
}

public sealed class SanPhamBUS
{
    public BindingList<SanPham> DanhSach { get; } = new(new SanPhamDAL().GetAll());
    public decimal TongTonKho => DanhSach.Sum(x=>x.ThanhTien);
    public void LuuMoi(SanPham p)
    {
        if(string.IsNullOrWhiteSpace(p.MaSP)||string.IsNullOrWhiteSpace(p.TenSP)||string.IsNullOrWhiteSpace(p.DanhMuc))throw new ArgumentException("Nhập đầy đủ mã, tên và danh mục sản phẩm.");
        if(p.SoLuong<0||p.DonGia<0)throw new ArgumentException("Số lượng và đơn giá không được âm.");
        if(DanhSach.Any(x=>x.MaSP.Equals(p.MaSP,StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException("Mã sản phẩm đã tồn tại.");
        DanhSach.Add(p);
    }
    public void CapNhat(SanPham p,string maCu)
    {
        if(string.IsNullOrWhiteSpace(p.MaSP)||string.IsNullOrWhiteSpace(p.TenSP)||string.IsNullOrWhiteSpace(p.DanhMuc)||p.SoLuong<0||p.DonGia<0)throw new ArgumentException("Dữ liệu sản phẩm chưa hợp lệ.");
        if(!p.MaSP.Equals(maCu,StringComparison.OrdinalIgnoreCase)&&DanhSach.Any(x=>x.MaSP.Equals(p.MaSP,StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException("Mã sản phẩm đã tồn tại.");
    }
}
