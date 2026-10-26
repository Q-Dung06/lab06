using System.ComponentModel;
using System.Drawing;
using Lab06.BUS;
using Lab06.DAL.Models;

namespace Lab06.GUI;

public sealed class frmMain : Form
{
    private readonly SinhVienBUS _svBus = new();
    private readonly SanPhamBUS _spBus = new();
    private readonly BindingSource _svSource = new();
    private readonly BindingSource _spSource = new();
    private readonly List<string> _khoas = new() { "Công nghệ thông tin", "Kinh tế", "Điện - Điện tử" };
    private readonly DataGridView _dgvSv = Grid(), _dgvSp = Grid();
    private readonly ListView _lv = new();
    private readonly TreeView _tvSv = new(), _tvSp = new();
    private readonly Label _svDetail = new(), _total = new();
    private readonly TextBox _maSv = new(), _tenSv = new(), _diemSv = new(), _searchSv = new();
    private readonly DateTimePicker _birth = new();
    private readonly ComboBox _khoa = new();
    private readonly ComboBox _gender = new();
    private readonly TextBox _maSp = new(), _tenSp = new(), _slSp = new(), _giaSp = new(), _searchSp = new();
    private readonly ComboBox _catSp = new();
    private readonly List<string> _cats = new() { "Điện tử", "Nội thất", "Văn phòng phẩm" };
    private bool _sortAsc = true;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    public frmMain()
    {
        Text = "Lab 6 – Quản lý dữ liệu và mô hình 3 lớp"; Width = 1250; Height = 790; MinimumSize = new Size(1050,650); StartPosition=FormStartPosition.CenterScreen;
        _tabs.TabPages.Add(BuildStudents()); _tabs.TabPages.Add(BuildStudentViews()); _tabs.TabPages.Add(BuildProducts());
        Controls.Add(_tabs);
        _svSource.DataSource=_svBus.DanhSach; _dgvSv.DataSource=_svSource; _lv.SmallImageList=MakeImages(); _lv.LargeImageList=_lv.SmallImageList;
        _spSource.DataSource=_spBus.DanhSach; _dgvSp.DataSource=_spSource;
        _khoa.DataSource=_khoas.ToList(); _gender.Items.AddRange(new object[]{"Nam","Nữ"});_gender.SelectedIndex=0;_catSp.DataSource=_cats.ToList();
        _dgvSv.SelectionChanged+=(_,_)=>LoadSvSelection(); _dgvSp.SelectionChanged+=(_,_)=>LoadSpSelection();
        _searchSv.TextChanged+=(_,_)=>FilterStudents(); _searchSp.TextChanged+=(_,_)=>FilterProducts();
        LoadStudentViews(); LoadProductTree(); UpdateTotal();
    }

    internal void CaptureTabs()
    {
        string dir=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","Screenshots"));Directory.CreateDirectory(dir);
        string[] names={"01_SinhVien.png","02_ListView_TreeView.png","03_SanPham_DanhMuc.png"};
        for(int i=0;i<_tabs.TabPages.Count;i++){_tabs.SelectedIndex=i;Application.DoEvents();System.Threading.Thread.Sleep(250);using var bmp=new Bitmap(Width,Height);DrawToBitmap(bmp,new Rectangle(0,0,Width,Height));bmp.Save(Path.Combine(dir,names[i]),System.Drawing.Imaging.ImageFormat.Png);}
    }

