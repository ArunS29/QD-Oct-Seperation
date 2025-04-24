function exportDataGridToPDF(gridId) {
    var grid = $("#" + gridId).dxDataGrid("instance");

    if (!grid) {
        console.error("DataGrid not found!");
        return;
    }

    grid.getDataSource().load().done(function (fullData) {
        const { jsPDF } = window.jspdf;

        var columns = grid.getVisibleColumns();
        var columnCount = columns.length;

        var orientation = columnCount > 10 ? "landscape" : "portrait";

        var doc = new jsPDF({
            orientation: orientation,
            unit: "mm",
            format: "a3"
        });

        // ✅ Arabic font setup (required before using setFont)
        doc.addFileToVFS("Amiri Regular-normal.ttf", Amiri_Regular);
        doc.addFont("Amiri Regular-normal.ttf", "Amiri", "normal");
        doc.setFont("Amiri");
        doc.setFontSize(9);

        var columnStyles = {};
        columns.forEach((col, index) => {
            columnStyles[index] = { cellWidth: "wrap", minCellWidth: 20 };
        });

        DevExpress.pdfExporter.exportDataGrid({
            jsPDFDocument: doc,
            component: grid,
            autoTableOptions: {
                styles: { font: "Amiri", fontSize: 9, cellPadding: 3 },
                tableWidth: "wrap",
                columnStyles: columnStyles,
                didDrawCell: function (data) {
                    const txt = data.cell.text;
                    if (typeof txt === "string" && /[\u0600-\u06FF]/.test(txt)) {
                        data.cell.text = txt.split(" ").reverse().join(" ");
                    }
                }
            }
        }).then(() => {
            var pdfBlob = doc.output("blob");

            var newWindow = window.open("", "_blank", "width=1200,height=800");
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

            var blobUrl = URL.createObjectURL(pdfBlob);
            newWindow.document.getElementById("pdfViewer").src = blobUrl;
        });
    }).fail(function () {
        console.error("Failed to load all data.");
    });
}
