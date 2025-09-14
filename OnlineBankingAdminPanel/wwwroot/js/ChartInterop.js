window.ChartInterop = {
    initPieChart: function (id, labels, data) {
        new Chart(document.getElementById(id), {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: ['#36A2EB', '#FFCE56', '#FF6384'],
                }]
            }
        });
    },
    initBarChart: function (id, labels, data) {
        new Chart(document.getElementById(id), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Umsatz',
                    data: data,
                    backgroundColor: '#4CAF50'
                }]
            }
        });
    },
    initDoughnutChart: function (id, labels, data) {
        new Chart(document.getElementById(id), {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: ['#FF6384', '#36A2EB']
                }]
            }
        });
    }
};
