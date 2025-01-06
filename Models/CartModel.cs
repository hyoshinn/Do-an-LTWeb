
using ShopBanDoGiaDung.Models;

namespace QuanLyShopDoGiaDung.Models
{
    [Serializable]
    public class CartModel
    {
        public Sanpham sanpham {get; set;}
        public int soluong {get; set;}
    }
}