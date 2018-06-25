$(document).ready(function () {
    //TAGS
    tagInput = $("input[name=tags]");
    tagList = $(".tags-list");

    tagInput.keypress(function (e) {
        if (e.which == 13) {
            //add tag
            tagList.append($('<li><input readonly class="tag-value" type="text" name="tags[]" value="#'+tagInput.val()+'"><i class="fa fa-times delete"></i></li >'))
            tagInput.val("");
            $(".delete").on("click", function () {
                $(this).parent().remove();
            })
        }
    })

    //VISIBILIDAD

    $("input[id=pub]").on("click", function () {
        $(".work-teams").hide(200)
    })

    $("input[id=priv]").on("click", function () {
        $(".work-teams").show(200)
    })

    $("input.submit").on("click", function () {
        console.log($("form"));
        $("form").submit();
    })
});