    private TabPage BuildStudents()
    {
        var page=new TabPage("Sinh viên · CRUD"); var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(10)}; root.RowStyles.Add(new RowStyle(SizeType.Absolute,170));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,75));page.Controls.Add(root);
        var form=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(5),AutoScroll=true};
        AddField(form,"Mã SV",_maSv,170);AddField(form,"Họ tên",_tenSv,250);AddField(form,"Ngày sinh",_birth,160);_birth.Format=DateTimePickerFormat.Short;AddField(form,"Khoa",_khoa,240);AddField(form,"Điểm TB",_diemSv,110);AddField(form,"Giới tính",_gender,100);
        form.Controls.Add(Button("Thêm",AddStudent));form.Controls.Add(Button("Cập nhật",EditStudent));form.Controls.Add(Button("Xóa",DeleteStudent));form.Controls.Add(Button("Làm mới",(_,_)=>ClearStudent()));form.Controls.Add(Button("Sắp xếp điểm",(_,_)=>SortStudents()));
        root.Controls.Add(form,0,0); ConfigureGrid(_dgvSv);root.Controls.Add(_dgvSv,0,1);
        var bottom=new FlowLayoutPanel{Dock=DockStyle.Fill};AddField(bottom,"Tìm mã/tên",_searchSv,260);bottom.Controls.Add(new Label{Text="Dữ liệu tự lưu CSV khi thêm/sửa/xóa.",AutoSize=true,Padding=new Padding(10)});root.Controls.Add(bottom,0,2);return page;
    }

    private TabPage BuildStudentViews()
    {
        var page=new TabPage("ListView · TreeView");var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=600};page.Controls.Add(split);
        _lv.Dock=DockStyle.Fill;_lv.View=View.Details;_lv.FullRowSelect=true;_lv.Columns.Add("Mã SV",90);_lv.Columns.Add("Họ tên",210);_lv.Columns.Add("Khoa",190);_lv.Columns.Add("Điểm",80);_lv.Columns.Add("Xếp loại",100);_lv.SelectedIndexChanged+=(_,_)=>{if(_lv.SelectedItems.Count>0&&_lv.SelectedItems[0].Tag is SinhVien sv)ShowStudent(sv);};
        _tvSv.Dock=DockStyle.Fill;_tvSv.AfterSelect+=(_,e)=>{if(e.Node?.Tag is SinhVien sv)ShowStudent(sv);};
        split.Panel1.Controls.Add(_lv);var right=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2};right.RowStyles.Add(new RowStyle(SizeType.Percent,100));right.RowStyles.Add(new RowStyle(SizeType.Absolute,100));right.Controls.Add(_tvSv,0,0);_svDetail.Dock=DockStyle.Fill;_svDetail.Padding=new Padding(10);right.Controls.Add(_svDetail,0,1);split.Panel2.Controls.Add(right);return page;
    }

    private TabPage BuildProducts()
    {
        var page=new TabPage("Sản phẩm · danh mục");var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=820};page.Controls.Add(split);
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,Padding=new Padding(8)};layout.RowStyles.Add(new RowStyle(SizeType.Absolute,170));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,75));split.Panel1.Controls.Add(layout);
        var form=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true};AddField(form,"Mã SP",_maSp,130);AddField(form,"Tên sản phẩm",_tenSp,220);AddField(form,"Danh mục",_catSp,180);AddField(form,"Số lượng",_slSp,90);AddField(form,"Đơn giá (VNĐ)",_giaSp,140);
        form.Controls.Add(Button("Thêm",AddProduct));form.Controls.Add(Button("Cập nhật",EditProduct));form.Controls.Add(Button("Xóa",DeleteProduct));form.Controls.Add(Button("Làm mới",(_,_)=>ClearProduct()));layout.Controls.Add(form,0,0);ConfigureGrid(_dgvSp);layout.Controls.Add(_dgvSp,0,1);
        var bottom=new FlowLayoutPanel{Dock=DockStyle.Fill};AddField(bottom,"Tìm mã/tên",_searchSp,230);bottom.Controls.Add(Button("Sắp xếp",(_,_)=>SortProducts()));_total.AutoSize=true;_total.Padding=new Padding(8);bottom.Controls.Add(_total);layout.Controls.Add(bottom,0,2);
        _tvSp.Dock=DockStyle.Fill;_tvSp.AfterSelect+=(_,e)=>{if(e.Node?.Tag is SanPham p){_spSource.Filter=null;_dgvSp.ClearSelection();foreach(DataGridViewRow row in _dgvSp.Rows)if(row.DataBoundItem==p){row.Selected=true;_dgvSp.FirstDisplayedScrollingRowIndex=Math.Max(0,row.Index);break;}}};split.Panel2.Controls.Add(_tvSp);BuildCategoryMenu();_tvSp.NodeMouseClick+=(_,e)=>{if(e.Button==MouseButtons.Right)_tvSp.SelectedNode=e.Node;};return page;
    }

    private void AddStudent(object? s,EventArgs e){try{_svBus.Them(ReadStudent());RefreshStudents();ClearStudent();}catch(Exception ex){Error(ex);}}
    private void EditStudent(object? s,EventArgs e){if(SelectedStudent() is not{} old)return;try{var x=ReadStudent();_svBus.CapNhat(x,old.MaSV);_svBus.DanhSach.ResetItem(_svBus.DanhSach.IndexOf(old));RefreshStudents();}catch(Exception ex){Error(ex);}}
    private void DeleteStudent(object? s,EventArgs e){if(SelectedStudent() is{} sv&&MessageBox.Show($"Xóa {sv.MaSV} - {sv.HoTen}?","Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes){_svBus.Xoa(sv);RefreshStudents();ClearStudent();}}
    private SinhVien ReadStudent()=>new(){MaSV=_maSv.Text,HoTen=_tenSv.Text,NgaySinh=_birth.Value,Khoa=_khoa.Text,DiemTB=double.TryParse(_diemSv.Text.Replace(',','.'),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out var d)?d:-1,GioiTinh=_gender.Text};
    private void LoadSvSelection(){if(SelectedStudent() is not{} x)return;_maSv.Text=x.MaSV;_tenSv.Text=x.HoTen;_birth.Value=x.NgaySinh;_khoa.SelectedItem=x.Khoa;_diemSv.Text=x.DiemTB.ToString("0.##");_gender.SelectedItem=x.GioiTinh;}
    private SinhVien? SelectedStudent()=>_dgvSv.CurrentRow?.DataBoundItem as SinhVien;
    private void ClearStudent(){_maSv.Clear();_tenSv.Clear();_diemSv.Clear();if(_khoa.Items.Count>0)_khoa.SelectedIndex=0;if(_gender.Items.Count>0)_gender.SelectedIndex=0;_dgvSv.ClearSelection();}
    private void FilterStudents(){string q=_searchSv.Text.Trim();_dgvSv.DataSource=null;_dgvSv.DataSource=string.IsNullOrEmpty(q)?_svBus.DanhSach:new BindingList<SinhVien>(_svBus.DanhSach.Where(x=>x.MaSV.Contains(q,StringComparison.CurrentCultureIgnoreCase)||x.HoTen.Contains(q,StringComparison.CurrentCultureIgnoreCase)).ToList());}
    private void SortStudents(){_sortAsc=!_sortAsc;var x=_sortAsc?_svBus.DanhSach.OrderBy(s=>s.DiemTB).ToList():_svBus.DanhSach.OrderByDescending(s=>s.DiemTB).ToList();_dgvSv.DataSource=new BindingList<SinhVien>(x);}
    private void RefreshStudents(){_dgvSv.DataSource=_svBus.DanhSach;LoadStudentViews();}
    private void LoadStudentViews(){_lv.BeginUpdate();_lv.Items.Clear();foreach(var sv in _svBus.DanhSach){int icon=sv.DiemTB>=8?0:sv.DiemTB>=5?1:2;var item=new ListViewItem(sv.MaSV,icon){Tag=sv,ToolTipText=sv.XepLoai};item.SubItems.Add(sv.HoTen);item.SubItems.Add(sv.Khoa);item.SubItems.Add(sv.DiemTB.ToString("N2"));item.SubItems.Add(sv.XepLoai);_lv.Items.Add(item);}_lv.EndUpdate();_tvSv.Nodes.Clear();foreach(var g in _svBus.DanhSach.GroupBy(x=>x.Khoa).OrderBy(x=>x.Key)){var n=_tvSv.Nodes.Add(g.Key);foreach(var sv in g)n.Nodes.Add(new TreeNode($"{sv.MaSV} - {sv.HoTen}"){Tag=sv});}_tvSv.ExpandAll();}
    private void ShowStudent(SinhVien s)=>_svDetail.Text=$"{s.MaSV}  |  {s.HoTen}\nKhoa: {s.Khoa}    Điểm: {s.DiemTB:N2} ({s.XepLoai})";
    private void SortProducts(){_sortAsc=!_sortAsc;_dgvSp.DataSource=new BindingList<SanPham>(_sortAsc?_spBus.DanhSach.OrderBy(x=>x.TenSP).ToList():_spBus.DanhSach.OrderByDescending(x=>x.TenSP).ToList());}
    private void FilterProducts(){string q=_searchSp.Text.Trim();_dgvSp.DataSource=string.IsNullOrEmpty(q)?_spBus.DanhSach:new BindingList<SanPham>(_spBus.DanhSach.Where(x=>x.MaSP.Contains(q,StringComparison.CurrentCultureIgnoreCase)||x.TenSP.Contains(q,StringComparison.CurrentCultureIgnoreCase)).ToList());}
    private void AddProduct(object? s,EventArgs e){try{_spBus.LuuMoi(ReadProduct());RefreshProducts();ClearProduct();}catch(Exception ex){Error(ex);}}
    private void EditProduct(object? s,EventArgs e){if(SelectedProduct() is not{} old)return;try{var x=ReadProduct();_spBus.CapNhat(x,old.MaSP);old.MaSP=x.MaSP;old.TenSP=x.TenSP;old.DanhMuc=x.DanhMuc;old.SoLuong=x.SoLuong;old.DonGia=x.DonGia;_spBus.DanhSach.ResetBindings();RefreshProducts();}catch(Exception ex){Error(ex);}}
    private void DeleteProduct(object? s,EventArgs e){if(SelectedProduct() is{} p&&MessageBox.Show($"Xóa {p.MaSP} - {p.TenSP}?","Xác nhận",MessageBoxButtons.YesNo)==DialogResult.Yes){_spBus.DanhSach.Remove(p);RefreshProducts();ClearProduct();}}
    private SanPham ReadProduct()=>new(){MaSP=_maSp.Text.Trim(),TenSP=_tenSp.Text.Trim(),DanhMuc=_catSp.Text,SoLuong=int.TryParse(_slSp.Text,out var n)?n:-1,DonGia=decimal.TryParse(_giaSp.Text,out var d)?d:-1};
    private SanPham? SelectedProduct()=>_dgvSp.CurrentRow?.DataBoundItem as SanPham;
    private void LoadSpSelection(){if(SelectedProduct() is not{} x)return;_maSp.Text=x.MaSP;_tenSp.Text=x.TenSP;_catSp.SelectedItem=x.DanhMuc;_slSp.Text=x.SoLuong.ToString();_giaSp.Text=x.DonGia.ToString("0.##");}
    private void ClearProduct(){_maSp.Clear();_tenSp.Clear();_slSp.Clear();_giaSp.Clear();_dgvSp.ClearSelection();}
    private void RefreshProducts(){_dgvSp.DataSource=_spBus.DanhSach;UpdateTotal();LoadProductTree();}
    private void UpdateTotal()=>_total.Text=$"Tổng giá trị tồn kho: {_spBus.TongTonKho:N0} VNĐ";
    private void LoadProductTree(){_tvSp.Nodes.Clear();foreach(string c in _cats){var n=_tvSp.Nodes.Add(c);foreach(var p in _spBus.DanhSach.Where(x=>x.DanhMuc==c))n.Nodes.Add(new TreeNode($"{p.MaSP} - {p.TenSP}"){Tag=p});}_tvSp.ExpandAll();}
    private void BuildCategoryMenu(){var menu=new ContextMenuStrip();menu.Items.Add("Thêm danh mục",null,(_,_)=>{string? name=Prompt("Tên danh mục mới:","Thêm danh mục");if(string.IsNullOrWhiteSpace(name))return;if(_cats.Contains(name,StringComparer.OrdinalIgnoreCase)){MessageBox.Show("Danh mục đã tồn tại.");return;}_cats.Add(name.Trim());_catSp.DataSource=null;_catSp.DataSource=_cats.ToList();LoadProductTree();});menu.Items.Add("Đổi tên danh mục",null,(_,_)=>{if(_tvSp.SelectedNode?.Parent!=null)return;string? old=_tvSp.SelectedNode?.Text;if(old==null)return;string? name=Prompt("Tên danh mục mới:","Đổi tên",old);if(string.IsNullOrWhiteSpace(name))return;foreach(var p in _spBus.DanhSach.Where(x=>x.DanhMuc==old))p.DanhMuc=name.Trim();_cats[_cats.IndexOf(old)]=name.Trim();_catSp.DataSource=null;_catSp.DataSource=_cats.ToList();RefreshProducts();});menu.Items.Add("Xóa danh mục",null,(_,_)=>{string? old=_tvSp.SelectedNode?.Text;if(old==null||_tvSp.SelectedNode?.Parent!=null)return;if(_spBus.DanhSach.Any(x=>x.DanhMuc==old)){MessageBox.Show("Hãy chuyển hoặc xóa sản phẩm trong danh mục trước.","Không thể xóa");return;}_cats.Remove(old);_catSp.DataSource=null;_catSp.DataSource=_cats.ToList();LoadProductTree();});_tvSp.ContextMenuStrip=menu;}
    private static string? Prompt(string label,string title,string value=""){using var f=new Form{Text=title,Width=380,Height=150,StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog};var l=new Label{Text=label,Left=12,Top=12,AutoSize=true};var t=new TextBox{Text=value,Left=12,Top=38,Width=335};var b=new Button{Text="OK",Left=260,Top=72,DialogResult=DialogResult.OK};f.Controls.AddRange(new Control[]{l,t,b});f.AcceptButton=b;return f.ShowDialog()==DialogResult.OK?t.Text:null;}
    private static ImageList MakeImages(){var i=new ImageList{ImageSize=new Size(16,16),ColorDepth=ColorDepth.Depth32Bit};i.Images.Add(SystemIcons.Information);i.Images.Add(SystemIcons.Warning);i.Images.Add(SystemIcons.Error);return i;}
    private static DataGridView Grid()=>new(){Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,RowHeadersVisible=false,AlternatingRowsDefaultCellStyle=new(){BackColor=Color.AliceBlue}};
    private static void ConfigureGrid(DataGridView g){g.DataError+=(_,e)=>e.ThrowException=false;g.DataBindingComplete+=(_,_)=>{foreach(DataGridViewColumn c in g.Columns){c.HeaderText=c.DataPropertyName switch{"MaSV"=>"Mã SV","HoTen"=>"Họ tên","NgaySinh"=>"Ngày sinh","Khoa"=>"Khoa","DiemTB"=>"Điểm TB","GioiTinh"=>"Giới tính","XepLoai"=>"Xếp loại","MaSP"=>"Mã SP","TenSP"=>"Tên sản phẩm","DanhMuc"=>"Danh mục","SoLuong"=>"Số lượng","DonGia"=>"Đơn giá (VNĐ)","ThanhTien"=>"Thành tiền (VNĐ)","TrangThai"=>"Tình trạng",_=>c.HeaderText};if(c.DataPropertyName is "DonGia" or "ThanhTien")c.DefaultCellStyle.Format="N0";}};}
    private static void AddField(FlowLayoutPanel p,string title,Control input,int width){var panel=new Panel{Width=width,Height=58,Margin=new Padding(5)};panel.Controls.Add(new Label{Text=title,Left=0,Top=0,Width=width-5,Height=20});input.Left=0;input.Top=23;input.Width=width-5;panel.Controls.Add(input);p.Controls.Add(panel);}
    private static Button Button(string text,EventHandler action){var b=new Button{Text=text,AutoSize=true,Height=32,Margin=new Padding(5,20,5,0)};b.Click+=action;return b;}
    private static void Error(Exception ex)=>MessageBox.Show(ex.Message,"Kiểm tra dữ liệu",MessageBoxButtons.OK,MessageBoxIcon.Warning);
}
