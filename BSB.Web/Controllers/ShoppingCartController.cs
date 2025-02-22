﻿using BSB.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BSB.Web.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService shoppingCartService;

        public ShoppingCartController(IShoppingCartService shoppingCartService)
        {
            this.shoppingCartService = shoppingCartService;
        }
        public async Task< IActionResult> GetCartInfo()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return View(await this.shoppingCartService.getShoppingCartInfo(userId));
        }
        public async Task<IActionResult> DeleteFromShoppingCart(Guid id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await this.shoppingCartService.deleteProductFromShoppingCart(userId, id);

            if (result)
            {
                return RedirectToAction("GetCartInfo", "ShoppingCart");
            }
            else
            {
                return RedirectToAction("GetCartInfo", "ShoppingCart");
            }
        }

        public async Task<IActionResult> PayOrder(string stripeEmail, string stripeToken)
        {
            var customerService = new CustomerService();
            var chargeService = new ChargeService();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await this.shoppingCartService.getShoppingCartInfo(userId);

            var customer = customerService.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            var charge = chargeService.Create(new ChargeCreateOptions
            {
                Amount = (Convert.ToInt32(order.TotalPrice) * 100),
                Description = "Book shop Application Payment",
                Currency = "usd",
                Customer = customer.Id
            });

            if (charge.Status == "succeeded")
            {
                var result = await this.Order();

                if (result)
                {
                    return RedirectToAction("GetOrders", "Order");
                }
                else
                {
                    return RedirectToAction("GetCartInfo", "ShoppingCart");
                }
            }

            return RedirectToAction("Index", "Products");
        }
        private async Task<bool> Order()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await this .shoppingCartService.orderNow(userId);

            return result;
        }
    }
}