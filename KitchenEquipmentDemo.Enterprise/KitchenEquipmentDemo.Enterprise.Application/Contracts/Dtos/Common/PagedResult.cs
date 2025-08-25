﻿using System.Collections.Generic;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.Common
{
    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
