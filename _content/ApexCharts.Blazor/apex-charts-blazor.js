// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

var charts = {}

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

export function getCharts() {
    console.log(charts)
}

export function createChart(chartId, options, extendedOptions) {
    console.log(extendedOptions)

    if (typeof options.tooltip != 'undefined' && extendedOptions.yFormatExpression != null) {
        // Here, we need to inject the formatter function.
        options.tooltip.y = {}
        options.tooltip.y.formatter = new Function('val', extendedOptions.yFormatExpression)
    }

    if (typeof options.dataLabels != 'undefined' && extendedOptions.dataLabelsFormatExpression != null) {
        // Here, we need to inject the formatter function.
        console.log(extendedOptions.dataLabelsFormatExpression)
        options.dataLabels.formatter = new Function('val', 'opt', extendedOptions.dataLabelsFormatExpression)
    }

    console.log(options)

    var chart = new ApexCharts(document.getElementById(chartId), options)
    charts[chartId] = chart
    chart.render()
}

export function updateChart(chartId, options, extendedOptions) {
    // find the chart
    var chart = charts[chartId]

    if (!chart)
        return;

    if (typeof options.tooltip != 'undefined' && extendedOptions.yFormatExpression != null) {
        // Here, we need to inject the formatter function.
        options.tooltip.y = {}
        options.tooltip.y.formatter = new Function('val', extendedOptions.yFormatExpression)
    }

    if (typeof options.dataLabels != 'undefined' && extendedOptions.dataLabelsFormatExpression != null) {
        // Here, we need to inject the formatter function.
        console.log(extendedOptions.dataLabelsFormatExpression)
        options.dataLabels.formatter = new Function('val', 'opt', extendedOptions.dataLabelsFormatExpression)
    }

    console.log(options)

    chart.destroy()

    chart = new ApexCharts(document.getElementById(chartId), options)
    charts[chartId] = chart
    chart.render()
}
