

    function GoToAlbum(id){
        window.location = "/album/?id=" + id;
     }
 
 
 
    function AddToCart(id, max) {


        var quant = $('#cart-quantity').html();

        var num = parseInt(quant);

        console.log(id);

        $('#cart-quantity').html(num + 1);

    
        var itemId = id;
        var quant = 1;
        var max = max;

        $.ajax({
            type: "POST",
            url: "/Home/JqAJAX",
            data: { itemId: itemId, quant: quant, max: max },
            dataType: "json",
            success: function (data) {
                console.log(data)
            }


        });

       

}


      


function ShowCart() {
    window.location = "/checkout";
}


function ChangeQuantity(quant,id, price, max) {
    var num = document.getElementById(id).innerHTML;

    var itemId = id;
    var quant = quant;
    var max = max;

    var confirm = false;

    if (quant < 0 && num > 1) {

        confirm = true;
        value = -price;
        console.log(parseInt(num) + quant)
        }



    if (quant > 0 && num < max) {


        confirm = true;
        value = +price;
        console.log(parseInt(num) + quant)
    }

        if (confirm) {
            document.getElementById(id).innerHTML = parseInt(num) + quant;
            var totalsum = price * (parseInt(num) + quant);
            document.getElementById(id + "total").innerHTML = totalsum;

            var totalcost = document.getElementById('totalcost').innerHTML;
            document.getElementById('totalcost').innerHTML = parseInt(totalcost) + value;

            var totaltotalcost = document.getElementById('totaltotalcost').innerHTML;
            document.getElementById('totaltotalcost').innerHTML = parseInt(totaltotalcost) + value;
  
            document.getElementById('totalsumtext').innerHTML = parseInt(totaltotalcost) + value;

            $.ajax({
                type: "POST",
                url: "/Home/JqAJAX",
                data: { itemId: itemId, quant: quant, max: max },
                dataType: "json",
                success: function (data) {
                    console.log(data)
                }


            });
        }


    }



function getShipping(shipping) {
    console.log(shipping)

    document.getElementById('shippingcost').innerHTML = shipping;

    var totaltotalcost = document.getElementById('totaltotalcost');
    var totalcost = document.getElementById('totalcost').innerHTML;

    totaltotalcost.innerHTML = parseInt(totalcost) + shipping;

    document.getElementById('totalsumtext').innerHTML = totaltotalcost.innerHTML;
}

function fillField(id) {
    var field = document.getElementById(id);
    var input = document.getElementById(id + 'input');

    field.innerHTML = input.value;

    console.log(id);


}

window.onload = function () {
    var windowLoc = $(location).attr('pathname');


    if (windowLoc == "/checkout") {

        var shipping = document.getElementById('standardshipping');
        console.log(shipping.children);
    shipping.children[3].checked = "checked";
    }


}

function showOrder(id) {


    $("#" + id).slideToggle("slow");

    var innertext = event.target.innerHTML;
    var sign = "";

    if (innertext == "+") {
        sign = "-";

    }

     if (innertext == "-"){
         sign = "+";
       console.log(event.target.innerHTML)
    }

    event.target.innerHTML = sign;
   

  
    
}

function WatchThis(user, itemid) {


    event.target.parentElement.innerHTML = "<button class='buttonoff'><i class='fas fa-check'></i> Bevakar</button>"
    $.ajax({
        type: "POST",
        url: "/Home/WatchListAjax",
        data: { itemid: itemid, user: user },
        dataType: "json",
        success: function (data) {
            
        }


    });

}