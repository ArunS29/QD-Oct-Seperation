function exportDataGridToPDF(gridId, title = "") {
    var grid = $("#" + gridId).dxDataGrid("instance");

    if (!grid) {
        console.error("DataGrid not found!");
        return;
    }

    grid.getDataSource().load().done(function (fullData) {
        const { jsPDF } = window.jspdf;

        var columns = grid.getVisibleColumns();
        var columnCount = columns.length;

        // ✅ Define orientation before using it
        var orientation = columnCount > 10 ? "landscape" : "portrait";

        var doc = new jsPDF({
            orientation: orientation,
            unit: "mm",
            format: "a1"

        });

        doc.setFontSize(22);       // Bigger font for title
        doc.setFont("helvetica", "bold");
        doc.text(title, 30, 10); // Custom title passed in

        doc.setFontSize(9); // Default font
        var columnStyles = {};
        columns.forEach((col, index) => {
            columnStyles[index] = { cellWidth: "wrap", minCellWidth: 20 };
        });

        DevExpress.pdfExporter.exportDataGrid({
            jsPDFDocument: doc,
            component: grid,
            autoTableOptions: {
                styles: { fontSize: 9, cellPadding: 3 },
                tableWidth: "wrap",
                columnStyles: columnStyles
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
            newWindow.document.getElementById("pdfViewer").src = blobUrl + "#zoom=80";

        });
    }).fail(function () {
        console.error("Failed to load all data.");
    });
}
