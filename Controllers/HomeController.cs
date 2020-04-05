using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using projekt.Data;
using projekt.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace projekt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AlbumContext _context;

        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;



        private IHttpContextAccessor _accessor;

        public HomeController(ILogger<HomeController> logger, AlbumContext context, IHttpContextAccessor accessor, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _env = env;
            _accessor = accessor;
        }

        public IActionResult Index()
        {
            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            ViewBag.Popular = _context.Albums.FromSqlRaw("select top 4 * from Albums order by Sales desc").ToList();
            ViewBag.News = _context.Albums.FromSqlRaw("select top 3 * from Albums order by AlbumId desc").ToList();
            var shuffled = _context.Albums.FromSqlRaw("select  * from Albums where Recommendation = 1").ToList();
            ViewBag.Recs = shuffled.OrderBy(x => Guid.NewGuid()).ToList().Take(3);
            return View();
        }


    



        [Route("/checkout")]
        public IActionResult Checkout(string order)
        {

            ViewBag.Page = "Checkout";
            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];



                var userid = Int32.Parse(user);

                var userinfo = (from c in _context.Users
                                where c.UserId == userid
                                select c).FirstOrDefault();

                ViewBag.UserInfo = userinfo;
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            if (order == "confirmed")
            {
                ViewBag.Confirmed = "confirmed";
            }

            List<CartTest> query = (from a in _context.CartItems
                                    join b in _context.Albums on a.ItemId equals b.AlbumId
                                    where a.User == user
                                    select new CartTest
                                    {
                                        name = b.Name,
                                        artist = b.Artist,
                                        id = b.AlbumId,
                                        user = a.User,
                                        price = b.Price,
                                        quantity = a.Quantity,
                                        image = b.Image,
                                        max = b.Quantity
                                    }).ToList();

            ViewBag.Query = query.ToList();

            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            return View();
        }

        [Route("/checkout")]
        [HttpPost]
        public IActionResult Checkout(Order model)
        {

            var user = "";
            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];



                var userid = Int32.Parse(user);

                var userinfo = (from c in _context.Users
                                where c.UserId == userid
                                select c).FirstOrDefault();

                ViewBag.UserInfo = userinfo;
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }


            var totalsum = 0;
            var shipping2 = model.Shipping;
            var h = model.Address;
            var z = model.Zip;
            var g = model.City;

            var useridsend = Int32.Parse(Request.Cookies["loggedin"]);
            List<CartTest> query = (from a in _context.CartItems
                                    join b in _context.Albums on a.ItemId equals b.AlbumId
                                    where a.User == Request.Cookies["loggedin"].ToString()
                                    select new CartTest
                                    {
                                        name = b.Name,
                                        artist = b.Artist,
                                        id = b.AlbumId,
                                        user = a.User,
                                        price = b.Price,
                                        quantity = a.Quantity,
                                        image = b.Image,
                                        max = b.Quantity
                                    }).ToList();

            ViewBag.Query = query.ToList();

            foreach (var item in ViewBag.Query)
            {
                totalsum = Int32.Parse(totalsum.ToString()) + (item.price * item.quantity);
            }

            if (ModelState.IsValid)
            {




                var temp = useridsend.ToString() + DateTime.Now;




                var order = new Order()
                {
                    Address = model.Address,
                    Zip = model.Zip,
                    UserId = useridsend,
                    City = model.City,
                    TempId = temp,
                    Date = DateTime.Now,
                    Shipping = model.Shipping,
                    Total = totalsum
                };

                _context.Orders.Add(order);

                _context.SaveChanges();

                var orderid = _context.Orders.FromSqlRaw("select * from Orders where TempId ='" + temp + "'").FirstOrDefault();
                var id = 0;

                id = orderid.OrderId;


                foreach (var item in ViewBag.Query)
                {
                    var orderitem = new OrderItem()
                    {
                        ItemId = item.id,
                        OrderId = id.ToString(),
                        Quantity = item.quantity
                    };

                    string x = Convert.ToString(item.id);
                    var Itemid = Int32.Parse(x);

                    var update = (from a in _context.Albums
                                  where a.AlbumId == Itemid
                                  select a).FirstOrDefault(); 

                    update.Quantity = update.Quantity - item.quantity;
                    _context.SaveChanges();

                    update.Sales = update.Sales + item.quantity;
                    _context.SaveChanges();

                    var del = (from c in _context.CartItems
                               where c.User == Request.Cookies["loggedin"].ToString() && c.ItemId == Itemid
                               select c).FirstOrDefault();

                    _context.CartItems.Remove(del);
                    _context.SaveChanges();

                    _context.OrderItems.Add(orderitem);

                    _context.SaveChanges();
                }

                var mailuser = (from a in _context.Users
                              where a.UserId == Int32.Parse(Request.Cookies["loggedin"].ToString())
                            select a).FirstOrDefault();

                // SKICKA ORDERBEKRÄFTELSE
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Planet Vinyl", "teamplanetvinyl@outlook.com"));
                message.To.Add(new MailboxAddress(mailuser.FirstName, mailuser.Mail));
                message.Subject = "Planet Vinyl Orderbekräftelse";

                //var webRoot = _env.WebRootPath;

                //var pathToFile = _env.WebRootPath
                //         + Path.DirectorySeparatorChar.ToString()
                //         + "Templates/"
                //         + "mail.html";



                BodyBuilder bodyBuilder = new BodyBuilder();



                var neworderid = _context.Orders.FromSqlRaw("select * from Orders where TempId ='" + temp + "'").FirstOrDefault();

                var itemsid = neworderid.OrderId;

                var orderitems = (from c in _context.OrderItems
                                  where c.OrderId == itemsid.ToString()
                                  select c).ToList();

                var items = "";

                foreach (var item in orderitems)
                {
                    var orderalbums = (from c in _context.Albums
                                       where c.AlbumId == item.ItemId
                                       select c).FirstOrDefault();

                    items += "<table border='0' cellpadding='0' cellspacing='0' align='center' width='100%' role='module' data-type='columns' style='padding:20px 20px 0px 30px;' bgcolor='#FFFFFF'> <tbody> <tr role='module-content'> <td height='100%' valign='top'> <table class='column' width='137' style='width:137px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor=''> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'><table class='wrapper' role='module' data-type='image' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='239f10b7-5807-4e0b-8f01-f2b8d25ec9d7'> <tbody> <tr> <td style='font-size:6px; line-height:10px; padding:0px 0px 0px 0px;' valign='top' align='left'> <img class='max-width' border='0' style='display:block; color:#000000; text-decoration:none; font-family:Helvetica, arial, sans-serif; font-size:16px;' width='104' alt='' data-proportionally-constrained='true' data-responsive='false' src='" + orderalbums.Image + "' height='104'> </td></tr></tbody> </table></td></tr></tbody> </table> <table class='column' width='137' style='width:137px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor=''> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='f404b7dc-487b-443c-bd6f-131ccde745e2'> <tbody> <tr> <td style='padding:18px 0px 18px 0px; line-height:22px; text-align:inherit;' height='100%' valign='top' bgcolor='' role='module-content'><div><div style='font-family: inherit; text-align: inherit'>" + orderalbums.Name + "</div><div style='font-family: inherit; text-align: inherit'><span style='color: #910037'>" + orderalbums.Price + "( x " + item.Quantity + ") SEK &nbsp;</span></div><div></div></div></td></tr></tbody> </table></td></tr></tbody> </table> <table width='137' style='width:137px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor='' class='column column-2'> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'></td></tr></tbody> </table><table width='137' style='width:137px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor='' class='column column-3'> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'></td></tr></tbody> </table></td></tr></tbody> </table>";
                }

                var names = (from c in _context.Users
                             where c.UserId == neworderid.UserId
                             select c).FirstOrDefault();

                var name = (names.FirstName + " " + names.LastName).ToString();



                var shipping = Int32.Parse(neworderid.Shipping.ToString());

                var days = 0;

                if (shipping == 0)
                {
                    days = 10;
                }

                else if (shipping == 39)
                {
                    days = 2;
                }

                else
                {
                    days = 5;
                }


                var sum = Int32.Parse(neworderid.Total.ToString());
                var total = sum + shipping;




                //bodyBuilder.HtmlBody = "<!DOCTYPE html><html><head><meta charset='utf-8'/><title>Planet Vinyl</title></head><body style='font-family:Arial;'><img src='https://i.imgur.com/eUNl06H.png'/> <div><h1> Orderbekräftelse </h1><h2>Tack för din beställning " + name + "</h2><table bgcolor='#F2F2F2' width='600' cellpadding='0' cellspacing='0' border='0' class='container' color='#000'> <thead>   <tr> <th>Skiva</th><th> Pris</th><th>  Antal</th> </tr> </thead><tbody>" + items + "  </tbody></table><h4> Totalt: " + total + " SEK </h4><h4> Beräknat leveransdatum: " + DateTime.Now.AddDays(5).ToString().Substring(0,10)  + ".</h4></div></body></html>";
                bodyBuilder.HtmlBody = "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'><html data-editor-version='2' class='sg-campaigns' xmlns='http://www.w3.org/1999/xhtml'><head> <meta http-equiv='Content-Type' content='text/html; charset=utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1, minimum-scale=1, maximum-scale=1'> <meta http-equiv='X-UA-Compatible' content='IE=Edge'><!--[if (gte mso 9)|(IE)]> <xml> <o:OfficeDocumentSettings> <o:AllowPNG/> <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings> </xml><![endif]--><!--[if (gte mso 9)|(IE)]> <style type='text/css'> body{width: 600px;margin: 0 auto;}table{border-collapse: collapse;}table, td{mso-table-lspace: 0pt;mso-table-rspace: 0pt;}img{-ms-interpolation-mode: bicubic;}</style><![endif]--> <style type='text/css'> body, p, div{font-family: inherit; font-size: 14px;}body{color: #000000;}body a{color: #1188E6; text-decoration: none;}p{margin: 0; padding: 0;}table.wrapper{width:100% !important; table-layout: fixed; -webkit-font-smoothing: antialiased; -webkit-text-size-adjust: 100%; -moz-text-size-adjust: 100%; -ms-text-size-adjust: 100%;}img.max-width{max-width: 100% !important;}.column.of-2{width: 50%;}.column.of-3{width: 33.333%;}.column.of-4{width: 25%;}@media screen and (max-width:480px){.preheader .rightColumnContent, .footer .rightColumnContent{text-align: left !important;}.preheader .rightColumnContent div, .preheader .rightColumnContent span, .footer .rightColumnContent div, .footer .rightColumnContent span{text-align: left !important;}.preheader .rightColumnContent, .preheader .leftColumnContent{font-size: 80% !important; padding: 5px 0;}table.wrapper-mobile{width: 100% !important; table-layout: fixed;}img.max-width{height: auto !important; max-width: 100% !important;}a.bulletproof-button{display: block !important; width: auto !important; font-size: 80%; padding-left: 0 !important; padding-right: 0 !important;}.columns{width: 100% !important;}.column{display: block !important; width: 100% !important; padding-left: 0 !important; padding-right: 0 !important; margin-left: 0 !important; margin-right: 0 !important;}}</style> <link href='https://fonts.googleapis.com/css?family=Comfortaa|Raleway&display=swap' rel='stylesheet'><style>body{font-family: 'Raleway', sans-serif;}</style> </head> <body> <center class='wrapper' data-link-color='#1188E6' data-body-style='font-size:14px; font-family:inherit; color:#000000; background-color:#f0f0f0;'> <div class='webkit'> <table cellpadding='0' cellspacing='0' border='0' width='100%' class='wrapper' bgcolor='#f0f0f0'> <tbody><tr> <td valign='top' bgcolor='#f0f0f0' width='100%'> <table width='100%' role='content-container' class='outer' align='center' cellpadding='0' cellspacing='0' border='0'> <tbody><tr> <td width='100%'> <table width='100%' cellpadding='0' cellspacing='0' border='0'> <tbody><tr> <td><!--[if mso]> <center> <table><tr><td width='600'><![endif]--> <table width='100%' cellpadding='0' cellspacing='0' border='0' style='width:100%; max-width:600px;' align='center'> <tbody><tr> <td role='modules-container' style='padding:0px 0px 0px 0px; color:#000000; text-align:left;' bgcolor='#ffffff' width='100%' align='left'><table class='module preheader preheader-hide' role='module' data-type='preheader' border='0' cellpadding='0' cellspacing='0' width='100%' style='display: none !important; mso-hide: all; visibility: hidden; opacity: 0; color: transparent; height: 0; width: 0;'> <tbody><tr> <td role='module-content'> <p></p></td></tr></tbody></table><table border='0' cellpadding='0' cellspacing='0' align='center' width='100%' role='module' data-type='columns' style='padding:0;' bgcolor='#181818'> <tbody> <tr role='module-content'> <td height='100%' valign='top'> <table class='column' width='600' style='width:600px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor=''> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'><table class='wrapper' role='module' data-type='image' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='b422590c-5d79-4675-8370-a10c2c76af02'> <tbody> <tr> <td style='font-size:6px; line-height:10px; padding:0px 0px 0px 0px;' valign='top' align='left'> <img class='max-width' border='0' style='display:block; color:#000000; text-decoration:none; font-family:Helvetica, arial, sans-serif; font-size:16px;' width='600' alt='' data-proportionally-constrained='true' data-responsive='false' src='https://i.imgur.com/eUNl06H.png' > </td></tr></tbody> </table><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='1995753e-0c64-4075-b4ad-321980b82dfe'> <tbody> <tr> <td style='padding:100px 30px 18px 20px; line-height:36px; text-align:inherit;' height='100%' valign='top' bgcolor='' role='module-content'><div><div style='font-family: 'Comfortaa'; text-align: inherit'><span style='color: #ffffff; font-size: 40px; font-family: inherit'>Tack för din beställning!</span></div><div></div></div></td></tr></tbody> </table><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='2ffbd984-f644-4c25-9a1e-ef76ac62a549'> <tbody> <tr> <td style='padding:18px 20px 20px 20px; line-height:24px; text-align:inherit;' height='100%' valign='top' bgcolor='' role='module-content'><div><div style='font-family: inherit; text-align: inherit'><span style='font-size: 24px; color: #ffffff;' >Vi jobbar nu på att skicka ut dina</span></div><div style='font-family: inherit; text-align: inherit'><span style='font-size: 24px; color: #ffffff;'> varor så snabbt som möjligt!</span></div><div></div></div></td></tr><tr> <td style='padding:18px 20px 20px 20px; line-height:24px; text-align:inherit;' height='100%' valign='top' bgcolor='' role='module-content'><div><div style='font-family: inherit; text-align: inherit'><span style='font-size: 12px; color: #ffffff;' >*** OBS DETTA ÄR ENDAST EN PROTOTYP ***</span></div><div style='font-family: inherit; text-align: inherit'><span style='font-size: 15px; color: #ffffff;'> Håll utkik efter din faktura som alltid skickas ut direkt från Klarna efter paketet lämnat vårt lager.</span></div><div></div></div></td></tr></tbody> </table><table border='0' cellpadding='0' cellspacing='0' class='module' data-role='module-button' data-type='button' role='module' style='table-layout:fixed;' width='100%' data-muid='69fc33ea-7c02-45ed-917a-b3b8a6866e89'> <tbody> <tr> <td align='left' bgcolor='' class='outer-td' style='padding:0px 0px 0px 0px;'> <table border='0' cellpadding='0' cellspacing='0' class='wrapper-mobile' style='text-align:center;'> <tbody> </tbody> </table> </td></tr></tbody> </table></td></tr></tbody> </table> </td></tr></tbody> </table><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='8b5181ed-0827-471c-972b-74c77e326e3d'> <tbody> <tr> <td style='padding:30px 20px 18px 30px; line-height:22px; text-align:inherit;' height='100%' valign='top' bgcolor='' role='module-content'><div><div style='font-family: inherit; text-align: inherit'><span style='color: #910037; font-size: 24px'>Orderbekräftelse</span></div><div></div></div></td></tr></tbody> </table><table class='module' role='module' data-type='divider' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='f7373f10-9ba4-4ca7-9a2e-1a2ba700deb9'> <tbody> <tr> <td style='padding:0px 30px 0px 30px;' role='module-content' height='100%' valign='top' bgcolor=''> <table border='0' cellpadding='0' cellspacing='0' align='center' width='100%' height='3px' style='line-height:3px; font-size:3px;'> <tbody> <tr> <td style='padding:0px 0px 3px 0px;' bgcolor='#e7e7e7'></td></tr></tbody> </table> </td></tr></tbody> </table><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='264ee24b-c2b0-457c-a9c1-d465879f9935'> <tbody> <tr> <td style='padding:18px 20px 18px 30px; line-height:22px; text-align:inherit;' height='100%' valign='top' bgcolor='' role='module-content'><div><div style='font-family: inherit; text-align: inherit'>Ordernummer: " + itemsid + ". </div><div style='font-family: inherit; text-align: inherit'><span style='color: #910037'><strong>Beräknad leverans: " + DateTime.Now.AddDays(days).ToString().Substring(0, 10) + ".</strong></span></div><div style='font-family: inherit; text-align: inherit'><br></div><div style='font-family: inherit; text-align: inherit; font-size: 24px;'>Din order:&nbsp;</div><div></div></div></td></tr></tbody> </table> " + items + " <table class='module' role='module' data-type='divider' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='f7373f10-9ba4-4ca7-9a2e-1a2ba700deb9.1'> <tbody> <tr> <td style='padding:20px 30px 0px 30px;' role='module-content' height='100%' valign='top' bgcolor=''> <table border='0' cellpadding='0' cellspacing='0' align='center' width='100%' height='3px' style='line-height:3px; font-size:3px;'> <tbody> <tr> <td style='padding:0px 0px 3px 0px;' bgcolor='E7E7E7'></td></tr></tbody> </table> </td></tr></tbody> </table><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='264ee24b-c2b0-457c-a9c1-d465879f9935.1'> <tbody> <tr> <td style='padding:18px 20px 30px 30px; line-height:22px; text-align:inherit;' height='100%' valign='top' bgcolor='' role='module-content'><div><div style='font-family: inherit; text-align: inherit'>Summa: " + sum + " SEK</div><div style='font-family: inherit; text-align: inherit'>Frakt: " + shipping + " SEK</div><div style='font-family: inherit; text-align: inherit'><br>Totalt: &nbsp;</div><div style='font-family: inherit; text-align: inherit'><br></div><div style='font-family: inherit; text-align: inherit'><span style='color: #910037; font-size: 32px; font-family: inherit'>" + total + " SEK</span></div><div></div></div></td></tr></tbody> </table><table border='0' cellpadding='0' cellspacing='0' align='center' width='100%' role='module' data-type='columns' style='padding:0px 20px 0px 20px;' bgcolor='#910037'> <tbody> <tr role='module-content'> <td height='100%' valign='top'> <table class='column' width='140' style='width:140px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor=''> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='9d43ffa1-8e24-438b-9484-db553cf5b092'> </table></td></tr></tbody> </table> <table class='column' width='140' style='width:140px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor=''> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='9d43ffa1-8e24-438b-9484-db553cf5b092.1'> <tbody> </tbody> </table></td></tr></tbody> </table> <table width='140' style='width:140px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor='' class='column column-2'> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='9d43ffa1-8e24-438b-9484-db553cf5b092.1.1'> <tbody> </tbody> </table></td></tr></tbody> </table><table width='140' style='width:140px; border-spacing:0; border-collapse:collapse; margin:0px 0px 0px 0px;' cellpadding='0' cellspacing='0' align='left' border='0' bgcolor='' class='column column-3'> <tbody> <tr> <td style='padding:0px;margin:0px;border-spacing:0;'><table class='module' role='module' data-type='text' border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;' data-muid='9d43ffa1-8e24-438b-9484-db553cf5b092.1.1.1'> <tbody> </tbody> </table></td></tr></tbody> </table></td></tr></tbody> </table><div data-role='module-unsubscribe' class='module' role='module' data-type='unsubscribe' style='background-color:#910037; color:#ffffff; font-size:12px; line-height:20px; padding:16px 16px 16px 16px; text-align:Center;' data-muid='4e838cf3-9892-4a6d-94d6-170e474d21e5'> </div><table border='0' cellpadding='0' cellspacing='0' class='module' data-role='module-button' data-type='button' role='module' style='table-layout:fixed;' width='100%' data-muid='e5cea269-a730-4c6b-8691-73d2709adc62'> </table></td></tr></tbody></table><!--[if mso]> </td></tr></table> </center><![endif]--> </td></tr></tbody></table> </td></tr></tbody></table> </td></tr></tbody></table> </div></center> </body></html>";
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.live.com", 587);
                    client.Authenticate("teamplanetvinyl@outlook.com", "VinylPass1");
                    client.Send(message);
                    client.Disconnect(true);
                }



                return Redirect("/checkout?order=confirmed");
            }

            return View();
        }

        [Route("/genre")]
        public IActionResult Genre(string id)

        {
            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            var Genre = id;
            if (Genre == "Rock" || Genre == "Jazz" || Genre == "Pop" || Genre == "Klassiskt")
            {
                ViewBag.Genre = _context.Albums.FromSqlRaw("select * from Albums Where Genre = '" + Genre + "'order by AlbumId desc").ToList();
                ViewBag.Title = Genre;
            }
            else if (Genre == "Alla")
            {
                ViewBag.Genre = _context.Albums.FromSqlRaw("select * from Albums order by AlbumId desc").ToList();
                ViewBag.Title = "Alla";
            }
            else
            {
                ViewBag.Genre = _context.Albums.FromSqlRaw("select * from Albums Where Genre != 'Rock' and  Genre != 'Pop' and Genre != 'Rock' and  Genre != 'Jazz' and Genre != 'Klassiskt' order by AlbumId desc").ToList();
                ViewBag.Title = "Övrigt";
            }
            return View();
        }

        [Route("/artist")]
        public IActionResult Artist(string id)
        {
            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            var Artist = id;

            if (Artist != null)
            {
                ViewBag.Artist = _context.Albums.FromSqlRaw("select * from Albums Where Artist = '" + Artist + "' order by AlbumId desc").ToList();
                ViewBag.Title = Artist;
            }
            else
            {
                return Redirect("/Music");
            }

            return View();
        }

        [Route("/login")]
        public IActionResult Login(string id, string album)
        {


            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            if (id != null)
            {
                if (album != null)
                {
                    HttpContext.Session.SetString("album", album);
                }

                HttpContext.Session.SetString("redirect", id);
            }
            ViewBag.Loggedin = HttpContext.Session.GetString("Loggedin");
            if (HttpContext.Session.GetString("Loggedin") == "Confirmed")
            {
                return Redirect("mypage");
            }
            return View();
        }

        [Route("/login")]
        [HttpPost]
        public IActionResult Login(Users model)
        {
            var user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();

            var pass = model.Password;
            var salt = "y9jlK/vps0uGsREJgoS5Gg==";
            var hash = Hash.Create(pass, salt);

            var login = _context.Users.FromSqlRaw("select * from Users where Mail = '" + model.Mail + "'").ToList();

            var count = login.Count;

            if (count > 0)
            {
                ViewBag.Count = count;

                foreach (var item in login)
                {
                    var match = Hash.Validate(pass, salt, item.Password);

                    ViewBag.Count = salt;

                    if (match)
                    {
                        CookieOptions cookies = new CookieOptions();
                        cookies.Expires = DateTime.Now.AddDays(1);

                        Response.Cookies.Append("loggedin", (item.UserId).ToString());






                        var usercart = (from a in _context.CartItems
                                        where a.User == user
                                        select a).ToList();


                        foreach (var cart in usercart)
                        {

                            var update = (from a in _context.CartItems
                                          where a.User == item.UserId.ToString() && a.ItemId == cart.ItemId
                                          select a).ToList();


                            foreach (var same in update)
                            {


                                _context.CartItems.Remove(same);
                                _context.SaveChanges();
                            }







                        }

                        var update2 = (from a in _context.CartItems
                                       where a.User == user
                                       select a).ToList();

                        foreach (var same in update2)
                        {

                            same.User = item.UserId.ToString();
                            _context.SaveChanges();
                        }
                    }



                    if (HttpContext.Session.GetString("redirect") == "checkout")
                    {

                        HttpContext.Session.SetString("redirect", "");
                        return Redirect("/checkout");
                    }
                    else if (HttpContext.Session.GetString("redirect") == "album")
                    {
                        HttpContext.Session.SetString("redirect", "");
                        var album = HttpContext.Session.GetString("album");
                        HttpContext.Session.SetString("album", "");

                        return Redirect("/album?id=" + album);
                    }
                    else
                    {
                        return Redirect("/mypage");
                    }
                }
            }


            return View();
        }

        [Route("/music")]
        public IActionResult Music()
        {
            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            ViewBag.Rock = _context.Albums.FromSqlRaw("select top 4 * from Albums Where Genre = 'Rock' order by AlbumId desc").ToList();
            ViewBag.Pop = _context.Albums.FromSqlRaw("select top 4  * from Albums Where Genre = 'Pop' order by AlbumId desc").ToList();
            ViewBag.Jazz = _context.Albums.FromSqlRaw("select top 4  * from Albums Where Genre = 'Jazz' order by AlbumId desc").ToList();
            ViewBag.Klassiskt = _context.Albums.FromSqlRaw("select top 4  * from Albums Where Genre = 'Klassiskt' order by AlbumId desc").ToList();
            ViewBag.Övrigt = _context.Albums.FromSqlRaw("select top 4  * from Albums Where Genre != 'Rock' and  Genre != 'Pop' and Genre != 'Rock' and  Genre != 'Jazz' and Genre != 'Klassiskt' order by AlbumId desc").ToList();

            return View();
        }

        [Route("/mypage")]
        public IActionResult Mypage()
        {
            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];



                var userid = Int32.Parse(user);

                var userinfo = (from c in _context.Users
                                where c.UserId == userid
                                select c).FirstOrDefault();

                ViewBag.UserInfo = userinfo;



                var orders = (from c in _context.Orders
                              where c.UserId == userinfo.UserId
                              orderby c.Date descending
                              select c).ToList();

                ViewBag.Orders = orders;

                var orderitems = (from c in _context.OrderItems
                                  select c).ToList();

                ViewBag.OrderItems = orderitems;

                var albums = (from c in _context.Albums
                              orderby c.Quantity descending
                              select c).ToList();

                ViewBag.Albums = albums;


            }

            ViewBag.WatchList = (from c in _context.WatchList
         
                                 where c.UserId == Request.Cookies["loggedin"].ToString()
                                 select c).ToList();


            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            return View();
        }

        [Route("/album")]
        public IActionResult Album(int id)
        {
            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
                ViewBag.WatchList = (from c in _context.WatchList
                                     where c.ItemId == id
                                     where c.UserId == Request.Cookies["loggedin"].ToString()
                                     select c).FirstOrDefault();

                if(ViewBag.WatchList != null)
                {
 ViewBag.WatchList = ViewBag.WatchList.ItemId;
                }
               
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            var ID = id;
            

            ViewBag.Album = _context.Albums.FromSqlRaw("select * from Albums Where AlbumId =" + ID + "").ToList();
            return View();
        }

        [Route("/register")]
        public IActionResult Register(string id)
        {
          

            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            if (HttpContext.Session.GetString("Loggedin") == "Confirmed")
            {
                return Redirect("mypage");
            }
            
            if (id != null)
            {
     
                HttpContext.Session.SetString("redirect", id);
            }
            ViewBag.Loggedin = HttpContext.Session.GetString("Loggedin");
           

            return View();
        }

        [Route("/register")]
        [HttpPost]
        public IActionResult Register(Users model)
        {
            if (ModelState.IsValid)
            {
                var userNameToCheck = model.Mail;
                var exists = _context.Users.Any(x => x.Mail == userNameToCheck);
                if (exists)
                {
                    ViewBag.Confirm = "Existerar redan";
                }
                else
                {
                    var pass = model.Password;
                    var salt = "y9jlK/vps0uGsREJgoS5Gg==";
                    var hash = Hash.Create(pass, salt);

                    var user = new Users()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Mail = model.Mail,
                        Password = hash,
                        PasswordConfirm = hash
                    };

                    _context.Users.Add(user);

                    _context.SaveChanges();
                }
            }
        

            return Redirect("login");
        }

        [Route("/logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("loggedin");

            return Redirect("Home/Index");
        }


       
        public IActionResult FAQ()
        {
            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            return View();
        }


        public IActionResult Contact()
        {
            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            return View();
        }

        public IActionResult About()
        {
            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            return View();
        }

        public IActionResult Shipping()
        {
            var currentcart = _context.CartItems.FromSqlRaw("select * from CartItems").ToList();
            var total = 0;

            var user = "";

            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            foreach (var cartitems in currentcart)
            {
                if (cartitems.User == user)
                {
                    total = total + cartitems.Quantity;
                }
            }

            ViewBag.CurrentCart = total;

            return View();
        }




        public IActionResult RemoveWatch(int id)
        {
            var user = Request.Cookies["loggedin"];
            var del = (from c in _context.WatchList where
                      c.WatchId == id
                       && c.UserId == user
                       select c).FirstOrDefault();

            _context.WatchList.Remove(del);
            _context.SaveChanges();

            return Redirect("/mypage");
        }

            public IActionResult Remove(int id)
        {
            var user = "";
            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"];
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            var del = (from c in _context.CartItems
                       where c.ItemId == id
                       && c.User == user
                       select c).FirstOrDefault();

            _context.CartItems.Remove(del);
            _context.SaveChanges();

            return Redirect("/checkout");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public ActionResult JqAJAX()
        {
            return View();
        }

        [HttpPost]
        public ActionResult JqAJAX(int itemId, int quant, int max)
        {
            List<CartItems> cart = new List<CartItems>();

            var user = "";
            if (Request.Cookies["loggedin"] != null)
            {
                user = Request.Cookies["loggedin"].ToString();
                ViewBag.Loggedin = "Confirmed";
                ViewBag.Cookie = Request.Cookies["loggedin"];
            }
            else
            {
                user = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            var exists = _context.CartItems.Any(x => x.ItemId == itemId && x.User == user);

            if (exists)
            {
                CartItems item = _context.CartItems.FirstOrDefault(x => x.ItemId == itemId && x.User == user);

                if (quant < 0)
                {
                    item.Quantity = item.Quantity + quant;
                    _context.SaveChanges();
                }
                else
                {
                    if (item.Quantity < max)
                    {
                        item.Quantity = item.Quantity + quant;
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                var newCart = new CartItems { ItemId = itemId, Quantity = quant, User = user };

                cart.Add(newCart);

                _context.CartItems.Add(newCart);

                _context.SaveChanges();
            }

            return Json(cart);
        }

        public ActionResult WatchListAjax()
        {

            return View();
        }

        [HttpPost]
        public ActionResult WatchListAjax(int itemid, string user)
        {
            user  = Request.Cookies["loggedin"];

            var newWatch = new WatchList { ItemId = itemid, UserId = user};

            _context.WatchList.Add(newWatch);

            _context.SaveChanges();

            return View();
        }
    }
}