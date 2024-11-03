using BLL.DoAn;
using DAL.D.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BanHang
{
    public partial class Oder : Form
    {

        private List<MonAn> danhSachMonDaChon = new List<MonAn>();  // Danh sách món đã chọn
        private bool isLoading = true;
        private QLOderService qlOrderService;
        private int soBan;
        private List<SelectedSanPham> selectedSanPhams = new List<SelectedSanPham>();
        public Oder(int soBan, List<SanPham> sanPhams)
        {
            InitializeComponent();
            this.soBan = soBan;
            this.Text = "Thông tin Bàn " + soBan;
            qlOrderService = new QLOderService();

            // Tải dữ liệu từ bộ nhớ tạm khi mở lại form
            this.selectedSanPhams = TemporaryOrderStorage.LoadOrder(soBan);
            RefreshDataGridView2();
      
            dataGridView2.CellClick += dataGridView2_CellClick;

            dataGridView2.CellEndEdit += dataGridView2_CellEndEdit;

        }
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView2.Columns["SoLuong"].ReadOnly = false;
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView2.Columns["SoLuong"].Index)
            {
                // Lấy sản phẩm từ danh sách
                var sanPham = (SelectedSanPham)dataGridView2.Rows[e.RowIndex].DataBoundItem;
                int newQuantity;

                if (int.TryParse(dataGridView2.Rows[e.RowIndex].Cells["SoLuong"].Value.ToString(), out newQuantity) && newQuantity >= 0)
                {
                    // Cập nhật số lượng trong danh sách đã chọn
                    sanPham.SoLuong = newQuantity;

                    // Cập nhật số lượng trong dịch vụ
                    qlOrderService.UpdateSanPhamQuantity(sanPham.MaSanPham, newQuantity);
                }
                else
                {
                    MessageBox.Show("Số lượng không hợp lệ. Vui lòng nhập số lớn hơn hoặc bằng 0.");
                    // Khôi phục lại giá trị ban đầu
                    dataGridView2.Rows[e.RowIndex].Cells["SoLuong"].Value = sanPham.SoLuong;
                    return;
                }

                // Cập nhật lại tổng số lượng và tổng tiền sau khi sửa
                var (tongSoLuong, tongTien) = qlOrderService.TinhTong(selectedSanPhams);
                txtSoLuong.Text = $"Tổng Số Lượng: {tongSoLuong}";
                txtTongTien.Text = $"Tổng Tiền: {tongTien:C2}"; // Định dạng tiền tệ
            }
        }


        private void Oder_Load(object sender, EventArgs e)
        {
            lblSoBan.Text = "Bàn số: " + soBan;
            LoadComboBoxLoai();
            LoadOrderTamThoi(); // Tải danh sách sản phẩm đã chọn
            

        }

        public static class TemporaryOrderStorage
        {
            private static Dictionary<int, List<SelectedSanPham>> orderData = new Dictionary<int, List<SelectedSanPham>>();

            public static void SaveOrder(int tableNumber, List<SelectedSanPham> selectedItems)
            {
                orderData[tableNumber] = new List<SelectedSanPham>(selectedItems); // Lưu dữ liệu
            }

            public static List<SelectedSanPham> LoadOrder(int tableNumber)
            {
                return orderData.ContainsKey(tableNumber) ? new List<SelectedSanPham>(orderData[tableNumber]) : new List<SelectedSanPham>();
            }

            public static void ClearOrder(int tableNumber)
            {
                orderData.Remove(tableNumber); // Xóa dữ liệu khi thanh toán
            }
        }


        private void LoadOrderTamThoi()
        {
            // Cập nhật DataSource của dataGridView2
            dataGridView2.DataSource = selectedSanPhams;

            // Nếu cần thiết, có thể chỉ định các cột cần hiển thị
            dataGridView2.Columns["MaSanPham"].HeaderText = "Mã Sản Phẩm";
            dataGridView2.Columns["TenSanPham"].HeaderText = "Tên Sản Phẩm";
            dataGridView2.Columns["SoLuong"].HeaderText = "Số Lượng";

            // Cho phép người dùng chỉnh sửa cột Số Lượng
            dataGridView2.Columns["SoLuong"].ReadOnly = false;

            // Cập nhật tổng số lượng và tổng tiền
            var (tongSoLuong, tongTien) = qlOrderService.TinhTong(selectedSanPhams);
            txtSoLuong.Text = $" {tongSoLuong}";
            txtTongTien.Text = $"{tongTien:C2}"; // Định dạng tiền tệ

            
        }


        private void LoadComboBoxLoai()
        {
            cmbLoai.SelectedIndexChanged -= cmbLoai_SelectedIndexChanged;

            List<LoaiNuoc> loaiNuocs = qlOrderService.GetAllLoaiNuoc();  // Lấy danh sách loại nước từ cơ sở dữ liệu

            if (loaiNuocs == null || !loaiNuocs.Any())
            {
                MessageBox.Show("Không có dữ liệu loại nước trong cơ sở dữ liệu.");
                return;
            }

            // Cài đặt DataSource cho ComboBox
            cmbLoai.DataSource = loaiNuocs;
            cmbLoai.DisplayMember = "TenLoaiNuoc";  // Hiển thị tên loại trong ComboBox
            cmbLoai.ValueMember = "MaLoaiNuoc";

            // Chọn loại nước đầu tiên (mặc định) và tải sản phẩm tương ứng
            cmbLoai.SelectedIndex = 0;

            // Tắt chế độ tải dữ liệu ban đầu
            isLoading = false;

            // Gọi phương thức hiển thị sản phẩm theo loại đã chọn
            LoadSanPhamTheoLoai((int)cmbLoai.SelectedValue);

            // Kết nối lại sự kiện
            cmbLoai.SelectedIndexChanged += cmbLoai_SelectedIndexChanged;
        }
        private void LoadSanPhamTheoLoai(int loaiId)
        {
            List<SanPham> sanPhams = qlOrderService.GetSanPhamByLoai(loaiId);  // Lấy danh sách sản phẩm theo loại nước
            dataGridView1.DataSource = sanPhams;
            dataGridView1.Columns["ChiTietDonHangs"].Visible = false;
            dataGridView1.Columns["LoaiNuoc"].Visible = false;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void cmbLoai_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isLoading && cmbLoai.SelectedValue != null)
            {
                if (int.TryParse(cmbLoai.SelectedValue.ToString(), out int selectedLoaiId))
                {
                    // Hiển thị danh sách sản phẩm tương ứng với loại nước đã chọn
                    LoadSanPhamTheoLoai(selectedLoaiId);
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn loại nước hợp lệ.");
                }
            }
        }







        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            LoadOrderTamThoi();

            if (e.RowIndex >= 0)
            {
                // Lấy thông tin sản phẩm đã chọn từ DataGridView1
                var selectedSanPham = new SelectedSanPham
                {
                    MaSanPham = (int)dataGridView1.Rows[e.RowIndex].Cells["MaSanPham"].Value,
                    TenSanPham = dataGridView1.Rows[e.RowIndex].Cells["TenSanPham"].Value.ToString(),
                    Gia = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["Gia"].Value),
                    SoLuong = 1
                };

                var existingProduct = selectedSanPhams.FirstOrDefault(sp => sp.MaSanPham == selectedSanPham.MaSanPham);
                if (existingProduct == null)
                {
                    selectedSanPhams.Add(selectedSanPham);
                    RefreshDataGridView2();
                    // Cập nhật lại tổng số lượng và tổng tiền
                    var (tongSoLuong, tongTien) = qlOrderService.TinhTong(selectedSanPhams);
                    txtSoLuong.Text = $" {tongSoLuong}";
                    txtTongTien.Text = $" {tongTien:C2}"; // Định dạng tiền tệ
                }
                else
                {
                    MessageBox.Show("Sản phẩm đã được thêm vào danh sách.");
                }
            }
        }



        private void RefreshDataGridView2()
        {

            dataGridView2.DataSource = null; // Clear current DataSource
            dataGridView2.DataSource = selectedSanPhams; // Update DataSource
            dataGridView2.Columns["MaSanPham"].ReadOnly = true;
            dataGridView2.Columns["TenSanPham"].ReadOnly = true;
            dataGridView2.Columns["SoLuong"].ReadOnly = false;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Auto size columns

            // Call the new method to update totals
            UpdateOrderSummary();

            // Save data to temporary storage
            TemporaryOrderStorage.SaveOrder(soBan, selectedSanPhams);

        }
        private void UpdateOrderSummary()
        {
            var (tongSoLuong, tongTien) = qlOrderService.TinhTong(selectedSanPhams);
            txtSoLuong.Text = $"Tổng Số Lượng: {tongSoLuong}";
            txtTongTien.Text = $"Tổng Tiền: {tongTien:C2}";
            btnThanhToan.Enabled = selectedSanPhams.Count > 0; // Enable button if there are items
        }



        private void btnBoMon_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow != null) // Kiểm tra dòng hiện tại
            {
                // Lấy thông tin sản phẩm từ dòng được chọn
                var sanPham = (SelectedSanPham)dataGridView2.CurrentRow.DataBoundItem;

                // Xóa món khỏi danh sách đã chọn
                selectedSanPhams.Remove(sanPham);

                // Cập nhật DataGridView2
                RefreshDataGridView2();

                // Tính lại tổng
                var (tongSoLuong, tongTien) = qlOrderService.TinhTong(selectedSanPhams);
                txtSoLuong.Text = $" {tongSoLuong}";
                txtTongTien.Text = $"{tongTien:C2}"; // Định dạng tiền tệ

                // Hiển thị thông báo thành công
                MessageBox.Show($"Đã bỏ món: {sanPham.TenSanPham}.");
            }
            else
            {
                // Nếu không có hàng nào được chọn, hiển thị thông báo
                MessageBox.Show("Vui lòng chọn món để bỏ.");
            }
        }


        private void btnThanhToan_Click(object sender, EventArgs e)
        {

            if (selectedSanPhams.Count == 0)
            {
                MessageBox.Show("Không có sản phẩm nào để thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (tongSoLuong, tongTien) = qlOrderService.TinhTong(selectedSanPhams);
            ThanhToan formThanhToan = new ThanhToan(this, selectedSanPhams, tongSoLuong, tongTien);
            formThanhToan.Show();
            // Xóa dữ liệu tạm sau khi thanh toán thành công
            TemporaryOrderStorage.ClearOrder(soBan);
        }
        

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView2.Columns["SoLuong"].Index)
            {
                // Lấy sản phẩm từ danh sách
                var sanPham = (SelectedSanPham)dataGridView2.Rows[e.RowIndex].DataBoundItem;
                int newQuantity;

                if (int.TryParse(dataGridView2.Rows[e.RowIndex].Cells["SoLuong"].Value.ToString(), out newQuantity) && newQuantity >= 0)
                {
                    // Cập nhật số lượng trong danh sách đã chọn
                    sanPham.SoLuong = newQuantity;

                    // Debug thông báo số lượng đã cập nhật
                    Debug.WriteLine($"Số lượng cập nhật: {sanPham.SoLuong}");

                    // Cập nhật số lượng trong dịch vụ
                    qlOrderService.UpdateSanPhamQuantity(sanPham.MaSanPham, newQuantity);
                }
                else
                {
                    MessageBox.Show("Số lượng không hợp lệ. Vui lòng nhập số lớn hơn hoặc bằng 0.");
                    // Khôi phục lại giá trị ban đầu
                    dataGridView2.Rows[e.RowIndex].Cells["SoLuong"].Value = sanPham.SoLuong;
                    return;
                }

                // Cập nhật lại tổng số lượng và tổng tiền sau khi sửa
                var (tongSoLuong, tongTien) = qlOrderService.TinhTong(selectedSanPhams);
                txtSoLuong.Text = $"Tổng Số Lượng: {tongSoLuong}";
                txtTongTien.Text = $"Tổng Tiền: {tongTien:C2}"; // Định dạng tiền tệ
            }
        }
    }
}
