namespace WebsiteBanHang.Models
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        // Trạng thái đơn hàng
        public const string StatusPending = "Đang chờ";
        public const string StatusInProcess = "Đang giao";
        public const string StatusShipped = "Đã giao";
        public const string StatusCancelled = "Bị hủy";
    }
}
