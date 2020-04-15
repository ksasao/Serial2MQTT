function updateAxes(chart, label, t_min,t_max, y_min,y_max) {
    chart.options = {
        scales: {
            xAxes: [{
                type: 'time',
                time: {
                    min: t_min,
                    max: t_max,
                    displayFormats: {
                        second: 'HH:mm:ss',
                        minute: 'M/D HH:mm',
                        hour: 'M/D HH:mm',
                        day: 'M/D'
                    }
                }
            }],
            yAxes: [{
                    scaleLabel: {
                    display: true,
                    fontStyle: 'bold',
                    fontSize: 20,
                    labelString: label
                },
                ticks: {
                    min: y_min,
                    max: y_max
                }
            }]
        },
        plugins: {
            colorschemes: {
                scheme: 'brewer.SetTwo8'
            }
        }
    };
    chart.update();
};

function addData(chart, list, data, item) {
    if(!list.some(d => d.label == data)){
        var newData = {
            label: data,
            lineTension: 0,
            pointStyle: `circle`,
            radius: 5,
            borderWidth: 2,
            fill: false,
            data: []
        };
        list.push(newData);
        // order by label
        list.sort(function(a,b){
            if(a.label < b.label) return -1;
            if(a.label > b.label) return 1;
            return 0;
        });
    }
    chart.data.datasets.forEach((d) => {
        if(d.label == data){
            d.data.push(item);
            // Remove old data
            if(d.data[0].x < chart.options.scales.xAxes[0].time.min){
                d.data.shift();
            }
        }
    });
    chart.update();
}

// Setup chart
Chart.defaults.global.defaultFontSize = 12;
var charts = {};
var topics = {};
var lists = {};
var axisNames = {};
var yMins ={};
var yMaxs = {};

function setChart(id, topic, axisName, yMin, yMax){
    let ctx = document.getElementById(id).getContext('2d');
    let list = [];
    var chart = new Chart(ctx, {
    type: 'line',
    data: { datasets: list },
    options: {
        legend: {
                display: true,
                labels: {
                    fontColor: 'rgb(64, 64, 64)'
                }
            },
    }
    });
    topics[topic] = topic;
    charts[topic] = chart;
    lists[topic] = list;
    axisNames[topic] = axisName;
    yMins[topic] = yMin;
    yMaxs[topic] = yMax;
}
