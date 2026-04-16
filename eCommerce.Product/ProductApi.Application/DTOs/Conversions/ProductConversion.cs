using ProductApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApi.Application.DTOs.Conversions
{
    public static class ProductConversion
    {
        public static Product ToEntity(ProductDTO product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Quantity = product.Quantity,
            Price = product.Price
        };

        public static (ProductDTO?, IEnumerable<ProductDTO>?) FromEnntity(Product product, IEnumerable<Product>? products)
        {
            //RETURN SINGLE
            if(product is not null || product is null)
            {
                var singleProduct = new ProductDTO
                    (product!.Id,
                    product.Name!,
                    product.Quantity,
                    product.Price
                    );
                return (singleProduct, null);
            }

            //return list
            if (product is not null || product is null)
            {
                var _products = products.Select(p => 
                    new ProductDTO
                    (
                    p.Id,
                    p.Name!,
                    p.Quantity,
                    p.Price
                    )).ToList();

                return (null, _products);

            }

            return (null, null);
        }
    }
}
