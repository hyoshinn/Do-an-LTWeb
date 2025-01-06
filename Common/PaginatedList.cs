using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyShopDoGiaDung.Common
{
    public class PaginatedList<T>
    {
        public int PageIndex { get; set; }  
        public int TotalPage { get; set; }
        public int Count {get; set;}
        public bool CanNext {get; set; }
        public bool CanPrevious { get; set; }   
        public List<T> Data { get ; set;}
        
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            Count= count;
            PageIndex = pageIndex;
            TotalPage = (int)Math.Ceiling(count / (double)pageSize);
            CanNext =  (int)Math.Ceiling(count / (double)pageSize) > pageIndex;
            CanPrevious = pageIndex > 1 ;
            Data = items;
        }
    }
}