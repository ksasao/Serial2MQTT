// Setup chart
Chart.defaults.global.defaultFontSize = 12;
let charts = {};
let topics = {};
let lists = {};
let axisNames = {};
let yMins ={};
let yMaxs = {};
let clientId;

function setChart(id, topic, axisName, yMin, yMax) {
    let ctx = document.getElementById(id).getContext('2d');
    let list = [];
    const chart = new Chart(ctx, {
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

//  MQTT Broker IP Address, MQTT over Websocket Port, duration (ms), repeat timer (ms) 
function startChart(serverAddress, serverPort, duration, repeat){
    // MQTT Broker
    clientId = "JavaScriptClient-"+(Math.floor(Math.random() * 100000));
    client = new Paho.MQTT.Client(serverAddress, Number(serverPort), clientId);

    // set callback handlers
    client.onConnectionLost = onConnectionLost;
    client.onMessageArrived = onMessageArrived;

    // connect the client
    client.connect({onSuccess:onConnect});

    // Update chart data
    setInterval(function(){
        const now = Date.now();
        const old = new Date(now - duration);
        for (let key in topics) {
            updateAxes(charts[key], axisNames[key], old, now, yMins[key],yMaxs[key]);
        }
    }, repeat);
}

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
        const newData = {
            label: data,
            lineTension: 0,
            pointStyle: `circle`,
            radius: 2,
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

function subscribeTopics(client){
    for (let key in topics) {
        client.subscribe(key);
    }
}


// called when the client connects
function onConnect() {
    // Once a connection has been made, make a subscription and send a message.
    console.log("onConnect");
    subscribeTopics(client);
    message = new Paho.MQTT.Message("Connect from " + clientId);
    message.destinationName = "Log";
    client.send(message);
}

// called when the client loses its connection
function onConnectionLost(responseObject) {
    if (responseObject.errorCode !== 0) {
    console.log("onConnectionLost:"+responseObject.errorMessage);
    }
}

// called when a message arrives
function onMessageArrived(message) {
    try{
        let topic = message.destinationName;
        const data = JSON.parse(message.payloadString);
        addData(charts[topic], lists[topic], data.Id, { x: Date.now(), y: data.Value });
    }catch(e){
        // nothing
    }
    console.log("onMessageArrived:"+message.payloadString);
}
