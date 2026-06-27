// ECharts renderers for the CRM dashboard.
// Relies on the global `echarts` loaded once from the shared wwwroot/js/echarts.min.js.
// Imported as an ES module from CrmDashboard.razor.cs via JS interop.

const charts = {};

// Compact Vietnamese money format: nghìn / triệu / tỷ.
function formatVnMoneyShort(value) {
    const num = Number(value) || 0;
    const sign = num < 0 ? "-" : "";
    const abs = Math.abs(num);

    const scale = (divisor, unit) => {
        const scaled = abs / divisor;
        const text = scaled % 1 === 0 ? scaled.toFixed(0) : scaled.toFixed(1);
        return sign + text.replace(".", ",") + " " + unit;
    };

    if (abs >= 1e9) return scale(1e9, "tỷ");
    if (abs >= 1e6) return scale(1e6, "triệu");
    if (abs >= 1e3) return scale(1e3, "nghìn");
    return sign + abs.toLocaleString("vi-VN");
}

// Create (or recreate) an ECharts instance bound to the given DOM element.
function getInstance(elementId) {
    const dom = document.getElementById(elementId);
    if (!dom || !window.echarts) {
        return null;
    }
    if (charts[elementId]) {
        charts[elementId].dispose();
    }
    const chart = window.echarts.init(dom);
    charts[elementId] = chart;
    return chart;
}

function bindResize(elementId, chart) {
    const handler = () => chart.resize();
    window.addEventListener("resize", handler);
    chart._resizeHandler = handler;
}

// Bar chart: opportunity count per stage. `items` = [{ label, count, color }].
export function renderStageCountBar(elementId, items) {
    const chart = getInstance(elementId);
    if (!chart) {
        return;
    }
    chart.setOption({
        tooltip: {
            trigger: "axis",
            axisPointer: { type: "shadow" },
            formatter: params => {
                const p = params[0];
                return `<b>${p.name}</b><br/>${Number(p.value).toLocaleString("vi-VN")} cơ hội`;
            }
        },
        grid: { left: "3%", right: "4%", bottom: "3%", top: 36, containLabel: true },
        xAxis: {
            type: "category",
            data: items.map(i => i.label),
            axisLabel: { fontSize: 12, color: "#64748B" }
        },
        yAxis: {
            type: "value",
            minInterval: 1,
            axisLabel: { color: "#64748B" }
        },
        series: [{
            type: "bar",
            barMaxWidth: 52,
            itemStyle: { borderRadius: [6, 6, 0, 0] },
            label: { show: true, position: "top", fontWeight: "bold", color: "#0F172A" },
            data: items.map(i => ({ value: i.count, itemStyle: { color: i.color } }))
        }]
    });
    bindResize(elementId, chart);
}

// Doughnut chart: pipeline value per stage. `items` = [{ label, amount, color }].
export function renderStageValueDonut(elementId, items) {
    const chart = getInstance(elementId);
    if (!chart) {
        return;
    }
    const data = items.map(i => ({ name: i.label, value: i.amount, itemStyle: { color: i.color } }));
    const total = data.reduce((sum, d) => sum + (d.value || 0), 0);

    chart.setOption({
        tooltip: {
            trigger: "item",
            formatter: p => `<b>${p.name}</b><br/>${formatVnMoneyShort(p.value)} (${p.percent.toFixed(1)}%)`
        },
        legend: {
            orient: "vertical",
            left: "left",
            top: "middle",
            textStyle: { fontSize: 13, color: "#334155" }
        },
        series: [{
            type: "pie",
            radius: ["45%", "72%"],
            center: ["64%", "52%"],
            avoidLabelOverlap: true,
            itemStyle: { borderRadius: 6, borderColor: "#fff", borderWidth: 2 },
            label: {
                show: true,
                formatter: p => `${formatVnMoneyShort(p.value)}`,
                fontSize: 11,
                color: "#475569"
            },
            emphasis: {
                label: { show: true, fontSize: 13, fontWeight: "bold" },
                itemStyle: { shadowBlur: 10, shadowColor: "rgba(0,0,0,0.2)" }
            },
            data: data
        }],
        graphic: [{
            type: "group",
            left: "59%",
            top: "46%",
            children: [
                {
                    type: "text",
                    style: {
                        text: formatVnMoneyShort(total),
                        textAlign: "center",
                        fontSize: 20,
                        fontWeight: "bold",
                        fill: "#0F172A"
                    }
                },
                {
                    type: "text",
                    style: {
                        text: "Tổng giá trị",
                        textAlign: "center",
                        fontSize: 12,
                        fill: "#64748B",
                        y: 26
                    }
                }
            ]
        }]
    });
    bindResize(elementId, chart);
}

export function dispose(elementId) {
    const chart = charts[elementId];
    if (chart) {
        if (chart._resizeHandler) {
            window.removeEventListener("resize", chart._resizeHandler);
        }
        chart.dispose();
        delete charts[elementId];
    }
}

export function disposeAll() {
    Object.keys(charts).forEach(dispose);
}
