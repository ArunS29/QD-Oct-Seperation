function exportDataGridToPDF(gridId) {
    const gridElement = document.getElementById(gridId);

    if (!gridElement) {
        DevExpress.ui.notify(`Grid element #${gridId} not found.`, "error", 3000);
        return;
    }

    if ($(gridElement).is(":hidden")) {
        DevExpress.ui.notify("Grid is not visible. Please show it before printing.", "warning", 3000);
        return;
    }

    let gridInstance = null;
    let isPivotGrid = false;

    try {
        gridInstance = $("#" + gridId).dxDataGrid("instance");
    } catch (e) {
        gridInstance = null;
    }

    if (!gridInstance) {
        try {
            gridInstance = $("#" + gridId).dxPivotGrid("instance");
            isPivotGrid = true;
        } catch (e) {
            gridInstance = null;
        }
    }

    if (!gridInstance) {
        DevExpress.ui.notify("Grid or PivotGrid instance not found!", "error", 3000);
        return;
    }

    if (!isPivotGrid) {
        // DataGrid export code as you have
    }
    else {
        // PivotGrid export with data load wait
        gridInstance.getDataSource().load().done(function () {
            const { jsPDF } = window.jspdf;
            const doc = new jsPDF({
                orientation: "landscape",
                unit: "mm",
                format: "a3"
            });

            doc.setFontSize(9);

            DevExpress.pdfExporter.exportPivotGrid({
                component: gridInstance,
                jsPDFDocument: doc,
                autoTableOptions: {
                    styles: { fontSize: 8, cellPadding: 2 },
                    tableWidth: "wrap"
                }
            }).then(() => showPDF(doc));
        }).fail(() => {
            DevExpress.ui.notify("Failed to load PivotGrid data.", "error", 3000);
        });
    }

    function showPDF(doc) {
        const pdfBlob = doc.output("blob");
        const newWindow = window.open("", "_blank", "width=1200,height=800");
        newWindow.document.write(`
            <html>
            <head>
                <title>Exported PDF</title>
                <script>
                    function printPDF() {
                        document.getElementById('pdfViewer').contentWindow.print();
                    }
                <\/script>
            </head>
            <body>
                <div style="display: flex; justify-content: space-between; padding: 10px; background: #ddd;">
                    <button onclick="printPDF()">🖨 Print</button>
                    <button onclick="window.close()">❌ Close</button>
                </div>
                <iframe id="pdfViewer" width="100%" height="90%" style="border:none;"></iframe>
            </body>
            </html>
        `);
        newWindow.document.close();

        const blobUrl = URL.createObjectURL(pdfBlob);
        newWindow.document.getElementById("pdfViewer").src = blobUrl;
    }
}
