﻿using Business.Abstract;
using Business.BusinessAspect.Autofac;
using Business.CCS;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Validation;
using Core.CrossCuttingConserns.Validation;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.InMemory;
using Entities.Concrete;
using Entities.CTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        //IProduct türünden bir değişken tanımlamak
        IProductDal _productDal;
        ICategoryService _CategoryService;

        public ProductManager(IProductDal productDal,ICategoryService categoryService)
        {
            _productDal = productDal;
            _CategoryService = categoryService;
        }
        [SecuredOperation("product.add,admin")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("")]
        [TransactionScopeAspect()]
        public IResult Add(Product product)
        {
            IResult result = BusinessRules.Run
                   (
                   CategoriesCountControl(product.CategoryID),
                   ProductNameControl(product.ProductName),
                   CategoryLimitControl()
                   );
            if (result != null)
            {
                return result;
            }
            _productDal.Add(product);
            return new SuccessResult(Messages.ProductAdded);    
        }
        [CacheAspect]
        public IDataResult<List<Product>> GetAll()
        {
            if (DateTime.Now.Hour == 4)
            {
                return new ErrorDataResult<List<Product>>(Messages.MaintenanceTime);
            }

            return new SuccessDataResult<List<Product>>(_productDal.GetAll(), Messages.ProductsListed);
        }

        public IDataResult<List<Product>> GetAllByCategoryId(int id)
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(p => p.CategoryID == id));
        }
        [CacheAspect]

        public IDataResult<Product> GetById(int productId)
        {
            return new SuccessDataResult<Product>(_productDal.Get(p => p.ProductID == productId));
        }

        public IDataResult<List<Product>> GetByUnitPrice(decimal min, decimal max)
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(p => p.UnitPrice >= min && p.UnitPrice <= max));
        }

        public IDataResult<List<ProductDetailDto>> GetProductDetails()
        {

            return new SuccessDataResult<List<ProductDetailDto>>(_productDal.GetProductDetailDto());
        }
        private IResult CategoriesCountControl(int categoryId)
        {
            var result = _productDal.GetAll(p => p.CategoryID == categoryId).Count;
            if (result >= 200)
            {
                return new ErrorResult(Messages.ProductCountOfCategryError);
            }
            return new SuccessResult();
        }
        private IResult ProductNameControl(string productName)
        {
            var result = _productDal.GetAll(p => p.ProductName == productName).Any();
            if (result)
            {
            return new ErrorResult(Messages.ProductNameOfError);
            }
                return new SuccessResult();
        }
        private IResult CategoryLimitControl()
        {
            var result = _CategoryService.GetAll();
            if (result.Data.Count >= 15)
            {
            return new ErrorResult(Messages.CategoriseCountOfCategryError);
            }
                return new SuccessResult();
        }
        [TransactionScopeAspect]
        public IResult AddTransactionalTest(Product product)
        {
            throw new NotImplementedException();
        }
    }
}
