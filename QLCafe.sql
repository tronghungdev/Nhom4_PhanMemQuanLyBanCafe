create database QLCaFe
use QLCaFe

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap NVARCHAR(50) UNIQUE NOT NULL,  -- Tên đăng nhập duy nhất
    MatKhau NVARCHAR(100) NOT NULL             -- Mật khẩu (nên mã hóa)
);

-- Tạo bảng LoaiNuoc (Loại nước)
CREATE TABLE LoaiNuoc (
    MaLoaiNuoc INT PRIMARY KEY,  -- Khóa chính do người dùng quản lý
    TenLoaiNuoc NVARCHAR(100) NOT NULL
);

-- Tạo bảng SanPham (Sản phẩm)
CREATE TABLE SanPham (
    MaSanPham INT PRIMARY KEY IDENTITY(1,1),
    TenSanPham NVARCHAR(100) NOT NULL,
    Gia DECIMAL(10, 2) NOT NULL,
    MaLoaiNuoc INT NOT NULL, -- Liên kết với bảng LoaiNuoc
    FOREIGN KEY (MaLoaiNuoc) REFERENCES LoaiNuoc(MaLoaiNuoc)
);

-- Tạo bảng KhachHang (Khách hàng)
CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    TenKhachHang NVARCHAR(100) NOT NULL,
    SoDienThoai NVARCHAR(15),
    DiemTichLuy INT DEFAULT 0
);

-- Tạo bảng DonHang (Đơn hàng)
CREATE TABLE DonHang (
    MaDonHang INT PRIMARY KEY IDENTITY(1,1),
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    TongTien DECIMAL(10, 2) NOT NULL,
    MaTaiKhoan INT NOT NULL,   -- Nhân viên bán hàng (Liên kết với bảng TaiKhoan)
    MaKhachHang INT NULL,      -- Khách hàng (Liên kết với bảng KhachHang)
    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);

-- Tạo bảng ChiTietDonHang (Chi tiết đơn hàng)
CREATE TABLE ChiTietDonHang (
    MaChiTiet INT PRIMARY KEY IDENTITY(1,1),
    MaDonHang INT NOT NULL,    -- Liên kết với bảng DonHang
    MaSanPham INT NOT NULL,    -- Liên kết với bảng SanPham
    SoLuong INT NOT NULL,
    GiaBan DECIMAL(10, 2) NOT NULL,  -- Giá bán tại thời điểm bán
    FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang),
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
);

-- Tạo bảng BaoCaoDoanhThu (Báo cáo doanh thu)
CREATE TABLE BaoCao (
    MaBaoCao INT PRIMARY KEY IDENTITY(1,1),
    NgayBaoCao DATE NOT NULL,
    TongDoanhThu DECIMAL(10, 2) NOT NULL,
    SoLuongDonHang INT NOT NULL
);


-- Dữ liệu mẫu cho bảng TaiKhoan
INSERT INTO TaiKhoan (TenDangNhap, MatKhau) 
VALUES ('admin', '123456'), ('seller', 'password123');

-- Dữ liệu mẫu cho bảng LoaiNuoc
-- Dữ liệu mẫu cho bảng LoaiNuoc với mã loại tự tạo
INSERT INTO LoaiNuoc (MaLoaiNuoc, TenLoaiNuoc) 
VALUES (1, N'Cafe'), (2, N'Sinh Tố');

Delete from SanPham;
-- Dữ liệu mẫu cho bảng SanPham
INSERT INTO SanPham (TenSanPham, Gia, MaLoaiNuoc) 
VALUES (N'Cafe Đen', 25000, 1), (N'Sinh Tố Bơ', 35000, 2);

-- Dữ liệu mẫu cho bảng KhachHang
INSERT INTO KhachHang (TenKhachHang, SoDienThoai, DiemTichLuy) 
VALUES ('Nguyen Van A', '0909123456', 10), ('Tran Thi B', '0909234567', 20);

-- Dữ liệu mẫu cho bảng DonHang
INSERT INTO DonHang (NgayTao, TongTien, MaTaiKhoan, MaKhachHang) 
VALUES (GETDATE(), 50000, 1, 1), (GETDATE(), 70000, 2, 2);

-- Dữ liệu mẫu cho bảng ChiTietDonHang
INSERT INTO ChiTietDonHang (MaDonHang, MaSanPham, SoLuong, GiaBan) 
VALUES (1, 1, 2, 25000), (2, 2, 2, 35000);

-- Dữ liệu mẫu cho bảng BaoCaoDoanhThu
INSERT INTO BaoCaoDoanhThu (NgayBaoCao, TongDoanhThu, SoLuongDonHang) 
VALUES (GETDATE(), 120000, 2), (GETDATE(), 150000, 3);

delete from LoaiNuoc
select * from LoaiNuoc
use QLCaFe
SELECT * FROM DonHang WHERE NgayTao = CAST(GETDATE() AS DATE);
SELECT NgayTao, CONVERT(DATE, NgayTao) AS Ngay FROM DonHang;
select * from DonHang
delete
FROM KhachHang
WHERE NgayTao >= CAST(GETDATE() AS DATE) -- Bắt đầu từ đầu ngày
AND NgayTao < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)); -- Kết thúc trước khi bắt đầu ngày mai
delete from KhachHang