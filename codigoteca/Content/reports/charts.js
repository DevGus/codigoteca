$(document).ready(function () {

    var today = new Date();
    $('.datepicker-only-init').datetimepicker({
        widgetPositioning: {
            horizontal: 'left'
        },
        locale: "en",
        icons: {
            time: "fa fa-clock-o",
            date: "fa fa-calendar",
            up: "fa fa-arrow-up",
            down: "fa fa-arrow-down"
        },
        format: 'DD/MM/YYYY'
    });

    $("form .button-go").on("click", function () {
        model = $(this).attr("data-model");
        $(".button-go").attr("href", "/reports/" + model + "?search=1&from=" + $("input[name=from]").val() + "&to=" + $("input[name=to]").val())

    })


    count = [];
    for (var i = 0; i < 12; i++) {
        data = $(".chart-info div[data-month=" + i + "]");
        total = (data.length != 0) ? data.attr("data-count") : 0;
        count.push(total);
    }

    var ctx = document.getElementById("myChart").getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ["E", "F", "M", "A", "M", "J", "J", "A", "S", "O", "N", "D"],
            datasets: [{
                label: $(".chart-info").data("label"),
                data: count,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(255, 159, 64, 0.2)',
                    'rgba(133, 148, 228, 0.2)',
                    'rgba(255, 165, 165, 0.2)',
                    'rgba(200, 231, 237, 0.2)',
                    'rgba(190, 48, 48, 0.2)',
                    'rgba(238, 238, 238, 0.2)',
                    'rgba(28, 114, 147, 0.2)',
                ],
                borderColor: [
                    'rgba(255,99,132,1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)',
                    'rgba(133, 148, 228, 1)',
                    'rgba(255, 165, 165, 1)',
                    'rgba(200, 231, 237, 1)',
                    'rgba(190, 48, 48, 1)',
                    'rgba(238, 238, 238, 1)',
                    'rgba(28, 114, 147, 1)',
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
    });
})