$(".btn.add").click(function () {
    $("span.error").hide(200);
    groupId = $(".groups").data("id");    
    
    $.ajax({
        url: "/groups/validateInvitation",
        method: "post",
        dataType: "json",
        data: {"mail": $(".addEmail").val(),"newGroup":false, 'GroupID':groupId },
        success: function (response) {
            if (!response.success) {
                $("span.error").text(response.responseText);
                $("span.error").show(200);
            } else {
                $(".invited").append($('<div class="invitation"><input readonly class="form-control" type="text" name="invited[]" value="' + $(".addEmail").val() +'" /><i class="delete fa fa-times delete"></i></div>'))
                $(".addEmail").val("");
                $(".delete").click(function () {
                    $(this).closest(".invitation").remove();
                })
            }
            $(".addEmail").focus();
        },
        error: function (response) {
            alert(response.responseText);
        }
    })

   
})
