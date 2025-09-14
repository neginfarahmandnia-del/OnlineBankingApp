window.transactionsExport = (function () {
    let chart = null;

    function ensureChartJs() {
        if (typeof Chart === "undefined") {
            console.warn("Chart.js ist nicht geladen. Bitte Chart.js im _Host.cshtml einbinden.");
            return false;
        }
        return true;
    }

    function clearChart(canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        if (chart) {
            chart.destroy();
            chart = null;
        }
    }

    function renderMonthlyChart(canvasId, labels, income, expenses, title) {
        if (!ensureChartJs()) return;
        const ctx = document.getElementById(canvasId).getContext("2d");
        if (chart) chart.destroy();

        chart = new Chart(ctx, {
            type: "bar",
            data: {
                labels,
                datasets: [
                    { label: "Einnahmen", data: income },
                    { label: "Ausgaben", data: expenses }
                ]
            },
            options: {
                responsive: true,
                plugins: {
                    title: { display: true, text: "Monatliche Bewegung: " + (title || "") },
                    legend: { position: "top" }
                },
                scales: {
                    x: { title: { display: true, text: "Tag" } },
                    y: { beginAtZero: true, title: { display: true, text: "Betrag" } }
                }
            }
        });
    }

    return { renderMonthlyChart, clearChart };
})();
