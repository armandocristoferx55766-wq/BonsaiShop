
using System.Collections.Generic;

namespace BonsaiShop.Models
{
    public class CheckoutViewModel
    {
        // Thông tin khách hàng
        public string FullName { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        // Danh sách sản phẩm
        public List<CartItem> CartItems { get; set; }

        // Tổng tiền
        public decimal GrandTotal { get; set; }

        //FullName: Họ tên người nhận hàng.
        //Phone: Số điện thoại nhận hàng.
        //Address: Địa chỉ giao hàng.
        //CartItems: Các sản phẩm trong giỏ.
        //GrandTotal: Tổng tiền đơn hàng.
    }
}
