namespace Lab06.DAL.Models;

public sealed class SinhVien
{
    public string MaSV { get; set; } = "";
    public string HoTen { get; set; } = "";
    public DateTime NgaySinh { get; set; } = DateTime.Today;
    public string Khoa { get; set; } = "";
    public double DiemTB { get; set; }
    public string GioiTinh { get; set; } = "";
    public string XepLoai => DiemTB >= 8 ? "Giỏi" : DiemTB >= 6.5 ? "Khá" : DiemTB >= 5 ? "Trung bình" : "Yếu";
}

public sealed class SanPham
{
    public string MaSP { get; set; } = "";
    public string TenSP { get; set; } = "";
    public string DanhMuc { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
    public string TrangThai => SoLuong == 0 ? "Hết hàng" : SoLuong < 5 ? "Sắp hết" : "Còn hàng";
}

public sealed class SinhVienDAL
{
    private readonly string _csvPath;
    private readonly List<SinhVien> _data;
    private static readonly string[] KhoaMau = { "Công nghệ thông tin", "Kinh tế", "Điện - Điện tử" };

    public SinhVienDAL(string? csvPath = null)
    {
        _csvPath = csvPath ?? Path.Combine(AppContext.BaseDirectory, "Data", "SinhVien.csv");
        _data = File.Exists(_csvPath) ? ReadCsv() : CreateSamples();
        if (!File.Exists(_csvPath)) Save();
    }

    public List<SinhVien> GetAll() => _data;
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_csvPath)!);
        using var writer = new StreamWriter(_csvPath, false, new System.Text.UTF8Encoding(true));
        writer.WriteLine("MaSV,HoTen,NgaySinh,Khoa,DiemTB,GioiTinh");
        foreach (var sv in _data)
            writer.WriteLine(string.Join(',', new[] { sv.MaSV, sv.HoTen, sv.NgaySinh.ToString("yyyy-MM-dd"), sv.Khoa, sv.DiemTB.ToString(System.Globalization.CultureInfo.InvariantCulture), sv.GioiTinh }.Select(Escape)));
    }

    private List<SinhVien> ReadCsv()
    {
        var rows = File.ReadAllLines(_csvPath).Skip(1).Where(x => !string.IsNullOrWhiteSpace(x)).Select(ParseCsv).ToList();
        return rows.Where(x => x.Length >= 6).Select(x => new SinhVien { MaSV=x[0], HoTen=x[1], NgaySinh=DateTime.Parse(x[2]), Khoa=x[3], DiemTB=double.Parse(x[4], System.Globalization.CultureInfo.InvariantCulture), GioiTinh=x[5] }).ToList();
    }
    private static string Escape(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
    private static string[] ParseCsv(string line)
    {
        var fields = new List<string>(); var field = new System.Text.StringBuilder(); bool quoted = false;
        for (int i=0;i<line.Length;i++) { char c=line[i]; if(c=='\"' && quoted && i+1<line.Length && line[i+1]=='\"'){field.Append('"');i++;} else if(c=='\"') quoted=!quoted; else if(c==',' && !quoted){fields.Add(field.ToString());field.Clear();} else field.Append(c); }
        fields.Add(field.ToString()); return fields.ToArray();
    }
    private static List<SinhVien> CreateSamples()
    {
        string[] names = { "Nguyễn Quang An", "Trần Thị Bình", "Lê Minh Cường", "Phạm Thu Hà", "Hoàng Gia Huy", "Vũ Ngọc Lan", "Đỗ Đức Minh", "Bùi Thanh Nga", "Đặng Quốc Phong" };
        double[] scores = { 8.4, 7.2, 9.1, 6.8, 5.5, 8.9, 7.8, 9.4, 6.2 };
        return Enumerable.Range(0,9).Select(i => new SinhVien { MaSV=$"SV{i+1:000}", HoTen=names[i], NgaySinh=new DateTime(2003+i%3,1+(i%12),5+i%20), Khoa=KhoaMau[i/3], DiemTB=scores[i], GioiTinh=i%2==0?"Nam":"Nữ" }).ToList();
    }
}

public sealed class SanPhamDAL
{
    private readonly List<SanPham> _data = new()
    {
        new(){MaSP="SP001",TenSP="Laptop văn phòng",DanhMuc="Điện tử",SoLuong=8,DonGia=18500000},
        new(){MaSP="SP002",TenSP="Chuột không dây",DanhMuc="Điện tử",SoLuong=3,DonGia=350000},
        new(){MaSP="SP003",TenSP="Bàn học gỗ",DanhMuc="Nội thất",SoLuong=12,DonGia=1250000},
        new(){MaSP="SP004",TenSP="Ghế xoay",DanhMuc="Nội thất",SoLuong=0,DonGia=2100000},
        new(){MaSP="SP005",TenSP="Sổ tay A5",DanhMuc="Văn phòng phẩm",SoLuong=30,DonGia=45000},
        new(){MaSP="SP006",TenSP="Bút bi xanh",DanhMuc="Văn phòng phẩm",SoLuong=2,DonGia=8000}
    };
    public List<SanPham> GetAll() => _data;
}
